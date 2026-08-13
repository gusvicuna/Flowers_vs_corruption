using System.Linq;
using FlowersVsCorruption.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FlowersVsCorruption.Tests
{
    public class WorldLayoutTests
    {
        private const int Count = 24;
        private const int Rocks = 5;
        private const int Seed = 12345;
        private const int SoilPerSide = 1;

        [Test]
        public void GenerateDefault_HasExpectedComposition()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);

            Assert.That(tiles.Length, Is.EqualTo(Count));
            Assert.That(tiles[0], Is.EqualTo(TileType.House));
            Assert.That(tiles[Count / 2], Is.EqualTo(TileType.CorruptionBase));
            Assert.That(tiles.Count(t => t == TileType.House), Is.EqualTo(1));
            Assert.That(tiles.Count(t => t == TileType.CorruptionBase), Is.EqualTo(1));
            Assert.That(tiles.Count(t => t == TileType.Rock), Is.EqualTo(Rocks));
            Assert.That(tiles.Count(t => t == TileType.Soil), Is.EqualTo(Count - 2 - Rocks));
        }

        [Test]
        public void GenerateDefault_NoRocksAdjacentToPoles()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);

            int baseIndex = Count / 2;
            int[] guarded = { 1, Count - 1, baseIndex - 1, baseIndex + 1 };
            foreach (int index in guarded)
                Assert.That(tiles[index], Is.EqualTo(TileType.Soil), $"index {index}");
        }

        [Test]
        public void GenerateDefault_SameSeed_IsDeterministic()
        {
            TileType[] first = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);
            TileType[] second = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void GenerateDefault_ExcessRocks_AreClamped()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(8, 100, Seed, SoilPerSide);

            // 8 tiles minus 2 poles minus 4 pole-adjacent = 2 rock candidates.
            Assert.That(tiles.Count(t => t == TileType.Rock), Is.EqualTo(2));
        }

        [Test]
        public void GenerateDefault_TooFewTiles_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => WorldLayout.GenerateDefault(3, 0, Seed, SoilPerSide));
        }

        [Test]
        public void GenerateDefault_GuaranteesSoilAroundHouse()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, 3);

            int[] guaranteed = { 1, 2, 3, Count - 3, Count - 2, Count - 1 };
            foreach (int index in guaranteed)
                Assert.That(tiles[index], Is.EqualTo(TileType.Soil), $"index {index}");
        }

        [Test]
        public void GenerateDefault_SoilZero_AllowsRocksNextToHouse()
        {
            // 8 tiles, soil span 0: only house, base, and base-adjacent are reserved,
            // so maxing out rocks forces them onto the house-adjacent tiles.
            TileType[] tiles = WorldLayout.GenerateDefault(8, 100, Seed, 0);

            Assert.That(tiles[1], Is.EqualTo(TileType.Rock));
            Assert.That(tiles[7], Is.EqualTo(TileType.Rock));
        }

        [Test]
        public void CreateGrid_RoundTripsThroughScriptableObject()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);
            WithLayout(tiles, 1, layout =>
            {
                WorldGrid grid = layout.CreateGrid();

                Assert.That(grid.TileCount, Is.EqualTo(Count));
                for (int i = 0; i < Count; i++)
                    Assert.That(grid.GetTile(i).Type, Is.EqualTo(tiles[i]), $"index {i}");
            });
        }

        [Test]
        public void CreateGrid_CorruptsBaseAndAdjacentTiles()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);
            WithLayout(tiles, 1, layout =>
            {
                WorldGrid grid = layout.CreateGrid();

                int baseIndex = Count / 2;
                for (int i = 0; i < Count; i++)
                {
                    bool expected = i >= baseIndex - 1 && i <= baseIndex + 1;
                    Assert.That(grid.IsCorrupted(i), Is.EqualTo(expected), $"index {i}");
                }
            });
        }

        [Test]
        public void CreateGrid_SpreadZero_OnlyBaseIsCorrupted()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);
            WithLayout(tiles, 0, layout =>
            {
                WorldGrid grid = layout.CreateGrid();

                for (int i = 0; i < Count; i++)
                    Assert.That(grid.IsCorrupted(i), Is.EqualTo(i == Count / 2), $"index {i}");
            });
        }

        [Test]
        public void CreateGrid_OversizedSpread_CorruptsWholeRingSafely()
        {
            TileType[] tiles = WorldLayout.GenerateDefault(Count, Rocks, Seed, SoilPerSide);
            WithLayout(tiles, Count * 2, layout =>
            {
                WorldGrid grid = layout.CreateGrid();

                for (int i = 0; i < Count; i++)
                    Assert.That(grid.IsCorrupted(i), Is.True, $"index {i}");
            });
        }

        [Test]
        public void CreateGrid_NoCorruptionBase_StartsClean()
        {
            var tiles = new TileType[Count];
            for (int i = 0; i < Count; i++)
                tiles[i] = TileType.Soil;
            tiles[0] = TileType.House;

            WithLayout(tiles, 1, layout =>
            {
                WorldGrid grid = layout.CreateGrid();

                for (int i = 0; i < Count; i++)
                    Assert.That(grid.IsCorrupted(i), Is.False, $"index {i}");
            });
        }

        private static void WithLayout(TileType[] tiles, int initialCorruptionPerSide, System.Action<WorldLayout> assert)
        {
            var layout = ScriptableObject.CreateInstance<WorldLayout>();
            try
            {
                var so = new SerializedObject(layout);
                SerializedProperty tilesProperty = so.FindProperty("_tiles");
                tilesProperty.arraySize = tiles.Length;
                for (int i = 0; i < tiles.Length; i++)
                    tilesProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)tiles[i];
                so.FindProperty("_initialCorruptionPerSide").intValue = initialCorruptionPerSide;
                so.ApplyModifiedPropertiesWithoutUndo();

                assert(layout);
            }
            finally
            {
                Object.DestroyImmediate(layout);
            }
        }
    }
}
