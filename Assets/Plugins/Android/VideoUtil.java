package cn.com.glsa.app.fightgame;

import android.content.Context;
import android.util.Log;

import com.coremedia.iso.boxes.Container;
import com.googlecode.mp4parser.authoring.Movie;
import com.googlecode.mp4parser.authoring.Track;
import com.googlecode.mp4parser.authoring.builder.DefaultMp4Builder;
import com.googlecode.mp4parser.authoring.container.mp4.MovieCreator;
import com.googlecode.mp4parser.authoring.tracks.AppendTrack;
import com.googlecode.mp4parser.authoring.tracks.CroppedTrack;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.RandomAccessFile;
import java.nio.channels.FileChannel;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;


public class VideoUtil {
  static final String TAG = "Unity "+VideoUtil.class.getSimpleName();

  /**
   * 裁剪视频
   * @param timeSpans
   * @param inputFile
   * @param outputFile
   */
  public static void cutVideo(Context context, final String timeSpans, final String inputFile, final String outputFile){
    long t = System.currentTimeMillis();
    List<float[]> timeSpanValues = new ArrayList<>();
    for(String timeSpan : timeSpans.split(";")){
      String[] timeSpanArr = timeSpan.split(",");
      if(timeSpanArr.length == 0) continue;
      timeSpanValues.add(new float[]{Float.parseFloat(timeSpanArr[0]), Float.parseFloat(timeSpanArr[1])});
    }

    File directory = new File(context.getCacheDir(), "tmpVideoFiles");
    if (!directory.exists()) {
      boolean isDirectoryCreated = directory.mkdirs();
      Log.i(TAG, "录屏 目录创建成功:"+isDirectoryCreated);
    }

    List<String> subVideoFiles = new ArrayList<>();

    for(int i=0; i<timeSpanValues.size(); i++){
      float[] span = timeSpanValues.get(i);
      File subVideo = new File(directory, i+".mp4");
      Log.i(TAG, "录屏 裁剪的时间:"+span[0]+"~"+span[1]);
      try {
        cutVideo(inputFile, subVideo.getAbsolutePath(), span[0], span[1]);
        subVideoFiles.add(subVideo.getAbsolutePath());
      } catch (IOException e) {
        e.printStackTrace();
        Log.e(TAG, "录屏 裁剪失败:"+e.getMessage());
      }
    }

    Log.i(TAG, "录屏 裁剪的子视频数量:"+subVideoFiles.size());

    Log.i(TAG, "录屏 开始合并视频...");
    try {
      mergeVideos(subVideoFiles, outputFile);
      Log.i(TAG, "视频合并成功...");
    } catch (Exception e) {
      e.printStackTrace();
      Log.e(TAG, "录屏 合并失败:"+e.getMessage());
    }
    Log.e(TAG, "录屏 CutVideo耗时:"+(System.currentTimeMillis()-t)+"ms");
  }

  /**
   * 裁剪视频
   *
   * @param src 源文件
   * @param dest 输出地址
   * @param startSec 开始时间
   * @param endSec 结束时间
   */
  public static void cutVideo(final String src, final String dest, final double startSec, final double endSec) throws IOException {
    double startSecond = startSec;
    double endSecond = endSec;
    //构造一个movie对象
    Movie movie = MovieCreator.build(src);
    List<Track> tracks = movie.getTracks();
    movie.setTracks(new ArrayList<Track>());

    boolean timeCorrected = false;
    // Here we try to find a track that has sync samples. Since we can only start decoding
    // at such a sample we SHOULD make sure that the start of the new fragment is exactly
    // such a frame
    for (Track track : tracks) {
      if (track.getSyncSamples() != null && track.getSyncSamples().length > 0) {
        if (timeCorrected) {
          // This exception here could be a false positive in case we have multiple tracks
          // with sync samples at exactly the same positions. E.g. a single movie containing
          // multiple qualities of the same video (Microsoft Smooth Streaming file)

          throw new RuntimeException(
                  "The startTime has already been corrected by another track with SyncSample. Not Supported.");
        }
        //矫正开始时间
        startSecond = correctTimeToSyncSample(track, startSecond, false);
        //矫正结束时间
        endSecond = correctTimeToSyncSample(track, endSecond, true);

        timeCorrected = true;
      }
    }

    //裁剪后的位置   startSecond:299400, endSecond:309390
    //矫正后的位置   startSecond:291.3327083333511, endSecond:313.18787500003214
    Log.e(TAG, "录屏 startSecond:" + startSecond + ", endSecond:" + endSecond);

    //fix bug: 部分视频矫正过后会超出10s,这里进行强制限制在10s内
    if (endSecond - startSecond > 10) {
      int duration = (int) (endSec - startSec);
      endSecond = startSecond + duration;
    }
    //fix bug: 部分视频裁剪后endSecond=0.0,导致播放失败
    if (endSecond == 0.0) {
      int duration = (int) (endSec - startSec);
      endSecond = startSecond + duration;
    }

    Log.i(TAG, "录屏 开始裁剪...");
    for (Track track : tracks) {
      Log.i(TAG, "track="+track.getName()+"...");
      long currentSample = 0;
      double currentTime = 0;
      double lastTime = -1;
      long startSample = -1;
      long endSample = -1;

      for (int i = 0; i < track.getSampleDurations().length; i++) {
        long delta = track.getSampleDurations()[i];

        if (currentTime > lastTime && currentTime <= startSecond) {
          // current sample is still before the new starttime
          startSample = currentSample;
        }
        if (currentTime > lastTime && currentTime <= endSecond) {
          // current sample is after the new start time and still before the new endtime
          endSample = currentSample;
        }

        lastTime = currentTime;
        //计算出某一帧的时长 = 采样时长 / 时间长度
        currentTime +=
                (double) delta / (double) track.getTrackMetaData().getTimescale();
        //这里就是帧数（采样）加一
        currentSample++;
      }
      //在这里，裁剪是根据关键帧进行裁剪的，而不是指定的开始时间和结束时间
      //startSample:2453, endSample:2846   393
      //startSample:4795, endSample:5564   769
      Log.e(TAG, "录屏 startSample:" + startSample + ", endSample:" + endSample);
      movie.addTrack(new CroppedTrack(track, startSample, endSample));

      Container out = new DefaultMp4Builder().build(movie);
      FileOutputStream fos = new FileOutputStream(String.format(dest));
      FileChannel fc = fos.getChannel();
      out.writeContainer(fc);

      fc.close();
      fos.close();
    }
    Log.i(TAG, "录屏 裁剪结束..");
  }

  /**
   * 矫正裁剪的sample位置
   *
   * @param track 视频轨道
   * @param cutHere 裁剪位置
   * @param next 是否还继续裁剪
   */
  private static double correctTimeToSyncSample(Track track, double cutHere, boolean next) {
    double[] timeOfSyncSamples = new double[track.getSyncSamples().length];
    long currentSample = 0;
    double currentTime = 0;
    for (int i = 0; i < track.getSampleDurations().length; i++) {
      long delta = track.getSampleDurations()[i];

      if (Arrays.binarySearch(track.getSyncSamples(), currentSample + 1) >= 0) {
        // samples always start with 1 but we start with zero therefore +1（采样的下标从1开始而不是0开始，所以要+1 ）
        timeOfSyncSamples[Arrays
                .binarySearch(track.getSyncSamples(), currentSample + 1)] = currentTime;
      }
      currentTime += (double) delta / (double) track.getTrackMetaData().getTimescale();
      currentSample++;

    }
    double previous = 0;
    for (double timeOfSyncSample : timeOfSyncSamples) {
      if (timeOfSyncSample > cutHere) {
        if (next) {
          return timeOfSyncSample;
        } else {
          return previous;
        }
      }
      previous = timeOfSyncSample;
    }
    return timeOfSyncSamples[timeOfSyncSamples.length - 1];
  }

  private final static String PREFIX_VIDEO_HANDLER = "vide";
  private final static String PREFIX_AUDIO_HANDLER = "soun";

  /**
   * 合并视频
   * @param inputVideos
   * @param outputPath
   * @throws IOException
   */
  public static void mergeVideos(List<String> inputVideos, String outputPath) throws Exception {
    Log.i(TAG, "合并视频 输出:"+outputPath);
    if(inputVideos.size() == 1){
      //单个视频，直接复制到outputPath
      inputStreamToFile(new FileInputStream(inputVideos.get(0)), new File(outputPath));
      return;
    }
    for(String s : inputVideos){
      Log.i(TAG, "合并视频 输入:"+s);
    }
    List<Movie> inputMovies = new ArrayList<>();
    for (String input : inputVideos) {
      inputMovies.add(MovieCreator.build(input));
    }

    List<Track> videoTracks = new LinkedList<>();
    List<Track> audioTracks = new LinkedList<>();

    for (Movie m : inputMovies) {
      for (Track t : m.getTracks()) {
        Log.i(TAG, "视频 轨道 时长:"+t.getDuration()+" Name:"+t.getName()+" SampleDurations数量:"+t.getSampleDurations().length+" Samples数:"+t.getSamples());
        if(t.getDuration() == 0) continue;
        if (PREFIX_AUDIO_HANDLER.equals(t.getHandler())) {
          audioTracks.add(t);
        }
        if (PREFIX_VIDEO_HANDLER.equals(t.getHandler())) {
          videoTracks.add(t);
        }
      }
    }

    Movie outputMovie = new Movie();
    Log.i(TAG, "audioTracks.size()="+audioTracks.size());
    Log.i(TAG, "videoTracks.size()="+videoTracks.size());
    if (audioTracks.size() > 0) {
      outputMovie.addTrack(new AppendTrack(audioTracks.toArray(new Track[audioTracks.size()])));
    }
    Log.i(TAG, "合并 音频 OK 01..");
    if (videoTracks.size() > 0) {
      outputMovie.addTrack(new AppendTrack(videoTracks.toArray(new Track[videoTracks.size()])));
    }
    Log.i(TAG, "合并 音频 OK 02..");

    Container out = new DefaultMp4Builder().build(outputMovie);

    Log.i(TAG, "合并 音频 OK 03..");

    FileChannel fc = new RandomAccessFile(outputPath, "rw").getChannel();

    Log.i(TAG, "合并 音频 OK 04..");

    out.writeContainer(fc);
    Log.i(TAG, "合并 音频 OK 05..");
    fc.close();
    Log.i(TAG, "合并 音频 OK 06..");
  }

  public static void mergeVideosFile(List<File> inputVideos, String outputPath) throws Exception {
    Log.i(TAG, "视频分享 mergeVideosFile 输出:"+outputPath);
    for(File f : inputVideos){
      Log.i(TAG, "视频分享 mergeVideosFile 输入:"+f.getAbsolutePath());
    }
    List<String> files = new ArrayList<>();
    for(File f : inputVideos){
      if(f != null && f.exists()){
        //判断是否是合法视频
        try{
          Movie m = MovieCreator.build(f.getAbsolutePath());
          //至少大于3秒钟才是正常视频
          if(m.getTracks() !=null && m.getTracks().size() > 0 && m.getTracks().get(0).getDuration() >3000){
            files.add(f.getAbsolutePath());
          }else{
            Log.i(TAG, "视频分享 mergeVideosFile视频太短了:"+f.getAbsolutePath());
          }
        }catch (Exception e){
          Log.i(TAG, "视频分享 mergeVideosFile:"+e.getMessage());
          e.printStackTrace();
        }
      }
    }
    Log.i(TAG, "视频分享 调用 开始调用mergeVideos方法...");
    mergeVideos(files, outputPath);
    Log.i(TAG, "视频分享 调用 调用mergeVideos方法结束");
  }

  public static void inputStreamToFile(InputStream inputStream, File file) throws IOException {
    try (FileOutputStream outputStream = new FileOutputStream(file)) {
      int read;
      byte[] bytes = new byte[8192];
      while ((read = inputStream.read(bytes)) != -1) {
        outputStream.write(bytes, 0, read);
      }
    }
  }
}
