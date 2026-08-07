using System;
using FlowersVsCorruption.World;
using UnityEngine;

namespace FlowersVsCorruption.Player
{
    /// <summary>
    /// Plain-C# movement model: an angular position walking the tile ring at
    /// constant speed. Positive input walks clockwise (decreasing angle,
    /// increasing tile index) which reads as screen-right whenever the camera's
    /// up is radial. No scene dependencies.
    /// </summary>
    public class RingWalker
    {
        private readonly WorldGrid _grid;

        /// <summary>Degrees, always normalized to [0, 360).</summary>
        public float AngleDegrees { get; private set; }

        public int CurrentTileIndex { get; private set; }

        /// <summary>Raised only when the walker crosses a tile boundary.</summary>
        public event Action<int> CurrentTileChanged;

        public RingWalker(WorldGrid grid, float startAngleDegrees)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            AngleDegrees = Mathf.Repeat(startAngleDegrees, 360f);
            CurrentTileIndex = _grid.IndexAtAngle(AngleDegrees);
        }

        public void Move(float inputAxis, float speedTilesPerSecond, float deltaTime)
        {
            int direction = Math.Sign(inputAxis);
            if (direction == 0)
                return;

            float degreesPerSecond = speedTilesPerSecond * _grid.ArcPerTileDegrees;
            AngleDegrees = Mathf.Repeat(AngleDegrees - direction * degreesPerSecond * deltaTime, 360f);

            int index = _grid.IndexAtAngle(AngleDegrees);
            if (index == CurrentTileIndex)
                return;

            CurrentTileIndex = index;
            CurrentTileChanged?.Invoke(index);
        }
    }
}
