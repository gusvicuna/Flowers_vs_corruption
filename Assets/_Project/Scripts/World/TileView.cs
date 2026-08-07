using FlowersVsCorruption.Core;
using UnityEngine;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// Dumb visual for one ring tile. Holds no authoritative state. The root
    /// transform is the tile's logical anchor (position/rotation on the ring);
    /// the sprite children are scaled to the configured tile size.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _ground;
        [SerializeField] private SpriteRenderer _corruptionOverlay;

        public int Index { get; private set; }

        public void Init(int index, TileType type, TileVisuals visuals, float width, float height)
        {
            Index = index;
            name = $"Tile_{index:00}_{type}";

            if (visuals.TryGet(type, out Sprite sprite, out Color tint))
            {
                _ground.sprite = sprite;
                _ground.color = tint;
            }
            else
            {
                _ground.color = Color.magenta;
                Debug.LogWarning($"No visuals entry for tile type {type}.", this);
            }

            _corruptionOverlay.sprite = visuals.CorruptionOverlaySprite;
            _corruptionOverlay.color = visuals.CorruptionColor;
            _corruptionOverlay.enabled = false;

            SpriteFitter.Fit(_ground, width, height);
            SpriteFitter.Fit(_corruptionOverlay, width, height);
        }

        public void Refresh(Tile tile)
        {
            _corruptionOverlay.enabled = tile.IsCorrupted;
        }
    }
}
