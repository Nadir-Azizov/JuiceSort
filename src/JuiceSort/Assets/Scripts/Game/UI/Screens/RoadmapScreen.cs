using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Roadmap screen with SpriteRenderer-based island map, dedicated camera,
    /// touch scroll, and uGUI header overlay. Integrates with ScreenManager
    /// via GameFlowState.Roadmap registration.
    /// </summary>
    public class RoadmapScreen : MonoBehaviour
    {
        // Map components
        private Camera _mapCamera;
        private RoadmapMapView _mapView;
        private Transform _mapRoot;
        private List<RoadmapNodeData> _nodes = new List<RoadmapNodeData>();

        // Header overlay
        private Canvas _headerCanvas;
        private TextMeshProUGUI _headerTitle;
        private TextMeshProUGUI _starCountText;
        private Sprite _headerGradientSprite;
        private Sprite _headerStarSprite;
        private Texture2D _headerStarTexture;

        // Scroll state
        private float _scrollVelocity;
        private float _scrollMinY;
        private float _scrollMaxY;
        private bool _isDragging;
        private float _lastDragY;
        private float _dragStartScreenY;
        private float _dragStartTime;
        private float _dragDistance;

        // Auto-scroll
        private bool _isAutoScrolling;
        private float _autoScrollFrom;
        private float _autoScrollTo;
        private float _autoScrollElapsed;

        // Tap detection
        private const float TapMaxDuration = 0.2f;
        private const float TapMaxDistance = 10f;

        private const int RoadmapLayer = 8;
        private const float CameraOrthoSize = 9.6f;

        private void Start()
        {
            // Exclude Roadmap layer from main camera to prevent double-rendering
            var mainCam = Camera.main;
            if (mainCam != null)
                mainCam.cullingMask &= ~(1 << RoadmapLayer);

            // OnEnable fires during AddComponent (before _mapCamera is assigned in Create).
            // If camera wasn't activated yet, do it now that fields are set.
            if (_mapCamera != null && !_mapCamera.enabled)
            {
                _mapCamera.enabled = true;
                Refresh();
            }
        }

        private void OnEnable()
        {
            if (_mapCamera != null)
            {
                _mapCamera.enabled = true;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (_mapCamera != null)
                _mapCamera.enabled = false;
        }

        public void Refresh()
        {
            if (_mapView == null || !Services.TryGet<IProgressionManager>(out var progression))
                return;

            _nodes = BuildNodeList(progression);
            _mapView.BuildMap(_nodes);

            // Update scroll bounds
            if (_nodes.Count > 0)
            {
                _scrollMinY = _nodes[0].WorldPosition.y - RoadmapConfig.ScrollPaddingBottom;
                _scrollMaxY = _nodes[_nodes.Count - 1].WorldPosition.y + RoadmapConfig.ScrollPaddingTop;
            }

            // Update header star count
            if (_starCountText != null)
                _starCountText.text = progression.GetTotalStars().ToString();

            // Auto-scroll to current level
            int currentLevel = progression.CurrentLevel;
            float targetY = CameraOrthoSize; // default
            foreach (var node in _nodes)
            {
                if (node.State == RoadmapLevelState.Current)
                {
                    targetY = node.WorldPosition.y;
                    break;
                }
            }

            // If no current node (all completed), scroll to last completed
            if (_nodes.Count > 0)
            {
                bool foundCurrent = false;
                foreach (var node in _nodes)
                {
                    if (node.State == RoadmapLevelState.Current)
                    {
                        foundCurrent = true;
                        break;
                    }
                }
                if (!foundCurrent)
                    targetY = _nodes[_nodes.Count - 1].WorldPosition.y;
            }

            StartAutoScroll(targetY);
        }

        private void Update()
        {
            if (_mapCamera == null) return;

            HandleTouchInput();
            UpdateScroll();
            _mapView.UpdateOceanPosition();
            _mapView.UpdatePooling();
        }

        private void HandleTouchInput()
        {
            // Use Input.mousePosition for both mouse and touch
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _isAutoScrolling = false;
                _scrollVelocity = 0f;
                _lastDragY = Input.mousePosition.y;
                _dragStartScreenY = Input.mousePosition.y;
                _dragStartTime = Time.time;
                _dragDistance = 0f;
            }
            else if (Input.GetMouseButton(0) && _isDragging)
            {
                float currentY = Input.mousePosition.y;
                float deltaScreen = currentY - _lastDragY;
                _dragDistance += Mathf.Abs(currentY - _dragStartScreenY);

                // Convert screen delta to world delta
                // Finger-following: drag up → content moves up → camera moves DOWN
                float worldDelta = deltaScreen * (_mapCamera.orthographicSize * 2f) / Screen.height;
                float newCamY = _mapCamera.transform.position.y - worldDelta;
                newCamY = Mathf.Clamp(newCamY, _scrollMinY, _scrollMaxY);
                _mapCamera.transform.position = new Vector3(
                    _mapCamera.transform.position.x,
                    newCamY,
                    _mapCamera.transform.position.z
                );

                _scrollVelocity = -worldDelta / Time.deltaTime;
                _lastDragY = currentY;
            }
            else if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                _isDragging = false;

                // Check for tap (short duration, small distance)
                float duration = Time.time - _dragStartTime;
                float totalDragPx = Mathf.Abs(Input.mousePosition.y - _dragStartScreenY);

                if (duration < TapMaxDuration && totalDragPx < TapMaxDistance)
                {
                    HandleTap(Input.mousePosition);
                    _scrollVelocity = 0f;
                }
            }
        }

        private void HandleTap(Vector3 screenPos)
        {
            Vector2 worldPos = _mapCamera.ScreenToWorldPoint(screenPos);
            var hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                var marker = hit.GetComponent<RoadmapIslandMarker>();
                if (marker != null && marker.State != RoadmapLevelState.Locked)
                {
                    OnLevelTapped(marker.LevelNumber);
                }
            }
        }

        private void UpdateScroll()
        {
            if (_isAutoScrolling)
            {
                _autoScrollElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_autoScrollElapsed / RoadmapConfig.AutoScrollDuration);
                // Ease-out curve
                t = 1f - (1f - t) * (1f - t);

                float y = Mathf.Lerp(_autoScrollFrom, _autoScrollTo, t);
                _mapCamera.transform.position = new Vector3(
                    _mapCamera.transform.position.x, y, _mapCamera.transform.position.z);

                if (_autoScrollElapsed >= RoadmapConfig.AutoScrollDuration)
                    _isAutoScrolling = false;

                return;
            }

            if (!_isDragging && Mathf.Abs(_scrollVelocity) > RoadmapConfig.ScrollMinVelocity)
            {
                // Framerate-independent decay
                _scrollVelocity *= Mathf.Pow(RoadmapConfig.ScrollDecay, Time.deltaTime * 60f);

                float newY = _mapCamera.transform.position.y + _scrollVelocity * Time.deltaTime;
                newY = Mathf.Clamp(newY, _scrollMinY, _scrollMaxY);
                _mapCamera.transform.position = new Vector3(
                    _mapCamera.transform.position.x, newY, _mapCamera.transform.position.z);
            }
            else if (!_isDragging)
            {
                _scrollVelocity = 0f;
            }
        }

        private void StartAutoScroll(float targetY)
        {
            targetY = Mathf.Clamp(targetY, _scrollMinY, _scrollMaxY);
            _autoScrollFrom = _mapCamera.transform.position.y;
            _autoScrollTo = targetY;
            _autoScrollElapsed = 0f;
            _isAutoScrolling = true;
            _scrollVelocity = 0f;
        }

        private void OnLevelTapped(int levelNumber)
        {
            if (!Services.TryGet<IProgressionManager>(out var progression))
                return;

            if (!Services.TryGet<GameplayManager>(out var gameplay))
                return;

            if (!Services.TryGet<ScreenManager>(out var screenMgr))
                return;

            if (progression.IsLevelCompleted(levelNumber))
            {
                gameplay.StartReplay(levelNumber);
            }
            else if (gameplay.HasPausedLevel(levelNumber))
            {
                gameplay.ResumeLevel(levelNumber);
            }
            else
            {
                gameplay.StartLevel(levelNumber);
            }

            screenMgr.TransitionTo(GameFlowState.Playing);
        }

        private List<RoadmapNodeData> BuildNodeList(IProgressionManager progression)
        {
            var nodes = new List<RoadmapNodeData>();

            int currentLevel = progression.CurrentLevel;
            int previousLevel = currentLevel - 1;
            bool gateBlocked = previousLevel > 0
                && progression.IsAtBatchGate(previousLevel)
                && !progression.CanPassBatchGate();

            // Zone bounds: each zone = 20 levels
            int currentZone = (currentLevel - 1) / 20;
            int zoneStart = currentZone * 20 + 1;
            int zoneEnd = (currentZone + 1) * 20;

            // Completed levels within current zone
            var records = progression.GetAllLevelRecords();
            foreach (var record in records)
            {
                int lvl = record.LevelNumber;
                if (lvl < zoneStart || lvl > zoneEnd) continue;

                var node = new RoadmapNodeData
                {
                    LevelNumber = lvl,
                    State = RoadmapLevelState.Completed,
                    StarsEarned = record.Stars,
                    IsBoss = lvl % 10 == 0,
                    IslandSprite = RoadmapConfig.GetIslandSprite(lvl, RoadmapLevelState.Completed),
                    FlipX = RoadmapConfig.ShouldFlip(lvl),
                    WorldPosition = RoadmapConfig.GetNodePosition(lvl)
                };
                nodes.Add(node);
            }

            // Current level (if within zone, not completed, not gate-blocked)
            int lastRenderedLevel = currentLevel;
            if (currentLevel >= zoneStart && currentLevel <= zoneEnd
                && !progression.IsLevelCompleted(currentLevel) && !gateBlocked)
            {
                var currentNode = new RoadmapNodeData
                {
                    LevelNumber = currentLevel,
                    State = RoadmapLevelState.Current,
                    StarsEarned = 0,
                    IsBoss = currentLevel % 10 == 0,
                    IslandSprite = RoadmapConfig.GetIslandSprite(currentLevel, RoadmapLevelState.Current),
                    FlipX = RoadmapConfig.ShouldFlip(currentLevel),
                    WorldPosition = RoadmapConfig.GetNodePosition(currentLevel)
                };
                nodes.Add(currentNode);
                lastRenderedLevel = currentLevel;
            }
            else if (gateBlocked)
            {
                lastRenderedLevel = currentLevel - 1;
            }

            // Remaining locked levels within current zone (up to zoneEnd)
            for (int lvl = lastRenderedLevel + 1; lvl <= zoneEnd; lvl++)
            {
                var lockedNode = new RoadmapNodeData
                {
                    LevelNumber = lvl,
                    State = RoadmapLevelState.Locked,
                    StarsEarned = 0,
                    IsBoss = lvl % 10 == 0,
                    IslandSprite = RoadmapConfig.GetIslandSprite(lvl, RoadmapLevelState.Locked),
                    FlipX = RoadmapConfig.ShouldFlip(lvl),
                    WorldPosition = RoadmapConfig.GetNodePosition(lvl)
                };
                nodes.Add(lockedNode);
            }

            // 4 preview islands from next zone (skip boss positions — multiples of 10)
            int previewCount = 0;
            int previewLevel = zoneEnd + 1;
            while (previewCount < 4)
            {
                if (previewLevel % 10 != 0) // skip boss positions in preview
                {
                    var previewNode = new RoadmapNodeData
                    {
                        LevelNumber = previewLevel,
                        State = RoadmapLevelState.Locked,
                        StarsEarned = 0,
                        IsBoss = false,
                        IsPreview = true,
                        IslandSprite = RoadmapConfig.GetIslandSprite(previewLevel, RoadmapLevelState.Locked, isPreview: true),
                        FlipX = RoadmapConfig.ShouldFlip(previewLevel),
                        WorldPosition = RoadmapConfig.GetNodePosition(previewLevel)
                    };
                    nodes.Add(previewNode);
                    previewCount++;
                }
                previewLevel++;
            }

            return nodes;
        }

        public static GameObject Create()
        {
            var go = new GameObject("RoadmapScreen");

            // CanvasGroup for ScreenManager transition compatibility
            go.AddComponent<CanvasGroup>();

            // Map root for all SpriteRenderer content
            var mapRoot = new GameObject("MapRoot");
            mapRoot.transform.SetParent(go.transform, false);

            // Dedicated orthographic camera for map content
            var camGO = new GameObject("RoadmapCamera");
            camGO.layer = RoadmapLayer;
            camGO.transform.SetParent(go.transform, false);
            camGO.transform.position = new Vector3(0f, CameraOrthoSize, -10f);

            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.024f, 0.290f, 0.408f, 1f); // #064A68 deep ocean fallback
            cam.orthographic = true;
            cam.orthographicSize = CameraOrthoSize;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            cam.depth = 10; // render AFTER main camera (depth 0) to avoid bleed-through
            // Only render Roadmap layer
            cam.cullingMask = 1 << RoadmapLayer;
            cam.enabled = false; // starts disabled, enabled in OnEnable

            // RoadmapMapView component
            var mapView = go.AddComponent<RoadmapMapView>();
            mapView.Initialize(cam, mapRoot.transform);

            // Header overlay (ScreenSpaceOverlay Canvas)
            var headerCanvas = CreateHeaderOverlay(go, out var titleText, out var starCountText, out var gradientSprite, out var starSprite, out var starTex);

            // RoadmapScreen component
            var screen = go.AddComponent<RoadmapScreen>();
            screen._mapCamera = cam;
            screen._mapView = mapView;
            screen._mapRoot = mapRoot.transform;
            screen._headerCanvas = headerCanvas;
            screen._headerTitle = titleText;
            screen._starCountText = starCountText;
            screen._headerGradientSprite = gradientSprite;
            screen._headerStarSprite = starSprite;
            screen._headerStarTexture = starTex;

            return go;
        }

        private static Canvas CreateHeaderOverlay(GameObject parent, out TextMeshProUGUI titleText, out TextMeshProUGUI starCountText, out Sprite gradientSprite, out Sprite headerStarSprite, out Texture2D headerStarTexture)
        {
            var headerGO = new GameObject("HeaderOverlay");
            headerGO.transform.SetParent(parent.transform, false);

            var canvas = headerGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = headerGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            headerGO.AddComponent<GraphicRaycaster>();

            // Header background gradient
            var bgGO = new GameObject("HeaderBg");
            bgGO.transform.SetParent(headerGO.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.92f);
            bgRect.anchorMax = new Vector2(1f, 1f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = bgGO.AddComponent<Image>();
            gradientSprite = ThemeConfig.CreateGradientSprite(
                RoadmapConfig.HeaderBgColor,
                new Color(0.03f, 0.20f, 0.29f, 0f));
            bgImage.sprite = gradientSprite;
            bgImage.type = Image.Type.Simple;

            // Safe area padding
            var safeArea = Screen.safeArea;
            float safeTopOffset = (Screen.height - safeArea.yMax) / Screen.height;

            // Title (left)
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(headerGO.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.04f, 0.94f);
            titleRect.anchorMax = new Vector2(0.5f, 0.98f - safeTopOffset);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "JuiceSort";
            titleText.fontSize = 22f;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.color = Color.white;
            titleText.font = ThemeConfig.GetFontBold();

            // Star counter (right)
            var starBgGO = new GameObject("StarCounter");
            starBgGO.transform.SetParent(headerGO.transform, false);
            var starBgRect = starBgGO.AddComponent<RectTransform>();
            starBgRect.anchorMin = new Vector2(0.72f, 0.945f);
            starBgRect.anchorMax = new Vector2(0.88f, 0.975f - safeTopOffset);
            starBgRect.offsetMin = Vector2.zero;
            starBgRect.offsetMax = Vector2.zero;

            var starBgImg = starBgGO.AddComponent<Image>();
            starBgImg.color = new Color(0f, 0f, 0f, 0.4f);

            // Star icon (runtime-generated star sprite as uGUI Image)
            var starIconGO = new GameObject("StarIcon");
            starIconGO.transform.SetParent(starBgGO.transform, false);
            var starIconRect = starIconGO.AddComponent<RectTransform>();
            starIconRect.anchorMin = new Vector2(0f, 0f);
            starIconRect.anchorMax = new Vector2(0.4f, 1f);
            starIconRect.offsetMin = new Vector2(8f, 4f);
            starIconRect.offsetMax = new Vector2(0f, -4f);

            var starIconImg = starIconGO.AddComponent<Image>();
            var starTex = RoadmapMapView.CreateStarTexture(32);
            var starSprite = Sprite.Create(starTex,
                new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
            starIconImg.sprite = starSprite;
            starIconImg.color = RoadmapConfig.StarEarnedColor;
            starIconImg.preserveAspect = true;
            headerStarSprite = starSprite;
            headerStarTexture = starTex;

            // Star count text
            var countGO = new GameObject("Count");
            countGO.transform.SetParent(starBgGO.transform, false);
            var countRect = countGO.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.4f, 0f);
            countRect.anchorMax = new Vector2(1f, 1f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = new Vector2(-8f, 0f);

            starCountText = countGO.AddComponent<TextMeshProUGUI>();
            starCountText.text = "0";
            starCountText.fontSize = 14f;
            starCountText.alignment = TextAlignmentOptions.MidlineLeft;
            starCountText.color = Color.white;
            starCountText.font = ThemeConfig.GetFont();

            // Close button (X) — top right
            var closeBtnGO = new GameObject("CloseButton");
            closeBtnGO.transform.SetParent(headerGO.transform, false);
            var closeRect = closeBtnGO.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.9f, 0.94f);
            closeRect.anchorMax = new Vector2(0.96f, 0.975f - safeTopOffset);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            closeRect.sizeDelta = new Vector2(44f, 44f);

            var closeBg = closeBtnGO.AddComponent<Image>();
            closeBg.color = new Color(0f, 0f, 0f, 0.3f);

            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtnGO.AddComponent<ButtonBounce>();
            closeBtn.onClick.AddListener(() =>
            {
                if (Services.TryGet<ScreenManager>(out var sm))
                    sm.TransitionTo(GameFlowState.MainMenu);
            });

            var closeTextGO = new GameObject("X");
            closeTextGO.transform.SetParent(closeBtnGO.transform, false);
            var closeTextRect = closeTextGO.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            var closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 22f;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
            closeText.font = ThemeConfig.GetFont();

            return canvas;
        }

        private void OnDestroy()
        {
            if (_headerGradientSprite != null)
            {
                if (_headerGradientSprite.texture != null)
                    Object.Destroy(_headerGradientSprite.texture);
                Object.Destroy(_headerGradientSprite);
            }
            if (_headerStarTexture != null)
                Object.Destroy(_headerStarTexture);
            if (_headerStarSprite != null)
                Object.Destroy(_headerStarSprite);
        }
    }
}
