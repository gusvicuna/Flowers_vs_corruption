using System;

namespace FlowersVsCorruption.Core
{
    public class DayNightClock
    {
        public event Action<int> DawnStarted;
        public event Action<int> DayStarted;
        public event Action<int> NightStarted;

        private readonly float _dawnSeconds;
        private readonly float _daySeconds;
        private readonly float _nightSeconds;

        private DayPhase _currentPhase;
        private int _dayNumber;
        private float _phaseElapsed;
        private bool _begun;

        public DayNightClock(float dawnSeconds, float daySeconds, float nightSeconds)
        {
            if (dawnSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(dawnSeconds));
            if (daySeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(daySeconds));
            if (nightSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(nightSeconds));

            _dawnSeconds = dawnSeconds;
            _daySeconds = daySeconds;
            _nightSeconds = nightSeconds;

            _currentPhase = DayPhase.Dawn;
            _dayNumber = 1;
            _phaseElapsed = 0f;
            IsRunning = true;
        }

        public DayPhase CurrentPhase => _currentPhase;
        public int DayNumber => _dayNumber;
        public float PhaseElapsed => _phaseElapsed;
        public float PhaseDuration => GetCurrentPhaseSeconds();
        public float PhaseTimeRemaining => GetCurrentPhaseSeconds() - _phaseElapsed;
        public float PhaseProgress01 => _phaseElapsed / GetCurrentPhaseSeconds();
        public bool IsRunning { get; private set; }

        public void Begin()
        {
            if (_begun) return;
            _begun = true;

            DawnStarted?.Invoke(_dayNumber);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || !IsRunning || !_begun) return;

            _phaseElapsed += deltaTime;
            while (_phaseElapsed >= GetCurrentPhaseSeconds())
            {
                _phaseElapsed -= GetCurrentPhaseSeconds();
                switch (_currentPhase)
                {
                    case DayPhase.Dawn:
                        _currentPhase = DayPhase.Day;
                        DayStarted?.Invoke(_dayNumber);
                        break;
                    case DayPhase.Day:
                        _currentPhase = DayPhase.Night;
                        NightStarted?.Invoke(_dayNumber);
                        break;
                    case DayPhase.Night:
                        _currentPhase = DayPhase.Dawn;
                        _dayNumber++;
                        DawnStarted?.Invoke(_dayNumber);
                        break;
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            IsRunning = true;
        }

        private float GetCurrentPhaseSeconds()
        {
            return _currentPhase switch
            {
                DayPhase.Dawn => _dawnSeconds,
                DayPhase.Day => _daySeconds,
                DayPhase.Night => _nightSeconds,
                _ => throw new InvalidOperationException("Unknown day phase")
            };
        }
    }
}
