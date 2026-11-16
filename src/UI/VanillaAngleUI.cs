using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class VanillaAngleUI
    {
        private readonly Controller controller;
        private readonly scnEditor editor;
        private readonly TMP_FontAsset font;
        private readonly UISprites sprites;
        private readonly Transform root;

        private TextMeshProUGUI polygonLabel;
        private TextMeshProUGUI historyLabel;
        private TextMeshProUGUI favoritesLabel;
        private TextMeshProUGUI sizeLabel;
        private TMP_InputField inputRef;
        private TMP_Text placeholderRef;
        private Image toggleAngleIcon;
        private TextMeshProUGUI toggleEightLabel;
        private bool toggleStyled;
        private float nextRefresh;
        private float scale = 1f;

        private static string PlaceholderText => I18n.Get("custom.placeholder");

        public VanillaAngleUI(Controller controller, scnEditor editor, TMP_FontAsset font, UISprites sprites, float scale)
        {
            this.controller = controller;
            this.editor = editor;
            this.font = font;
            this.sprites = sprites;
            this.scale = Mathf.Clamp(scale, ModConfig.MinScale, ModConfig.MaxScale);
            root = editor.floorButtonArbitraryContainer != null
                ? editor.floorButtonArbitraryContainer.transform
                : null;
        }

        private float S(float value)
        {
            return value * scale;
        }

        public void SetScale(float value)
        {
            value = Mathf.Clamp(value, ModConfig.MinScale, ModConfig.MaxScale);
            if (Mathf.Abs(value - scale) < 0.0001f)
            {
                return;
            }

            scale = value;
            Cleanup();
            Apply();
        }

        public void Apply()
        {
            if (root == null)
            {
                Main.Log("Custom angle container not found");
                return;
            }

            try
            {
                BuildCard();
                StyleAddButton("AddTile", sprites.ButtonNormal, I18n.Get("common.normal"), UISprites.DarkText);
                StyleAddButton("AddMidspin", sprites.ButtonMidspin, I18n.Get("common.midspin"), UISprites.DarkText);
                StyleInput();
                StyleToggleButton();
                BuildToolbar();
                Refresh();
            }
            catch (System.Exception e)
            {
                Main.LogException(e);
            }
        }

        public void Cleanup()
        {
            if (root == null)
            {
                return;
            }

            DestroyChild(root, "BCD_Card");
            DestroyChild(root, "BCD_CardTitle");
            DestroyChild(root, "BCD_Language");
            DestroyChild(root, "BCD_ResetPositions");
            DestroyChild(root, "BCD_Toolbar");
            RemoveLabel(root.Find("AddTile"));
            RemoveLabel(root.Find("AddMidspin"));
            RemoveLabel(root.Find("AngleInputText"));

            Button toggle = editor.buttonToggleAngleInput;
            if (toggle != null)
            {
                DestroyChild(toggle.transform, "BCD_ToggleBg");
                DestroyChild(toggle.transform, "BCD_ToggleIcon");
                DestroyChild(toggle.transform, "BCD_ToggleEight");
                DestroyChild(toggle.transform, "BCD_Label");
                var image = toggle.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.white;
                    image.raycastTarget = true;
                    toggle.targetGraphic = image;
                }
                ApplyButtonColors(toggle);
            }

            inputRef = null;
            placeholderRef = null;
            toggleAngleIcon = null;
            toggleEightLabel = null;
            polygonLabel = null;
            historyLabel = null;
            favoritesLabel = null;
            sizeLabel = null;
            toggleStyled = false;
        }

        public void Refresh()
        {
            if (root == null)
            {
                return;
            }

            UpdateToggleState();

            if (Time.unscaledTime < nextRefresh)
            {
                return;
            }
            nextRefresh = Time.unscaledTime + 0.5f;

            if (inputRef != null && placeholderRef != null && placeholderRef.text != PlaceholderText)
            {
                placeholderRef.text = PlaceholderText;
            }
        }

        private void UpdateToggleState()
        {
            bool customMode = editor.floorButtonArbitraryContainer != null &&
                              editor.floorButtonArbitraryContainer.activeSelf;

            if (toggleAngleIcon != null)
            {
                toggleAngleIcon.gameObject.SetActive(customMode);
                toggleAngleIcon.color = UISprites.Accent;
            }
            if (toggleEightLabel != null)
            {
                toggleEightLabel.gameObject.SetActive(!customMode);
                toggleEightLabel.color = UISprites.SubText;
            }
        }

        private void BuildCard()
        {
            var card = UIFactory.Rect(root, "BCD_Card", new Vector2(0f, S(177f)), new Vector2(S(360f), S(260f)));
            card.SetAsFirstSibling();
            var image = UIFactory.AddImage(card, sprites.Card, Color.white, true);
            image.type = Image.Type.Sliced;

            var title = UIFactory.AddText(
                root,
                "BCD_CardTitle",
                I18n.Get("custom.title"),
                font,
                S(I18n.IsEnglish ? 17f : 18f),
                UISprites.Text,
                TextAlignmentOptions.Center);
            title.overflowMode = TextOverflowModes.Overflow;
            title.enableWordWrapping = false;
            UIFactory.Place(title.rectTransform, Center, Center, Center, new Vector2(0f, S(284f)), new Vector2(S(330f), S(30f)));

            var langRect = UIFactory.Rect(root, "BCD_Language", new Vector2(S(92f), S(284f)), new Vector2(S(36f), S(24f)));
            UIFactory.AddButton(langRect, sprites.ButtonDark, I18n.IsEnglish ? "EN" : "中", font, S(11f), UISprites.SubText, controller.CycleLanguage);

            var resetRect = UIFactory.Rect(root, "BCD_ResetPositions", new Vector2(S(145f), S(284f)), new Vector2(S(56f), S(24f)));
            UIFactory.AddButton(resetRect, sprites.ButtonDark, I18n.Get("common.reset"), font, S(I18n.IsEnglish ? 10f : 12f), UISprites.SubText, controller.ResetWindowPositions);
        }

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private void StyleAddButton(string objectName, Sprite background, string label, Color textColor)
        {
            Transform child = root.Find(objectName);
            if (child == null)
            {
                Main.Log("Missing custom angle button: " + objectName);
                return;
            }

            var rect = child as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Center;
                rect.anchorMax = Center;
                rect.pivot = Center;
                rect.anchoredPosition = new Vector2(objectName == "AddTile" ? S(-56f) : S(56f), S(222f));
                rect.sizeDelta = new Vector2(S(84f), S(84f));
            }

            var image = child.GetComponent<Image>();
            if (image == null)
            {
                image = child.gameObject.AddComponent<Image>();
            }
            image.sprite = background;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = child.GetComponent<Button>();
            if (button == null)
            {
                button = child.gameObject.AddComponent<Button>();
            }
            button.targetGraphic = image;
            ApplyButtonColors(button);

            Transform labelTransform = child.Find("BCD_Label");
            TextMeshProUGUI labelText;
            if (labelTransform == null)
            {
                labelText = UIFactory.AddText(child, "BCD_Label", label, font, S(I18n.IsEnglish ? 18f : 24f), textColor, TextAlignmentOptions.Center);
                UIFactory.Stretch(labelText.rectTransform, 0f, 0f, 0f, 0f);
            }
            else
            {
                labelText = labelTransform.GetComponent<TextMeshProUGUI>();
                if (labelText != null)
                {
                    labelText.text = label;
                    labelText.font = font;
                    labelText.fontSize = S(I18n.IsEnglish ? 18f : 24f);
                    labelText.color = textColor;
                }
            }
        }

        private void StyleInput()
        {
            TMP_InputField input = editor.floorButtonArbitrary;
            if (input == null)
            {
                Main.Log("Missing floorButtonArbitrary");
                return;
            }

            var rect = input.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Center;
                rect.anchorMax = Center;
                rect.pivot = Center;
                rect.anchoredPosition = new Vector2(0f, S(132f));
                rect.sizeDelta = new Vector2(S(310f), S(56f));
            }

            var image = input.GetComponent<Image>();
            if (image == null)
            {
                image = input.gameObject.AddComponent<Image>();
            }
            image.sprite = sprites.Input;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;
            input.targetGraphic = image;

            RectTransform area = input.textViewport;
            if (area == null)
            {
                Transform areaTransform = input.transform.Find("Text Area");
                area = areaTransform as RectTransform;
            }
            if (area != null)
            {
                area.anchorMin = Vector2.zero;
                area.anchorMax = Vector2.one;
                area.pivot = Center;
                area.offsetMin = new Vector2(S(12f), S(6f));
                area.offsetMax = new Vector2(S(-12f), S(-6f));
            }

            TMP_Text text = input.textComponent;
            if (text != null)
            {
                text.font = font;
                text.fontSize = S(24f);
                text.color = UISprites.Text;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.margin = Vector4.zero;
                text.enableWordWrapping = false;
                text.overflowMode = TextOverflowModes.Overflow;
                text.raycastTarget = false;
            }

            TMP_Text placeholder = input.placeholder as TMP_Text;
            inputRef = input;
            placeholderRef = placeholder;
            if (placeholder != null)
            {
                placeholder.text = PlaceholderText;
                placeholder.font = font;
                placeholder.fontSize = S(I18n.IsEnglish ? 17f : 19f);
                placeholder.color = UISprites.SubText;
                placeholder.alignment = TextAlignmentOptions.MidlineLeft;
                placeholder.enableWordWrapping = false;
                placeholder.overflowMode = TextOverflowModes.Ellipsis;
                placeholder.raycastTarget = false;
            }

            input.pointSize = S(24f);
            input.caretColor = UISprites.Accent;
            input.customCaretColor = true;
            input.caretWidth = 2;
            input.selectionColor = new Color(UISprites.Accent.r, UISprites.Accent.g, UISprites.Accent.b, 0.3f);
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterValidation = TMP_InputField.CharacterValidation.None;
            input.characterLimit = 0;

            var watcher = input.GetComponent<AngleInputWatcher>();
            if (watcher == null)
            {
                watcher = input.gameObject.AddComponent<AngleInputWatcher>();
            }
            watcher.Input = input;
        }

        private void StyleToggleButton()
        {
            Button button = editor.buttonToggleAngleInput;
            if (button == null)
            {
                return;
            }
            toggleStyled = true;

            var rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Center;
                rect.anchorMax = Center;
                rect.pivot = Center;
                rect.anchoredPosition = new Vector2(S(255f), S(-200f));
                rect.sizeDelta = new Vector2(S(64f), S(64f));
            }

            var original = button.GetComponent<Image>();
            if (original == null)
            {
                original = button.gameObject.AddComponent<Image>();
            }
            original.color = Color.clear;
            original.raycastTarget = false;

            var bgRect = UIFactory.Rect(button.transform, "BCD_ToggleBg");
            UIFactory.Stretch(bgRect, 0f, 0f, 0f, 0f);
            var bg = UIFactory.AddImage(bgRect, sprites.Circle, Color.white, true, Image.Type.Simple);
            button.targetGraphic = bg;
            ApplyButtonColors(button);

            var iconRect = UIFactory.Rect(button.transform, "BCD_ToggleIcon", Vector2.zero, new Vector2(S(34f), S(34f)));
            toggleAngleIcon = UIFactory.AddImage(iconRect, sprites.AngleIcon, UISprites.Accent, false, Image.Type.Simple);
            toggleAngleIcon.preserveAspect = true;

            toggleEightLabel = UIFactory.AddText(
                button.transform,
                "BCD_ToggleEight",
                "8",
                font,
                S(30f),
                UISprites.SubText,
                TextAlignmentOptions.Center);
            UIFactory.Stretch(toggleEightLabel.rectTransform, 0f, 0f, 0f, 0f);

            for (int i = 0; i < button.transform.childCount; i++)
            {
                Transform child = button.transform.GetChild(i);
                if (child.name != "BCD_ToggleBg" &&
                    child.name != "BCD_ToggleIcon" &&
                    child.name != "BCD_ToggleEight")
                {
                    child.gameObject.SetActive(false);
                }
            }

            UpdateToggleState();
        }

        private void BuildToolbar()
        {
            var bar = UIFactory.Rect(root, "BCD_Toolbar", new Vector2(0f, S(82f)), new Vector2(S(348f), S(36f)));
            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = S(6f);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            polygonLabel = AddToolButton(bar, I18n.Get("common.polygon"), controller.TogglePolygon);
            historyLabel = AddToolButton(bar, I18n.Get("common.history"), controller.ToggleHistory);
            favoritesLabel = AddToolButton(bar, I18n.Get("common.favorites"), controller.ToggleFavorites);
            sizeLabel = AddToolButton(bar, I18n.Get("common.size"), controller.ToggleSizeSettings);
        }

        private TextMeshProUGUI AddToolButton(Transform parent, string label, System.Action onClick)
        {
            var rect = UIFactory.Rect(parent, "BCD_Tool_" + label, Vector2.zero, new Vector2(S(82f), S(36f)));
            var button = UIFactory.AddButton(rect, sprites.ButtonDark, label, font, S(I18n.IsEnglish ? 13f : 16f), UISprites.SubText, onClick);
            UIFactory.AddLayoutElement(rect, S(82f), S(36f), 1f);
            return button.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetToolbarState(bool polygonOpen, bool historyOpen, bool favoritesOpen, bool sizeOpen)
        {
            if (polygonLabel != null)
            {
                polygonLabel.color = polygonOpen ? UISprites.Accent : UISprites.SubText;
            }
            if (historyLabel != null)
            {
                historyLabel.color = historyOpen ? UISprites.Accent : UISprites.SubText;
            }
            if (favoritesLabel != null)
            {
                favoritesLabel.color = favoritesOpen ? UISprites.Accent : UISprites.SubText;
            }
            if (sizeLabel != null)
            {
                sizeLabel.color = sizeOpen ? UISprites.Accent : UISprites.SubText;
            }
        }

        private static void ApplyButtonColors(Button button)
        {
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return;
            }
            Transform child = parent.Find(name);
            if (child != null)
            {
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
        }

        private static void RemoveLabel(Transform parent)
        {
            if (parent == null)
            {
                return;
            }
            DestroyChild(parent, "BCD_Label");
        }
    }
}
