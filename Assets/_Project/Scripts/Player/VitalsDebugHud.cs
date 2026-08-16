// TEMP debug overlay — remove before release
using UnityEngine;

namespace FlowersVsCorruption.Player
{
    public class VitalsDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerVitalsController _vitalsController;

        private const int BarWidth  = 130;
        private const int BarHeight = 14;
        private const int Padding   = 8;
        private const int Spacing   = 4;

        private Texture2D _red;
        private Texture2D _orange;
        private Texture2D _dark;

        private void Awake()
        {
            _red    = MakeTex(new Color(0.85f, 0.15f, 0.15f));
            _orange = MakeTex(new Color(1f, 0.55f, 0.1f));
            _dark   = MakeTex(new Color(0.1f, 0.1f, 0.1f, 0.7f));
        }

        private void OnDestroy()
        {
            Destroy(_red);
            Destroy(_orange);
            Destroy(_dark);
        }

        private void OnGUI()
        {
            var vitals = _vitalsController?.PlayerVitals;
            if (vitals == null) return;

            int x       = Padding;
            int healthY = Screen.height - Padding - BarHeight;
            int hungerY = healthY - BarHeight - Spacing;

            DrawBar(x, hungerY, vitals.Hunger.Fraction01, _orange, $"Hunger {vitals.Hunger.Current:0}/{vitals.Hunger.Max:0}");
            DrawBar(x, healthY, vitals.Health.Fraction01, _red,    $"Health {vitals.Health.Current:0}/{vitals.Health.Max:0}");
        }

        private void DrawBar(int x, int y, float fraction, Texture2D fill, string label)
        {
            GUI.DrawTexture(new Rect(x, y, BarWidth, BarHeight), _dark);
            GUI.DrawTexture(new Rect(x, y, BarWidth * Mathf.Clamp01(fraction), BarHeight), fill);
            GUI.Label(new Rect(x + 2, y, BarWidth, BarHeight), label);
        }

        private static Texture2D MakeTex(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
    }
}
