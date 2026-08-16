using System;

namespace BrickKids3D
{
    public enum ItemCategory
    {
        Bricks = 0,
        Building = 1,
        Roads = 2,
        Nature = 3,
        Vehicles = 4
    }

    public enum ItemVisual
    {
        Brick,
        Door,
        Window,
        GlassPanel,
        Mirror,
        Fence,
        Roof,
        RoadStraight,
        RoadCorner,
        Crosswalk,
        Sidewalk,
        Parking,
        TreeRound,
        TreePine,
        Bush,
        Flower,
        Rock,
        Water,
        Car,
        Truck,
        Bus,
        Lamp,
        Bench
    }

    [Serializable]
    public struct BrickSpec
    {
        public string id;
        public int width;
        public int depth;
        public int heightLayers;
        public ItemCategory category;
        public ItemVisual visual;
        public bool groundOnly;
        public bool usesPalette;
        public bool isSurface;

        public BrickSpec(
            string id,
            int width,
            int depth,
            int heightLayers,
            ItemCategory category,
            ItemVisual visual,
            bool groundOnly,
            bool usesPalette,
            bool isSurface)
        {
            this.id = id;
            this.width = width;
            this.depth = depth;
            this.heightLayers = Math.Max(1, heightLayers);
            this.category = category;
            this.visual = visual;
            this.groundOnly = groundOnly;
            this.usesPalette = usesPalette;
            this.isSurface = isSurface;
        }
    }

    public static class BrickCatalog
    {
        public static readonly BrickSpec[] Specs =
        {
            // Bricks
            new BrickSpec("1x1", 1, 1, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("1x2", 1, 2, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("1x3", 1, 3, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("1x4", 1, 4, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("1x6", 1, 6, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("2x2", 2, 2, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("2x3", 2, 3, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("2x4", 2, 4, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("2x6", 2, 6, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),
            new BrickSpec("2x8", 2, 8, 1, ItemCategory.Bricks, ItemVisual.Brick, false, true, false),

            // Building
            new BrickSpec("door", 2, 1, 5, ItemCategory.Building, ItemVisual.Door, false, true, false),
            new BrickSpec("window", 2, 1, 4, ItemCategory.Building, ItemVisual.Window, false, true, false),
            new BrickSpec("glass", 2, 1, 4, ItemCategory.Building, ItemVisual.GlassPanel, false, false, false),
            new BrickSpec("mirror", 2, 1, 4, ItemCategory.Building, ItemVisual.Mirror, false, false, false),
            new BrickSpec("fence", 4, 1, 3, ItemCategory.Building, ItemVisual.Fence, true, true, false),
            new BrickSpec("roof", 4, 2, 2, ItemCategory.Building, ItemVisual.Roof, false, true, false),

            // Roads / hardscape. These are visual surface layers and do not block cars/trees/buildings above them.
            new BrickSpec("road_straight", 4, 6, 1, ItemCategory.Roads, ItemVisual.RoadStraight, true, false, true),
            new BrickSpec("road_corner", 6, 6, 1, ItemCategory.Roads, ItemVisual.RoadCorner, true, false, true),
            new BrickSpec("crosswalk", 4, 4, 1, ItemCategory.Roads, ItemVisual.Crosswalk, true, false, true),
            new BrickSpec("sidewalk", 2, 6, 1, ItemCategory.Roads, ItemVisual.Sidewalk, true, false, true),
            new BrickSpec("parking", 6, 6, 1, ItemCategory.Roads, ItemVisual.Parking, true, false, true),

            // Nature
            new BrickSpec("tree_round", 2, 2, 7, ItemCategory.Nature, ItemVisual.TreeRound, true, false, false),
            new BrickSpec("tree_pine", 2, 2, 8, ItemCategory.Nature, ItemVisual.TreePine, true, false, false),
            new BrickSpec("bush", 2, 2, 2, ItemCategory.Nature, ItemVisual.Bush, true, false, false),
            new BrickSpec("flower", 1, 1, 2, ItemCategory.Nature, ItemVisual.Flower, true, false, false),
            new BrickSpec("rock", 2, 2, 2, ItemCategory.Nature, ItemVisual.Rock, true, false, false),
            new BrickSpec("water", 4, 4, 1, ItemCategory.Nature, ItemVisual.Water, true, false, true),

            // Vehicles / street furniture
            new BrickSpec("car", 2, 4, 2, ItemCategory.Vehicles, ItemVisual.Car, true, true, false),
            new BrickSpec("truck", 2, 6, 3, ItemCategory.Vehicles, ItemVisual.Truck, true, true, false),
            new BrickSpec("bus", 2, 8, 3, ItemCategory.Vehicles, ItemVisual.Bus, true, true, false),
            new BrickSpec("lamp", 1, 1, 7, ItemCategory.Vehicles, ItemVisual.Lamp, true, false, false),
            new BrickSpec("bench", 1, 3, 2, ItemCategory.Vehicles, ItemVisual.Bench, true, true, false)
        };

        public static BrickSpec Get(string id)
        {
            for (int i = 0; i < Specs.Length; i++)
                if (Specs[i].id == id) return Specs[i];

            return Specs[7];
        }

        public static bool Contains(string id)
        {
            for (int i = 0; i < Specs.Length; i++)
                if (Specs[i].id == id) return true;
            return false;
        }

        public static BrickSpec[] ForCategory(ItemCategory category)
        {
            int count = 0;
            for (int i = 0; i < Specs.Length; i++)
                if (Specs[i].category == category) count++;

            BrickSpec[] result = new BrickSpec[count];
            int index = 0;
            for (int i = 0; i < Specs.Length; i++)
                if (Specs[i].category == category) result[index++] = Specs[i];

            return result;
        }
    }
}
