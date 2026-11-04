using Cysharp.Threading.Tasks;
using LetsPiu.Main;
using LetsPiu.UI.UIScript.GamesHallScripts.Offline.UI;
using UnityEngine;

namespace LetsPiu.UI.UIScript.GamesHallScripts.Offline.Logic
{

    public class OfflinePageLogic : AbstractPage
    {
        public OfflinePageUI offlinePageUI;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            MqttConnector.Instance.ConnectAsync(MqttConnector.ConnectType.Hall).Forget();
        }

        public override void OnClose()
        {
            base.OnClose();
        }
    }
}
