using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BrickKids3D
{
    public class RuntimeUI : MonoBehaviour
    {
        public BuildManager manager;
        private Font font;
        private Text status;
        private readonly List<Button> brickButtons = new List<Button>();
        private readonly List<Button> slotButtons = new List<Button>();

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGO = new GameObject("KidsUI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 800);
            canvasGO.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            MakeTopBar(canvas.transform);
            MakeBrickBar(canvas.transform);
            MakeColorBar(canvas.transform);
            Refresh("SAN SANG XEP HINH");
        }

        private void MakeTopBar(Transform parent)
        {
            RectTransform bar = Panel(parent, "TopBar", new Color(0.07f, 0.09f, 0.13f, 0.88f));
            Anchor(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -84), Vector2.zero);

            AddButton(bar, "XOAY KHOI", new Vector2(12, 10), new Vector2(125, 62), () => manager.RotateSelected());
            AddButton(bar, "XOA", new Vector2(144, 10), new Vector2(92, 62), () => manager.ToggleDeleteMode());
            AddButton(bar, "UNDO", new Vector2(242, 10), new Vector2(92, 62), () => manager.Undo());
            AddButton(bar, "REDO", new Vector2(340, 10), new Vector2(92, 62), () => manager.Redo());
            AddButton(bar, "LUU", new Vector2(438, 10), new Vector2(82, 62), () => manager.SaveCurrent());
            AddButton(bar, "MO", new Vector2(526, 10), new Vector2(82, 62), () => manager.LoadCurrent());

            for (int i = 1; i <= 3; i++)
            {
                int slot = i;
                Button b = AddButton(bar, "S" + i, new Vector2(614 + (i - 1) * 58, 10), new Vector2(52, 62), () => manager.SetSlot(slot));
                slotButtons.Add(b);
            }

            AddButton(bar, "MAU", new Vector2(792, 10), new Vector2(82, 62), () => manager.LoadDemo());
            AddButton(bar, "ANH", new Vector2(880, 10), new Vector2(82, 62), () => manager.CaptureScreenshot());
            AddButton(bar, "XOA HET", new Vector2(968, 10), new Vector2(102, 62), () => manager.ClearAll());

            status = Label(bar, "Status", "", 19, TextAnchor.MiddleRight);
            RectTransform srt = status.rectTransform;
            srt.anchorMin = new Vector2(1, 0);
            srt.anchorMax = new Vector2(1, 1);
            srt.pivot = new Vector2(1, 0.5f);
            srt.sizeDelta = new Vector2(200, 0);
            srt.anchoredPosition = new Vector2(-12, 0);
        }

        private void MakeBrickBar(Transform parent)
        {
            RectTransform bar = Panel(parent, "BrickBar", new Color(0.07f, 0.09f, 0.13f, 0.92f));
            Anchor(bar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 95), Vector2.zero);

            float x = 12;
            foreach (var spec in BrickCatalog.Specs)
            {
                string id = spec.id;
                Button b = AddButton(bar, id, new Vector2(x, 14), new Vector2(86, 66), () => manager.SetBrick(id));
                brickButtons.Add(b);
                x += 92;
            }

            var hint = Label(bar, "Hint", "1 ngon: dat khoi | 2 ngon: xoay + zoom | PC: chuot phai xoay", 17, TextAnchor.MiddleRight);
            RectTransform rt = hint.rectTransform;
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(330, 0);
            rt.anchoredPosition = new Vector2(-12, 0);
        }

        private void MakeColorBar(Transform parent)
        {
            RectTransform bar = Panel(parent, "ColorBar", new Color(0.06f, 0.07f, 0.10f, 0.80f));
            bar.anchorMin = new Vector2(0, 0.5f);
            bar.anchorMax = new Vector2(0, 0.5f);
            bar.pivot = new Vector2(0, 0.5f);
            bar.sizeDelta = new Vector2(74, 490);
            bar.anchoredPosition = new Vector2(10, 0);

            Color[] colors =
            {
                new Color(0.92f,0.12f,0.08f), new Color(1f,0.72f,0.05f), new Color(0.05f,0.35f,0.92f),
                new Color(0.05f,0.72f,0.24f), new Color(1f,0.46f,0.04f), new Color(0.62f,0.16f,0.75f),
                new Color(0.96f,0.46f,0.72f), new Color(0.12f,0.12f,0.14f), new Color(0.95f,0.95f,0.95f),
                new Color(0.52f,0.31f,0.18f), new Color(0.48f,0.52f,0.58f), new Color(0.10f,0.72f,0.75f)
            };

            float y = 8;
            foreach (Color c in colors)
            {
                Color local = c;
                Button b = AddButton(bar, "", new Vector2(9, y), new Vector2(56, 34), () => manager.SetColor(local));
                b.GetComponent<Image>().color = c;
                y += 39;
            }
        }

        public void Refresh(string message = null)
        {
            if (status == null || manager == null) return;
            string mode = manager.DeleteMode ? "XOA" : "XEP";
            status.text = (message ?? (manager.SelectedBrickId + "  |  SLOT " + manager.CurrentSlot)) + "  |  " + mode;
        }

        private Button AddButton(Transform parent, string text, Vector2 pos, Vector2 size, UnityAction action)
        {
            var go = new GameObject("Btn_" + text);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.30f, 0.96f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(pos.x, -pos.y);
            rt.sizeDelta = size;

            Text label = Label(go.transform, "Text", text, 18, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, 5);
            return button;
        }

        private RectTransform Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go.GetComponent<RectTransform>();
        }

        private Text Label(Transform parent, string name, string value, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = value;
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 10;
            t.resizeTextMaxSize = fontSize;
            return t;
        }

        private void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = min; rt.anchorMax = max; rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }

        private void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
