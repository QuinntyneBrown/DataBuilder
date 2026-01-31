# IdeaApp UI Style Audit Report

**Date:** January 31, 2026
**Audited Against:** `docs/ADMIN-UI-IMPLEMENTATION-GUIDE.md`
**Project:** `artifacts/IdeaApp/src/IdeaApp.Ui`

---

## Executive Summary

This audit reviews the inline styles in the IdeaApp Angular UI components against the Admin UI Implementation Guide. Overall, the components follow the structural patterns well but have consistency issues with hardcoded values instead of CSS variables.

| Component | Compliance | Issues |
|-----------|------------|--------|
| main-layout.component.ts | **Good** | Minor - missing some variables |
| idea-list.component.ts | **Fair** | Hardcoded colors/spacing |
| idea-detail.component.ts | **Fair** | Hardcoded colors/spacing |
| category-list.component.ts | **Fair** | Hardcoded colors/spacing |
| category-detail.component.ts | **Fair** | Hardcoded colors/spacing, unused styles |

---

## 1. main-layout.component.ts

### Styles Following the Guide

| Style/Pattern | Guide Reference | Status |
|--------------|-----------------|--------|
| CSS variables in `:host` | Section 2.1 | Follows guide |
| `--primary-500: #2196f3` | Section 2.1 | Correct |
| `--surface-background: #121212` | Section 2.1 | Correct |
| `--surface-card: #1e1e1e` | Section 2.1 | Correct |
| `--text-primary: rgba(255, 255, 255, 0.87)` | Section 2.1 | Correct |
| `--text-secondary: rgba(255, 255, 255, 0.60)` | Section 2.1 | Correct |
| `--spacing-*` (xs, sm, md, lg, xl) | Section 2.1 | Correct |
| `--elevation-4` box shadow | Section 2.1 | Correct |
| `.app-container` flex column layout | Section 5.1 | Follows guide |
| Toolbar height `64px` | Section 5.1 | Correct |
| Sidenav width `256px` | Section 5.1 | Correct |
| `.toolbar-spacer { flex: 1 }` | Section 5.1 | Correct |
| Active link with `--surface-selected` | Section 5.2 | Correct |
| Hover state with `--surface-hover` | Section 5.2 | Correct |

### Styles Not Needed / Could Be Improved

| Style | Issue | Recommendation |
|-------|-------|----------------|
| `border-radius: 4px` (line 167) | Hardcoded value | Use `var(--radius-sm)` |
| Missing `--radius-*` variables | Incomplete token set | Add radius tokens from guide |
| Missing `--transition-*` variables | Incomplete token set | Add transition tokens |
| Missing `--status-*` variables | Incomplete token set | Add status color tokens |

### Assessment: **GOOD**
The main layout component is well-structured and properly uses CSS variables. It serves as the design system foundation for child components.

---

## 2. idea-list.component.ts

### Styles Following the Guide

| Style/Pattern | Guide Reference | Status |
|--------------|-----------------|--------|
| `.page-header` flex layout | Section 5.3 | Follows guide |
| `.page-title` 28px, font-weight 400 | Section 3.1 | Correct |
| `.page-subtitle` 14px, secondary color | Section 3.1 | Correct |
| `.filter-bar` with mat-card | Section 6.1 | Follows guide |
| `.pagination` flex between layout | Section 9.2 | Follows guide |
| `.monospace` font-family | Section 3.1 | Follows guide |
| `.actions-cell` flex layout | Section 9.1 | Follows guide |
| `.table-row:hover` background | Section 9.1 | Follows guide |

### Styles Not Needed / Issues

| Style | Issue | Recommendation |
|-------|-------|----------------|
| `padding: 32px` (line 140) | Hardcoded | Use `var(--spacing-xl)` |
| `margin-bottom: 32px` (line 151) | Hardcoded | Use `var(--spacing-xl)` |
| `margin-bottom: 24px` (line 181) | Hardcoded | Use `var(--spacing-lg)` |
| `gap: 16px` (line 179) | Hardcoded | Use `var(--spacing-md)` |
| `padding: 16px` (line 182) | Hardcoded | Use `var(--spacing-md)` |
| `border-radius: 8px` (line 183, 197) | Hardcoded | Use `var(--radius-md)` |
| `color: rgba(255, 255, 255, 0.87)` (line 162) | Hardcoded | Use `var(--text-primary)` |
| `color: rgba(255, 255, 255, 0.6)` (lines 167, 212, etc.) | Hardcoded | Use `var(--text-secondary)` |
| `rgba(255, 255, 255, 0.04)` (line 206) | Wrong hover opacity | Use `var(--surface-hover)` (0.08) |
| `rgba(255, 255, 255, 0.12)` (line 235) | Hardcoded | Use `var(--surface-divider)` |
| `color: #f44336` (line 252) | Hardcoded | Use `var(--warn-500)` |
| Missing CSS variables | No `:host` variable definitions | Add variables or inherit from parent |
| `.add-button` height 40px | Non-standard | Guide uses 36px for buttons |

### Unused Styles
None identified - all styles are in use.

### Assessment: **FAIR**
The structural patterns are correct, but the component uses hardcoded values throughout instead of CSS variables, reducing maintainability and consistency.

---

## 3. idea-detail.component.ts

### Styles Following the Guide

| Style/Pattern | Guide Reference | Status |
|--------------|-----------------|--------|
| `.breadcrumb` flex layout | Section 5.4 | Follows guide |
| `.page-header` flex layout | Section 5.3 | Follows guide |
| `.page-title` 28px, font-weight 400 | Section 3.1 | Correct |
| `.form-card` with header/divider/content | Section 4.2 | Follows guide |
| `.form-row` 2-column grid | Section 8.1 | Correct |
| `.form-field--full` grid-column span | Section 8.1 | Correct |
| `.toggle-field` layout | Section 8.2 | Follows guide |
| `.toggle-label__title` / `__description` | Section 8.2 | Correct |
| `.monospace` / `.json-input` | Section 3.1 | Follows guide |
| Responsive breakpoint at 768px | Best practice | Good |

### Styles Not Needed / Issues

| Style | Issue | Recommendation |
|-------|-------|----------------|
| `padding: 32px` (line 179) | Hardcoded | Use `var(--spacing-xl)` |
| `gap: 8px` (line 188) | Hardcoded | Use `var(--spacing-sm)` |
| `margin-bottom: 24px` (line 189) | Hardcoded | Use `var(--spacing-lg)` |
| `font-size: 18px` (line 147) | Hardcoded | Use `var(--spacing-sm)` for separator icon |
| `color: rgba(255, 255, 255, 0.6)` (multiple) | Hardcoded | Use `var(--text-secondary)` |
| `color: rgba(255, 255, 255, 0.87)` (multiple) | Hardcoded | Use `var(--text-primary)` |
| `border-radius: 8px` (line 243) | Hardcoded | Use `var(--radius-md)` |
| `border-radius: 4px` (line 306) | Hardcoded | Use `var(--radius-sm)` |
| `background-color: rgba(255, 255, 255, 0.05)` (line 305) | Non-standard | Use `var(--surface-elevated)` (#2d2d2d) |
| `var(--mat-primary)` (line 194) | Angular Material variable | Use `var(--primary-500)` for consistency |
| `gap: 24px` (line 274) | Hardcoded | Use `var(--spacing-lg)` |
| Missing CSS variables | No `:host` variable definitions | Add variables or inherit from parent |

### Unused Styles
None identified - all styles appear to be in use.

### Assessment: **FAIR**
Follows structural patterns from the guide but uses hardcoded values. The toggle field background uses a lighter shade than the guide recommends.

---

## 4. category-list.component.ts

### Styles Following the Guide
**Identical to idea-list.component.ts** - Same patterns apply.

### Styles Not Needed / Issues
**Same issues as idea-list.component.ts:**
- All hardcoded color values
- All hardcoded spacing values
- All hardcoded border-radius values
- Missing CSS variables definition

### Assessment: **FAIR**

---

## 5. category-detail.component.ts

### Styles Following the Guide
**Nearly identical to idea-detail.component.ts** - Same patterns apply.

### Styles Not Needed / Issues

**Same issues as idea-detail.component.ts, plus:**

| Style | Issue | Recommendation |
|-------|-------|----------------|
| `.json-input` (lines 228-231) | **Unused** | Remove - no JSON fields in template |
| `.toggle-field` section (lines 243-272) | **Unused** | Remove - no toggle fields in template |
| `.toggle-label` (lines 258-261) | **Unused** | Remove - not used in template |
| `.toggle-label__title` (lines 263-267) | **Unused** | Remove - not used in template |
| `.toggle-label__description` (lines 269-272) | **Unused** | Remove - not used in template |

### Assessment: **FAIR**
Has the same issues as other components plus unused styles that should be removed to reduce bundle size.

---

## Recommendations

### High Priority

1. **Create a shared styles file or use CSS custom properties inheritance**
   - Define CSS variables once in `styles.scss` or a shared component
   - Child components can then reference these variables

2. **Replace all hardcoded color values with CSS variables:**
   ```css
   /* Instead of: */
   color: rgba(255, 255, 255, 0.87);
   /* Use: */
   color: var(--text-primary);
   ```

3. **Replace all hardcoded spacing values with CSS variables:**
   ```css
   /* Instead of: */
   padding: 32px;
   gap: 16px;
   /* Use: */
   padding: var(--spacing-xl);
   gap: var(--spacing-md);
   ```

### Medium Priority

4. **Add missing CSS variables to main-layout.component.ts:**
   - `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-chip`
   - `--transition-fast`, `--transition-standard`
   - `--status-success`, `--status-warning`, `--status-error`, `--status-info`

5. **Fix hover opacity:**
   - Change `rgba(255, 255, 255, 0.04)` to `var(--surface-hover)` which is `rgba(255, 255, 255, 0.08)`

6. **Standardize button height:**
   - Guide specifies 36px for buttons, but components use 40px

### Low Priority

7. **Remove unused styles from category-detail.component.ts:**
   - `.json-input`
   - `.toggle-field` and related styles

8. **Use consistent breadcrumb link color:**
   - Replace `var(--mat-primary)` with `var(--primary-500)`

---

## Style Variable Reference

For quick reference, here are the CSS variables from the guide that should be used:

### Colors
| Usage | Variable | Value |
|-------|----------|-------|
| Primary text | `--text-primary` | `rgba(255, 255, 255, 0.87)` |
| Secondary text | `--text-secondary` | `rgba(255, 255, 255, 0.60)` |
| Disabled text | `--text-disabled` | `rgba(255, 255, 255, 0.38)` |
| Page background | `--surface-background` | `#121212` |
| Card background | `--surface-card` | `#1e1e1e` |
| Elevated surfaces | `--surface-elevated` | `#2d2d2d` |
| Dividers | `--surface-divider` | `rgba(255, 255, 255, 0.12)` |
| Hover state | `--surface-hover` | `rgba(255, 255, 255, 0.08)` |
| Selected state | `--surface-selected` | `rgba(33, 150, 243, 0.16)` |
| Primary color | `--primary-500` | `#2196f3` |
| Warn/Error | `--warn-500` | `#f44336` |

### Spacing
| Size | Variable | Value |
|------|----------|-------|
| Extra small | `--spacing-xs` | `4px` |
| Small | `--spacing-sm` | `8px` |
| Medium | `--spacing-md` | `16px` |
| Large | `--spacing-lg` | `24px` |
| Extra large | `--spacing-xl` | `32px` |

### Border Radius
| Usage | Variable | Value |
|-------|----------|-------|
| Buttons, inputs | `--radius-sm` | `4px` |
| Cards, dialogs | `--radius-md` | `8px` |
| Large containers | `--radius-lg` | `12px` |
| Chips | `--radius-chip` | `16px` |

---

## Conclusion

The IdeaApp UI components follow the structural patterns defined in the Admin UI Implementation Guide well. The main issues are:

1. **Inconsistent use of CSS variables** - Most components use hardcoded values instead of the defined CSS custom properties
2. **Missing variable definitions** - Only main-layout.component.ts defines CSS variables; other components don't inherit or define their own
3. **Unused styles** - category-detail.component.ts contains styles for features not present in its template

Addressing these issues will improve maintainability, ensure visual consistency, and make future theme changes easier to implement.
