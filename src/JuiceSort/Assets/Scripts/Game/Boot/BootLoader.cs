using System;
using UnityEngine;
using UnityEngine.EventSystems;
using JuiceSort.Core;
using JuiceSort.Game.Progression;
using JuiceSort.Game.Save;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.Ads;
using JuiceSort.Game.Audio;
using JuiceSort.Game.Effects;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;
using JuiceSort.Game.UI.Screens;

namespace JuiceSort.Game.Boot
{
    /// <summary>
    /// Initializes the game by creating all services and showing the main menu.
    /// Attach this to a GameObject in the Boot scene (scene index 0).
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        private static bool _initialized;

        // Stored for unsubscription in OnDestroy
        private LevelCompleteScreen _lcScreen;
        private StarGateScreen _gateScreen;
        private ScreenManager _screenMgr;
        private Action _onNextLevel;
        private Action _onReplay;
        private Action _onRoadmap;
        private Action _onContinue;
        private Action<int> _onGateLevelTapped;
        private Action<GameFlowState> _onStateChanged;

        private void Awake()
        {
            if (_initialized)
            {
                Destroy(gameObject);
                return;
            }

            _initialized = true;
            DontDestroyOnLoad(gameObject);

            Services.Clear();
            CreateEventSystem();
            CreateGlobalExceptionHandler();
            CreateBloomSetup();
            CreateServices();

            Debug.Log("[BootLoader] All services created.");
        }

        private void CreateBloomSetup()
        {
            var bloom = Effects.BloomSetup.Create();
            DontDestroyOnLoad(bloom.gameObject);
        }

        private void CreateGlobalExceptionHandler()
        {
            var handler = GlobalExceptionHandler.Create();
            DontDestroyOnLoad(handler.gameObject);
        }

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                DontDestroyOnLoad(esGo);
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        private void CreateServices()
        {
            // SaveManager — no dependencies
            var smGo = new GameObject("SaveManager");
            DontDestroyOnLoad(smGo);
            Services.Register<ISaveManager>(smGo.AddComponent<SaveManager>());

            // ProgressionManager — loads from SaveManager in Start()
            var pmGo = new GameObject("ProgressionManager");
            DontDestroyOnLoad(pmGo);
            Services.Register<IProgressionManager>(pmGo.AddComponent<ProgressionManager>());

            // CoinManager — loads balance from SaveManager in Start()
            var cmGo = new GameObject("CoinManager");
            DontDestroyOnLoad(cmGo);
            Services.Register<ICoinManager>(cmGo.AddComponent<Economy.CoinManager>());

            // AudioManager — reads settings from ProgressionManager in Start()
            var amGo = new GameObject("AudioManager");
            DontDestroyOnLoad(amGo);
            Services.Register<IAudioManager>(amGo.AddComponent<AudioManager>());

            // AdManager — placeholder for Google AdMob
            var adGo = new GameObject("AdManager");
            DontDestroyOnLoad(adGo);
            Services.Register<IAdManager>(adGo.AddComponent<AdManager>());

            // ScreenManager — manages screen transitions
            var screenMgrGo = new GameObject("ScreenManager");
            DontDestroyOnLoad(screenMgrGo);
            var screenMgr = screenMgrGo.AddComponent<ScreenManager>();
            Services.Register<ScreenManager>(screenMgr);

            // GameplayManager — passive, waits for StartLevel call
            var gmGo = new GameObject("GameplayManager");
            DontDestroyOnLoad(gmGo);
            var gm = gmGo.AddComponent<GameplayManager>();
            Services.Register<GameplayManager>(gm);

            // Loading screen — shown immediately on boot
            var loading = LoadingScreen.Create();
            DontDestroyOnLoad(loading);
            screenMgr.RegisterScreen(GameFlowState.Loading, loading);

            // Create and register screens
            var hub = HubScreen.Create();
            DontDestroyOnLoad(hub);
            screenMgr.RegisterScreen(GameFlowState.MainMenu, hub);

            var roadmap = RoadmapScreen.Create();
            DontDestroyOnLoad(roadmap);
            screenMgr.RegisterScreen(GameFlowState.Roadmap, roadmap);

            // Level Complete screen
            var levelComplete = LevelCompleteScreen.Create();
            DontDestroyOnLoad(levelComplete);
            screenMgr.RegisterScreen(GameFlowState.LevelComplete, levelComplete);

            _lcScreen = levelComplete.GetComponent<LevelCompleteScreen>();
            _onNextLevel = () => { gm.NextLevel(); screenMgr.HideOverlay(GameFlowState.LevelComplete); };
            _onReplay = () => { gm.RestartLevel(); screenMgr.HideOverlay(GameFlowState.LevelComplete); };
            _onRoadmap = () => { screenMgr.HideOverlay(GameFlowState.LevelComplete); screenMgr.TransitionTo(GameFlowState.Roadmap); };
            _onContinue = () =>
            {
                screenMgr.HideOverlay(GameFlowState.LevelComplete);
                if (Services.TryGet<IProgressionManager>(out var prog) && prog.CanPassBatchGate())
                    gm.NextLevel();
                else
                    screenMgr.TransitionTo(GameFlowState.Roadmap);
            };
            _lcScreen.OnNextLevel += _onNextLevel;
            _lcScreen.OnReplay += _onReplay;
            _lcScreen.OnRoadmap += _onRoadmap;
            _lcScreen.OnContinue += _onContinue;

            // Star Gate screen
            var starGate = StarGateScreen.Create();
            DontDestroyOnLoad(starGate);
            screenMgr.RegisterScreen(GameFlowState.Gate, starGate);

            _gateScreen = starGate.GetComponent<StarGateScreen>();
            _onGateLevelTapped = (levelNum) =>
            {
                screenMgr.HideOverlay(GameFlowState.Gate);
                gm.StartReplay(levelNum);
                screenMgr.TransitionTo(GameFlowState.Playing);
            };
            _gateScreen.OnLevelTapped += _onGateLevelTapped;

            // Settings screen
            var settings = SettingsScreen.Create();
            DontDestroyOnLoad(settings);
            screenMgr.RegisterScreen(GameFlowState.Settings, settings);

            // Ambient floating light particles disabled — caused streaking line artifacts
            // FloatingLights.Create();

            // Refresh screens when transitioning to them
            _screenMgr = screenMgr;
            _onStateChanged = (state) =>
            {
                if (state == GameFlowState.MainMenu)
                {
                    var hubScreen = hub.GetComponent<HubScreen>();
                    if (hubScreen != null)
                        hubScreen.Refresh();
                }
                else if (state == GameFlowState.Roadmap)
                {
                    var roadmapScreen = roadmap.GetComponent<RoadmapScreen>();
                    if (roadmapScreen != null)
                        roadmapScreen.Refresh();
                }
                else if (state == GameFlowState.Settings)
                {
                    var settingsScreen = settings.GetComponent<SettingsScreen>();
                    if (settingsScreen != null)
                        settingsScreen.Refresh();
                }
            };
            _screenMgr.OnStateChanged += _onStateChanged;
        }

        private void Start()
        {
            // Show loading screen first, then transition to Hub after minimum delay
            if (Services.TryGet<ScreenManager>(out var screenMgr))
            {
                screenMgr.TransitionTo(GameFlowState.Loading);
                StartCoroutine(WaitForLoadingThenShowHub(screenMgr));
            }
        }

        private System.Collections.IEnumerator WaitForLoadingThenShowHub(ScreenManager screenMgr)
        {
            // Find the LoadingScreen component to check readiness
            var loadingGo = screenMgr.GetScreen(GameFlowState.Loading);
            var loadingScreen = loadingGo != null ? loadingGo.GetComponent<LoadingScreen>() : null;

            if (loadingScreen != null)
            {
                // Wait until minimum display time has elapsed
                while (!loadingScreen.IsReady)
                    yield return null;
            }
            else
            {
                // Fallback: just wait 2 seconds if component not found
                yield return new WaitForSecondsRealtime(2f);
            }

            screenMgr.TransitionTo(GameFlowState.MainMenu);
        }

        private void OnDestroy()
        {
            // Unsubscribe all event listeners to allow proper GC and editor domain reload
            if (_lcScreen != null)
            {
                _lcScreen.OnNextLevel -= _onNextLevel;
                _lcScreen.OnReplay -= _onReplay;
                _lcScreen.OnRoadmap -= _onRoadmap;
                _lcScreen.OnContinue -= _onContinue;
            }
            if (_gateScreen != null)
                _gateScreen.OnLevelTapped -= _onGateLevelTapped;
            if (_screenMgr != null)
                _screenMgr.OnStateChanged -= _onStateChanged;

            UIShapeUtils.ClearCache();
            ThemeConfig.ClearCachedGradients();

            _initialized = false;
        }
    }
}
