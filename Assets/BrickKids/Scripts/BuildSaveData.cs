using System;
using System.Collections.Generic;

namespace BrickKids3D
{
    [Serializable]
    public class BrickRecord
    {
        public string id;
        public int x;
        public int y;
        public int z;
        public int rotation;
        public float r;
        public float g;
        public float b;
        public float a;
        public int materialStyle;
    }

    [Serializable]
    public class BuildSaveData
    {
        public List<BrickRecord> bricks = new List<BrickRecord>();
    }
}
