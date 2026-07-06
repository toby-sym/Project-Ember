using System;
using UnityEngine;

namespace ProjectEmber.Simulation
{
    public sealed class TimeSimulationEngine : MonoBehaviour
    {
        [SerializeField] private float realSecondsPerGameMinute = 0.35f;
        [SerializeField] private Gradient ambientGradient;

        private float accumulator;

        public event Action OnTimeChanged;

        public int Minute { get; private set; }
        public int Hour { get; private set; } = 8;
        public int Day { get; private set; } = 1;
        public int Season { get; private set; }
        public float NormalizedDayTime => (Hour * 60f + Minute) / 1440f;

        private void Awake()
        {
            EnsureAmbientGradient();
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

            if (Camera.main != null && ambientGradient != null)
            {
                Camera.main.backgroundColor = ambientGradient.Evaluate(NormalizedDayTime);
            }

            OnTimeChanged?.Invoke();
        }

        private void Reset()
        {
            EnsureAmbientGradient();
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
                    new GradientColorKey(new Color(0.04f, 0.06f, 0.14f), 0f),
                    new GradientColorKey(new Color(0.95f, 0.45f, 0.16f), 0.25f),
                    new GradientColorKey(new Color(0.58f, 0.78f, 0.95f), 0.5f),
                    new GradientColorKey(new Color(0.9f, 0.36f, 0.12f), 0.75f),
                    new GradientColorKey(new Color(0.04f, 0.06f, 0.14f), 1f)
                },
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            };
        }
    }
}
