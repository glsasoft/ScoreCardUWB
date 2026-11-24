using System;
using System.Collections.Generic;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// uid/deviceName → 稳定的 Venue 玩家 id / 队名。击杀数据接入前用默认血量与队别。
    /// </summary>
    [Serializable]
    public class UwbTagRegistry
    {
        [Serializable]
        public class Binding
        {
            public string uid;
            public int playerId;
            public int team;
            public string displayName;
        }

        public List<Binding> bindings = new List<Binding>();
        public int defaultMaxHp = 100;

        readonly Dictionary<string, Binding> _byUid = new Dictionary<string, Binding>(64);
        int _nextAutoId = 1;

        public void Rebuild()
        {
            _byUid.Clear();
            int maxId = 0;
            for (int i = 0; i < bindings.Count; i++)
            {
                Binding b = bindings[i];
                if (b == null || string.IsNullOrEmpty(b.uid))
                    continue;
                _byUid[b.uid] = b;
                if (b.playerId > maxId)
                    maxId = b.playerId;
            }

            _nextAutoId = Math.Max(1, maxId + 1);
        }

        public Binding Resolve(string uid, string deviceName)
        {
            if (string.IsNullOrEmpty(uid))
                uid = deviceName;
            if (string.IsNullOrEmpty(uid))
                return null;

            if (_byUid.TryGetValue(uid, out Binding existing))
                return existing;

            // 也试 deviceName 作 key
            if (!string.IsNullOrEmpty(deviceName) && _byUid.TryGetValue(deviceName, out existing))
                return existing;

            var created = new Binding
            {
                uid = uid,
                playerId = _nextAutoId++,
                team = (_nextAutoId - 1) % 2,
                displayName = string.IsNullOrEmpty(deviceName) ? uid : deviceName
            };
            bindings.Add(created);
            _byUid[uid] = created;
            return created;
        }
    }
}
