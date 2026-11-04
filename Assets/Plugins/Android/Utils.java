package cn.com.glsa.app.fightgame;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.os.ParcelFileDescriptor;
import android.provider.MediaStore;
import android.util.Log;

import java.io.BufferedOutputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class Utils {
    public static String copyAssetsFile(Context context, String assetsPath, String outputFilePath){
        try{
            try (InputStream inputStream = context.getAssets().open(assetsPath)) {
                try (FileOutputStream outputStream = new FileOutputStream(outputFilePath)) {
                    int read;
                    byte[] bytes = new byte[8192];
                    while ((read = inputStream.read(bytes)) != -1) {
                        outputStream.write(bytes, 0, read);
                    }
                }
            }
            return null;
        }catch (Throwable e){
            e.printStackTrace();
            return e.getMessage();
        }
    }

    /**
     * 保存图片/视频到相册
     * @param activity
     * @param filePath
     * @param fileName
     * @param isVideo
     */
    public static void saveFileToGallery(Activity activity, String filePath, String fileName, boolean isVideo){
        saveFile2Gallery(activity, isVideo, Uri.fromFile(new File(filePath)), fileName);
    }

    public static byte[] readAssetFile(Context context, String fileName) {
        AssetManager assetManager = context.getAssets();
        InputStream inputStream = null;
        ByteArrayOutputStream outputStream = null;
        try {
            inputStream = assetManager.open(fileName);
            outputStream = new ByteArrayOutputStream();
            byte[] buffer = new byte[1024];
            int length;
            while ((length = inputStream.read(buffer)) != -1) {
                outputStream.write(buffer, 0, length);
            }
            return outputStream.toByteArray();
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            try {
                if (inputStream != null) {
                    inputStream.close();
                }
                if (outputStream != null) {
                    outputStream.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return null;
    }

    public static Uri saveFile2Gallery(Context context, boolean isVideo, Uri uri, String fileName) {
        String folderName = "";
        if (isVideo) {
            folderName = Environment.DIRECTORY_DCIM + "/" + fileName;
        } else {
            folderName = Environment.DIRECTORY_PICTURES + "/" + fileName;
        }
        Uri uriSavedFile;
        File createdFile = null;
        ContentResolver resolver = context.getContentResolver();
        ContentValues valuesFile;
        valuesFile = new ContentValues();

        if (Build.VERSION.SDK_INT >= 29) {
            valuesFile.put(MediaStore.Video.Media.RELATIVE_PATH, folderName);
            valuesFile.put(MediaStore.Video.Media.TITLE, fileName);
            valuesFile.put(MediaStore.Video.Media.DISPLAY_NAME, fileName);
            valuesFile.put(MediaStore.Video.Media.MIME_TYPE, isVideo? "video/mp4" : "image/jpeg");
            valuesFile.put(
                    MediaStore.Video.Media.DATE_ADDED,
                    System.currentTimeMillis() / 1000);

            Uri collection = isVideo? MediaStore.Video.Media.getContentUri(MediaStore.VOLUME_EXTERNAL_PRIMARY) :
                    MediaStore.Images.Media.getContentUri(MediaStore.VOLUME_EXTERNAL_PRIMARY);
            uriSavedFile = resolver.insert(collection, valuesFile);
        } else {
            File directory = Environment.getExternalStoragePublicDirectory(isVideo? Environment.DIRECTORY_DCIM : Environment.DIRECTORY_PICTURES);
            File parent = new File(directory.getAbsolutePath() + "/" + fileName);
            if (!parent.exists()) {
                parent.mkdir();
            }
            createdFile = new File(parent, fileName + (isVideo? ".mp4" : ".jpg"));

            valuesFile.put(MediaStore.Video.Media.TITLE, fileName);
            valuesFile.put(MediaStore.Video.Media.DISPLAY_NAME, fileName);
            valuesFile.put(MediaStore.Video.Media.MIME_TYPE, isVideo ? "video/mp4" : "image/jpeg");
            valuesFile.put(
                    MediaStore.Video.Media.DATE_ADDED,
                    System.currentTimeMillis() / 1000);
            valuesFile.put(MediaStore.Video.Media.DATA, createdFile.getAbsolutePath());

            if (isVideo) {
                uriSavedFile = context.getContentResolver().insert(
                        MediaStore.Video.Media.EXTERNAL_CONTENT_URI,
                        valuesFile);
            } else {
                uriSavedFile = context.getContentResolver().insert(
                        MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
                        valuesFile);
            }
        }

        if (Build.VERSION.SDK_INT >= 29) {
            valuesFile.put(MediaStore.Video.Media.DATE_TAKEN, System.currentTimeMillis());
            valuesFile.put(MediaStore.Video.Media.IS_PENDING, 1);
        }

        try {
            if (isVideo) {
                File videoFile = new File(uri.getPath());
                try{
                    ParcelFileDescriptor pfd;
                    Log.i("Unity Utils", "openFileDescriptor:"+uriSavedFile);
                    pfd = context.getContentResolver().openFileDescriptor(uriSavedFile, "w");

                    FileOutputStream out = new FileOutputStream(pfd.getFileDescriptor());
                    // get the already saved video as fileinputstream
                    // The Directory where your file is saved
                    // File storageDir = new File(uri.getPath());
//Directory and the name of your video file to copy
                    FileInputStream in = new FileInputStream(videoFile);
                    byte[] buf = new byte[8192];
                    int len;
                    while ((len = in.read(buf)) > 0) {
                        out.write(buf, 0, len);
                    }

                    out.close();
                    in.close();
                    pfd.close();
                }catch (Exception e){
                    e.printStackTrace();
                    //兼容保存
                    boolean res = saveMp4ToAlbumBeforeQ(context, videoFile);
                    Log.i("Unity Utils", "Android Q以下保存相册结果:"+res);
                }
            } else {
                OutputStream out = context.getContentResolver().openOutputStream(uriSavedFile);
                BitmapFactory.Options options = new BitmapFactory.Options();
                Bitmap bmp = BitmapFactory.decodeFile(uri.getPath(), options);
                bmp.compress(Bitmap.CompressFormat.JPEG, 90, out);
                bmp.recycle();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        if (Build.VERSION.SDK_INT >= 29) {
            valuesFile.clear();
            valuesFile.put(MediaStore.Video.Media.IS_PENDING, 0);
            context.getContentResolver().update(uriSavedFile, valuesFile, null, null);
            return uriSavedFile;
        } else {
            Intent localIntent = new Intent("android.intent.action.MEDIA_SCANNER_SCAN_FILE");
            final Uri localUri = Uri.fromFile(createdFile);
            localIntent.setData(localUri);
            context.sendBroadcast(localIntent);
            return localUri;
        }
    }

    private static boolean saveMp4ToAlbumBeforeQ(Context context, File videoFile) {
        File picDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);
        File destFile = new File(picDir, context.getPackageName() + File.separator + System.currentTimeMillis() + ".mp4");
        OutputStream os = null;
        boolean result = false;
        FileInputStream in = null;
        try {
            if (!destFile.exists()) {
                destFile.getParentFile().mkdirs();
                destFile.createNewFile();
            }
            os = new BufferedOutputStream(new FileOutputStream(destFile));
            in = new FileInputStream(videoFile);
            byte[] buf = new byte[8192];
            int len;
            while ((len = in.read(buf)) > 0) {
                os.write(buf, 0, len);
            }
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            try {
                if (in != null) {
                    in.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
            try {
                if (os != null) {
                    os.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        MediaScannerConnection.scanFile(
                context,
                new String[]{destFile.getAbsolutePath()},
                new String[]{"video/mp4"},
                (path, uri) -> {
                    Log.d("Unity Utils", "saveMp4ToAlbum: " + path + " " + uri);
                    // Scan Completed
                });
        return result;
    }

    /**
     * 删除files文件
     * @param context
     * @param fileName
     * @return
     */
    public static boolean deleteFromFilesDir(Context context, String fileName){
        try{
            File file = new File(context.getFilesDir(), fileName);
            Log.i("Unity Android", "文件:"+file.getAbsolutePath()+" 是否存在:"+file.exists()+" 是否目录:"+file.isDirectory());
            if(!file.exists()){
                return true;
            }
            if(file.isDirectory()){
                File[] files = file.listFiles();
                if(files == null) return true;
                for(File f : files){
                    boolean b = f.delete();
                    Log.i("Unity Android", "删除了文件:"+f.getName()+" Success:"+b);
                }
            }else{
                boolean b = file.delete();
                Log.i("Unity Android", "删除了文件:"+file.getName()+" Success:"+b);
            }
            return true;
        }catch (Exception e){
            e.printStackTrace();
            return false;
        }
    }
}
