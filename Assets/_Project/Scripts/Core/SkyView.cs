using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FlowersVsCorruption.Core
{
    public class SkyView : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private TimeSystem _timeSystem;
        [SerializeField] private Camera _camera;
        [SerializeField] private Light2D _sunLight;

        private void Update()
        {
            float t = 1 - Mathf.Exp(-Time.deltaTime / _config.SkyTransitionSeconds);
            // Update the camera background color based on the current phase
            switch (_timeSystem.CurrentPhase)
            {
                case DayPhase.Dawn:
                    _camera.backgroundColor = Color.Lerp(_camera.backgroundColor, _config.DawnSkyColor, t);
                    _sunLight.color = Color.Lerp(_sunLight.color, _config.DawnLightColor, t);
                    break;
                case DayPhase.Day:
                    _camera.backgroundColor = Color.Lerp(_camera.backgroundColor, _config.DaySkyColor, t);
                    _sunLight.color = Color.Lerp(_sunLight.color, _config.DayLightColor, t);
                    break;
                case DayPhase.Night:
                    _camera.backgroundColor = Color.Lerp(_camera.backgroundColor, _config.NightSkyColor, t);
                    _sunLight.color = Color.Lerp(_sunLight.color, _config.NightLightColor, t);
                    break;
            }
        }
    }
}
