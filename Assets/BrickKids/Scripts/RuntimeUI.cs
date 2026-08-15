using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrickKids3D
{
    public enum UIFeedback
    {
        None,
        Save,
        Load,
        Screenshot,
        Clear,
        Template,
        Error
    }

    public class RuntimeUI : MonoBehaviour
    {
        public BuildManager manager;

        private static readonly Color Panel = new Color(0.070f, 0.090f, 0.125f, 0.97f);
        private static readonly Color PanelMid = new Color(0.115f, 0.145f, 0.195f, 1f);
        private static readonly Color Accent = new Color(0.08f, 0.57f, 0.94f, 1f);
        private static readonly Color Danger = new Color(0.92f, 0.20f, 0.20f, 1f);
        private static readonly Color IconNormal = new Color(0.94f, 0.97f, 1f, 1f);
        private static readonly Color IconMuted = new Color(0.58f, 0.66f, 0.76f, 0.60f);

        private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, ButtonVisual> brickButtons = new Dictionary<string, ButtonVisual>();
        private readonly List<ColorVisual> colorButtons = new List<ColorVisual>();

        private ButtonVisual deleteButton;
        private ButtonVisual undoButton;
        private ButtonVisual redoButton;
        private Image selectedBrickImage;
        private Image selectedColorImage;

        private GameObject clearOverlay;
        private GameObject templateOverlay;

        private CanvasGroup toastGroup;
        private Image toastIcon;
        private Coroutine toastRoutine;

        private class ButtonVisual
        {
            public Button button;
            public Image background;
            public Image icon;
            public string id;
        }

        private class ColorVisual
        {
            public Button button;
            public Image outer;
            public Image inner;
            public Color color;
        }

        public void Build()
        {
            GameObject canvasObject = new GameObject(
                "KidsUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();

            CreateTopBar(root);
            CreateColorPanel(root);
            CreateBrickTray(root);
            CreateCameraTools(root);
            CreateWorkspaceFrame(root);
            CreateToast(root);
            CreateClearOverlay(root);
            CreateTemplateOverlay(root);

            RefreshState();
        }

        public void RefreshState()
        {
            if (manager == null) return;

            foreach (KeyValuePair<string, ButtonVisual> pair in brickButtons)
            {
                bool selected = pair.Key == manager.SelectedBrickId;
                pair.Value.background.color = selected ? Accent : PanelMid;
                pair.Value.icon.color = selected
                    ? manager.SelectedColor
                    : new Color(0.76f, 0.83f, 0.91f, 1f);
            }

            for (int i = 0; i < colorButtons.Count; i++)
            {
                ColorVisual visual = colorButtons[i];
                bool selected = ColorDistance(visual.color, manager.SelectedColor) < 0.05f;
                visual.outer.color = selected ? Accent : PanelMid;
                visual.inner.color = visual.color;
            }

            if (deleteButton != null)
                deleteButton.background.color = manager.DeleteMode ? Danger : PanelMid;

            SetEnabled(undoButton, manager.CanUndo);
            SetEnabled(redoButton, manager.CanRedo);

            if (selectedBrickImage != null)
            {
                selectedBrickImage.sprite = BrickSprite(manager.SelectedBrickId);
                selectedBrickImage.color = manager.SelectedColor;
            }

            if (selectedColorImage != null)
                selectedColorImage.color = manager.SelectedColor;
        }

        public void ShowFeedback(UIFeedback feedback)
        {
            string iconName = "icon_check";
            Color color = new Color(0.18f, 0.88f, 0.49f, 1f);

            if (feedback == UIFeedback.Load) iconName = "icon_open";
            else if (feedback == UIFeedback.Screenshot) iconName = "icon_camera";
            else if (feedback == UIFeedback.Error)
            {
                iconName = "icon_close";
                color = Danger;
            }

            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine(iconName, color));
        }

        private void CreateTopBar(RectTransform parent)
        {
            RectTransform bar = PanelRect(
                parent,
                "TopToolbar",
                new Vector2(0.004f, 0.905f),
                new Vector2(0.996f, 0.996f),
                Panel);

            HorizontalLayoutGroup layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image appMark = AddLayoutImage(bar, "AppMark", LoadSprite("app_mark"), 66f, 66f);
            appMark.preserveAspect = true;
            AddSpacer(bar, 8f, 0f);

            AddToolbarButton(bar, "Rotate", "icon_rotate", 64f, manager.RotateSelected);
            deleteButton = AddToolbarButton(bar, "Delete", "icon_delete", 64f, manager.ToggleDeleteMode);
            undoButton = AddToolbarButton(bar, "Undo", "icon_undo", 64f, manager.Undo);
            redoButton = AddToolbarButton(bar, "Redo", "icon_redo", 64f, manager.Redo);
            AddToolbarButton(bar, "Save", "icon_save", 64f, manager.SaveCurrent);
            AddToolbarButton(bar, "Open", "icon_open", 64f, manager.LoadCurrent);
            AddToolbarButton(bar, "Screenshot", "icon_camera", 64f, manager.CaptureScreenshot);

            ButtonVisual clear = AddToolbarButton(bar, "Clear", "icon_clear", 64f, ShowClearOverlay);
            clear.background.color = new Color(0.17f, 0.12f, 0.15f, 1f);

            AddFlexibleSpace(bar);

            RectTransform previewCard = AddLayoutRect(bar, "SelectedBrick", 100f, 68f);
            Image previewBackground = previewCard.gameObject.AddComponent<Image>();
            previewBackground.sprite = RoundedSprite();
            previewBackground.type = Image.Type.Sliced;
            previewBackground.color = PanelMid;

            GameObject previewObject = new GameObject("BrickPreview", typeof(RectTransform), typeof(Image));
            previewObject.transform.SetParent(previewCard, false);
            selectedBrickImage = previewObject.GetComponent<Image>();
            Stretch(selectedBrickImage.rectTransform, 7f);
            selectedBrickImage.preserveAspect = true;
            selectedBrickImage.raycastTarget = false;

            RectTransform colorCard = AddLayoutRect(bar, "SelectedColor", 58f, 58f);
            Image outer = colorCard.gameObject.AddComponent<Image>();
            outer.sprite = CircleSprite();
            outer.color = new Color(0.85f, 0.90f, 0.96f, 1f);

            GameObject innerObject = new GameObject("Color", typeof(RectTransform), typeof(Image));
            innerObject.transform.SetParent(colorCard, false);
            selectedColorImage = innerObject.GetComponent<Image>();
            selectedColorImage.sprite = CircleSprite();
            selectedColorImage.raycastTarget = false;
            Stretch(selectedColorImage.rectTransform, 7f);
        }

        private void CreateColorPanel(RectTransform parent)
        {
            RectTransform panel = PanelRect(
                parent,
                "ColorPalette",
                new Vector2(0.004f, 0.158f),
                new Vector2(0.087f, 0.898f),
                Panel);

            GameObject headerObject = new GameObject("PaletteIcon", typeof(RectTransform), typeof(Image));
            headerObject.transform.SetParent(panel, false);
            Image header = headerObject.GetComponent<Image>();
            header.sprite = LoadSprite("icon_palette");
            header.color = IconNormal;
            header.raycastTarget = false;

            RectTransform headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(42f, 42f);
            headerRect.anchoredPosition = new Vector2(0f, -14f);

            GameObject gridObject = new GameObject("ColorGrid", typeof(RectTransform));
            gridObject.transform.SetParent(panel, false);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 0f);
            gridRect.anchorMax = new Vector2(1f, 1f);
            gridRect.offsetMin = new Vector2(9f, 14f);
            gridRect.offsetMax = new Vector2(-9f, -66f);

            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(45f, 45f);
            grid.spacing = new Vector2(9f, 10f);
            grid.padding = new RectOffset(3, 3, 4, 4);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;

            Color[] colors =
            {
                new Color(0.93f,0.18f,0.13f),
                new Color(1.00f,0.70f,0.07f),
                new Color(0.06f,0.39f,0.92f),
                new Color(0.08f,0.70f,0.31f),
                new Color(1.00f,0.42f,0.06f),
                new Color(0.57f,0.20f,0.78f),
                new Color(0.94f,0.38f,0.67f),
                new Color(0.08f,0.09f,0.11f),
                new Color(0.94f,0.95f,0.96f),
                new Color(0.47f,0.27f,0.15f),
                new Color(0.45f,0.51f,0.58f),
                new Color(0.05f,0.69f,0.72f)
            };

            for (int i = 0; i < colors.Length; i++)
                AddColorButton(gridRect, colors[i]);
        }

        private void CreateBrickTray(RectTransform parent)
        {
            RectTransform tray = PanelRect(
                parent,
                "BrickTray",
                new Vector2(0.091f, 0.006f),
                new Vector2(0.939f, 0.148f),
                Panel);

            HorizontalLayoutGroup layout = tray.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < BrickCatalog.Specs.Length; i++)
                AddBrickButton(tray, BrickCatalog.Specs[i]);
        }

        private void CreateCameraTools(RectTransform parent)
        {
            RectTransform panel = PanelRect(
                parent,
                "CameraTools",
                new Vector2(0.943f, 0.158f),
                new Vector2(0.996f, 0.898f),
                Panel);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(7, 7, 12, 12);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AddToolbarButton(panel, "Templates", "icon_templates", 58f, ShowTemplateOverlay);
            AddToolbarButton(panel, "ResetCamera", "icon_reset", 58f, manager.ResetCamera);
            AddToolbarButton(panel, "ZoomIn", "icon_zoom_in", 58f, manager.ZoomIn);
            AddToolbarButton(panel, "ZoomOut", "icon_zoom_out", 58f, manager.ZoomOut);
        }

        private void CreateWorkspaceFrame(RectTransform parent)
        {
            Color border = new Color(0.21f, 0.28f, 0.38f, 1f);
            float thickness = 4f;

            MakeFrameLine(parent, "FrameTop",
                new Vector2(0.091f, 0.898f), new Vector2(0.939f, 0.898f),
                new Vector2(0f, -thickness), new Vector2(0f, 0f), border);
            MakeFrameLine(parent, "FrameBottom",
                new Vector2(0.091f, 0.152f), new Vector2(0.939f, 0.152f),
                new Vector2(0f, 0f), new Vector2(0f, thickness), border);
            MakeFrameLine(parent, "FrameLeft",
                new Vector2(0.091f, 0.152f), new Vector2(0.091f, 0.898f),
                new Vector2(0f, 0f), new Vector2(thickness, 0f), border);
            MakeFrameLine(parent, "FrameRight",
                new Vector2(0.939f, 0.152f), new Vector2(0.939f, 0.898f),
                new Vector2(-thickness, 0f), new Vector2(0f, 0f), border);
        }

        private void CreateToast(RectTransform parent)
        {
            GameObject toast = new GameObject(
                "Toast",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup));

            toast.transform.SetParent(parent, false);
            RectTransform rt = toast.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.515f, 0.82f);
            rt.anchorMax = new Vector2(0.515f, 0.82f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(76f, 76f);

            Image background = toast.GetComponent<Image>();
            background.sprite = RoundedSprite();
            background.type = Image.Type.Sliced;
            background.color = new Color(0.05f, 0.07f, 0.10f, 0.92f);
            background.raycastTarget = false;

            toastGroup = toast.GetComponent<CanvasGroup>();
            toastGroup.alpha = 0f;
            toastGroup.blocksRaycasts = false;
            toastGroup.interactable = false;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(toast.transform, false);
            toastIcon = iconObject.GetComponent<Image>();
            toastIcon.raycastTarget = false;
            Stretch(toastIcon.rectTransform, 17f);
        }

        private void CreateClearOverlay(RectTransform parent)
        {
            clearOverlay = CreateModalBackdrop(parent, "ClearOverlay");
            clearOverlay.SetActive(false);

            RectTransform panel = ModalPanel(clearOverlay.transform, 300f, 205f);
            Image bigIcon = ModalIcon(panel, "icon_clear", new Vector2(82f, 82f), new Vector2(0f, 32f));
            bigIcon.color = new Color(1f, 0.37f, 0.33f, 1f);

            ButtonVisual cancel = FloatingButton(panel, "Cancel", "icon_close", new Vector2(-48f, -62f), 66f, HideClearOverlay);
            ButtonVisual confirm = FloatingButton(panel, "Confirm", "icon_check", new Vector2(48f, -62f), 66f, ConfirmClear);
            cancel.background.color = PanelMid;
            confirm.background.color = Danger;
        }

        private void CreateTemplateOverlay(RectTransform parent)
        {
            templateOverlay = CreateModalBackdrop(parent, "TemplateOverlay");
            templateOverlay.SetActive(false);

            RectTransform panel = ModalPanel(templateOverlay.transform, 690f, 245f);
            string[] icons = { "icon_house", "icon_car", "icon_robot", "icon_tower" };

            for (int i = 0; i < icons.Length; i++)
            {
                int templateIndex = i;
                float x = -225f + i * 150f;

                ButtonVisual visual = FloatingButton(
                    panel,
                    "Template" + i,
                    icons[i],
                    new Vector2(x, 0f),
                    118f,
                    delegate
                    {
                        manager.LoadTemplate(templateIndex);
                        HideTemplateOverlay();
                    });

                visual.background.color = PanelMid;
            }

            ButtonVisual close = FloatingButton(
                panel,
                "CloseTemplates",
                "icon_close",
                new Vector2(306f, 91f),
                48f,
                HideTemplateOverlay);

            close.background.color = new Color(0.16f, 0.18f, 0.23f, 1f);
        }

        private void ShowClearOverlay() { clearOverlay.SetActive(true); }
        private void HideClearOverlay() { clearOverlay.SetActive(false); }

        private void ConfirmClear()
        {
            clearOverlay.SetActive(false);
            manager.ClearAll(true);
        }

        private void ShowTemplateOverlay() { templateOverlay.SetActive(true); }
        private void HideTemplateOverlay() { templateOverlay.SetActive(false); }

        private IEnumerator ToastRoutine(string iconName, Color color)
        {
            toastIcon.sprite = LoadSprite(iconName);
            toastIcon.color = color;
            toastGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(0.55f);

            float time = 0f;
            while (time < 0.35f)
            {
                time += Time.unscaledDeltaTime;
                toastGroup.alpha = 1f - Mathf.Clamp01(time / 0.35f);
                yield return null;
            }

            toastGroup.alpha = 0f;
            toastRoutine = null;
        }

        private ButtonVisual AddToolbarButton(
            Transform parent,
            string name,
            string iconName,
            float size,
            UnityAction action)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement),
                typeof(PressScale));

            buttonObject.transform.SetParent(parent, false);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = size;
            layout.preferredHeight = size;
            layout.minWidth = size;
            layout.minHeight = size;

            Image background = buttonObject.GetComponent<Image>();
            background.sprite = RoundedSprite();
            background.type = Image.Type.Sliced;
            background.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = LoadSprite(iconName);
            icon.color = IconNormal;
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            Stretch(icon.rectTransform, size * 0.22f);

            return new ButtonVisual
            {
                button = button,
                background = background,
                icon = icon,
                id = name
            };
        }

        private void AddBrickButton(Transform parent, BrickSpec spec)
        {
            string idLocal = spec.id;

            GameObject buttonObject = new GameObject(
                "Brick_" + idLocal,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement),
                typeof(PressScale));

            buttonObject.transform.SetParent(parent, false);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 112f;
            layout.preferredHeight = 98f;
            layout.minWidth = 112f;
            layout.minHeight = 98f;

            Image background = buttonObject.GetComponent<Image>();
            background.sprite = RoundedSprite();
            background.type = Image.Type.Sliced;
            background.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(delegate { manager.SetBrick(idLocal); });

            GameObject iconObject = new GameObject("BrickIcon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = BrickSprite(idLocal);
            icon.color = new Color(0.76f, 0.83f, 0.91f, 1f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            Stretch(icon.rectTransform, 6f);

            brickButtons[idLocal] = new ButtonVisual
            {
                button = button,
                background = background,
                icon = icon,
                id = idLocal
            };
        }

        private void AddColorButton(Transform parent, Color color)
        {
            Color localColor = color;

            GameObject buttonObject = new GameObject(
                "Color",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(PressScale));

            buttonObject.transform.SetParent(parent, false);

            Image outer = buttonObject.GetComponent<Image>();
            outer.sprite = CircleSprite();
            outer.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = outer;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(delegate { manager.SetColor(localColor); });

            GameObject innerObject = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            innerObject.transform.SetParent(buttonObject.transform, false);
            Image inner = innerObject.GetComponent<Image>();
            inner.sprite = CircleSprite();
            inner.color = localColor;
            inner.raycastTarget = false;
            Stretch(inner.rectTransform, 5f);

            colorButtons.Add(new ColorVisual
            {
                button = button,
                outer = outer,
                inner = inner,
                color = localColor
            });
        }

        private ButtonVisual FloatingButton(
            Transform parent,
            string name,
            string iconName,
            Vector2 position,
            float size,
            UnityAction action)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(PressScale));

            buttonObject.transform.SetParent(parent, false);

            RectTransform rt = buttonObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = position;

            Image background = buttonObject.GetComponent<Image>();
            background.sprite = RoundedSprite();
            background.type = Image.Type.Sliced;
            background.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = LoadSprite(iconName);
            icon.color = IconNormal;
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            Stretch(icon.rectTransform, size * 0.20f);

            return new ButtonVisual
            {
                button = button,
                background = background,
                icon = icon,
                id = name
            };
        }

        private GameObject CreateModalBackdrop(RectTransform parent, string name)
        {
            GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            RectTransform rt = overlay.GetComponent<RectTransform>();
            Stretch(rt, 0f);

            Image image = overlay.GetComponent<Image>();
            image.color = new Color(0.015f, 0.020f, 0.030f, 0.72f);
            image.raycastTarget = true;
            return overlay;
        }

        private RectTransform ModalPanel(Transform parent, float width, float height)
        {
            GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform rt = panelObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);

            Image image = panelObject.GetComponent<Image>();
            image.sprite = RoundedSprite();
            image.type = Image.Type.Sliced;
            image.color = Panel;
            return rt;
        }

        private Image ModalIcon(RectTransform parent, string iconName, Vector2 size, Vector2 position)
        {
            GameObject iconObject = new GameObject("ModalIcon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(parent, false);

            Image image = iconObject.GetComponent<Image>();
            image.sprite = LoadSprite(iconName);
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rt = image.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return image;
        }

        private RectTransform PanelRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform rt = panelObject.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(4f, 4f);
            rt.offsetMax = new Vector2(-4f, -4f);

            Image image = panelObject.GetComponent<Image>();
            image.sprite = RoundedSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            return rt;
        }

        private void MakeFrameLine(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            GameObject lineObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            lineObject.transform.SetParent(parent, false);

            RectTransform rt = lineObject.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image image = lineObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private Image AddLayoutImage(Transform parent, string name, Sprite sprite, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            LayoutElement layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.minWidth = width;
            layout.minHeight = height;

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private RectTransform AddLayoutRect(Transform parent, string name, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            LayoutElement layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.minWidth = width;
            layout.minHeight = height;
            return go.GetComponent<RectTransform>();
        }

        private void AddSpacer(Transform parent, float width, float flexibleWidth)
        {
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(parent, false);

            LayoutElement layout = spacer.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.flexibleWidth = flexibleWidth;
        }

        private void AddFlexibleSpace(Transform parent)
        {
            AddSpacer(parent, 0f, 1f);
        }

        private Sprite BrickSprite(string id)
        {
            return LoadSprite("brick_" + id.Replace("x", "_"));
        }

        private Sprite RoundedSprite()
        {
            return LoadSprite("ui_roundrect", true);
        }

        private Sprite CircleSprite()
        {
            return LoadSprite("ui_circle");
        }

        private Sprite LoadSprite(string name)
        {
            return LoadSprite(name, false);
        }

        private Sprite LoadSprite(string name, bool sliced)
        {
            string key = name + (sliced ? "_sliced" : "");
            Sprite sprite;

            if (spriteCache.TryGetValue(key, out sprite))
                return sprite;

            Texture2D texture = Resources.Load<Texture2D>("UI/" + name);
            if (texture == null) return null;

            Vector4 border = sliced
                ? new Vector4(28f, 28f, 28f, 28f)
                : Vector4.zero;

            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border);

            sprite.name = name;
            spriteCache[key] = sprite;
            return sprite;
        }

        private void SetEnabled(ButtonVisual visual, bool enabled)
        {
            if (visual == null) return;

            visual.button.interactable = enabled;
            visual.background.color = enabled
                ? PanelMid
                : new Color(0.08f, 0.10f, 0.13f, 0.70f);
            visual.icon.color = enabled ? IconNormal : IconMuted;
        }

        private float ColorDistance(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }

        private void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
