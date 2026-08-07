using FlowersVsCorruption.Core;
using FlowersVsCorruption.World;
using UnityEngine;

namespace FlowersVsCorruption.Player
{
    /// <summary>
    /// Single marker sprite that snaps to the tile under the player. Purely
    /// event-driven: no per-frame work, no per-tile highlight state.
    /// </summary>
    public class TileHighlightView : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private WorldView _worldView;
        [SerializeField] private TileVisuals _visuals;
        [SerializeField] private GameConfig _config;
        [SerializeField] private SpriteRenderer _marker;

        private void OnEnable()
        {
            _player.CurrentTileChanged += SnapToTile;
        }

        private void OnDisable()
        {
            _player.CurrentTileChanged -= SnapToTile;
        }

        private void Start()
        {
            _marker.sprite = _visuals.HighlightSprite;
            _marker.color = _visuals.HighlightColor;
            SpriteFitter.Fit(_marker, _config.TileWidth * _config.TileOverlapScale, _config.TileHeight);
        }

        private void SnapToTile(int index)
        {
            float angle = _worldView.Grid.AngleOf(index);
            float radius = _config.PlanetRadius(_worldView.Grid.TileCount);
            transform.SetPositionAndRotation(
                _worldView.transform.position + RingGeometry.PositionAt(angle, radius),
                RingGeometry.RotationAt(angle));
        }
    }
}
