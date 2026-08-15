using System;
using UnityEngine;

namespace BrickKids3D
{
    [Serializable]
    public struct BrickSpec
    {
        public string id;
        public int width;
        public int depth;

        public BrickSpec(string id, int width, int depth)
        {
            this.id = id;
            this.width = width;
            this.depth = depth;
        }
    }

    public static class BrickCatalog
    {
        public static readonly BrickSpec[] Specs =
        {
            new BrickSpec("1x1", 1, 1),
            new BrickSpec("1x2", 1, 2),
            new BrickSpec("1x3", 1, 3),
            new BrickSpec("1x4", 1, 4),
            new BrickSpec("1x6", 1, 6),
            new BrickSpec("2x2", 2, 2),
            new BrickSpec("2x3", 2, 3),
            new BrickSpec("2x4", 2, 4),
            new BrickSpec("2x6", 2, 6),
            new BrickSpec("2x8", 2, 8)
        };

        public static BrickSpec Get(string id)
        {
            foreach (var spec in Specs)
                if (spec.id == id) return spec;
            return Specs[7];
        }
    }
}
