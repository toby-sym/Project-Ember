using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectEmber.Simulation
{
    public sealed class TimeSimulationEngine : MonoBehaviour
    {
        [SerializeField] private float realSecondsPerGameMinute = 0.35f;
        [SerializeField] private Gradient ambientGradient;
        [SerializeField] private AnimationCurve lightIntensityCurve;
        [SerializeField] private Light2D globalLight;

        private float accumulator;

        public event Action OnTimeChanged;

        public int Minute { get; private set; }
        public int Hour { get; private set; } = 8;
        public int Day { get; private set; } = 1;
        public int Season { get; private set; }
        public float NormalizedDayTime => (Hour * 60f + Minute) / 1440f;

        public Light2D GlobalLight
        {
            get => globalLight;
            set
            {
                globalLight = value;
                ApplyAmbientLighting();
            }
        }

        private void Awake()
        {
            EnsureAmbientGradient();
            EnsureIntensityCurve();
        }

        private void Start()
        {
            ApplyAmbientLighting();
        }

        private void Update()
        {
            accumulator += Time.deltaTime;
            while (accumulator >= realSecondsPerGameMinute)
            {
                accumulator -= realSecondsPerGameMinute;
                AdvanceMinute();
            }
        }

        public void SetClock(int minute, int hour, int day, int season)
        {
            Minute = Mathf.Clamp(minute, 0, 59);
            Hour = Mathf.Clamp(hour, 0, 23);
            Day = Mathf.Max(1, day);
            Season = ((season % 4) + 4) % 4;

            ApplyAmbientLighting();
            OnTimeChanged?.Invoke();
        }

        private void AdvanceMinute()
        {
            Minute++;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour++;
            }

            if (Hour >= 24)
            {
                Hour = 0;
                Day++;
                Season = (Day / 28) % 4;
            }

            ApplyAmbientLighting();

            OnTimeChanged?.Invoke();
        }

        private void ApplyAmbientLighting()
        {
            EnsureAmbientGradient();
            EnsureIntensityCurve();

            var normalized = NormalizedDayTime;
            var ambientColor = ambientGradient.Evaluate(normalized);

            if (Camera.main != null)
            {
                Camera.main.backgroundColor = ambientColor;
            }

            if (globalLight != null)
            {
                globalLight.color = ambientColor;
                globalLight.intensity = Mathf.Max(0f, lightIntensityCurve.Evaluate(normalized));
            }
        }

        private void Reset()
        {
            EnsureAmbientGradient();
            EnsureIntensityCurve();
        }

        private void EnsureAmbientGradient()
        {
            if (ambientGradient != null)
            {
                return;
            }

            ambientGradient = new Gradient
            {
                colorKeys = new[]
                {
                    // Dark blue night, orange sunrise, bright afternoon, amber sunset, dark blue night.
                    new GradientColorKey(new Color(0.04f, 0.06f, 0.14f), 0f),
                    new GradientColorKey(new Color(0.95f, 0.45f, 0.16f), 0.25f),
                    new GradientColorKey(new Color(0.58f, 0.78f, 0.95f), 0.5f),
                    new GradientColorKey(new Color(0.9f, 0.36f, 0.12f), 0.75f),
                    new GradientColorKey(new Color(0.04f, 0.06f, 0.14f), 1f)
                },
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            };
        }

        private void EnsureIntensityCurve()
        {
            if (lightIntensityCurve != null && lightIntensityCurve.length > 0)
            {
                return;
            }

            // Dim at night (0.0 / 1.0), rising through sunrise, peaking at midday, dimming into sunset.
            lightIntensityCurve = new AnimationCurve(
                new Keyframe(0f, 0.18f),
                new Keyframe(0.25f, 0.65f),
                new Keyframe(0.5f, 1.15f),
                new Keyframe(0.75f, 0.6f),
                new Keyframe(1f, 0.18f));
        }
    }
}
