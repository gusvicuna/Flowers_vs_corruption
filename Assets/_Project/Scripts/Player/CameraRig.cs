using FlowersVsCorruption.Core;
using FlowersVsCorruption.World;
using UnityEngine;

namespace FlowersVsCorruption.Player
{
    /// <summary>
    /// Follows the player around the ring, rotating so the player stays
    /// upright while the planet turns underneath. Only the angle is smoothed:
    /// the position derives from it, so the player never drifts off the
    /// vertical screen axis.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private WorldView _worldView;
        [SerializeField] private GameConfig _config;
        [SerializeField] private Camera _camera;

        private float _angle;
        private float _angleVelocity;
        private bool _initialized;

        private void LateUpdate()
        {
            if (!_initialized)
            {
                _angle = _player.AngleDegrees;
                _initialized = true;
            }
            else
            {
                _angle = Mathf.SmoothDampAngle(
                    _angle, _player.AngleDegrees, ref _angleVelocity, _config.CameraSmoothTime);
            }

            float radius = _config.PlanetRadius(_worldView.Grid.TileCount)
                + _config.TileHeight * 0.5f
                + _config.CameraVerticalOffset;

            Vector3 position = _worldView.transform.position + RingGeometry.PositionAt(_angle, radius);
            position.z = transform.position.z;

            transform.SetPositionAndRotation(position, RingGeometry.RotationAt(_angle));
            _camera.orthographicSize = _config.CameraOrthoSize;
        }
    }
}
