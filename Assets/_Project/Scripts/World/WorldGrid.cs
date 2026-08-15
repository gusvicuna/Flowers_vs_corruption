using System;
using System.Collections.Generic;
using FlowersVsCorruption.Farming;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// The central world model: an ordered ring of tiles. Plain C#, no scene
    /// dependencies. Index 0 is the top of the planet, indices increase clockwise.
    /// </summary>
    public class WorldGrid
    {
        private readonly Tile[] _tiles;

        public int TileCount => _tiles.Length;
        public float ArcPerTileDegrees => 360f / _tiles.Length;

        public event Action<int> TileChanged;
        public event Action<int> TileCorrupted;

        public WorldGrid(IReadOnlyList<TileType> tileTypes)
        {
            if (tileTypes == null || tileTypes.Count == 0)
                throw new ArgumentException("WorldGrid needs at least one tile.", nameof(tileTypes));
            if (tileTypes[0] != TileType.House)
                throw new ArgumentException("Index 0 must be the House tile.", nameof(tileTypes));

            _tiles = new Tile[tileTypes.Count];
            for (int i = 0; i < _tiles.Length; i++)
                _tiles[i] = new Tile(tileTypes[i]);
        }

        public int WrapIndex(int index)
        {
            int wrapped = index % _tiles.Length;
            return wrapped < 0 ? wrapped + _tiles.Length : wrapped;
        }

        public Tile GetTile(int index) => _tiles[WrapIndex(index)];

        public int NextIndex(int index) => WrapIndex(index + 1);

        public int PreviousIndex(int index) => WrapIndex(index - 1);

        /// <summary>Angle of the tile's center in degrees (index 0 = 90°, clockwise).</summary>
        public float AngleOf(int index) => 90f - WrapIndex(index) * ArcPerTileDegrees;

        /// <summary>Inverse of <see cref="AngleOf"/>: which tile sits at this angle.</summary>
        public int IndexAtAngle(float degrees)
        {
            float raw = (90f - degrees) / ArcPerTileDegrees;
            return WrapIndex((int)Math.Round(raw, MidpointRounding.AwayFromZero));
        }

        public bool IsCorrupted(int index) => GetTile(index).IsCorrupted;

        public void SetCorrupted(int index, bool corrupted)
        {
            int wrapped = WrapIndex(index);
            Tile tile = _tiles[wrapped];
            if (tile.IsCorrupted == corrupted)
                return;

            tile.IsCorrupted = corrupted;
            TileChanged?.Invoke(wrapped);
            if (corrupted)
                TileCorrupted?.Invoke(wrapped);
        }

        /// <summary>First index holding the given type, or -1 if absent.</summary>
        public int FindFirstIndex(TileType type)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].Type == type)
                    return i;
            }

            return -1;
        }

        public Crop GetCrop(int index) => GetTile(index).crop;

        public void SetCrop(int index, Crop crop)
        {
            int wrapped = WrapIndex(index);
            Tile tile = _tiles[wrapped];
            if (tile.crop == crop)
                return;

            tile.crop = crop;
            TileChanged?.Invoke(wrapped);
        }

        /// <summary>
        /// Re-broadcasts a tile whose crop mutated in place (watered, grew) —
        /// those changes don't go through SetCrop but views still need them.
        /// </summary>
        internal void NotifyTileChanged(int index) => TileChanged?.Invoke(WrapIndex(index));
    }
}
