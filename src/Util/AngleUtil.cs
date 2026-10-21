using System;
using System.Globalization;

namespace BetterCustomAngles
{
    public static class AngleUtil
    {
        public const string PolygonNotePrefix = "@polygon:";

        public static string FormatNote(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return string.Empty;
            }
            if (raw.StartsWith(PolygonNotePrefix))
            {
                if (int.TryParse(raw.Substring(PolygonNotePrefix.Length), out int sides))
                {
                    return I18n.Format("note.polygon", sides);
                }
            }
            return raw;
        }

        public static float Round6(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0f;
            }

            double rounded = Math.Round((double)value, 6, MidpointRounding.AwayFromZero);
            if (Math.Abs(rounded) < 0.0000005)
            {
                return 0f;
            }
            return (float)rounded;
        }

        public static string Format(float value)
        {
            double rounded = Math.Round((double)Round6(value), 6, MidpointRounding.AwayFromZero);
            if (Math.Abs(rounded) < 0.0000005)
            {
                rounded = 0.0;
            }
            return rounded.ToString("0.######", CultureInfo.InvariantCulture);
        }

        public static string Display(float value)
        {
            return Format(value) + "°";
        }

        public static bool TryParse(string text, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return true;
            }
            return false;
        }

        public static long Key(float value)
        {
            return (long)Math.Round((double)Round6(value) * 1000000.0, MidpointRounding.AwayFromZero);
        }
    }
}
