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
        private readonly Dictionary<string, ButtonVisual> itemButtons = new Dictionary<string, ButtonVisual>();
        private readonly Dictionary<ItemCategory, ButtonVisual> categoryButtons = new Dictionary<ItemCategory, ButtonVisual>();
        private readonly Dictionary<MaterialStyle, ButtonVisual> materialButtons = new Dictionary<MaterialStyle, ButtonVisual>();
        private readonly List<ColorVisual> colorButtons = new List<ColorVisual>();
        private readonly List<ThemeVisual> themeButtons = new List<ThemeVisual>();

        private ItemCategory currentCategory = ItemCategory.Bricks;

        private ButtonVisual deleteButton;
        private ButtonVisual undoButton;
        private ButtonVisual redoButton;
        private ButtonVisual panButton;
        private Image selectedItemImage;
        private Image selectedColorImage;
        private Image selectedMaterialImage;
        private ScrollRect itemLibraryScroll;
        private RectTransform itemLibraryContent;

        private GameObject clearOverlay;
        private GameObject templateOverlay;
        private GameObject themeOverlay;

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

        private class ThemeVisual
        {
            public Button button;
            public Image background;
            public int index;
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
            CreateBottomLibrary(root);
            CreateCameraTools(root);
            CreateWorkspaceFrame(root);
            CreateToast(root);
            CreateClearOverlay(root);
            CreateTemplateOverlay(root);
            CreateThemeOverlay(root);

            SetCategory(ItemCategory.Bricks);
            RefreshState();
        }

        public void RefreshState()
        {
            if (manager == null) return;

            BrickSpec selectedSpec = BrickCatalog.Get(manager.SelectedBrickId);

            foreach (KeyValuePair<string, ButtonVisual> pair in itemButtons)
            {
                BrickSpec spec = BrickCatalog.Get(pair.Key);
                bool selected = pair.Key == manager.SelectedBrickId;
                pair.Value.background.color = selected ? Accent : PanelMid;
                pair.Value.icon.color = spec.visual == ItemVisual.Brick && selected
                    ? manager.SelectedColor
                    : Color.white;
            }

            foreach (KeyValuePair<ItemCategory, ButtonVisual> pair in categoryButtons)
                pair.Value.background.color = pair.Key == currentCategory ? Accent : PanelMid;

            foreach (KeyValuePair<MaterialStyle, ButtonVisual> pair in materialButtons)
                pair.Value.background.color = pair.Key == manager.SelectedMaterialStyle ? Accent : PanelMid;

            for (int i = 0; i < colorButtons.Count; i++)
            {
                ColorVisual visual = colorButtons[i];
                bool selected = ColorDistance(visual.color, manager.SelectedColor) < 0.05f;
                visual.outer.color = selected ? Accent : PanelMid;
                visual.inner.color = visual.color;
            }

            if (deleteButton != null)
                deleteButton.background.color = manager.DeleteMode ? Danger : PanelMid;

            if (panButton != null)
                panButton.background.color = manager.CameraNavigationMode ? Accent : PanelMid;

            SetEnabled(undoButton, manager.CanUndo);
            SetEnabled(redoButton, manager.CanRedo);

            if (selectedItemImage != null)
            {
                selectedItemImage.sprite = ItemSprite(selectedSpec);
                selectedItemImage.color = selectedSpec.visual == ItemVisual.Brick
                    ? manager.SelectedColor
                    : Color.white;
            }

            if (selectedColorImage != null)
                selectedColorImage.color = manager.SelectedColor;

            if (selectedMaterialImage != null)
            {
                selectedMaterialImage.sprite = MaterialSprite(manager.SelectedMaterialStyle);
                selectedMaterialImage.color = Color.white;
            }

            RefreshThemeSelection();
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
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image appMark = AddLayoutImage(bar, "AppMark", LoadSprite("app_mark"), 64f, 64f);
            appMark.preserveAspect = true;
            AddSpacer(bar, 7f, 0f);

            AddToolbarButton(bar, "Rotate", "icon_rotate", 62f, manager.RotateSelected);
            deleteButton = AddToolbarButton(bar, "Delete", "icon_delete", 62f, manager.ToggleDeleteMode);
            undoButton = AddToolbarButton(bar, "Undo", "icon_undo", 62f, manager.Undo);
            redoButton = AddToolbarButton(bar, "Redo", "icon_redo", 62f, manager.Redo);
            AddToolbarButton(bar, "Save", "icon_save", 62f, manager.SaveCurrent);
            AddToolbarButton(bar, "Open", "icon_open", 62f, manager.LoadCurrent);
            AddToolbarButton(bar, "Screenshot", "icon_camera", 62f, manager.CaptureScreenshot);

            ButtonVisual clear = AddToolbarButton(bar, "Clear", "icon_clear", 62f, ShowClearOverlay);
            clear.background.color = new Color(0.17f, 0.12f, 0.15f, 1f);

            AddFlexibleSpace(bar);

            RectTransform itemCard = AddLayoutRect(bar, "SelectedItem", 96f, 64f);
            Image itemBackground = itemCard.gameObject.AddComponent<Image>();
            itemBackground.sprite = RoundedSprite();
            itemBackground.type = Image.Type.Sliced;
            itemBackground.color = PanelMid;
            selectedItemImage = ChildImage(itemCard, "Item", 5f);
            selectedItemImage.preserveAspect = true;

            RectTransform materialCard = AddLayoutRect(bar, "SelectedMaterial", 58f, 58f);
            Image materialBackground = materialCard.gameObject.AddComponent<Image>();
            materialBackground.sprite = RoundedSprite();
            materialBackground.type = Image.Type.Sliced;
            materialBackground.color = PanelMid;
            selectedMaterialImage = ChildImage(materialCard, "Material", 8f);
            selectedMaterialImage.preserveAspect = true;

            RectTransform colorCard = AddLayoutRect(bar, "SelectedColor", 54f, 54f);
            Image outer = colorCard.gameObject.AddComponent<Image>();
            outer.sprite = CircleSprite();
            outer.color = new Color(0.85f, 0.90f, 0.96f, 1f);
            selectedColorImage = ChildImage(colorCard, "Color", 7f);
            selectedColorImage.sprite = CircleSprite();
        }

        private void CreateColorPanel(RectTransform parent)
        {
            RectTransform panel = PanelRect(
                parent,
                "ColorPalette",
                new Vector2(0.004f, 0.205f),
                new Vector2(0.065f, 0.898f),
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
            headerRect.sizeDelta = new Vector2(38f, 38f);
            headerRect.anchoredPosition = new Vector2(0f, -10f);

            RectTransform viewport;
            RectTransform content;
            ScrollRect scroll = CreateScrollArea(
                panel,
                "ColorScroll",
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(7f, 10f),
                new Vector2(-7f, -54f),
                false,
                true,
                out viewport,
                out content);

            VerticalLayoutGroup vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(5, 5, 5, 5);
            vertical.spacing = 8f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = false;
            vertical.childControlHeight = false;
            vertical.childForceExpandWidth = false;
            vertical.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            Color[] colors =
            {
                new Color(0.93f,0.18f,0.13f), new Color(1.00f,0.70f,0.07f),
                new Color(0.06f,0.39f,0.92f), new Color(0.08f,0.70f,0.31f),
                new Color(1.00f,0.42f,0.06f), new Color(0.57f,0.20f,0.78f),
                new Color(0.94f,0.38f,0.67f), new Color(0.08f,0.09f,0.11f),
                new Color(0.94f,0.95f,0.96f), new Color(0.47f,0.27f,0.15f),
                new Color(0.45f,0.51f,0.58f), new Color(0.05f,0.69f,0.72f),
                new Color(0.12f,0.32f,0.16f), new Color(0.55f,0.62f,0.18f),
                new Color(0.30f,0.18f,0.10f), new Color(0.70f,0.73f,0.76f),
                new Color(0.85f,0.66f,0.47f), new Color(0.15f,0.19f,0.25f)
            };

            for (int i = 0; i < colors.Length; i++) AddColorButton(content, colors[i]);
            scroll.verticalNormalizedPosition = 1f;
        }

        private void CreateBottomLibrary(RectTransform parent)
        {
            RectTransform dock = PanelRect(
                parent,
                "BottomLibrary",
                new Vector2(0.068f, 0.006f),
                new Vector2(0.939f, 0.195f),
                Panel);

            // Top row: category icons, horizontally scrollable when more categories are added.
            RectTransform categoryViewport;
            RectTransform categoryContent;
            CreateScrollArea(
                dock,
                "CategoryScroll",
                new Vector2(0f, 0.64f),
                new Vector2(1f, 1f),
                new Vector2(8f, 4f),
                new Vector2(-8f, -4f),
                true,
                false,
                out categoryViewport,
                out categoryContent);

            HorizontalLayoutGroup categoryLayout = categoryContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            categoryLayout.padding = new RectOffset(5, 5, 3, 3);
            categoryLayout.spacing = 7f;
            categoryLayout.childAlignment = TextAnchor.MiddleLeft;
            categoryLayout.childControlWidth = false;
            categoryLayout.childControlHeight = false;
            categoryLayout.childForceExpandWidth = false;
            categoryLayout.childForceExpandHeight = false;

            ContentSizeFitter catFitter = categoryContent.gameObject.AddComponent<ContentSizeFitter>();
            catFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddCategoryButton(categoryContent, ItemCategory.Bricks, "cat_brick");
            AddCategoryButton(categoryContent, ItemCategory.Primitives, "cat_primitive");
            AddCategoryButton(categoryContent, ItemCategory.Building, "cat_building");
            AddCategoryButton(categoryContent, ItemCategory.Furniture, "cat_furniture");
            AddCategoryButton(categoryContent, ItemCategory.Roads, "cat_road");
            AddCategoryButton(categoryContent, ItemCategory.Nature, "cat_nature");
            AddCategoryButton(categoryContent, ItemCategory.Vehicles, "cat_vehicle");
            AddCategoryButton(categoryContent, ItemCategory.Characters, "cat_character");
            AddCategoryButton(categoryContent, ItemCategory.Props, "cat_props");
            AddCategoryButton(categoryContent, ItemCategory.Materials, "cat_material");

            // Bottom row: the actual library. This is the important swipe area.
            RectTransform itemViewport;
            RectTransform itemContent;
            ScrollRect itemScroll = CreateScrollArea(
                dock,
                "ItemScroll",
                new Vector2(0f, 0f),
                new Vector2(1f, 0.63f),
                new Vector2(8f, 7f),
                new Vector2(-8f, -3f),
                true,
                false,
                out itemViewport,
                out itemContent);

            itemLibraryScroll = itemScroll;
            itemLibraryContent = itemContent;

            HorizontalLayoutGroup itemLayout = itemContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            itemLayout.padding = new RectOffset(5, 5, 4, 4);
            itemLayout.spacing = 8f;
            itemLayout.childAlignment = TextAnchor.MiddleLeft;
            itemLayout.childControlWidth = false;
            itemLayout.childControlHeight = false;
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;

            ContentSizeFitter itemFitter = itemContent.gameObject.AddComponent<ContentSizeFitter>();
            itemFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < BrickCatalog.Specs.Length; i++)
                AddItemButton(itemContent, BrickCatalog.Specs[i]);

            foreach (MaterialStyle style in System.Enum.GetValues(typeof(MaterialStyle)))
                AddMaterialButton(itemContent, style);

            itemScroll.horizontalNormalizedPosition = 0f;
        }

        private void CreateCameraTools(RectTransform parent)
        {
            RectTransform panel = PanelRect(
                parent,
                "CameraTools",
                new Vector2(0.943f, 0.205f),
                new Vector2(0.996f, 0.898f),
                Panel);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(7, 7, 9, 9);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AddToolbarButton(panel, "Templates", "icon_templates", 54f, ShowTemplateOverlay);
            panButton = AddToolbarButton(panel, "Pan", "icon_hand", 54f, manager.ToggleCameraNavigationMode);
            AddToolbarButton(panel, "Fit", "icon_fit", 54f, manager.FocusAll);
            AddToolbarButton(panel, "BottomView", "icon_bottom_view", 54f, manager.BottomView);
            AddToolbarButton(panel, "ResetCamera", "icon_reset", 54f, manager.ResetCamera);
            AddToolbarButton(panel, "ZoomIn", "icon_zoom_in", 54f, manager.ZoomIn);
            AddToolbarButton(panel, "ZoomOut", "icon_zoom_out", 54f, manager.ZoomOut);
            AddToolbarButton(panel, "Background", "icon_background", 54f, ShowThemeOverlay);
        }

        private void CreateWorkspaceFrame(RectTransform parent)
        {
            Color border = new Color(0.21f, 0.28f, 0.38f, 1f);
            float t = 4f;

            MakeFrameLine(parent, "FrameTop", new Vector2(0.068f, 0.902f), new Vector2(0.939f, 0.902f), new Vector2(0f, -t), Vector2.zero, border);
            MakeFrameLine(parent, "FrameBottom", new Vector2(0.068f, 0.199f), new Vector2(0.939f, 0.199f), Vector2.zero, new Vector2(0f, t), border);
            MakeFrameLine(parent, "FrameLeft", new Vector2(0.068f, 0.199f), new Vector2(0.068f, 0.902f), Vector2.zero, new Vector2(t, 0f), border);
            MakeFrameLine(parent, "FrameRight", new Vector2(0.939f, 0.199f), new Vector2(0.939f, 0.902f), new Vector2(-t, 0f), Vector2.zero, border);
        }

        private void CreateToast(RectTransform parent)
        {
            GameObject toast = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            toast.transform.SetParent(parent, false);
            RectTransform rt = toast.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.51f, 0.82f);
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

            toastIcon = ChildImage(rt, "Icon", 17f);
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
            string[] icons = { "icon_house", "icon_car", "cat_nature", "icon_tower" };

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

            ButtonVisual close = FloatingButton(panel, "CloseTemplates", "icon_close", new Vector2(306f, 91f), 48f, HideTemplateOverlay);
            close.background.color = new Color(0.16f, 0.18f, 0.23f, 1f);
        }

        private void CreateThemeOverlay(RectTransform parent)
        {
            themeOverlay = CreateModalBackdrop(parent, "ThemeOverlay");
            themeOverlay.SetActive(false);
            RectTransform panel = ModalPanel(themeOverlay.transform, 830f, 210f);

            float startX = -345f;
            ThemeButton(panel, 0, startX, true);
            for (int index = 1; index <= 6; index++) ThemeButton(panel, index, startX + index * 108f, false);

            ButtonVisual close = FloatingButton(panel, "CloseTheme", "icon_close", new Vector2(374f, 76f), 42f, HideThemeOverlay);
            close.background.color = new Color(0.16f, 0.18f, 0.23f, 1f);
        }

        private void ThemeButton(RectTransform parent, int index, float x, bool auto)
        {
            GameObject buttonObject = new GameObject("Theme_" + index, typeof(RectTransform), typeof(Image), typeof(Button), typeof(PressScale));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rt = buttonObject.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(82f, 82f);
            rt.anchoredPosition = new Vector2(x, -2f);

            Image background = buttonObject.GetComponent<Image>();
            background.sprite = CircleSprite();
            background.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            int localIndex = index;
            button.onClick.AddListener(delegate { manager.SetEnvironmentTheme(localIndex); RefreshThemeSelection(); });

            Image inner = ChildImage(rt, "ThemePreview", 10f);
            inner.preserveAspect = true;
            if (auto)
            {
                inner.sprite = LoadSprite("icon_auto");
                inner.color = IconNormal;
            }
            else
            {
                inner.sprite = CircleSprite();
                inner.color = EnvironmentController.ThemeGroundColor(index);
            }

            themeButtons.Add(new ThemeVisual { button = button, background = background, index = index });
        }

        private void SetCategory(ItemCategory category)
        {
            currentCategory = category;

            foreach (KeyValuePair<string, ButtonVisual> pair in itemButtons)
            {
                BrickSpec spec = BrickCatalog.Get(pair.Key);
                pair.Value.button.gameObject.SetActive(category != ItemCategory.Materials && spec.category == category);
            }

            foreach (KeyValuePair<MaterialStyle, ButtonVisual> pair in materialButtons)
                pair.Value.button.gameObject.SetActive(category == ItemCategory.Materials);

            if (itemLibraryScroll != null)
            {
                itemLibraryScroll.velocity = Vector2.zero;
                itemLibraryScroll.horizontalNormalizedPosition = 0f;
            }
            if (itemLibraryContent != null)
                itemLibraryContent.anchoredPosition = new Vector2(0f, itemLibraryContent.anchoredPosition.y);

            RefreshState();
        }

        private void AddCategoryButton(Transform parent, ItemCategory category, string iconName)
        {
            ItemCategory localCategory = category;
            ButtonVisual visual = AddToolbarButton(
                parent,
                "Category_" + category,
                iconName,
                48f,
                delegate { SetCategory(localCategory); });
            categoryButtons[category] = visual;
        }

        private void AddItemButton(Transform parent, BrickSpec spec)
        {
            string idLocal = spec.id;
            GameObject buttonObject = LibraryButtonObject(parent, "Item_" + idLocal, 82f, 82f);
            Image background = buttonObject.GetComponent<Image>();
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(delegate { manager.SetItem(idLocal); });

            Image icon = ChildImage(buttonObject.GetComponent<RectTransform>(), "ItemIcon", 5f);
            icon.sprite = ItemSprite(spec);
            icon.color = Color.white;
            icon.preserveAspect = true;

            itemButtons[idLocal] = new ButtonVisual { button = button, background = background, icon = icon, id = idLocal };
        }

        private void AddMaterialButton(Transform parent, MaterialStyle style)
        {
            MaterialStyle localStyle = style;
            GameObject buttonObject = LibraryButtonObject(parent, "Material_" + style, 82f, 82f);
            Image background = buttonObject.GetComponent<Image>();
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(delegate { manager.SetMaterialStyle(localStyle); });

            Image icon = ChildImage(buttonObject.GetComponent<RectTransform>(), "MaterialIcon", 6f);
            icon.sprite = MaterialSprite(style);
            icon.color = Color.white;
            icon.preserveAspect = true;

            materialButtons[style] = new ButtonVisual { button = button, background = background, icon = icon, id = style.ToString() };
        }

        private GameObject LibraryButtonObject(Transform parent, string name, float width, float height)
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
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.minWidth = width;
            layout.minHeight = height;

            Image background = buttonObject.GetComponent<Image>();
            background.sprite = RoundedSprite();
            background.type = Image.Type.Sliced;
            background.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            return buttonObject;
        }

        private void AddColorButton(Transform parent, Color color)
        {
            Color localColor = color;
            GameObject buttonObject = new GameObject("Color", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(PressScale));
            buttonObject.transform.SetParent(parent, false);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 44f;
            layout.preferredHeight = 44f;
            layout.minWidth = 44f;
            layout.minHeight = 44f;

            Image outer = buttonObject.GetComponent<Image>();
            outer.sprite = CircleSprite();
            outer.color = PanelMid;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = outer;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(delegate { manager.SetColor(localColor); });

            Image inner = ChildImage(buttonObject.GetComponent<RectTransform>(), "Swatch", 5f);
            inner.sprite = CircleSprite();
            inner.color = localColor;

            colorButtons.Add(new ColorVisual { button = button, outer = outer, inner = inner, color = localColor });
        }

        private ScrollRect CreateScrollArea(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            bool horizontal,
            bool vertical,
            out RectTransform viewport,
            out RectTransform content)
        {
            GameObject scrollObject = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = anchorMin;
            scrollRect.anchorMax = anchorMax;
            scrollRect.offsetMin = offsetMin;
            scrollRect.offsetMax = offsetMax;

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            viewport = viewportObject.GetComponent<RectTransform>();
            Stretch(viewport, 0f);
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            Mask mask = viewportObject.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = horizontal ? new Vector2(0f, 0f) : new Vector2(0f, 1f);
            content.anchorMax = horizontal ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            content.pivot = horizontal ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            if (horizontal) content.sizeDelta = new Vector2(0f, 0f);
            else content.sizeDelta = new Vector2(0f, 0f);

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = horizontal;
            scroll.vertical = vertical;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.10f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 34f;
            return scroll;
        }

        private void ShowClearOverlay() { clearOverlay.SetActive(true); }
        private void HideClearOverlay() { clearOverlay.SetActive(false); }
        private void ConfirmClear() { clearOverlay.SetActive(false); manager.ClearAll(true); }
        private void ShowTemplateOverlay() { templateOverlay.SetActive(true); }
        private void HideTemplateOverlay() { templateOverlay.SetActive(false); }
        private void ShowThemeOverlay() { themeOverlay.SetActive(true); RefreshThemeSelection(); }
        private void HideThemeOverlay() { themeOverlay.SetActive(false); }

        private void RefreshThemeSelection()
        {
            int selected = manager != null && manager.environmentController != null
                ? manager.environmentController.ThemeIndex
                : 0;
            for (int i = 0; i < themeButtons.Count; i++)
                themeButtons[i].background.color = themeButtons[i].index == selected ? Accent : PanelMid;
        }

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

        private ButtonVisual AddToolbarButton(Transform parent, string name, string iconName, float size, UnityAction action)
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

            Image icon = ChildImage(buttonObject.GetComponent<RectTransform>(), "Icon", size * 0.22f);
            icon.sprite = LoadSprite(iconName);
            icon.color = IconNormal;
            icon.preserveAspect = true;

            return new ButtonVisual { button = button, background = background, icon = icon, id = name };
        }

        private ButtonVisual FloatingButton(Transform parent, string name, string iconName, Vector2 position, float size, UnityAction action)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(PressScale));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rt = buttonObject.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
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

            Image icon = ChildImage(rt, "Icon", size * 0.20f);
            icon.sprite = LoadSprite(iconName);
            icon.color = IconNormal;
            icon.preserveAspect = true;

            return new ButtonVisual { button = button, background = background, icon = icon, id = name };
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
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
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
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return image;
        }

        private RectTransform PanelRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

        private void MakeFrameLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
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

        private void AddFlexibleSpace(Transform parent) { AddSpacer(parent, 0f, 1f); }

        private Image ChildImage(RectTransform parent, string name, float inset)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            Stretch(image.rectTransform, inset);
            return image;
        }

        private Sprite ItemSprite(BrickSpec spec)
        {
            if (spec.visual == ItemVisual.Brick)
                return LoadSprite("brick_" + spec.id.Replace("x", "_"));
            return LoadSprite("item_" + spec.id);
        }

        private Sprite MaterialSprite(MaterialStyle style)
        {
            switch (style)
            {
                case MaterialStyle.GlossyPlastic: return LoadSprite("mat_gloss");
                case MaterialStyle.MattePlastic: return LoadSprite("mat_matte");
                case MaterialStyle.Metal: return LoadSprite("mat_metal");
                case MaterialStyle.Chrome: return LoadSprite("mat_chrome");
                case MaterialStyle.Wood: return LoadSprite("mat_wood");
                case MaterialStyle.Concrete: return LoadSprite("mat_concrete");
                case MaterialStyle.Brick: return LoadSprite("mat_brick");
                case MaterialStyle.Stone: return LoadSprite("mat_stone");
                case MaterialStyle.Glass: return LoadSprite("mat_glass");
                case MaterialStyle.Mirror: return LoadSprite("mat_mirror");
                default: return LoadSprite("mat_gloss");
            }
        }

        private Sprite RoundedSprite() { return LoadSprite("ui_roundrect", true); }
        private Sprite CircleSprite() { return LoadSprite("ui_circle"); }
        private Sprite LoadSprite(string name) { return LoadSprite(name, false); }

        private Sprite LoadSprite(string name, bool sliced)
        {
            string key = name + (sliced ? "_sliced" : "");
            Sprite sprite;
            if (spriteCache.TryGetValue(key, out sprite)) return sprite;

            Texture2D texture = Resources.Load<Texture2D>("UI/" + name);
            if (texture == null) return null;

            Vector4 border = sliced ? new Vector4(28f, 28f, 28f, 28f) : Vector4.zero;
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
            visual.background.color = enabled ? PanelMid : new Color(0.08f, 0.10f, 0.13f, 0.70f);
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
