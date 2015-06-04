#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64)
#define UNITY_WIN 1
#elif defined(__MACH__) || defined(__APPLE__)
#define UNITY_OSX 1
#elif defined(__ANDROID__)
#define UNITY_ANDROID 1
#elif defined(__linux__)
#define UNITY_LINUX 1
#endif

#include <stdio.h>

#if UNITY_OSX | UNITY_LINUX
	#include <sys/mman.h>
	#include <sys/types.h>
	#include <sys/stat.h>
	#include <fcntl.h>
	#include <unistd.h>
	#include <string.h>
#endif

#include "TeleportLib.h"

#if defined(__GNUC__) || defined(__SNC__)
#define TELEPORT_ALIGN(val) __attribute__((aligned(val))) __attribute__((packed))
#elif defined(_MSC_VER)
#define TELEPORT_ALIGN(val) __declspec(align(val))
#else
#define TELEPORT_ALIGN(val)
#endif

#define LOG(...) do { if(0) fprintf(stdout, "*** " __VA_ARGS__); fflush(stdout); } while(0)
#define LOGX(...) LOG(#__VA_ARGS__ "\n"); __VA_ARGS__
#define LOGF() LogFunc lf(__FUNCTION__)

struct LogFunc
{
	const char* name;
	LogFunc(const char* _name): name(_name) { LOG("> %s", name); }
	~LogFunc() { LOG("< %s\n", name); }
};

namespace Teleport
{
	const int NUMPARAMS = 4;
	const int NUMSTREAMS = 8;
	
	struct Parameter
	{
		float value;
		int changed;
	} TELEPORT_ALIGN(4);
	
	struct Stream
	{
		enum { LENGTH = 2 * 44100 };
		
		Parameter params[NUMPARAMS];

		int readpos;
		int writepos;
		float buffer[LENGTH];
		
		inline bool Read(float& val)
		{
			int r = readpos;
			if(r == writepos)
				return false;
			readpos = (r == LENGTH - 1) ? 0 : (r + 1);
			val = buffer[r];
			return true;
		}
		
		inline bool Feed(float input)
		{
			int w = writepos;
			writepos = (w == LENGTH - 1) ? 0 : (w + 1);
			buffer[w] = input;
			return true;
		}
		
		inline int GetNumBuffered() const
		{
			int b = writepos - readpos;
			if(b < 0)
				b += LENGTH;
			return b;
		}
	} TELEPORT_ALIGN(4);
	
	struct SharedMemory
	{
		Stream streams[NUMSTREAMS];
	};

	class SharedMemoryHandle
	{
	protected:
		SharedMemory* data;
#if UNITY_WIN
		HANDLE hMapFile;
#endif

	public:
		SharedMemoryHandle()
		{
			LOGF();

			for(int attempt = 0; attempt < 10; attempt++)
			{
				bool clearmemory = true;
				char filename[1024];

#if UNITY_WIN

				sprintf_s(filename, "UnityAudioTeleport%d", attempt);
				hMapFile = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(SharedMemory), filename);
				if(hMapFile == NULL)
				{
					clearmemory = false;
					hMapFile = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, filename);
				}
				if (hMapFile == NULL)
				{
					printf("Could not create file mapping object (%d).\n", GetLastError());
					continue;
				}

				data = (SharedMemory*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SharedMemory));
				if (data == NULL)
				{
					printf("Could not map view of file (%d).\n", GetLastError());
					CloseHandle(hMapFile);
					continue;
				}

#else
				sprintf(filename, "/tmp/UnityAudioTeleport%d", attempt);
				clearmemory = (access(filename, F_OK) == -1);
				LOGX(int handle = shm_open(filename, O_RDWR | O_CREAT, 0777));
				if (handle == -1)
				{
					fprintf(stderr, "Open failed\n");
					continue;
				}
			
				LOGX(if (ftruncate(handle, sizeof(SharedMemory)) == -1))
				{
					fprintf(stderr, "ftruncate error (ignored)\n");
					//continue;
				}
			
				LOGX(data = (SharedMemory*)mmap(0, sizeof(SharedMemory), PROT_READ | PROT_WRITE, MAP_SHARED, handle, 0));
				if (data == (void *) -1)
				{
					fprintf(stderr, "mmap failed\n");
					continue;
				}
				
				//close(handle);
				//shm_unlink(filename);

#endif
				
				if(clearmemory)
					memset(data, 0, sizeof(SharedMemory));

				break; // intentional (see continue's above)
			}
		}

		~SharedMemoryHandle()
		{
			LOGF();

#if UNITY_WIN

			UnmapViewOfFile(data);
			CloseHandle(hMapFile);

#else

			if(data)
			{
				LOGX(munmap(data, sizeof(SharedMemory)));
			}
			
#endif
		}
		
		inline SharedMemory* operator -> () const { return data; }
	};
	
	inline SharedMemoryHandle& GetSharedMemory()
	{
		static SharedMemoryHandle shared;
		return shared;
	}
}

extern "C" UNITY_AUDIODSP_EXPORT_API int TeleportFeed (int stream, float* samples, int numsamples)
{
	//LOGF();
	Teleport::Stream& s = Teleport::GetSharedMemory()->streams[stream];
	for(int n = 0; n < numsamples; n++)
		s.Feed(samples[n]);
	return s.writepos;
}

extern "C" UNITY_AUDIODSP_EXPORT_API int TeleportRead (int stream, float* samples, int numsamples)
{
	//LOGF();
	Teleport::Stream& s = Teleport::GetSharedMemory()->streams[stream];
	for(int n = 0; n < numsamples; n++)
		if(!s.Read(samples[n]))
			samples[n] = 0.0f;
	return s.readpos;
}

extern "C" UNITY_AUDIODSP_EXPORT_API int TeleportGetNumBuffered (int stream)
{
	//LOGF();
	Teleport::Stream& s = Teleport::GetSharedMemory()->streams[stream];
	return s.GetNumBuffered();
}

extern "C" UNITY_AUDIODSP_EXPORT_API int TeleportSetParameter (int stream, int index, float value)
{
	//LOGF();
	Teleport::Stream& s = Teleport::GetSharedMemory()->streams[stream];
	s.params[index].changed = 1;
	s.params[index].value = value;
	return 1;
}

extern "C" UNITY_AUDIODSP_EXPORT_API int TeleportGetParameter (int stream, int index, float* value)
{
	//LOGF();
	Teleport::Stream& s = Teleport::GetSharedMemory()->streams[stream];
	*value = s.params[index].value;
	int changed = s.params[index].changed;
	s.params[index].changed = 0;
	return changed;
}
