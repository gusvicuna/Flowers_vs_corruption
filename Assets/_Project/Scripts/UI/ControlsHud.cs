using FlowersVsCorruption.Core;
using UnityEngine;

namespace FlowersVsCorruption.UI
{
    // TEMP: control reference shown only during day 1; the real screens/UI
    // feature will replace it.
    public class ControlsHud : MonoBehaviour
    {
        [SerializeField] private TimeSystem _timeSystem;

        private void OnGUI()
        {
            if (_timeSystem == null || _timeSystem.DayNumber > 1)
                return;

            const float width = 360f;
            const float height = 84f;
            var box = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 12f, width, height);
            GUI.Box(box, GUIContent.none);

            var content = new Rect(box.x + 10f, box.y + 8f, box.width - 20f, box.height - 16f);
            GUILayout.BeginArea(content);
            GUILayout.Label("Move — A/D · arrows · left stick");
            GUILayout.Label("Action (plant / water / harvest) — S · down · gamepad south");
            GUILayout.Label("Change seed — Q/E · shoulder buttons");
            GUILayout.EndArea();
        }
    }
}
