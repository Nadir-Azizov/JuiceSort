using System.Collections.Generic;
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
        private int _undoUsageCount;

        // Paused level state — keeps one level's state for resume
        private PuzzleState _pausedPuzzle;
        private int _pausedLevelNumber = -1;
        private int _pausedMoveCount;
        private int _pausedExtraBottlesUsed;

        private GameplayHUD _hud;
        private GameObject _hudCanvas;
        private BackgroundManager _backgroundManager;
        private PourStreamVFX _pourStream;
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

            // Create reusable pour stream VFX (single instance across all pours)
            if (_pourStream == null)
                _pourStream = PourStreamVFX.Create();

            _moveCount = 0;
            _isLevelComplete = false;
            _selectedContainerIndex = -1;
            _extraBottlesUsed = 0;
            _undoUsageCount = 0;
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
            _hud.OnExitPressed = GoBackToHub;
            _hud.SetLevelInfo(levelNumber, definition.CityName ?? "", definition.CountryName ?? "", definition.Mood);
            _hud.UpdateDisplay(0);
            if (Services.TryGet<ICoinManager>(out var coinMgr))
            {
                _hud.UpdateCoinDisplay(coinMgr.GetBalance());
                coinMgr.OnBalanceChanged += _hud.UpdateCoinDisplay;
                coinMgr.OnBalanceChanged += OnCoinBalanceChanged;
            }
            RefreshUndoState();
            RefreshExtraBottleState();

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

            // Unsubscribe coin balance listeners to prevent leaks
            if (Services.TryGet<ICoinManager>(out var coinMgr))
            {
                if (_hud != null)
                    coinMgr.OnBalanceChanged -= _hud.UpdateCoinDisplay;
                coinMgr.OnBalanceChanged -= OnCoinBalanceChanged;
            }

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
            _extraBottlesUsed = 0;
            _undoUsageCount = 0;
            _undoStack.Clear();
            DeselectCurrent();
            _bottleBoard.RebindPuzzle(_currentPuzzle);
            _hud?.UpdateDisplay(_moveCount);
            RefreshUndoState();
            RefreshExtraBottleState();

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

            // Coin payment gate
            if (!Services.TryGet<ICoinManager>(out var coinMgr))
                return;

            var config = Economy.CoinConfig.Default();
            int cost = config.GetExtraBottleCost(_extraBottlesUsed);
            if (!coinMgr.SpendCoins(cost))
            {
                Debug.Log($"[GameplayManager] Extra bottle blocked: insufficient coins (need {cost}, have {coinMgr.GetBalance()})");
                return;
            }

            _currentPuzzle = PuzzleEngine.AddExtraContainer(_currentPuzzle, _currentDefinition.SlotCount);
            _extraBottlesUsed++;

            // Block input during re-layout animation
            _isAnimating = true;
            _bottleBoard.SetAllSparklesEnabled(false);

            // Add new container view with animated re-layout
            var newContainer = _currentPuzzle.GetContainer(_currentPuzzle.ContainerCount - 1);
            _bottleBoard.AddContainerView(newContainer, _currentPuzzle.ContainerCount - 1,
                onComplete: () =>
                {
                    _isAnimating = false;
                    _bottleBoard.SetAllSparklesEnabled(true);
                });

            // Clear undo stack — old snapshots have fewer containers
            _undoStack.Clear();
            _hud?.UpdateDisplay(_moveCount);
            RefreshExtraBottleState();
            RefreshUndoState();

            Debug.Log($"[GameplayManager] Extra bottle added (coin). Used: {_extraBottlesUsed}/{GameConstants.MaxExtraBottles}, Cost: {cost}");
        }

        public void WatchAdForCoins()
        {
            if (_isLevelComplete || _isAnimating)
                return;

            if (!Services.TryGet<IAdManager>(out var adManager))
                return;

            adManager.ShowRewardedAd(
                onRewarded: () =>
                {
                    if (Services.TryGet<ICoinManager>(out var coinMgr))
                    {
                        var config = Economy.CoinConfig.Default();
                        coinMgr.AddCoins(config.AdRewardAmount);
                        Debug.Log($"[GameplayManager] Ad reward: +{config.AdRewardAmount} coins");
                    }
                        },
                onFailed: () =>
                {
                    Debug.Log("[GameplayManager] Ad failed — no coins awarded");
                }
            );
        }

        /// <summary>
        /// Pauses current level and returns to hub. Saves state for resume.
        /// Only one paused level is kept — starting a different level clears it.
        /// </summary>
        public void GoBackToHub()
        {
            if (_isAnimating)
                return;

            // Collapse settings panel if open
            _hud?.CollapseSettingsIfOpen();

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
                screenMgr.TransitionTo(GameFlowState.MainMenu);
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
            _hud.OnExitPressed = GoBackToHub;
            _hud.SetLevelInfo(_currentLevelNumber, _currentDefinition.CityName ?? "", _currentDefinition.CountryName ?? "", _currentDefinition.Mood);
            _hud.UpdateDisplay(_moveCount);
            if (Services.TryGet<ICoinManager>(out var coinMgrResume))
            {
                _hud.UpdateCoinDisplay(coinMgrResume.GetBalance());
                coinMgrResume.OnBalanceChanged += _hud.UpdateCoinDisplay;
                coinMgrResume.OnBalanceChanged += OnCoinBalanceChanged;
            }
            RefreshUndoState();
            RefreshExtraBottleState();

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

            // Block gameplay taps while settings panel is open
            if (_hud != null && _hud.IsSettingsOpen)
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

            // Snapshot container data BEFORE ExecutePour — animation needs pre-pour state
            var sourceDataSnapshot = source.Clone();
            var targetDataSnapshot = target.Clone();

            // Snapshot which containers are already completed before the pour
            var wasCompleted = new HashSet<int>();
            for (int i = 0; i < _currentPuzzle.ContainerCount; i++)
            {
                if (_currentPuzzle.GetContainer(i).IsCompleted())
                    wasCompleted.Add(i);
            }

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
            _bottleBoard.SetAllSparklesEnabled(false);

            // Bottle world height for movement clearance (sprite height * current scale)
            float bottleWorldHeight = _bottleBoard.BottleSpriteHeight * sourceView.transform.localScale.x;

            StartCoroutine(PourAnimator.Animate(
                sourceView, targetView, pourCount, pourColor,
                sourceDataSnapshot, targetDataSnapshot,
                bottleWorldHeight,
                _pourStream,
                onMidPour: () =>
                {
                    if (Services.TryGet<IAudioManager>(out var pourAudio))
                        pourAudio.PlaySFX(AudioClipType.Pour);
                },
                onComplete: () =>
                {
                    _hud?.UpdateDisplay(_moveCount);
                    RefreshUndoState();
                    Debug.Log($"[GameplayManager] Pour animated. Moves: {_moveCount}");

                    // Detect newly completed containers
                    var newlyCompleted = new List<int>();
                    for (int i = 0; i < _currentPuzzle.ContainerCount; i++)
                    {
                        if (_currentPuzzle.GetContainer(i).IsCompleted() && !wasCompleted.Contains(i))
                            newlyCompleted.Add(i);
                    }

                    if (newlyCompleted.Count > 0)
                    {
                        PlayCompletionEffects(newlyCompleted);
                    }
                    else
                    {
                        _isAnimating = false;
                        _bottleBoard?.SetAllSparklesEnabled(true);
                        if (_currentPuzzle.IsAllSorted())
                            OnLevelComplete();
                    }
                }
            ));
        }

        private void PlayCompletionEffects(List<int> newlyCompletedIndices)
        {
            int remaining = newlyCompletedIndices.Count;
            bool allSorted = _currentPuzzle.IsAllSorted();

            foreach (int idx in newlyCompletedIndices)
            {
                var view = _bottleBoard.GetContainerView(idx);
                view.PlayCompletionEffect(onComplete: () =>
                {
                    remaining--;
                    if (remaining <= 0)
                    {
                        _isAnimating = false;
                        _bottleBoard?.SetAllSparklesEnabled(true);
                        if (allSorted)
                            OnLevelComplete();
                    }
                });
            }
        }

        public void Undo()
        {
            if (_isLevelComplete || _isAnimating)
                return;

            // Pop first to verify stack isn't empty (no coins charged if empty)
            var snapshot = _undoStack.Pop();
            if (snapshot == null)
                return;

            // Attempt coin payment
            if (Services.TryGet<ICoinManager>(out var coinMgr))
            {
                var config = Economy.CoinConfig.Default();
                int cost = config.GetUndoCost(_undoUsageCount);
                if (!coinMgr.SpendCoins(cost))
                {
                    // Insufficient coins — re-push snapshot and abort
                    _undoStack.Push(snapshot);
                    Debug.Log($"[GameplayManager] Undo blocked: insufficient coins (need {cost}, have {coinMgr.GetBalance()})");
                    return;
                }
                _undoUsageCount++;
            }

            _currentPuzzle = snapshot;
            _moveCount--;
            DeselectCurrent();
            _bottleBoard.RebindPuzzle(_currentPuzzle);
            _hud?.UpdateDisplay(_moveCount);
            RefreshUndoState();

            Debug.Log($"[GameplayManager] Undo. Moves: {_moveCount}, Undos remaining: {_undoStack.Count}, Undo uses: {_undoUsageCount}");
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

            // Calculate and award coin reward with streak bonus
            int coinReward = 0;
            if (Services.TryGet<ICoinManager>(out var coinMgr))
            {
                var config = Economy.CoinConfig.Default();
                int baseReward = Economy.CoinRewardCalculator.CalculateReward(_lastStarRating, _currentDefinition, config);

                coinMgr.IncrementStreak();
                int streakBonus = Economy.StreakBonusCalculator.GetStreakBonus(coinMgr.StreakCount, config);

                coinReward = baseReward + streakBonus;
                coinMgr.AddCoins(coinReward);
            }

            bool wasReplay = _isReplay;
            _isReplay = false;

            Debug.Log($"[GameplayManager] Level {_currentLevelNumber} Complete! Moves: {_moveCount}, Optimal: ~{estimatedOptimal}, Stars: {_lastStarRating}, Coins: +{coinReward}{(wasReplay ? " (replay)" : "")}");
            ShowWinOverlay(estimatedOptimal, wasReplay, coinReward);
        }

        private void ShowWinOverlay(int estimatedOptimal, bool wasReplay, int coinReward = 0)
        {
            if (Services.TryGet<ScreenManager>(out var screenMgr))
            {
                var completeScreen = screenMgr.GetScreen(GameFlowState.LevelComplete)?.GetComponent<UI.Screens.LevelCompleteScreen>();
                if (completeScreen != null)
                {
                    completeScreen.Show(_currentLevelNumber, _currentDefinition?.CityName ?? "", _lastStarRating, _moveCount, estimatedOptimal, wasReplay, coinReward);
                    screenMgr.ShowOverlay(GameFlowState.LevelComplete);
                }
            }
        }

        private void OnDestroy()
        {
            if (_bottleBoard != null)
                _bottleBoard.OnContainerTapped -= OnContainerTapped;

            // Clean up pour stream VFX to prevent orphaned GameObjects
            if (_pourStream != null)
            {
                Destroy(_pourStream.gameObject);
                _pourStream = null;
            }

            // Unsubscribe coin listeners in case DestroyBoard wasn't called (e.g., scene unload)
            if (Services.TryGet<ICoinManager>(out var coinMgr))
            {
                if (_hud != null)
                    coinMgr.OnBalanceChanged -= _hud.UpdateCoinDisplay;
                coinMgr.OnBalanceChanged -= OnCoinBalanceChanged;
            }
        }

        private void SelectContainer(int index)
        {
            _selectedContainerIndex = index;
            _bottleBoard.GetContainerView(index).Select();

            if (Services.TryGet<IAudioManager>(out var audio))
                audio.PlaySFX(AudioClipType.Select);
        }

        private void OnCoinBalanceChanged(int newBalance)
        {
            RefreshUndoState();
            RefreshExtraBottleState();
        }

        private void RefreshUndoState()
        {
            if (_hud == null) return;
            var config = Economy.CoinConfig.Default();
            int cost = config.GetUndoCost(_undoUsageCount);
            bool hasUndos = _undoStack != null && _undoStack.Count > 0;
            bool canAfford = true;
            if (Services.TryGet<ICoinManager>(out var coinMgr))
                canAfford = coinMgr.GetBalance() >= cost;
            _hud.UpdateUndoState(cost, canAfford, hasUndos);
        }

        private void RefreshExtraBottleState()
        {
            if (_hud == null) return;
            var config = Economy.CoinConfig.Default();
            bool hasRemaining = _extraBottlesUsed < GameConstants.MaxExtraBottles;
            int cost = config.GetExtraBottleCost(_extraBottlesUsed);
            bool canAfford = true;
            if (Services.TryGet<ICoinManager>(out var coinMgr))
                canAfford = coinMgr.GetBalance() >= cost;
            _hud.UpdateExtraBottleState(cost, canAfford, hasRemaining);
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
