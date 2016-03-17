package com.weplaydots.UnityPdHelper;

import android.content.Context;
import android.content.res.AssetManager;
import android.content.res.AssetFileDescriptor;
import android.util.Log;

import java.lang.String;
import java.io.File;
import java.io.FileDescriptor;
import java.io.IOException;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.nio.channels.FileChannel;
import java.util.ArrayList;

public class UnityPdHelper {
	static Context context;
	
	public static void SetContext( Context unityContext ) {
		context = unityContext;
	}

	public static void CopyAssetsFolderToSd( String apkFolderPath, String persistantDataPath ) {
		ArrayList<String> files = listAssetsContent( apkFolderPath );
		
		try {
			AssetManager am = context.getResources().getAssets();
			AssetFileDescriptor descriptor = null;
			for ( String apkPath : files ) {
				Log.v( "UnityPdHelper", "Copying " + apkPath );
				descriptor = am.openFd( apkPath );

				// Create new file to copy into.
				File file = new File( persistantDataPath + java.io.File.separator + apkPath );
				file.getParentFile().mkdirs();
				file.createNewFile();

				copyFdToFile( descriptor.getFileDescriptor(), file );

			} 
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	static ArrayList<String> listAssetsContent(String path)
	{
		ArrayList<String> l = new ArrayList<>();
		
		try {
			AssetManager am = context.getResources().getAssets();	// instance is a reference to the activity
			
			String [] list = am.list(path);
			for (String s : list)
			{
				String subPath = path + "/" + s;
				if (subPath.charAt(0) == '/')
				{
					subPath = subPath.substring(1);
				}
				
				ArrayList subList = listAssetsContent(subPath);
				
				if (subList.size() > 0)
				{
					// directory
					l.addAll(subList);
				}
				else
				{
					// file
					l.add(subPath);
				}
			}
			
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		return l;
	}
	
	static void copyFdToFile(FileDescriptor src, File dst) throws IOException {
		FileChannel inChannel = new FileInputStream(src).getChannel();
		FileChannel outChannel = new FileOutputStream(dst).getChannel();
		try {
			inChannel.transferTo(0, inChannel.size(), outChannel);
			Log.v( "UnityPdHelper", "Copied! " + dst.getPath() );
		} finally {
			if (inChannel != null)
				inChannel.close();
			if (outChannel != null)
				outChannel.close();
		}
	}
}