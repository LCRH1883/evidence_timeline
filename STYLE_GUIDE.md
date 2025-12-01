# Evidence Timeline - UI Style Guide

## Overview

This style guide defines the visual and interaction design standards for the Evidence Timeline application. All UI components should follow these guidelines to ensure consistency, readability, and a professional appearance.

## Design Principles

1. **Compact and Efficient** - Maximize usable space without cluttering the interface
2. **Consistent and Predictable** - Similar elements should look and behave the same way
3. **Readable and Clear** - Information hierarchy should be immediately apparent
4. **Professional and Clean** - Minimal visual noise, focused on functionality

---

## Typography

### Font Sizes

All font sizes use **pixels (px)** for consistency:

| Style | Size | Usage |
|-------|------|-------|
| **Header Text** | 13px | Section headers, primary headings |
| **Section Header** | 12px | Subsection headers, group labels |
| **Label Text** | 11px | Form labels, field names |
| **Body Text** | 11px | Content, descriptions, list items |
| **Small Text** | 10px | Save status, timestamps, secondary info |

### Font Weights

- **SemiBold (600)** - Headers and labels
- **Normal (400)** - Body text and content
- **Bold (700)** - Rarely used, only for emphasis

### Typography Styles

Use predefined styles from `App.xaml`:

```xaml
<!-- Headers -->
<TextBlock Style="{StaticResource HeaderText}" />

<!-- Section headers -->
<TextBlock Style="{StaticResource SectionHeaderText}" />

<!-- Form labels -->
<TextBlock Style="{StaticResource LabelText}" />

<!-- Body content -->
<TextBlock Style="{StaticResource BodyText}" />
```

---

## Colors

### Color Palette

| Color Name | Hex Value | Usage |
|------------|-----------|-------|
| **BorderBrush** | `#dee2e6` | Borders, dividers, separators |
| **BackgroundLight** | `#f8f9fa` | Header backgrounds, panels |
| **TextPrimary** | `#212529` | Main text color |
| **TextSecondary** | `#6c757d` | Secondary text, disabled states |

### Usage in XAML

Always use static resources instead of hardcoded colors:

```xaml
<Border BorderBrush="{StaticResource BorderBrush}"
        Background="{StaticResource BackgroundLight}" />
```

---

## Spacing System

Use a consistent spacing scale based on **multiples of 4px**:

| Value | Usage |
|-------|-------|
| **4px** | Tight spacing (label to control, between small elements) |
| **6px** | Medium spacing (between form fields) |
| **8px** | Standard spacing (content margins, panel padding) |
| **12px** | Large spacing (between sections, dialog margins) |
| **16px** | Extra large spacing (window margins for dialogs) |

### Common Spacing Patterns

**Form Fields:**
```xaml
<TextBlock Style="{StaticResource LabelText}" Margin="0 0 0 2" />
<TextBox Margin="0 0 0 6" />
```

**Sections:**
```xaml
<StackPanel Margin="0 0 0 8">
    <!-- Section content -->
</StackPanel>
```

**Dialog Content:**
```xaml
<Grid Margin="8">  <!-- Standard dialog margin -->
    <!-- Or -->
<Grid Margin="12">  <!-- Larger dialog margin for forms -->
```

---

## Buttons

### Button Styles

Three main button styles are available:

#### 1. Standard Button (default)
```xaml
<Button Content="Action" />
```
- **Size:** MinHeight 26px, MinWidth 60px
- **Padding:** 10px horizontal, 4px vertical
- **Usage:** Primary actions, toolbars

#### 2. Dialog Button
```xaml
<Button Style="{StaticResource DialogButton}" Content="OK" />
```
- **Size:** Width 80px, Height 26px
- **Padding:** 12px horizontal, 4px vertical
- **Usage:** Dialog OK/Cancel/Save buttons

#### 3. Icon Button
```xaml
<Button Style="{StaticResource IconButton}">
    <iconPacks:PackIconBootstrapIcons Kind="Floppy" Width="14" Height="14" />
</Button>
```
- **Size:** 28px × 28px
- **Usage:** Toolbar icons, compact actions

#### 4. Compact Icon Button
```xaml
<Button Style="{StaticResource CompactIconButton}">
    <iconPacks:PackIconBootstrapIcons Kind="X" Width="12" Height="12" />
</Button>
```
- **Size:** 24px × 24px
- **Usage:** Very compact spaces, inline actions

### Button Spacing

In horizontal button groups:
```xaml
<StackPanel Orientation="Horizontal">
    <Button Content="Save" Margin="0 0 6 0" />
    <Button Content="Cancel" />
</StackPanel>
```

---

## Form Controls

### Standard Controls

All form controls inherit consistent sizing:

| Control | Font Size | Padding | Min Height |
|---------|-----------|---------|------------|
| TextBox | 11px | 6px horizontal, 4px vertical | 26px |
| ComboBox | 11px | 6px horizontal, 4px vertical | 26px |
| DatePicker | 11px | 6px horizontal, 4px vertical | 26px |
| CheckBox | 11px | — | — |

### Control Margins

Standard pattern for form fields:
```xaml
<TextBlock Text="Field Name" Style="{StaticResource LabelText}" Margin="0 0 0 2" />
<TextBox Margin="0 0 0 6" />
```

---

## Icons

### Icon System

The application uses **Bootstrap Icons** via MahApps.Metro.IconPacks.

### Standard Icon Sizes

| Context | Size | Example |
|---------|------|---------|
| Toolbar buttons | 16×16px | New Evidence, Open |
| Compact buttons | 14×14px | Save, formatting buttons |
| Small inline buttons | 12×12px | Remove, close |

### Common Icons

| Action | Icon Name | Kind Value |
|--------|-----------|------------|
| Save | Floppy disk | `Floppy` |
| Close/Remove | X | `X` |
| Delete | Trash can | `Trash` |
| Open | Folder open | `Folder2Open` |
| New file | File plus | `FileEarmarkPlus` |
| Open external | Arrow up right | `BoxArrowUpRight` |
| Bold | Bold B | `TypeBold` |
| Italic | Italic I | `TypeItalic` |
| Underline | Underline U | `TypeUnderline` |
| Bullet list | List bullets | `ListUl` |
| Numbered list | List numbers | `ListOl` |

### Icon Usage

```xaml
<iconPacks:PackIconBootstrapIcons Kind="Floppy" Width="14" Height="14" />
```

---

## Lists and Tables

### DataGrid

```xaml
<DataGrid FontSize="11" RowHeight="28" ColumnHeaderHeight="30" />
```

### ListBox

```xaml
<ListBox FontSize="11" Padding="4" />
```

---

## Containers

### GroupBox

```xaml
<GroupBox Header="Section Title">
    <Grid Margin="6">
        <!-- Content with 6px padding -->
    </Grid>
</GroupBox>
```

### Border

Always use resource for border color:
```xaml
<Border BorderBrush="{StaticResource BorderBrush}"
        BorderThickness="1"
        Padding="8">
```

---

## Dialogs

### Dialog Layout Pattern

```xaml
<adonis:AdonisWindow>
    <Grid Margin="12">  <!-- or 8 for smaller dialogs -->
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Dialog content -->
        <ScrollViewer Grid.Row="0" VerticalScrollBarVisibility="Auto">
            <!-- Form fields -->
        </ScrollViewer>

        <!-- Button panel -->
        <StackPanel Grid.Row="1"
                    Orientation="Horizontal"
                    HorizontalAlignment="Right"
                    Margin="0 8 0 0">
            <Button Style="{StaticResource DialogButton}"
                    Content="OK"
                    Margin="0 0 6 0"
                    IsDefault="True" />
            <Button Style="{StaticResource DialogButton}"
                    Content="Cancel"
                    IsCancel="True" />
        </StackPanel>
    </Grid>
</adonis:AdonisWindow>
```

---

## Headers and Title Bars

### Window Title Bar

```xaml
<Border DockPanel.Dock="Top"
        Background="{StaticResource BackgroundLight}"
        BorderBrush="{StaticResource BorderBrush}"
        BorderThickness="0 0 0 1"
        Padding="8 4">
    <DockPanel>
        <TextBlock Text="Title"
                   FontSize="12"
                   FontWeight="SemiBold"
                   VerticalAlignment="Center" />
    </DockPanel>
</Border>
```

### Section Headers

```xaml
<Border Background="{StaticResource BackgroundLight}"
        BorderBrush="{StaticResource BorderBrush}"
        BorderThickness="0 0 0 1"
        Padding="8 4">
    <DockPanel VerticalAlignment="Center">
        <TextBlock Text="Section Name"
                   Style="{StaticResource SectionHeaderText}"
                   VerticalAlignment="Center"
                   DockPanel.Dock="Left" />
        <!-- Optional actions on right -->
    </DockPanel>
</Border>
```

---

## Menu System

### Main Menu

```xaml
<Menu DockPanel.Dock="Top">
    <MenuItem Header="_File">
        <MenuItem Header="_New Case" Command="{Binding NewCommand}" />
        <Separator />
        <MenuItem Header="E_xit" />
    </MenuItem>
</Menu>
```

Menu items inherit 11px font size and 8px×4px padding.

---

## Best Practices

### DO:

✅ Use static resources for colors
✅ Use predefined styles for typography
✅ Follow the spacing scale (4px, 6px, 8px, 12px, 16px)
✅ Use `DialogButton` style for dialog buttons
✅ Keep form field margins consistent (0 0 0 6)
✅ Use Bootstrap Icons for all icons

### DON'T:

❌ Hardcode colors in XAML
❌ Mix font sizes arbitrarily
❌ Use random margin/padding values
❌ Create custom button styles inline
❌ Forget to add labels to form fields
❌ Use Material Design icons (use Bootstrap instead)

---

## Implementation Checklist

When creating new UI:

- [ ] All colors use static resources
- [ ] All typography uses predefined styles
- [ ] Spacing follows the 4px scale
- [ ] Buttons use appropriate styles
- [ ] Icons are Bootstrap Icons with correct sizes
- [ ] Form fields have consistent margins
- [ ] Dialogs follow the standard layout pattern
- [ ] Headers use proper background/border colors
- [ ] All margins are multiples of 2px minimum

---

## Resources

All styles are defined in:
- **App.xaml** - Global application styles and resources

Icons are provided by:
- **MahApps.Metro.IconPacks** - Bootstrap Icons package
- Icon browser: https://icons.getbootstrap.com/

---

*Last Updated: 2025-11-30*
