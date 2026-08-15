using System.IO;
using UnityEngine;

namespace BrickKids3D
{
    public static class SaveSystem
    {
        private static string PathFor(int slot)
        {
            return System.IO.Path.Combine(
                Application.persistentDataPath,
                "brickkids_slot_" + Mathf.Clamp(slot, 1, 3) + ".json");
        }

        public static bool Exists(int slot)
        {
            return File.Exists(PathFor(slot));
        }

        public static void Save(int slot, BuildSaveData data)
        {
            File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
        }

        public static BuildSaveData Load(int slot)
        {
            string path = PathFor(slot);
            if (!File.Exists(path)) return new BuildSaveData();
            try
            {
                BuildSaveData data = JsonUtility.FromJson<BuildSaveData>(File.ReadAllText(path));
                return data ?? new BuildSaveData();
            }
            catch
            {
                return new BuildSaveData();
            }
        }
    }
}
