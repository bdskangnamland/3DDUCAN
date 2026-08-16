using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public static class BrickMaterialLibrary
    {
        private static Material styled;
        private static Material woodFixed;
        private static Material ghost;
        private static Material glass;
        private static Material mirror;
        private static Material water;

        public static Material Plastic { get { return ForStyle(MaterialStyle.GlossyPlastic); } }
        public static Material Matte { get { return ForStyle(MaterialStyle.MattePlastic); } }
        public static Material Road { get { return ForStyle(MaterialStyle.MattePlastic); } }
        public static Material Foliage { get { return ForStyle(MaterialStyle.MattePlastic); } }
        public static Material Wood
        {
            get
            {
                if (woodFixed == null)
                {
                    Material baseMaterial = ForStyle(MaterialStyle.Wood);
                    woodFixed = new Material(baseMaterial);
                    woodFixed.name = "BrickKids_Wood_Runtime";
                    woodFixed.enableInstancing = true;
                    woodFixed.hideFlags = HideFlags.DontSave;
                    if (woodFixed.HasProperty("_Style")) woodFixed.SetFloat("_Style", (float)MaterialStyle.Wood);
                    if (woodFixed.HasProperty("_Smoothness")) woodFixed.SetFloat("_Smoothness", 0.30f);
                    if (woodFixed.HasProperty("_Metallic")) woodFixed.SetFloat("_Metallic", 0.0f);
                }
                return woodFixed;
            }
        }

        public static Material Ghost
        {
            get
            {
                if (ghost == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Ghost");
                    if (shader == null) shader = Shader.Find("Transparent/Diffuse");
                    ghost = new Material(shader);
                    ghost.name = "BrickKids_Ghost_Runtime";
                    ghost.enableInstancing = true;
                    ghost.hideFlags = HideFlags.DontSave;
                    if (ghost.HasProperty("_Smoothness")) ghost.SetFloat("_Smoothness", 0.45f);
                }
                return ghost;
            }
        }

        public static Material Glass
        {
            get
            {
                if (glass == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Glass");
                    if (shader == null) shader = Shader.Find("Transparent/Diffuse");
                    glass = new Material(shader);
                    glass.name = "BrickKids_Glass_Runtime";
                    glass.enableInstancing = true;
                    glass.hideFlags = HideFlags.DontSave;
                    if (glass.HasProperty("_Metallic")) glass.SetFloat("_Metallic", 0.05f);
                    if (glass.HasProperty("_Smoothness")) glass.SetFloat("_Smoothness", 0.92f);
                }
                return glass;
            }
        }

        public static Material Mirror
        {
            get
            {
                if (mirror == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Mirror");
                    if (shader == null) shader = Shader.Find("Specular");
                    mirror = new Material(shader);
                    mirror.name = "BrickKids_Mirror_Runtime";
                    mirror.enableInstancing = true;
                    mirror.hideFlags = HideFlags.DontSave;
                    if (mirror.HasProperty("_Metallic")) mirror.SetFloat("_Metallic", 0.94f);
                    if (mirror.HasProperty("_Smoothness")) mirror.SetFloat("_Smoothness", 0.96f);
                }
                return mirror;
            }
        }

        public static Material Water
        {
            get
            {
                if (water == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Glass");
                    if (shader == null) shader = Shader.Find("Transparent/Diffuse");
                    water = new Material(shader);
                    water.name = "BrickKids_Water_Runtime";
                    water.enableInstancing = true;
                    water.hideFlags = HideFlags.DontSave;
                    if (water.HasProperty("_Metallic")) water.SetFloat("_Metallic", 0.0f);
                    if (water.HasProperty("_Smoothness")) water.SetFloat("_Smoothness", 0.88f);
                }
                return water;
            }
        }

        public static Material ForStyle(MaterialStyle style)
        {
            if (style == MaterialStyle.Glass) return Glass;
            if (style == MaterialStyle.Mirror) return Mirror;

            if (styled == null)
            {
                Shader shader = Resources.Load<Shader>("Shaders/Styled");
                if (shader == null) shader = Resources.Load<Shader>("Shaders/Plastic");
                if (shader == null) shader = Shader.Find("Specular");
                styled = new Material(shader);
                styled.name = "BrickKids_Styled_Runtime";
                styled.enableInstancing = true;
                styled.hideFlags = HideFlags.DontSave;
            }
            return styled;
        }

        public static void StyleProperties(MaterialStyle style, out float smoothness, out float metallic)
        {
            smoothness = 0.62f;
            metallic = 0.03f;
            switch (style)
            {
                case MaterialStyle.MattePlastic: smoothness = 0.22f; metallic = 0.0f; break;
                case MaterialStyle.Metal: smoothness = 0.56f; metallic = 0.72f; break;
                case MaterialStyle.Chrome: smoothness = 0.94f; metallic = 0.98f; break;
                case MaterialStyle.Wood: smoothness = 0.30f; metallic = 0.0f; break;
                case MaterialStyle.Concrete: smoothness = 0.12f; metallic = 0.0f; break;
                case MaterialStyle.Brick: smoothness = 0.18f; metallic = 0.0f; break;
                case MaterialStyle.Stone: smoothness = 0.24f; metallic = 0.0f; break;
                case MaterialStyle.Glass: smoothness = 0.94f; metallic = 0.04f; break;
                case MaterialStyle.Mirror: smoothness = 0.98f; metallic = 0.96f; break;
            }
        }

        public static void SetStyled(Renderer renderer, Color color, MaterialStyle style)
        {
            if (renderer == null) return;
            float smoothness, metallic;
            StyleProperties(style, out smoothness, out metallic);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Color c = color;
            if (style == MaterialStyle.Glass && c.a > 0.72f) c.a = 0.42f;
            block.SetColor("_Color", c);
            block.SetFloat("_Smoothness", smoothness);
            block.SetFloat("_Metallic", metallic);
            block.SetFloat("_Style", (float)style);
            renderer.SetPropertyBlock(block);
        }

        public static void SetSurface(Renderer renderer, Color color, float smoothness, float metallic)
        {
            if (renderer == null) return;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            block.SetFloat("_Smoothness", smoothness);
            block.SetFloat("_Metallic", metallic);
            renderer.SetPropertyBlock(block);
        }

        public static void SetColor(Renderer renderer, Color color)
        {
            SetSurface(renderer, color, 0.62f, 0.03f);
        }
    }

    public static class PrimitiveMeshLibrary
    {
        private static Mesh cube;
        private static Mesh cylinder;
        private static Mesh sphere;
        private static Mesh lowPolyStud;
        private static Mesh cone;

        public static Mesh Cube
        {
            get
            {
                if (cube == null) cube = GetPrimitiveMesh(PrimitiveType.Cube);
                return cube;
            }
        }

        public static Mesh Cylinder
        {
            get
            {
                if (cylinder == null) cylinder = GetPrimitiveMesh(PrimitiveType.Cylinder);
                return cylinder;
            }
        }

        public static Mesh Sphere
        {
            get
            {
                if (sphere == null) sphere = GetPrimitiveMesh(PrimitiveType.Sphere);
                return sphere;
            }
        }

        public static Mesh LowPolyStud
        {
            get
            {
                if (lowPolyStud == null) lowPolyStud = CreateCylinder(10);
                return lowPolyStud;
            }
        }

        public static Mesh Cone
        {
            get
            {
                if (cone == null) cone = CreateCone(16);
                return cone;
            }
        }

        private static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            GameObject temp = GameObject.CreatePrimitive(type);
            temp.name = "BrickKids_TempMesh_" + type;
            temp.hideFlags = HideFlags.HideAndDontSave;
            temp.SetActive(false);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            return mesh;
        }

        private static Mesh CreateCylinder(int segments)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                float x = Mathf.Cos(angle) * 0.5f;
                float z = Mathf.Sin(angle) * 0.5f;
                vertices.Add(new Vector3(x, -0.5f, z));
                vertices.Add(new Vector3(x, 0.5f, z));
                Vector3 n = new Vector3(x, 0f, z).normalized;
                normals.Add(n);
                normals.Add(n);
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int a = i * 2;
                int b = next * 2;
                int c = a + 1;
                int d = b + 1;

                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }

            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -0.5f, 0f));
            normals.Add(Vector3.down);

            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, 0.5f, 0f));
            normals.Add(Vector3.up);

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(bottomCenter); triangles.Add(next * 2); triangles.Add(i * 2);
                triangles.Add(topCenter); triangles.Add(i * 2 + 1); triangles.Add(next * 2 + 1);
            }

            Mesh mesh = new Mesh();
            mesh.name = "BrickKids_LowPolyCylinder";
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        private static Mesh CreateCone(int segments)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Base ring.
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                vertices.Add(new Vector3(
                    Mathf.Cos(angle) * 0.5f,
                    -0.5f,
                    Mathf.Sin(angle) * 0.5f));
            }

            int tip = vertices.Count;
            vertices.Add(new Vector3(0f, 0.5f, 0f));

            int baseCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -0.5f, 0f));

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(i); triangles.Add(tip); triangles.Add(next);
                triangles.Add(baseCenter); triangles.Add(next); triangles.Add(i);
            }

            Mesh mesh = new Mesh();
            mesh.name = "BrickKids_Cone";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        public static Mesh Combine(IList<CombineInstance> combines, string meshName)
        {
            Mesh mesh = new Mesh();
            mesh.name = meshName;
            mesh.indexFormat = IndexFormat.UInt32;

            CombineInstance[] array = new CombineInstance[combines.Count];
            for (int i = 0; i < combines.Count; i++) array[i] = combines[i];

            mesh.CombineMeshes(array, true, true, false);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
