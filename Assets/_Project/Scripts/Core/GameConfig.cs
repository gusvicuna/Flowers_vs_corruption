using UnityEngine;

namespace FlowersVsCorruption.Core
{
    [CreateAssetMenu(menuName = "FvC/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("World Geometry")]
        [SerializeField, Min(0.01f)] private float _tileWidth = 1f;
        [SerializeField, Min(0.01f)] private float _tileHeight = 0.6f;
        [Tooltip("Widens each tile slightly so rotated rectangles leave no gaps on the ring.")]
        [SerializeField, Min(1f)] private float _tileOverlapScale = 1.08f;

        public float TileWidth => _tileWidth;
        public float TileHeight => _tileHeight;
        public float TileOverlapScale => _tileOverlapScale;

        /// <summary>Ring centerline radius so tileCount tiles of TileWidth close the circle.</summary>
        public float PlanetRadius(int tileCount) => tileCount * _tileWidth / (2f * Mathf.PI);
    }
}
