using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using UnityEngine;

namespace BetterCustomAngles
{
    [EnableReloading]
    public static class Main
    {
        public static UnityModManager.ModEntry Mod { get; private set; }
        public static Harmony Harmony { get; private set; }
        public static ModConfig Config { get; private set; }
        public static Controller Controller { get; private set; }
        public static bool Enabled { get; private set; }

        private static bool patched;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Mod = modEntry;
            Config = ModConfig.Load(modEntry.Path);
            I18n.Initialize(modEntry.Path, Config.language);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;

            Harmony = new Harmony(modEntry.Info.Id);

            var go = new GameObject("BetterCustomAngles_Root");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Controller = go.AddComponent<Controller>();
            Controller.Initialize();

            Log("Loaded v" + modEntry.Info.Version + " (" + I18n.CurrentLanguage + ")");
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry entry, bool value)
        {
            Enabled = value;
            try
            {
                if (value)
                {
                    if (!patched)
                    {
                        Harmony.PatchAll(Assembly.GetExecutingAssembly());
                        patched = true;
                    }
                    Controller?.SetEnabled(true);
                    Log("Enabled");
                }
                else
                {
                    if (patched)
                    {
                        Harmony.UnpatchAll(entry.Info.Id);
                        patched = false;
                    }
                    Controller?.SetEnabled(false);
                    Log("Disabled");
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry entry)
        {
            Config?.Draw();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry entry)
        {
            Config?.Save();
        }

        private static bool OnUnload(UnityModManager.ModEntry entry)
        {
            try
            {
                if (patched)
                {
                    Harmony.UnpatchAll(entry.Info.Id);
                    patched = false;
                }
                Controller?.Shutdown();
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return true;
        }

        public static void Log(string message)
        {
            Mod?.Logger?.Log(message);
        }

        public static void LogException(Exception e)
        {
            Mod?.Logger?.LogException(e);
        }
    }
}
