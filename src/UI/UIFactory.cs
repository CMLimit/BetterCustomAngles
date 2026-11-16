using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public static class UIFactory
    {
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.localScale = Vector3.one;
            return rt;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var rt = Rect(parent, name);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            return rt;
        }

        public static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }

        public static Image AddImage(RectTransform rt, Sprite sprite, Color color, bool raycastTarget, Image.Type type = Image.Type.Sliced)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = type;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static TextMeshProUGUI AddText(
            Transform parent,
            string name,
            string text,
            TMP_FontAsset font,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            bool wrap = false)
        {
            var rt = Rect(parent, name);
            Stretch(rt, 0f, 0f, 0f, 0f);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null)
            {
                tmp.font = font;
            }
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = wrap;
            tmp.overflowMode = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            tmp.richText = false;
            return tmp;
        }

        public static Button AddButton(
            RectTransform rt,
            Sprite sprite,
            string label,
            TMP_FontAsset font,
            float fontSize,
            Color textColor,
            Action onClick,
            bool raycastTarget = true)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = raycastTarget;

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var text = AddText(rt, "Label", label, font, fontSize, textColor, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, 2f, 1f, 2f, 1f);
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
            return button;
        }

        public static Button AddTextButton(
            RectTransform rt,
            string label,
            TMP_FontAsset font,
            float fontSize,
            Color textColor,
            Action onClick)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = null;
            image.color = Color.clear;
            image.raycastTarget = true;

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            var text = AddText(rt, "Label", label, font, fontSize, textColor, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
            return button;
        }

        public static TMP_InputField AddInputField(
            RectTransform rt,
            Sprite background,
            TMP_FontAsset font,
            float fontSize,
            string placeholderText,
            Color textColor,
            Color placeholderColor,
            Color caretColor,
            bool integerOnly,
            int characterLimit)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = background;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var input = rt.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = image;

            var area = Rect(rt, "Text Area");
            Stretch(area, 10f, 6f, 10f, 6f);

            var text = AddText(area, "Text", string.Empty, font, fontSize, textColor, TextAlignmentOptions.MidlineLeft);
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;

            var placeholder = AddText(area, "Placeholder", placeholderText, font, fontSize, placeholderColor, TextAlignmentOptions.MidlineLeft);
            Stretch(placeholder.rectTransform, 0f, 0f, 0f, 0f);
            placeholder.enableWordWrapping = false;
            placeholder.overflowMode = TextOverflowModes.Ellipsis;
            placeholder.raycastTarget = false;

            input.textViewport = area;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = integerOnly ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.Standard;
            input.characterValidation = integerOnly ? TMP_InputField.CharacterValidation.Integer : TMP_InputField.CharacterValidation.None;
            input.characterLimit = characterLimit;
            input.pointSize = fontSize;
            input.caretColor = caretColor;
            input.customCaretColor = true;
            input.caretWidth = 2;
            input.selectionColor = new Color(caretColor.r, caretColor.g, caretColor.b, 0.3f);
            input.onSubmit.AddListener(_ => input.DeactivateInputField());
            return input;
        }

        public static Slider AddSlider(
            RectTransform rt,
            Sprite background,
            Sprite fillSprite,
            Sprite handleSprite,
            float minValue,
            float maxValue,
            float value,
            Action<float> onChanged)
        {
            var slider = rt.gameObject.AddComponent<Slider>();

            // 透明命中区：让整条滑轨都能响应点击和拖动
            var hitArea = rt.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            var bg = Rect(rt, "Background");
            Place(bg, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 6f));
            var bgImage = bg.gameObject.AddComponent<Image>();
            bgImage.sprite = background;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Color.white;
            bgImage.raycastTarget = false;

            var fillArea = Rect(rt, "Fill Area");
            Place(fillArea, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 6f));
            var fill = Rect(fillArea, "Fill");
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.pivot = new Vector2(0.5f, 0.5f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.sprite = fillSprite;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Color.white;
            fillImage.raycastTarget = false;

            var handleArea = Rect(rt, "Handle Slide Area");
            Place(handleArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            handleArea.offsetMin = new Vector2(9f, 0f);
            handleArea.offsetMax = new Vector2(-9f, 0f);

            var handle = Rect(handleArea, "Handle");
            handle.anchorMin = new Vector2(0f, 0f);
            handle.anchorMax = new Vector2(0f, 1f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.sizeDelta = new Vector2(18f, 0f);
            handle.anchoredPosition = Vector2.zero;
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = handleSprite;
            handleImage.preserveAspect = true;
            handleImage.color = Color.white;
            handleImage.raycastTarget = true;

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp(value, minValue, maxValue);

            if (onChanged != null)
            {
                slider.onValueChanged.AddListener(v => onChanged(v));
            }

            return slider;
        }

        public static void AddLayoutElement(RectTransform rt, float preferredWidth, float preferredHeight, float flexibleWidth = -1f)
        {
            var element = rt.gameObject.AddComponent<LayoutElement>();
            if (preferredWidth >= 0f)
            {
                element.preferredWidth = preferredWidth;
                element.minWidth = preferredWidth;
            }
            if (preferredHeight >= 0f)
            {
                element.preferredHeight = preferredHeight;
                element.minHeight = preferredHeight;
            }
            if (flexibleWidth >= 0f)
            {
                element.flexibleWidth = flexibleWidth;
            }
        }
    }
}
