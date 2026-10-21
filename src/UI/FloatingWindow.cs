using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BetterCustomAngles
{
    public class FloatingWindow
    {
        public readonly Controller Controller;
        public readonly string Key;
        public readonly RectTransform Root;
        public readonly RectTransform Content;
        public readonly Canvas Canvas;
        public readonly RectTransform CanvasRect;
        public readonly bool TopRight;

        public event Action<bool> OpenChanged;

        public FloatingWindow(
            Controller controller,
            string key,
            string title,
            Vector2 size,
            bool topRight,
            Vector2 position)
        {
            Controller = controller;
            Key = key;
            TopRight = topRight;
            Canvas = controller.OverlayCanvas;
            CanvasRect = controller.OverlayRect;

            Root = UIFactory.Rect(controller.OverlayRect, "BCD_Window_" + key);
            Root.anchorMin = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            Root.anchorMax = Root.anchorMin;
            Root.pivot = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            Root.anchoredPosition = position;
            Root.sizeDelta = size;

            var background = Root.gameObject.AddComponent<Image>();
            background.sprite = controller.Sprites.Window;
            background.type = Image.Type.Sliced;
            background.color = Color.white;
            background.raycastTarget = true;

            var focus = Root.gameObject.AddComponent<WindowFocus>();
            focus.Window = this;

            var titleBar = UIFactory.Rect(Root, "TitleBar");
            titleBar.anchorMin = new Vector2(0f, 1f);
            titleBar.anchorMax = new Vector2(1f, 1f);
            titleBar.pivot = new Vector2(0.5f, 1f);
            titleBar.anchoredPosition = Vector2.zero;
            titleBar.sizeDelta = new Vector2(0f, 40f);

            var dragHit = titleBar.gameObject.AddComponent<Image>();
            dragHit.color = new Color(1f, 1f, 1f, 0f);
            dragHit.raycastTarget = true;
            var drag = titleBar.gameObject.AddComponent<WindowDrag>();
            drag.Window = this;

            var titleText = UIFactory.AddText(
                titleBar,
                "Title",
                title,
                controller.FontAsset,
                17f,
                UISprites.Text,
                TextAlignmentOptions.MidlineLeft);
            UIFactory.Stretch(titleText.rectTransform, 14f, 0f, 46f, 0f);

            var closeRect = UIFactory.Rect(titleBar, "Close", new Vector2(-18f, 0f), new Vector2(28f, 28f));
            UIFactory.Place(closeRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 0f), new Vector2(28f, 28f));
            UIFactory.AddButton(closeRect, controller.Sprites.ButtonDark, "×", controller.FontAsset, 20f, UISprites.SubText, Close);

            var divider = UIFactory.Rect(Root, "Divider");
            UIFactory.Place(divider, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(0f, 1f));
            var dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.color = new Color32(46, 49, 56, 255);
            dividerImage.raycastTarget = false;

            Content = UIFactory.Rect(Root, "Content");
            Content.anchorMin = Vector2.zero;
            Content.anchorMax = Vector2.one;
            Content.pivot = new Vector2(0.5f, 0.5f);
            Content.offsetMin = new Vector2(12f, 12f);
            Content.offsetMax = new Vector2(-12f, -48f);

            Root.gameObject.SetActive(false);
        }

        public bool IsOpen => Root.gameObject.activeSelf;

        public void SetOpen(bool open)
        {
            if (Root == null)
            {
                return;
            }

            bool changed = Root.gameObject.activeSelf != open;
            Root.gameObject.SetActive(open);
            if (open)
            {
                Root.SetAsLastSibling();
                ClampToScreen();
            }

            if (changed)
            {
                OpenChanged?.Invoke(open);
            }
        }

        public void Toggle()
        {
            SetOpen(!IsOpen);
        }

        public void Close()
        {
            SetOpen(false);
        }

        public void BringToFront()
        {
            Root.SetAsLastSibling();
        }

        public void NotifyMoved()
        {
            Controller.OnWindowMoved(this, Root.anchoredPosition);
        }

        public void SetPosition(Vector2 position)
        {
            Root.anchoredPosition = position;
            ClampToScreen();
        }

        public void ClampToScreen()
        {
            if (CanvasRect == null)
            {
                return;
            }

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(CanvasRect, Root);
            Vector2 canvasSize = CanvasRect.rect.size;
            const float margin = 24f;

            float dx = 0f;
            float dy = 0f;
            if (bounds.min.x < -canvasSize.x * 0.5f + margin)
            {
                dx = (-canvasSize.x * 0.5f + margin) - bounds.min.x;
            }
            if (bounds.max.x > canvasSize.x * 0.5f - margin)
            {
                dx = (canvasSize.x * 0.5f - margin) - bounds.max.x;
            }
            if (bounds.min.y < -canvasSize.y * 0.5f + margin)
            {
                dy = (-canvasSize.y * 0.5f + margin) - bounds.min.y;
            }
            if (bounds.max.y > canvasSize.y * 0.5f - margin)
            {
                dy = (canvasSize.y * 0.5f - margin) - bounds.max.y;
            }

            if (dx != 0f || dy != 0f)
            {
                Root.anchoredPosition += new Vector2(dx, dy);
            }
        }
    }

    public class WindowDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public FloatingWindow Window;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Window?.BringToFront();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Window == null)
            {
                return;
            }

            float scale = Window.Canvas != null ? Window.Canvas.scaleFactor : 1f;
            if (scale <= 0f)
            {
                scale = 1f;
            }
            Window.Root.anchoredPosition += eventData.delta / scale;
            Window.ClampToScreen();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Window?.NotifyMoved();
        }
    }

    public class WindowFocus : MonoBehaviour, IPointerDownHandler
    {
        public FloatingWindow Window;

        public void OnPointerDown(PointerEventData eventData)
        {
            Window?.BringToFront();
        }
    }
}
