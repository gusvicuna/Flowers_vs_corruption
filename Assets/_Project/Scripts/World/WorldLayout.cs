using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// Authored ring composition. Use the context menu (gear icon) action
    /// "Generate Default Layout" instead of hand-editing 24 entries.
    /// </summary>
    [CreateAssetMenu(menuName = "FvC/World Layout", fileName = "WorldLayout")]
    public class WorldLayout : ScriptableObject
    {
        [SerializeField] private TileType[] _tiles = Array.Empty<TileType>();
        [Tooltip("How many tiles on EACH side of the corruption base start corrupted. The base itself is always corrupted.")]
        [SerializeField, Min(0)] private int _initialCorruptionPerSide = 5;
        [Header("Generator")]
        [SerializeField, Min(4)] private int _generateTileCount = 24;
        [SerializeField, Min(0)] private int _rockCount = 5;
        [Tooltip("How many tiles on EACH side of the house are guaranteed Soil (no rocks) — the player's starting farmland.")]
        [SerializeField, Min(0)] private int _soilNextToHousePerSide = 2;
        [SerializeField] private int _seed = 12345;

        public IReadOnlyList<TileType> Tiles => _tiles;

        public WorldGrid CreateGrid()
        {
            var grid = new WorldGrid(_tiles);

            int baseIndex = grid.FindFirstIndex(TileType.CorruptionBase);
            if (baseIndex >= 0)
            {
                grid.SetCorrupted(baseIndex, true);
                for (int offset = 1; offset <= _initialCorruptionPerSide; offset++)
                {
                    grid.SetCorrupted(baseIndex + offset, true);
                    grid.SetCorrupted(baseIndex - offset, true);
                }
            }

            return grid;
        }

        /// <summary>
        /// All soil, House at 0, CorruptionBase at the opposite pole, rocks scattered
        /// on random tiles. Guarantees soilNextToHousePerSide Soil tiles on each side
        /// of the house (starting farmland) and keeps the tiles flanking the
        /// corruption base rock-free (unblocked corruption exit).
        /// </summary>
        public static TileType[] GenerateDefault(int tileCount, int rockCount, int seed, int soilNextToHousePerSide)
        {
            if (tileCount < 4)
                throw new ArgumentOutOfRangeException(nameof(tileCount), "Need at least 4 tiles.");

            var tiles = new TileType[tileCount];
            for (int i = 0; i < tileCount; i++)
                tiles[i] = TileType.Soil;

            const int houseIndex = 0;
            int baseIndex = tileCount / 2;
            tiles[houseIndex] = TileType.House;
            tiles[baseIndex] = TileType.CorruptionBase;

            var reserved = new HashSet<int>
            {
                houseIndex,
                baseIndex,
                (baseIndex + 1) % tileCount,
                (baseIndex - 1 + tileCount) % tileCount
            };

            int soilSpan = Math.Min(Math.Max(0, soilNextToHousePerSide), tileCount / 2);
            for (int offset = 1; offset <= soilSpan; offset++)
            {
                reserved.Add((houseIndex + offset) % tileCount);
                reserved.Add((houseIndex - offset + tileCount) % tileCount);
            }

            var candidates = new List<int>();
            for (int i = 0; i < tileCount; i++)
            {
                if (!reserved.Contains(i))
                    candidates.Add(i);
            }

            var random = new System.Random(seed);
            int rocksToPlace = Math.Min(rockCount, candidates.Count);
            for (int r = 0; r < rocksToPlace; r++)
            {
                int pick = random.Next(candidates.Count);
                tiles[candidates[pick]] = TileType.Rock;
                candidates.RemoveAt(pick);
            }

            return tiles;
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Default Layout")]
        private void GenerateDefaultLayout()
        {
            _tiles = GenerateDefault(_generateTileCount, _rockCount, _seed, _soilNextToHousePerSide);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
