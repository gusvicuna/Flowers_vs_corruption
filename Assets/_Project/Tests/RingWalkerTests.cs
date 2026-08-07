using System.Collections.Generic;
using FlowersVsCorruption.Player;
using FlowersVsCorruption.World;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    public class RingWalkerTests
    {
        private const int Count = 24;

        private static WorldGrid MakeGrid()
        {
            var types = new TileType[Count];
            for (int i = 0; i < Count; i++)
                types[i] = TileType.Soil;
            types[0] = TileType.House;
            types[Count / 2] = TileType.CorruptionBase;
            return new WorldGrid(types);
        }

        private static RingWalker MakeWalkerAtTile0(WorldGrid grid, out List<int> events)
        {
            var walker = new RingWalker(grid, grid.AngleOf(0));
            var received = new List<int>();
            walker.CurrentTileChanged += received.Add;
            events = received;
            return walker;
        }

        [Test]
        public void Constructor_StartAngle_SetsCurrentTileIndex()
        {
            WorldGrid grid = MakeGrid();

            var walker = new RingWalker(grid, grid.AngleOf(5));

            Assert.That(walker.CurrentTileIndex, Is.EqualTo(5));
            Assert.That(walker.AngleDegrees, Is.EqualTo(grid.AngleOf(5)).Within(0.001f));
        }

        [Test]
        public void Move_PositiveInput_DecreasesAngle()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out _);
            float startAngle = walker.AngleDegrees;

            walker.Move(1f, 1f, 0.1f);

            Assert.That(walker.AngleDegrees, Is.LessThan(startAngle));
        }

        [Test]
        public void Move_NegativeInput_IncreasesAngle()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out _);
            float startAngle = walker.AngleDegrees;

            walker.Move(-1f, 1f, 0.1f);

            Assert.That(walker.AngleDegrees, Is.GreaterThan(startAngle));
        }

        [Test]
        public void Move_SpeedConversion_TilesPerSecondToDegrees()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out _);
            float startAngle = walker.AngleDegrees;

            walker.Move(1f, 1f, 1f);

            float displacement = startAngle - walker.AngleDegrees;
            Assert.That(displacement, Is.EqualTo(grid.ArcPerTileDegrees).Within(0.001f));
        }

        [Test]
        public void Move_ZeroInput_NoMovementNoEvent()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out List<int> events);
            float startAngle = walker.AngleDegrees;

            walker.Move(0f, 5f, 1f);

            Assert.That(walker.AngleDegrees, Is.EqualTo(startAngle));
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void Move_PartialAxis_MovesAtFullSpeed()
        {
            WorldGrid grid = MakeGrid();
            RingWalker partial = MakeWalkerAtTile0(grid, out _);
            RingWalker full = MakeWalkerAtTile0(grid, out _);

            partial.Move(0.4f, 2f, 0.5f);
            full.Move(1f, 2f, 0.5f);

            Assert.That(partial.AngleDegrees, Is.EqualTo(full.AngleDegrees).Within(0.001f));
        }

        [Test]
        public void Move_WrapsAngleAcrossZero()
        {
            WorldGrid grid = MakeGrid();
            var walker = new RingWalker(grid, 5f);

            walker.Move(1f, 1f, 1f);   // 15° clockwise from 5° crosses 0°

            Assert.That(walker.AngleDegrees, Is.InRange(0f, 360f));
            Assert.That(walker.AngleDegrees, Is.EqualTo(350f).Within(0.001f));
        }

        [Test]
        public void Move_ManySmallSteps_EventFiresOncePerBoundary()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out List<int> events);

            // One tile of travel split into 60 tiny steps: exactly one boundary.
            for (int i = 0; i < 60; i++)
                walker.Move(1f, 1f, 1f / 60f);

            Assert.That(events, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Move_FullLap_ReturnsToStartTile()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out List<int> events);
            float startAngle = walker.AngleDegrees;

            for (int i = 0; i < 240; i++)
                walker.Move(1f, 1f, 0.1f);   // 24 tiles of travel total

            Assert.That(walker.CurrentTileIndex, Is.EqualTo(0));
            Assert.That(walker.AngleDegrees, Is.EqualTo(startAngle).Within(0.01f));
            Assert.That(events.Count, Is.EqualTo(Count));
            Assert.That(events[Count - 1], Is.EqualTo(0));
        }

        [Test]
        public void Move_RightFromHouse_EntersIndex1()
        {
            WorldGrid grid = MakeGrid();
            RingWalker walker = MakeWalkerAtTile0(grid, out List<int> events);

            walker.Move(1f, 1f, 1f);   // exactly one tile clockwise

            Assert.That(walker.CurrentTileIndex, Is.EqualTo(1));
            Assert.That(events, Is.EqualTo(new[] { 1 }));
        }
    }
}
