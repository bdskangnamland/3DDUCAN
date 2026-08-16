using System;

namespace BrickKids3D
{
    public enum ItemCategory
    {
        Bricks = 0,
        Primitives = 1,
        Building = 2,
        Furniture = 3,
        Roads = 4,
        Nature = 5,
        Vehicles = 6,
        Characters = 7,
        Props = 8,
        Materials = 9
    }

    public enum MaterialStyle
    {
        GlossyPlastic = 0,
        MattePlastic = 1,
        Metal = 2,
        Chrome = 3,
        Wood = 4,
        Concrete = 5,
        Brick = 6,
        Stone = 7,
        Glass = 8,
        Mirror = 9
    }

    public enum ItemVisual
    {
        Brick,
        Cube,
        Cylinder,
        Sphere,
        Cone,
        Slab,
        Column,
        Beam,
        Stair,
        Door,
        Window,
        GlassPanel,
        Mirror,
        Fence,
        Roof,
        Wall,
        Arch,
        Chair,
        Table,
        Sofa,
        Bed,
        Cabinet,
        Shelf,
        Desk,
        RoadStraight,
        RoadCorner,
        Crosswalk,
        Sidewalk,
        Parking,
        Grass,
        Sand,
        TreeRound,
        TreePine,
        Palm,
        Bush,
        Flower,
        Rock,
        Water,
        Car,
        Truck,
        Bus,
        Bicycle,
        Motorcycle,
        Lamp,
        Bench,
        PersonAdult,
        PersonChild,
        PersonWorker,
        PersonCasual,
        TrafficLight,
        RoadSign,
        TrashBin,
        Hydrant,
        Planter,
        Umbrella,
        Bollard
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
        public bool usesSelectedMaterial;

        public BrickSpec(
            string id,
            int width,
            int depth,
            int heightLayers,
            ItemCategory category,
            ItemVisual visual,
            bool groundOnly,
            bool usesPalette,
            bool isSurface,
            bool usesSelectedMaterial)
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
            this.usesSelectedMaterial = usesSelectedMaterial;
        }
    }

    public static class BrickCatalog
    {
        public static readonly BrickSpec[] Specs =
        {
            // Building bricks
            S("1x1",1,1,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("1x2",1,2,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("1x3",1,3,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("1x4",1,4,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("1x6",1,6,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("2x2",2,2,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("2x3",2,3,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("2x4",2,4,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("2x6",2,6,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),
            S("2x8",2,8,1,ItemCategory.Bricks,ItemVisual.Brick,false,true,false,true),

            // Basic 3D primitives
            S("cube",2,2,2,ItemCategory.Primitives,ItemVisual.Cube,false,true,false,true),
            S("cylinder",2,2,3,ItemCategory.Primitives,ItemVisual.Cylinder,false,true,false,true),
            S("sphere",2,2,3,ItemCategory.Primitives,ItemVisual.Sphere,false,true,false,true),
            S("cone",2,2,3,ItemCategory.Primitives,ItemVisual.Cone,false,true,false,true),
            S("slab",4,4,1,ItemCategory.Primitives,ItemVisual.Slab,false,true,false,true),
            S("column",1,1,6,ItemCategory.Primitives,ItemVisual.Column,false,true,false,true),
            S("beam",1,6,1,ItemCategory.Primitives,ItemVisual.Beam,false,true,false,true),
            S("stair",4,4,4,ItemCategory.Primitives,ItemVisual.Stair,false,true,false,true),

            // Architecture
            S("door",2,1,5,ItemCategory.Building,ItemVisual.Door,false,true,false,true),
            S("window",2,1,4,ItemCategory.Building,ItemVisual.Window,false,true,false,true),
            S("glass",2,1,4,ItemCategory.Building,ItemVisual.GlassPanel,false,false,false,false),
            S("mirror",2,1,4,ItemCategory.Building,ItemVisual.Mirror,false,false,false,false),
            S("fence",4,1,3,ItemCategory.Building,ItemVisual.Fence,true,true,false,true),
            S("roof",4,2,2,ItemCategory.Building,ItemVisual.Roof,false,true,false,true),
            S("wall",4,1,5,ItemCategory.Building,ItemVisual.Wall,false,true,false,true),
            S("arch",4,1,5,ItemCategory.Building,ItemVisual.Arch,false,true,false,true),

            // Furniture
            S("chair",2,2,3,ItemCategory.Furniture,ItemVisual.Chair,true,true,false,true),
            S("table",3,4,3,ItemCategory.Furniture,ItemVisual.Table,true,true,false,true),
            S("sofa",3,5,3,ItemCategory.Furniture,ItemVisual.Sofa,true,true,false,true),
            S("bed",4,6,2,ItemCategory.Furniture,ItemVisual.Bed,true,true,false,true),
            S("cabinet",3,2,6,ItemCategory.Furniture,ItemVisual.Cabinet,true,true,false,true),
            S("shelf",3,1,6,ItemCategory.Furniture,ItemVisual.Shelf,true,true,false,true),
            S("desk",3,5,3,ItemCategory.Furniture,ItemVisual.Desk,true,true,false,true),

            // Roads / terrain surfaces
            S("road_straight",4,6,1,ItemCategory.Roads,ItemVisual.RoadStraight,true,false,true,false),
            S("road_corner",6,6,1,ItemCategory.Roads,ItemVisual.RoadCorner,true,false,true,false),
            S("crosswalk",4,4,1,ItemCategory.Roads,ItemVisual.Crosswalk,true,false,true,false),
            S("sidewalk",2,6,1,ItemCategory.Roads,ItemVisual.Sidewalk,true,false,true,false),
            S("parking",6,6,1,ItemCategory.Roads,ItemVisual.Parking,true,false,true,false),
            S("grass",4,4,1,ItemCategory.Roads,ItemVisual.Grass,true,false,true,false),
            S("sand",4,4,1,ItemCategory.Roads,ItemVisual.Sand,true,false,true,false),

            // Nature
            S("tree_round",2,2,7,ItemCategory.Nature,ItemVisual.TreeRound,true,false,false,false),
            S("tree_pine",2,2,8,ItemCategory.Nature,ItemVisual.TreePine,true,false,false,false),
            S("palm",2,2,8,ItemCategory.Nature,ItemVisual.Palm,true,false,false,false),
            S("bush",2,2,2,ItemCategory.Nature,ItemVisual.Bush,true,false,false,false),
            S("flower",1,1,2,ItemCategory.Nature,ItemVisual.Flower,true,false,false,false),
            S("rock",2,2,2,ItemCategory.Nature,ItemVisual.Rock,true,false,false,false),
            S("water",4,4,1,ItemCategory.Nature,ItemVisual.Water,true,false,true,false),

            // Vehicles / street furniture
            S("car",2,4,2,ItemCategory.Vehicles,ItemVisual.Car,true,true,false,true),
            S("truck",2,6,3,ItemCategory.Vehicles,ItemVisual.Truck,true,true,false,true),
            S("bus",2,8,3,ItemCategory.Vehicles,ItemVisual.Bus,true,true,false,true),
            S("bicycle",1,3,2,ItemCategory.Vehicles,ItemVisual.Bicycle,true,true,false,true),
            S("motorcycle",1,3,2,ItemCategory.Vehicles,ItemVisual.Motorcycle,true,true,false,true),
            S("lamp",1,1,7,ItemCategory.Vehicles,ItemVisual.Lamp,true,false,false,false),
            S("bench",1,3,2,ItemCategory.Vehicles,ItemVisual.Bench,true,true,false,true),

            // Characters
            S("person_adult",1,1,5,ItemCategory.Characters,ItemVisual.PersonAdult,true,true,false,true),
            S("person_child",1,1,4,ItemCategory.Characters,ItemVisual.PersonChild,true,true,false,true),
            S("person_worker",1,1,5,ItemCategory.Characters,ItemVisual.PersonWorker,true,true,false,true),
            S("person_casual",1,1,5,ItemCategory.Characters,ItemVisual.PersonCasual,true,true,false,true),

            // Props / city objects
            S("traffic_light",1,1,7,ItemCategory.Props,ItemVisual.TrafficLight,true,false,false,false),
            S("road_sign",1,1,6,ItemCategory.Props,ItemVisual.RoadSign,true,false,false,false),
            S("trash_bin",1,1,2,ItemCategory.Props,ItemVisual.TrashBin,true,true,false,true),
            S("hydrant",1,1,2,ItemCategory.Props,ItemVisual.Hydrant,true,true,false,true),
            S("planter",2,2,2,ItemCategory.Props,ItemVisual.Planter,true,true,false,true),
            S("umbrella",2,2,5,ItemCategory.Props,ItemVisual.Umbrella,true,true,false,true),
            S("bollard",1,1,2,ItemCategory.Props,ItemVisual.Bollard,true,true,false,true)
        };

        private static BrickSpec S(
            string id,
            int width,
            int depth,
            int heightLayers,
            ItemCategory category,
            ItemVisual visual,
            bool groundOnly,
            bool usesPalette,
            bool isSurface,
            bool usesSelectedMaterial)
        {
            return new BrickSpec(
                id,
                width,
                depth,
                heightLayers,
                category,
                visual,
                groundOnly,
                usesPalette,
                isSurface,
                usesSelectedMaterial);
        }

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
            if (category == ItemCategory.Materials)
                return new BrickSpec[0];

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
