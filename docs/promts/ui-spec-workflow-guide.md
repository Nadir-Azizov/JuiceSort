# UI Spec Workflow Guide

## Flow

```
Claude.ai          →  HTML mockup
Claude Code         →  quick-spec (spec yarat)
Claude Code         →  review ui spec (yoxla)
Claude Code         →  quick-spec (patch et, lazım olsa)
Claude Code         →  review ui spec (təkrar yoxla)
Claude Code (YENİ)  →  quick-dev (implement et)
```

## Addımlar

### 1. HTML mockup yarat
Claude.ai-da (və ya yeni chat-da) dizaynı HTML-də yarat. Vizual olaraq gözəl görünənə qədər iterate et. Burada agent yoxdur, sadəcə söhbət.

### 2. `gds-quick-spec` ilə UI spec yarat
Claude Code-da `quick-spec` de, HTML mockup-u ver. Spec yarananda ona de ki "bütün dəyərlər HTML CSS × 3 olmalıdır, LayoutGroup-lar hər flex container üçün yazılmalıdır, sizeDelta-da auto olmamalıdır." `ui-spec-regen-prompt.md` faylını reference kimi istifadə et.

### 3. `review ui spec` ilə spec-i yoxla
Claude Code-da `review ui spec` de, yaranan spec-i ver. Checklist-ə görə yoxlayacaq, tapıntıları verəcək, patch prompt generate edəcək.

### 4. Patch et
Review-dan CRITICAL və ya HIGH issue çıxsa, generate olunan patch prompt-u götür, spec agent-ə ver, düzəltdir. Sonra yenidən `review ui spec` — ta ki "READY FOR DEVELOPMENT" verdicti alana qədər.

### 5. `gds-quick-dev` ilə implement et
Spec təmiz olanda Claude Code-da **yeni context-də** `quick-dev {spec-path}` de. Yeni context vacibdir — dev agent-in yaddaşı təmiz olmalıdır ki yalnız spec-ə fokuslanasın.

## Əsas Qayda
**Review addımını heç vaxt atla.** 5 dəqiqə review = saatlarla debug-dan xilas.
