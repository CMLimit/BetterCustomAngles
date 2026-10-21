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
