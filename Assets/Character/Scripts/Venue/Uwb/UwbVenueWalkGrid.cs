using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 从场景墙体碰撞体（WallBox_*、Collider/Wall*）采样可走网格，
    /// 并额外把无 Collider 的障碍箱（s_slg_obstruct*）按包围盒打孔。
    /// 供 Mock A* 漫游与后续客户端纠偏复用。
    /// </summary>
    public sealed class UwbVenueWalkGrid
    {
        const float DefaultCell = 0.5f;
        const float SampleHeightMeters = 0.25f;
        // 略减小半径，避免窄门被采成断连，便于跨区漫游
        const float AgentRadiusMeters = 0.22f;
        const float ObstructClearanceMeters = 0.12f;
        const float MapBoundsPaddingMeters = 1.5f;
        const string ObstructNamePrefix = "s_slg_obstruct";

        readonly float _originX;
        readonly float _originZ;
        readonly float _cell;
        readonly int _cols;
        readonly int _rows;
        // 开阔格代价接近 1，避免全体贴墙走成蚁群长队
        const float OpenSpaceCostMul = 1.05f;

        readonly bool[] _walkable;
        readonly bool[] _nearWall;
        readonly int[] _component;
        readonly List<int> _walkableCells = new List<int>(256);
        readonly List<int> _nearWallCells = new List<int>(128);
        readonly List<int> _openCells = new List<int>(128);
        readonly Dictionary<int, List<int>> _cellsByComponent = new Dictionary<int, List<int>>(16);
        readonly Dictionary<int, List<int>> _nearWallByComponent = new Dictionary<int, List<int>>(16);
        readonly Dictionary<int, List<int>> _openByComponent = new Dictionary<int, List<int>>(16);
        int _largestComponent = -1;

        static readonly int[] OrthDx = { 1, -1, 0, 0 };
        static readonly int[] OrthDz = { 0, 0, 1, -1 };
        static readonly int[] NeighborDx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        static readonly int[] NeighborDz = { 0, 0, 1, -1, 1, -1, 1, -1 };
        static readonly float[] NeighborCost =
        {
            1f, 1f, 1f, 1f,
            1.41421356f, 1.41421356f, 1.41421356f, 1.41421356f
        };

        public bool Ready => _walkableCells.Count > 0;
        public int WalkableCount => _walkableCells.Count;
        public int NearWallCount => _nearWallCells.Count;
        public int ComponentCount => _cellsByComponent.Count;
        public float CellSize => _cell;
        public int LargestComponentSize =>
            _largestComponent >= 0 && _cellsByComponent.TryGetValue(_largestComponent, out List<int> list)
                ? list.Count
                : 0;

        UwbVenueWalkGrid(float originX, float originZ, float cell, int cols, int rows, bool[] walkable)
        {
            _originX = originX;
            _originZ = originZ;
            _cell = cell;
            _cols = cols;
            _rows = rows;
            _walkable = walkable;
            _nearWall = new bool[walkable.Length];
            _component = new int[walkable.Length];
            for (int i = 0; i < _component.Length; i++)
                _component[i] = -1;

            for (int i = 0; i < walkable.Length; i++)
            {
                if (walkable[i])
                    _walkableCells.Add(i);
            }

            LabelComponents();
            BuildNearWallLists();
        }

        /// <summary>
        /// 按 WallBox/障碍箱包围盒自动覆盖整张地图（允许不对称），避免中心对称裁切漏掉半边场地。
        /// </summary>
        public static UwbVenueWalkGrid BuildForMap(
            VenueSpace space,
            float fallbackAreaX = 24f,
            float fallbackAreaZ = 24f,
            float cellSize = DefaultCell,
            int mapLayer = Layer.Obstacles,
            float agentRadius = AgentRadiusMeters)
        {
            if (space == null)
                return null;

            float originX;
            float originZ;
            float sizeX;
            float sizeZ;
            if (TryEstimateVenueBounds(space, out float minX, out float maxX, out float minZ, out float maxZ))
            {
                originX = minX - MapBoundsPaddingMeters;
                originZ = minZ - MapBoundsPaddingMeters;
                sizeX = (maxX - minX) + MapBoundsPaddingMeters * 2f;
                sizeZ = (maxZ - minZ) + MapBoundsPaddingMeters * 2f;
                Debug.Log(
                    $"[UwbWalkGrid] mapBounds venue x=[{minX:0.##},{maxX:0.##}] z=[{minZ:0.##},{maxZ:0.##}] " +
                    $"grid=[{originX:0.##},{originX + sizeX:0.##}]x[{originZ:0.##},{originZ + sizeZ:0.##}]");
            }
            else
            {
                sizeX = Mathf.Max(1f, fallbackAreaX);
                sizeZ = Mathf.Max(1f, fallbackAreaZ);
                originX = -sizeX * 0.5f;
                originZ = -sizeZ * 0.5f;
                Debug.LogWarning("[UwbWalkGrid] 无法估地图包围盒，回退中心对称区域");
            }

            return BuildRect(space, originX, originZ, sizeX, sizeZ, cellSize, mapLayer, agentRadius);
        }

        public static UwbVenueWalkGrid Build(
            VenueSpace space,
            float areaX,
            float areaZ,
            float cellSize = DefaultCell,
            int mapLayer = Layer.Obstacles)
        {
            float sizeX = Mathf.Max(1f, areaX);
            float sizeZ = Mathf.Max(1f, areaZ);
            return BuildRect(space, -sizeX * 0.5f, -sizeZ * 0.5f, sizeX, sizeZ, cellSize, mapLayer);
        }

        static UwbVenueWalkGrid BuildRect(
            VenueSpace space,
            float originX,
            float originZ,
            float sizeX,
            float sizeZ,
            float cellSize,
            int mapLayer,
            float agentRadius = AgentRadiusMeters)
        {
            float cell = Mathf.Max(0.25f, cellSize);
            int cols = Mathf.Max(2, Mathf.CeilToInt(sizeX / cell));
            int rows = Mathf.Max(2, Mathf.CeilToInt(sizeZ / cell));
            var walkable = new bool[cols * rows];
            int layerMask = 1 << mapLayer;
            float radiusWorld = agentRadius * Mathf.Max(0.0001f, space.unityUnitsPerMeter);

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    float vx = originX + (x + 0.5f) * cell;
                    float vz = originZ + (z + 0.5f) * cell;
                    Vector3 world = space.ToWorld(new Vector3(vx, SampleHeightMeters, vz));
                    bool blocked = IsBlocked(world, radiusWorld, layerMask);
                    walkable[Index(x, z, cols)] = !blocked;
                }
            }

            int stamped = StampObstructBoxes(space, walkable, originX, originZ, cell, cols, rows);
            var grid = new UwbVenueWalkGrid(originX, originZ, cell, cols, rows, walkable);
            Debug.Log(
                $"[UwbWalkGrid] build cells={cols}x{rows} origin=({originX:0.##},{originZ:0.##}) " +
                $"walkable={grid.WalkableCount}/{walkable.Length} nearWall={grid.NearWallCount} " +
                $"components={grid.ComponentCount} largest={grid.LargestComponentSize} " +
                $"obstructStamped={stamped} cell={cell}");
            return grid;
        }

        static bool TryEstimateVenueBounds(
            VenueSpace space,
            out float minX,
            out float maxX,
            out float minZ,
            out float maxZ)
        {
            minX = maxX = minZ = maxZ = 0f;
            bool any = false;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                Transform[] transforms = roots[r].GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform t = transforms[i];
                    bool isWall = t.name.StartsWith("WallBox_", System.StringComparison.Ordinal);
                    bool isBox = t.name.StartsWith(ObstructNamePrefix, System.StringComparison.Ordinal);
                    if (!isWall && !isBox)
                        continue;
                    if (isBox && t.parent != null &&
                        t.parent.name.StartsWith(ObstructNamePrefix, System.StringComparison.Ordinal))
                        continue;

                    Bounds b;
                    Collider col = t.GetComponent<Collider>();
                    if (col != null)
                    {
                        b = col.bounds;
                    }
                    else
                    {
                        Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
                        if (rends == null || rends.Length == 0)
                            continue;
                        b = rends[0].bounds;
                        for (int k = 1; k < rends.Length; k++)
                            b.Encapsulate(rends[k].bounds);
                    }

                    Vector3 a = space.FromWorld(new Vector3(b.min.x, b.center.y, b.min.z));
                    Vector3 c = space.FromWorld(new Vector3(b.max.x, b.center.y, b.max.z));
                    float x0 = Mathf.Min(a.x, c.x);
                    float x1 = Mathf.Max(a.x, c.x);
                    float z0 = Mathf.Min(a.z, c.z);
                    float z1 = Mathf.Max(a.z, c.z);
                    if (!any)
                    {
                        minX = x0;
                        maxX = x1;
                        minZ = z0;
                        maxZ = z1;
                        any = true;
                    }
                    else
                    {
                        if (x0 < minX) minX = x0;
                        if (x1 > maxX) maxX = x1;
                        if (z0 < minZ) minZ = z0;
                        if (z1 > maxZ) maxZ = z1;
                    }
                }
            }

            return any && (maxX - minX) > 2f && (maxZ - minZ) > 2f;
        }

        /// <summary>
        /// 场景里 8 个箱子无 Collider，按 Renderer 包围盒在网格上打孔（含人体半径余量）。
        /// </summary>
        static int StampObstructBoxes(
            VenueSpace space,
            bool[] walkable,
            float originX,
            float originZ,
            float cell,
            int cols,
            int rows)
        {
            var boxes = new List<Bounds>(8);
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                Transform[] transforms = roots[r].GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform t = transforms[i];
                    if (!t.name.StartsWith(ObstructNamePrefix, System.StringComparison.Ordinal))
                        continue;
                    // 只取父节点（实例根），避免重复打孔子物体
                    if (t.parent != null &&
                        t.parent.name.StartsWith(ObstructNamePrefix, System.StringComparison.Ordinal))
                        continue;

                    Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
                    if (rends == null || rends.Length == 0)
                        continue;

                    Bounds b = rends[0].bounds;
                    for (int k = 1; k < rends.Length; k++)
                        b.Encapsulate(rends[k].bounds);
                    boxes.Add(b);
                }
            }

            if (boxes.Count == 0)
                return 0;

            // 障碍箱的外扩与墙体半径无关：不同网格（Mock 漫游/纠偏）里箱子间的缝隙要一致，否则拓扑对不上
            float pad = AgentRadiusMeters + ObstructClearanceMeters;
            int blockedCells = 0;
            for (int bi = 0; bi < boxes.Count; bi++)
            {
                Bounds wb = boxes[bi];
                // 世界包围盒四角 → 场馆 XZ，再扩一圈人体半径
                Vector3 c000 = space.FromWorld(new Vector3(wb.min.x, wb.center.y, wb.min.z));
                Vector3 c100 = space.FromWorld(new Vector3(wb.max.x, wb.center.y, wb.min.z));
                Vector3 c010 = space.FromWorld(new Vector3(wb.min.x, wb.center.y, wb.max.z));
                Vector3 c110 = space.FromWorld(new Vector3(wb.max.x, wb.center.y, wb.max.z));

                float minX = Mathf.Min(Mathf.Min(c000.x, c100.x), Mathf.Min(c010.x, c110.x)) - pad;
                float maxX = Mathf.Max(Mathf.Max(c000.x, c100.x), Mathf.Max(c010.x, c110.x)) + pad;
                float minZ = Mathf.Min(Mathf.Min(c000.z, c100.z), Mathf.Min(c010.z, c110.z)) - pad;
                float maxZ = Mathf.Max(Mathf.Max(c000.z, c100.z), Mathf.Max(c010.z, c110.z)) + pad;

                int x0 = Mathf.Clamp(Mathf.FloorToInt((minX - originX) / cell), 0, cols - 1);
                int x1 = Mathf.Clamp(Mathf.FloorToInt((maxX - originX) / cell), 0, cols - 1);
                int z0 = Mathf.Clamp(Mathf.FloorToInt((minZ - originZ) / cell), 0, rows - 1);
                int z1 = Mathf.Clamp(Mathf.FloorToInt((maxZ - originZ) / cell), 0, rows - 1);

                for (int z = z0; z <= z1; z++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int idx = Index(x, z, cols);
                        if (!walkable[idx])
                            continue;
                        walkable[idx] = false;
                        blockedCells++;
                    }
                }
            }

            Debug.Log($"[UwbWalkGrid] stamped obstruct boxes={boxes.Count} cells={blockedCells}");
            return boxes.Count;
        }

        public bool IsWalkableVenue(float venueX, float venueZ)
        {
            if (!TryVenueToCell(venueX, venueZ, out int cx, out int cz))
                return false;
            return _walkable[Index(cx, cz, _cols)];
        }

        /// <summary>
        /// 路径拉直：从起点开始，每次跳到最远的、与当前点直线可走的路点，去掉 A* 格点阶梯造成的 S 形折线。
        /// </summary>
        public void SimplifyPath(List<Vector2> path)
        {
            if (path == null || path.Count < 3)
                return;

            var result = new List<Vector2>(path.Count);
            result.Add(path[0]);
            int cur = 0;
            while (cur < path.Count - 1)
            {
                int next = cur + 1;
                for (int j = path.Count - 1; j > cur + 1; j--)
                {
                    if (SegmentClear(path[cur], path[j]))
                    {
                        next = j;
                        break;
                    }
                }

                result.Add(path[next]);
                cur = next;
            }

            path.Clear();
            path.AddRange(result);
        }

        /// <summary>
        /// 比 SegmentWalkable 更严：线段中段（离两端超过一格）还必须是不贴墙的开阔格，
        /// 拉直路径时用，避免直线蹭着墙角擦过去。
        /// </summary>
        bool SegmentClear(Vector2 a, Vector2 b)
        {
            float len = Vector2.Distance(a, b);
            int n = Mathf.Max(1, Mathf.CeilToInt(len / (_cell * 0.5f)));
            for (int i = 1; i <= n; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, i / (float)n);
                if (!IsWalkableVenue(p.x, p.y))
                    return false;
                if (Vector2.Distance(p, a) > _cell && Vector2.Distance(p, b) > _cell &&
                    IsNearWall(p.x, p.y))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 线段 a→b 是否全程落在可走格内（半格步长采样），用来判断是否穿墙/穿角。
        /// </summary>
        public bool SegmentWalkable(Vector2 a, Vector2 b)
        {
            float len = Vector2.Distance(a, b);
            int n = Mathf.Max(1, Mathf.CeilToInt(len / (_cell * 0.5f)));
            for (int i = 1; i <= n; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, i / (float)n);
                if (!IsWalkableVenue(p.x, p.y))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 是否靠近拐角（自身可走，且正交邻域至少两边被墙挡住）。
        /// </summary>
        public bool IsNearCorner(float venueX, float venueZ)
        {
            if (!TryVenueToCell(venueX, venueZ, out int cx, out int cz))
                return false;
            if (!_walkable[Index(cx, cz, _cols)])
                return false;

            int blocked = 0;
            for (int k = 0; k < 4; k++)
            {
                int nx = cx + OrthDx[k];
                int nz = cz + OrthDz[k];
                if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows || !_walkable[Index(nx, nz, _cols)])
                    blocked++;
            }

            return blocked >= 2;
        }

        /// <summary>
        /// 是否贴墙（自身可走且正交邻域有墙）。
        /// </summary>
        public bool IsNearWall(float venueX, float venueZ)
        {
            if (!TryVenueToCell(venueX, venueZ, out int cx, out int cz))
                return false;
            if (!_walkable[Index(cx, cz, _cols)])
                return true;

            for (int k = 0; k < 4; k++)
            {
                int nx = cx + OrthDx[k];
                int nz = cz + OrthDz[k];
                if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows || !_walkable[Index(nx, nz, _cols)])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 拐角单向跨墙。室内 UWB 拐角多径常见约 0.4~0.7m（按 ~0.5m 量级取候选）。
        /// </summary>
        public bool TryCornerCross(Vector2 from, out Vector2 landed, out float jumpMeters)
        {
            landed = from;
            jumpMeters = 0f;
            if (!Ready)
                return false;
            if (!IsNearCorner(from.x, from.y))
                return false;
            if (!TryVenueToCell(from.x, from.y, out int cx, out int cz))
                return false;

            int[] order = { 0, 1, 2, 3, 4, 5, 6, 7 };
            for (int i = order.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = order[i];
                order[i] = order[j];
                order[j] = tmp;
            }

            int startComp = _component[Index(cx, cz, _cols)];
            Vector2 best = default;
            float bestScore = float.PositiveInfinity;
            bool found = false;

            for (int oi = 0; oi < order.Length; oi++)
            {
                int k = order[oi];
                int dx = NeighborDx[k];
                int dz = NeighborDz[k];
                int nx = cx + dx;
                int nz = cz + dz;
                if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows)
                    continue;
                if (_walkable[Index(nx, nz, _cols)])
                    continue;

                for (int step = 2; step <= 4; step++)
                {
                    int tx = cx + dx * step;
                    int tz = cz + dz * step;
                    if (tx < 0 || tz < 0 || tx >= _cols || tz >= _rows)
                        break;
                    int ti = Index(tx, tz, _cols);
                    if (!_walkable[ti])
                        continue;

                    CellCenter(ti, out float lx, out float lz);
                    float dist = Vector2.Distance(from, new Vector2(lx, lz));
                    // 真机拐角跳点量级：约 0.3~0.85m，偏好贴近 0.5m
                    if (dist < 0.3f || dist > 0.85f)
                        continue;

                    float score = Mathf.Abs(dist - 0.5f);
                    if (startComp >= 0 && _component[ti] != startComp)
                        score -= 0.15f;

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = new Vector2(lx, lz);
                        jumpMeters = dist;
                        found = true;
                    }

                    break;
                }
            }

            if (!found)
                return false;
            landed = best;
            return true;
        }

        /// <summary>
        /// 沿前进方向短距穿墙。真机常见墙厚+多径约 0.35~0.7m，侧偏约 ±0.15m。
        /// </summary>
        public bool TryForwardCross(Vector2 from, Vector2 forward, out Vector2 landed, out float jumpMeters)
        {
            landed = from;
            jumpMeters = 0f;
            if (!Ready)
                return false;
            if (forward.sqrMagnitude < 0.0001f)
                return false;

            Vector2 dir = forward.normalized;
            Vector2 side = new Vector2(-dir.y, dir.x);
            float lateral = Random.Range(-0.15f, 0.15f);

            bool hitWall = false;
            for (float d = 0.25f; d <= 0.7f; d += 0.1f)
            {
                Vector2 p = from + dir * d;
                if (!IsWalkableVenue(p.x, p.y))
                {
                    hitWall = true;
                    break;
                }
            }

            if (!hitWall)
                return false;

            Vector2 best = default;
            float bestScore = float.PositiveInfinity;
            bool found = false;
            for (float d = 0.4f; d <= 0.8f; d += 0.05f)
            {
                Vector2 p = from + dir * d + side * lateral;
                if (!IsWalkableVenue(p.x, p.y))
                    continue;

                float dist = Vector2.Distance(from, p);
                if (dist < 0.3f || dist > 0.75f)
                    continue;

                float score = Mathf.Abs(dist - 0.5f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = p;
                    jumpMeters = dist;
                    found = true;
                }
            }

            if (!found)
                return false;
            landed = best;
            return true;
        }

        public bool TryRandomWalkable(out Vector2 venueXz)
        {
            venueXz = default;
            if (_walkableCells.Count == 0)
                return false;
            int idx = _walkableCells[Random.Range(0, _walkableCells.Count)];
            CellCenter(idx, out venueXz.x, out venueXz.y);
            return true;
        }

        /// <summary>
        /// 优先抽贴墙/门框旁的可走点（preferNearWall 概率），否则任意可走。
        /// </summary>
        public bool TryRandomWalkablePreferNearWall(out Vector2 venueXz, float preferNearWall = 0.85f)
        {
            venueXz = default;
            if (_nearWallCells.Count > 0 && Random.value < preferNearWall)
            {
                int idx = _nearWallCells[Random.Range(0, _nearWallCells.Count)];
                CellCenter(idx, out venueXz.x, out venueXz.y);
                return true;
            }

            return TryRandomWalkable(out venueXz);
        }

        public bool TryRandomReachableFrom(Vector2 fromVenueXz, out Vector2 venueXz)
        {
            return TryRandomReachableFrom(fromVenueXz, out venueXz, preferNearWall: 0f);
        }

        /// <summary>
        /// 同连通域内抽目标；preferNearWall>0 时多数目标落在贴墙格。
        /// </summary>
        public bool TryRandomReachableFrom(Vector2 fromVenueXz, out Vector2 venueXz, float preferNearWall)
        {
            return TryRandomReachableFrom(fromVenueXz, out venueXz, preferNearWall, minDistance: 0f);
        }

        /// <summary>
        /// 同连通域抽目标（兼容旧调用）。内部改为距离带抽样，不再“越远越好”。
        /// </summary>
        public bool TryRandomReachableFrom(
            Vector2 fromVenueXz,
            out Vector2 venueXz,
            float preferNearWall,
            float minDistance,
            float preferOpen = 0f)
        {
            float maxDistance = Mathf.Max(minDistance + 4f, minDistance * 1.6f);
            if (minDistance <= 0.01f)
            {
                maxDistance = 1000f;
                minDistance = 0f;
            }

            return TryRandomReachableInBand(
                fromVenueXz, out venueXz, minDistance, maxDistance, preferNearWall, preferOpen);
        }

        /// <summary>
        /// 在 [minDist, maxDist] 距离带内均匀抽目标；可选限制到指定 3x3 扇区。
        /// </summary>
        public bool TryRandomReachableInBand(
            Vector2 fromVenueXz,
            out Vector2 venueXz,
            float minDistance,
            float maxDistance,
            float preferNearWall = 0.2f,
            float preferOpen = 0.55f,
            int preferSector = -1)
        {
            venueXz = default;
            if (!TryResolveCell(fromVenueXz.x, fromVenueXz.y, out int cell))
                return TrySpawnInSector(0, 1, out venueXz, preferNearWall, preferOpen);

            int comp = _component[cell];
            if (comp < 0 || !_cellsByComponent.TryGetValue(comp, out List<int> list) || list.Count == 0)
                return TrySpawnInSector(0, 1, out venueXz, preferNearWall, preferOpen);

            List<int> pool = PickPool(comp, list, preferNearWall, preferOpen);
            float minSqr = minDistance * minDistance;
            float maxSqr = maxDistance * maxDistance;

            for (int tries = 0; tries < 48; tries++)
            {
                int idx = pool[Random.Range(0, pool.Count)];
                CellCenter(idx, out float px, out float pz);
                if (preferSector >= 0 && !IsInSector(px, pz, preferSector))
                    continue;
                float dx = px - fromVenueXz.x;
                float dz = pz - fromVenueXz.y;
                float sqr = dx * dx + dz * dz;
                if (sqr < minSqr || sqr > maxSqr)
                    continue;
                venueXz = new Vector2(px, pz);
                return true;
            }

            for (int tries = 0; tries < 24; tries++)
            {
                int idx = pool[Random.Range(0, pool.Count)];
                CellCenter(idx, out float px, out float pz);
                float dx = px - fromVenueXz.x;
                float dz = pz - fromVenueXz.y;
                if (dx * dx + dz * dz < minSqr)
                    continue;
                venueXz = new Vector2(px, pz);
                return true;
            }

            int any = list[Random.Range(0, list.Count)];
            CellCenter(any, out venueXz.x, out venueXz.y);
            return true;
        }

        public bool IsInSector(float venueX, float venueZ, int sector)
        {
            int s = ((sector % 9) + 9) % 9;
            int sx = s % 3;
            int sz = s / 3;
            float sizeX = _cols * _cell;
            float sizeZ = _rows * _cell;
            float x0 = _originX + sizeX * (sx / 3f);
            float x1 = _originX + sizeX * ((sx + 1) / 3f);
            float z0 = _originZ + sizeZ * (sz / 3f);
            float z1 = _originZ + sizeZ * ((sz + 1) / 3f);
            return venueX >= x0 && venueX <= x1 && venueZ >= z0 && venueZ <= z1;
        }

        static float CellNoise01(int cellIndex, int salt)
        {
            unchecked
            {
                uint h = (uint)(cellIndex * 374761393 + salt * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        /// <summary>
        /// 按扇区分散出生，避免所有人挤在同一角。
        /// </summary>
        public bool TrySpawnInSector(
            int slot,
            int totalSlots,
            out Vector2 venueXz,
            float preferNearWall = 0.2f,
            float preferOpen = 0.55f)
        {
            venueXz = default;
            if (_largestComponent < 0 ||
                !_cellsByComponent.TryGetValue(_largestComponent, out List<int> list) ||
                list.Count == 0)
                return TryRandomWalkable(out venueXz);

            int sectors = Mathf.Max(1, totalSlots);
            int sector = ((slot % sectors) + sectors) % sectors;
            int sx = sector % 3;
            int sz = sector / 3;
            if (sectors <= 3)
            {
                sx = sector;
                sz = 1;
            }
            else if (sectors <= 6)
            {
                sx = sector % 3;
                sz = sector / 3;
            }
            else
            {
                sx = sector % 3;
                sz = (sector / 3) % 3;
            }

            float minX = _originX;
            float minZ = _originZ;
            float sizeX = _cols * _cell;
            float sizeZ = _rows * _cell;
            float x0 = minX + sizeX * (sx / 3f);
            float x1 = minX + sizeX * ((sx + 1) / 3f);
            float z0 = minZ + sizeZ * (sz / 3f);
            float z1 = minZ + sizeZ * ((sz + 1) / 3f);

            List<int> pool = PickPool(_largestComponent, list, preferNearWall, preferOpen);
            for (int tries = 0; tries < 48; tries++)
            {
                int idx = pool[Random.Range(0, pool.Count)];
                CellCenter(idx, out float px, out float pz);
                if (px < x0 || px > x1 || pz < z0 || pz > z1)
                    continue;
                venueXz = new Vector2(px, pz);
                return true;
            }

            // 扇区没命中：全图随机
            int any = list[Random.Range(0, list.Count)];
            CellCenter(any, out venueXz.x, out venueXz.y);
            return true;
        }

        public bool TryRandomInLargestComponent(
            out Vector2 venueXz,
            float preferNearWall = 0.2f,
            float preferOpen = 0.55f)
        {
            return TrySpawnInSector(Random.Range(0, 9), 9, out venueXz, preferNearWall, preferOpen);
        }

        /// <summary>
        /// 最大连通域内均匀随机一点（中央/墙边/角落概率按可走格面积，不做扇区偏好）。
        /// </summary>
        public bool TryRandomUniformInLargestComponent(out Vector2 venueXz)
        {
            venueXz = default;
            if (_largestComponent < 0 ||
                !_cellsByComponent.TryGetValue(_largestComponent, out List<int> list) ||
                list.Count == 0)
                return TryRandomWalkable(out venueXz);

            int idx = list[Random.Range(0, list.Count)];
            CellCenter(idx, out venueXz.x, out venueXz.y);
            return true;
        }

        /// <summary>
        /// 沿行进方向左右各探 probe 米，哪侧是墙/障碍就返回指向该侧的单位法线；两侧都是墙则随机一侧。
        /// </summary>
        public bool TryWallSideNormal(Vector2 pos, Vector2 heading, float probe, out Vector2 normalToWall)
        {
            normalToWall = default;
            if (heading.sqrMagnitude < 1e-6f)
                return false;

            heading.Normalize();
            Vector2 left = new Vector2(-heading.y, heading.x);
            bool blockedL = !IsWalkableVenue(pos.x + left.x * probe, pos.y + left.y * probe);
            bool blockedR = !IsWalkableVenue(pos.x - left.x * probe, pos.y - left.y * probe);
            if (!blockedL && !blockedR)
                return false;

            if (blockedL && blockedR)
                normalToWall = Random.value < 0.5f ? left : -left;
            else
                normalToWall = blockedL ? left : -left;
            return true;
        }

        const float HeatBlockM = 3f;
        const float HeatTauSec = 120f;
        float[] _heat;
        int _heatCols;
        int _heatRows;

        void EnsureHeat()
        {
            if (_heat != null)
                return;
            _heatCols = Mathf.Max(1, Mathf.CeilToInt(_cols * _cell / HeatBlockM));
            _heatRows = Mathf.Max(1, Mathf.CeilToInt(_rows * _cell / HeatBlockM));
            _heat = new float[_heatCols * _heatRows];
        }

        int HeatIndexOfCell(int cell)
        {
            CellCenter(cell, out float x, out float z);
            return HeatIndexOfVenue(x, z);
        }

        int HeatIndexOfVenue(float x, float z)
        {
            int hx = Mathf.Clamp(Mathf.FloorToInt((x - _originX) / HeatBlockM), 0, _heatCols - 1);
            int hz = Mathf.Clamp(Mathf.FloorToInt((z - _originZ) / HeatBlockM), 0, _heatRows - 1);
            return hz * _heatCols + hx;
        }

        /// <summary>
        /// 选中的目标所在区块记一次热度；热度随时间指数衰减，用来让各房间被走到的频率趋于均衡。
        /// </summary>
        public void AddGoalHeat(Vector2 venueXz, float amount = 1f)
        {
            EnsureHeat();
            _heat[HeatIndexOfVenue(venueXz.x, venueXz.y)] += amount;
        }

        public void DecayGoalHeat(float dt)
        {
            if (_heat == null)
                return;
            float k = Mathf.Exp(-dt / HeatTauSec);
            for (int i = 0; i < _heat.Length; i++)
                _heat[i] *= k;
        }

        /// <summary>
        /// 目标候选：从 from 出发 [minDist, maxDist] 内、同连通域、优先对角方向的可走格，
        /// 按区块热度加权（冷门房间权重大）无放回抽 count 个。
        /// </summary>
        public int PickGoalCandidates(
            Vector2 from,
            float minDist,
            float maxDist,
            int count,
            List<Vector2> outGoals)
        {
            outGoals.Clear();
            if (!Ready || maxDist < minDist || count <= 0)
                return 0;
            EnsureHeat();

            int comp = _largestComponent;
            if (TryResolveCell(from.x, from.y, out int fromCell) && _component[fromCell] >= 0)
                comp = _component[fromCell];

            int fcx = Mathf.FloorToInt((from.x - _originX) / _cell);
            int fcz = Mathf.FloorToInt((from.y - _originZ) / _cell);
            int rad = Mathf.CeilToInt(maxDist / _cell);
            int x0 = Mathf.Max(0, fcx - rad);
            int x1 = Mathf.Min(_cols - 1, fcx + rad);
            int z0 = Mathf.Max(0, fcz - rad);
            int z1 = Mathf.Min(_rows - 1, fcz + rad);

            float minSqr = minDist * minDist;
            float maxSqr = maxDist * maxDist;

            var cand = new List<int>(256);
            // 方向约束逐级放宽：严格对角 → 宽松对角 → 任意方向
            for (int level = 0; level < 3 && cand.Count < 6; level++)
            {
                cand.Clear();
                for (int z = z0; z <= z1; z++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int idx = Index(x, z, _cols);
                        if (!_walkable[idx] || _component[idx] != comp)
                            continue;
                        CellCenter(idx, out float gx, out float gz);
                        if (!IsDiagonalNear(from.x, from.y, gx, gz, minSqr, maxSqr, level))
                            continue;
                        cand.Add(idx);
                    }
                }
            }

            if (cand.Count == 0)
                return 0;

            var keys = new float[cand.Count];
            for (int i = 0; i < cand.Count; i++)
            {
                float h = _heat[HeatIndexOfCell(cand[i])];
                float w = 1f / ((1f + h) * (1f + h));
                // Efraimidis-Spirakis 加权无放回抽样：key 越小越优先
                keys[i] = -Mathf.Log(Mathf.Max(1e-6f, Random.value)) / w;
            }

            int take = Mathf.Min(count, cand.Count);
            for (int n = 0; n < take; n++)
            {
                int best = n;
                for (int i = n + 1; i < cand.Count; i++)
                {
                    if (keys[i] < keys[best])
                        best = i;
                }

                int tmpC = cand[n]; cand[n] = cand[best]; cand[best] = tmpC;
                float tmpK = keys[n]; keys[n] = keys[best]; keys[best] = tmpK;

                CellCenter(cand[n], out float px, out float pz);
                outGoals.Add(new Vector2(px, pz));
            }

            return outGoals.Count;
        }

        static bool IsDiagonalNear(
            float fx, float fz, float gx, float gz, float minSqr, float maxSqr, int level)
        {
            float dx = gx - fx;
            float dz = gz - fz;
            float dSqr = dx * dx + dz * dz;
            if (dSqr < minSqr || dSqr > maxSqr)
                return false;
            if (level >= 2)
                return true;

            float ax = Mathf.Abs(dx);
            float az = Mathf.Abs(dz);
            float minAxis = level == 0 ? 1f : 0.5f;
            if (ax < minAxis || az < minAxis)
                return false;
            float ratio = ax / az;
            return level == 0
                ? ratio >= 0.45f && ratio <= 2.2f
                : ratio >= 0.2f && ratio <= 5f;
        }

        List<int> PickPool(int comp, List<int> all, float preferNearWall, float preferOpen)
        {
            float roll = Random.value;
            if (preferOpen > 0f && roll < preferOpen &&
                _openByComponent.TryGetValue(comp, out List<int> openList) && openList.Count > 0)
                return openList;
            if (preferNearWall > 0f && roll < preferOpen + preferNearWall &&
                _nearWallByComponent.TryGetValue(comp, out List<int> nearList) && nearList.Count > 0)
                return nearList;
            return all;
        }

        public bool TryFindPath(Vector2 fromVenueXz, Vector2 toVenueXz, List<Vector2> pathOut)
        {
            return TryFindPath(fromVenueXz, toVenueXz, pathOut, agentSalt: 0);
        }

        /// <summary>
        /// agentSalt≠0 时给格点加确定性代价噪声，使相近起终点走出不同走廊，减轻扎堆。
        /// </summary>
        public bool TryFindPath(Vector2 fromVenueXz, Vector2 toVenueXz, List<Vector2> pathOut, int agentSalt)
        {
            pathOut.Clear();
            if (!Ready)
                return false;

            if (!TryResolveCell(fromVenueXz.x, fromVenueXz.y, out int start))
                return false;
            if (!TryResolveCell(toVenueXz.x, toVenueXz.y, out int goal))
                return false;

            if (_component[start] != _component[goal] || _component[start] < 0)
                return false;

            if (start == goal)
            {
                CellCenter(goal, out float px, out float pz);
                pathOut.Add(new Vector2(px, pz));
                return true;
            }

            int total = _cols * _rows;
            var cameFrom = new int[total];
            var gScore = new float[total];
            var closed = new bool[total];
            for (int i = 0; i < total; i++)
            {
                cameFrom[i] = -1;
                gScore[i] = float.PositiveInfinity;
            }

            var open = new List<int>(64);
            gScore[start] = 0f;
            open.Add(start);

            bool found = false;
            while (open.Count > 0)
            {
                int bestI = 0;
                float bestF = float.PositiveInfinity;
                for (int i = 0; i < open.Count; i++)
                {
                    int n = open[i];
                    float f = gScore[n] + Heuristic(n, goal);
                    if (f < bestF)
                    {
                        bestF = f;
                        bestI = i;
                    }
                }

                int current = open[bestI];
                open.RemoveAt(bestI);
                if (current == goal)
                {
                    found = true;
                    break;
                }

                if (closed[current])
                    continue;
                closed[current] = true;

                int cx = current % _cols;
                int cz = current / _cols;
                for (int k = 0; k < 8; k++)
                {
                    int nx = cx + NeighborDx[k];
                    int nz = cz + NeighborDz[k];
                    if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows)
                        continue;
                    int ni = Index(nx, nz, _cols);
                    if (!_walkable[ni] || closed[ni])
                        continue;
                    if (NeighborDx[k] != 0 && NeighborDz[k] != 0)
                    {
                        if (!_walkable[Index(cx + NeighborDx[k], cz, _cols)] ||
                            !_walkable[Index(cx, cz + NeighborDz[k], _cols)])
                            continue;
                    }

                    float mul = _nearWall[ni] ? 1f : OpenSpaceCostMul;
                    if (agentSalt != 0)
                        mul *= 1f + 0.35f * CellNoise01(ni, agentSalt); // 0~+35% 分化走廊
                    float tentative = gScore[current] + NeighborCost[k] * _cell * mul;
                    if (tentative >= gScore[ni])
                        continue;
                    cameFrom[ni] = current;
                    gScore[ni] = tentative;
                    if (!open.Contains(ni))
                        open.Add(ni);
                }
            }

            if (!found)
                return false;

            var stack = new List<int>(32);
            int cursor = goal;
            while (cursor >= 0)
            {
                stack.Add(cursor);
                if (cursor == start)
                    break;
                cursor = cameFrom[cursor];
            }

            for (int i = stack.Count - 1; i >= 0; i--)
            {
                CellCenter(stack[i], out float px, out float pz);
                pathOut.Add(new Vector2(px, pz));
            }

            return pathOut.Count > 0;
        }

        void LabelComponents()
        {
            int next = 0;
            _largestComponent = -1;
            int largestSize = 0;
            var queue = new Queue<int>();
            for (int i = 0; i < _walkable.Length; i++)
            {
                if (!_walkable[i] || _component[i] >= 0)
                    continue;

                int id = next++;
                var list = new List<int>(32);
                _component[i] = id;
                queue.Enqueue(i);
                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    list.Add(cur);
                    int cx = cur % _cols;
                    int cz = cur / _cols;
                    for (int k = 0; k < 4; k++)
                    {
                        int nx = cx + OrthDx[k];
                        int nz = cz + OrthDz[k];
                        if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows)
                            continue;
                        int ni = Index(nx, nz, _cols);
                        if (!_walkable[ni] || _component[ni] >= 0)
                            continue;
                        _component[ni] = id;
                        queue.Enqueue(ni);
                    }
                }

                _cellsByComponent[id] = list;
                if (list.Count > largestSize)
                {
                    largestSize = list.Count;
                    _largestComponent = id;
                }
            }
        }

        void BuildNearWallLists()
        {
            _nearWallCells.Clear();
            _openCells.Clear();
            _nearWallByComponent.Clear();
            _openByComponent.Clear();
            for (int i = 0; i < _walkable.Length; i++)
            {
                _nearWall[i] = false;
                if (!_walkable[i])
                    continue;

                int comp = _component[i];
                if (CellTouchesBlocked(i))
                {
                    _nearWall[i] = true;
                    _nearWallCells.Add(i);
                    if (comp >= 0)
                    {
                        if (!_nearWallByComponent.TryGetValue(comp, out List<int> nearList))
                        {
                            nearList = new List<int>(16);
                            _nearWallByComponent[comp] = nearList;
                        }

                        nearList.Add(i);
                    }
                }
                else
                {
                    _openCells.Add(i);
                    if (comp >= 0)
                    {
                        if (!_openByComponent.TryGetValue(comp, out List<int> openList))
                        {
                            openList = new List<int>(16);
                            _openByComponent[comp] = openList;
                        }

                        openList.Add(i);
                    }
                }
            }
        }

        bool CellTouchesBlocked(int index)
        {
            int cx = index % _cols;
            int cz = index / _cols;
            // 正交邻域碰到墙/障碍/边界，视为贴墙或门框旁
            for (int k = 0; k < 4; k++)
            {
                int nx = cx + OrthDx[k];
                int nz = cz + OrthDz[k];
                if (nx < 0 || nz < 0 || nx >= _cols || nz >= _rows)
                    return true;
                if (!_walkable[Index(nx, nz, _cols)])
                    return true;
            }

            return false;
        }

        bool TryResolveCell(float venueX, float venueZ, out int cell)
        {
            cell = -1;
            if (TryVenueToCell(venueX, venueZ, out int cx, out int cz))
            {
                int idx = Index(cx, cz, _cols);
                if (_walkable[idx])
                {
                    cell = idx;
                    return true;
                }
            }

            return TryNearestWalkable(venueX, venueZ, out cell);
        }

        bool TryNearestWalkable(float venueX, float venueZ, out int index)
        {
            index = -1;
            float best = float.PositiveInfinity;
            for (int i = 0; i < _walkableCells.Count; i++)
            {
                int id = _walkableCells[i];
                CellCenter(id, out float px, out float pz);
                float dx = px - venueX;
                float dz = pz - venueZ;
                float d = dx * dx + dz * dz;
                if (d >= best)
                    continue;
                best = d;
                index = id;
            }

            return index >= 0;
        }

        float Heuristic(int a, int b)
        {
            int ax = a % _cols;
            int az = a / _cols;
            int bx = b % _cols;
            int bz = b / _cols;
            float dx = ax - bx;
            float dz = az - bz;
            return Mathf.Sqrt(dx * dx + dz * dz) * _cell;
        }

        bool TryVenueToCell(float venueX, float venueZ, out int cx, out int cz)
        {
            cx = Mathf.FloorToInt((venueX - _originX) / _cell);
            cz = Mathf.FloorToInt((venueZ - _originZ) / _cell);
            return cx >= 0 && cz >= 0 && cx < _cols && cz < _rows;
        }

        void CellCenter(int index, out float venueX, out float venueZ)
        {
            int cx = index % _cols;
            int cz = index / _cols;
            venueX = _originX + (cx + 0.5f) * _cell;
            venueZ = _originZ + (cz + 0.5f) * _cell;
        }

        static int Index(int x, int z, int cols)
        {
            return z * cols + x;
        }

        static bool IsBlocked(Vector3 world, float radiusWorld, int layerMask)
        {
            Collider[] hits = Physics.OverlapSphere(world, radiusWorld, layerMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider c = hits[i];
                if (c == null)
                    continue;
                if (IsIgnoredCollider(c))
                    continue;
                return true;
            }

            return false;
        }

        static bool IsIgnoredCollider(Collider c)
        {
            string n = c.name;
            if (n == "Ground")
                return true;
            if (n.StartsWith("Pillar"))
                return true;
            return false;
        }
    }
}
