using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    public class ModelReadyMessage
    {
        public int playerId;             // shortID
        public GameObject model;         // 加载完成的模型

        public ModelReadyMessage(int playerId, GameObject model)
        {
            this.playerId = playerId;
            this.model = model;
        }
    }
}
