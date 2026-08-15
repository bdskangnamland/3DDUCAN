using UnityEngine;

namespace BrickKids3D
{
    public static class BrickKidsRuntimeStarter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartBrickKids()
        {
            if (Object.FindObjectOfType<BuildManager>() != null) return;

            BrickKidsBootstrap existing = Object.FindObjectOfType<BrickKidsBootstrap>();
            if (existing == null)
            {
                GameObject go = new GameObject("BrickKidsBootstrap_Runtime");
                go.AddComponent<BrickKidsBootstrap>();
            }
            else
            {
                existing.EnsureWorld();
            }
        }
    }
}
