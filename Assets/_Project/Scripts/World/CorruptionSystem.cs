using System;
using System.Collections.Generic;

namespace FlowersVsCorruption.World
{
    public class CorruptionSystem
    {
        private readonly WorldGrid _grid;
        public CorruptionSystem(WorldGrid grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            _grid = grid;

            _grid.TileChanged += OnTileChanged;
        }

        /// Predicado inyectable: si devuelve true para un índice, ese tile no
        /// puede corromperse (futuras flores). null = nada guardado.
        public Func<int, bool> IsTileGuarded { get; set; }

        /// Disparado cuando el tile House pasa de limpio a corrupto,
        /// SIN IMPORTAR la fuente (spread, debug, lo que sea). Arg: índice del tile.
        public event Action<int> HouseCorrupted;

        public event Action<int> SpreadBlocked;

        /// Avanza la corrupción `waves` oleadas (no-op si waves <= 0).
        public void Spread(int waves)
        {
            if (waves <= 0) return;

            for (int wave = 0; wave < waves; wave++)
            {
                if (IsWorldCorrupted() || IsWorldHealed()) return;
                List<int> tilesToCorrupt = FindTilesToCorrupt();
                CorruptTiles(tilesToCorrupt);
            }
        }

        private List<int> FindTilesToCorrupt()
        {
            var tilesToCorrupt = new List<int>();
            for (int i = 0; i < _grid.TileCount; i++)
            {
                if (_grid.IsCorrupted(i)) continue;
                if (_grid.IsCorrupted(_grid.NextIndex(i)) || _grid.IsCorrupted(_grid.PreviousIndex(i)))
                {
                    if (IsTileGuarded != null && IsTileGuarded(i))
                    {
                        SpreadBlocked?.Invoke(i);
                        continue;
                    }
                    tilesToCorrupt.Add(i);
                }
            }

            return tilesToCorrupt;
        }
        private void CorruptTiles(List<int> tilesToCorrupt)
        {
            foreach (var tile in tilesToCorrupt)
            {
                _grid.SetCorrupted(tile, true);
            }
        }
        private bool IsWorldCorrupted()
        {
            if (_grid == null) throw new InvalidOperationException("World grid is not initialized.");
            for (int i = 0; i < _grid.TileCount; i++)
            {
                if (!_grid.IsCorrupted(i)) return false;
            }
            return true;
        }
        private bool IsWorldHealed()
        {
            if (_grid == null) throw new InvalidOperationException("World grid is not initialized.");
            for (int i = 0; i < _grid.TileCount; i++)
            {
                if (_grid.IsCorrupted(i)) return false;
            }
            return true;
        }

        private void OnTileChanged(int index)
        {
            // TileChanged also fires when a tile is CLEANSED — only a house
            // that is now corrupted counts.
            if (_grid.GetTile(index).Type == TileType.House && _grid.IsCorrupted(index))
            {
                HouseCorrupted?.Invoke(index);
            }
        }
    }
}
