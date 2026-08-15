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
            if (tile.crop != null) return false;
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
            if (tile.crop == null) return false;
            if (tile.crop.WateredToday || tile.crop.IsGrown) return false;
            tile.crop.Water();
            _grid.NotifyTileChanged(index);
            CropWatered?.Invoke(index);
            return true;
        }
        public bool Harvest(int index)
        {
            Tile tile = _grid.GetTile(index);
            Crop crop = tile.crop;
            if (crop == null) return false;
            if (!crop.IsGrown || !crop.IsHarvestable) return false;
            _grid.SetCrop(index, null);
            CropHarvested?.Invoke(index, crop.Definition);
            return true;
        }

        public bool IsTileGuarded(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.crop == null) return false;
            return tile.crop.IsGrown && tile.crop.Definition.GuardsWhenGrown;
        }
        public void OnSpreadBlocked(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.crop != null)
            {
                Crop crop = tile.crop;
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
        public void OnCorruptionEntered(int index)
        {
            Tile tile = _grid.GetTile(index);
            if (tile.crop != null && tile.IsCorrupted)
            {
                Crop crop = tile.crop;
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
                Tile tile = _grid.GetTile(i);
                if (tile.crop != null)
                {
                    if (tile.crop.IsGrown && tile.crop.Definition.CleansesWhenGrown)
                    {
                        Tile leftTile = _grid.GetTile(_grid.PreviousIndex(i));
                        Tile rightTile = _grid.GetTile(_grid.NextIndex(i));
                        if (leftTile.IsCorrupted)
                        {
                            _grid.SetCorrupted(_grid.PreviousIndex(i), false);
                            if (leftTile.crop != null)
                            {
                                CropKilled?.Invoke(_grid.PreviousIndex(i), leftTile.crop.Definition);
                                _grid.SetCrop(_grid.PreviousIndex(i), null);
                            }
                        }
                        if (rightTile.IsCorrupted)
                        {
                            _grid.SetCorrupted(_grid.NextIndex(i), false);
                            if (rightTile.crop != null)
                            {
                                CropKilled?.Invoke(_grid.NextIndex(i), rightTile.crop.Definition);
                                _grid.SetCrop(_grid.NextIndex(i), null);
                            }
                        }
                    }
                }
            }
        }

        private void Growth()
        {
            for (int i = 0; i < _grid.TileCount; i++)
            {
                Tile tile = _grid.GetTile(i);
                if (tile.crop != null)
                {
                    Crop crop = tile.crop;
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
                if (tile.crop != null && tile.crop.WateredToday)
                {
                    tile.crop.ResetWatered();
                    // Growth() already notified, but views redraw synchronously
                    // and saw WateredToday still true — re-notify so the final
                    // redraw of the dawn sees the reset flag (dry soil again).
                    _grid.NotifyTileChanged(i);
                }
            }
        }
    }
}
