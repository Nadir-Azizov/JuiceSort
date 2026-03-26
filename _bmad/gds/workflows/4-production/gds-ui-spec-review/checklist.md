# UI Spec Review — Validation Checklist

## A. Source of Truth Alignment

- [ ] Spec declares a single source of truth (HTML mockup, Figma, etc.)
- [ ] Every numeric value in §3 (RectTransform) traces back to source via CSS Source column or comment
- [ ] Conversion factor stated and applied consistently (e.g., CSS × 3 for 360px → 1080px)
- [ ] No leftover "old code" values — if a differences table exists, the spec is NOT unified
- [ ] No section contradicts another section (e.g., §3 says one size, §9 constants say another)

## B. Size Containment (Critical)

For EVERY parent-child pair, verify:

- [ ] Parent `sizeDelta` height ≥ largest child height + padding top + padding bottom
- [ ] Parent `sizeDelta` width ≥ sum of child widths + gaps + padding left + padding right
- [ ] If parent uses `ContentSizeFitter`, it is explicitly stated with `horizontalFit` and `verticalFit`
- [ ] If parent uses fixed `sizeDelta`, all children fit within it at the stated sizes
- [ ] No child element is larger than its parent (e.g., 84px icon inside 54px pill = FAIL)

## C. Layout System Completeness

For EVERY element that has multiple children arranged in a row or column:

- [ ] `HorizontalLayoutGroup` or `VerticalLayoutGroup` is specified
- [ ] `spacing` value matches source (CSS `gap` × conversion factor)
- [ ] `padding L/R/T/B` values match source (CSS `padding` × conversion factor)
- [ ] `childAlignment` is specified
- [ ] `childForceExpandWidth` and `childForceExpandHeight` are specified
- [ ] `childControlWidth` and `childControlHeight` are specified
- [ ] Each child has `LayoutElement` with `preferredWidth`/`preferredHeight` or `flexibleWidth`/`flexibleHeight`
- [ ] Elements that should NOT participate in layout have `LayoutElement.ignoreLayout = true` (e.g., glow effects, shadows, border rings)

## D. Ambiguity Detection (Zero Tolerance)

Search the spec for these patterns — each one is a finding:

- [ ] No `sizeDelta` contains the word "auto" — Unity has no auto sizing
- [ ] No "OR" choices in implementation (e.g., "Outline OR 9-slice") — one strategy must be chosen
- [ ] No "optional" without clear default behavior stated
- [ ] No "~" approximate values in `sizeDelta` — all must be exact
- [ ] No "TBD", "TODO", "to be determined" anywhere
- [ ] No "see mockup" without specific extracted values
- [ ] Every "Ideal" / "Fallback" pair has the fallback fully specified (not just "flat color" — which flat color?)

## E. RectTransform Mathematics

For elements with manual anchoring (no LayoutGroup parent):

- [ ] `anchoredPosition` matches source position × conversion factor
- [ ] `anchor + pivot` combination produces the expected alignment (left-aligned = anchorMin.x=0, pivot.x=0)
- [ ] Elements anchored to parent edges have correct sign (negative X for right-anchored, negative Y for top-anchored with top pivot)
- [ ] Stretch-anchored elements: `offsetMin`/`offsetMax` are consistent with stated padding

## F. Dependent Value Consistency

- [ ] §9 Constants match the values used in §3 (e.g., `BarHeight` constant = TopBar sizeDelta.y)
- [ ] §5 Font sizes match what's stated in §3 TMPro sections
- [ ] §4 Colors match what's stated in §3 Visual sections
- [ ] §6 Sprite specifications match what's referenced in §3
- [ ] §2 Hierarchy comments (sizes in parentheses) match §3 actual values
- [ ] If element A's position depends on element B's size (e.g., SettingsPanel Y = -TopBarHeight), both values are in sync

## G. Visual Effect Completeness

For each CSS visual effect in the source:

- [ ] `linear-gradient` → Unity equivalent stated (gradient sprite, material, or flat fallback with specific hex)
- [ ] `box-shadow` (outer) → Glow child specified with size, color, sprite
- [ ] `box-shadow` (3D depth) → Shadow child specified with offset, color
- [ ] `box-shadow` (inset) → Highlight child specified with height, color
- [ ] `border` → Border strategy specified (which approach, width, color per element)
- [ ] `text-shadow` → TMPro Underlay settings specified (color, offset, dilate, softness)
- [ ] `border-radius` → Correct sprite radius referenced from §6
- [ ] `radial-gradient` → Procedural texture or baked sprite specified with colors and offset

## H. State & Theme Coverage

- [ ] Every mood-dependent element has BOTH Night and Morning values
- [ ] Every toggle-able element has BOTH ON and OFF visual states
- [ ] Disabled states specify: which property changes, what value, which component (CanvasGroup, Image, etc.)
- [ ] Default/initial state is specified for every element (especially `SetActive` for hidden panels)

## I. Font & Text

- [ ] Every TMPro element has: text, fontSize, fontStyle, alignment, color, font asset
- [ ] Font weight mapping is stated (CSS weight → TMPro fontStyle)
- [ ] If source uses a different font family than project, mapping is noted
- [ ] `raycastTarget = false` on decorative text elements

## J. Animation

- [ ] Every animated property has: target, range (from → to), duration, easing
- [ ] Lifecycle specified (when starts, when stops)
- [ ] If animation modifies a LayoutGroup child's size, `ignoreLayout` is noted

---

_Reviewer: {{user_name}} on {{date}}_
_Spec file: {{spec_path}}_
_Findings: {{finding_count}} total ({{critical_count}} Critical, {{high_count}} High, {{medium_count}} Medium, {{low_count}} Low)_
