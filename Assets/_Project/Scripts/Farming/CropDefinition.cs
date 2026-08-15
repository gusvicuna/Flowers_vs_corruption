using UnityEngine;

namespace FlowersVsCorruption.Farming
{
    [CreateAssetMenu(fileName = "NewCropDefinition", menuName = "FvC/Crop Definition")]
    public class CropDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _tint = Color.white;   // no default = invisible (0,0,0,0)
        [SerializeField][Min(2)] private int _growthStages = 3;
        [SerializeField] private PlantableGround _plantableOn;
        [SerializeField] private bool _guardsWhenGrown;
        [SerializeField] private bool _cleansesWhenGrown;
        [SerializeField][Min(1)] private int _grownHealth = 3;
        [SerializeField] private bool _isHarvestable;

        public string DisplayName => _displayName;
        public Sprite Sprite => _sprite;
        public Color Tint => _tint;
        public int GrowthStages => _growthStages;
        public PlantableGround PlantableOn => _plantableOn;
        public bool GuardsWhenGrown => _guardsWhenGrown;
        public bool CleansesWhenGrown => _cleansesWhenGrown;
        public int GrownHealth => _grownHealth;
        public bool IsHarvestable => _isHarvestable;
    }
}
