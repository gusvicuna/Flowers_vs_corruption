using System;
using System.Collections.Generic;
using FlowersVsCorruption.Farming;
using FlowersVsCorruption.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FlowersVsCorruption.Tests
{
    /// <summary>
    /// Spec for FarmingSystem (Feature 5). 24-tile ring: House 0, Rock 6,
    /// CorruptionBase 12, Soil everywhere else. Definitions are built per test
    /// via ScriptableObject + SerializedObject (WorldLayoutTests pattern).
    /// Dawn order is strict: cleanse -> growth -> watered reset.
    /// </summary>
    public class FarmingSystemTests
    {
        private const int Count = 24;
        private const int HouseIndex = 0;
        private const int RockIndex = 6;
        private const int BaseIndex = 12;
        private const int SoilA = 3;    // plain soil far from everything
        private const int SoilB = 9;

        private WorldGrid _grid;
        private FarmingSystem _farming;
        private List<CropDefinition> _defs;

        [SetUp]
        public void SetUp()
        {
            var types = new TileType[Count];
            for (int i = 0; i < Count; i++)
                types[i] = TileType.Soil;
            types[HouseIndex] = TileType.House;
            types[RockIndex] = TileType.Rock;
            types[BaseIndex] = TileType.CorruptionBase;
            _grid = new WorldGrid(types);
            _farming = new FarmingSystem(_grid);
            _defs = new List<CropDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (CropDefinition def in _defs)
                UnityEngine.Object.DestroyImmediate(def);
        }

        private CropDefinition MakeDef(
            string name, PlantableGround plantableOn, int stages,
            bool guards, bool cleanses, int grownHealth, bool harvestable)
        {
            var def = ScriptableObject.CreateInstance<CropDefinition>();
            var so = new SerializedObject(def);
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_growthStages").intValue = stages;
            so.FindProperty("_plantableOn").enumValueIndex = (int)plantableOn;
            so.FindProperty("_guardsWhenGrown").boolValue = guards;
            so.FindProperty("_cleansesWhenGrown").boolValue = cleanses;
            so.FindProperty("_grownHealth").intValue = grownHealth;
            so.FindProperty("_isHarvestable").boolValue = harvestable;
            so.ApplyModifiedPropertiesWithoutUndo();
            _defs.Add(def);
            return def;
        }

        // Flowers ARE harvestable (they yield flower powder once the inventory
        // exists) — harvesting a grown one deliberately gives up the shield.
        private CropDefinition MakeFlowerDef(int stages = 2, int health = 3) =>
            MakeDef("Flower", PlantableGround.CleanSoil, stages,
                guards: true, cleanses: true, grownHealth: health, harvestable: true);

        private CropDefinition MakeFoodDef(int stages = 2) =>
            MakeDef("Food", PlantableGround.CleanSoil, stages,
                guards: false, cleanses: false, grownHealth: 1, harvestable: true);

        private CropDefinition MakeCorruptedDef(int stages = 2) =>
            MakeDef("Corrupted", PlantableGround.CorruptedSoil, stages,
                guards: false, cleanses: false, grownHealth: 1, harvestable: true);

        /// <summary>Waters + dawns until the crop at index is grown.</summary>
        private void GrowToGrown(int index)
        {
            for (int safety = 0; safety < 20 && !_grid.GetCrop(index).IsGrown; safety++)
            {
                _farming.Water(index);
                _farming.OnDawn();
            }

            Assert.That(_grid.GetCrop(index).IsGrown, Is.True, "helper failed to grow the crop");
        }

        // ---------- constructor ----------

        [Test]
        public void Constructor_NullGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FarmingSystem(null));
        }

        // ---------- planting ----------

        [Test]
        public void Plant_OnCleanSoil_SucceedsAndNotifies()
        {
            CropDefinition food = MakeFoodDef();
            var planted = new List<int>();
            _farming.CropPlanted += planted.Add;

            bool result = _farming.Plant(SoilA, food);

            Assert.That(result, Is.True);
            Assert.That(_grid.GetCrop(SoilA), Is.Not.Null);
            Assert.That(_grid.GetCrop(SoilA).Definition, Is.SameAs(food));
            Assert.That(_grid.GetCrop(SoilA).Stage, Is.EqualTo(0));
            Assert.That(planted, Is.EqualTo(new[] { SoilA }));
        }

        [Test]
        public void Plant_OnNonSoilTiles_Fails()
        {
            CropDefinition food = MakeFoodDef();

            Assert.That(_farming.Plant(HouseIndex, food), Is.False, "house");
            Assert.That(_farming.Plant(RockIndex, food), Is.False, "rock");
            Assert.That(_farming.Plant(BaseIndex, food), Is.False, "corruption base");
        }

        [Test]
        public void Plant_OnOccupiedTile_Fails()
        {
            CropDefinition food = MakeFoodDef();
            _farming.Plant(SoilA, food);

            Assert.That(_farming.Plant(SoilA, food), Is.False);
        }

        [Test]
        public void Plant_CleanSeedOnCorruptedSoil_Fails()
        {
            CropDefinition food = MakeFoodDef();
            _grid.SetCorrupted(SoilA, true);

            Assert.That(_farming.CanPlant(SoilA, food), Is.False);
            Assert.That(_farming.Plant(SoilA, food), Is.False);
        }

        [Test]
        public void Plant_CorruptedSeedOnCorruptedSoil_Succeeds()
        {
            CropDefinition corrupted = MakeCorruptedDef();
            _grid.SetCorrupted(SoilA, true);

            Assert.That(_farming.Plant(SoilA, corrupted), Is.True);
            Assert.That(_grid.GetCrop(SoilA).Definition, Is.SameAs(corrupted));
        }

        [Test]
        public void Plant_CorruptedSeedOnCleanSoil_Fails()
        {
            CropDefinition corrupted = MakeCorruptedDef();

            Assert.That(_farming.Plant(SoilA, corrupted), Is.False);
        }

        [Test]
        public void Plant_NullDefinition_Fails()
        {
            Assert.That(_farming.CanPlant(SoilA, null), Is.False);
            Assert.That(_farming.Plant(SoilA, null), Is.False);
        }

        [Test]
        public void Plant_WrapsIndex()
        {
            CropDefinition food = MakeFoodDef();

            Assert.That(_farming.Plant(SoilA + Count, food), Is.True);
            Assert.That(_grid.GetCrop(SoilA), Is.Not.Null);
        }

        // ---------- watering ----------

        [Test]
        public void Water_GrowingCrop_SucceedsAndNotifies()
        {
            _farming.Plant(SoilA, MakeFoodDef());
            var watered = new List<int>();
            _farming.CropWatered += watered.Add;

            bool result = _farming.Water(SoilA);

            Assert.That(result, Is.True);
            Assert.That(_grid.GetCrop(SoilA).WateredToday, Is.True);
            Assert.That(watered, Is.EqualTo(new[] { SoilA }));
        }

        [Test]
        public void Water_SameDayTwice_Fails()
        {
            _farming.Plant(SoilA, MakeFoodDef());
            _farming.Water(SoilA);

            Assert.That(_farming.Water(SoilA), Is.False);
        }

        [Test]
        public void Water_GrownCrop_Fails()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 2));
            GrowToGrown(SoilA);

            Assert.That(_farming.Water(SoilA), Is.False);
        }

        [Test]
        public void Water_EmptyTile_Fails()
        {
            Assert.That(_farming.Water(SoilA), Is.False);
        }

        [Test]
        public void Water_RaisesTileChanged_SoViewsRedraw()
        {
            _farming.Plant(SoilA, MakeFoodDef());
            var changed = new List<int>();
            _grid.TileChanged += changed.Add;

            _farming.Water(SoilA);

            Assert.That(changed, Does.Contain(SoilA),
                "watering mutates the crop in place — the grid must re-broadcast the tile or the wet-soil visual never updates");
        }

        [Test]
        public void OnDawn_Growth_RaisesTileChanged_SoViewsRedraw()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));
            _farming.Water(SoilA);
            var changed = new List<int>();
            _grid.TileChanged += changed.Add;

            _farming.OnDawn();

            Assert.That(changed, Does.Contain(SoilA),
                "growing at dawn mutates the crop in place — the grid must re-broadcast the tile or the crop never changes on screen");
        }

        [Test]
        public void OnDawn_FinalNotification_SeesWateredAlreadyReset()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));
            _farming.Water(SoilA);
            bool? lastSeenWatered = null;
            _grid.TileChanged += i =>
            {
                if (i == SoilA)
                    lastSeenWatered = _grid.GetCrop(SoilA)?.WateredToday;
            };

            _farming.OnDawn();

            Assert.That(lastSeenWatered, Is.False,
                "views redraw synchronously per notification — the dawn's final redraw must observe the watered flag already reset, or the soil stays dark all day");
        }

        // ---------- dawn growth ----------

        [Test]
        public void OnDawn_WateredGrows_UnwateredDoesNot()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));
            _farming.Plant(SoilB, MakeFoodDef(stages: 3));
            _farming.Water(SoilA);

            _farming.OnDawn();

            Assert.That(_grid.GetCrop(SoilA).Stage, Is.EqualTo(1));
            Assert.That(_grid.GetCrop(SoilB).Stage, Is.EqualTo(0));
        }

        [Test]
        public void OnDawn_ResetsWateredFlag()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));
            _farming.Water(SoilA);

            _farming.OnDawn();

            Assert.That(_grid.GetCrop(SoilA).WateredToday, Is.False);
            Assert.That(_farming.Water(SoilA), Is.True, "watering must be available again after dawn");
        }

        [Test]
        public void OnDawn_MultipleDawns_ReachesGrownAndStops()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));

            _farming.Water(SoilA);
            _farming.OnDawn();   // stage 1
            _farming.Water(SoilA);
            _farming.OnDawn();   // stage 2 = grown
            _farming.OnDawn();   // extra dawn must not overflow

            Crop crop = _grid.GetCrop(SoilA);
            Assert.That(crop.Stage, Is.EqualTo(2));
            Assert.That(crop.IsGrown, Is.True);
        }

        // ---------- harvest ----------

        [Test]
        public void Harvest_GrownHarvestable_RemovesAndNotifies()
        {
            CropDefinition food = MakeFoodDef(stages: 2);
            _farming.Plant(SoilA, food);
            GrowToGrown(SoilA);
            var harvested = new List<(int, CropDefinition)>();
            _farming.CropHarvested += (i, def) => harvested.Add((i, def));

            bool result = _farming.Harvest(SoilA);

            Assert.That(result, Is.True);
            Assert.That(_grid.GetCrop(SoilA), Is.Null);
            Assert.That(harvested, Is.EqualTo(new[] { (SoilA, food) }));
        }

        [Test]
        public void Harvest_NotGrown_Fails()
        {
            _farming.Plant(SoilA, MakeFoodDef(stages: 3));

            Assert.That(_farming.Harvest(SoilA), Is.False);
            Assert.That(_grid.GetCrop(SoilA), Is.Not.Null);
        }

        [Test]
        public void Harvest_GrownFlower_Succeeds_GivingUpTheShield()
        {
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2));
            GrowToGrown(SoilA);

            Assert.That(_farming.Harvest(SoilA), Is.True);
            Assert.That(_grid.GetCrop(SoilA), Is.Null);
            Assert.That(_farming.IsTileGuarded(SoilA), Is.False,
                "harvesting the flower is a deliberate trade: powder for protection");
        }

        [Test]
        public void Harvest_NonHarvestableDefinition_Fails()
        {
            CropDefinition decorative = MakeDef("Decorative", PlantableGround.CleanSoil, 2,
                guards: false, cleanses: false, grownHealth: 1, harvestable: false);
            _farming.Plant(SoilA, decorative);
            GrowToGrown(SoilA);

            Assert.That(_farming.Harvest(SoilA), Is.False);
            Assert.That(_grid.GetCrop(SoilA), Is.Not.Null);
        }

        // ---------- guarding ----------

        [Test]
        public void IsTileGuarded_TrueOnlyForGrownLivingFlower()
        {
            Assert.That(_farming.IsTileGuarded(SoilA), Is.False, "empty tile");

            _farming.Plant(SoilA, MakeFlowerDef(stages: 2));
            Assert.That(_farming.IsTileGuarded(SoilA), Is.False, "growing flower");

            GrowToGrown(SoilA);
            Assert.That(_farming.IsTileGuarded(SoilA), Is.True, "grown flower");

            _farming.Plant(SoilB, MakeFoodDef(stages: 2));
            GrowToGrown(SoilB);
            Assert.That(_farming.IsTileGuarded(SoilB), Is.False, "grown food crop");
        }

        // ---------- dawn cleanse ----------

        [Test]
        public void OnDawn_GrownFlower_CleansesBothCorruptedNeighbors()
        {
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2));
            GrowToGrown(SoilA);
            _grid.SetCorrupted(SoilA - 1, true);
            _grid.SetCorrupted(SoilA + 1, true);

            _farming.OnDawn();

            Assert.That(_grid.IsCorrupted(SoilA - 1), Is.False);
            Assert.That(_grid.IsCorrupted(SoilA + 1), Is.False);
        }

        [Test]
        public void OnDawn_Cleanse_ReachesOnlyDirectNeighbors()
        {
            _farming.Plant(SoilB, MakeFlowerDef(stages: 2));
            GrowToGrown(SoilB);
            _grid.SetCorrupted(SoilB + 1, true);
            _grid.SetCorrupted(SoilB + 2, true);
            _grid.SetCorrupted(SoilB + 3, true);

            _farming.OnDawn();

            Assert.That(_grid.IsCorrupted(SoilB + 1), Is.False, "direct neighbor cleansed");
            Assert.That(_grid.IsCorrupted(SoilB + 2), Is.True, "two tiles away untouched");
            Assert.That(_grid.IsCorrupted(SoilB + 3), Is.True);
        }

        [Test]
        public void OnDawn_FlowerGrownThisDawn_CleansesNextDawn()
        {
            // Stages 2: one watered dawn to grow. Cleanse runs BEFORE growth,
            // so the dawn the flower grows must not cleanse yet.
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2));
            _grid.SetCorrupted(SoilA + 1, true);
            _farming.Water(SoilA);

            _farming.OnDawn();   // flower reaches grown here

            Assert.That(_grid.GetCrop(SoilA).IsGrown, Is.True);
            Assert.That(_grid.IsCorrupted(SoilA + 1), Is.True, "no cleanse on the growth dawn");

            _farming.OnDawn();

            Assert.That(_grid.IsCorrupted(SoilA + 1), Is.False, "cleansed the following dawn");
        }

        [Test]
        public void OnDawn_CleanseKillsCorruptedCropOnCleansedTile()
        {
            CropDefinition corrupted = MakeCorruptedDef();
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2));
            GrowToGrown(SoilA);
            _grid.SetCorrupted(SoilA + 1, true);
            _farming.Plant(SoilA + 1, corrupted);
            var killed = new List<(int, CropDefinition)>();
            _farming.CropKilled += (i, def) => killed.Add((i, def));

            _farming.OnDawn();

            Assert.That(_grid.IsCorrupted(SoilA + 1), Is.False);
            Assert.That(_grid.GetCrop(SoilA + 1), Is.Null, "corrupted crop dies with its ground");
            Assert.That(killed, Is.EqualTo(new[] { (SoilA + 1, corrupted) }));
        }

        // ---------- corruption killing crops ----------

        [Test]
        public void CorruptionEntering_KillsFoodCrop()
        {
            CropDefinition food = MakeFoodDef();
            _farming.Plant(SoilA, food);
            var killed = new List<(int, CropDefinition)>();
            _farming.CropKilled += (i, def) => killed.Add((i, def));

            _grid.SetCorrupted(SoilA, true);   // any source: the system watches the grid

            Assert.That(_grid.GetCrop(SoilA), Is.Null);
            Assert.That(killed, Is.EqualTo(new[] { (SoilA, food) }));
        }

        [Test]
        public void CorruptionEntering_KillsGrowingFlower()
        {
            CropDefinition flower = MakeFlowerDef(stages: 3);
            _farming.Plant(SoilA, flower);   // never grown: vulnerable

            _grid.SetCorrupted(SoilA, true);

            Assert.That(_grid.GetCrop(SoilA), Is.Null);
        }

        // ---------- flower health vs spread ----------

        [Test]
        public void OnSpreadBlocked_ReducesFlowerHealth()
        {
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2, health: 3));
            GrowToGrown(SoilA);

            _farming.OnSpreadBlocked(SoilA);

            Assert.That(_grid.GetCrop(SoilA).Health, Is.EqualTo(2));
            Assert.That(_farming.IsTileGuarded(SoilA), Is.True, "still alive, still guarding");
        }

        [Test]
        public void OnSpreadBlocked_SurvivingDamage_RaisesTileChanged()
        {
            _farming.Plant(SoilA, MakeFlowerDef(stages: 2, health: 3));
            GrowToGrown(SoilA);
            var changed = new List<int>();
            _grid.TileChanged += changed.Add;

            _farming.OnSpreadBlocked(SoilA);

            Assert.That(changed, Does.Contain(SoilA),
                "damage without death mutates the crop in place — views (health bar) need the re-broadcast");
        }

        [Test]
        public void OnSpreadBlocked_HealthZero_FlowerDies_TileCleanAndUnguarded()
        {
            CropDefinition flower = MakeFlowerDef(stages: 2, health: 1);
            _farming.Plant(SoilA, flower);
            GrowToGrown(SoilA);
            var killed = new List<(int, CropDefinition)>();
            _farming.CropKilled += (i, def) => killed.Add((i, def));

            _farming.OnSpreadBlocked(SoilA);

            Assert.That(_grid.GetCrop(SoilA), Is.Null);
            Assert.That(killed, Is.EqualTo(new[] { (SoilA, flower) }));
            Assert.That(_grid.IsCorrupted(SoilA), Is.False, "the killing wave is spent on the flower");
            Assert.That(_farming.IsTileGuarded(SoilA), Is.False);
        }

        // ---------- integration: siege ----------

        [Test]
        public void Siege_FlowerAbsorbsWavesThenFalls()
        {
            var corruption = new CorruptionSystem(_grid);
            corruption.IsTileGuarded = _farming.IsTileGuarded;
            corruption.SpreadBlocked += _farming.OnSpreadBlocked;

            // Flower at 3 grows to grown with health 2; front sits at 1-2.
            _farming.Plant(3, MakeFlowerDef(stages: 2, health: 2));
            _farming.Water(3);
            _farming.OnDawn();
            _grid.SetCorrupted(1, true);
            _grid.SetCorrupted(2, true);
            Assert.That(_farming.IsTileGuarded(3), Is.True);

            corruption.Spread(1);   // night 1: flower blocks (2 -> 1 health)
            Assert.That(_grid.IsCorrupted(3), Is.False);
            Assert.That(_grid.GetCrop(3).Health, Is.EqualTo(1));

            corruption.Spread(1);   // night 2: blocks again and dies; tile stays clean
            Assert.That(_grid.GetCrop(3), Is.Null);
            Assert.That(_grid.IsCorrupted(3), Is.False);

            corruption.Spread(1);   // night 3: nothing guards the tile anymore
            Assert.That(_grid.IsCorrupted(3), Is.True);
        }
    }
}
