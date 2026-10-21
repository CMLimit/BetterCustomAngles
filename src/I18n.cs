using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace BetterCustomAngles
{
    public static class I18n
    {
        public const string Auto = "auto";

        private static readonly Dictionary<string, Dictionary<string, string>> Tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<string> LanguageCodes = new List<string>();
        private static Dictionary<string, string> current;
        private static string modPath;

        public static string CurrentLanguage { get; private set; } = "zh-CN";
        public static IReadOnlyList<string> AvailableLanguages => LanguageCodes;

        public static event Action LanguageChanged;

        public static void Initialize(string path, string configured)
        {
            modPath = path;
            Tables.Clear();
            LanguageCodes.Clear();

            AddBuiltIn("zh-CN", Chinese());
            AddBuiltIn("en-US", English());
            LoadExternal();
            SetLanguage(configured, false);
        }

        public static void SetLanguage(string code, bool notify = true)
        {
            string resolved = Resolve(code);
            if (!Tables.TryGetValue(resolved, out Dictionary<string, string> table))
            {
                resolved = Tables.ContainsKey("en-US") ? "en-US" : "zh-CN";
                table = Tables[resolved];
            }

            current = table;
            CurrentLanguage = resolved;
            if (notify)
            {
                LanguageChanged?.Invoke();
            }
        }

        public static string Get(string key)
        {
            if (current != null && current.TryGetValue(key, out string value))
            {
                return value;
            }
            if (Tables.TryGetValue("en-US", out Dictionary<string, string> en) && en.TryGetValue(key, out value))
            {
                return value;
            }
            if (Tables.TryGetValue("zh-CN", out Dictionary<string, string> zh) && zh.TryGetValue(key, out value))
            {
                return value;
            }
            return key;
        }

        public static string Format(string key, params object[] args)
        {
            try
            {
                return string.Format(Get(key), args);
            }
            catch
            {
                return Get(key);
            }
        }

        public static bool IsEnglish
        {
            get { return CurrentLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase); }
        }

        public static string DisplayName(string code)
        {
            if (string.IsNullOrEmpty(code) || string.Equals(code, Auto, StringComparison.OrdinalIgnoreCase))
            {
                return Get("language.auto");
            }

            if (Tables.TryGetValue(code, out Dictionary<string, string> table) &&
                table.TryGetValue("language.name", out string name) &&
                !string.IsNullOrEmpty(name))
            {
                return name;
            }

            return code;
        }

        private static string Resolve(string code)
        {
            if (string.IsNullOrEmpty(code) || string.Equals(code, Auto, StringComparison.OrdinalIgnoreCase))
            {
                return DetectSystemLanguage();
            }

            foreach (string key in Tables.Keys)
            {
                if (string.Equals(key, code, StringComparison.OrdinalIgnoreCase))
                {
                    return key;
                }
            }

            return DetectSystemLanguage();
        }

        private static string DetectSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return "zh-CN";
                default:
                    return "en-US";
            }
        }

        private static void AddBuiltIn(string code, Dictionary<string, string> table)
        {
            Tables[code] = table;
            if (!LanguageCodes.Contains(code))
            {
                LanguageCodes.Add(code);
            }
            if (!LanguageCodes.Contains(Auto))
            {
                LanguageCodes.Insert(0, Auto);
            }
        }

        private static void LoadExternal()
        {
            if (string.IsNullOrEmpty(modPath))
            {
                return;
            }

            try
            {
                string dir = Path.Combine(modPath, "Lang");
                if (!Directory.Exists(dir))
                {
                    return;
                }

                foreach (string file in Directory.GetFiles(dir, "*.json"))
                {
                    string code = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(code) ||
                        string.Equals(code, Auto, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
                        if (dict == null || dict.Count == 0)
                        {
                            continue;
                        }

                        string canonical = null;
                        foreach (string existing in Tables.Keys)
                        {
                            if (string.Equals(existing, code, StringComparison.OrdinalIgnoreCase))
                            {
                                canonical = existing;
                                break;
                            }
                        }

                        if (canonical == null)
                        {
                            canonical = code;
                            Tables[canonical] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            if (!LanguageCodes.Contains(canonical))
                            {
                                LanguageCodes.Add(canonical);
                            }
                        }

                        foreach (KeyValuePair<string, string> pair in dict)
                        {
                            Tables[canonical][pair.Key] = pair.Value;
                        }

                        Main.Log("Loaded language: " + canonical + " (" + file + ")");
                    }
                    catch (Exception e)
                    {
                        Main.LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
        }

        private static Dictionary<string, string> Chinese()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "language.name", "中文" },
                { "language.auto", "自动" },
                { "mod.name", "更好的自定义角度" },
                { "common.normal", "普通" },
                { "common.midspin", "中旋" },
                { "common.polygon", "多边形" },
                { "common.history", "历史" },
                { "common.favorites", "收藏" },
                { "common.reset", "复位" },
                { "common.save", "保存" },
                { "common.cancel", "取消" },
                { "common.delete", "删除" },
                { "custom.title", "自定义角度" },
                { "custom.placeholder", "角度或公式，如 1*180+26" },
                { "toggle.custom", "当前：自定义角度" },
                { "toggle.default", "当前：八方向角度" },
                { "history.title", "角度历史" },
                { "history.empty", "暂无历史角度" },
                { "favorites.title", "角度收藏" },
                { "favorites.empty", "暂无收藏角度" },
                { "polygon.title", "正多边形计算器" },
                { "polygon.sides", "边数 n" },
                { "polygon.invalid", "请输入 3 ~ 360" },
                { "polygon.place_normal", "普通放置" },
                { "polygon.place_midspin", "中旋放置" },
                { "note.polygon", "{0}边形" },
                { "note.add", "添加备注" },
                { "note.title", "角度备注" },
                { "note.placeholder", "输入备注" },
                { "note.hint", "点击角度下方的小字可编辑备注" },
                { "settings.title", "更好的自定义角度" },
                { "settings.language", "语言" },
                { "settings.history_limit", "历史上限" },
                { "settings.open_history", "进入编辑器时打开历史窗口" },
                { "settings.open_favorites", "进入编辑器时打开收藏窗口" },
                { "settings.open_polygon", "进入编辑器时打开多边形窗口" },
                { "settings.note", "修改历史上限后会立即裁剪旧记录。" },
                { "settings.reset_windows", "重置窗口位置" },
                { "settings.save", "保存设置" },
                { "settings.language_hint", "在 Lang 文件夹新建 xx-XX.json 即可添加语言（需重启游戏）" },
            };
        }

        private static Dictionary<string, string> English()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "language.name", "English" },
                { "language.auto", "Auto" },
                { "mod.name", "BetterCustomAngles" },
                { "common.normal", "Normal" },
                { "common.midspin", "Midspin" },
                { "common.polygon", "Polygon" },
                { "common.history", "History" },
                { "common.favorites", "Favorites" },
                { "common.reset", "Reset" },
                { "common.save", "Save" },
                { "common.cancel", "Cancel" },
                { "common.delete", "Delete" },
                { "custom.title", "Custom Angle" },
                { "custom.placeholder", "Angle or formula, e.g. 1*180+26" },
                { "toggle.custom", "Current: custom angle" },
                { "toggle.default", "Current: 8-direction" },
                { "history.title", "Angle History" },
                { "history.empty", "No angle history" },
                { "favorites.title", "Angle Favorites" },
                { "favorites.empty", "No favorite angles" },
                { "polygon.title", "Polygon Calculator" },
                { "polygon.sides", "Sides n" },
                { "polygon.invalid", "Enter 3 - 360" },
                { "polygon.place_normal", "Place Normal" },
                { "polygon.place_midspin", "Place Midspin" },
                { "note.polygon", "{0} polygon" },
                { "note.add", "Add note" },
                { "note.title", "Angle Note" },
                { "note.placeholder", "Enter a note" },
                { "note.hint", "Click the small text below an angle to edit its note" },
                { "settings.title", "BetterCustomAngles" },
                { "settings.language", "Language" },
                { "settings.history_limit", "History limit" },
                { "settings.open_history", "Open history window on editor start" },
                { "settings.open_favorites", "Open favorites window on editor start" },
                { "settings.open_polygon", "Open polygon window on editor start" },
                { "settings.note", "Lowering the limit trims old history immediately." },
                { "settings.reset_windows", "Reset window positions" },
                { "settings.save", "Save settings" },
                { "settings.language_hint", "Add Lang/xx-XX.json to add a language (restart the game)" },
            };
        }
    }
}
