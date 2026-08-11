using UnityEngine;

namespace FlowersVsCorruption.Core
{
    [CreateAssetMenu(menuName = "FvC/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Time")]
        [SerializeField, Min(0.1f)] private float _dawnDurationSeconds = 3f;
        [SerializeField, Min(0.1f)] private float _dayDurationSeconds = 90f;
        [SerializeField, Min(0.1f)] private float _nightDurationSeconds = 30f;

        [Header("Sky")]
        [SerializeField] private Color _dawnSkyColor = new Color(1f, 0.7f, 0.5f);
        [SerializeField] private Color _daySkyColor = new Color(0.5f, 0.8f, 1f);
        [SerializeField] private Color _nightSkyColor = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private Color _dawnLightColor = new Color(1f, 0.8f, 0.6f);
        [SerializeField] private Color _dayLightColor = new Color(1f, 1f, 0.9f);
        [SerializeField] private Color _nightLightColor = new Color(0.2f, 0.2f, 0.5f);
        [SerializeField, Min(0.1f)] private float _skyTransitionSeconds = 5f;

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
        public float DawnDurationSeconds => _dawnDurationSeconds;
        public float DayDurationSeconds => _dayDurationSeconds;
        public float NightDurationSeconds => _nightDurationSeconds;
        public Color DawnSkyColor => _dawnSkyColor;
        public Color DaySkyColor => _daySkyColor;
        public Color NightSkyColor => _nightSkyColor;
        public Color DawnLightColor => _dawnLightColor;
        public Color DayLightColor => _dayLightColor;
        public Color NightLightColor => _nightLightColor;
        public float SkyTransitionSeconds => _skyTransitionSeconds;

        /// <summary>Ring centerline radius so tileCount tiles of TileWidth close the circle.</summary>
        public float PlanetRadius(int tileCount) => tileCount * _tileWidth / (2f * Mathf.PI);
    }
}
