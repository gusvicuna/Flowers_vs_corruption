using System;
using System.Collections.Generic;
using FlowersVsCorruption.World;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    public class WorldGridTests
    {
        private const int Count = 24;

        private static WorldGrid MakeGrid(int count = Count)
        {
            var types = new TileType[count];
            for (int i = 0; i < count; i++)
                types[i] = TileType.Soil;
            types[0] = TileType.House;
            types[count / 2] = TileType.CorruptionBase;
            return new WorldGrid(types);
        }

        [Test]
        public void Constructor_MapsTypesToTiles()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.TileCount, Is.EqualTo(Count));
            Assert.That(grid.GetTile(0).Type, Is.EqualTo(TileType.House));
            Assert.That(grid.GetTile(Count / 2).Type, Is.EqualTo(TileType.CorruptionBase));
            Assert.That(grid.GetTile(1).Type, Is.EqualTo(TileType.Soil));
        }

        [Test]
        public void Constructor_NullOrEmpty_Throws()
        {
            Assert.Throws<ArgumentException>(() => new WorldGrid(null));
            Assert.Throws<ArgumentException>(() => new WorldGrid(new List<TileType>()));
        }

        [Test]
        public void Constructor_NoHouseAtIndexZero_Throws()
        {
            var types = new TileType[Count];
            for (int i = 0; i < Count; i++)
                types[i] = TileType.Soil;

            Assert.Throws<ArgumentException>(() => new WorldGrid(types));
        }

        [Test]
        public void WrapIndex_HandlesNegativesAndOverflow()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.WrapIndex(0), Is.EqualTo(0));
            Assert.That(grid.WrapIndex(-1), Is.EqualTo(Count - 1));
            Assert.That(grid.WrapIndex(Count), Is.EqualTo(0));
            Assert.That(grid.WrapIndex(-Count - 2), Is.EqualTo(Count - 2));
            Assert.That(grid.WrapIndex(Count * 3 + 5), Is.EqualTo(5));
        }

        [Test]
        public void GetTile_WrapsAroundTheRing()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.GetTile(-1), Is.SameAs(grid.GetTile(Count - 1)));
            Assert.That(grid.GetTile(Count), Is.SameAs(grid.GetTile(0)));
        }

        [Test]
        public void NextAndPrevious_WrapAtSeams()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.NextIndex(Count - 1), Is.EqualTo(0));
            Assert.That(grid.NextIndex(3), Is.EqualTo(4));
            Assert.That(grid.PreviousIndex(0), Is.EqualTo(Count - 1));
            Assert.That(grid.PreviousIndex(4), Is.EqualTo(3));
        }

        [Test]
        public void AngleOf_Index0IsTop_Clockwise()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.AngleOf(0), Is.EqualTo(90f));
            Assert.That(grid.AngleOf(1), Is.EqualTo(90f - grid.ArcPerTileDegrees));
        }

        [Test]
        public void IndexAtAngle_IsInverseOfAngleOf()
        {
            WorldGrid grid = MakeGrid();

            for (int i = 0; i < Count; i++)
                Assert.That(grid.IndexAtAngle(grid.AngleOf(i)), Is.EqualTo(i), $"index {i}");
        }

        [Test]
        public void IndexAtAngle_WrapsOutOfRangeAngles()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.IndexAtAngle(450f), Is.EqualTo(0));
            Assert.That(grid.IndexAtAngle(-270f), Is.EqualTo(0));
            Assert.That(grid.IndexAtAngle(90f + grid.ArcPerTileDegrees), Is.EqualTo(Count - 1));
        }

        [Test]
        public void SetCorrupted_FlipsStateAndRaisesEvent()
        {
            WorldGrid grid = MakeGrid();
            var received = new List<int>();
            grid.TileChanged += received.Add;

            grid.SetCorrupted(5, true);

            Assert.That(grid.IsCorrupted(5), Is.True);
            Assert.That(received, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void SetCorrupted_SameValue_DoesNotRaiseEvent()
        {
            WorldGrid grid = MakeGrid();
            int events = 0;
            grid.TileChanged += _ => events++;

            grid.SetCorrupted(5, false);
            grid.SetCorrupted(5, true);
            grid.SetCorrupted(5, true);

            Assert.That(events, Is.EqualTo(1));
        }

        [Test]
        public void SetCorrupted_WrapsIndexInEventPayload()
        {
            WorldGrid grid = MakeGrid();
            int? received = null;
            grid.TileChanged += i => received = i;

            grid.SetCorrupted(-1, true);

            Assert.That(received, Is.EqualTo(Count - 1));
            Assert.That(grid.IsCorrupted(Count - 1), Is.True);
        }

        [Test]
        public void FindFirstIndex_FindsPolesAndReportsMissing()
        {
            WorldGrid grid = MakeGrid();

            Assert.That(grid.FindFirstIndex(TileType.House), Is.EqualTo(0));
            Assert.That(grid.FindFirstIndex(TileType.CorruptionBase), Is.EqualTo(Count / 2));
            Assert.That(grid.FindFirstIndex(TileType.Rock), Is.EqualTo(-1));
        }
    }
}
