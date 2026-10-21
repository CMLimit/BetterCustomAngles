using UnityEngine;

namespace BetterCustomAngles
{
    public class UISprites
    {
        public static readonly Color Text = new Color32(242, 243, 245, 255);
        public static readonly Color SubText = new Color32(162, 168, 179, 255);
        public static readonly Color Accent = new Color32(74, 222, 128, 255);
        public static readonly Color Midspin = new Color32(183, 156, 255, 255);
        public static readonly Color Star = new Color32(255, 209, 102, 255);
        public static readonly Color DarkText = new Color32(17, 19, 22, 255);
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

        public readonly Sprite Card;
        public readonly Sprite Window;
        public readonly Sprite Input;
        public readonly Sprite ButtonDark;
        public readonly Sprite ButtonNormal;
        public readonly Sprite ButtonMidspin;
        public readonly Sprite Row;
        public readonly Sprite Circle;
        public readonly Sprite StarIcon;
        public readonly Sprite AngleIcon;

        public UISprites()
        {
            Card = Rounded("BCD_Card", new Color32(24, 25, 29, 242), new Color32(52, 55, 63, 255), 18, 1);
            Window = Rounded("BCD_Window", new Color32(20, 21, 25, 250), new Color32(59, 62, 70, 255), 16, 1);
            Input = Rounded("BCD_Input", new Color32(31, 33, 39, 255), new Color32(59, 62, 70, 255), 10, 1);
            ButtonDark = Rounded("BCD_ButtonDark", new Color32(39, 42, 49, 255), new Color32(59, 62, 70, 255), 10, 1);
            ButtonNormal = Rounded("BCD_ButtonNormal", new Color32(69, 214, 138, 255), new Color32(69, 214, 138, 255), 10, 1);
            ButtonMidspin = Rounded("BCD_ButtonMidspin", new Color32(183, 156, 255, 255), new Color32(183, 156, 255, 255), 10, 1);
            Row = Rounded("BCD_Row", new Color32(30, 32, 37, 255), new Color32(45, 48, 55, 255), 9, 1);
            Circle = CircleSprite("BCD_Circle", new Color32(30, 32, 38, 255), new Color32(74, 222, 128, 255), 2, 96);
            StarIcon = StarSprite("BCD_Star", Color.white, 96);
            AngleIcon = AngleSprite("BCD_Angle", Color.white, 96);
        }

        private static Sprite Rounded(string name, Color fill, Color border, int radius, int borderWidth, int size = 128)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name + "_Tex";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[size * size];
            float half = size * 0.5f - 1f;
            const int samples = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float aSum = 0f;
                    float rSum = 0f;
                    float gSum = 0f;
                    float bSum = 0f;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = x + (sx + 0.5f) / samples - size * 0.5f;
                            float py = y + (sy + 0.5f) / samples - size * 0.5f;
                            float dist = RoundedDistance(px, py, half, radius);
                            Color c = Color.clear;
                            if (dist <= -borderWidth)
                            {
                                c = fill;
                            }
                            else if (dist <= 0f)
                            {
                                c = border;
                            }
                            if (c.a > 0f)
                            {
                                rSum += c.r * c.a;
                                gSum += c.g * c.a;
                                bSum += c.b * c.a;
                                aSum += c.a;
                            }
                        }
                    }
                    float alpha = aSum / (samples * samples);
                    pixels[y * size + x] = alpha > 0f
                        ? new Color(rSum / aSum, gSum / aSum, bSum / aSum, alpha)
                        : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            float b = radius + borderWidth + 2f;
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(b, b, b, b));
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static Sprite CircleSprite(string name, Color fill, Color border, int borderWidth, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name + "_Tex";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[size * size];
            float radius = size * 0.5f - 1f;
            const int samples = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float aSum = 0f;
                    float rSum = 0f;
                    float gSum = 0f;
                    float bSum = 0f;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = x + (sx + 0.5f) / samples - size * 0.5f;
                            float py = y + (sy + 0.5f) / samples - size * 0.5f;
                            float dist = Mathf.Sqrt(px * px + py * py) - radius;
                            Color c = Color.clear;
                            if (dist <= -borderWidth)
                            {
                                c = fill;
                            }
                            else if (dist <= 0f)
                            {
                                c = border;
                            }
                            if (c.a > 0f)
                            {
                                rSum += c.r * c.a;
                                gSum += c.g * c.a;
                                bSum += c.b * c.a;
                                aSum += c.a;
                            }
                        }
                    }
                    float alpha = aSum / (samples * samples);
                    pixels[y * size + x] = alpha > 0f
                        ? new Color(rSum / aSum, gSum / aSum, bSum / aSum, alpha)
                        : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static float RoundedDistance(float px, float py, float half, float radius)
        {
            float qx = Mathf.Abs(px) - (half - radius);
            float qy = Mathf.Abs(py) - (half - radius);
            float ax = Mathf.Max(qx, 0f);
            float ay = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        private static Sprite AngleSprite(string name, Color color, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name + "_Tex";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Vector2 a = new Vector2(18f, 22f) * (size / 96f);
            Vector2 b = new Vector2(78f, 22f) * (size / 96f);
            Vector2 c = new Vector2(64f, 76f) * (size / 96f);
            float halfThickness = 5f * (size / 96f);

            var pixels = new Color[size * size];
            const int samples = 4;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int inside = 0;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            Vector2 p = new Vector2(x + (sx + 0.5f) / samples, y + (sy + 0.5f) / samples);
                            float d = Mathf.Min(DistanceToSegment(p, a, b), DistanceToSegment(p, a, c));
                            if (d <= halfThickness)
                            {
                                inside++;
                            }
                        }
                    }
                    float alpha = (float)inside / (samples * samples);
                    pixels[y * size + x] = alpha > 0f
                        ? new Color(color.r, color.g, color.b, alpha)
                        : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.0001f)
            {
                return Vector2.Distance(p, a);
            }
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
            return Vector2.Distance(p, a + ab * t);
        }

        private static Sprite StarSprite(string name, Color color, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name + "_Tex";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var points = new Vector2[10];
            float center = size * 0.5f;
            const float outerRadius = 0.47f;
            const float innerRadius = 0.20f;
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (-90f + i * 36f) * Mathf.Deg2Rad;
                float radius = (i % 2 == 0 ? outerRadius : innerRadius) * size;
                points[i] = new Vector2(center + Mathf.Cos(angle) * radius, center + Mathf.Sin(angle) * radius);
            }

            var pixels = new Color[size * size];
            const int samples = 4;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int inside = 0;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = x + (sx + 0.5f) / samples;
                            float py = y + (sy + 0.5f) / samples;
                            if (PointInPolygon(px, py, points))
                            {
                                inside++;
                            }
                        }
                    }
                    float alpha = (float)inside / (samples * samples);
                    pixels[y * size + x] = alpha > 0f
                        ? new Color(color.r, color.g, color.b, alpha)
                        : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static bool PointInPolygon(float x, float y, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 pi = polygon[i];
                Vector2 pj = polygon[j];
                if (((pi.y > y) != (pj.y > y)) &&
                    (x < (pj.x - pi.x) * (y - pi.y) / (pj.y - pi.y) + pi.x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
