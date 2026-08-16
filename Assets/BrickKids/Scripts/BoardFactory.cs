using System.Collections.Generic;
using UnityEngine;

namespace BrickKids3D
{
    public static class BoardFactory
    {
        public static BoardController CreateBaseplate(int halfSize)
        {
            GameObject board = new GameObject("BuildBoard");
            BoardController controller = board.AddComponent<BoardController>();
            controller.Resize(halfSize);
            return controller;
        }

        public static GameObject CreateStudioFloor()
        {
            GameObject floor = new GameObject("StudioFloor");
            MeshFilter filter = floor.AddComponent<MeshFilter>();
            filter.sharedMesh = PrimitiveMeshLibrary.Cube;
            MeshRenderer renderer = floor.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = BrickMaterialLibrary.Plastic;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            BrickMaterialLibrary.SetColor(renderer, new Color(0.88f, 0.92f, 0.95f, 1f));
            floor.transform.position = new Vector3(0f, -0.42f, 0f);
            floor.transform.localScale = new Vector3(100f, 0.10f, 100f);
            return floor;
        }
    }

    public class BoardController : MonoBehaviour
    {
        public int HalfSize { get; private set; }

        private MeshFilter filter;
        private MeshRenderer meshRenderer;
        private BoxCollider boxCollider;
        private Mesh generatedMesh;

        private void EnsureComponents()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();

            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();

            meshRenderer.sharedMaterial = BrickMaterialLibrary.Plastic;
            meshRenderer.receiveShadows = true;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        public void Resize(int halfSize)
        {
            HalfSize = Mathf.Max(8, halfSize);
            EnsureComponents();

            if (generatedMesh != null)
            {
                Destroy(generatedMesh);
                generatedMesh = null;
            }

            generatedMesh = BuildMesh(HalfSize);
            filter.sharedMesh = generatedMesh;
            BrickMaterialLibrary.SetColor(meshRenderer, new Color(0.71f, 0.80f, 0.86f, 1f));

            int size = HalfSize * 2;
            boxCollider.center = new Vector3(0f, -0.10f, 0f);
            boxCollider.size = new Vector3(size, 0.24f, size);
        }

        private Mesh BuildMesh(int halfSize)
        {
            int size = halfSize * 2;
            List<CombineInstance> parts = new List<CombineInstance>(1 + size * size);

            CombineInstance body = new CombineInstance();
            body.mesh = PrimitiveMeshLibrary.Cube;
            body.transform = Matrix4x4.TRS(
                new Vector3(0f, -0.11f, 0f),
                Quaternion.identity,
                new Vector3(size, 0.22f, size));
            parts.Add(body);

            Mesh studMesh = PrimitiveMeshLibrary.LowPolyStud;
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    CombineInstance stud = new CombineInstance();
                    stud.mesh = studMesh;
                    stud.transform = Matrix4x4.TRS(
                        new Vector3(
                            -halfSize + 0.5f + x,
                            0.035f,
                            -halfSize + 0.5f + z),
                        Quaternion.identity,
                        new Vector3(0.38f, 0.08f, 0.38f));
                    parts.Add(stud);
                }
            }

            return PrimitiveMeshLibrary.Combine(parts, "BrickKids_Baseplate_" + size + "x" + size);
        }

        private void OnDestroy()
        {
            if (generatedMesh != null) Destroy(generatedMesh);
        }
    }
}
