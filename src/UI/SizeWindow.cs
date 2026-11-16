using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class SizeWindow
    {
        public readonly FloatingWindow Frame;

        private readonly Controller controller;
        private readonly Slider cardSlider;
        private readonly Slider windowSlider;
        private readonly TextMeshProUGUI cardValue;
        private readonly TextMeshProUGUI windowValue;

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        public bool IsOpen => Frame.IsOpen;

        public SizeWindow(Controller controller, Vector2 position)
        {
            this.controller = controller;

            Frame = new FloatingWindow(
                controller,
                "size",
                I18n.Get("size.title"),
                new Vector2(400f, 240f),
                false,
                position);

            TMP_FontAsset font = controller.FontAsset;
            UISprites sprites = controller.Sprites;

            var cardLabel = UIFactory.AddText(
                Frame.Content,
                "CardLabel",
                I18n.Get("size.card"),
                font,
                14f,
                UISprites.SubText,
                TextAlignmentOptions.MidlineLeft);
            UIFactory.Place(cardLabel.rectTransform, Center, Center, Center, new Vector2(-95f, 66f), new Vector2(180f, 20f));

            cardValue = UIFactory.AddText(
                Frame.Content,
                "CardValue",
                Percent(controller.CardScale),
                font,
                14f,
                UISprites.Accent,
                TextAlignmentOptions.MidlineRight);
            UIFactory.Place(cardValue.rectTransform, Center, Center, Center, new Vector2(135f, 66f), new Vector2(60f, 20f));

            var cardSliderRect = UIFactory.Rect(Frame.Content, "CardSlider", new Vector2(0f, 42f), new Vector2(330f, 22f));
            cardSlider = UIFactory.AddSlider(
                cardSliderRect,
                sprites.Row,
                sprites.ButtonNormal,
                sprites.Circle,
                ModConfig.MinScale,
                ModConfig.MaxScale,
                controller.CardScale,
                OnCardSliderChanged);

            var windowLabel = UIFactory.AddText(
                Frame.Content,
                "WindowLabel",
                I18n.Get("size.windows"),
                font,
                14f,
                UISprites.SubText,
                TextAlignmentOptions.MidlineLeft);
            UIFactory.Place(windowLabel.rectTransform, Center, Center, Center, new Vector2(-95f, 6f), new Vector2(180f, 20f));

            windowValue = UIFactory.AddText(
                Frame.Content,
                "WindowValue",
                Percent(controller.WindowScale),
                font,
                14f,
                UISprites.Accent,
                TextAlignmentOptions.MidlineRight);
            UIFactory.Place(windowValue.rectTransform, Center, Center, Center, new Vector2(135f, 6f), new Vector2(60f, 20f));

            var windowSliderRect = UIFactory.Rect(Frame.Content, "WindowSlider", new Vector2(0f, -18f), new Vector2(330f, 22f));
            windowSlider = UIFactory.AddSlider(
                windowSliderRect,
                sprites.Row,
                sprites.ButtonNormal,
                sprites.Circle,
                ModConfig.MinScale,
                ModConfig.MaxScale,
                controller.WindowScale,
                OnWindowSliderChanged);

            var resetRect = UIFactory.Rect(Frame.Content, "Reset", new Vector2(0f, -58f), new Vector2(130f, 30f));
            UIFactory.AddButton(resetRect, sprites.ButtonDark, I18n.Get("common.reset"), font, 14f, UISprites.SubText, ResetScales);
        }

        private static string Percent(float value)
        {
            return Mathf.RoundToInt(value * 100f) + "%";
        }

        private void OnCardSliderChanged(float value)
        {
            float next = Mathf.Round(value * 50f) / 50f;
            if (cardValue != null)
            {
                cardValue.text = Percent(next);
            }
            controller.SetCardScale(next);
        }

        private void OnWindowSliderChanged(float value)
        {
            float next = Mathf.Round(value * 100f) / 100f;
            if (windowValue != null)
            {
                windowValue.text = Percent(next);
            }
            controller.SetWindowScale(next);
        }

        private void ResetScales()
        {
            if (cardSlider != null)
            {
                cardSlider.SetValueWithoutNotify(1f);
            }
            if (windowSlider != null)
            {
                windowSlider.SetValueWithoutNotify(1f);
            }
            if (cardValue != null)
            {
                cardValue.text = Percent(1f);
            }
            if (windowValue != null)
            {
                windowValue.text = Percent(1f);
            }
            controller.SetCardScale(1f);
            controller.SetWindowScale(1f);
        }

        public void Toggle()
        {
            Frame.Toggle();
        }
    }
}
