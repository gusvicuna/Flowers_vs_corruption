using UnityEngine;
using System.Collections.Generic;
using FlowersVsCorruption.World;
using FlowersVsCorruption.Player;
using FlowersVsCorruption.Core;
using UnityEngine.InputSystem;

namespace FlowersVsCorruption.Farming
{
    public class FarmingController : MonoBehaviour
    {
        [SerializeField] private WorldView _worldView;
        [SerializeField] private TimeSystem _timeSystem;
        [SerializeField] private CorruptionController _corruptionController;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TileVisuals _tileVisuals;
        [SerializeField] private CropDefinition[] _seedCycle;
        [SerializeField] private InputActionReference _previousSeed;
        [SerializeField] private InputActionReference _nextSeed;

        private FarmingSystem _farmingSystem;
        private int _selectedSeedIndex = 0;
        public int SelectedSeedIndex => _selectedSeedIndex;
        public CropDefinition SelectedSeed => _seedCycle != null && _seedCycle.Length > 0 ? _seedCycle[_selectedSeedIndex] : null;

        public FarmingSystem FarmingSystem => _farmingSystem;

        private void OnEnable()
        {
            _previousSeed.action.Enable();
            _nextSeed.action.Enable();
            _previousSeed.action.performed += OnPreviousSeedPerformed;
            _nextSeed.action.performed += OnNextSeedPerformed;
        }

        private void OnDisable()
        {
            // A lambda passed to -= is a NEW delegate instance and removes
            // nothing — named handlers are required to unsubscribe.
            _previousSeed.action.performed -= OnPreviousSeedPerformed;
            _nextSeed.action.performed -= OnNextSeedPerformed;
            _previousSeed.action.Disable();
            _nextSeed.action.Disable();
        }

        private void OnPreviousSeedPerformed(InputAction.CallbackContext _) => SelectPreviousSeed();

        private void OnNextSeedPerformed(InputAction.CallbackContext _) => SelectNextSeed();

        private void Start()
        {
            _farmingSystem = new FarmingSystem(_worldView.Grid);
            _corruptionController.CorruptionSystem.IsTileGuarded += _farmingSystem.IsTileGuarded;
            _corruptionController.CorruptionSystem.SpreadBlocked += _farmingSystem.OnSpreadBlocked;
            _timeSystem.DawnStarted += _ => _farmingSystem.OnDawn();

            _playerController.TileActionPerformed += OnTileAction;
        }

        private void OnDestroy()
        {
            _playerController.TileActionPerformed -= OnTileAction;
        }

        private void OnTileAction(int index)
        {
            if (_farmingSystem.Harvest(index))
            {
                Debug.Log($"Harvested crop at tile {index}");
            }
            else if (_farmingSystem.Water(index))
            {
                Debug.Log($"Watered crop at tile {index}");
            }
            else if (_farmingSystem.Plant(index, SelectedSeed))
            {
                Debug.Log($"Planted {SelectedSeed.name} at tile {index}");
            }
            else
            {
                Debug.Log($"No action performed at tile {index}");
            }
        }

        private void SelectPreviousSeed()
        {
            if (_seedCycle == null || _seedCycle.Length == 0) return;
            _selectedSeedIndex = (_selectedSeedIndex - 1 + _seedCycle.Length) % _seedCycle.Length;
            Debug.Log($"Selected seed: {_seedCycle[_selectedSeedIndex].name}");
        }

        private void SelectNextSeed()
        {
            if (_seedCycle == null || _seedCycle.Length == 0) return;
            _selectedSeedIndex = (_selectedSeedIndex + 1) % _seedCycle.Length;
            Debug.Log($"Selected seed: {_seedCycle[_selectedSeedIndex].name}");
        }
    }
}
