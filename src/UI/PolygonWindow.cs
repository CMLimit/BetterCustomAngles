using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class PolygonWindow
    {
        public readonly FloatingWindow Frame;

        private readonly Controller controller;
        private TMP_InputField nInput;
        private TextMeshProUGUI resultText;
        private TextMeshProUGUI formulaText;
        private Button normalButton;
        private Button midspinButton;

        private int nValue = 6;
        private float angle;
        private bool valid;

        public bool IsOpen => Frame.IsOpen;

        public PolygonWindow(Controller controller, Vector2 position)
        {
            this.controller = controller;
            Frame = new FloatingWindow(
                controller,
                "polygon",
                I18n.Get("polygon.title"),
                new Vector2(350f, 310f),
                false,
                position);

            var label = UIFactory.AddText(
                Frame.Content,
                "NLabel",
                I18n.Get("polygon.sides"),
                controller.FontAsset,
                16f,
                UISprites.SubText,
                TextAlignmentOptions.MidlineLeft);
            UIFactory.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-112f, 86f), new Vector2(80f, 36f));

            var inputRect = UIFactory.Rect(Frame.Content, "NInput", new Vector2(-28f, 86f), new Vector2(74f, 38f));
            nInput = UIFactory.AddInputField(
                inputRect,
                controller.Sprites.Input,
                controller.FontAsset,
                20f,
                "n",
                UISprites.Text,
                UISprites.SubText,
                UISprites.Accent,
                true,
                3);
            nInput.text = "6";
            if (nInput.textComponent != null)
            {
                nInput.textComponent.alignment = TextAlignmentOptions.Center;
            }
            if (nInput.placeholder is TMP_Text ph)
            {
                ph.alignment = TextAlignmentOptions.Center;
            }
            nInput.onValueChanged.AddListener(OnNChanged);
            nInput.onEndEdit.AddListener(OnNEndEdit);

            var minus = UIFactory.Rect(Frame.Content, "Minus", new Vector2(24f, 86f), new Vector2(34f, 34f));
            UIFactory.AddButton(minus, controller.Sprites.ButtonDark, "−", controller.FontAsset, 20f, UISprites.Text, () => SetN(nValue - 1));
            var plus = UIFactory.Rect(Frame.Content, "Plus", new Vector2(64f, 86f), new Vector2(34f, 34f));
            UIFactory.AddButton(plus, controller.Sprites.ButtonDark, "+", controller.FontAsset, 20f, UISprites.Text, () => SetN(nValue + 1));

            BuildPresets();

            var resultBack = UIFactory.Rect(Frame.Content, "ResultBack", new Vector2(0f, -4f), new Vector2(170f, 54f));
            var resultBackImage = UIFactory.AddImage(resultBack, controller.Sprites.Row, new Color(0.18f, 0.55f, 0.35f, 0.35f), false);
            resultBackImage.type = Image.Type.Sliced;

            resultText = UIFactory.AddText(
                Frame.Content,
                "Result",
                "120°",
                controller.FontAsset,
                32f,
                UISprites.Accent,
                TextAlignmentOptions.Center);
            resultText.overflowMode = TextOverflowModes.Overflow;
            resultText.enableWordWrapping = false;
            UIFactory.Place(resultText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(300f, 60f));

            formulaText = UIFactory.AddText(
                Frame.Content,
                "Formula",
                "(6-2)*180/6",
                controller.FontAsset,
                14f,
                UISprites.SubText,
                TextAlignmentOptions.Center);
            formulaText.overflowMode = TextOverflowModes.Overflow;
            UIFactory.Place(formulaText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(300f, 24f));

            var normalRect = UIFactory.Rect(Frame.Content, "PlaceNormal", new Vector2(-78f, -92f), new Vector2(142f, 38f));
            normalButton = UIFactory.AddButton(normalRect, controller.Sprites.ButtonNormal, I18n.Get("polygon.place_normal"), controller.FontAsset, I18n.IsEnglish ? 14f : 16f, UISprites.DarkText, () => Place(false));
            var midspinRect = UIFactory.Rect(Frame.Content, "PlaceMidspin", new Vector2(78f, -92f), new Vector2(142f, 38f));
            midspinButton = UIFactory.AddButton(midspinRect, controller.Sprites.ButtonMidspin, I18n.Get("polygon.place_midspin"), controller.FontAsset, I18n.IsEnglish ? 14f : 16f, UISprites.DarkText, () => Place(true));

            Compute();
        }

        private void BuildPresets()
        {
            int[] presets = { 3, 4, 5, 6, 8, 12 };
            float startX = -125f;
            for (int i = 0; i < presets.Length; i++)
            {
                int preset = presets[i];
                var rect = UIFactory.Rect(Frame.Content, "Preset" + preset, new Vector2(startX + i * 50f, 44f), new Vector2(46f, 28f));
                UIFactory.AddButton(rect, controller.Sprites.ButtonDark, preset.ToString(), controller.FontAsset, 14f, UISprites.SubText, () => SetN(preset));
            }
        }

        private void OnNChanged(string text)
        {
            int parsed;
            if (int.TryParse(text, out parsed) && parsed >= 3 && parsed <= 360)
            {
                nValue = parsed;
                Compute();
            }
            else
            {
                valid = false;
                if (resultText != null)
                {
                    resultText.text = "—";
                }
                if (formulaText != null)
                {
                    formulaText.text = I18n.Get("polygon.invalid");
                }
                SetButtonsInteractable(false);
            }
        }

        private void OnNEndEdit(string text)
        {
            int parsed;
            if (!int.TryParse(text, out parsed))
            {
                parsed = nValue;
            }
            parsed = Mathf.Clamp(parsed, 3, 360);
            SetN(parsed);
        }

        private void SetN(int value)
        {
            nValue = Mathf.Clamp(value, 3, 360);
            if (nInput != null)
            {
                nInput.SetTextWithoutNotify(nValue.ToString());
            }
            Compute();
        }

        private void Compute()
        {
            if (nValue < 3 || nValue > 360)
            {
                valid = false;
                if (resultText != null) resultText.text = "—";
                if (formulaText != null) formulaText.text = I18n.Get("polygon.invalid");
                SetButtonsInteractable(false);
                return;
            }

            double value = (nValue - 2) * 180.0 / nValue;
            angle = AngleUtil.Round6((float)value);
            valid = true;
            if (resultText != null)
            {
                resultText.text = AngleUtil.Display(angle);
            }
            if (formulaText != null)
            {
                formulaText.text = "(" + nValue + "-2)*180/" + nValue;
            }
            SetButtonsInteractable(true);

        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (normalButton != null)
            {
                normalButton.interactable = interactable;
            }
            if (midspinButton != null)
            {
                midspinButton.interactable = interactable;
            }
        }

        private void Place(bool midspin)
        {
            if (valid)
            {
                if (controller.PlaceAngle(angle, midspin))
                {
                    controller.Store.SetNote(angle, AngleUtil.PolygonNotePrefix + nValue);
                }
            }
        }

        public void Toggle()
        {
            Frame.Toggle();
        }
    }
}
