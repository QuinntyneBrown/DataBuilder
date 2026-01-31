# Form Field Inconsistency Report

**Date:** January 31, 2026
**Issue:** Generated Angular Material form fields do not match the Admin UI Implementation Guide's prescribed input styling

---

## Summary

The generated IdeaApp uses Angular Material's `mat-form-field` with `appearance="outline"`, which produces inputs with transparent backgrounds and floating labels. However, the **ADMIN-UI-IMPLEMENTATION-GUIDE.md** specifies a **filled input style** with solid backgrounds and static labels above the input.

This creates a visual inconsistency between what the guide prescribes and what the generated app produces.

---

## Guide Specification vs. Generated Output

### What the Guide Specifies

**Reference: ADMIN-UI-IMPLEMENTATION-GUIDE.md, Section 8.1 "Form Layout" (lines 1598-1623)**

```css
/* Text Input */
.form-input {
  width: 100%;
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);  /* #2d2d2d - FILLED background */
  border: 1px solid var(--surface-divider);
  border-radius: var(--radius-sm);            /* 4px */
  color: var(--text-primary);
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
  transition: border-color var(--transition-fast);
}

.form-input:focus {
  outline: none;
  border-color: var(--primary-500);
}
```

**Guide HTML pattern (lines 1650-1661):**
```html
<div class="form-field">
  <label class="form-label">Display Name</label>
  <input type="text" class="form-input" value="John Smith">
</div>
```

**Key characteristics prescribed by the guide:**
| Property | Guide Value | Description |
|----------|-------------|-------------|
| Background | `var(--surface-elevated)` (#2d2d2d) | **Filled/solid** background |
| Border | `1px solid var(--surface-divider)` | Simple border all around |
| Border radius | `var(--radius-sm)` (4px) | Small radius |
| Label position | Above input (static) | Separate `<label>` element |
| Label style | Uppercase, 12px, secondary color | `text-transform: uppercase` |

### What the Generated App Produces

**Generated code (idea-detail.component.ts, lines 99-105):**
```html
<mat-form-field appearance="outline" class="form-field">
  <mat-label>Title</mat-label>
  <input matInput formControlName="title">
  @if (form.get('title')?.hasError('required')) {
    <mat-error>Title is required</mat-error>
  }
</mat-form-field>
```

**Key characteristics of generated output:**
| Property | Generated Value | Description |
|----------|-----------------|-------------|
| Background | Transparent | No fill, just outline |
| Border | Notched outline | Complex outline with label notch |
| Border radius | Angular Material default | ~4px but with notch |
| Label position | Floating inside outline | Animates on focus |
| Label style | Sentence case, standard size | No uppercase transformation |

---

## Visual Comparison

### Guide Style (Filled Input)
```
┌─────────────────────────────────────┐
│ DISPLAY NAME                        │  ← Static label above (uppercase, 12px)
├─────────────────────────────────────┤
│ ████████████████████████████████████│  ← Filled background (#2d2d2d)
│ John Smith                          │  ← Input text
│ ████████████████████████████████████│
└─────────────────────────────────────┘
  ↑ 1px solid border all around
```

### Generated Style (Angular Material Outline)
```
┌──── Display Name ───────────────────┐  ← Floating label in notch
│                                     │  ← Transparent background
│ John Smith                          │  ← Input text
│                                     │
└─────────────────────────────────────┘
  ↑ Outline border with notch for label
```

---

## Root Cause Analysis

### 1. Framework Mismatch

The ADMIN-UI-IMPLEMENTATION-GUIDE.md was written as a **framework-agnostic** style guide using vanilla HTML and CSS. It prescribes custom CSS classes (`.form-input`, `.form-label`) that would be applied to native HTML `<input>` elements.

However, the generated app uses **Angular Material** components (`mat-form-field`, `matInput`) which have their own opinionated styling system that doesn't directly map to the guide's CSS.

### 2. Insufficient Angular Material Customization

**File: `styles.scss` (lines 145-152)**

The global styles only minimally customize Angular Material form fields:

```scss
/* Form field enhancements */
.mat-mdc-form-field {
  width: 100%;
}

.mat-mdc-form-field-outline {
  color: var(--surface-divider) !important;
}
```

This changes the outline color but does **not**:
- Add a filled background
- Change the label behavior
- Match the border radius to the guide

### 3. Appearance Choice

The generated components use `appearance="outline"`:

```html
<mat-form-field appearance="outline" class="form-field">
```

Angular Material offers multiple appearances:
- `outline` - Transparent with outline border (currently used)
- `fill` - Has a filled background (closer to the guide)
- `legacy` - Older style (deprecated)

---

## Custom Styles Causing the Inconsistency

### In Component Files

**idea-list.component.ts (lines 185-192):**
```css
.search-field {
  flex: 1;
  max-width: 400px;
}

.search-field ::ng-deep .mat-mdc-form-field-subscript-wrapper {
  display: none;  /* Hides hint/error area - deviates from Material guidelines */
}
```

**idea-detail.component.ts (lines 276-282):**
```css
.form-field {
  width: 100%;
}

.form-field--full {
  grid-column: 1 / -1;
}
```

These styles don't cause the visual inconsistency but also don't fix it.

### In Global Styles

**styles.scss (lines 145-152):**
```scss
.mat-mdc-form-field {
  width: 100%;
}

.mat-mdc-form-field-outline {
  color: var(--surface-divider) !important;
}
```

**Missing customizations to match the guide:**
- No background color override
- No label position/style override
- No removal of the notch behavior

---

## Guide References

The following sections of ADMIN-UI-IMPLEMENTATION-GUIDE.md specify the filled input style:

| Section | Lines | Relevant CSS |
|---------|-------|--------------|
| 8.1 Form Layout | 1598-1623 | `.form-input { background-color: var(--surface-elevated); ... }` |
| 8.2 Toggle Switch Fields | 1686-1694 | `.toggle-field { background-color: var(--surface-elevated); ... }` |
| 6.1 Filter Bar | 1036-1056 | `.search-input { background-color: var(--surface-elevated); ... }` |
| 8.3 Info Grid | 1769-1795 | Read-only data display pattern |

All form input patterns in the guide consistently use `background-color: var(--surface-elevated)` (#2d2d2d).

---

## Recommendations

### Option A: Update Generated Code to Match Guide (Change App)

Modify the generated Angular code to use `appearance="fill"` and add custom styling:

```scss
// In styles.scss
.mat-mdc-form-field.mat-form-field-appearance-fill {
  .mat-mdc-text-field-wrapper {
    background-color: var(--surface-elevated) !important;
    border-radius: var(--radius-sm) !important;
  }

  .mat-mdc-form-field-focus-overlay {
    background-color: transparent !important;
  }
}
```

And change templates to use `appearance="fill"`:
```html
<mat-form-field appearance="fill" class="form-field">
```

### Option B: Update Guide to Support Angular Material (Change Guide)

Add a section to the ADMIN-UI-IMPLEMENTATION-GUIDE.md specifically for Angular Material:

```markdown
### 8.4 Angular Material Form Fields

When using Angular Material, use `appearance="outline"` for consistency with
Material Design 3 patterns. The outline appearance provides:
- Clear visual affordance for input areas
- Accessible floating labels
- Built-in error state handling

Customize the outline color to match the design system:
```scss
.mat-mdc-form-field-outline {
  color: var(--surface-divider) !important;
}
```
```

### Option C: Hybrid Approach (Recommended)

1. **Keep Angular Material outline appearance** for its accessibility and UX benefits
2. **Add filled background** to match the guide's visual style
3. **Update the guide** to document Angular Material-specific patterns

```scss
// Add to styles.scss
.mat-mdc-text-field-wrapper {
  background-color: var(--surface-elevated) !important;
}

.mat-mdc-form-field-outline {
  color: var(--surface-divider) !important;
}

// Keep the floating label but match colors
.mat-mdc-floating-label {
  color: var(--text-secondary) !important;
}
```

---

## Conclusion

The inconsistency arises from using Angular Material's `appearance="outline"` (transparent background, floating label) while the ADMIN-UI-IMPLEMENTATION-GUIDE.md specifies a filled input style with solid background and static labels.

**Root cause:** The guide was written for vanilla HTML/CSS but the generator produces Angular Material components without the necessary customizations to match the guide's visual specifications.

**Impact:** Visual inconsistency between documented design system and generated output.

**Fix:** Either customize Angular Material to match the guide, update the guide to reflect Angular Material patterns, or implement a hybrid approach that combines both.
