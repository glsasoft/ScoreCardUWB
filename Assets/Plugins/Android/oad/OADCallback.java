package cn.com.glsa.app.fightgame.oad;

public interface OADCallback {
    void onSuccess();
    void onError(String error);
    void onProgress(int sentBlocks, int totalBlocks);
}