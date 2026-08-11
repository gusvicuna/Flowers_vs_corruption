using System;
using System.Collections.Generic;
using FlowersVsCorruption.Core;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    /// <summary>
    /// Spec for DayNightClock (Feature 3). Short readable durations:
    /// dawn 2 s, day 5 s, night 3 s. Cycle: Dawn -> Day -> Night -> Dawn...
    /// </summary>
    public class DayNightClockTests
    {
        private const float Dawn = 2f;
        private const float Day = 5f;
        private const float Night = 3f;

        private static DayNightClock MakeClock() => new DayNightClock(Dawn, Day, Night);

        private static List<string> Record(DayNightClock clock)
        {
            var events = new List<string>();
            clock.DawnStarted += day => events.Add($"Dawn{day}");
            clock.DayStarted += day => events.Add($"Day{day}");
            clock.NightStarted += day => events.Add($"Night{day}");
            return events;
        }

        [Test]
        public void Constructor_StartsAtDawnDayOne_NoTicksApplied()
        {
            DayNightClock clock = MakeClock();

            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(clock.DayNumber, Is.EqualTo(1));
            Assert.That(clock.PhaseElapsed, Is.EqualTo(0f));
            Assert.That(clock.PhaseDuration, Is.EqualTo(Dawn));
            Assert.That(clock.PhaseProgress01, Is.EqualTo(0f));
            Assert.That(clock.IsRunning, Is.True);
        }

        [Test]
        public void Constructor_InvalidDurations_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(0f, Day, Night));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(-1f, Day, Night));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(Dawn, 0f, Night));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(Dawn, -1f, Night));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(Dawn, Day, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightClock(Dawn, Day, -1f));
        }

        [Test]
        public void Begin_FiresDawnStartedDayOne_OnlyOnce()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);

            clock.Begin();
            clock.Begin();   // second call must be a no-op

            Assert.That(events, Is.EqualTo(new[] { "Dawn1" }));
        }

        [Test]
        public void Tick_BeforeBegin_DoesNothing()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);

            clock.Tick(10f);

            Assert.That(clock.PhaseElapsed, Is.EqualTo(0f));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void Tick_WithinPhase_NoEvents_ProgressAdvances()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            events.Clear();

            clock.Tick(1f);

            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(clock.PhaseElapsed, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(clock.PhaseProgress01, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void Tick_DawnComplete_FiresDayStarted()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            events.Clear();

            clock.Tick(Dawn + 0.5f);

            Assert.That(events, Is.EqualTo(new[] { "Day1" }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Day));
            Assert.That(clock.PhaseElapsed, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(clock.PhaseDuration, Is.EqualTo(Day));
        }

        [Test]
        public void Tick_DayComplete_FiresNightStarted()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            clock.Tick(Dawn);        // -> Day
            events.Clear();

            clock.Tick(Day);         // -> Night

            Assert.That(events, Is.EqualTo(new[] { "Night1" }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Night));
            Assert.That(clock.DayNumber, Is.EqualTo(1));
        }

        [Test]
        public void Tick_NightComplete_FiresDawnStarted_DayTwo()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            clock.Tick(Dawn);        // -> Day
            clock.Tick(Day);         // -> Night
            events.Clear();

            clock.Tick(Night);       // -> Dawn of day 2

            Assert.That(events, Is.EqualTo(new[] { "Dawn2" }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(clock.DayNumber, Is.EqualTo(2));
        }

        [Test]
        public void Tick_FullCycles_EventOrderCorrect()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();

            // Two full cycles plus a margin, in small steps. The margin lands us
            // mid-Dawn on day 3 so no event here depends on hitting a boundary
            // exactly — float accumulation over hundreds of ticks would make that
            // a coin flip. Exact-boundary behavior is covered on its own below.
            const int steps = 205;
            float total = (Dawn + Day + Night) * 2f + Dawn * 0.5f;
            for (int i = 0; i < steps; i++)
                clock.Tick(total / steps);

            Assert.That(events, Is.EqualTo(new[]
            {
                "Dawn1", "Day1", "Night1",
                "Dawn2", "Day2", "Night2",
                "Dawn3"
            }));
        }

        [Test]
        public void Tick_ExactBoundaryDt_TransitionsAndResetsElapsed()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            events.Clear();

            clock.Tick(Dawn);   // exactly the phase duration

            Assert.That(events, Is.EqualTo(new[] { "Day1" }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Day));
            Assert.That(clock.PhaseElapsed, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void Tick_HugeDt_CrossesMultipleBoundariesInOrder()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            events.Clear();

            // Dawn(2) + Day(5) + Night(3) + Dawn(2) + Day(5) = 17 in ONE tick.
            clock.Tick(Dawn + Day + Night + Dawn + Day);

            Assert.That(events, Is.EqualTo(new[]
            {
                "Day1", "Night1", "Dawn2", "Day2", "Night2"
            }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Night));
            Assert.That(clock.DayNumber, Is.EqualTo(2));
            Assert.That(clock.PhaseElapsed, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void Stop_TickIgnored_NoProgressNoEvents()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            clock.Tick(1f);
            events.Clear();

            clock.Stop();
            clock.Tick(100f);

            Assert.That(clock.IsRunning, Is.False);
            Assert.That(clock.PhaseElapsed, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void Resume_ContinuesFromStopPoint()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            clock.Tick(1f);          // 1 s into the 2 s dawn
            clock.Stop();
            clock.Tick(100f);        // swallowed
            events.Clear();

            clock.Resume();
            clock.Tick(1f);          // completes the dawn exactly

            Assert.That(clock.IsRunning, Is.True);
            Assert.That(events, Is.EqualTo(new[] { "Day1" }));
            Assert.That(clock.CurrentPhase, Is.EqualTo(DayPhase.Day));
        }

        [Test]
        public void PhaseRemainingAndProgress_MidPhaseValues()
        {
            DayNightClock clock = MakeClock();
            clock.Begin();

            clock.Tick(0.5f);   // 0.5 of 2 s dawn

            Assert.That(clock.PhaseTimeRemaining, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(clock.PhaseProgress01, Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void Tick_ZeroOrNegativeDt_DoesNothing()
        {
            DayNightClock clock = MakeClock();
            List<string> events = Record(clock);
            clock.Begin();
            events.Clear();

            clock.Tick(0f);
            clock.Tick(-5f);

            Assert.That(clock.PhaseElapsed, Is.EqualTo(0f));
            Assert.That(events, Is.Empty);
        }
    }
}
