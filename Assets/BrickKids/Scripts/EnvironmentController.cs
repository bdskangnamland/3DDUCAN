using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public class EnvironmentController : MonoBehaviour
    {
        public Camera worldCamera;
        public Camera backgroundCamera;
        public InfiniteWorldSurface worldSurface;
        public Light sun;
        public Light fill;

        // 0 = Auto, 1 = Dark, 2 = Neutral, 3 = Warm, 4 = Cool, 5 = Green, 6 = Light
        public int ThemeIndex { get; private set; }

        private int lastResolvedAuto = -1;
        private float nextAutoCheck;

        private const string ThemeKey = "BrickKids_EnvironmentTheme";

        private void Start()
        {
            ThemeIndex = PlayerPrefs.GetInt(ThemeKey, 0);
            ApplyTheme(ThemeIndex, true);
        }

        private void Update()
        {
            if (ThemeIndex != 0) return;
            if (Time.unscaledTime < nextAutoCheck) return;

            nextAutoCheck = Time.unscaledTime + 1.25f;
            int resolved = ResolveAutoTheme();

            if (resolved != lastResolvedAuto)
            {
                lastResolvedAuto = resolved;
                ApplyResolvedTheme(resolved);
            }
        }

        public void SetTheme(int index)
        {
            ThemeIndex = Mathf.Clamp(index, 0, 6);
            PlayerPrefs.SetInt(ThemeKey, ThemeIndex);
            PlayerPrefs.Save();

            ApplyTheme(ThemeIndex, true);
        }

        public static Color ThemeGroundColor(int index)
        {
            switch (index)
            {
                case 1: return new Color(0.18f, 0.20f, 0.23f);
                case 2: return new Color(0.49f, 0.54f, 0.58f);
                case 3: return new Color(0.55f, 0.48f, 0.39f);
                case 4: return new Color(0.34f, 0.46f, 0.56f);
                case 5: return new Color(0.32f, 0.48f, 0.37f);
                case 6: return new Color(0.70f, 0.75f, 0.79f);
                default: return new Color(0.49f, 0.54f, 0.58f);
            }
        }

        private void ApplyTheme(int index, bool force)
        {
            if (index == 0)
            {
                int resolved = ResolveAutoTheme();
                lastResolvedAuto = resolved;
                ApplyResolvedTheme(resolved);
            }
            else
            {
                lastResolvedAuto = -1;
                ApplyResolvedTheme(index);
            }
        }

        private int ResolveAutoTheme()
        {
            float brightness = Screen.brightness;

            // Some devices can report an unusable value; neutral is the safe fallback.
            if (brightness < 0f || brightness > 1f)
                brightness = 0.55f;

            if (brightness <= 0.28f) return 1; // Dark room
            if (brightness <= 0.52f) return 4; // Cool / easy on eyes
            if (brightness <= 0.78f) return 2; // Neutral
            return 6;                          // Bright environment
        }

        private void ApplyResolvedTheme(int index)
        {
            Color ground;
            Color workspace;
            Color outer;
            Color ambient;
            float sunIntensity;
            float fillIntensity;

            switch (index)
            {
                case 1: // Dark
                    ground = new Color(0.18f, 0.20f, 0.23f);
                    workspace = new Color(0.075f, 0.09f, 0.11f);
                    outer = new Color(0.035f, 0.045f, 0.060f);
                    ambient = new Color(0.48f, 0.52f, 0.58f);
                    sunIntensity = 0.95f;
                    fillIntensity = 0.26f;
                    break;

                case 3: // Warm
                    ground = new Color(0.55f, 0.48f, 0.39f);
                    workspace = new Color(0.30f, 0.26f, 0.21f);
                    outer = new Color(0.12f, 0.10f, 0.085f);
                    ambient = new Color(0.64f, 0.57f, 0.48f);
                    sunIntensity = 1.08f;
                    fillIntensity = 0.32f;
                    break;

                case 4: // Cool
                    ground = new Color(0.34f, 0.46f, 0.56f);
                    workspace = new Color(0.15f, 0.22f, 0.28f);
                    outer = new Color(0.055f, 0.075f, 0.095f);
                    ambient = new Color(0.52f, 0.61f, 0.69f);
                    sunIntensity = 1.02f;
                    fillIntensity = 0.30f;
                    break;

                case 5: // Green / landscape
                    ground = new Color(0.32f, 0.48f, 0.37f);
                    workspace = new Color(0.15f, 0.24f, 0.18f);
                    outer = new Color(0.055f, 0.085f, 0.065f);
                    ambient = new Color(0.50f, 0.60f, 0.52f);
                    sunIntensity = 1.04f;
                    fillIntensity = 0.30f;
                    break;

                case 6: // Light
                    ground = new Color(0.70f, 0.75f, 0.79f);
                    workspace = new Color(0.56f, 0.64f, 0.70f);
                    outer = new Color(0.20f, 0.24f, 0.29f);
                    ambient = new Color(0.66f, 0.69f, 0.73f);
                    sunIntensity = 1.12f;
                    fillIntensity = 0.34f;
                    break;

                default: // Neutral
                    ground = new Color(0.49f, 0.54f, 0.58f);
                    workspace = new Color(0.25f, 0.30f, 0.35f);
                    outer = new Color(0.085f, 0.105f, 0.13f);
                    ambient = new Color(0.58f, 0.61f, 0.65f);
                    sunIntensity = 1.06f;
                    fillIntensity = 0.31f;
                    break;
            }

            if (worldSurface != null)
                worldSurface.SetGroundColor(ground);

            if (worldCamera != null)
                worldCamera.backgroundColor = workspace;

            if (backgroundCamera != null)
                backgroundCamera.backgroundColor = outer;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;

            if (sun != null) sun.intensity = sunIntensity;
            if (fill != null) fill.intensity = fillIntensity;
        }
    }
}
