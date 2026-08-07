using System;
using FlowersVsCorruption.Core;
using FlowersVsCorruption.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlowersVsCorruption.Player
{
    /// <summary>
    /// Thin adapter: reads input, ticks the RingWalker, and keeps the transform
    /// standing on the ring surface. Future systems learn where the player is
    /// through CurrentTileIndex / CurrentTileChanged.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private WorldView _worldView;
        [SerializeField] private GameConfig _config;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _tileAction;

        private RingWalker _walker;
        private float _surfaceRadius;

        public float AngleDegrees => _walker?.AngleDegrees ?? 0f;
        public int CurrentTileIndex => _walker?.CurrentTileIndex ?? 0;

        /// <summary>Also fired once at Start with the initial tile.</summary>
        public event Action<int> CurrentTileChanged;

        public event Action<int> TileActionPerformed;

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _tileAction.action.Enable();
            _tileAction.action.performed += OnTileAction;
        }

        private void OnDisable()
        {
            _tileAction.action.performed -= OnTileAction;
        }

        private void Start()
        {
            WorldGrid grid = _worldView.Grid;

            int startIndex = grid.FindFirstIndex(TileType.House);
            if (startIndex < 0)
                startIndex = 0;

            _walker = new RingWalker(grid, grid.AngleOf(startIndex));
            _walker.CurrentTileChanged += index => CurrentTileChanged?.Invoke(index);

            _surfaceRadius = _config.PlanetRadius(grid.TileCount) + _config.TileHeight * 0.5f;

            SpriteFitter.Fit(_sprite, _config.PlayerSize.x, _config.PlayerSize.y);
            _sprite.transform.localPosition = new Vector3(0f, _config.PlayerSize.y * 0.5f, 0f);

            ApplyTransform();
            CurrentTileChanged?.Invoke(_walker.CurrentTileIndex);
        }

        private void Update()
        {
            _walker.Move(
                _moveAction.action.ReadValue<float>(),
                _config.WalkSpeedTilesPerSecond,
                Time.deltaTime);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            Vector3 center = _worldView.transform.position;
            transform.SetPositionAndRotation(
                center + RingGeometry.PositionAt(_walker.AngleDegrees, _surfaceRadius),
                RingGeometry.RotationAt(_walker.AngleDegrees));
        }

        private void OnTileAction(InputAction.CallbackContext context)
        {
            if (_walker == null)
                return;

            int index = _walker.CurrentTileIndex;
            TileActionPerformed?.Invoke(index);

            // TEMP (Feature 2 debug): toggles corruption to prove the
            // input -> tile -> grid -> view chain; replaced by real tile
            // actions in the farming feature.
            WorldGrid grid = _worldView.Grid;
            grid.SetCorrupted(index, !grid.IsCorrupted(index));
            Debug.Log($"Tile action on {index} ({grid.GetTile(index).Type})", this);
        }
    }
}
