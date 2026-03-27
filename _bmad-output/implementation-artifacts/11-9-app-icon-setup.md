# Story 11.9: App Icon Setup (Android Adaptive Icon)

Status: done

## Story

As a player,
I want the JuiceSort app to have a polished, recognizable icon on my device,
so that it looks professional and is easy to find in my app drawer.

## Priority

MEDIUM — Branding/polish task; needed before any public release or store listing update.

## Acceptance Criteria

1. App icon displays correctly in Android launcher (test via Build & Run or APK install)
2. App icon is not stretched or cropped incorrectly on adaptive icon devices
3. No compression artifacts visible on icon
4. Icon asset is in correct folder: `Assets/Resources/Icons/`

## Prerequisites

- Final app icon PNG: `app_icon_1024x1024.png` must be placed in `Assets/Resources/Icons/` before running

## Tasks / Subtasks

- [x] Task 1: Configure icon import settings (AC: 3, 4)
  - [x] 1.1 Verify `Assets/Resources/Icons/app_icon_1024x1024.png` exists
  - [x] 1.2 Set Texture Type: `Default`, Sprite Mode: `Single`
  - [x] 1.3 Set Max Size: `1024`, Compression: `None`, sRGB: `true`

- [x] Task 2: Configure Player Settings for Android icon (AC: 1, 2)
  - [x] 2.1 Set `app_icon_1024x1024` as the default icon
  - [x] 2.2 Configure Adaptive Icon (Android 8.0+): Foreground = `app_icon_1024x1024`, Background = solid color `#0a1e32`
  - [x] 2.3 Set legacy icons for all density buckets (48/72/96/144/192) using same source
  - [x] 2.4 Do NOT add separate round icon — Android generates it from adaptive layers

## Dev Notes

### Key Files to MODIFY
- `Assets/Resources/Icons/app_icon_1024x1024.png.meta` — import settings (texture type, compression, max size)
- `ProjectSettings/ProjectSettings.asset` — Android icon configuration (default, adaptive, legacy)

### Architecture Notes
- Unity 6.0 handles icon scaling automatically from the 1024x1024 source
- For Adaptive Icon, the foreground has ~66% safe zone — the cat's face is centered so this works naturally
- Current state: all Android icon slots in PlayerSettings are **empty** (`m_Textures: []`) — ready for configuration
- Icon kinds in PlayerSettings: Kind 0 (main), Kind 1 (round), Kind 2 (adaptive)
- Required sizes per kind: 432px, 324px, 216px, 162px, 108px, 81px

### Important Constraints
- Compression must be `None` — icons must stay crisp at all sizes
- Do NOT use Addressables for the app icon — it's a build-time asset, not runtime-loaded
- This is a Unity Editor / ProjectSettings change, no runtime code needed

### References
- [Source: _bmad-output/game-architecture.md — Asset Organization]
- [Source: ProjectSettings/ProjectSettings.asset — Android icon configuration]
- [Source: _bmad-output/visual-direction-tropical-fresh.md — color palette context]

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References
N/A — no runtime code changes, Editor/ProjectSettings only

### Completion Notes List
- Verified app_icon_1024x1024.png exists in Assets/Resources/Icons/
- Fixed .meta import settings: textureType 8→0 (Default), spriteMode 2→1 (Single), maxTextureSize 2048→1024, textureCompression 1→0 (None), sRGB already true
- Created solid-color background PNG (adaptive_bg_1024x1024.png, #0a1e32) for adaptive icon background layer
- Configured m_BuildTargetIcons with default Android icon reference
- Set all 6 adaptive icon sizes (Kind 2) with background + foreground texture layers
- Set all 6 legacy icon sizes (Kind 0) with icon texture
- Left all 6 round icon sizes (Kind 1) empty — Android auto-generates from adaptive layers
- No runtime code changes — purely Editor/asset configuration

### File List
- src/JuiceSort/Assets/Resources/Icons/app_icon_1024x1024.png.meta (modified — import settings)
- src/JuiceSort/Assets/Resources/Icons/adaptive_bg_1024x1024.png (new — solid color #0a1e32 background for adaptive icon)
- src/JuiceSort/Assets/Resources/Icons/adaptive_bg_1024x1024.png.meta (new — import settings for background)
- src/JuiceSort/ProjectSettings/ProjectSettings.asset (modified — Android icon configuration)
