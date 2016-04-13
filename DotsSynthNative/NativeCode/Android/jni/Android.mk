LOCAL_PATH := $(call my-dir)

PD_ROOT = $(LOCAL_PATH)/../../../../libpd
DST_PATH = $(LOCAL_PATH)/../../../../UnityPd/Android

# PD-specific flags
PD_SRC_FILES := \
  $(PD_ROOT)/pure-data/src/d_arithmetic.c $(PD_ROOT)/pure-data/src/d_array.c $(PD_ROOT)/pure-data/src/d_ctl.c \
  $(PD_ROOT)/pure-data/src/d_dac.c $(PD_ROOT)/pure-data/src/d_delay.c $(PD_ROOT)/pure-data/src/d_fft.c \
  $(PD_ROOT)/pure-data/src/d_fft_fftsg.c \
  $(PD_ROOT)/pure-data/src/d_filter.c $(PD_ROOT)/pure-data/src/d_global.c $(PD_ROOT)/pure-data/src/d_math.c \
  $(PD_ROOT)/pure-data/src/d_misc.c $(PD_ROOT)/pure-data/src/d_osc.c $(PD_ROOT)/pure-data/src/d_resample.c \
  $(PD_ROOT)/pure-data/src/d_soundfile.c $(PD_ROOT)/pure-data/src/d_ugen.c \
  $(PD_ROOT)/pure-data/src/g_all_guis.c $(PD_ROOT)/pure-data/src/g_array.c $(PD_ROOT)/pure-data/src/g_bang.c \
  $(PD_ROOT)/pure-data/src/g_canvas.c $(PD_ROOT)/pure-data/src/g_editor.c $(PD_ROOT)/pure-data/src/g_graph.c \
  $(PD_ROOT)/pure-data/src/g_guiconnect.c $(PD_ROOT)/pure-data/src/g_hdial.c \
  $(PD_ROOT)/pure-data/src/g_hslider.c $(PD_ROOT)/pure-data/src/g_io.c $(PD_ROOT)/pure-data/src/g_mycanvas.c \
  $(PD_ROOT)/pure-data/src/g_numbox.c $(PD_ROOT)/pure-data/src/g_readwrite.c \
  $(PD_ROOT)/pure-data/src/g_rtext.c $(PD_ROOT)/pure-data/src/g_scalar.c $(PD_ROOT)/pure-data/src/g_template.c \
  $(PD_ROOT)/pure-data/src/g_text.c $(PD_ROOT)/pure-data/src/g_toggle.c $(PD_ROOT)/pure-data/src/g_traversal.c \
  $(PD_ROOT)/pure-data/src/g_vdial.c $(PD_ROOT)/pure-data/src/g_vslider.c $(PD_ROOT)/pure-data/src/g_vumeter.c \
  $(PD_ROOT)/pure-data/src/m_atom.c $(PD_ROOT)/pure-data/src/m_binbuf.c $(PD_ROOT)/pure-data/src/m_class.c \
  $(PD_ROOT)/pure-data/src/m_conf.c $(PD_ROOT)/pure-data/src/m_glob.c $(PD_ROOT)/pure-data/src/m_memory.c \
  $(PD_ROOT)/pure-data/src/m_obj.c $(PD_ROOT)/pure-data/src/m_pd.c $(PD_ROOT)/pure-data/src/m_sched.c \
  $(PD_ROOT)/pure-data/src/s_audio.c $(PD_ROOT)/pure-data/src/s_audio_dummy.c \
  $(PD_ROOT)/pure-data/src/s_file.c $(PD_ROOT)/pure-data/src/s_inter.c \
  $(PD_ROOT)/pure-data/src/s_loader.c $(PD_ROOT)/pure-data/src/s_main.c $(PD_ROOT)/pure-data/src/s_path.c \
  $(PD_ROOT)/pure-data/src/s_print.c $(PD_ROOT)/pure-data/src/s_utf8.c $(PD_ROOT)/pure-data/src/x_acoustics.c \
  $(PD_ROOT)/pure-data/src/x_arithmetic.c $(PD_ROOT)/pure-data/src/x_connective.c \
  $(PD_ROOT)/pure-data/src/x_gui.c $(PD_ROOT)/pure-data/src/x_list.c $(PD_ROOT)/pure-data/src/x_midi.c \
  $(PD_ROOT)/pure-data/src/x_misc.c $(PD_ROOT)/pure-data/src/x_net.c $(PD_ROOT)/pure-data/src/x_array.c \
  $(PD_ROOT)/pure-data/src/x_time.c $(PD_ROOT)/pure-data/src/x_interface.c $(PD_ROOT)/pure-data/src/x_scalar.c \
  $(PD_ROOT)/pure-data/src/x_text.c $(PD_ROOT)/libpd_wrapper/s_libpdmidi.c \
  $(PD_ROOT)/libpd_wrapper/x_libpdreceive.c $(PD_ROOT)/libpd_wrapper/z_libpd.c \
  $(PD_ROOT)/libpd_wrapper/util/ringbuffer.c $(PD_ROOT)/libpd_wrapper/util/z_queued.c \
  $(PD_ROOT)/libpd_wrapper/z_hooks.c
PD_C_INCLUDES := $(PD_ROOT)/pure-data/src $(PD_ROOT)/libpd_wrapper \
  $(PD_ROOT)/libpd_wrapper/util
PD_CFLAGS := -DPD -DHAVE_UNISTD_H -DHAVE_LIBDL -DUSEAPI_DUMMY -w
PD_JNI_CFLAGS := -Wno-int-to-pointer-cast -Wno-pointer-to-int-cast
PD_LDLIBS := -ldl

# build LibPd static library
include $(CLEAR_VARS)
LOCAL_MODULE := pd
LOCAL_C_INCLUDES := $(PD_C_INCLUDES)
LOCAL_CFLAGS := $(PD_CFLAGS)
LOCAL_SRC_FILES := $(PD_SRC_FILES:$(LOCAL_PATH)/%=%)
LOCAL_EXPORT_C_INCLUDES := $(PD_C_INCLUDES)
include $(BUILD_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := AudioPlugin_UnityPd
LOCAL_C_INCLUDES := $(LOCAL_PATH)/../..
LOCAL_SRC_FILES := ../../Plugin_UnityPd.cpp ../../AudioPluginUtil.cpp
LOCAL_STATIC_LIBRARIES := pd
include $(BUILD_SHARED_LIBRARY)

all: $(DST_PATH)/$(notdir $(LOCAL_BUILT_MODULE))

$(DST_PATH)/$(notdir $(LOCAL_BUILT_MODULE)): $(LOCAL_BUILT_MODULE)
	cp $< $@
