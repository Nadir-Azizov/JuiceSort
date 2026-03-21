namespace JuiceSort.Core
{
    /// <summary>
    /// Save/load service interface. Uses string (JSON) to avoid Core→Game dependency.
    /// </summary>
    public interface ISaveManager
    {
        void Save(string json);
        string LoadJson();
        bool HasSave();
        void DeleteSave();
    }
}
