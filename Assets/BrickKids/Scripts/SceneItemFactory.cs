using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public static class SceneItemFactory
    {
        private static readonly Color RoadColor = new Color(0.12f, 0.14f, 0.17f);
        private static readonly Color Concrete = new Color(0.63f, 0.66f, 0.68f);
        private static readonly Color White = new Color(0.94f, 0.95f, 0.96f);
        private static readonly Color Yellow = new Color(0.96f, 0.72f, 0.08f);
        private static readonly Color Trunk = new Color(0.34f, 0.19f, 0.09f);
        private static readonly Color Green = new Color(0.12f, 0.53f, 0.20f);
        private static readonly Color DarkGreen = new Color(0.06f, 0.37f, 0.14f);
        private static readonly Color GlassBlue = new Color(0.58f, 0.83f, 0.96f, 0.38f);
        private static readonly Color MirrorSilver = new Color(0.70f, 0.78f, 0.85f);
        private static readonly Color Tire = new Color(0.035f, 0.04f, 0.05f);
        private static readonly Color Metal = new Color(0.34f, 0.38f, 0.42f);

        public static BrickPiece Create(
            string id,
            int gx,
            int gy,
            int gz,
            int rotationStep,
            Color paletteColor,
            bool preview,
            Transform parent)
        {
            BrickSpec spec = BrickCatalog.Get(id);
            int rs = ((rotationStep % 4) + 4) % 4;
            int worldWidth = rs % 2 == 0 ? spec.width : spec.depth;
            int worldDepth = rs % 2 == 0 ? spec.depth : spec.width;

            GameObject root = new GameObject("Item_" + id);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(
                gx + worldWidth * 0.5f,
                gy * BrickFactory.BrickHeight,
                gz + worldDepth * 0.5f);
            root.transform.rotation = Quaternion.Euler(0f, rs * 90f, 0f);

            BuildVisual(root.transform, spec, paletteColor, preview);

            float physicalHeight = spec.isSurface
                ? 0.12f
                : Mathf.Max(
                    0.18f,
                    spec.heightLayers * BrickFactory.BrickHeight);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, physicalHeight * 0.5f, 0f);
            collider.size = new Vector3(
                Mathf.Max(0.25f, spec.width - 0.05f),
                physicalHeight,
                Mathf.Max(0.25f, spec.depth - 0.05f));
            collider.enabled = !preview;

            BrickPiece piece = root.AddComponent<BrickPiece>();
            piece.Configure(
                id,
                gx,
                gy,
                gz,
                rs,
                paletteColor,
                preview,
                null);

            return piece;
        }

        private static void BuildVisual(
            Transform root,
            BrickSpec spec,
            Color palette,
            bool preview)
        {
            if (preview)
            {
                BuildPreview(root, spec);
                return;
            }

            switch (spec.visual)
            {
                case ItemVisual.Door:
                    BuildDoor(root, palette);
                    break;
                case ItemVisual.Window:
                    BuildWindow(root, palette);
                    break;
                case ItemVisual.GlassPanel:
                    BuildGlass(root);
                    break;
                case ItemVisual.Mirror:
                    BuildMirror(root);
                    break;
                case ItemVisual.Fence:
                    BuildFence(root, palette);
                    break;
                case ItemVisual.Roof:
                    BuildRoof(root, palette);
                    break;
                case ItemVisual.RoadStraight:
                    BuildRoadStraight(root);
                    break;
                case ItemVisual.RoadCorner:
                    BuildRoadCorner(root);
                    break;
                case ItemVisual.Crosswalk:
                    BuildCrosswalk(root);
                    break;
                case ItemVisual.Sidewalk:
                    BuildSidewalk(root);
                    break;
                case ItemVisual.Parking:
                    BuildParking(root);
                    break;
                case ItemVisual.TreeRound:
                    BuildTreeRound(root);
                    break;
                case ItemVisual.TreePine:
                    BuildTreePine(root);
                    break;
                case ItemVisual.Bush:
                    BuildBush(root);
                    break;
                case ItemVisual.Flower:
                    BuildFlower(root);
                    break;
                case ItemVisual.Rock:
                    BuildRock(root);
                    break;
                case ItemVisual.Water:
                    BuildWater(root);
                    break;
                case ItemVisual.Car:
                    BuildCar(root, palette);
                    break;
                case ItemVisual.Truck:
                    BuildTruck(root, palette);
                    break;
                case ItemVisual.Bus:
                    BuildBus(root, palette);
                    break;
                case ItemVisual.Lamp:
                    BuildLamp(root);
                    break;
                case ItemVisual.Bench:
                    BuildBench(root, palette);
                    break;
                default:
                    BuildPreview(root, spec);
                    break;
            }
        }

        private static void BuildPreview(Transform root, BrickSpec spec)
        {
            float h = Mathf.Max(
                spec.isSurface ? 0.10f : 0.22f,
                spec.heightLayers * BrickFactory.BrickHeight);

            Part(
                root,
                "Preview",
                PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.5f, 0f),
                new Vector3(
                    Mathf.Max(0.25f, spec.width - 0.08f),
                    h,
                    Mathf.Max(0.25f, spec.depth - 0.08f)),
                Quaternion.identity,
                BrickMaterialLibrary.Ghost,
                new Color(0.18f, 0.92f, 0.42f, 0.46f),
                0.42f,
                0f,
                false);
        }

        private static void BuildDoor(Transform root, Color color)
        {
            float h = 5f * BrickFactory.BrickHeight;

            Part(root, "Door", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.5f, 0f),
                new Vector3(1.58f, h - 0.08f, 0.22f),
                Quaternion.identity, BrickMaterialLibrary.Wood,
                color, 0.36f, 0f, true);

            Color frame = Darken(color, 0.32f);
            Part(root, "LeftFrame", PrimitiveMeshLibrary.Cube,
                new Vector3(-0.88f, h * 0.5f, 0f),
                new Vector3(0.16f, h, 0.30f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.24f, 0f, true);

            Part(root, "RightFrame", PrimitiveMeshLibrary.Cube,
                new Vector3(0.88f, h * 0.5f, 0f),
                new Vector3(0.16f, h, 0.30f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.24f, 0f, true);

            Part(root, "TopFrame", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h - 0.09f, 0f),
                new Vector3(1.92f, 0.18f, 0.30f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.24f, 0f, true);

            Part(root, "Knob", PrimitiveMeshLibrary.Sphere,
                new Vector3(0.55f, h * 0.48f, -0.16f),
                new Vector3(0.13f, 0.13f, 0.13f),
                Quaternion.identity, BrickMaterialLibrary.Mirror,
                new Color(0.75f, 0.72f, 0.54f), 0.92f, 0.80f, true);
        }

        private static void BuildWindow(Transform root, Color color)
        {
            float h = 4f * BrickFactory.BrickHeight;
            Color frame = color;

            Part(root, "Glass", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.55f, 0f),
                new Vector3(1.52f, h * 0.64f, 0.08f),
                Quaternion.identity, BrickMaterialLibrary.Glass,
                GlassBlue, 0.92f, 0.05f, false);

            float frameW = 0.15f;
            Part(root, "FrameL", PrimitiveMeshLibrary.Cube,
                new Vector3(-0.87f, h * 0.55f, 0f),
                new Vector3(frameW, h * 0.78f, 0.22f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                frame, 0.58f, 0.02f, true);
            Part(root, "FrameR", PrimitiveMeshLibrary.Cube,
                new Vector3(0.87f, h * 0.55f, 0f),
                new Vector3(frameW, h * 0.78f, 0.22f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                frame, 0.58f, 0.02f, true);
            Part(root, "FrameT", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.94f, 0f),
                new Vector3(1.88f, frameW, 0.22f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                frame, 0.58f, 0.02f, true);
            Part(root, "FrameB", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.16f, 0f),
                new Vector3(1.88f, frameW, 0.22f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                frame, 0.58f, 0.02f, true);
            Part(root, "Middle", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.55f, -0.01f),
                new Vector3(0.11f, h * 0.68f, 0.24f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                frame, 0.58f, 0.02f, true);
        }

        private static void BuildGlass(Transform root)
        {
            float h = 4f * BrickFactory.BrickHeight;
            Part(root, "GlassPanel", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.5f, 0f),
                new Vector3(1.90f, h - 0.10f, 0.08f),
                Quaternion.identity, BrickMaterialLibrary.Glass,
                GlassBlue, 0.94f, 0.04f, false);
        }

        private static void BuildMirror(Transform root)
        {
            float h = 4f * BrickFactory.BrickHeight;

            Part(root, "MirrorSurface", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.52f, 0f),
                new Vector3(1.58f, h * 0.76f, 0.07f),
                Quaternion.identity, BrickMaterialLibrary.Mirror,
                MirrorSilver, 0.98f, 0.95f, true);

            Color frame = new Color(0.16f, 0.17f, 0.19f);
            float frameWidth = 0.15f;

            Part(root, "FrameL", PrimitiveMeshLibrary.Cube,
                new Vector3(-0.87f, h * 0.52f, 0.03f),
                new Vector3(frameWidth, h * 0.92f, 0.12f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.30f, 0.2f, true);

            Part(root, "FrameR", PrimitiveMeshLibrary.Cube,
                new Vector3(0.87f, h * 0.52f, 0.03f),
                new Vector3(frameWidth, h * 0.92f, 0.12f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.30f, 0.2f, true);

            Part(root, "FrameT", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.96f, 0.03f),
                new Vector3(1.90f, frameWidth, 0.12f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.30f, 0.2f, true);

            Part(root, "FrameB", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.08f, 0.03f),
                new Vector3(1.90f, frameWidth, 0.12f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                frame, 0.30f, 0.2f, true);
        }

        private static void BuildFence(Transform root, Color color)
        {
            float h = 3f * BrickFactory.BrickHeight;
            for (int i = 0; i < 5; i++)
            {
                float x = -1.8f + i * 0.9f;
                Part(root, "Post", PrimitiveMeshLibrary.Cube,
                    new Vector3(x, h * 0.5f, 0f),
                    new Vector3(0.13f, h, 0.18f),
                    Quaternion.identity, BrickMaterialLibrary.Plastic,
                    color, 0.45f, 0.02f, true);
            }

            Part(root, "RailTop", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.72f, 0f),
                new Vector3(3.85f, 0.16f, 0.17f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                color, 0.45f, 0.02f, true);

            Part(root, "RailBottom", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, h * 0.30f, 0f),
                new Vector3(3.85f, 0.16f, 0.17f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                color, 0.45f, 0.02f, true);
        }

        private static void BuildRoof(Transform root, Color color)
        {
            float h = 2f * BrickFactory.BrickHeight;
            Part(root, "RoofLeft", PrimitiveMeshLibrary.Cube,
                new Vector3(-0.75f, h * 0.55f, 0f),
                new Vector3(2.35f, 0.20f, 1.92f),
                Quaternion.Euler(0f, 0f, 28f),
                BrickMaterialLibrary.Plastic, color, 0.58f, 0.02f, true);

            Part(root, "RoofRight", PrimitiveMeshLibrary.Cube,
                new Vector3(0.75f, h * 0.55f, 0f),
                new Vector3(2.35f, 0.20f, 1.92f),
                Quaternion.Euler(0f, 0f, -28f),
                BrickMaterialLibrary.Plastic, color, 0.58f, 0.02f, true);
        }

        private static void BuildRoadStraight(Transform root)
        {
            RoadBase(root, 4f, 6f);

            for (int i = -2; i <= 2; i++)
            {
                Part(root, "Lane", PrimitiveMeshLibrary.Cube,
                    new Vector3(0f, 0.075f, i * 1.05f),
                    new Vector3(0.10f, 0.025f, 0.65f),
                    Quaternion.identity, BrickMaterialLibrary.Matte,
                    Yellow, 0.22f, 0f, false);
            }
        }

        private static void BuildRoadCorner(Transform root)
        {
            RoadBase(root, 6f, 6f);

            Part(root, "CornerLineA", PrimitiveMeshLibrary.Cube,
                new Vector3(-1.25f, 0.075f, 0.8f),
                new Vector3(0.10f, 0.025f, 3.9f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                Yellow, 0.22f, 0f, false);

            Part(root, "CornerLineB", PrimitiveMeshLibrary.Cube,
                new Vector3(0.80f, 0.075f, -1.25f),
                new Vector3(3.9f, 0.025f, 0.10f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                Yellow, 0.22f, 0f, false);
        }

        private static void BuildCrosswalk(Transform root)
        {
            RoadBase(root, 4f, 4f);

            for (int i = -3; i <= 3; i++)
            {
                Part(root, "Stripe", PrimitiveMeshLibrary.Cube,
                    new Vector3(i * 0.50f, 0.078f, 0f),
                    new Vector3(0.28f, 0.028f, 3.6f),
                    Quaternion.identity, BrickMaterialLibrary.Matte,
                    White, 0.20f, 0f, false);
            }
        }

        private static void BuildSidewalk(Transform root)
        {
            Part(root, "Sidewalk", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.06f, 0f),
                new Vector3(1.95f, 0.12f, 5.95f),
                Quaternion.identity, BrickMaterialLibrary.Matte,
                Concrete, 0.24f, 0f, true);

            for (int i = -2; i <= 2; i++)
            {
                Part(root, "Joint", PrimitiveMeshLibrary.Cube,
                    new Vector3(0f, 0.125f, i * 1.0f),
                    new Vector3(1.75f, 0.010f, 0.025f),
                    Quaternion.identity, BrickMaterialLibrary.Road,
                    new Color(0.40f, 0.42f, 0.44f), 0.12f, 0f, false);
            }
        }

        private static void BuildParking(Transform root)
        {
            RoadBase(root, 6f, 6f);

            for (int i = -2; i <= 2; i++)
            {
                Part(root, "ParkingLine", PrimitiveMeshLibrary.Cube,
                    new Vector3(i * 1.05f, 0.078f, 0f),
                    new Vector3(0.08f, 0.028f, 5.4f),
                    Quaternion.identity, BrickMaterialLibrary.Matte,
                    White, 0.20f, 0f, false);
            }
        }

        private static void RoadBase(Transform root, float w, float d)
        {
            Part(root, "Road", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.045f, 0f),
                new Vector3(w - 0.04f, 0.09f, d - 0.04f),
                Quaternion.identity, BrickMaterialLibrary.Road,
                RoadColor, 0.10f, 0f, true);
        }

        private static void BuildTreeRound(Transform root)
        {
            float trunkH = 2.6f;
            Part(root, "Trunk", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(0f, trunkH * 0.5f, 0f),
                new Vector3(0.45f, trunkH, 0.45f),
                Quaternion.identity, BrickMaterialLibrary.Wood,
                Trunk, 0.28f, 0f, true);

            Part(root, "CrownA", PrimitiveMeshLibrary.Sphere,
                new Vector3(0f, 3.1f, 0f),
                new Vector3(2.0f, 1.75f, 2.0f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                Green, 0.24f, 0f, true);
            Part(root, "CrownB", PrimitiveMeshLibrary.Sphere,
                new Vector3(-0.55f, 3.45f, 0.2f),
                new Vector3(1.25f, 1.15f, 1.25f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                DarkGreen, 0.24f, 0f, true);
            Part(root, "CrownC", PrimitiveMeshLibrary.Sphere,
                new Vector3(0.58f, 3.38f, -0.20f),
                new Vector3(1.20f, 1.10f, 1.20f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                new Color(0.18f, 0.62f, 0.22f), 0.24f, 0f, true);
        }

        private static void BuildTreePine(Transform root)
        {
            Part(root, "Trunk", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(0f, 1.25f, 0f),
                new Vector3(0.40f, 2.5f, 0.40f),
                Quaternion.identity, BrickMaterialLibrary.Wood,
                Trunk, 0.26f, 0f, true);

            for (int i = 0; i < 4; i++)
            {
                float y = 1.35f + i * 0.70f;
                float size = 2.45f - i * 0.38f;
                Part(root, "PineLayer", PrimitiveMeshLibrary.Cone,
                    new Vector3(0f, y + 0.65f, 0f),
                    new Vector3(size, 1.55f, size),
                    Quaternion.identity, BrickMaterialLibrary.Foliage,
                    i % 2 == 0 ? DarkGreen : Green, 0.20f, 0f, true);
            }
        }

        private static void BuildBush(Transform root)
        {
            Part(root, "BushA", PrimitiveMeshLibrary.Sphere,
                new Vector3(-0.38f, 0.48f, 0f),
                new Vector3(1.20f, 0.90f, 1.10f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                Green, 0.22f, 0f, true);
            Part(root, "BushB", PrimitiveMeshLibrary.Sphere,
                new Vector3(0.40f, 0.52f, 0.12f),
                new Vector3(1.15f, 0.95f, 1.10f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                DarkGreen, 0.22f, 0f, true);
        }

        private static void BuildFlower(Transform root)
        {
            Part(root, "Stem", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(0f, 0.45f, 0f),
                new Vector3(0.08f, 0.90f, 0.08f),
                Quaternion.identity, BrickMaterialLibrary.Foliage,
                DarkGreen, 0.20f, 0f, true);

            Color petal = new Color(0.95f, 0.24f, 0.52f);
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.PI * 2f * i / 5f;
                Part(root, "Petal", PrimitiveMeshLibrary.Sphere,
                    new Vector3(Mathf.Cos(a) * 0.20f, 0.95f, Mathf.Sin(a) * 0.20f),
                    new Vector3(0.24f, 0.13f, 0.24f),
                    Quaternion.identity, BrickMaterialLibrary.Plastic,
                    petal, 0.46f, 0.01f, true);
            }

            Part(root, "FlowerCenter", PrimitiveMeshLibrary.Sphere,
                new Vector3(0f, 0.96f, 0f),
                new Vector3(0.20f, 0.16f, 0.20f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                Yellow, 0.48f, 0.01f, true);
        }

        private static void BuildRock(Transform root)
        {
            Part(root, "Rock", PrimitiveMeshLibrary.Sphere,
                new Vector3(0f, 0.55f, 0f),
                new Vector3(1.60f, 1.05f, 1.35f),
                Quaternion.Euler(7f, 20f, -5f),
                BrickMaterialLibrary.Matte,
                new Color(0.42f, 0.45f, 0.48f), 0.16f, 0f, true);
        }

        private static void BuildWater(Transform root)
        {
            Part(root, "Water", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.035f, 0f),
                new Vector3(3.96f, 0.07f, 3.96f),
                Quaternion.identity, BrickMaterialLibrary.Water,
                new Color(0.12f, 0.55f, 0.85f, 0.62f), 0.90f, 0f, false);
        }

        private static void BuildCar(Transform root, Color color)
        {
            VehicleBody(root, color, 3.7f, 1.75f, 0.55f, 0.42f);

            Part(root, "Cabin", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.86f, -0.25f),
                new Vector3(1.55f, 0.62f, 1.65f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                Lighten(color, 0.10f), 0.70f, 0.03f, true);

            Part(root, "FrontGlass", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.92f, -1.10f),
                new Vector3(1.30f, 0.38f, 0.055f),
                Quaternion.Euler(18f, 0f, 0f), BrickMaterialLibrary.Glass,
                GlassBlue, 0.92f, 0.05f, false);

            Wheels(root, 1.72f, 0.77f, 0.38f);
        }

        private static void BuildTruck(Transform root, Color color)
        {
            VehicleBody(root, color, 5.7f, 1.78f, 0.62f, 0.45f);

            Part(root, "Cargo", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 1.10f, 0.85f),
                new Vector3(1.72f, 1.35f, 2.90f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                Lighten(color, 0.16f), 0.55f, 0.02f, true);

            Part(root, "Cab", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.98f, -1.80f),
                new Vector3(1.70f, 1.05f, 1.55f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                color, 0.66f, 0.03f, true);

            Part(root, "TruckGlass", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 1.14f, -2.60f),
                new Vector3(1.36f, 0.45f, 0.06f),
                Quaternion.Euler(10f, 0f, 0f), BrickMaterialLibrary.Glass,
                GlassBlue, 0.92f, 0.05f, false);

            Wheels(root, 2.45f, 0.79f, 0.40f);
            Wheels(root, 0.65f, 0.79f, 0.40f);
            Wheels(root, -1.90f, 0.79f, 0.40f);
        }

        private static void BuildBus(Transform root, Color color)
        {
            VehicleBody(root, color, 7.65f, 1.82f, 0.62f, 0.48f);

            Part(root, "BusBody", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 1.12f, 0f),
                new Vector3(1.78f, 1.45f, 7.15f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                color, 0.62f, 0.03f, true);

            for (int i = -2; i <= 2; i++)
            {
                Part(root, "BusWindowL", PrimitiveMeshLibrary.Cube,
                    new Vector3(-0.91f, 1.35f, i * 1.22f),
                    new Vector3(0.05f, 0.58f, 0.88f),
                    Quaternion.identity, BrickMaterialLibrary.Glass,
                    GlassBlue, 0.92f, 0.05f, false);

                Part(root, "BusWindowR", PrimitiveMeshLibrary.Cube,
                    new Vector3(0.91f, 1.35f, i * 1.22f),
                    new Vector3(0.05f, 0.58f, 0.88f),
                    Quaternion.identity, BrickMaterialLibrary.Glass,
                    GlassBlue, 0.92f, 0.05f, false);
            }

            Wheels(root, 2.80f, 0.81f, 0.41f);
            Wheels(root, -2.55f, 0.81f, 0.41f);
        }

        private static void VehicleBody(
            Transform root,
            Color color,
            float length,
            float width,
            float height,
            float y)
        {
            Part(root, "Chassis", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, y, 0f),
                new Vector3(width, height, length),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                color, 0.66f, 0.03f, true);
        }

        private static void Wheels(Transform root, float z, float x, float radius)
        {
            Quaternion wheelRotation = Quaternion.Euler(0f, 0f, 90f);

            Part(root, "Wheel", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(-x, radius, z),
                new Vector3(radius * 2f, 0.24f, radius * 2f),
                wheelRotation, BrickMaterialLibrary.Road,
                Tire, 0.18f, 0.02f, true);

            Part(root, "Wheel", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(x, radius, z),
                new Vector3(radius * 2f, 0.24f, radius * 2f),
                wheelRotation, BrickMaterialLibrary.Road,
                Tire, 0.18f, 0.02f, true);
        }

        private static void BuildLamp(Transform root)
        {
            Part(root, "Pole", PrimitiveMeshLibrary.LowPolyStud,
                new Vector3(0f, 1.8f, 0f),
                new Vector3(0.12f, 3.6f, 0.12f),
                Quaternion.identity, BrickMaterialLibrary.Mirror,
                Metal, 0.75f, 0.75f, true);

            Part(root, "Arm", PrimitiveMeshLibrary.Cube,
                new Vector3(0.34f, 3.48f, 0f),
                new Vector3(0.70f, 0.10f, 0.10f),
                Quaternion.identity, BrickMaterialLibrary.Mirror,
                Metal, 0.75f, 0.75f, true);

            Part(root, "LampHead", PrimitiveMeshLibrary.Sphere,
                new Vector3(0.70f, 3.40f, 0f),
                new Vector3(0.34f, 0.22f, 0.34f),
                Quaternion.identity, BrickMaterialLibrary.Plastic,
                new Color(1.0f, 0.83f, 0.36f), 0.72f, 0.02f, true);
        }

        private static void BuildBench(Transform root, Color color)
        {
            Color wood = color;

            Part(root, "Seat", PrimitiveMeshLibrary.Cube,
                new Vector3(0f, 0.50f, 0f),
                new Vector3(0.90f, 0.16f, 2.75f),
                Quaternion.identity, BrickMaterialLibrary.Wood,
                wood, 0.32f, 0f, true);

            Part(root, "Back", PrimitiveMeshLibrary.Cube,
                new Vector3(0.38f, 0.92f, 0f),
                new Vector3(0.15f, 0.78f, 2.75f),
                Quaternion.Euler(0f, 0f, -8f), BrickMaterialLibrary.Wood,
                wood, 0.32f, 0f, true);

            for (int z = -1; z <= 1; z += 2)
            {
                Part(root, "Leg", PrimitiveMeshLibrary.Cube,
                    new Vector3(-0.22f, 0.24f, z * 0.95f),
                    new Vector3(0.18f, 0.48f, 0.18f),
                    Quaternion.identity, BrickMaterialLibrary.Mirror,
                    Metal, 0.55f, 0.55f, true);
                Part(root, "Leg", PrimitiveMeshLibrary.Cube,
                    new Vector3(0.22f, 0.24f, z * 0.95f),
                    new Vector3(0.18f, 0.48f, 0.18f),
                    Quaternion.identity, BrickMaterialLibrary.Mirror,
                    Metal, 0.55f, 0.55f, true);
            }
        }

        private static GameObject Part(
            Transform root,
            string name,
            Mesh mesh,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material,
            Color color,
            float smoothness,
            float metallic,
            bool castShadow)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(root, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            MeshFilter filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadow
                ? ShadowCastingMode.On
                : ShadowCastingMode.Off;
            renderer.receiveShadows = true;

            BrickMaterialLibrary.SetSurface(
                renderer,
                color,
                smoothness,
                metallic);

            return part;
        }

        private static Color Darken(Color c, float amount)
        {
            return new Color(
                Mathf.Clamp01(c.r * (1f - amount)),
                Mathf.Clamp01(c.g * (1f - amount)),
                Mathf.Clamp01(c.b * (1f - amount)),
                c.a);
        }

        private static Color Lighten(Color c, float amount)
        {
            return new Color(
                Mathf.Lerp(c.r, 1f, amount),
                Mathf.Lerp(c.g, 1f, amount),
                Mathf.Lerp(c.b, 1f, amount),
                c.a);
        }
    }
}
