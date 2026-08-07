using System;
using UnityEngine;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// Maps tile types to their look. Art swaps happen here only — never in
    /// prefabs or scenes.
    /// </summary>
    [CreateAssetMenu(menuName = "FvC/Tile Visuals", fileName = "TileVisuals")]
    public class TileVisuals : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public TileType Type;
            public Sprite Sprite;
            public Color Tint;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        [Header("Corruption")]
        [SerializeField] private Sprite _corruptionOverlaySprite;
        [SerializeField] private Color _corruptionColor = new Color(0.24f, 0.04f, 0.37f, 0.7f);

        public Sprite CorruptionOverlaySprite => _corruptionOverlaySprite;
        public Color CorruptionColor => _corruptionColor;

        public bool TryGet(TileType type, out Sprite sprite, out Color tint)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Type == type)
                {
                    sprite = entry.Sprite;
                    tint = entry.Tint;
                    return true;
                }
            }

            sprite = null;
            tint = Color.magenta;
            return false;
        }
    }
}
