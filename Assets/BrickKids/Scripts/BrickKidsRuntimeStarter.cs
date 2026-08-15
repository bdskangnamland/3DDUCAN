using UnityEngine;

namespace BrickKids3D
{
    public static class BrickKidsRuntimeStarter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartBrickKids()
        {
            // Do not depend on a MonoBehaviour being serialized into the build scene.
            // GitHub builds a clean empty scene, then this method starts the game.
            if (Object.FindObjectOfType<BuildManager>() != null)
                return;

            var oldBootstrap = Object.FindObjectOfType<BrickKidsBootstrap>();
            if (oldBootstrap == null)
            {
                var go = new GameObject("BrickKidsBootstrap_Runtime");
                go.AddComponent<BrickKidsBootstrap>();
            }
            else
            {
                oldBootstrap.EnsureWorld();
            }
        }
    }
}
