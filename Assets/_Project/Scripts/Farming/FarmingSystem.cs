using System;
using FlowersVsCorruption.World;

namespace FlowersVsCorruption.Farming
{
    public class FarmingSystem
    {
        private readonly WorldGrid _grid;
        public FarmingSystem(WorldGrid grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            _grid = grid;
            _grid.TileCorrupted += OnCorruptionEntered;
        }

        public event Action<int> CropPlanted;
        public event Action<int> CropWatered;
        public event Action<int, CropDefinition> CropHarvested;
        public event Action<int, CropDefinition> CropKilled;

        public bool CanPlant(int index, CropDefinition def)
        {
            if (def == null) return false;
            Tile tile = _grid.GetTile(index);
            if (tile.Crop != null) return false;
            if (tile.Type == TileType.Soil)
            {
                if (def.PlantableOn == PlantableGround.CleanSoil && !tile.IsCorrupted) return true;
                if (def.PlantableOn == PlantableGround.CorruptedSoil && tile.IsCorrupted) return true;
            }
            return false;
        }
        public bool Plant(int index, CropDefinition def)
        {
            if (def == null) return false;
            if (!CanPlant(index, def)) return false;
            _grid.SetCrop(index, new Crop(def));
            CropPlanted?.Invoke(index);
            return true;
        }
        public bool Water(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.Crop == null) return false;
            if (tile.Crop.WateredToday || tile.Crop.IsGrown) return false;
            tile.Crop.Water();
            _grid.NotifyTileChanged(index);
            CropWatered?.Invoke(index);
            return true;
        }
        public bool Harvest(int index)
        {
            Tile tile = _grid.GetTile(index);
            Crop crop = tile.Crop;
            if (crop == null) return false;
            if (!crop.IsGrown || !crop.IsHarvestable) return false;
            _grid.SetCrop(index, null);
            CropHarvested?.Invoke(index, crop.Definition);
            return true;
        }

        public bool IsTileGuarded(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.Crop == null) return false;
            return tile.Crop.IsGrown && tile.Crop.Definition.GuardsWhenGrown;
        }
        public void OnSpreadBlocked(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.Crop != null)
            {
                Crop crop = tile.Crop;
                crop.Damage(1);
                if (crop.Health <= 0)
                {
                    _grid.SetCrop(index, null);
                    CropKilled?.Invoke(index, crop.Definition);
                }
                else
                {
                    // Surviving damage is another in-place mutation: re-notify
                    // so views (health bar) redraw.
                    _grid.NotifyTileChanged(index);
                }
            }
        }
        // Subscribed to WorldGrid.TileCorrupted in the constructor: corruption
        // from ANY source kills whatever grows there.
        private void OnCorruptionEntered(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.Crop != null && tile.IsCorrupted)
            {
                Crop crop = tile.Crop;
                _grid.SetCrop(index, null);
                CropKilled?.Invoke(index, crop.Definition);
            }
        }
        public void OnDawn()
        {
            Cleanse();
            Growth();
            ResetWatered();
        }

        private void Cleanse()
        {
            for (int i = 0; i < _grid.TileCount; i++)
            {
                Crop crop = _grid.GetTile(i).Crop;
                if (crop == null || !crop.IsGrown || !crop.Definition.CleansesWhenGrown)
                    continue;

                CleanseTile(_grid.PreviousIndex(i));
                CleanseTile(_grid.NextIndex(i));
            }
        }

        private void CleanseTile(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (!tile.IsCorrupted)
                return;

            _grid.SetCorrupted(index, false);

            // A corrupted crop dies with its ground — the exact mirror of
            // corruption killing a clean crop.
            Crop crop = tile.Crop;
            if (crop != null)
            {
                _grid.SetCrop(index, null);
                CropKilled?.Invoke(index, crop.Definition);
            }
        }

        private void Growth()
        {
            for (int i = 0; i < _grid.TileCount; i++)
            {
                Tile tile = _grid.GetTile(i);
                if (tile.Crop != null)
                {
                    Crop crop = tile.Crop;
                    if (!crop.IsGrown && crop.WateredToday)
                    {
                        crop.AdvanceGrowth();
                        _grid.NotifyTileChanged(i);
                    }
                }
            }
        }

        private void ResetWatered()
        {
            for (int i = 0; i < _grid.TileCount; i++)
            {
                Tile tile = _grid.GetTile(i);
                if (tile.Crop != null && tile.Crop.WateredToday)
                {
                    tile.Crop.ResetWatered();
                    // Growth() already notified, but views redraw synchronously
                    // and saw WateredToday still true — re-notify so the final
                    // redraw of the dawn sees the reset flag (dry soil again).
                    _grid.NotifyTileChanged(i);
                }
            }
        }
    }
}
