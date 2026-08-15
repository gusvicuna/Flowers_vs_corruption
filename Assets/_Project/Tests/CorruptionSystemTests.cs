using System;
using System.Collections.Generic;
using System.Linq;
using FlowersVsCorruption.World;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    /// <summary>
    /// Spec for CorruptionSystem (Feature 4). 24-tile ring, House at 0,
    /// CorruptionBase at 12. Each Spread wave corrupts every clean, unguarded
    /// tile adjacent to corruption — computed from a snapshot, so a wave never
    /// chains onto tiles it corrupted itself.
    /// </summary>
    public class CorruptionSystemTests
    {
        private const int Count = 24;
        private const int HouseIndex = 0;
        private const int BaseIndex = 12;

        private static WorldGrid MakeGrid()
        {
            var types = new TileType[Count];
            for (int i = 0; i < Count; i++)
                types[i] = TileType.Soil;
            types[HouseIndex] = TileType.House;
            types[BaseIndex] = TileType.CorruptionBase;
            return new WorldGrid(types);
        }

        private static List<int> CorruptedIndices(WorldGrid grid)
        {
            return Enumerable.Range(0, grid.TileCount).Where(grid.IsCorrupted).ToList();
        }

        [Test]
        public void Constructor_NullGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CorruptionSystem(null));
        }

        [Test]
        public void Spread_SingleCorruptedTile_CorruptsBothNeighbors()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);

            system.Spread(1);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 11, 12, 13 }));
        }

        [Test]
        public void Spread_ContiguousArc_OnlyEndsAdvance()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(11, true);
            grid.SetCorrupted(12, true);
            grid.SetCorrupted(13, true);

            system.Spread(1);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 10, 11, 12, 13, 14 }));
        }

        [Test]
        public void Spread_TwoWaves_EachFrontAdvancesTwo()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);

            system.Spread(2);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 10, 11, 12, 13, 14 }));
        }

        [Test]
        public void Spread_MultipleRegions_AllGrow()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(12, true);
            grid.SetCorrupted(20, true);

            system.Spread(1);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 11, 12, 13, 19, 20, 21 }));
        }

        [Test]
        public void Spread_WrapsAroundRingSeam()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(23, true);

            system.Spread(1);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 0, 22, 23 }));
        }

        [Test]
        public void Spread_GuardedTile_BlocksThatFront_OtherFrontAdvances()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);
            system.IsTileGuarded = index => index == 13;

            system.Spread(1);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 11, 12 }));
        }

        [Test]
        public void Spread_TwoWaves_GuardStopsAdvance_NoSkipping()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);
            system.IsTileGuarded = index => index == 13;

            system.Spread(2);

            // Left front advances 2 (11, 10); right front is stopped at the
            // guard and must NOT jump over it to 14.
            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 10, 11, 12 }));
        }

        [Test]
        public void Spread_FullyCorruptedRing_NoOp()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            for (int i = 0; i < Count; i++)
                grid.SetCorrupted(i, true);

            Assert.DoesNotThrow(() => system.Spread(1));
            Assert.That(CorruptedIndices(grid).Count, Is.EqualTo(Count));
        }

        [Test]
        public void Spread_NoCorruption_NoOp()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);

            system.Spread(3);

            Assert.That(CorruptedIndices(grid), Is.Empty);
        }

        [Test]
        public void Spread_ZeroOrNegativeWaves_NoOp()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);

            system.Spread(0);
            system.Spread(-2);

            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { BaseIndex }));
        }

        [Test]
        public void SpreadBlocked_FiredForBlockedFrontierTile()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);
            system.IsTileGuarded = index => index == 13;
            var blocked = new List<int>();
            system.SpreadBlocked += blocked.Add;

            system.Spread(1);

            Assert.That(blocked, Is.EqualTo(new[] { 13 }));
            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 11, 12 }));
        }

        [Test]
        public void SpreadBlocked_FiredOncePerWave()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);
            system.IsTileGuarded = index => index == 13;
            var blocked = new List<int>();
            system.SpreadBlocked += blocked.Add;

            system.Spread(2);

            Assert.That(blocked, Is.EqualTo(new[] { 13, 13 }));
        }

        [Test]
        public void SpreadBlocked_NotFiredForGuardedTilesAwayFromTheFront()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            grid.SetCorrupted(BaseIndex, true);
            // Guard a tile nowhere near the corruption: it is not being
            // attacked, so it must NOT report a blocked attempt.
            system.IsTileGuarded = index => index == 5;
            var blocked = new List<int>();
            system.SpreadBlocked += blocked.Add;

            system.Spread(1);

            Assert.That(blocked, Is.Empty);
            Assert.That(CorruptedIndices(grid), Is.EquivalentTo(new[] { 11, 12, 13 }));
        }

        [Test]
        public void HouseCorrupted_FiresOnceOnDirectSet()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            var received = new List<int>();
            system.HouseCorrupted += received.Add;

            // Any source counts: the system watches the grid, not just Spread.
            grid.SetCorrupted(HouseIndex, true);
            grid.SetCorrupted(HouseIndex, true);   // no change -> no second event

            Assert.That(received, Is.EqualTo(new[] { HouseIndex }));
        }

        [Test]
        public void HouseCorrupted_FiresWhenSpreadReachesHouse()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            var received = new List<int>();
            system.HouseCorrupted += received.Add;
            grid.SetCorrupted(1, true);   // adjacent to the house

            system.Spread(1);

            Assert.That(received, Is.EqualTo(new[] { HouseIndex }));
        }

        [Test]
        public void HouseCorrupted_CleansingHouse_DoesNotFire()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            var received = new List<int>();
            system.HouseCorrupted += received.Add;

            grid.SetCorrupted(HouseIndex, true);    // fires (corruption)
            grid.SetCorrupted(HouseIndex, false);   // cleanse must NOT fire
            grid.SetCorrupted(HouseIndex, true);    // fires again (new corruption)

            Assert.That(received, Is.EqualTo(new[] { HouseIndex, HouseIndex }));
        }

        [Test]
        public void HouseCorrupted_CorruptingOtherTiles_DoesNotFire()
        {
            WorldGrid grid = MakeGrid();
            var system = new CorruptionSystem(grid);
            int events = 0;
            system.HouseCorrupted += _ => events++;
            grid.SetCorrupted(BaseIndex, true);

            system.Spread(1);

            Assert.That(events, Is.EqualTo(0));
        }
    }
}
