using System.Collections.Generic;
using UnityEngine;

namespace BrickKids3D
{
    public static class BoardFactory
    {
        public static GameObject CreateBaseplate(int halfSize)
        {
            int size = halfSize * 2;
            List<CombineInstance> parts = new List<CombineInstance>(1 + size * size);

            CombineInstance body = new CombineInstance();
            body.mesh = PrimitiveMeshLibrary.Cube;
            body.transform = Matrix4x4.TRS(
                new Vector3(0f, -0.12f, 0f),
                Quaternion.identity,
                new Vector3(size, 0.22f, size));
            parts.Add(body);

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    CombineInstance stud = new CombineInstance();
                    stud.mesh = PrimitiveMeshLibrary.Cylinder;
                    stud.transform = Matrix4x4.TRS(
                        new Vector3(-halfSize + 0.5f + x, 0.035f, -halfSize + 0.5f + z),
                        Quaternion.identity,
                        new Vector3(0.38f, 0.052f, 0.38f));
                    parts.Add(stud);
                }
            }

            Mesh mesh = PrimitiveMeshLibrary.Combine(parts, "BrickKids_BaseplateMesh");
            GameObject board = new GameObject("BuildBoard");

            MeshFilter filter = board.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = board.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = BrickMaterialLibrary.Plastic;
            BrickMaterialLibrary.SetColor(renderer, new Color(0.74f, 0.81f, 0.86f, 1f));

            BoxCollider collider = board.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, -0.12f, 0f);
            collider.size = new Vector3(size, 0.22f, size);

            BoardMeshOwner owner = board.AddComponent<BoardMeshOwner>();
            owner.mesh = mesh;
            return board;
        }

        public static GameObject CreateStudioFloor()
        {
            GameObject floor = new GameObject("StudioFloor");
            MeshFilter filter = floor.AddComponent<MeshFilter>();
            filter.sharedMesh = PrimitiveMeshLibrary.Cube;
            MeshRenderer renderer = floor.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = BrickMaterialLibrary.Plastic;
            BrickMaterialLibrary.SetColor(renderer, new Color(0.91f, 0.94f, 0.96f, 1f));
            floor.transform.position = new Vector3(0f, -0.36f, 0f);
            floor.transform.localScale = new Vector3(34f, 0.08f, 34f);
            return floor;
        }
    }

    public class BoardMeshOwner : MonoBehaviour
    {
        public Mesh mesh;

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }
    }
}
