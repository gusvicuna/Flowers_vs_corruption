using FlowersVsCorruption.Core;
using FlowersVsCorruption.Farming;
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
        [SerializeField] private SpriteRenderer _crop;
        [SerializeField] private SpriteRenderer _healthBarBackground;
        [SerializeField] private SpriteRenderer _healthBarFill;

        private Color _groundBaseColor;
        private float _tileWidth;
        private float _tileHeight;

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

            _tileWidth = width;
            _tileHeight = height;
            _groundBaseColor = _ground.color;
            _crop.enabled = false;

            _healthBarBackground.sortingOrder = 16;
            _healthBarFill.sortingOrder = 17;
            _healthBarBackground.enabled = false;
            _healthBarFill.enabled = false;
        }

        public void Refresh(Tile tile)
        {
            _corruptionOverlay.enabled = tile.IsCorrupted;

            Crop crop = tile.crop;
            if (crop != null)
            {
                _crop.enabled = true;
                _crop.sprite = crop.Definition.Sprite;
                _crop.color = crop.Definition.Tint;
                int maxStage = crop.Definition.GrowthStages - 1;
                float t = maxStage > 0 ? crop.Stage / (float)maxStage : 1f;
                // bounds is a world-space AABB — wrong for rotated tiles.
                // Use the sizes Init received; square keeps the placeholder round.
                float cropSize = Mathf.Lerp(0.35f, 0.8f, t) * _tileHeight;
                SpriteFitter.Fit(_crop, cropSize, cropSize);
                _crop.transform.localPosition = new Vector3(0f, _tileHeight * 0.5f + cropSize * 0.5f, 0f);
                RefreshHealthBar(crop, cropSize);
            }
            else
            {
                _crop.enabled = false;
                _healthBarBackground.enabled = false;
                _healthBarFill.enabled = false;
            }

            Color b = _groundBaseColor;
            _ground.color = crop != null && crop.WateredToday
                ? new Color(b.r * 0.8f, b.g * 0.8f, b.b * 0.8f, b.a)
                : _groundBaseColor;
        }

        /// <summary>
        /// Only grown crops show the bar: full = ready (food) / unhurt (flower);
        /// it drains as spread waves hit a guarding flower.
        /// </summary>
        private void RefreshHealthBar(Crop crop, float cropSize)
        {
            bool visible = crop.IsGrown;
            _healthBarBackground.enabled = visible;
            _healthBarFill.enabled = visible;
            if (!visible)
                return;

            float barWidth = cropSize;
            float barHeight = 0.12f * _tileHeight;
            float barY = _tileHeight * 0.5f + cropSize + barHeight;

            int maxHealth = Mathf.Max(1, crop.Definition.GrownHealth);
            float fraction = Mathf.Clamp01(crop.Health / (float)maxHealth);

            _healthBarBackground.color = new Color(0f, 0f, 0f, 0.6f);
            SpriteFitter.Fit(_healthBarBackground, barWidth, barHeight);
            _healthBarBackground.transform.localPosition = new Vector3(0f, barY, 0f);

            float innerWidth = barWidth * 0.92f;
            _healthBarFill.color = Color.Lerp(Color.red, Color.green, fraction);
            SpriteFitter.Fit(_healthBarFill, Mathf.Max(fraction * innerWidth, 0.001f), barHeight * 0.6f);
            // Keep the fill's LEFT edge fixed so it drains toward the left.
            _healthBarFill.transform.localPosition = new Vector3(-innerWidth * (1f - fraction) * 0.5f, barY, 0f);
        }
    }
}
