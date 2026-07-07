using System;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    public static class ProceduralPixelArtGenerator
    {
        private const int DefaultResolution = 64;
        private const int DefaultItemResolution = 32;

        private static readonly Color HandleWood = new(0.46f, 0.25f, 0.1f);
        private static readonly Color HandleWoodDark = new(0.3f, 0.16f, 0.06f);
        private static readonly Color Silver = new(0.78f, 0.81f, 0.84f);
        private static readonly Color SilverDark = new(0.5f, 0.53f, 0.57f);
        private static readonly Color LogBrown = new(0.55f, 0.34f, 0.16f);
        private static readonly Color LogBrownDark = new(0.38f, 0.22f, 0.1f);
        private static readonly Color BerryRed = new(0.78f, 0.08f, 0.12f);
        private static readonly Color LeafGreen = new(0.2f, 0.5f, 0.2f);
        
        public static Texture2D GeneratePixelArtTexture(VectorLayer[] layers, int resolution = DefaultResolution)
        {
            return BuildTexture(resolution, (colors, res) =>
            {
                for (var i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    if (layer == null || !layer.CloseLoop)
                    {
                        continue;
                    }

                    var points = layer.GetRenderPoints();
                    if (points.Length < 3)
                    {
                        continue;
                    }

                    RasterizeLayer(points, layer.Color, colors, res, layer.SortingOrderWithinSprite);
                }
            });
        }
        
        public static Texture2D GenerateCharacterTexture(Color hairColor, Color clothingColor, Color skinTone, int seed, int resolution = DefaultResolution)
        {
            return BuildTexture(resolution, (colors, res) =>
                DrawPixelCharacter(colors, res, hairColor, clothingColor, skinTone, new System.Random(seed)));
        }
        
        public static Texture2D GenerateTreeTexture(Color barkColor, Color[] leafColors, int seed, int resolution = DefaultResolution)
        {
            return BuildTexture(resolution, (colors, res) =>
                DrawPixelTree(colors, res, barkColor, leafColors, new System.Random(seed)));
        }
        
        public static Texture2D GenerateTileTexture(Color baseColor, int seed, int resolution = DefaultResolution)
        {
            return BuildTexture(resolution, (colors, res) =>
                DrawPixelTile(colors, res, baseColor, new System.Random(seed)));
        }

        public static Texture2D GenerateItemTexture(ItemType itemType, int seed, int resolution = DefaultItemResolution)
        {
            return BuildTexture(resolution, (colors, res) =>
                DrawPixelItem(colors, res, itemType, new System.Random(seed)));
        }

        private static void DrawPixelItem(Color[] colors, int resolution, ItemType itemType, System.Random random)
        {
            switch (itemType)
            {
                case ItemType.Axe:
                    DrawPixelAxe(colors, resolution);
                    break;
                case ItemType.Pickaxe:
                    DrawPixelPickaxe(colors, resolution);
                    break;
                case ItemType.Sword:
                    DrawPixelSword(colors, resolution);
                    break;
                case ItemType.Logs:
                    DrawPixelLogs(colors, resolution);
                    break;
                case ItemType.Berries:
                    DrawPixelBerries(colors, resolution);
                    break;
                default:
                    DrawPixelLogs(colors, resolution);
                    break;
            }

            AddPixelNoise(colors, resolution, random, 0.05f);
        }

        private static void DrawPixelAxe(Color[] colors, int resolution)
        {
            var unit = resolution / 32f;
            // Brown handle running diagonally from bottom-left to upper area.
            DrawPixelRect(colors, resolution, R(13, unit), R(4, unit), R(3, unit), R(20, unit), HandleWood);
            DrawPixelRect(colors, resolution, R(13, unit), R(4, unit), R(1, unit), R(20, unit), HandleWoodDark);
            // Silver wedge head at the top.
            DrawPixelRect(colors, resolution, R(14, unit), R(22, unit), R(9, unit), R(6, unit), Silver);
            DrawPixelRect(colors, resolution, R(18, unit), R(20, unit), R(6, unit), R(3, unit), Silver);
            DrawPixelRect(colors, resolution, R(21, unit), R(22, unit), R(2, unit), R(6, unit), SilverDark);
        }

        private static void DrawPixelPickaxe(Color[] colors, int resolution)
        {
            var unit = resolution / 32f;
            // Brown handle (vertical).
            DrawPixelRect(colors, resolution, R(15, unit), R(3, unit), R(3, unit), R(22, unit), HandleWood);
            DrawPixelRect(colors, resolution, R(15, unit), R(3, unit), R(1, unit), R(22, unit), HandleWoodDark);
            // Silver curved head (bar tapering at both ends).
            DrawPixelRect(colors, resolution, R(6, unit), R(23, unit), R(20, unit), R(3, unit), Silver);
            DrawPixelRect(colors, resolution, R(4, unit), R(21, unit), R(4, unit), R(3, unit), SilverDark);
            DrawPixelRect(colors, resolution, R(24, unit), R(21, unit), R(4, unit), R(3, unit), SilverDark);
        }

        private static void DrawPixelSword(Color[] colors, int resolution)
        {
            var unit = resolution / 32f;
            // Silver blade (vertical).
            DrawPixelRect(colors, resolution, R(14, unit), R(11, unit), R(4, unit), R(17, unit), Silver);
            DrawPixelRect(colors, resolution, R(14, unit), R(11, unit), R(1, unit), R(17, unit), SilverDark);
            // Blade tip.
            DrawPixelRect(colors, resolution, R(15, unit), R(27, unit), R(2, unit), R(2, unit), Silver);
            // Cross guard.
            DrawPixelRect(colors, resolution, R(10, unit), R(9, unit), R(12, unit), R(2, unit), SilverDark);
            // Wooden grip and pommel.
            DrawPixelRect(colors, resolution, R(15, unit), R(3, unit), R(2, unit), R(6, unit), HandleWood);
            DrawPixelRect(colors, resolution, R(14, unit), R(2, unit), R(4, unit), R(2, unit), HandleWoodDark);
        }

        private static void DrawPixelLogs(Color[] colors, int resolution)
        {
            var unit = resolution / 32f;
            // Two stacked logs.
            DrawPixelRect(colors, resolution, R(5, unit), R(6, unit), R(22, unit), R(8, unit), LogBrown);
            DrawPixelRect(colors, resolution, R(8, unit), R(15, unit), R(22, unit), R(8, unit), LogBrownDark);
            // End-grain rings.
            DrawPixelCircle(colors, resolution, R(6, unit), R(10, unit), R(3, unit), LogBrownDark);
            DrawPixelCircle(colors, resolution, R(6, unit), R(10, unit), R(1, unit), LogBrown);
            DrawPixelCircle(colors, resolution, R(9, unit), R(19, unit), R(3, unit), LogBrown);
            DrawPixelCircle(colors, resolution, R(9, unit), R(19, unit), R(1, unit), LogBrownDark);
        }

        private static void DrawPixelBerries(Color[] colors, int resolution)
        {
            var unit = resolution / 32f;
            // Leaf.
            DrawPixelRect(colors, resolution, R(15, unit), R(20, unit), R(6, unit), R(3, unit), LeafGreen);
            // Cluster of berries.
            DrawPixelCircle(colors, resolution, R(12, unit), R(14, unit), R(4, unit), BerryRed);
            DrawPixelCircle(colors, resolution, R(20, unit), R(15, unit), R(4, unit), BerryRed);
            DrawPixelCircle(colors, resolution, R(16, unit), R(9, unit), R(4, unit), BerryRed);
        }

        private static int R(int value, float unit)
        {
            return Mathf.RoundToInt(value * unit);
        }

        private static Texture2D BuildTexture(int resolution, Action<Color[], int> draw)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var colors = new Color[resolution * resolution];
            draw(colors, resolution);

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
        
        private static void RasterizeLayer(Vector2[] points, Color color, Color[] colors, int resolution, int sortOrder)
        {
            // Scale points to texture space
            var scaledPoints = new Vector2[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                scaledPoints[i] = new Vector2(
                    (points[i].x + 1f) * 0.5f * resolution,
                    (points[i].y + 1f) * 0.5f * resolution);
            }
            
            // Simple scanline rasterization with dithering
            var bounds = GetBounds(scaledPoints);
            var minX = Mathf.Max(0, Mathf.FloorToInt(bounds.x));
            var maxX = Mathf.Min(resolution - 1, Mathf.CeilToInt(bounds.z));
            var minY = Mathf.Max(0, Mathf.FloorToInt(bounds.y));
            var maxY = Mathf.Min(resolution - 1, Mathf.CeilToInt(bounds.w));
            
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsPointInPolygon(point, scaledPoints))
                    {
                        var index = y * resolution + x;
                        colors[index] = ApplyDithering(color, x, y, resolution);
                    }
                }
            }
        }
        
        private static void DrawPixelCharacter(Color[] colors, int resolution, Color hair, Color clothing, Color skin, System.Random random)
        {
            var centerX = resolution / 2;
            var centerY = resolution / 2;
            
            // Draw body (torso)
            DrawPixelRect(colors, resolution, centerX - 4, centerY - 2, 8, 10, clothing);
            
            // Draw head
            DrawPixelCircle(colors, resolution, centerX, centerY + 8, 5, skin);
            
            // Draw hair
            DrawPixelHair(colors, resolution, centerX, centerY + 10, 6, hair, random);
            
            // Draw legs
            DrawPixelRect(colors, resolution, centerX - 3, centerY - 8, 2, 6, clothing);
            DrawPixelRect(colors, resolution, centerX + 1, centerY - 8, 2, 6, clothing);
            
            // Draw arms
            DrawPixelRect(colors, resolution, centerX - 6, centerY - 1, 2, 5, skin);
            DrawPixelRect(colors, resolution, centerX + 4, centerY - 1, 2, 5, skin);
            
            // Add pixel noise for texture
            AddPixelNoise(colors, resolution, random, 0.05f);
        }
        
        private static void DrawPixelTree(Color[] colors, int resolution, Color bark, Color[] leaves, System.Random random)
        {
            var centerX = resolution / 2;
            var centerY = resolution / 2;
            
            // Draw trunk
            DrawPixelRect(colors, resolution, centerX - 2, centerY - 8, 4, 12, bark);
            
            // Draw foliage clusters
            var leafClusters = new[]
            {
                new Vector2(centerX, centerY + 2),
                new Vector2(centerX - 4, centerY),
                new Vector2(centerX + 4, centerY),
                new Vector2(centerX - 2, centerY + 4),
                new Vector2(centerX + 2, centerY + 4)
            };
            
            foreach (var cluster in leafClusters)
            {
                var leafColor = leaves[random.Next(leaves.Length)];
                DrawPixelCircle(colors, resolution, (int)cluster.x, (int)cluster.y, 4 + random.Next(2), leafColor);
            }
            
            AddPixelNoise(colors, resolution, random, 0.08f);
        }
        
        private static void DrawPixelTile(Color[] colors, int resolution, Color baseColor, System.Random random)
        {
            // Fill with base color
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = ApplyDithering(baseColor, i % resolution, i / resolution, resolution);
            }
            
            // Add generic detail based on color
            if (baseColor.g > 0.3f && baseColor.g > baseColor.r)
            {
                // Greenish - grass details
                DrawPixelGrassDetails(colors, resolution, baseColor, random);
            }
            else if (baseColor.r > 0.3f && baseColor.r > baseColor.g)
            {
                // Reddish/brown - dirt details
                DrawPixelDirtDetails(colors, resolution, baseColor, random);
            }
            else if (baseColor.b > 0.3f && baseColor.b > baseColor.r)
            {
                // Blueish - water details
                DrawPixelWaterDetails(colors, resolution, baseColor, random);
            }
            else
            {
                // Grayish - stone details
                DrawPixelStoneDetails(colors, resolution, baseColor, random);
            }
            
            AddPixelNoise(colors, resolution, random, 0.1f);
        }
        
        private static void DrawPixelGrassDetails(Color[] colors, int resolution, Color baseColor, System.Random random)
        {
            var darkColor = baseColor * 0.8f;
            var lightColor = baseColor * 1.2f;
            
            for (var i = 0; i < resolution * resolution / 4; i++)
            {
                var x = random.Next(resolution);
                var y = random.Next(resolution);
                var index = y * resolution + x;
                
                if (random.NextDouble() > 0.5f)
                {
                    colors[index] = darkColor;
                }
                else
                {
                    colors[index] = lightColor;
                }
            }
        }
        
        private static void DrawPixelDirtDetails(Color[] colors, int resolution, Color baseColor, System.Random random)
        {
            var darkColor = baseColor * 0.7f;
            
            // Small rocks
            for (var i = 0; i < 5; i++)
            {
                var x = random.Next(resolution - 2);
                var y = random.Next(resolution - 2);
                DrawPixelRect(colors, resolution, x, y, 2, 2, darkColor);
            }
        }
        
        private static void DrawPixelWaterDetails(Color[] colors, int resolution, Color baseColor, System.Random random)
        {
            var lightColor = new Color(baseColor.r, baseColor.g, baseColor.b + 0.2f, baseColor.a);
            
            // Wave patterns
            for (var y = 0; y < resolution; y += 2)
            {
                for (var x = 0; x < resolution; x++)
                {
                    if ((x + y) % 4 == 0)
                    {
                        var index = y * resolution + x;
                        colors[index] = lightColor;
                    }
                }
            }
        }
        
        private static void DrawPixelStoneDetails(Color[] colors, int resolution, Color baseColor, System.Random random)
        {
            var darkColor = baseColor * 0.6f;
            var lightColor = baseColor * 1.3f;
            
            // Cracks and highlights
            for (var i = 0; i < 8; i++)
            {
                var x = random.Next(resolution);
                var y = random.Next(resolution);
                var index = y * resolution + x;
                
                colors[index] = random.NextDouble() > 0.5f ? darkColor : lightColor;
            }
        }
        
        private static void DrawPixelRect(Color[] colors, int resolution, int x, int y, int width, int height, Color color)
        {
            for (var dy = 0; dy < height; dy++)
            {
                for (var dx = 0; dx < width; dx++)
                {
                    var px = x + dx;
                    var py = y + dy;
                    if (px >= 0 && px < resolution && py >= 0 && py < resolution)
                    {
                        colors[py * resolution + px] = color;
                    }
                }
            }
        }
        
        private static void DrawPixelCircle(Color[] colors, int resolution, int centerX, int centerY, int radius, Color color)
        {
            var radiusSq = radius * radius;
            for (var dy = -radius; dy <= radius; dy++)
            {
                for (var dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radiusSq)
                    {
                        var px = centerX + dx;
                        var py = centerY + dy;
                        if (px >= 0 && px < resolution && py >= 0 && py < resolution)
                        {
                            colors[py * resolution + px] = color;
                        }
                    }
                }
            }
        }
        
        private static void DrawPixelHair(Color[] colors, int resolution, int centerX, int centerY, int radius, Color color, System.Random random)
        {
            var radiusSq = radius * radius;
            for (var dy = -radius; dy <= radius; dy++)
            {
                for (var dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radiusSq)
                    {
                        var px = centerX + dx;
                        var py = centerY + dy;
                        if (px >= 0 && px < resolution && py >= 0 && py < resolution)
                        {
                            // Add some variation
                            var variation = (float)(random.NextDouble() - 0.5) * 0.1f;
                            colors[py * resolution + px] = new Color(
                                Mathf.Clamp01(color.r + variation),
                                Mathf.Clamp01(color.g + variation),
                                Mathf.Clamp01(color.b + variation),
                                color.a);
                        }
                    }
                }
            }
        }
        
        private static void AddPixelNoise(Color[] colors, int resolution, System.Random random, float intensity)
        {
            for (var i = 0; i < colors.Length; i++)
            {
                if (colors[i].a > 0)
                {
                    var noise = (float)(random.NextDouble() - 0.5f) * intensity;
                    colors[i] = new Color(
                        Mathf.Clamp01(colors[i].r + noise),
                        Mathf.Clamp01(colors[i].g + noise),
                        Mathf.Clamp01(colors[i].b + noise),
                        colors[i].a);
                }
            }
        }
        
        private static Color ApplyDithering(Color color, int x, int y, int resolution)
        {
            // Bayer matrix dithering
            var bayerMatrix = new[,]
            {
                { 0, 8, 2, 10 },
                { 12, 4, 14, 6 },
                { 3, 11, 1, 9 },
                { 15, 7, 13, 5 }
            };
            
            var bayerValue = bayerMatrix[x % 4, y % 4] / 16f;
            var dither = (bayerValue - 0.5f) * 0.1f;
            
            return new Color(
                Mathf.Clamp01(color.r + dither),
                Mathf.Clamp01(color.g + dither),
                Mathf.Clamp01(color.b + dither),
                color.a);
        }
        
        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            var inside = false;
            var j = polygon.Length - 1;
            
            for (var i = 0; i < polygon.Length; i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            
            return inside;
        }
        
        private static Vector4 GetBounds(Vector2[] points)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            
            foreach (var point in points)
            {
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }
            
            return new Vector4(minX, minY, maxX, maxY);
        }
    }
}
