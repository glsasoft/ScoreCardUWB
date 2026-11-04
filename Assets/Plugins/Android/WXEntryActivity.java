package cn.com.glsa.app.fightgame.wxapi;
import com.unity3d.player.UnityPlayer;
import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.os.Handler;
import android.os.Looper;

import com.tencent.mm.opensdk.modelbase.BaseReq;
import com.tencent.mm.opensdk.modelbase.BaseResp;
import com.tencent.mm.opensdk.modelmsg.SendAuth;
import com.tencent.mm.opensdk.modelmsg.SendMessageToWX;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;

import org.json.JSONException;
import org.json.JSONObject;

public class WXEntryActivity extends Activity implements IWXAPIEventHandler {

    public static String WX_APP_ID = "";

    private IWXAPI api;

    static final String TAG = "Unity "+WXEntryActivity.class.getSimpleName();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.i(TAG, "onCreate...");
        api = WXAPIFactory.createWXAPI(this, WX_APP_ID, false);
        try {
            Intent intent = getIntent();
            api.handleIntent(intent, this);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    protected void onNewIntent(Intent intent) {
        Log.i(TAG, "onNewIntent...");
        //微信 sdk必须
        super.onNewIntent(intent);
        setIntent(intent);
        api.handleIntent(intent, this);
    }

    @Override
    public void onReq(BaseReq baseReq) {
        Log.i(TAG, "微信授权回调 onReq:"+baseReq.openId+" "+baseReq.transaction);
        this.finish();
    }

    @Override
    public void onResp(BaseResp baseResp) {
        Log.i(TAG, "微信授权回调:"+baseResp.errCode+" err="+baseResp.errStr+" transaction="+baseResp.transaction);
        if(baseResp.transaction.equals("login")){
            Log.i(TAG, "微信授权回调: 微信登录回调...");

            String json = null;
            
            switch (baseResp.errCode) {
                //ERR_OK = 0(用户同意) ERR_AUTH_DENIED = -4（用户拒绝授权） ERR_USER_CANCEL = -2（用户取消）
                case BaseResp.ErrCode.ERR_OK:
                    //UITools.toast(this, "用户同意微信授权");
                    SendAuth.Resp authResp = (SendAuth.Resp) baseResp;
                    Log.i(TAG, "用户登录结果 code="+authResp.code);
                    Log.i(TAG, "用户登录结果 openId="+authResp.openId);
                    Log.i(TAG, "用户登录结果 url="+authResp.url);
                    Log.i(TAG, "用户登录结果 country="+authResp.country);
                    Log.i(TAG, "用户登录结果 lang="+authResp.lang);
                    Log.i(TAG, "用户登录结果 type="+authResp.getType());
                    /* 获取token和用户信息 https://blog.csdn.net/ruiruiddd/article/details/116134653 */
                    try {
                        JSONObject object = new JSONObject();
                        object.put("code", authResp.code);
                        object.put("openId", authResp.openId);
                        object.put("url", authResp.url);
                        object.put("country", authResp.country);
                        object.put("lang", authResp.lang);
                        object.put("type", authResp.getType());
                        object.put("authResult", authResp.authResult);
                        json = object.toString();
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case BaseResp.ErrCode.ERR_USER_CANCEL:
                    Log.i(TAG, "微信授权回调: 用户取消微信登陆...");
                    break;
                case BaseResp.ErrCode.ERR_AUTH_DENIED:
                    Log.i(TAG, "微信授权回调: 用户拒绝微信授权...");
                    break;
                default:
                    break;
            }
            
            Log.i(TAG, "微信授权回调json="+json);
            if(json == null) json = "{}";
            UnityPlayer.UnitySendMessage("LoginController", "AndroidWxLoginCallback", json);
        }else{
            Log.i(TAG, "微信授权回调: 分享回调...");
        }
        finish();
    }
}
