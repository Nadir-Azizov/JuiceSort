using UnityEngine;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Attached to each island GameObject to identify its level number for tap detection.
    /// </summary>
    public class RoadmapIslandMarker : MonoBehaviour
    {
        public int LevelNumber;
        public RoadmapLevelState State;
    }
}
