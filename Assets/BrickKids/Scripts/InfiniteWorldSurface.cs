using UnityEngine;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public class InfiniteWorldSurface : MonoBehaviour
    {
        public Transform focus;
        public OrbitCamera orbitCamera;

        private const int ChunkSize = 16;
        private const int Radius = 3;

        private Mesh chunkMesh;
        private GameObject[] chunks;
        private Renderer[] chunkRenderers;
        private GameObject farFloor;
        private Renderer farFloorRenderer;

        private int lastCenterX = int.MinValue;
        private int lastCenterZ = int.MinValue;
        private Color groundColor = new Color(0.52f, 0.58f, 0.62f);

        public void Build()
        {
            chunkMesh = CreateChunkMesh();

            int side = Radius * 2 + 1;
            chunks = new GameObject[side * side];
            chunkRenderers = new Renderer[chunks.Length];

            int index = 0;
            for (int z = -Radius; z <= Radius; z++)
            {
                for (int x = -Radius; x <= Radius; x++)
                {
                    GameObject chunk = new GameObject("InfiniteBaseplateChunk");
                    chunk.transform.SetParent(transform, false);

                    MeshFilter filter = chunk.AddComponent<MeshFilter>();
                    filter.sharedMesh = chunkMesh;

                    MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = BrickMaterialLibrary.Matte;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = true;
                    BrickMaterialLibrary.SetSurface(renderer, groundColor, 0.28f, 0f);

                    chunks[index] = chunk;
                    chunkRenderers[index] = renderer;
                    index++;
                }
            }

            farFloor = new GameObject("InfiniteFarFloor");
            farFloor.transform.SetParent(transform, false);

            MeshFilter farFilter = farFloor.AddComponent<MeshFilter>();
            farFilter.sharedMesh = PrimitiveMeshLibrary.Cube;

            farFloorRenderer = farFloor.AddComponent<MeshRenderer>();
            farFloorRenderer.sharedMaterial = BrickMaterialLibrary.Matte;
            farFloorRenderer.shadowCastingMode = ShadowCastingMode.Off;
            farFloorRenderer.receiveShadows = true;
            BrickMaterialLibrary.SetSurface(farFloorRenderer, groundColor, 0.20f, 0f);

            RefreshChunkPositions(true);
        }

        private void LateUpdate()
        {
            RefreshChunkPositions(false);
            RefreshFarFloor();
        }

        public void SetGroundColor(Color color)
        {
            groundColor = color;

            if (chunkRenderers != null)
            {
                for (int i = 0; i < chunkRenderers.Length; i++)
                {
                    BrickMaterialLibrary.SetSurface(
                        chunkRenderers[i],
                        groundColor,
                        0.28f,
                        0f);
                }
            }

            if (farFloorRenderer != null)
            {
                BrickMaterialLibrary.SetSurface(
                    farFloorRenderer,
                    groundColor,
                    0.20f,
                    0f);
            }
        }

        private void RefreshChunkPositions(bool force)
        {
            if (chunks == null || focus == null) return;

            int centerX = Mathf.FloorToInt(focus.position.x / ChunkSize);
            int centerZ = Mathf.FloorToInt(focus.position.z / ChunkSize);

            if (!force && centerX == lastCenterX && centerZ == lastCenterZ)
                return;

            lastCenterX = centerX;
            lastCenterZ = centerZ;

            int index = 0;
            for (int z = -Radius; z <= Radius; z++)
            {
                for (int x = -Radius; x <= Radius; x++)
                {
                    int chunkX = centerX + x;
                    int chunkZ = centerZ + z;

                    chunks[index].transform.position = new Vector3(
                        chunkX * ChunkSize + ChunkSize * 0.5f,
                        0f,
                        chunkZ * ChunkSize + ChunkSize * 0.5f);
                    index++;
                }
            }
        }

        private void RefreshFarFloor()
        {
            if (farFloor == null || focus == null) return;

            float distance = orbitCamera != null ? orbitCamera.distance : 20f;
            float size = Mathf.Max(1200f, distance * 14f);

            farFloor.transform.position = new Vector3(
                focus.position.x,
                -0.28f,
                focus.position.z);
            farFloor.transform.localScale = new Vector3(size, 0.10f, size);
        }

        private Mesh CreateChunkMesh()
        {
            System.Collections.Generic.List<CombineInstance> parts =
                new System.Collections.Generic.List<CombineInstance>(1 + ChunkSize * ChunkSize);

            CombineInstance body = new CombineInstance();
            body.mesh = PrimitiveMeshLibrary.Cube;
            body.transform = Matrix4x4.TRS(
                new Vector3(0f, -0.12f, 0f),
                Quaternion.identity,
                new Vector3(ChunkSize, 0.22f, ChunkSize));
            parts.Add(body);

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    CombineInstance stud = new CombineInstance();
                    stud.mesh = PrimitiveMeshLibrary.LowPolyStud;
                    stud.transform = Matrix4x4.TRS(
                        new Vector3(
                            -ChunkSize * 0.5f + 0.5f + x,
                            0.035f,
                            -ChunkSize * 0.5f + 0.5f + z),
                        Quaternion.identity,
                        new Vector3(0.38f, 0.10f, 0.38f));
                    parts.Add(stud);
                }
            }

            return PrimitiveMeshLibrary.Combine(parts, "BrickKids_InfiniteBaseplateChunk");
        }

        private void OnDestroy()
        {
            if (chunkMesh != null)
            {
                Destroy(chunkMesh);
                chunkMesh = null;
            }
        }
    }
}
