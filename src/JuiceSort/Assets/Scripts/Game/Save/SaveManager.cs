using System;
using System.IO;
using UnityEngine;
using JuiceSort.Core;

namespace JuiceSort.Game.Save
{
    /// <summary>
    /// Handles JSON save/load to Application.persistentDataPath.
    /// Try-catch at I/O boundaries only.
    /// </summary>
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        private const string FileName = "save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public void Save(string json)
        {
            try
            {
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        public string LoadJson()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return null;

                return File.ReadAllText(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Load failed: {e.Message}");
                return null;
            }
        }

        public bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    Debug.Log("[SaveManager] Save deleted.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Delete failed: {e.Message}");
            }
        }
    }
}
