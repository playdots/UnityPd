/* 
 * UnityPdHelper.java
 * Copyright (C) 2017 Playdots, Inc.
 * ----------------------------
 */
package com.weplaydots.UnityPdHelper;

import android.content.Context;
import android.content.res.AssetManager;
import android.content.res.AssetFileDescriptor;
import android.util.Log;

import java.lang.String;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;

public class UnityPdHelper {
	static Context context;
	
	public static void SetContext( Context unityContext ) {
		context = unityContext;
	}

	public static void CopyAssetsFolderToPersistantData( String apkFolderPath, String persistantDataPath ) {
		ArrayList<String> files = listAssetsContent( apkFolderPath );
		
		try {
			AssetManager am = context.getResources().getAssets();
			AssetFileDescriptor descriptor = null;
			for ( String apkPath : files ) {
				Log.v( "UnityPdHelper", "Copying..." + apkPath );
				// Create new file to copy into.
				String persistantPath = persistantDataPath + java.io.File.separator + apkPath;
				Log.v( "UnityPdHelper", "Copied!" );

				copyToFile( apkPath, persistantPath, am );

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
	
	static void copyToFile( String fromFile, String toFile, AssetManager assetManager ) throws IOException {
		InputStream in = null;
		FileOutputStream out = null;
		try {
			in = assetManager.open(fromFile);
			File outFile = new File( toFile );
			outFile.getParentFile().mkdirs();
			out = new FileOutputStream( outFile );

			byte[] buffer = new byte[1024];
			int read;
			while ((read = in.read(buffer)) != -1) {
				out.write(buffer, 0, read);
			}
			in.close();
			in = null;
			out.flush();
			out.close();
			out = null;
		} catch (Exception e) {
			Log.e("UnityPdHelper", e.getMessage());
		}
	}
}
