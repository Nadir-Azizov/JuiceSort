using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.Audio;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;
using JuiceSort.Game.UI.Screens;
using JuiceSort.Game.Ads;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Manages the gameplay scene. Handles level generation, selection,
    /// pour, win detection, undo, restart, and next level flow.
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        private PuzzleState _currentPuzzle;
        private BottleBoardView _bottleBoard;
        private int _selectedContainerIndex = -1;
        private int _moveCount;
        private bool _isLevelComplete;
        private UndoStack _undoStack;
        private int _currentLevelNumber = 1;
        private LevelDefinition _currentDefinition;
        private int _lastStarRating;
        private bool _isReplay;
        private int _extraBottlesUsed;

        // Paused level state — keeps one level's state for resume
        private PuzzleState _pausedPuzzle;
        private int _pausedLevelNumber = -1;
        private int _pausedMoveCount;
        private int _pausedExtraBottlesUsed;

        private GameplayHUD _hud;
        private GameObject _hudCanvas;
        private BackgroundManager _backgroundManager;
        private bool _isAnimating;

        public int SelectedContainerIndex => _selectedContainerIndex;
        public int MoveCount => _moveCount;
        public bool IsLevelComplete => _isLevelComplete;
        public int UndoRemaining => _undoStack != null ? _undoStack.Count : 0;
        public int CurrentLevelNumber => _currentLevelNumber;
        public int LastStarRating => _lastStarRating;
        public bool IsReplay => _isReplay;
        public int ExtraBottlesRemaining => GameConstants.MaxExtraBottles - _extraBottlesUsed;

        /// <summary>
        /// Starts a new level. Called externally by ScreenManager/Roadmap.
        /// </summary>
        public void StartLevel(int levelNumber)
        {
            _isReplay = false;
            // Clear paused state — starting a new level discards any paused progress
            _pausedPuzzle = null;
            _pausedLevelNumber = -1;
            DestroyBoard();
            LoadLevel(levelNumber);
        }

        /// <summary>
        /// Starts a replay of a completed level. Called externally by Roadmap/GateScreen.
        /// </summary>
        public void StartReplay(int levelNumber)
        {
            if (!Services.TryGet<IProgressionManager>(out var progression))
                return;

            if (!progression.IsLevelCompleted(levelNumber))
            {
                Debug.LogWarning($"[GameplayManager] Cannot replay uncompleted level {levelNumber}");
                return;
            }

            _isReplay = true;
            DestroyBoard();
            LoadLevel(levelNumber);
        }

        private void LoadLevel(int levelNumber)
        {
            _currentLevelNumber = levelNumber;

            // Generate puzzle from difficulty parameters
            _currentDefinition = DifficultyScaler.GetLevelDefinition(levelNumber);
            var definition = _currentDefinition;
            var city = CityAssigner.AssignCity(levelNumber);
            definition.CityName = city.CityName;
            definition.CountryName = city.CountryName;
            definition.Mood = CityAssigner.AssignMood(levelNumber);
            _currentPuzzle = LevelGenerator.Generate(definition);
            ThemeConfig.CurrentMood = definition.Mood;

            // Update music
            if (Services.TryGet<IAudioManager>(out var levelAudio))
                levelAudio.PlayMusic(definition.Mood);

            // Update background
            if (_backgroundManager == null)
                _backgroundManager = BackgroundManager.Create();
            _backgroundManager.SetBackground(definition.CityName ?? "", definition.Mood);

            _moveCount = 0;
            _isLevelComplete = false;
            _selectedContainerIndex = -1;
            _extraBottlesUsed = 0;
            _undoStack = new UndoStack(GameConstants.MaxUndo);
            // _isReplay is set true by LoadSpecificLevel before calling LoadLevel
            // Don't reset here — it's consumed in OnLevelComplete then reset

            // Create board UI
            _bottleBoard = BottleBoardView.Create(_currentPuzzle);
            _bottleBoard.OnContainerTapped += OnContainerTapped;


            _hudCanvas = CreateHUDCanvas();
            _hud = GameplayHUD.Create(_hudCanvas.transform);
            _hud.OnUndoPressed = Undo;
            _hud.OnRestartPressed = RestartLevel;
            _hud.OnExtraBottlePressed = RequestExtraBottle;
            _hud.OnBackPressed = GoBackToRoadmap;
            _hud.SetLevelInfo(levelNumber, definition.CityName ?? "", definition.CountryName ?? "", definition.Mood);
            _hud.UpdateDisplay(0, _undoStack.Count, ExtraBottlesRemaining);

            Debug.Log($"[GameplayManager] Level {levelNumber} loaded: {_currentPuzzle.ContainerCount} containers, {definition.ColorCount} colors, {definition.SlotCount} slots");
        }

        private GameObject CreateHUDCanvas()
        {
            var go = new GameObject("HUDCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return go;
        }

        private void DestroyBoard()
        {
            _isAnimating = false;

            if (_bottleBoard != null)
            {
                _bottleBoard.OnContainerTapped -= OnContainerTapped;
                Destroy(_bottleBoard.gameObject);
                _bottleBoard = null;
            }

            if (_hudCanvas != null)
            {
                Destroy(_hudCanvas);
                _hudCanvas = null;
            }

            _hud = null;
            // Hide overlay screens managed by ScreenManager
            if (Services.TryGet<ScreenManager>(out var screenMgr))
            {
                screenMgr.HideOverlay(GameFlowState.LevelComplete);
                screenMgr.HideOverlay(GameFlowState.Gate);
            }
        }

        public void NextLevel()
        {
            // Check batch gate
            if (Services.TryGet<IProgressionManager>(out var progression))
            {
                if (progression.IsAtBatchGate(_currentLevelNumber) && !progression.CanPassBatchGate())
                {
                    ShowGateScreen(progression);
                    return;
                }
            }

            DestroyBoard();
            LoadLevel(_currentLevelNumber + 1);
        }

        public void RestartLevel()
        {
            if (_isAnimating)
                return;

            // Regenerate same level (same seed = same params/container count)
            // Use RebindPuzzle instead of full board recreation since container count is unchanged
            var definition = DifficultyScaler.GetLevelDefinition(_currentLevelNumber);
            _currentPuzzle = LevelGenerator.Generate(definition);

            _moveCount = 0;
            _isLevelComplete = false;
            _undoStack.Clear();
            DeselectCurrent();
            _bottleBoard.RebindPuzzle(_currentPuzzle);
            _hud?.UpdateDisplay(_moveCount, _undoStack.Count);

            // Hide overlay screens
            if (Services.TryGet<ScreenManager>(out var sm))
            {
                sm.HideOverlay(GameFlowState.LevelComplete);
                sm.HideOverlay(GameFlowState.Gate);
            }

            Debug.Log($"[GameplayManager] Level {_currentLevelNumber} restarted.");
        }

        public void RequestExtraBottle()
        {
            if (_isLevelComplete || _isAnimating)
                return;

            if (_extraBottlesUsed >= GameConstants.MaxExtraBottles)
            {
                Debug.Log("[GameplayManager] Extra bottle limit reached");
                return;
            }

            if (!Services.TryGet<IAdManager>(out var adManager))
                return;

            adManager.ShowRewardedAd(
                onRewarded: () =>
                {
                    _currentPuzzle = PuzzleEngine.AddExtraContainer(_currentPuzzle, _currentDefinition.SlotCount);
                    _extraBottlesUsed++;

                    // Add new container view dynamically
                    var newContainer = _currentPuzzle.GetContainer(_currentPuzzle.ContainerCount - 1);
                    _bottleBoard.AddContainerView(newContainer, _currentPuzzle.ContainerCount - 1);

                    // Clear undo stack — old snapshots have fewer containers
                    _undoStack.Clear();
                    _hud?.UpdateDisplay(_moveCount, _undoStack.Count, ExtraBottlesRemaining);

                    Debug.Log($"[GameplayManager] Extra bottle added. Used: {_extraBottlesUsed}/{GameConstants.MaxExtraBottles}");
                },
                onFailed: () =>
                {
                    Debug.Log("[GameplayManager] Ad failed — no extra bottle");
                }
            );
        }

        /// <summary>
        /// Pauses current level and returns to roadmap. Saves state for resume.
        /// Only one paused level is kept — starting a different level clears it.
        /// </summary>
        public void GoBackToRoadmap()
        {
            if (_isAnimating)
                return;

            if (_currentPuzzle != null && !_isLevelComplete)
            {
                // Save current state for resume
                _pausedPuzzle = _currentPuzzle.Clone();
                _pausedLevelNumber = _currentLevelNumber;
                _pausedMoveCount = _moveCount;
                _pausedExtraBottlesUsed = _extraBottlesUsed;
                Debug.Log($"[GameplayManager] Level {_currentLevelNumber} paused.");
            }

            DestroyBoard();

            if (Services.TryGet<ScreenManager>(out var screenMgr))
                screenMgr.TransitionTo(GameFlowState.Roadmap);
        }

        /// <summary>
        /// Checks if a paused level can be resumed.
        /// </summary>
        public bool HasPausedLevel(int levelNumber)
        {
            return _pausedLevelNumber == levelNumber && _pausedPuzzle != null;
        }

        /// <summary>
        /// Resumes a previously paused level.
        /// </summary>
        public void ResumeLevel(int levelNumber)
        {
            if (!HasPausedLevel(levelNumber))
            {
                StartLevel(levelNumber);
                return;
            }

            _currentLevelNumber = _pausedLevelNumber;
            _currentPuzzle = _pausedPuzzle;
            _moveCount = _pausedMoveCount;
            _extraBottlesUsed = _pausedExtraBottlesUsed;
            _isLevelComplete = false;
            _selectedContainerIndex = -1;
            _undoStack = new UndoStack(GameConstants.MaxUndo);

            // Set up definition for theme/background
            _currentDefinition = DifficultyScaler.GetLevelDefinition(_currentLevelNumber);
            var city = CityAssigner.AssignCity(_currentLevelNumber);
            _currentDefinition.CityName = city.CityName;
            _currentDefinition.CountryName = city.CountryName;
            _currentDefinition.Mood = CityAssigner.AssignMood(_currentLevelNumber);
            ThemeConfig.CurrentMood = _currentDefinition.Mood;

            if (_backgroundManager == null)
                _backgroundManager = BackgroundManager.Create();
            _backgroundManager.SetBackground(_currentDefinition.CityName, _currentDefinition.Mood);

            // Create board from paused state
            _bottleBoard = BottleBoardView.Create(_currentPuzzle);
            _bottleBoard.OnContainerTapped += OnContainerTapped;


            _hudCanvas = CreateHUDCanvas();
            _hud = GameplayHUD.Create(_hudCanvas.transform);
            _hud.OnUndoPressed = Undo;
            _hud.OnRestartPressed = RestartLevel;
            _hud.OnExtraBottlePressed = RequestExtraBottle;
            _hud.OnBackPressed = GoBackToRoadmap;
            _hud.SetLevelInfo(_currentLevelNumber, _currentDefinition.CityName ?? "", _currentDefinition.CountryName ?? "", _currentDefinition.Mood);
            _hud.UpdateDisplay(_moveCount, _undoStack.Count, ExtraBottlesRemaining);

            // Clear paused state
            _pausedPuzzle = null;
            _pausedLevelNumber = -1;

            Debug.Log($"[GameplayManager] Level {_currentLevelNumber} resumed. Moves: {_moveCount}");
        }

        private void ShowGateScreen(IProgressionManager progression)
        {
            if (Services.TryGet<ScreenManager>(out var screenMgr))
            {
                var gateScreen = screenMgr.GetScreen(GameFlowState.Gate)?.GetComponent<UI.Screens.StarGateScreen>();
                if (gateScreen != null)
                {
                    gateScreen.Show(progression);
                    screenMgr.ShowOverlay(GameFlowState.Gate);
                }
            }
        }

        public void OnContainerTapped(int index)
        {
            if (_isLevelComplete || _isAnimating)
                return;

            var containerData = _currentPuzzle.GetContainer(index);

            // Closed bottles ignore all taps
            if (containerData.IsCompleted())
                return;

            if (_selectedContainerIndex < 0)
            {
                if (!containerData.IsEmpty())
                {
                    SelectContainer(index);
                }
            }
            else if (_selectedContainerIndex == index)
            {
                DeselectCurrent();

                if (Services.TryGet<IAudioManager>(out var deselectAudio))
                    deselectAudio.PlaySFX(AudioClipType.Deselect);
            }
            else
            {
                AttemptPour(_selectedContainerIndex, index);
            }
        }

        private void AttemptPour(int sourceIndex, int targetIndex)
        {
            if (!PuzzleEngine.CanPour(_currentPuzzle, sourceIndex, targetIndex))
            {
                // Re-select: if tapped bottle is non-empty and not completed, switch selection to it
                var tappedData = _currentPuzzle.GetContainer(targetIndex);
                if (!tappedData.IsEmpty() && !tappedData.IsCompleted())
                {
                    if (Services.TryGet<IAudioManager>(out var reselectAudio))
                        reselectAudio.PlaySFX(AudioClipType.Deselect);
                    DeselectCurrent();
                    SelectContainer(targetIndex);
                }
                return;
            }

            var source = _currentPuzzle.GetContainer(sourceIndex);
            var target = _currentPuzzle.GetContainer(targetIndex);

            // Capture pour info before executing logic (data mutates after ExecutePour)
            DrinkColor pourColor = source.GetTopColor();
            int available = source.GetTopColorCount();
            int emptySlots = target.SlotCount - target.FilledCount();
            int pourCount = available < emptySlots ? available : emptySlots;
            int sourceTopIndex = source.GetTopIndex();
            int targetFirstEmpty = target.GetFirstEmptyIndex();

            // Execute game logic immediately (data changes now)
            _undoStack.Push(_currentPuzzle.Clone());
            PuzzleEngine.ExecutePour(_currentPuzzle, sourceIndex, targetIndex);
            _moveCount++;

            // Reset source to idle instantly (no deselect animation — pour animation takes over)
            var sourceView = _bottleBoard.GetContainerView(sourceIndex);
            sourceView.ResetVisualState();
            _selectedContainerIndex = -1;

            // Play animation (visuals catch up to data)
            var targetView = _bottleBoard.GetContainerView(targetIndex);
            _isAnimating = true;

            StartCoroutine(PourAnimator.Animate(
                sourceView, targetView, pourCount, pourColor,
                sourceTopIndex, targetFirstEmpty,
                onMidPour: () =>
                {
                    if (Services.TryGet<IAudioManager>(out var pourAudio))
                        pourAudio.PlaySFX(AudioClipType.Pour);
                },
                onComplete: () =>
                {
                    _isAnimating = false;
                    _hud?.UpdateDisplay(_moveCount, _undoStack.Count);
                    Debug.Log($"[GameplayManager] Pour animated. Moves: {_moveCount}");

                    if (_currentPuzzle.IsAllSorted())
                    {
                        OnLevelComplete();
                    }
                }
            ));
        }

        public void Undo()
        {
            if (_isLevelComplete || _isAnimating)
                return;

            var snapshot = _undoStack.Pop();
            if (snapshot == null)
                return;

            _currentPuzzle = snapshot;
            _moveCount--;
            DeselectCurrent();
            _bottleBoard.RebindPuzzle(_currentPuzzle);
            _hud?.UpdateDisplay(_moveCount, _undoStack.Count);

            Debug.Log($"[GameplayManager] Undo. Moves: {_moveCount}, Undos remaining: {_undoStack.Count}");
        }

        private void OnLevelComplete()
        {
            _isLevelComplete = true;

            int estimatedOptimal = OptimalMoveEstimator.Estimate(_currentLevelNumber, _currentDefinition);
            _lastStarRating = StarCalculator.CalculateStars(_moveCount, estimatedOptimal);

            // Play completion and star SFX
            if (Services.TryGet<IAudioManager>(out var completeAudio))
            {
                completeAudio.PlaySFX(AudioClipType.LevelComplete);
                completeAudio.PlaySFX(AudioClipType.StarAwarded);
            }

            // Record completion in progression system
            if (Services.TryGet<IProgressionManager>(out var progression))
            {
                progression.CompleteLevelWithStars(_currentLevelNumber, _lastStarRating, _currentDefinition);

                // After replay, re-check gate if applicable
                if (_isReplay)
                {
                    int gateLevel = (_currentLevelNumber - 1) / GameConstants.LevelsPerBatch * GameConstants.LevelsPerBatch;
                    if (gateLevel > 0 && progression.IsAtBatchGate(gateLevel))
                    {
                        if (progression.CanPassBatchGate())
                        {
                            Debug.Log("[GameplayManager] Gate unlocked after replay improvement!");
                        }
                    }
                }
            }

            bool wasReplay = _isReplay;
            _isReplay = false;

            Debug.Log($"[GameplayManager] Level {_currentLevelNumber} Complete! Moves: {_moveCount}, Optimal: ~{estimatedOptimal}, Stars: {_lastStarRating}{(wasReplay ? " (replay)" : "")}");
            ShowWinOverlay(estimatedOptimal, wasReplay);
        }

        private void ShowWinOverlay(int estimatedOptimal, bool wasReplay = false)
        {
            if (Services.TryGet<ScreenManager>(out var screenMgr))
            {
                var completeScreen = screenMgr.GetScreen(GameFlowState.LevelComplete)?.GetComponent<UI.Screens.LevelCompleteScreen>();
                if (completeScreen != null)
                {
                    completeScreen.Show(_currentLevelNumber, _currentDefinition?.CityName ?? "", _lastStarRating, _moveCount, estimatedOptimal, wasReplay);
                    screenMgr.ShowOverlay(GameFlowState.LevelComplete);
                }
            }
        }

        private void OnDestroy()
        {
            if (_bottleBoard != null)
            {
                _bottleBoard.OnContainerTapped -= OnContainerTapped;

            }
        }

        private void SelectContainer(int index)
        {
            _selectedContainerIndex = index;
            _bottleBoard.GetContainerView(index).Select();

            if (Services.TryGet<IAudioManager>(out var audio))
                audio.PlaySFX(AudioClipType.Select);
        }

        private void DeselectCurrent()
        {
            if (_selectedContainerIndex >= 0)
            {
                _bottleBoard.GetContainerView(_selectedContainerIndex).Deselect();
                _selectedContainerIndex = -1;
            }
        }
    }
}
