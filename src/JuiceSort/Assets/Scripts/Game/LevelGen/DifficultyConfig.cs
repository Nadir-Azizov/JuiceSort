using UnityEngine;

namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// ScriptableObject holding difficulty scaling parameters.
    /// Editable in Unity Inspector for playtesting balance.
    /// Create asset: Right-click in Project > Create > JuiceSort > DifficultyConfig
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "JuiceSort/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Color Scaling")]
        [SerializeField] private int _baseColorCount = 3;
        [SerializeField] private int _colorsPerLevelStep = 20;
        [SerializeField] private int _maxColors = 5;

        [Header("Slot Scaling")]
        [SerializeField] private int _baseSlotCount = 4;
        [SerializeField] private int _slotsPerLevelStep = 100;
        [SerializeField] private int _maxSlots = 6;

        [Header("Empty Containers")]
        [SerializeField] private int _emptyContainerCount = 1;

        [Header("Shuffle")]
        [SerializeField] private int _shuffleMultiplier = 4;

        public int BaseColorCount => _baseColorCount;
        public int ColorsPerLevelStep => _colorsPerLevelStep;
        public int MaxColors => _maxColors;
        public int BaseSlotCount => _baseSlotCount;
        public int SlotsPerLevelStep => _slotsPerLevelStep;
        public int MaxSlots => _maxSlots;
        public int EmptyContainerCount => _emptyContainerCount;
        public int ShuffleMultiplier => _shuffleMultiplier;
    }
}
