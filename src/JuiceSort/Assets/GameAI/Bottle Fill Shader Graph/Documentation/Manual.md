# Bottle Fill Shader Graph Manual

## Properties
| Name | Reference | Default | Range | Notes |
| --- | --- | --- | --- | --- |
| Band Color 1 | _BandColor1 | RGBA(1, 0.03, 0.03, 1) | - | Topmost fill band color. |
| Band Color 2 | _BandColor2 | RGBA(1, 0.98, 0, 1) | - | Second fill band color. |
| Band Color 3 | _BandColor3 | RGBA(0.05, 0.02, 1, 1) | - | Third fill band color. |
| Band Color 4 | _BandColor4 | RGBA(0, 1, 0.02, 1) | - | Bottom fill band color. |
| Fill Amount | _FillAmount | 0.34 | 0 - 1 | Normalized fill amount. |
| Sprite Height | _SpriteHeight | 0 | - | Sprite height multiplier for fill normalization. |
| Band Scale | _BandScale | 1 | 0.28 - 1 | Controls spacing between bands. |
| Band Count | _BandCount | 4 | 1 - 4 | Number of active fill bands. |
| Alpha Clip Threshold | _AlphaClipThreshold | 0.5 | 0 - 1 | Cutoff threshold for alpha clipping. |

## Usage Notes
- Use a SpriteRenderer or compatible renderer.
- For runtime control, update material properties at runtime.
- If you use MaterialPropertyBlock, keep property names consistent.

## Demo Scene
- Open `Assets/Bottle Fill Shader Graph/Demo.unity`.
- Select a bottle object that has `BottleFillDemoController`.
- Enter Play mode to preview runtime changes.

## BottleFillDemoController
- `Fill Amount` sets the fill height (0–1).
- `Band Scale` controls the spacing between color bands.
- `Palette` defines band colors; palette length determines band count (max 4).
- `Auto Sprite Height` reads the sprite bounds for height normalization.
- `Use Property Block` overrides material inspector values while enabled.

## Support
- Publisher: GameAI
- Email: hello@gameai.one
- URL: https://gameai.one
