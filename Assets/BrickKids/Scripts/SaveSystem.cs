using System.IO;
using UnityEngine;

namespace BrickKids3D
{
    public static class SaveSystem
    {
        private static string PathFor(int slot)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "brickkids_slot_" + Mathf.Clamp(slot, 1, 3) + ".json");
        }

        public static void Save(int slot, BuildSaveData data)
        {
            File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
        }

        public static BuildSaveData Load(int slot)
        {
            string path = PathFor(slot);
            if (!File.Exists(path)) return new BuildSaveData();
            try { return JsonUtility.FromJson<BuildSaveData>(File.ReadAllText(path)) ?? new BuildSaveData(); }
            catch { return new BuildSaveData(); }
        }
    }
}
