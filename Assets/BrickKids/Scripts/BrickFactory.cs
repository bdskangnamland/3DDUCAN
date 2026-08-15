using UnityEngine;

namespace BrickKids3D
{
    public static class BrickFactory
    {
        public const float GridUnit = 1f;
        public const float BrickHeight = 0.60f;
        private const float Gap = 0.035f;

        public static BrickPiece Create(string id, int gx, int gy, int gz, int rotationStep, Color color, bool preview, Transform parent)
        {
            BrickSpec baseSpec = BrickCatalog.Get(id);
            int rs = ((rotationStep % 4) + 4) % 4;
            int w = rs % 2 == 0 ? baseSpec.width : baseSpec.depth;
            int d = rs % 2 == 0 ? baseSpec.depth : baseSpec.width;

            var root = new GameObject("Brick_" + id);
            root.transform.SetParent(parent, false);
            var piece = root.AddComponent<BrickPiece>();
            piece.Configure(id, gx, gy, gz, rs, color, preview);

            float cx = gx + w * 0.5f;
            float cz = gz + d * 0.5f;
            float cy = gy * BrickHeight + BrickHeight * 0.5f;
            root.transform.position = new Vector3(cx, cy, cz);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(w * GridUnit - Gap, BrickHeight - Gap, d * GridUnit - Gap);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            ApplyMaterial(body.GetComponent<Renderer>(), color, preview);

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    var stud = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stud.name = "Stud";
                    stud.transform.SetParent(root.transform, false);
                    stud.transform.localPosition = new Vector3(
                        -w * 0.5f + 0.5f + x,
                        BrickHeight * 0.5f + 0.075f,
                        -d * 0.5f + 0.5f + z);
                    stud.transform.localScale = new Vector3(0.46f, 0.075f, 0.46f);
                    Object.DestroyImmediate(stud.GetComponent<Collider>());
                    ApplyMaterial(stud.GetComponent<Renderer>(), color, preview);
                }
            }

            var collider = root.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(w * GridUnit - Gap, BrickHeight - Gap, d * GridUnit - Gap);
            collider.enabled = !preview;
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

        private static void ApplyMaterial(Renderer renderer, Color color, bool transparent)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            Color c = color;
            if (transparent) c.a = 0.48f;
            mat.color = c;

            if (transparent && mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            renderer.material = mat;
        }
    }
}
