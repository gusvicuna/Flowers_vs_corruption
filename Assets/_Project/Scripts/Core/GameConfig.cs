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

        [Header("Player")]
        [SerializeField, Min(0.1f)] private float _walkSpeedTilesPerSecond = 2f;
        [SerializeField] private Vector2 _playerSize = new Vector2(0.6f, 1f);

        [Header("Camera")]
        [SerializeField, Min(0.5f)] private float _cameraOrthoSize = 2.5f;
        [SerializeField, Min(0f)] private float _cameraSmoothTime = 0.15f;
        [Tooltip("World units the camera center sits outward of the player's feet — bigger = player lower on screen.")]
        [SerializeField, Min(0f)] private float _cameraVerticalOffset = 1f;

        public float TileWidth => _tileWidth;
        public float TileHeight => _tileHeight;
        public float TileOverlapScale => _tileOverlapScale;
        public float WalkSpeedTilesPerSecond => _walkSpeedTilesPerSecond;
        public Vector2 PlayerSize => _playerSize;
        public float CameraOrthoSize => _cameraOrthoSize;
        public float CameraSmoothTime => _cameraSmoothTime;
        public float CameraVerticalOffset => _cameraVerticalOffset;

        /// <summary>Ring centerline radius so tileCount tiles of TileWidth close the circle.</summary>
        public float PlanetRadius(int tileCount) => tileCount * _tileWidth / (2f * Mathf.PI);
    }
}
