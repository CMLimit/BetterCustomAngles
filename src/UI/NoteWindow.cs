using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class NoteWindow
    {
        public readonly FloatingWindow Frame;

        private readonly Controller controller;
        private TextMeshProUGUI angleLabel;
        private TMP_InputField input;
        private float angle;

        public bool IsOpen => Frame.IsOpen;
        public float Angle => angle;

        public NoteWindow(Controller controller, Vector2 position)
        {
            this.controller = controller;
            Frame = new FloatingWindow(
                controller,
                "note",
                I18n.Get("note.title"),
                new Vector2(330f, 190f),
                false,
                position);

            angleLabel = UIFactory.AddText(
                Frame.Content,
                "Angle",
                string.Empty,
                controller.FontAsset,
                19f,
                UISprites.Accent,
                TextAlignmentOptions.Center);
            UIFactory.Place(angleLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(290f, 28f));

            var inputRect = UIFactory.Rect(Frame.Content, "NoteInput", new Vector2(0f, 6f), new Vector2(290f, 40f));
            input = UIFactory.AddInputField(
                inputRect,
                controller.Sprites.Input,
                controller.FontAsset,
                17f,
                I18n.Get("note.placeholder"),
                UISprites.Text,
                UISprites.SubText,
                UISprites.Accent,
                false,
                120);
            input.onSubmit.AddListener(_ => Save());
            Frame.OpenChanged += open =>
            {
                if (!open && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            };

            var saveRect = UIFactory.Rect(Frame.Content, "Save", new Vector2(-75f, -48f), new Vector2(132f, 34f));
            UIFactory.AddButton(saveRect, controller.Sprites.ButtonNormal, I18n.Get("common.save"), controller.FontAsset, 15f, UISprites.DarkText, Save);

            var cancelRect = UIFactory.Rect(Frame.Content, "Cancel", new Vector2(75f, -48f), new Vector2(132f, 34f));
            UIFactory.AddButton(cancelRect, controller.Sprites.ButtonDark, I18n.Get("common.cancel"), controller.FontAsset, 15f, UISprites.SubText, Close);
        }

        public void Open(float value)
        {
            angle = AngleUtil.Round6(value);
            if (angleLabel != null)
            {
                angleLabel.text = AngleUtil.Display(angle);
            }
            Frame.SetOpen(true);
            Frame.BringToFront();
            if (input != null)
            {
                input.SetTextWithoutNotify(AngleUtil.FormatNote(controller.Store.GetNote(angle)));
                input.ActivateInputField();
                input.Select();
            }
        }

        public void Close()
        {
            Frame.SetOpen(false);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void Save()
        {
            if (input != null)
            {
                controller.Store.SetNote(angle, input.text);
            }
            Close();
        }

    }
}
