using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character
{
    /// <summary>
    /// 服务器没接上之前，在场馆坐标里走几个人，用来看客户端同步。
    /// 接到真实数据后关掉这个组件，改由网络层调用 VenuePlayerSync.PushAll。
    /// </summary>
    [DisallowMultipleComponent]
    public class VenuePlayerMockFeed : MonoBehaviour
    {
        sealed class Agent
        {
            public int id;
            public int team;
            public string name;
            public Vector3 pos;
            public Vector3 target;
            public float yaw;
            public int hp;
            public int maxHp;
            public float fireIn;
            public float deadFor;
            public bool firing;
        }

        [Tooltip("接到服务器后取消勾选，或直接删掉这个组件。")]
        public bool playing = true;

        public VenuePlayerSync sync;
        public int playerCount = 4;
        public float areaLength = 24f;
        public float areaWidth = 16f;
        public float moveSpeed = 3.2f;
        public float sendInterval = 0.1f;
        public int maxHp = 100;

        Agent[] _agents;
        readonly List<VenuePlayerSnapshot> _buffer = new List<VenuePlayerSnapshot>(8);
        float _send;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<VenuePlayerSync>();
        }

        void OnEnable()
        {
            int count = Mathf.Max(1, playerCount);
            _agents = new Agent[count];
            for (int i = 0; i < count; i++)
                _agents[i] = CreateAgent(i);
        }

        void Update()
        {
            if (!playing || sync == null || _agents == null)
                return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _agents.Length; i++)
                Step(_agents[i], dt);

            _send += dt;
            if (_send < sendInterval)
                return;
            _send = 0f;
            Publish();
        }

        void Publish()
        {
            double now = Time.timeAsDouble;
            _buffer.Clear();
            for (int i = 0; i < _agents.Length; i++)
            {
                Agent agent = _agents[i];
                _buffer.Add(new VenuePlayerSnapshot
                {
                    id = agent.id,
                    team = agent.team,
                    name = agent.name,
                    x = agent.pos.x,
                    y = agent.pos.y,
                    z = agent.pos.z,
                    yaw = agent.yaw,
                    hp = agent.hp,
                    maxHp = agent.maxHp,
                    firing = agent.firing,
                    time = now
                });
            }
            sync.PushAll(_buffer);
        }

        void Step(Agent agent, float dt)
        {
            agent.firing = false;
            if (agent.hp <= 0)
            {
                agent.deadFor -= dt;
                if (agent.deadFor <= 0f)
                {
                    agent.hp = agent.maxHp;
                    agent.pos = RandomPoint();
                    agent.target = RandomPoint();
                }
                return;
            }

            Vector3 to = agent.target - agent.pos;
            to.y = 0f;
            if (to.magnitude < 0.6f)
            {
                agent.target = RandomPoint();
            }
            else
            {
                Vector3 dir = to.normalized;
                agent.pos += dir * (moveSpeed * dt);
                agent.yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            }

            agent.fireIn -= dt;
            if (agent.fireIn > 0f)
                return;
            agent.firing = true;
            agent.fireIn = Random.Range(1.2f, 2.8f);
            TryHit(agent);
        }

        void TryHit(Agent agent)
        {
            float yawRad = agent.yaw * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
            Agent best = null;
            float bestDist = 22f;
            for (int i = 0; i < _agents.Length; i++)
            {
                Agent other = _agents[i];
                if (other == agent || other.hp <= 0 || other.team == agent.team)
                    continue;
                Vector3 to = other.pos - agent.pos;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist > bestDist || dist < 0.01f)
                    continue;
                if (Vector3.Dot(forward, to / dist) < 0.5f)
                    continue;
                best = other;
                bestDist = dist;
            }
            if (best == null)
                return;
            best.hp = Mathf.Max(0, best.hp - 25);
            if (best.hp == 0)
                best.deadFor = 4f;
        }

        Agent CreateAgent(int index)
        {
            int team = index % 2;
            return new Agent
            {
                id = index + 1,
                team = team,
                name = (team == 0 ? "蓝" : "红") + (index / 2 + 1),
                maxHp = maxHp,
                hp = maxHp,
                pos = RandomPoint(),
                target = RandomPoint(),
                fireIn = Random.Range(0.4f, 2f)
            };
        }

        Vector3 RandomPoint()
        {
            return new Vector3(
                Random.Range(-areaLength * 0.5f, areaLength * 0.5f),
                0f,
                Random.Range(-areaWidth * 0.5f, areaWidth * 0.5f));
        }

        void OnDrawGizmosSelected()
        {
            VenueSpace venue = GetComponent<VenueSpace>();
            Vector3 a = Corner(-0.5f, -0.5f, venue);
            Vector3 b = Corner(0.5f, -0.5f, venue);
            Vector3 c = Corner(0.5f, 0.5f, venue);
            Vector3 d = Corner(-0.5f, 0.5f, venue);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        Vector3 Corner(float x, float z, VenueSpace venue)
        {
            Vector3 venuePoint = new Vector3(x * areaLength, 0.05f, z * areaWidth);
            return venue != null ? venue.ToWorld(venuePoint) : venuePoint;
        }
    }
}
