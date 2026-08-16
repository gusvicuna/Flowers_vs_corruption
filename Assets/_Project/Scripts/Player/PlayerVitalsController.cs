using UnityEngine;
using FlowersVsCorruption.Core;
using FlowersVsCorruption.World;

namespace FlowersVsCorruption.Player
{
    public class PlayerVitalsController : MonoBehaviour
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private WorldView _worldView;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TimeSystem _timeSystem;

        private PlayerVitals _playerVitals;

        public PlayerVitals PlayerVitals => _playerVitals;

        private void Awake()
        {
            _playerVitals = new PlayerVitals(new VitalsSettings
            {
                MaxHunger = _gameConfig.MaxHunger,
                MaxHealth = _gameConfig.MaxHealth,
                HungerDrainPerSecond = _gameConfig.HungerDrainPerSecond,
                CorruptionDamagePerSecond = _gameConfig.CorruptionDamagePerSecond,
                StarvingDamagePerSecond = _gameConfig.StarvingDamagePerSecond,
                StartingHunger = _gameConfig.MaxHunger,
                StartingHealth = _gameConfig.MaxHealth
            });
        }

        private void OnEnable()
        {
            _playerVitals.PlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            _playerVitals.PlayerDied -= OnPlayerDied;
        }

        private void Update()
        {
            _playerVitals.Tick(
                Time.deltaTime,
                _worldView.Grid.IsCorrupted(_playerController.CurrentTileIndex)
            );
        }

        private void OnPlayerDied()
        {
            _timeSystem.StopClock();
            Debug.LogWarning("Player has died. Game over.");
        }

        // TEMP (Feature 6 debug): lets us feed the player before the inventory
        // exists. Janhavi's inventory calls PlayerVitals.Consume with the eaten
        // crop's own values instead.
        [ContextMenu("Debug: Eat")]
        private void DebugEat()
        {
            _playerVitals.Consume(hungerRestored: 20f, healthRestored: 10f, healthDamage: 0f);
        }

    }
}
