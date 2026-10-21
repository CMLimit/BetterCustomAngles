using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class Controller : MonoBehaviour
    {
        public static Controller Instance { get; private set; }

        public AngleStore Store { get; private set; }
        public Canvas OverlayCanvas { get; private set; }
        public RectTransform OverlayRect { get; private set; }
        public TMP_FontAsset FontAsset { get; private set; }
        public UISprites Sprites { get; private set; }

        private ModConfig config;
        private GameObject overlayRoot;
        private scnEditor editor;
        private VanillaAngleUI vanillaUI;
        private AngleListWindow historyWindow;
        private AngleListWindow favoritesWindow;
        private PolygonWindow polygonWindow;
        private NoteWindow noteWindow;
        private UnityEngine.Font fallbackFont;
        private bool modEnabled = true;
        private bool firstEditor = true;
        private bool overlayVisible;
        private float nextWindowClamp;

        public void Initialize()
        {
            Instance = this;
            config = Main.Config;
            Sprites = new UISprites();
            Store = new AngleStore(Main.Mod.Path, config.historyLimit);
            CreateOverlay();
        }

        public void SetEnabled(bool value)
        {
            modEnabled = value;
            if (!value)
            {
                SetOverlayVisible(false);
                vanillaUI?.Cleanup();
                vanillaUI = null;
                editor = null;
            }
            else if (scnEditor.instance != null)
            {
                OnEditorChanged(scnEditor.instance);
            }
        }

        private void CreateOverlay()
        {
            overlayRoot = new GameObject(
                "BetterCustomAngles_Overlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(overlayRoot);

            OverlayCanvas = overlayRoot.GetComponent<Canvas>();
            OverlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            OverlayCanvas.sortingOrder = 32000;

            var scaler = overlayRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            OverlayRect = overlayRoot.GetComponent<RectTransform>();
            overlayRoot.SetActive(false);
        }

        private void Update()
        {
            if (!modEnabled)
            {
                return;
            }

            scnEditor current = scnEditor.instance;
            if (current == null)
            {
                if (editor != null)
                {
                    editor = null;
                    vanillaUI = null;
                    SetOverlayVisible(false);
                }
                return;
            }

            if (current != editor)
            {
                OnEditorChanged(current);
            }

            if (editor == null || editor.levelData == null)
            {
                SetOverlayVisible(false);
                return;
            }

            bool visible = !editor.levelData.isOldLevel && !editor.playMode;
            SetOverlayVisible(visible);

            if (vanillaUI != null)
            {
                vanillaUI.Refresh();
                vanillaUI.SetToolbarState(
                    polygonWindow != null && polygonWindow.IsOpen,
                    historyWindow != null && historyWindow.IsOpen,
                    favoritesWindow != null && favoritesWindow.IsOpen);
            }

            if (Store.Limit != config.historyLimit)
            {
                Store.SetLimit(config.historyLimit);
            }

            historyWindow?.RefreshIfDirty();
            favoritesWindow?.RefreshIfDirty();
            ClampOpenWindows();
        }

        private void OnEditorChanged(scnEditor current)
        {
            editor = current;
            try
            {
                FontAsset = ResolveFont(current);
                if (FontAsset == null)
                {
                    Main.Log("Could not resolve a usable font; UI may not render text");
                }

                vanillaUI = new VanillaAngleUI(this, current, FontAsset, Sprites);
                vanillaUI.Apply();

                EnsureWindows();

                if (firstEditor)
                {
                    firstEditor = false;
                    historyWindow.Frame.SetOpen(config.showHistoryOnStart);
                    favoritesWindow.Frame.SetOpen(config.showFavoritesOnStart);
                    polygonWindow.Frame.SetOpen(config.showPolygonOnStart);
                }
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
        }

        private void EnsureWindows()
        {
            if (historyWindow == null)
            {
                historyWindow = new AngleListWindow(
                    this,
                    "history",
                    I18n.Get("history.title"),
                    new Vector2(360f, 400f),
                    false,
                    new Vector2(config.historyX, config.historyY),
                    false);
            }

            if (favoritesWindow == null)
            {
                favoritesWindow = new AngleListWindow(
                    this,
                    "favorites",
                    I18n.Get("favorites.title"),
                    new Vector2(360f, 320f),
                    true,
                    new Vector2(config.favoritesX, config.favoritesY),
                    true);
            }

            if (polygonWindow == null)
            {
                polygonWindow = new PolygonWindow(
                    this,
                    new Vector2(config.polygonX, config.polygonY));
            }
        }

        private TMP_FontAsset ResolveFont(scnEditor current)
        {
            TMP_FontAsset font = null;
            try
            {
                if (current.floorButtonArbitrary != null && current.floorButtonArbitrary.textComponent != null)
                {
                    font = current.floorButtonArbitrary.textComponent.font;
                }
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }

            if (font == null)
            {
                font = TMP_Settings.defaultFontAsset;
            }

            if (font != null && font.HasCharacter('角'))
            {
                return font;
            }

            try
            {
                fallbackFont = UnityEngine.Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Noto Sans CJK SC", "Arial Unicode MS" },
                    32);
                if (fallbackFont != null)
                {
                    var created = TMP_FontAsset.CreateFontAsset(fallbackFont);
                    if (created != null)
                    {
                        created.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                        Main.Log("Using fallback CJK font: " + fallbackFont.name);
                        return created;
                    }
                }
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }

            return font;
        }

        private void SetOverlayVisible(bool visible)
        {
            if (overlayRoot == null || overlayVisible == visible)
            {
                return;
            }
            overlayVisible = visible;
            overlayRoot.SetActive(visible);
        }

        public void ToggleHistory()
        {
            EnsureWindows();
            historyWindow.Toggle();
        }

        public void ToggleFavorites()
        {
            EnsureWindows();
            favoritesWindow.Toggle();
        }

        public void TogglePolygon()
        {
            EnsureWindows();
            polygonWindow.Toggle();
        }

        public bool PlaceAngle(float angle, bool midspin)
        {
            scnEditor current = scnEditor.instance;
            if (current == null || current.levelData == null || current.levelData.isOldLevel)
            {
                return false;
            }
            if (current.selectedFloors == null || current.selectedFloors.Count != 1)
            {
                Main.Log("PlaceAngle ignored: select exactly one floor first");
                return false;
            }

            TMP_InputField field = current.floorButtonArbitrary;
            if (field == null)
            {
                return false;
            }

            field.text = AngleUtil.Format(angle);
            if (midspin)
            {
                current.CreateArbitraryMidspin();
            }
            else
            {
                current.CreateArbitraryFloor();
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            return true;
        }

        public static void HandleAnglePlaced(scnEditor placedEditor)
        {
            Controller controller = Instance;
            if (controller == null || !controller.modEnabled || placedEditor == null)
            {
                return;
            }
            if (placedEditor.levelData == null || placedEditor.levelData.isOldLevel)
            {
                return;
            }
            if (placedEditor.selectedFloors == null || placedEditor.selectedFloors.Count == 0)
            {
                return;
            }

            TMP_InputField field = placedEditor.floorButtonArbitrary;
            if (field == null || !AngleUtil.TryParse(field.text, out float value))
            {
                return;
            }

            value = AngleUtil.Round6(value);
            field.text = AngleUtil.Format(value);
            controller.Store.AddHistory(value);
        }

        public void CycleLanguage()
        {
            string next = I18n.CurrentLanguage.StartsWith("en") ? "zh-CN" : "en-US";
            config.language = next;
            I18n.SetLanguage(next);
            OnLanguageChanged();
            config.Save();
        }

        public void OpenNoteEditor(float angle)
        {
            if (noteWindow == null)
            {
                noteWindow = new NoteWindow(this, new Vector2(620f, -380f));
            }
            noteWindow.Open(angle);
        }

        public void OnLanguageChanged()
        {
            if (editor == null)
            {
                return;
            }

            bool historyOpen = historyWindow != null && historyWindow.IsOpen;
            bool favoritesOpen = favoritesWindow != null && favoritesWindow.IsOpen;
            bool polygonOpen = polygonWindow != null && polygonWindow.IsOpen;
            bool noteOpen = noteWindow != null && noteWindow.IsOpen;
            float noteAngle = noteWindow != null ? noteWindow.Angle : 0f;

            vanillaUI?.Cleanup();
            vanillaUI = null;

            if (overlayRoot != null)
            {
                UnityEngine.Object.Destroy(overlayRoot);
                overlayRoot = null;
            }
            overlayVisible = false;
            historyWindow = null;
            favoritesWindow = null;
            polygonWindow = null;
            noteWindow = null;

            CreateOverlay();
            OnEditorChanged(editor);

            historyWindow?.Frame.SetOpen(historyOpen);
            favoritesWindow?.Frame.SetOpen(favoritesOpen);
            polygonWindow?.Frame.SetOpen(polygonOpen);
            if (noteOpen)
            {
                OpenNoteEditor(noteAngle);
            }
        }

        public void ResetWindowPositions()
        {
            config.ResetWindowPositions();
            historyWindow?.Frame.SetPosition(new Vector2(config.historyX, config.historyY));
            favoritesWindow?.Frame.SetPosition(new Vector2(config.favoritesX, config.favoritesY));
            polygonWindow?.Frame.SetPosition(new Vector2(config.polygonX, config.polygonY));
            noteWindow?.Frame.SetPosition(new Vector2(620f, -380f));
            config.Save();
        }

        public void OnWindowMoved(FloatingWindow window, Vector2 position)
        {
            if (window == null)
            {
                return;
            }

            switch (window.Key)
            {
                case "history":
                    config.historyX = position.x;
                    config.historyY = position.y;
                    break;
                case "favorites":
                    config.favoritesX = position.x;
                    config.favoritesY = position.y;
                    break;
                case "polygon":
                    config.polygonX = position.x;
                    config.polygonY = position.y;
                    break;
            }
            config.Save();
        }

        private void ClampOpenWindows()
        {
            if (Time.unscaledTime < nextWindowClamp)
            {
                return;
            }
            nextWindowClamp = Time.unscaledTime + 0.5f;

            if (historyWindow != null && historyWindow.IsOpen)
            {
                historyWindow.Frame.ClampToScreen();
            }
            if (favoritesWindow != null && favoritesWindow.IsOpen)
            {
                favoritesWindow.Frame.ClampToScreen();
            }
            if (polygonWindow != null && polygonWindow.IsOpen)
            {
                polygonWindow.Frame.ClampToScreen();
            }
            if (noteWindow != null && noteWindow.IsOpen)
            {
                noteWindow.Frame.ClampToScreen();
            }
        }

        public void Shutdown()
        {
            try
            {
                Store?.Save();
                config?.Save();
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }

            if (overlayRoot != null)
            {
                UnityEngine.Object.Destroy(overlayRoot);
                overlayRoot = null;
            }
            if (Instance == this)
            {
                Instance = null;
            }
            UnityEngine.Object.Destroy(gameObject);
        }

        private void OnApplicationQuit()
        {
            Store?.Save();
            config?.Save();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Store?.Save();
            config?.Save();
        }
    }
}
