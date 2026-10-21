using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class AngleListWindow
    {
        private class RowView
        {
            public float Angle;
            public Image Star;
        }

        public readonly FloatingWindow Frame;

        private readonly Controller controller;
        private readonly AngleStore store;
        private readonly bool favoritesMode;
        private readonly ScrollRect scroll;
        private readonly RectTransform content;
        private readonly List<RowView> rows = new List<RowView>();
        private bool dirty = true;

        public bool IsOpen => Frame.IsOpen;

        public AngleListWindow(
            Controller controller,
            string key,
            string title,
            Vector2 size,
            bool topRight,
            Vector2 position,
            bool favoritesMode)
        {
            this.controller = controller;
            this.store = controller.Store;
            this.favoritesMode = favoritesMode;

            Frame = new FloatingWindow(controller, key, title, size, topRight, position);
            Frame.OpenChanged += open =>
            {
                if (open)
                {
                    dirty = true;
                }
            };

            var viewport = Frame.Content;
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 25f;
            scroll.inertia = true;

            content = UIFactory.Rect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(0, 0, 0, 4);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;

            if (favoritesMode)
            {
                store.FavoritesChanged += MarkDirty;
            }
            else
            {
                store.HistoryChanged += MarkDirty;
                store.FavoritesChanged += RefreshStars;
            }
            store.NotesChanged += MarkDirty;
        }

        public void Toggle()
        {
            Frame.Toggle();
            if (Frame.IsOpen)
            {
                dirty = true;
            }
        }

        public void RefreshIfDirty()
        {
            if (!dirty || !Frame.IsOpen)
            {
                return;
            }
            dirty = false;
            Rebuild();
        }

        private void MarkDirty()
        {
            dirty = true;
        }

        private void Rebuild()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject child = content.GetChild(i).gameObject;
                child.transform.SetParent(null, false);
                UnityEngine.Object.Destroy(child);
            }
            rows.Clear();

            IReadOnlyList<float> list = favoritesMode ? store.Favorites : store.History;
            if (list.Count == 0)
            {
                var empty = UIFactory.AddText(
                    content,
                    "Empty",
                    I18n.Get(favoritesMode ? "favorites.empty" : "history.empty"),
                    controller.FontAsset,
                    15f,
                    UISprites.SubText,
                    TextAlignmentOptions.Center);
                UIFactory.AddLayoutElement(empty.rectTransform, -1, 60f);
                empty.enableWordWrapping = false;
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                CreateRow(list[i]);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void CreateRow(float angle)
        {
            float captured = angle;
            var row = UIFactory.Rect(content, "Row", Vector2.zero, new Vector2(0f, 38f));
            var background = row.gameObject.AddComponent<Image>();
            background.sprite = controller.Sprites.Row;
            background.type = Image.Type.Sliced;
            background.color = Color.white;
            background.raycastTarget = false;
            UIFactory.AddLayoutElement(row, -1, 38f, 1f);

            float buttonWidth = I18n.IsEnglish ? 54f : 46f;
            const float starWidth = 30f;
            const float deleteWidth = 24f;
            const float gap = 4f;
            const float rightPad = 8f;

            float starX = -(rightPad + starWidth * 0.5f);
            float cursor = starX - starWidth * 0.5f - gap;

            float deleteX = 0f;
            bool showDelete = !favoritesMode;
            if (showDelete)
            {
                deleteX = cursor - deleteWidth * 0.5f;
                cursor = cursor - deleteWidth - gap;
            }

            float midspinX = cursor - buttonWidth * 0.5f;
            cursor = cursor - buttonWidth - gap;
            float normalX = cursor - buttonWidth * 0.5f;
            cursor = cursor - buttonWidth - gap;

            float labelRight = -cursor;

            var textBlock = UIFactory.Rect(row, "TextBlock");
            textBlock.anchorMin = Vector2.zero;
            textBlock.anchorMax = Vector2.one;
            textBlock.pivot = new Vector2(0.5f, 0.5f);
            textBlock.offsetMin = new Vector2(8f, 3f);
            textBlock.offsetMax = new Vector2(-labelRight, -3f);

            var label = UIFactory.AddText(
                textBlock,
                "Angle",
                AngleUtil.Display(captured),
                controller.FontAsset,
                15f,
                UISprites.Text,
                TextAlignmentOptions.MidlineLeft);
            label.overflowMode = TextOverflowModes.Ellipsis;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(96f, 0f);

            string note = AngleUtil.FormatNote(store.GetNote(captured));
            bool hasNote = !string.IsNullOrEmpty(note);
            var noteText = UIFactory.AddText(
                textBlock,
                "Note",
                hasNote ? note : I18n.Get("note.add"),
                controller.FontAsset,
                10f,
                hasNote ? UISprites.SubText : new Color(UISprites.SubText.r, UISprites.SubText.g, UISprites.SubText.b, 0.65f),
                TextAlignmentOptions.MidlineLeft);
            noteText.overflowMode = TextOverflowModes.Ellipsis;
            noteText.raycastTarget = true;
            var noteRect = noteText.rectTransform;
            noteRect.anchorMin = new Vector2(0f, 0f);
            noteRect.anchorMax = new Vector2(1f, 1f);
            noteRect.pivot = new Vector2(0f, 0.5f);
            noteRect.anchoredPosition = new Vector2(96f, 0f);
            noteRect.sizeDelta = new Vector2(-96f, 0f);

            var noteButton = noteText.gameObject.AddComponent<Button>();
            noteButton.targetGraphic = noteText;
            noteButton.transition = Selectable.Transition.ColorTint;
            var noteColors = noteButton.colors;
            noteColors.normalColor = Color.white;
            noteColors.highlightedColor = new Color(0.82f, 1f, 0.86f, 1f);
            noteColors.pressedColor = new Color(0.7f, 0.9f, 0.75f, 1f);
            noteColors.fadeDuration = 0.08f;
            noteButton.colors = noteColors;
            noteButton.onClick.AddListener(() => controller.OpenNoteEditor(captured));

            var normal = UIFactory.Rect(row, "Normal", Vector2.zero, new Vector2(buttonWidth, 26f));
            UIFactory.Place(normal, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(normalX, 0f), new Vector2(buttonWidth, 26f));
            UIFactory.AddButton(normal, controller.Sprites.ButtonDark, I18n.Get("common.normal"), controller.FontAsset, I18n.IsEnglish ? 10f : 12f, UISprites.Accent,
                () => controller.PlaceAngle(captured, false));

            var midspin = UIFactory.Rect(row, "Midspin", Vector2.zero, new Vector2(buttonWidth, 26f));
            UIFactory.Place(midspin, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(midspinX, 0f), new Vector2(buttonWidth, 26f));
            UIFactory.AddButton(midspin, controller.Sprites.ButtonDark, I18n.Get("common.midspin"), controller.FontAsset, I18n.IsEnglish ? 10f : 12f, UISprites.Midspin,
                () => controller.PlaceAngle(captured, true));

            if (showDelete)
            {
                var delete = UIFactory.Rect(row, "Delete", Vector2.zero, new Vector2(deleteWidth, 26f));
                UIFactory.Place(delete, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(deleteX, 0f), new Vector2(deleteWidth, 26f));
                UIFactory.AddButton(delete, controller.Sprites.ButtonDark, "×", controller.FontAsset, 16f, UISprites.SubText,
                    () => store.RemoveHistory(captured));
            }

            bool favorite = store.IsFavorite(captured);
            var star = UIFactory.Rect(row, "Star", Vector2.zero, new Vector2(starWidth, 26f));
            UIFactory.Place(star, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(starX, 0f), new Vector2(starWidth, 26f));
            UIFactory.AddButton(
                star,
                controller.Sprites.ButtonDark,
                string.Empty,
                controller.FontAsset,
                10f,
                Color.clear,
                () => store.ToggleFavorite(captured));

            var iconRect = UIFactory.Rect(star, "Icon", Vector2.zero, new Vector2(17f, 17f));
            var icon = UIFactory.AddImage(
                iconRect,
                controller.Sprites.StarIcon,
                favorite ? UISprites.Star : UISprites.SubText,
                false,
                Image.Type.Simple);
            icon.preserveAspect = true;

            rows.Add(new RowView
            {
                Angle = captured,
                Star = icon
            });
        }

        private void RefreshStars()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                RowView row = rows[i];
                if (row.Star == null)
                {
                    continue;
                }
                bool favorite = store.IsFavorite(row.Angle);
                row.Star.color = favorite ? UISprites.Star : UISprites.SubText;
            }
        }
    }
}
