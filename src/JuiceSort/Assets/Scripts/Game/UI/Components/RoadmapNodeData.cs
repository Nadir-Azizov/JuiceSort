using UnityEngine;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Level state for roadmap display.
    /// </summary>
    public enum RoadmapLevelState
    {
        Locked,
        Current,
        Completed
    }

    /// <summary>
    /// View data for a single island node on the roadmap.
    /// Built from IProgressionManager data, NOT stored — computed each time the roadmap opens.
    /// </summary>
    public class RoadmapNodeData
    {
        public int LevelNumber;
        public RoadmapLevelState State;
        public int StarsEarned;         // 0-3
        public bool IsBoss;             // true for every 10th level
        public Sprite IslandSprite;     // resolved sprite reference
        public bool FlipX;              // whether to flip horizontally
        public bool IsPreview;          // true for faded next-zone preview islands
        public Vector2 WorldPosition;   // calculated position in world space
    }
}
