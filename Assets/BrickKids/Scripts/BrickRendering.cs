using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public static class BrickMaterialLibrary
    {
        private static Material plastic;
        private static Material matte;
        private static Material road;
        private static Material foliage;
        private static Material wood;
        private static Material ghost;
        private static Material glass;
        private static Material mirror;
        private static Material water;

        public static Material Plastic
        {
            get
            {
                if (plastic == null)
                {
                    plastic = MakeOpaque("BrickKids_Plastic_Runtime", 0.03f, 0.68f);
                }
                return plastic;
            }
        }

        public static Material Matte
        {
            get
            {
                if (matte == null)
                {
                    matte = MakeOpaque("BrickKids_Matte_Runtime", 0.0f, 0.20f);
                }
                return matte;
            }
        }

        public static Material Road
        {
            get
            {
                if (road == null)
                {
                    road = MakeOpaque("BrickKids_Road_Runtime", 0.0f, 0.10f);
                }
                return road;
            }
        }

        public static Material Foliage
        {
            get
            {
                if (foliage == null)
                {
                    foliage = MakeOpaque("BrickKids_Foliage_Runtime", 0.0f, 0.25f);
                }
                return foliage;
            }
        }

        public static Material Wood
        {
            get
            {
                if (wood == null)
                {
                    wood = MakeOpaque("BrickKids_Wood_Runtime", 0.0f, 0.30f);
                }
                return wood;
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

        private static Material MakeOpaque(string name, float metallic, float smoothness)
        {
            Shader shader = Resources.Load<Shader>("Shaders/Plastic");
            if (shader == null) shader = Shader.Find("Specular");
            Material material = new Material(shader);
            material.name = name;
            material.enableInstancing = true;
            material.hideFlags = HideFlags.DontSave;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
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
