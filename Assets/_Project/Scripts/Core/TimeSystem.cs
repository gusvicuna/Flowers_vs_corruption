using UnityEngine;
using System;

namespace FlowersVsCorruption.Core
{
    public class TimeSystem : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        private DayNightClock _dayNightClock;

        // Other systems subscribe to these in OnEnable (always safe: they don't
        // need the clock to exist yet). The initial DawnStarted of day 1 fires
        // from Start, after every OnEnable has run.
        public event Action<int> DawnStarted;
        public event Action<int> DayStarted;
        public event Action<int> NightStarted;

        public DayPhase CurrentPhase => _dayNightClock.CurrentPhase;
        public int DayNumber => _dayNightClock.DayNumber;
        public float PhaseProgress01 => _dayNightClock.PhaseProgress01;
        public float PhaseTimeRemaining => _dayNightClock.PhaseTimeRemaining;

        private void Awake()
        {
            _dayNightClock = new DayNightClock(
                _config.DawnDurationSeconds,
                _config.DayDurationSeconds,
                _config.NightDurationSeconds
            );

            _dayNightClock.DawnStarted += day => DawnStarted?.Invoke(day);
            _dayNightClock.DayStarted += day => DayStarted?.Invoke(day);
            _dayNightClock.NightStarted += day => NightStarted?.Invoke(day);
        }


        private void Start()
        {
            _dayNightClock.Begin();
        }

        private void Update()
        {
            _dayNightClock.Tick(Time.deltaTime);
        }
    }
}
