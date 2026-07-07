using System;
using UnityEngine;

namespace ProjectEmber.Shared
{
    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static Color NextColor(this Random random, float min, float max)
        {
            return new Color(
                random.NextFloat(min, max),
                random.NextFloat(min, max),
                random.NextFloat(min, max));
        }
    }
}
