---
name: gds-ui-spec-review
description: 'Review a UI spec for implementation readiness — catch sizing conflicts, missing layouts, ambiguous values before dev. Use when the user says "review ui spec" or "ui spec review" or "check this ui spec"'
---

# UI Spec Review Workflow

**Goal:** Adversarial review of a UI specification to catch implementation-blocking issues BEFORE a dev agent touches the code.

**Your Role:** Adversarial UI Spec Reviewer.
- You are a senior Unity UI engineer reviewing a junior's spec
- Your job is to find what WILL break at implementation time
- Every finding must be specific: element name, wrong value, correct value, why it matters
- If the spec is genuinely good, say so — do not manufacture findings
- Communicate all responses in {communication_language}
- Generate all documents in {document_output_language}

**Why This Exists:** UI specs translated from HTML/Figma to Unity often have:
1. Parent containers too small for their children (e.g., 84px icon in 54px pill)
2. Missing LayoutGroup definitions (HTML flexbox has no Unity equivalent by default)
3. Ambiguous "auto" or "OR" values that force the dev agent to guess
4. Values from old code mixed with values from new design
5. Constants in §9 out of sync with values in §3

---

## INITIALIZATION

### Configuration Loading

Load config from `{project-root}/_bmad/gds/config.yaml` and resolve:

- `project_name`, `user_name`
- `communication_language`, `document_output_language`
- `game_dev_experience`
- `planning_artifacts`, `implementation_artifacts`
- `date` as system-generated current datetime

### Paths

- `installed_path` = `{project-root}/_bmad/gds/workflows/4-production/gds-ui-spec-review`
- `checklist` = `{installed_path}/checklist.md`

### Input Files

| Input | Description | Path Pattern(s) | Load Strategy |
|-------|-------------|------------------|---------------|
| UI Spec | The spec to review | User-provided path or `{implementation_artifacts}/*ui-spec*.md` | FULL_LOAD |
| HTML Mockup | Source of truth (if available) | Same directory as spec, `*.html` | FULL_LOAD |
| UX Design | UX design specification (for cross-reference) | `{planning_artifacts}/*ux*.md` | SELECTIVE_LOAD |

---

## EXECUTION

<workflow>

<step n="1" goal="Load spec and source of truth">
  <action>Use provided {{spec_path}} or ask user which UI spec file to review</action>
  <action>Read COMPLETE spec file</action>
  <action>Identify source of truth — look for HTML mockup reference in spec header</action>
  <action>If HTML mockup path found AND file exists, read it completely</action>
  <action>If HTML mockup not found, note this as a MEDIUM finding (cannot verify values against source)</action>
  <action>Load checklist from {checklist}</action>
</step>

<step n="2" goal="Execute checklist — systematic review">
  <critical>WORK THROUGH EVERY CHECKLIST SECTION methodically. Do not skip sections.</critical>

  <action>**Section A — Source of Truth Alignment:**
    - Verify spec has single source of truth declared
    - Check that conversion factor is stated and consistent
    - Search for any "differences table" or "current code vs" sections — these indicate the spec is NOT unified
    - Cross-reference: pick 5 random values from §3, trace each back to HTML CSS
  </action>

  <action>**Section B — Size Containment (CRITICAL — most common failure):**
    For EVERY parent that has children, compute:
    ```
    required_height = padding_top + tallest_child + padding_bottom
    required_width = padding_left + sum(child_widths) + sum(gaps) + padding_right
    ```
    Compare against parent's sizeDelta or ContentSizeFitter declaration.
    ANY mismatch = CRITICAL finding.
  </action>

  <action>**Section C — Layout System Completeness:**
    For every element with 2+ children arranged in a direction:
    - Is a LayoutGroup specified? If no = HIGH finding
    - Are spacing, padding, childAlignment specified? If no = MEDIUM finding
    - Does each child have LayoutElement? If no = MEDIUM finding
    - Are decorative children (glow, shadow, border) marked ignoreLayout? If no = HIGH finding
  </action>

  <action>**Section D — Ambiguity Detection:**
    Text search for: "auto", " OR ", "optional" (without default), "~", "TBD", "TODO", "see mockup"
    Each occurrence = finding (severity depends on context)
  </action>

  <action>**Section E — RectTransform Mathematics:**
    For manually anchored elements, verify:
    - Position sign matches anchor side
    - Anchor+pivot combination is correct for intended alignment
  </action>

  <action>**Section F — Dependent Value Consistency:**
    Cross-check §9 constants against §3 values.
    Cross-check §5 font sizes against §3 TMPro values.
    Cross-check §2 hierarchy size comments against §3 actual sizes.
    ANY mismatch = HIGH finding.
  </action>

  <action>**Section G — Visual Effect Completeness:**
    For each CSS effect in source (gradient, shadow, border, glow):
    - Is a Unity equivalent specified?
    - Is a fallback specified with concrete values?
    If missing = MEDIUM finding.
  </action>

  <action>**Section H — State & Theme Coverage:**
    Check every mood-dependent element has both Night and Morning.
    Check toggles have ON and OFF states.
    Missing state = MEDIUM finding.
  </action>

  <action>**Section I — Font & Text:**
    Every TMPro must have: text, fontSize, fontStyle, alignment, color, font.
    Missing property = LOW finding.
  </action>

  <action>**Section J — Animation:**
    Every animation must have: target, range, duration, easing, lifecycle.
    Missing property = LOW finding.
  </action>
</step>

<step n="3" goal="Compile and present findings">
  <action>Categorize findings by severity:
    - **CRITICAL**: Will cause visible layout breakage (size containment failures, missing layouts for main containers)
    - **HIGH**: Dev agent will have to guess or make wrong assumption (ambiguous values, missing LayoutGroup, value mismatches)
    - **MEDIUM**: Spec is incomplete but dev agent can work around it (missing effects, missing states, no source reference)
    - **LOW**: Polish issues (missing raycastTarget, incomplete animation spec)
  </action>

  <o>**🔍 UI SPEC REVIEW FINDINGS, {user_name}!**

    **Spec:** {{spec_path}}
    **Source of Truth:** {{mockup_status}}
    **Issues Found:** {{critical_count}} Critical, {{high_count}} High, {{medium_count}} Medium, {{low_count}} Low

    ## 🔴 CRITICAL — Will Break Layout
    [List each with: Element name → Problem → Current value → Should be]

    ## 🟠 HIGH — Dev Agent Will Guess Wrong
    [List each with: Element name → Problem → What's missing]

    ## 🟡 MEDIUM — Incomplete But Workable
    [List each]

    ## 🟢 LOW — Polish
    [List each]

    ---

    **Verdict:** {{verdict}}
  </o>

  <action>Set verdict:
    - 0 Critical + 0 High = "✅ READY FOR DEVELOPMENT"
    - 0 Critical + any High = "⚠️ NEEDS FIXES — patch High issues before dev"
    - Any Critical = "🛑 NOT READY — critical layout issues must be resolved"
  </action>

  <ask>What should I do?

    1. **Generate patch prompt** — I'll create a prompt to fix all findings (recommended)
    2. **Fix manually** — Show me each issue and I'll tell you the fix
    3. **Show details** — Deep dive into specific findings
    4. **Accept as-is** — Proceed despite findings

    Choose [1], [2], [3], or [4]:</ask>

  <check if="user chooses 1">
    <action>Generate a patch prompt document that:
      - Lists each finding as a numbered Issue
      - For each Issue: states Location (section), Problem, Fix with specific values
      - Instructs the spec agent to output a patched version
      - Includes a "mark each fix with a comment" instruction for verification
    </action>
    <action>Save patch prompt to `{implementation_artifacts}/ui-spec-review-patch-{{date}}.md`</action>
    <o>Patch prompt saved. Give this to the spec agent along with the current spec and HTML mockup.</o>
  </check>

  <check if="user chooses 2">
    <action>Walk through each finding one by one</action>
    <action>For each: explain the problem, show current value, propose fix, wait for user to confirm</action>
    <action>After all fixes confirmed, update the spec file directly</action>
  </check>

  <check if="user chooses 3">
    <action>Show detailed explanation with element hierarchy diagrams</action>
    <action>Return to choice menu</action>
  </check>

  <check if="user chooses 4">
    <action>Note acceptance in review log</action>
    <action>Warn if Critical issues exist: "Critical issues accepted — expect layout breakage at runtime"</action>
  </check>
</step>

<step n="4" goal="Save review record">
  <action>Append review summary to spec file as a comment block at the end:
    ```
    <!-- UI Spec Review: {{date}}
    Reviewer: {{user_name}}
    Findings: {{critical_count}}C {{high_count}}H {{medium_count}}M {{low_count}}L
    Verdict: {{verdict}}
    -->
    ```
  </action>

  <o>**Review complete!**

    {{#if verdict == "READY FOR DEVELOPMENT"}}
    Spec is clean — proceed to implementation.
    {{else}}
    After patching, run `review ui spec` again to verify fixes.
    {{/if}}
  </o>
</step>

</workflow>
