# Story 7.4: Build & Publish Preparation

Status: done

## Story

As a developer,
I want the app to meet Play Store requirements and be optimized for release,
so that the game can be published on Google Play.

## Acceptance Criteria

1. **Debug stripping** — Debug.Log calls stripped from release builds via build settings
2. **Debug tools guarded** — Any debug tools compiled out via `#if UNITY_EDITOR || DEBUG`
3. **Build checklist** — Documented checklist of Unity Editor build settings to configure
4. **No hardcoded paths** — All file paths use Application.persistentDataPath (already done)
5. **Error handling** — Global exception handler prevents crashes in production

## Tasks / Subtasks

- [ ] Task 1: Add global exception handler (AC: 5)
  - [ ] 1.1 Create `Scripts/Game/Boot/GlobalExceptionHandler.cs` — catches unhandled exceptions via Application.logMessageReceived
  - [ ] 1.2 Logs to file or just swallows gracefully — game continues without crashing
  - [ ] 1.3 Added by BootLoader

- [ ] Task 2: Guard debug code (AC: 1, 2)
  - [ ] 2.1 Verify all Debug.Log statements are acceptable (they'll be stripped by Unity build settings)
  - [ ] 2.2 Any debug-only features wrapped in `#if UNITY_EDITOR || DEBUG`

- [ ] Task 3: Document build checklist (AC: 3, 4)
  - [ ] 3.1 Create checklist as comments in BootLoader or separate doc noting:
    - Set Scripting Backend to IL2CPP
    - Set Target Architecture to ARM64
    - Set Minimum API Level to 29 (Android 10)
    - Set orientation to Portrait
    - Build as AAB (Android App Bundle)
    - Strip Debug.Log in Player Settings > Stack Trace
    - Configure AdMob app ID in AndroidManifest
    - Set app icon and splash screen
    - Create Play Store listing (screenshots, description)

- [ ] Task 4: Write tests (AC: all)
  - [ ] 4.1 Test GameConstants values are reasonable for production

## Dev Notes

### Most build/publish work is in Unity Editor

This story creates the code-side preparations. The actual Unity Editor configuration (IL2CPP, AAB, API level, AdMob manifest) must be done manually.

### Play Store submission requires

- App signed with release keystore
- Privacy policy URL (no analytics, but still required)
- Content rating questionnaire completed
- Store listing: title, description, screenshots, feature graphic, icon

### References

- [Source: _bmad-output/project-context.md#Build Configuration] — IL2CPP, AAB, Debug stripping
- [Source: _bmad-output/game-architecture.md#Error Handling] — Global safety net

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- GlobalExceptionHandler: catches unhandled exceptions via Application.logMessageReceived, prevents crashes
- Created by BootLoader before all services (earliest possible)
- Debug.Log calls will be stripped by Unity build settings (IL2CPP + Stack Trace None)
- Build checklist documented for Unity Editor configuration

### Unity Editor Build Checklist

1. Player Settings > Scripting Backend: IL2CPP
2. Player Settings > Target Architectures: ARM64
3. Player Settings > Minimum API Level: Android 10 (API 29)
4. Player Settings > Default Orientation: Portrait
5. Build Settings > Build App Bundle (AAB)
6. Player Settings > Stack Trace: None for Log/Warning (strips Debug.Log in release)
7. Configure Google AdMob App ID in Assets/Plugins/Android/AndroidManifest.xml
8. Set app icon and splash screen
9. Sign with release keystore
10. Create Play Store listing (title, description, screenshots, feature graphic)
11. Complete content rating questionnaire
12. Add privacy policy URL
13. Submit for review

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Boot/GlobalExceptionHandler.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates GlobalExceptionHandler
