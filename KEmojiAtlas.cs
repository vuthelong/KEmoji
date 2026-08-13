#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Kingfisher.KEmoji
{
    internal static class KEmojiAtlas
    {
        #region Field

        private const int MinTextureSize = 1;
        private const int NoDepthBuffer = 0;
        private const float MinAlpha = .0001f;

        #endregion

        #region Atlas

        public static Result Build(List<KEmojiData.Entry> entries, int cellSize)
        {
            var columns = Mathf.CeilToInt(Mathf.Sqrt(entries.Count));
            var rows = Mathf.CeilToInt((float)entries.Count / columns);

            var result = new Result
            {
                Width = columns * cellSize,
                Height = rows * cellSize,
            };

            var texture = new Texture2D(result.Width, result.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

            try
            {
                ClearTransparent(texture);

                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var cell = BuildCell(entry, cellSize);

                    try
                    {
                        var x = i % columns * cellSize;
                        var y = result.Height - (i / columns + 1) * cellSize;

                        texture.SetPixels(x, y, cellSize, cellSize, cell.GetPixels());

                        result.Placements.Add(new Placement
                        {
                            Name = string.IsNullOrEmpty(entry.spriteName) ? entry.sprite.name : entry.spriteName,
                            X = x,
                            Y = y,
                        });
                    }
                    finally
                    {
                        Object.DestroyImmediate(cell);
                    }
                }

                texture.Apply();

                result.Texture = texture;

                return result;
            }
            catch
            {
                Object.DestroyImmediate(texture);

                throw;
            }
        }

        private static void ClearTransparent(Texture2D texture) => texture.SetPixels32(new Color32[texture.width * texture.height]);

        #endregion

        #region Cell

        private static Texture2D BuildCell(KEmojiData.Entry entry, int cellSize)
        {
            var source = ReadSprite(entry.sprite);
            Texture2D scaled = null;

            try
            {
                var width = Mathf.Max(MinTextureSize, Mathf.RoundToInt(source.width * entry.sizeRatio));
                var height = Mathf.Max(MinTextureSize, Mathf.RoundToInt(source.height * entry.sizeRatio));

                scaled = Scale(source, width, height);

                return Compose(scaled, cellSize);
            }
            finally
            {
                Object.DestroyImmediate(source);

                if (scaled) Object.DestroyImmediate(scaled);
            }
        }

        private static Texture2D ReadSprite(Sprite sprite)
        {
            var spriteRect = sprite.textureRect;
            var renderTexture = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, NoDepthBuffer, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previousActive = RenderTexture.active;
            Texture2D readable = null;

            try
            {
                Graphics.Blit(sprite.texture, renderTexture);

                RenderTexture.active = renderTexture;

                readable = new Texture2D((int)spriteRect.width, (int)spriteRect.height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };

                readable.ReadPixels(spriteRect, 0, 0);
                readable.Apply();

                return readable;
            }
            catch
            {
                if (readable) Object.DestroyImmediate(readable);

                throw;
            }
            finally
            {
                RenderTexture.active = previousActive;

                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private static Texture2D Compose(Texture2D scaled, int cellSize)
        {
            var cell = new Texture2D(cellSize, cellSize, TextureFormat.RGBA32, false);

            ClearTransparent(cell);

            var copyWidth = Mathf.Min(scaled.width, cellSize);
            var copyHeight = Mathf.Min(scaled.height, cellSize);

            var pixels = scaled.GetPixels((scaled.width - copyWidth) / 2, (scaled.height - copyHeight) / 2, copyWidth, copyHeight);

            cell.SetPixels((cellSize - copyWidth) / 2, (cellSize - copyHeight) / 2, copyWidth, copyHeight, pixels);
            cell.Apply();

            return cell;
        }

        #endregion

        #region Scaling

        private static Texture2D Scale(Texture2D source, int width, int height)
        {
            var sourceWidth = source.width;
            var sourceHeight = source.height;
            var sourcePixels = source.GetPixels();

            var scaleX = (float)sourceWidth / width;
            var scaleY = (float)sourceHeight / height;

            var pixels = new Color[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    pixels[y * width + x] = Sample(x, y);
                }
            }

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };

            result.SetPixels(pixels);
            result.Apply();

            return result;

            Color Sample(int x, int y)
            {
                var left = x * scaleX;
                var right = (x + 1) * scaleX;
                var bottom = y * scaleY;
                var top = (y + 1) * scaleY;

                var firstX = Mathf.Max(0, Mathf.FloorToInt(left));
                var lastX = Mathf.Min(sourceWidth - 1, Mathf.CeilToInt(right) - 1);
                var firstY = Mathf.Max(0, Mathf.FloorToInt(bottom));
                var lastY = Mathf.Min(sourceHeight - 1, Mathf.CeilToInt(top) - 1);

                var red = 0f;
                var green = 0f;
                var blue = 0f;
                var alpha = 0f;
                var totalWeight = 0f;

                for (var sampleY = firstY; sampleY <= lastY; sampleY++)
                {
                    var weightY = OverlapWeight(sampleY, bottom, top);

                    for (var sampleX = firstX; sampleX <= lastX; sampleX++)
                    {
                        var weight = OverlapWeight(sampleX, left, right) * weightY;

                        if (weight <= 0) continue;

                        var pixel = sourcePixels[sampleY * sourceWidth + sampleX];

                        red += pixel.r * pixel.a * weight;
                        green += pixel.g * pixel.a * weight;
                        blue += pixel.b * pixel.a * weight;
                        alpha += pixel.a * weight;

                        totalWeight += weight;
                    }
                }

                if (totalWeight <= 0 || alpha <= MinAlpha) return Color.clear;

                var averageAlpha = alpha / totalWeight;

                return new Color(red / totalWeight / averageAlpha, green / totalWeight / averageAlpha, blue / totalWeight / averageAlpha, averageAlpha);
            }
        }

        private static float OverlapWeight(int texelIndex, float rangeStart, float rangeEnd) => Mathf.Max(0, Mathf.Min(texelIndex + 1f, rangeEnd) - Mathf.Max(texelIndex, rangeStart));

        #endregion

        #region Nested Type

        public class Result
        {
            public Texture2D Texture;

            public int Width;
            public int Height;

            public readonly List<Placement> Placements = new();
        }

        public class Placement
        {
            public string Name;

            public int X;
            public int Y;
        }

        #endregion
    }
}
#endif
