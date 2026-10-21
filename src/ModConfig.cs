using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BetterCustomAngles
{
    [Serializable]
    public class ModConfig
    {
        public string language = I18n.Auto;
        public int historyLimit = 50;
        public bool showHistoryOnStart = false;
        public bool showFavoritesOnStart = false;
        public bool showPolygonOnStart = false;

        public float historyX = 25f;
        public float historyY = -345f;
        public float favoritesX = -1000f;
        public float favoritesY = -60f;
        public float polygonX = 25f;
        public float polygonY = -25f;

        private static string FilePath => Path.Combine(Main.Mod.Path, "settings.json");

        public static ModConfig Load(string modPath)
        {
            try
            {
                string path = Path.Combine(modPath, "settings.json");
                if (File.Exists(path))
                {
                    var cfg = JsonUtility.FromJson<ModConfig>(File.ReadAllText(path));
                    if (cfg != null)
                    {
                        cfg.Normalize();
                        return cfg;
                    }
                }
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
            return new ModConfig();
        }

        public void Save()
        {
            try
            {
                Normalize();
                File.WriteAllText(FilePath, JsonUtility.ToJson(this, true));
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
        }

        private void Normalize()
        {
            historyLimit = Mathf.Clamp(historyLimit, 5, 200);
        }

        public void ResetWindowPositions()
        {
            historyX = 25f;
            historyY = -345f;
            favoritesX = -1000f;
            favoritesY = -60f;
            polygonX = 25f;
            polygonY = -25f;
        }

        public void Draw()
        {
            GUILayout.Label(I18n.Get("settings.title"), GUILayout.ExpandWidth(true));
            GUILayout.Space(4f);

            GUILayout.Label(I18n.Get("settings.language"));
            IReadOnlyList<string> codes = I18n.AvailableLanguages;
            int selected = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (string.Equals(codes[i], language, System.StringComparison.OrdinalIgnoreCase))
                {
                    selected = i;
                    break;
                }
            }

            var labels = new string[codes.Count];
            for (int i = 0; i < codes.Count; i++)
            {
                labels[i] = I18n.DisplayName(codes[i]);
            }

            int next = GUILayout.SelectionGrid(selected, labels, 3);
            if (next >= 0 && next < codes.Count && next != selected)
            {
                language = codes[next];
                I18n.SetLanguage(language);
                Save();
                Main.Controller?.OnLanguageChanged();
            }
            GUILayout.Label(I18n.Get("settings.language_hint"));

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUILayout.Label(I18n.Get("settings.history_limit"), GUILayout.Width(90f));
            string limitText = GUILayout.TextField(historyLimit.ToString(), GUILayout.Width(60f));
            int parsed;
            if (int.TryParse(limitText, out parsed))
            {
                historyLimit = Mathf.Clamp(parsed, 5, 200);
            }
            GUILayout.EndHorizontal();

            showHistoryOnStart = GUILayout.Toggle(showHistoryOnStart, I18n.Get("settings.open_history"));
            showFavoritesOnStart = GUILayout.Toggle(showFavoritesOnStart, I18n.Get("settings.open_favorites"));
            showPolygonOnStart = GUILayout.Toggle(showPolygonOnStart, I18n.Get("settings.open_polygon"));

            GUILayout.Space(6f);
            GUILayout.Label(I18n.Get("settings.note"));
            if (GUILayout.Button(I18n.Get("settings.reset_windows")))
            {
                ResetWindowPositions();
                Main.Controller?.ResetWindowPositions();
                Save();
            }
            if (GUILayout.Button(I18n.Get("settings.save")))
            {
                Save();
            }
        }

    }
}
