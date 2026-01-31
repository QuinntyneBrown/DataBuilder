# Admin UI Style Guide & Implementation Recipe

A comprehensive, system-agnostic guide for building Admin UIs with a consistent Material Design 3 dark theme. This guide provides prescriptive patterns for CRUD interfaces, complex entities with child relationships, and all supporting UI components.

---

## Table of Contents

1. [Design Foundation](#1-design-foundation)
2. [CSS Variables & Tokens](#2-css-variables--tokens)
3. [Typography System](#3-typography-system)
4. [Component Library](#4-component-library)
5. [Layout Patterns](#5-layout-patterns)
6. [CRUD UI Patterns](#6-crud-ui-patterns)
7. [Complex Entity Patterns](#7-complex-entity-patterns)
8. [Form Patterns](#8-form-patterns)
9. [Data Display Patterns](#9-data-display-patterns)
10. [Complete Code Examples](#10-complete-code-examples)

---

## 1. Design Foundation

### 1.1 Core Principles

| Principle | Description |
|-----------|-------------|
| **Dark Theme First** | Optimized for extended use; reduces eye strain |
| **Material Design 3** | Google's latest design language |
| **8px Grid System** | All spacing based on multiples of 8px |
| **Elevation via Shadow** | Depth communicated through box-shadow |
| **Color for Meaning** | Status colors convey information at a glance |

### 1.2 Required Dependencies

```html
<!-- Fonts -->
<link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&family=Roboto+Mono:wght@400;500&display=swap" rel="stylesheet">

<!-- Icons -->
<link href="https://fonts.googleapis.com/icon?family=Material+Icons+Outlined" rel="stylesheet">
```

### 1.3 Base Reset

```css
*, *::before, *::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

html, body {
  height: 100%;
  font-family: 'Roboto', sans-serif;
  background-color: var(--surface-background);
  color: var(--text-primary);
  font-size: 14px;
  line-height: 1.5;
}
```

---

## 2. CSS Variables & Tokens

### 2.1 Complete Token Set

```css
:root {
  /* ============================================
     PRIMARY COLORS (Blue)
     ============================================ */
  --primary-50: #e3f2fd;
  --primary-100: #bbdefb;
  --primary-200: #90caf9;
  --primary-300: #64b5f6;
  --primary-400: #42a5f5;
  --primary-500: #2196f3;   /* Main brand color */
  --primary-600: #1e88e5;   /* Hover state */
  --primary-700: #1976d2;   /* Active/pressed state */
  --primary-800: #1565c0;
  --primary-900: #0d47a1;

  /* ============================================
     ACCENT COLORS (Orange)
     ============================================ */
  --accent-500: #ff9800;    /* Secondary actions, FAB */
  --accent-600: #fb8c00;
  --accent-700: #f57c00;

  /* ============================================
     WARN/ERROR COLORS (Red)
     ============================================ */
  --warn-500: #f44336;      /* Destructive actions */
  --warn-600: #e53935;
  --warn-700: #d32f2f;

  /* ============================================
     SURFACE COLORS (Dark Theme)
     ============================================ */
  --surface-background: #121212;           /* Page background */
  --surface-card: #1e1e1e;                 /* Cards, toolbar, sidenav */
  --surface-elevated: #2d2d2d;             /* Elevated elements within cards */
  --surface-dialog: #2d2d2d;               /* Modal backgrounds */
  --surface-divider: rgba(255, 255, 255, 0.12);
  --surface-hover: rgba(255, 255, 255, 0.08);
  --surface-selected: rgba(33, 150, 243, 0.16);

  /* ============================================
     TEXT COLORS
     ============================================ */
  --text-primary: rgba(255, 255, 255, 0.87);
  --text-secondary: rgba(255, 255, 255, 0.60);
  --text-disabled: rgba(255, 255, 255, 0.38);
  --text-hint: rgba(255, 255, 255, 0.38);

  /* ============================================
     STATUS COLORS
     ============================================ */
  --status-success: #4caf50;    /* Green - active, success */
  --status-warning: #ff9800;    /* Orange - warning, locked */
  --status-error: #f44336;      /* Red - error, inactive */
  --status-info: #2196f3;       /* Blue - information */

  /* ============================================
     SPACING (8px base unit)
     ============================================ */
  --spacing-xs: 4px;     /* 0.5x */
  --spacing-sm: 8px;     /* 1x */
  --spacing-md: 16px;    /* 2x */
  --spacing-lg: 24px;    /* 3x */
  --spacing-xl: 32px;    /* 4x */
  --spacing-xxl: 48px;   /* 6x */

  /* ============================================
     BORDER RADIUS
     ============================================ */
  --radius-xs: 2px;
  --radius-sm: 4px;      /* Buttons, inputs */
  --radius-md: 8px;      /* Cards, dialogs */
  --radius-lg: 12px;     /* Large containers */
  --radius-chip: 16px;   /* Chips, pills */
  --radius-full: 50%;    /* Circular elements */

  /* ============================================
     ELEVATION (Box Shadows)
     ============================================ */
  --elevation-0: none;
  --elevation-1: 0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24);
  --elevation-2: 0 3px 6px rgba(0,0,0,0.15), 0 2px 4px rgba(0,0,0,0.12);
  --elevation-4: 0 10px 20px rgba(0,0,0,0.15), 0 3px 6px rgba(0,0,0,0.10);
  --elevation-8: 0 15px 25px rgba(0,0,0,0.15), 0 5px 10px rgba(0,0,0,0.05);
  --elevation-16: 0 20px 40px rgba(0,0,0,0.2);
  --elevation-24: 0 25px 50px rgba(0,0,0,0.25);

  /* ============================================
     TRANSITIONS
     ============================================ */
  --transition-fast: 150ms ease;
  --transition-standard: 300ms ease;
  --transition-slow: 500ms ease;
}
```

---

## 3. Typography System

### 3.1 Type Scale

```css
/* Headlines */
.headline-1 { font-size: 96px; font-weight: 300; letter-spacing: -1.5px; }
.headline-2 { font-size: 60px; font-weight: 300; letter-spacing: -0.5px; }
.headline-3 { font-size: 48px; font-weight: 400; letter-spacing: 0; }
.headline-4 { font-size: 34px; font-weight: 400; letter-spacing: 0.25px; }
.headline-5 { font-size: 24px; font-weight: 400; letter-spacing: 0; }
.headline-6 { font-size: 20px; font-weight: 500; letter-spacing: 0.15px; }

/* Page Titles - Primary heading for pages */
.page-title {
  font-size: 28px;
  font-weight: 400;
  color: var(--text-primary);
  margin-bottom: var(--spacing-xs);
}

/* Subtitles */
.subtitle-1 { font-size: 16px; font-weight: 400; letter-spacing: 0.15px; }
.subtitle-2 { font-size: 14px; font-weight: 500; letter-spacing: 0.1px; }

/* Page Subtitle */
.page-subtitle {
  font-size: 14px;
  color: var(--text-secondary);
}

/* Body Text */
.body-1 { font-size: 16px; font-weight: 400; letter-spacing: 0.5px; }
.body-2 { font-size: 14px; font-weight: 400; letter-spacing: 0.25px; }

/* Captions and Labels */
.caption {
  font-size: 12px;
  font-weight: 400;
  letter-spacing: 0.4px;
  color: var(--text-secondary);
}

/* Overline - Used for labels above fields */
.overline {
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.5px;
  text-transform: uppercase;
  color: var(--text-secondary);
}

/* Button Text */
.button-text {
  font-size: 14px;
  font-weight: 500;
  letter-spacing: 1.25px;
  text-transform: uppercase;
}

/* Monospace - For data, codes, IDs */
.monospace {
  font-family: 'Roboto Mono', monospace;
}
```

---

## 4. Component Library

### 4.1 Buttons

```css
/* Base Button */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0 var(--spacing-md);
  height: 36px;
  border: none;
  border-radius: var(--radius-sm);
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 1.25px;
  cursor: pointer;
  gap: var(--spacing-sm);
  transition: all var(--transition-fast);
  text-decoration: none;
}

/* Primary Button - Main actions (Save, Add, Create) */
.btn--primary {
  background-color: var(--primary-500);
  color: white;
  box-shadow: var(--elevation-2);
}

.btn--primary:hover {
  background-color: var(--primary-700);
  box-shadow: var(--elevation-4);
}

/* Outlined/Stroked Button - Secondary actions (Cancel, Back) */
.btn--outlined {
  background-color: transparent;
  color: var(--primary-500);
  border: 1px solid var(--primary-500);
}

.btn--outlined:hover {
  background-color: rgba(33, 150, 243, 0.08);
}

/* Danger Button - Destructive actions (Delete, Deactivate) */
.btn--danger {
  background-color: transparent;
  color: var(--warn-500);
  border: 1px solid var(--warn-500);
}

.btn--danger:hover {
  background-color: rgba(244, 67, 54, 0.08);
}

/* Warn Button - Warning actions (Deactivate, Revoke) */
.btn--warn {
  background-color: transparent;
  color: var(--warn-500);
  border: 1px solid var(--warn-500);
}

/* Disabled State */
.btn:disabled {
  opacity: 0.38;
  cursor: not-allowed;
  pointer-events: none;
}

/* Icon Button - Circular buttons for icons */
.icon-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--text-primary);
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.icon-button:hover {
  background-color: var(--surface-hover);
}

.icon-button:disabled {
  color: var(--text-disabled);
  cursor: not-allowed;
}

/* FAB (Floating Action Button) */
.fab {
  width: 56px;
  height: 56px;
  border-radius: var(--radius-full);
  background-color: var(--accent-500);
  color: white;
  border: none;
  box-shadow: var(--elevation-4);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
}

.fab:hover {
  box-shadow: var(--elevation-8);
  background-color: var(--accent-700);
}
```

**Button HTML Examples:**
```html
<!-- Primary Action -->
<button class="btn btn--primary">
  <span class="material-icons-outlined">save</span>
  Save Changes
</button>

<!-- Secondary Action -->
<button class="btn btn--outlined">
  <span class="material-icons-outlined">cancel</span>
  Cancel
</button>

<!-- Danger Action -->
<button class="btn btn--danger">
  <span class="material-icons-outlined">delete</span>
  Delete
</button>

<!-- Icon Button -->
<button class="icon-button" title="Edit">
  <span class="material-icons-outlined">edit</span>
</button>
```

### 4.2 Cards

```css
/* Base Card */
.mat-card {
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
  box-shadow: var(--elevation-1);
  overflow: hidden;
}

/* Card with padding (for forms) */
.mat-card--padded {
  padding: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
}

/* Card Header */
.mat-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--surface-divider);
}

.mat-card__title {
  font-size: 18px;
  font-weight: 500;
  color: var(--text-primary);
}

.mat-card__subtitle {
  font-size: 12px;
  color: var(--text-secondary);
  margin-top: var(--spacing-xs);
}

/* Card Content */
.mat-card__content {
  padding: var(--spacing-md);
}

/* Card Actions */
.mat-card__actions {
  display: flex;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  border-top: 1px solid var(--surface-divider);
}
```

**Card HTML Example:**
```html
<div class="mat-card mat-card--padded">
  <div class="mat-card__header">
    <div>
      <h2 class="mat-card__title">Basic Information</h2>
      <p class="mat-card__subtitle">User identity and description</p>
    </div>
    <!-- Optional: Header actions -->
    <button class="icon-button">
      <span class="material-icons-outlined">more_vert</span>
    </button>
  </div>

  <!-- Card content goes here -->
  <div class="form-row">
    <!-- Form fields -->
  </div>
</div>
```

### 4.3 Status Chips/Badges

```css
/* Base Chip */
.chip {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: var(--radius-chip);
  font-size: 12px;
  font-weight: 500;
  gap: var(--spacing-xs);
}

/* Status Variants */
.chip--success, .chip--active {
  background-color: rgba(76, 175, 80, 0.2);
  color: var(--status-success);
}

.chip--error, .chip--inactive {
  background-color: rgba(244, 67, 54, 0.2);
  color: var(--status-error);
}

.chip--warning, .chip--locked {
  background-color: rgba(255, 152, 0, 0.2);
  color: var(--status-warning);
}

.chip--info {
  background-color: rgba(33, 150, 243, 0.2);
  color: var(--primary-500);
}

/* Small Badge (for tags in lists) */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  font-size: 11px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.badge--system {
  background-color: rgba(255, 152, 0, 0.2);
  color: var(--accent-500);
}

.badge--custom {
  background-color: rgba(33, 150, 243, 0.2);
  color: var(--primary-500);
}

/* Role Chip (smaller, for role lists) */
.role-chip {
  background-color: rgba(33, 150, 243, 0.2);
  color: var(--primary-500);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 500;
}
```

**Chip HTML Examples:**
```html
<!-- Status Chips -->
<span class="chip chip--active">Active</span>
<span class="chip chip--inactive">Inactive</span>
<span class="chip chip--locked">Locked</span>

<!-- With Icon -->
<span class="chip chip--success">
  <span class="material-icons-outlined" style="font-size: 16px;">check_circle</span>
  Active
</span>

<!-- Badges -->
<span class="badge badge--system">System Role</span>

<!-- Role Chips in a list -->
<div class="role-list">
  <span class="role-chip">Admin</span>
  <span class="role-chip">User</span>
</div>
```

### 4.4 Avatar

```css
/* Base Avatar */
.avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  background: linear-gradient(135deg, var(--primary-500), var(--accent-500));
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 500;
  font-size: 16px;
  color: white;
  flex-shrink: 0;
}

/* Large Avatar (for detail pages) */
.avatar--large {
  width: 80px;
  height: 80px;
  font-size: 32px;
}

/* Entity Icon (for roles, etc.) */
.entity-icon {
  width: 64px;
  height: 64px;
  border-radius: var(--radius-md);
  background: linear-gradient(135deg, var(--primary-500), var(--accent-500));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  color: white;
}
```

**Avatar HTML Examples:**
```html
<!-- Standard Avatar with Initials -->
<div class="avatar">JS</div>

<!-- Large Avatar -->
<div class="avatar avatar--large">JS</div>

<!-- Entity Icon -->
<div class="entity-icon">
  <span class="material-icons-outlined">admin_panel_settings</span>
</div>
```

### 4.5 Notice/Alert Banners

```css
/* Notice Banner */
.notice {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border-radius: var(--radius-sm);
  margin-bottom: var(--spacing-lg);
}

.notice--info {
  background-color: rgba(33, 150, 243, 0.1);
  border-left: 3px solid var(--primary-500);
}

.notice--warning {
  background-color: rgba(255, 152, 0, 0.1);
  border-left: 3px solid var(--accent-500);
}

.notice--error {
  background-color: rgba(244, 67, 54, 0.1);
  border-left: 3px solid var(--warn-500);
}

.notice--success {
  background-color: rgba(76, 175, 80, 0.1);
  border-left: 3px solid var(--status-success);
}

.notice__icon {
  color: inherit;
}

.notice--info .notice__icon { color: var(--primary-500); }
.notice--warning .notice__icon { color: var(--accent-500); }
.notice--error .notice__icon { color: var(--warn-500); }
.notice--success .notice__icon { color: var(--status-success); }

.notice__text {
  color: var(--text-primary);
  font-size: 14px;
}
```

**Notice HTML Example:**
```html
<div class="notice notice--warning">
  <span class="material-icons-outlined notice__icon">info</span>
  <span class="notice__text">System roles cannot be modified. Clone this role to create a custom version.</span>
</div>
```

---

## 5. Layout Patterns

### 5.1 App Shell

```
┌─────────────────────────────────────────────────────────────┐
│                      TOOLBAR (64px)                         │
│  [≡] App Title                        [🔔] [❓] [👤]        │
├───────────┬─────────────────────────────────────────────────┤
│           │                                                 │
│  SIDENAV  │              MAIN CONTENT                       │
│  (256px)  │                                                 │
│           │                                                 │
│  • Item 1 │   ┌─────────────────────────────────────────┐   │
│  • Item 2 │   │ Page Header          [Action Buttons]   │   │
│  ───────  │   └─────────────────────────────────────────┘   │
│  • Item 3 │                                                 │
│  • Item 4 │   ┌─────────────────────────────────────────┐   │
│           │   │ Content Cards / Tables                  │   │
│           │   │                                         │   │
│           │   └─────────────────────────────────────────┘   │
│           │                                                 │
└───────────┴─────────────────────────────────────────────────┘
```

```css
/* App Container */
.app-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

/* Toolbar */
.mat-toolbar {
  display: flex;
  align-items: center;
  padding: 0 var(--spacing-md);
  height: 64px;
  background-color: var(--surface-card);
  box-shadow: var(--elevation-4);
  z-index: 1000;
  flex-shrink: 0;
}

.mat-toolbar__title {
  font-size: 20px;
  font-weight: 500;
  margin-left: var(--spacing-md);
}

.mat-toolbar__spacer {
  flex: 1;
}

/* Layout Container (Sidenav + Content) */
.layout-container {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* Sidenav */
.mat-sidenav {
  width: 256px;
  background-color: var(--surface-card);
  border-right: 1px solid var(--surface-divider);
  overflow-y: auto;
  flex-shrink: 0;
}

/* Main Content */
.main-content {
  flex: 1;
  overflow-y: auto;
  padding: var(--spacing-xl);
}

/* Centered Content (for detail pages) */
.main-content--centered {
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}
```

### 5.2 Sidenav Navigation

```css
/* Navigation List */
.mat-nav-list {
  list-style: none;
  padding: var(--spacing-sm) 0;
}

/* Navigation Item */
.mat-list-item {
  display: flex;
  align-items: center;
  padding: var(--spacing-md) var(--spacing-lg);
  color: var(--text-primary);
  text-decoration: none;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.mat-list-item:hover {
  background-color: var(--surface-hover);
}

/* Active State */
.mat-list-item--active {
  background-color: var(--surface-selected);
  color: var(--primary-500);
}

.mat-list-item__icon {
  margin-right: var(--spacing-md);
  color: var(--text-secondary);
}

.mat-list-item--active .mat-list-item__icon {
  color: var(--primary-500);
}

/* Divider */
.nav-divider {
  margin: var(--spacing-md) 0;
  height: 1px;
  background-color: var(--surface-divider);
}
```

**Sidenav HTML Example:**
```html
<nav class="mat-sidenav">
  <ul class="mat-nav-list">
    <li class="mat-list-item">
      <span class="material-icons-outlined mat-list-item__icon">dashboard</span>
      <span>Dashboard</span>
    </li>
    <li class="mat-list-item mat-list-item--active">
      <span class="material-icons-outlined mat-list-item__icon">people</span>
      <span>User Management</span>
    </li>
    <li class="nav-divider"></li>
    <li class="mat-list-item">
      <span class="material-icons-outlined mat-list-item__icon">settings</span>
      <span>Settings</span>
    </li>
  </ul>
</nav>
```

### 5.3 Page Header Patterns

**List Page Header:**
```css
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-xl);
}

.page-header__info {
  flex: 1;
}
```

```html
<div class="page-header">
  <div class="page-header__info">
    <h2 class="page-title">User Management</h2>
    <p class="page-subtitle">Manage system users and their access</p>
  </div>
  <button class="btn btn--primary">
    <span class="material-icons-outlined">add</span>
    Add User
  </button>
</div>
```

**Detail Page Header:**
```css
.page-header--detail {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: var(--spacing-xl);
}

.page-title-section {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
}

.page-title-content h1 {
  font-size: 28px;
  font-weight: 400;
  margin-bottom: var(--spacing-xs);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.status-badges {
  display: flex;
  gap: var(--spacing-sm);
  margin-top: var(--spacing-sm);
}

.action-buttons {
  display: flex;
  gap: var(--spacing-sm);
}
```

```html
<div class="page-header page-header--detail">
  <div class="page-title-section">
    <div class="avatar avatar--large">JS</div>
    <div class="page-title-content">
      <h1>
        John Smith
        <span class="badge badge--system">Admin</span>
      </h1>
      <p class="page-subtitle">john.smith@example.com</p>
      <div class="status-badges">
        <span class="chip chip--success">
          <span class="material-icons-outlined" style="font-size: 16px;">check_circle</span>
          Active
        </span>
        <span class="chip chip--info">
          <span class="material-icons-outlined" style="font-size: 16px;">verified_user</span>
          MFA Enabled
        </span>
      </div>
    </div>
  </div>
  <div class="action-buttons">
    <button class="btn btn--outlined">
      <span class="material-icons-outlined">lock_reset</span>
      Reset Password
    </button>
    <button class="btn btn--warn">
      <span class="material-icons-outlined">block</span>
      Deactivate
    </button>
    <button class="btn btn--primary">
      <span class="material-icons-outlined">save</span>
      Save Changes
    </button>
  </div>
</div>
```

### 5.4 Breadcrumb Navigation

```css
.breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-lg);
  color: var(--text-secondary);
  font-size: 14px;
}

.breadcrumb a {
  color: var(--primary-500);
  text-decoration: none;
}

.breadcrumb a:hover {
  text-decoration: underline;
}

.breadcrumb__separator {
  font-size: 16px;
  color: var(--text-secondary);
}
```

```html
<nav class="breadcrumb">
  <a href="#">Admin</a>
  <span class="material-icons-outlined breadcrumb__separator">chevron_right</span>
  <a href="#">User Management</a>
  <span class="material-icons-outlined breadcrumb__separator">chevron_right</span>
  <span>John Smith</span>
</nav>
```

---

## 6. CRUD UI Patterns

### 6.1 List View Pattern (READ - Multiple Records)

The List View is the entry point for managing entities. It displays records in a table with filtering, search, and pagination.

**Structure:**
```
┌─────────────────────────────────────────────────────────────┐
│ Page Header                              [+ Add Entity]     │
├─────────────────────────────────────────────────────────────┤
│ Stats Cards (optional)                                      │
│ [Total: 247] [Active: 231] [Inactive: 16] [Locked: 3]      │
├─────────────────────────────────────────────────────────────┤
│ Filter Bar                                                  │
│ [🔍 Search...        ] [Status ▼] [Role ▼] [🔽 Filter]     │
├─────────────────────────────────────────────────────────────┤
│ Data Table                                                  │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ USER     │ ROLES   │ STATUS │ LAST LOGIN │ ACTIONS     │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ [AV] JS  │ [Admin] │ Active │ 2h ago     │ [✏️] [⋮]    │ │
│ │ [AV] SA  │ [User]  │ Active │ 5h ago     │ [✏️] [⋮]    │ │
│ └─────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│ Pagination: 1-5 of 247                        [◀] [▶]      │
└─────────────────────────────────────────────────────────────┘
```

**Stats Cards (Optional):**

Stats Cards are an optional component for list views. Include them when displaying key aggregate metrics adds value to the user experience (e.g., showing counts of total, active, inactive records). Omit them when the metrics are not meaningful or would add unnecessary clutter.

```css
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
}

.stat-card {
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  box-shadow: var(--elevation-1);
}

.stat-card__value {
  font-size: 32px;
  font-weight: 300;
  color: var(--text-primary);
  margin-bottom: var(--spacing-xs);
}

.stat-card__label {
  font-size: 14px;
  color: var(--text-secondary);
}

/* Colored stat values */
.stat-card__value--primary { color: var(--primary-500); }
.stat-card__value--success { color: var(--status-success); }
.stat-card__value--warning { color: var(--status-warning); }
.stat-card__value--error { color: var(--status-error); }
```

**Filter Bar:**
```css
.filter-bar {
  display: flex;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding: var(--spacing-md);
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
}

.search-field {
  flex: 1;
  position: relative;
}

.search-input {
  width: 100%;
  padding: var(--spacing-md) var(--spacing-md) var(--spacing-md) 48px;
  background-color: var(--surface-elevated);
  border: 1px solid var(--surface-divider);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-500);
}

.search-input::placeholder {
  color: var(--text-secondary);
}

.search-icon {
  position: absolute;
  left: var(--spacing-md);
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-secondary);
}

.select-field {
  min-width: 150px;
}

.select-input {
  width: 100%;
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border: 1px solid var(--surface-divider);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
  cursor: pointer;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='rgba(255,255,255,0.6)'%3E%3Cpath d='M7 10l5 5 5-5z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 8px center;
  padding-right: 40px;
}
```

### 6.2 Detail/Edit View Pattern (READ/UPDATE Single Record)

The Detail View shows a single entity with all its properties organized into logical sections (cards).

**Structure:**
```
┌─────────────────────────────────────────────────────────────┐
│ Breadcrumb: Admin > Users > John Smith                      │
├─────────────────────────────────────────────────────────────┤
│ Page Header                                                 │
│ [AV] John Smith                    [Reset] [Deactivate]     │
│      john@example.com               [Save Changes]          │
│      [Active] [MFA Enabled]                                 │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Basic Information                                       │ │
│ │ ─────────────────────────────────────────────────────── │ │
│ │ [Display Name    ] [Email Address   ]                   │ │
│ │ [User ID (disabled)] [Created (disabled)]               │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Security Settings                                       │ │
│ │ ─────────────────────────────────────────────────────── │ │
│ │ [Toggle] Email Verified                                 │ │
│ │ [Toggle] MFA Enabled                                    │ │
│ │ [Toggle] Account Active                                 │ │
│ │                                                         │ │
│ │ Last Login: ...    Failed Attempts: 0                   │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Role Assignment (Child Entity)                          │ │
│ │ ─────────────────────────────────────────────────────── │ │
│ │ [✓] Admin - Full system access           [SYSTEM]       │ │
│ │ [✓] User - Standard access               [SYSTEM]       │ │
│ │ [ ] Read Only - View only access         [SYSTEM]       │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Recent Activity (Read-only Child)                       │ │
│ │ ─────────────────────────────────────────────────────── │ │
│ │ [🔓] Successful Login          2 hours ago              │ │
│ │ [✏️] Profile Updated           1 day ago                │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 6.3 Create View Pattern (CREATE)

The Create View is similar to the Detail View but:
- No breadcrumb to specific entity
- Empty form fields
- No read-only fields (like ID, Created date)
- Primary action is "Create" instead of "Save Changes"
- No destructive actions (Delete, Deactivate)

### 6.4 Delete Pattern

Delete operations typically happen:
1. **From List View**: Via action menu (more_vert icon)
2. **From Detail View**: Via danger button in action area

Always use a confirmation dialog:
```html
<div class="dialog">
  <div class="dialog__header">
    <h3>Delete User?</h3>
  </div>
  <div class="dialog__content">
    <p>Are you sure you want to delete John Smith? This action cannot be undone.</p>
  </div>
  <div class="dialog__actions">
    <button class="btn btn--outlined">Cancel</button>
    <button class="btn btn--danger">Delete</button>
  </div>
</div>
```

---

## 7. Complex Entity Patterns

### 7.1 Entity with Child Entities (One-to-Many)

**Example: Role with Permissions**

A Role entity has many Permission assignments. This is displayed using grouped checkboxes within the Role detail view.

**Pattern: Grouped Permission Assignment**
```css
.permission-groups {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.permission-group {
  background-color: var(--surface-elevated);
  border-radius: var(--radius-sm);
  padding: var(--spacing-md);
}

.permission-group__header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
  font-weight: 500;
  color: var(--text-primary);
}

.permission-group__icon {
  color: var(--primary-500);
}

.permission-actions {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--spacing-md);
  padding-left: var(--spacing-xl);
}

.checkbox-field {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  cursor: pointer;
}

.checkbox-field input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
  accent-color: var(--primary-500);
}

.checkbox-field label {
  cursor: pointer;
  color: var(--text-primary);
  font-size: 14px;
}
```

```html
<div class="mat-card mat-card--padded">
  <div class="mat-card__header">
    <div>
      <h2 class="mat-card__title">Permission Assignment</h2>
      <p class="mat-card__subtitle">Select permissions for this role</p>
    </div>
  </div>

  <div class="permission-groups">
    <!-- Resource Group: Users -->
    <div class="permission-group">
      <div class="permission-group__header">
        <span class="material-icons-outlined permission-group__icon">people</span>
        <span>Users</span>
      </div>
      <div class="permission-actions">
        <div class="checkbox-field">
          <input type="checkbox" id="users-create" checked>
          <label for="users-create">Create</label>
        </div>
        <div class="checkbox-field">
          <input type="checkbox" id="users-read" checked>
          <label for="users-read">Read</label>
        </div>
        <div class="checkbox-field">
          <input type="checkbox" id="users-update" checked>
          <label for="users-update">Update</label>
        </div>
        <div class="checkbox-field">
          <input type="checkbox" id="users-delete">
          <label for="users-delete">Delete</label>
        </div>
        <div class="checkbox-field">
          <input type="checkbox" id="users-admin">
          <label for="users-admin">Admin</label>
        </div>
      </div>
    </div>

    <!-- Resource Group: Settings -->
    <div class="permission-group">
      <div class="permission-group__header">
        <span class="material-icons-outlined permission-group__icon">settings</span>
        <span>Settings</span>
      </div>
      <div class="permission-actions">
        <div class="checkbox-field">
          <input type="checkbox" id="settings-read" checked>
          <label for="settings-read">Read</label>
        </div>
        <div class="checkbox-field">
          <input type="checkbox" id="settings-update">
          <label for="settings-update">Update</label>
        </div>
      </div>
    </div>
  </div>
</div>
```

### 7.2 Entity with Many-to-Many Relationships

**Example: User with Roles**

Users can have multiple Roles. This is displayed as a selectable list with checkboxes.

**Pattern: Role Assignment List**
```css
.role-assignment {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.role-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border-radius: var(--radius-sm);
  transition: background-color var(--transition-fast);
}

.role-item:hover {
  background-color: var(--surface-hover);
}

.role-item__info {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.role-item__details {
  flex: 1;
}

.role-item__name {
  font-weight: 500;
  margin-bottom: 2px;
}

.role-item__description {
  font-size: 12px;
  color: var(--text-secondary);
}

/* Custom Checkbox */
.checkbox {
  width: 20px;
  height: 20px;
  border: 2px solid var(--text-secondary);
  border-radius: 2px;
  background-color: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all var(--transition-fast);
}

.checkbox--checked {
  background-color: var(--primary-500);
  border-color: var(--primary-500);
}

.checkbox--checked::after {
  content: '✓';
  color: white;
  font-size: 14px;
  font-weight: bold;
}
```

```html
<div class="mat-card mat-card--padded">
  <div class="mat-card__header">
    <h2 class="mat-card__title">Role Assignment</h2>
  </div>

  <div class="role-assignment">
    <div class="role-item">
      <div class="role-item__info">
        <div class="checkbox checkbox--checked"></div>
        <div class="role-item__details">
          <div class="role-item__name">Admin</div>
          <div class="role-item__description">Full system access with administrative privileges</div>
        </div>
      </div>
      <span class="badge badge--system">SYSTEM</span>
    </div>

    <div class="role-item">
      <div class="role-item__info">
        <div class="checkbox"></div>
        <div class="role-item__details">
          <div class="role-item__name">User</div>
          <div class="role-item__description">Standard user access</div>
        </div>
      </div>
      <span class="badge badge--system">SYSTEM</span>
    </div>

    <div class="role-item">
      <div class="role-item__info">
        <div class="checkbox"></div>
        <div class="role-item__details">
          <div class="role-item__name">Mission Analyst</div>
          <div class="role-item__description">Custom role for analysis tasks</div>
        </div>
      </div>
      <!-- No badge for custom roles -->
    </div>
  </div>
</div>
```

### 7.3 Read-Only Child Entities (Activity Log)

Some child entities are read-only displays of historical data.

**Pattern: Activity Timeline**
```css
.activity-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.activity-item {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border-radius: var(--radius-sm);
}

.activity-item__icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  background-color: rgba(33, 150, 243, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-500);
  flex-shrink: 0;
}

.activity-item__content {
  flex: 1;
}

.activity-item__title {
  font-weight: 500;
  margin-bottom: 2px;
}

.activity-item__description {
  font-size: 12px;
  color: var(--text-secondary);
}

.activity-item__time {
  font-size: 12px;
  color: var(--text-secondary);
  white-space: nowrap;
}
```

```html
<div class="mat-card mat-card--padded">
  <div class="mat-card__header">
    <h2 class="mat-card__title">Recent Activity</h2>
  </div>

  <div class="activity-list">
    <div class="activity-item">
      <div class="activity-item__icon">
        <span class="material-icons-outlined">login</span>
      </div>
      <div class="activity-item__content">
        <div class="activity-item__title">Successful Login</div>
        <div class="activity-item__description">Logged in from 192.168.1.105</div>
      </div>
      <div class="activity-item__time">2 hours ago</div>
    </div>

    <div class="activity-item">
      <div class="activity-item__icon">
        <span class="material-icons-outlined">edit</span>
      </div>
      <div class="activity-item__content">
        <div class="activity-item__title">Profile Updated</div>
        <div class="activity-item__description">Changed display name</div>
      </div>
      <div class="activity-item__time">1 day ago</div>
    </div>
  </div>
</div>
```

### 7.4 Assigned Entities List (User List on Role)

When viewing a parent entity, show assigned child entities.

**Pattern: Compact User List**
```css
.user-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.user-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border-radius: var(--radius-sm);
  transition: background-color var(--transition-fast);
}

.user-item:hover {
  background-color: var(--surface-hover);
}

.user-info {
  flex: 1;
}

.user-info__name {
  font-weight: 500;
  color: var(--text-primary);
}

.user-info__email {
  font-size: 12px;
  color: var(--text-secondary);
}
```

```html
<div class="mat-card mat-card--padded">
  <div class="mat-card__header">
    <div>
      <h2 class="mat-card__title">Assigned Users</h2>
      <p class="mat-card__subtitle">Users with this role</p>
    </div>
  </div>

  <div class="user-list">
    <div class="user-item">
      <div class="avatar">JD</div>
      <div class="user-info">
        <div class="user-info__name">John Doe</div>
        <div class="user-info__email">john.doe@example.com</div>
      </div>
    </div>
    <div class="user-item">
      <div class="avatar">AS</div>
      <div class="user-info">
        <div class="user-info__name">Alice Smith</div>
        <div class="user-info__email">alice.smith@example.com</div>
      </div>
    </div>
  </div>
</div>
```

---

## 8. Form Patterns

### 8.1 Form Layout

```css
/* Two-column form row */
.form-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
}

/* Single form field */
.form-field {
  display: flex;
  flex-direction: column;
}

/* Full-width field (spans both columns) */
.form-field--full {
  grid-column: 1 / -1;
}

/* Field Label */
.form-label {
  font-size: 12px;
  font-weight: 500;
  color: var(--text-secondary);
  margin-bottom: var(--spacing-xs);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* Text Input */
.form-input {
  width: 100%;
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border: 1px solid var(--surface-divider);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
  transition: border-color var(--transition-fast);
}

.form-input:focus {
  outline: none;
  border-color: var(--primary-500);
}

.form-input:disabled {
  color: var(--text-disabled);
  cursor: not-allowed;
}

.form-input::placeholder {
  color: var(--text-hint);
}

/* Textarea */
.form-textarea {
  min-height: 100px;
  resize: vertical;
}

/* Helper Text */
.form-hint {
  font-size: 12px;
  color: var(--text-secondary);
  margin-top: var(--spacing-xs);
}

/* Error State */
.form-input--error {
  border-color: var(--warn-500);
}

.form-error {
  font-size: 12px;
  color: var(--warn-500);
  margin-top: var(--spacing-xs);
}
```

```html
<div class="form-row">
  <div class="form-field">
    <label class="form-label">Display Name</label>
    <input type="text" class="form-input" value="John Smith">
  </div>
  <div class="form-field">
    <label class="form-label">Email Address</label>
    <input type="email" class="form-input" value="john@example.com">
    <div class="form-hint">Email is used for login and notifications</div>
  </div>
</div>

<div class="form-row">
  <div class="form-field">
    <label class="form-label">User ID</label>
    <input type="text" class="form-input" value="a1b2c3d4-..." disabled>
  </div>
  <div class="form-field">
    <label class="form-label">Created At</label>
    <input type="text" class="form-input" value="January 15, 2025" disabled>
  </div>
</div>

<!-- Full width field -->
<div class="form-row">
  <div class="form-field form-field--full">
    <label class="form-label">Description</label>
    <textarea class="form-input form-textarea">Full description here...</textarea>
  </div>
</div>
```

### 8.2 Toggle Switch Fields

```css
.toggle-field {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  background-color: var(--surface-elevated);
  border-radius: var(--radius-sm);
  margin-bottom: var(--spacing-md);
}

.toggle-label {
  display: flex;
  flex-direction: column;
}

.toggle-label__title {
  font-weight: 500;
  margin-bottom: var(--spacing-xs);
}

.toggle-label__description {
  font-size: 12px;
  color: var(--text-secondary);
}

/* Toggle Switch */
.toggle-switch {
  width: 44px;
  height: 24px;
  background-color: rgba(255, 255, 255, 0.3);
  border-radius: 12px;
  position: relative;
  cursor: pointer;
  transition: background-color var(--transition-fast);
  flex-shrink: 0;
}

.toggle-switch--checked {
  background-color: rgba(33, 150, 243, 0.5);
}

.toggle-switch__thumb {
  width: 20px;
  height: 20px;
  background-color: #fafafa;
  border-radius: var(--radius-full);
  position: absolute;
  top: 2px;
  left: 2px;
  box-shadow: var(--elevation-1);
  transition: all var(--transition-fast);
}

.toggle-switch--checked .toggle-switch__thumb {
  background-color: var(--primary-500);
  left: 22px;
}
```

```html
<div class="toggle-field">
  <div class="toggle-label">
    <div class="toggle-label__title">Email Verified</div>
    <div class="toggle-label__description">User has verified their email address</div>
  </div>
  <div class="toggle-switch toggle-switch--checked">
    <div class="toggle-switch__thumb"></div>
  </div>
</div>

<div class="toggle-field">
  <div class="toggle-label">
    <div class="toggle-label__title">Multi-Factor Authentication</div>
    <div class="toggle-label__description">Require additional verification at login</div>
  </div>
  <div class="toggle-switch">
    <div class="toggle-switch__thumb"></div>
  </div>
</div>
```

### 8.3 Info Grid (Read-Only Data)

```css
.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-lg);
}

.info-item {
  display: flex;
  flex-direction: column;
}

.info-item__label {
  font-size: 12px;
  color: var(--text-secondary);
  margin-bottom: var(--spacing-xs);
}

.info-item__value {
  font-size: 14px;
  font-weight: 500;
}

.info-item__value--success { color: var(--status-success); }
.info-item__value--warning { color: var(--status-warning); }
.info-item__value--error { color: var(--status-error); }
```

```html
<div class="info-grid">
  <div class="info-item">
    <div class="info-item__label">Last Login</div>
    <div class="info-item__value">January 19, 2025 at 5:30 AM</div>
  </div>
  <div class="info-item">
    <div class="info-item__label">Failed Login Attempts</div>
    <div class="info-item__value">0</div>
  </div>
  <div class="info-item">
    <div class="info-item__label">Active Sessions</div>
    <div class="info-item__value">2</div>
  </div>
  <div class="info-item">
    <div class="info-item__label">Lockout Status</div>
    <div class="info-item__value info-item__value--success">Not Locked</div>
  </div>
</div>
```

---

## 9. Data Display Patterns

### 9.1 Data Table

```css
.mat-table {
  width: 100%;
  border-collapse: collapse;
  background-color: var(--surface-card);
}

.mat-table th,
.mat-table td {
  padding: var(--spacing-md);
  text-align: left;
  border-bottom: 1px solid var(--surface-divider);
}

.mat-table th {
  font-weight: 500;
  color: var(--text-secondary);
  background-color: var(--surface-elevated);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.mat-table tbody tr {
  transition: background-color var(--transition-fast);
}

.mat-table tbody tr:hover {
  background-color: var(--surface-hover);
}

.mat-table tbody tr:last-child td {
  border-bottom: none;
}

/* Table cell with user info */
.user-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

/* Role list in cell */
.role-list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
}

/* Actions cell */
.actions-cell {
  display: flex;
  gap: var(--spacing-xs);
}
```

### 9.2 Pagination

```css
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  border-top: 1px solid var(--surface-divider);
}

.pagination__info {
  color: var(--text-secondary);
  font-size: 14px;
}

.pagination__controls {
  display: flex;
  gap: var(--spacing-sm);
}
```

```html
<div class="pagination">
  <div class="pagination__info">1-10 of 247 users</div>
  <div class="pagination__controls">
    <button class="icon-button" disabled>
      <span class="material-icons-outlined">chevron_left</span>
    </button>
    <button class="icon-button">
      <span class="material-icons-outlined">chevron_right</span>
    </button>
  </div>
</div>
```

### 9.3 Statistics Cards

```css
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
}

.stat-card {
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  box-shadow: var(--elevation-1);
  text-align: center;
}

.stat-card__value {
  font-size: 32px;
  font-weight: 300;
  margin-bottom: var(--spacing-xs);
}

.stat-card__label {
  font-size: 12px;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
```

```html
<div class="stats-grid">
  <div class="stat-card">
    <div class="stat-card__value">247</div>
    <div class="stat-card__label">Total Users</div>
  </div>
  <div class="stat-card">
    <div class="stat-card__value" style="color: var(--status-success);">231</div>
    <div class="stat-card__label">Active</div>
  </div>
  <div class="stat-card">
    <div class="stat-card__value" style="color: var(--status-error);">16</div>
    <div class="stat-card__label">Inactive</div>
  </div>
  <div class="stat-card">
    <div class="stat-card__value" style="color: var(--status-warning);">3</div>
    <div class="stat-card__label">Locked</div>
  </div>
</div>
```

---

## 10. Complete Code Examples

### 10.1 Complete List View (User Management)

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>User Management</title>
  <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap" rel="stylesheet">
  <link href="https://fonts.googleapis.com/icon?family=Material+Icons+Outlined" rel="stylesheet">
  <style>
    /* Include all CSS variables and component styles from above */
    :root {
      --primary-500: #2196f3;
      --accent-500: #ff9800;
      --warn-500: #f44336;
      --surface-background: #121212;
      --surface-card: #1e1e1e;
      --surface-elevated: #2d2d2d;
      --surface-divider: rgba(255, 255, 255, 0.12);
      --surface-hover: rgba(255, 255, 255, 0.08);
      --text-primary: rgba(255, 255, 255, 0.87);
      --text-secondary: rgba(255, 255, 255, 0.60);
      --text-disabled: rgba(255, 255, 255, 0.38);
      --status-success: #4caf50;
      --status-warning: #ff9800;
      --status-error: #f44336;
      --spacing-xs: 4px;
      --spacing-sm: 8px;
      --spacing-md: 16px;
      --spacing-lg: 24px;
      --spacing-xl: 32px;
      --radius-sm: 4px;
      --radius-md: 8px;
      --elevation-1: 0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24);
      --elevation-2: 0 3px 6px rgba(0,0,0,0.15), 0 2px 4px rgba(0,0,0,0.12);
      --elevation-4: 0 10px 20px rgba(0,0,0,0.15), 0 3px 6px rgba(0,0,0,0.10);
    }

    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
    html, body {
      height: 100%;
      font-family: 'Roboto', sans-serif;
      background-color: var(--surface-background);
      color: var(--text-primary);
      font-size: 14px;
    }

    /* App Shell */
    .app-container { display: flex; flex-direction: column; height: 100vh; }
    .mat-toolbar {
      display: flex; align-items: center;
      padding: 0 var(--spacing-md);
      height: 64px;
      background-color: var(--surface-card);
      box-shadow: var(--elevation-4);
      z-index: 1000;
    }
    .mat-toolbar__title { font-size: 20px; font-weight: 500; margin-left: var(--spacing-md); }
    .mat-toolbar__spacer { flex: 1; }
    .layout-container { display: flex; flex: 1; overflow: hidden; }
    .mat-sidenav {
      width: 256px;
      background-color: var(--surface-card);
      border-right: 1px solid var(--surface-divider);
      overflow-y: auto;
    }
    .main-content { flex: 1; overflow-y: auto; padding: var(--spacing-xl); }

    /* Navigation */
    .mat-nav-list { list-style: none; padding: var(--spacing-sm) 0; }
    .mat-list-item {
      display: flex; align-items: center;
      padding: var(--spacing-md) var(--spacing-lg);
      color: var(--text-primary);
      text-decoration: none;
      cursor: pointer;
    }
    .mat-list-item:hover { background-color: var(--surface-hover); }
    .mat-list-item--active {
      background-color: rgba(33, 150, 243, 0.16);
      color: var(--primary-500);
    }
    .mat-list-item__icon { margin-right: var(--spacing-md); color: var(--text-secondary); }
    .mat-list-item--active .mat-list-item__icon { color: var(--primary-500); }

    /* Page Header */
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: var(--spacing-xl);
    }
    .page-title { font-size: 28px; font-weight: 400; }
    .page-subtitle { color: var(--text-secondary); margin-top: var(--spacing-xs); }

    /* Buttons */
    .btn {
      display: inline-flex; align-items: center;
      padding: 0 var(--spacing-md);
      height: 36px; border: none; border-radius: var(--radius-sm);
      font-size: 14px; font-weight: 500;
      text-transform: uppercase; letter-spacing: 1.25px;
      cursor: pointer; gap: var(--spacing-sm);
    }
    .btn--primary {
      background-color: var(--primary-500);
      color: white;
      box-shadow: var(--elevation-2);
    }
    .icon-button {
      display: flex; align-items: center; justify-content: center;
      width: 40px; height: 40px; border: none; border-radius: 50%;
      background: transparent; color: var(--text-primary); cursor: pointer;
    }
    .icon-button:hover { background-color: var(--surface-hover); }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--spacing-md);
      margin-bottom: var(--spacing-lg);
    }
    .stat-card {
      background-color: var(--surface-card);
      border-radius: var(--radius-md);
      padding: var(--spacing-lg);
      box-shadow: var(--elevation-1);
    }
    .stat-card__value { font-size: 32px; font-weight: 300; margin-bottom: var(--spacing-xs); }
    .stat-card__label { font-size: 14px; color: var(--text-secondary); }

    /* Filter Bar */
    .filter-bar {
      display: flex;
      gap: var(--spacing-md);
      margin-bottom: var(--spacing-lg);
      padding: var(--spacing-md);
      background-color: var(--surface-card);
      border-radius: var(--radius-md);
    }
    .search-field { flex: 1; position: relative; }
    .search-input {
      width: 100%;
      padding: var(--spacing-md) var(--spacing-md) var(--spacing-md) 48px;
      background-color: var(--surface-elevated);
      border: 1px solid var(--surface-divider);
      border-radius: var(--radius-sm);
      color: var(--text-primary);
      font-size: 14px;
    }
    .search-input:focus { outline: none; border-color: var(--primary-500); }
    .search-icon {
      position: absolute;
      left: var(--spacing-md);
      top: 50%;
      transform: translateY(-50%);
      color: var(--text-secondary);
    }
    .select-field { min-width: 150px; }
    .select-input {
      width: 100%;
      padding: var(--spacing-md);
      background-color: var(--surface-elevated);
      border: 1px solid var(--surface-divider);
      border-radius: var(--radius-sm);
      color: var(--text-primary);
      font-size: 14px;
      appearance: none;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='rgba(255,255,255,0.6)'%3E%3Cpath d='M7 10l5 5 5-5z'/%3E%3C/svg%3E");
      background-repeat: no-repeat;
      background-position: right 8px center;
      padding-right: 40px;
    }

    /* Table */
    .mat-card {
      background-color: var(--surface-card);
      border-radius: var(--radius-md);
      box-shadow: var(--elevation-1);
      overflow: hidden;
    }
    .mat-table { width: 100%; border-collapse: collapse; }
    .mat-table th, .mat-table td {
      padding: var(--spacing-md);
      text-align: left;
      border-bottom: 1px solid var(--surface-divider);
    }
    .mat-table th {
      font-weight: 500;
      color: var(--text-secondary);
      background-color: var(--surface-elevated);
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .mat-table tr:hover { background-color: var(--surface-hover); }

    /* Chips */
    .chip {
      display: inline-flex;
      align-items: center;
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
    }
    .chip--active { background-color: rgba(76, 175, 80, 0.2); color: var(--status-success); }
    .chip--inactive { background-color: rgba(244, 67, 54, 0.2); color: var(--status-error); }
    .chip--locked { background-color: rgba(255, 152, 0, 0.2); color: var(--status-warning); }

    /* Avatar */
    .avatar {
      width: 40px; height: 40px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--primary-500), var(--accent-500));
      display: flex; align-items: center; justify-content: center;
      font-weight: 500; font-size: 16px;
    }
    .user-cell { display: flex; align-items: center; gap: var(--spacing-md); }
    .user-info__name { font-weight: 500; }
    .user-info__email { font-size: 12px; color: var(--text-secondary); }
    .role-list { display: flex; flex-wrap: wrap; gap: var(--spacing-xs); }
    .role-chip {
      background-color: rgba(33, 150, 243, 0.2);
      color: var(--primary-500);
      padding: 2px 8px;
      border-radius: 10px;
      font-size: 11px;
    }
    .actions-cell { display: flex; gap: var(--spacing-xs); }

    /* Pagination */
    .pagination {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--spacing-md);
      border-top: 1px solid var(--surface-divider);
    }
    .pagination__info { color: var(--text-secondary); font-size: 14px; }
    .pagination__controls { display: flex; gap: var(--spacing-sm); }
  </style>
</head>
<body>
  <div class="app-container">
    <!-- Toolbar -->
    <header class="mat-toolbar">
      <button class="icon-button"><span class="material-icons-outlined">menu</span></button>
      <h1 class="mat-toolbar__title">Admin Panel</h1>
      <div class="mat-toolbar__spacer"></div>
      <button class="icon-button"><span class="material-icons-outlined">notifications</span></button>
      <button class="icon-button"><span class="material-icons-outlined">account_circle</span></button>
    </header>

    <div class="layout-container">
      <!-- Sidenav -->
      <nav class="mat-sidenav">
        <ul class="mat-nav-list">
          <li class="mat-list-item">
            <span class="material-icons-outlined mat-list-item__icon">dashboard</span>
            <span>Dashboard</span>
          </li>
          <li class="mat-list-item mat-list-item--active">
            <span class="material-icons-outlined mat-list-item__icon">people</span>
            <span>User Management</span>
          </li>
          <li class="mat-list-item">
            <span class="material-icons-outlined mat-list-item__icon">admin_panel_settings</span>
            <span>Role Management</span>
          </li>
          <li class="mat-list-item">
            <span class="material-icons-outlined mat-list-item__icon">settings</span>
            <span>Settings</span>
          </li>
        </ul>
      </nav>

      <!-- Main Content -->
      <main class="main-content">
        <div class="page-header">
          <div>
            <h2 class="page-title">User Management</h2>
            <p class="page-subtitle">Manage system users and their access</p>
          </div>
          <button class="btn btn--primary">
            <span class="material-icons-outlined">add</span>
            Add User
          </button>
        </div>

        <!-- Stats -->
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-card__value">247</div>
            <div class="stat-card__label">Total Users</div>
          </div>
          <div class="stat-card">
            <div class="stat-card__value" style="color: var(--status-success);">231</div>
            <div class="stat-card__label">Active Users</div>
          </div>
          <div class="stat-card">
            <div class="stat-card__value" style="color: var(--status-error);">16</div>
            <div class="stat-card__label">Inactive Users</div>
          </div>
          <div class="stat-card">
            <div class="stat-card__value" style="color: var(--status-warning);">3</div>
            <div class="stat-card__label">Locked Users</div>
          </div>
        </div>

        <!-- Filter Bar -->
        <div class="filter-bar">
          <div class="search-field">
            <span class="material-icons-outlined search-icon">search</span>
            <input type="text" class="search-input" placeholder="Search users by name or email...">
          </div>
          <div class="select-field">
            <select class="select-input">
              <option>All Status</option>
              <option>Active</option>
              <option>Inactive</option>
              <option>Locked</option>
            </select>
          </div>
          <div class="select-field">
            <select class="select-input">
              <option>All Roles</option>
              <option>Admin</option>
              <option>User</option>
            </select>
          </div>
        </div>

        <!-- Users Table -->
        <div class="mat-card">
          <table class="mat-table">
            <thead>
              <tr>
                <th>User</th>
                <th>Roles</th>
                <th>Status</th>
                <th>Last Login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>
                  <div class="user-cell">
                    <div class="avatar">JS</div>
                    <div>
                      <div class="user-info__name">John Smith</div>
                      <div class="user-info__email">john.smith@example.com</div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="role-list">
                    <span class="role-chip">Admin</span>
                    <span class="role-chip">User</span>
                  </div>
                </td>
                <td><span class="chip chip--active">Active</span></td>
                <td style="color: var(--text-secondary);">2 hours ago</td>
                <td>
                  <div class="actions-cell">
                    <button class="icon-button" title="Edit">
                      <span class="material-icons-outlined">edit</span>
                    </button>
                    <button class="icon-button" title="More">
                      <span class="material-icons-outlined">more_vert</span>
                    </button>
                  </div>
                </td>
              </tr>
              <tr>
                <td>
                  <div class="user-cell">
                    <div class="avatar">SA</div>
                    <div>
                      <div class="user-info__name">Sarah Anderson</div>
                      <div class="user-info__email">sarah.anderson@example.com</div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="role-list">
                    <span class="role-chip">User</span>
                  </div>
                </td>
                <td><span class="chip chip--active">Active</span></td>
                <td style="color: var(--text-secondary);">5 hours ago</td>
                <td>
                  <div class="actions-cell">
                    <button class="icon-button" title="Edit">
                      <span class="material-icons-outlined">edit</span>
                    </button>
                    <button class="icon-button" title="More">
                      <span class="material-icons-outlined">more_vert</span>
                    </button>
                  </div>
                </td>
              </tr>
              <tr>
                <td>
                  <div class="user-cell">
                    <div class="avatar">MC</div>
                    <div>
                      <div class="user-info__name">Michael Chen</div>
                      <div class="user-info__email">michael.chen@example.com</div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="role-list">
                    <span class="role-chip">User</span>
                  </div>
                </td>
                <td><span class="chip chip--locked">Locked</span></td>
                <td style="color: var(--text-secondary);">3 days ago</td>
                <td>
                  <div class="actions-cell">
                    <button class="icon-button" title="Edit">
                      <span class="material-icons-outlined">edit</span>
                    </button>
                    <button class="icon-button" title="More">
                      <span class="material-icons-outlined">more_vert</span>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="pagination">
            <div class="pagination__info">1-3 of 247 users</div>
            <div class="pagination__controls">
              <button class="icon-button" disabled>
                <span class="material-icons-outlined">chevron_left</span>
              </button>
              <button class="icon-button">
                <span class="material-icons-outlined">chevron_right</span>
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</body>
</html>
```

---

## Summary: Admin UI Recipe

### Quick Reference: Building Any Admin Entity

1. **List View (Index)**
   - Page header with title + "Add" button
   - Stats cards (optional, for key metrics)
   - Filter bar with search + dropdowns
   - Data table with sortable columns
   - Row actions (edit, more menu)
   - Pagination

2. **Detail/Edit View**
   - Breadcrumb navigation
   - Page header with entity info + status badges + action buttons
   - Information notice (if applicable)
   - Card-based sections for logical grouping
   - Form fields in 2-column layout
   - Toggle switches for boolean settings
   - Read-only info grid for metadata
   - Child entity sections (checkboxes, lists)
   - Activity log (read-only timeline)
   - Footer actions (delete, save)

3. **Create View**
   - Similar to detail view
   - No breadcrumb to specific entity
   - No read-only fields
   - "Create" button instead of "Save"
   - No destructive actions

### Key Visual Principles

| Element | Value |
|---------|-------|
| Page background | `#121212` |
| Card background | `#1e1e1e` |
| Elevated background | `#2d2d2d` |
| Primary color | `#2196f3` |
| Accent color | `#ff9800` |
| Success | `#4caf50` |
| Error | `#f44336` |
| Warning | `#ff9800` |
| Text primary | `rgba(255,255,255,0.87)` |
| Text secondary | `rgba(255,255,255,0.60)` |
| Border radius (cards) | `8px` |
| Border radius (buttons) | `4px` |
| Spacing base | `8px` |
| Toolbar height | `64px` |
| Sidenav width | `256px` |

This guide provides everything needed to replicate the Admin UI design system in any framework or technology stack.
