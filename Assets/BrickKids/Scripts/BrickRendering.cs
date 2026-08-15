using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public static class BrickMaterialLibrary
    {
        private static Material plastic;
        private static Material ghost;

        public static Material Plastic
        {
            get
            {
                if (plastic == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Plastic");
                    if (shader == null) shader = Shader.Find("Unlit/Color");
                    plastic = new Material(shader);
                    plastic.name = "BrickKids_Plastic_Runtime";
                    plastic.enableInstancing = true;
                    plastic.hideFlags = HideFlags.DontSave;
                }
                return plastic;
            }
        }

        public static Material Ghost
        {
            get
            {
                if (ghost == null)
                {
                    Shader shader = Resources.Load<Shader>("Shaders/Ghost");
                    if (shader == null) shader = Shader.Find("Unlit/Transparent");
                    ghost = new Material(shader);
                    ghost.name = "BrickKids_Ghost_Runtime";
                    ghost.enableInstancing = true;
                    ghost.hideFlags = HideFlags.DontSave;
                }
                return ghost;
            }
        }

        public static void SetColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }
    }

    public static class PrimitiveMeshLibrary
    {
        private static Mesh cube;
        private static Mesh cylinder;

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
