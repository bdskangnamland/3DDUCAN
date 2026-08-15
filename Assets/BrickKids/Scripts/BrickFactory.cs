using System.Collections.Generic;
using UnityEngine;

namespace BrickKids3D
{
    public static class BrickFactory
    {
        public const float GridUnit = 1f;
        public const float BrickHeight = 0.62f;
        private const float Gap = 0.045f;

        public static BrickPiece Create(string id, int gx, int gy, int gz, int rotationStep, Color color, bool preview, Transform parent)
        {
            BrickSpec spec = BrickCatalog.Get(id);
            int rs = ((rotationStep % 4) + 4) % 4;
            int w = rs % 2 == 0 ? spec.width : spec.depth;
            int d = rs % 2 == 0 ? spec.depth : spec.width;

            List<CombineInstance> parts = new List<CombineInstance>(1 + w * d);

            CombineInstance body = new CombineInstance();
            body.mesh = PrimitiveMeshLibrary.Cube;
            body.transform = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.identity,
                new Vector3(w * GridUnit - Gap, BrickHeight - 0.045f, d * GridUnit - Gap));
            parts.Add(body);

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    CombineInstance stud = new CombineInstance();
                    stud.mesh = PrimitiveMeshLibrary.Cylinder;
                    stud.transform = Matrix4x4.TRS(
                        new Vector3(
                            -w * 0.5f + 0.5f + x,
                            BrickHeight * 0.5f + 0.055f,
                            -d * 0.5f + 0.5f + z),
                        Quaternion.identity,
                        new Vector3(0.44f, 0.055f, 0.44f));
                    parts.Add(stud);
                }
            }

            Mesh mesh = PrimitiveMeshLibrary.Combine(parts, "BrickMesh_" + id);

            GameObject root = new GameObject("Brick_" + id);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(
                gx + w * 0.5f,
                gy * BrickHeight + BrickHeight * 0.5f,
                gz + d * 0.5f);

            MeshFilter filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = preview ? BrickMaterialLibrary.Ghost : BrickMaterialLibrary.Plastic;
            Color visualColor = color;
            if (preview) visualColor.a = 0.46f;
            BrickMaterialLibrary.SetColor(renderer, visualColor);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(w * GridUnit - Gap, BrickHeight - 0.03f, d * GridUnit - Gap);
            collider.enabled = !preview;

            BrickPiece piece = root.AddComponent<BrickPiece>();
            piece.Configure(id, gx, gy, gz, rs, color, preview, mesh);
            return piece;
        }

        public static void Move(BrickPiece piece, int gx, int gy, int gz)
        {
            piece.SetGridPosition(gx, gy, gz);
            piece.transform.position = new Vector3(
                gx + piece.Width * 0.5f,
                gy * BrickHeight + BrickHeight * 0.5f,
                gz + piece.Depth * 0.5f);
        }
    }
}
