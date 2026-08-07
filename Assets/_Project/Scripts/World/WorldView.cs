using FlowersVsCorruption.Core;
using UnityEngine;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// Builds the visual ring from the layout and keeps tile views in sync with
    /// the model via WorldGrid.TileChanged. Future systems reach the model
    /// through <see cref="Grid"/>.
    /// </summary>
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private WorldLayout _layout;
        [SerializeField] private GameConfig _config;
        [SerializeField] private TileVisuals _visuals;
        [SerializeField] private TileView _tilePrefab;
        [SerializeField] private SpriteRenderer _planetCore;
        [SerializeField, Min(0f)] private float _coreBleed = 0.1f;

        private TileView[] _tileViews;

        public WorldGrid Grid { get; private set; }

        private void Awake()
        {
            Grid = _layout.CreateGrid();
            BuildRing();
            Grid.TileChanged += OnTileChanged;
        }

        private void OnDestroy()
        {
            if (Grid != null)
                Grid.TileChanged -= OnTileChanged;
        }

        private void BuildRing()
        {
            float radius = _config.PlanetRadius(Grid.TileCount);
            float tileWidth = _config.TileWidth * _config.TileOverlapScale;
            _tileViews = new TileView[Grid.TileCount];

            for (int i = 0; i < Grid.TileCount; i++)
            {
                float angle = Grid.AngleOf(i);
                float radians = angle * Mathf.Deg2Rad;

                TileView view = Instantiate(_tilePrefab, transform);
                view.transform.localPosition = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
                view.transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
                view.Init(i, Grid.GetTile(i).Type, _visuals, tileWidth, _config.TileHeight);
                view.Refresh(Grid.GetTile(i));   // pick up state set before we subscribed (initial corruption)
                _tileViews[i] = view;
            }

            float coreDiameter = 2f * (radius - _config.TileHeight * 0.5f) + _coreBleed;
            SpriteFitter.Fit(_planetCore, coreDiameter, coreDiameter);
        }

        private void OnTileChanged(int index)
        {
            _tileViews[index].Refresh(Grid.GetTile(index));
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Corrupt Random Tile")]
        private void DebugCorruptRandomTile()
        {
            if (Grid == null)
            {
                Debug.LogWarning("Enter Play Mode first.", this);
                return;
            }

            Grid.SetCorrupted(Random.Range(0, Grid.TileCount), true);
        }
#endif
    }
}
