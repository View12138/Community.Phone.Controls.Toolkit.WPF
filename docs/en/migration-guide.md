# Migration guide: Windows Phone Toolkit → WPF

## 1. Install

```powershell
dotnet add package Community.Phone.Controls.Toolkit.WPF
```

Add the namespace:

```xml
xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF"
xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone.Controls.Toolkit.WPF"
```

Original WP XAML used `assembly=Microsoft.Phone` and `assembly=Microsoft.Phone.Controls.Toolkit`. Those assembly names are gone; the namespace and type names are preserved where possible, but the `assembly=` value must change.

## 2. Merge the theme resources

The package contains `Themes/Generic.xaml` with Metro colors, fonts, text styles, frame templates and control templates. Implicit control styles load from the assembly theme automatically, but **named** resources used by application XAML (`PhoneTextTitle1Style`, `PhoneAccentBrush`, etc.) should be merged into `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## 3. Application startup and navigation

Replace the WP `PhoneApplicationFrame` startup with a WPF `Window` that hosts a `TransitionFrame`:

```xml
<Window x:Class="MyApp.MainWindow" ...>
    <phone:TransitionFrame x:Name="Frame" />
</Window>
```

```csharp
public MainWindow()
{
    InitializeComponent();
    Frame.Navigate(new HomePage());
}
```

Derive pages from `phone:PhoneApplicationPage`:

```xml
<phone:PhoneApplicationPage
    x:Class="MyApp.HomePage"
    xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF">
    ...
</phone:PhoneApplicationPage>
```

`PhoneApplicationPage` exposes `State`, `NavigationContext`, `Orientation`, `SupportedOrientations`, `NavigatedTo`, `NavigatingFrom`, `NavigatedFrom`, `BackKeyPress`, and `ApplicationBar`. Lifecycle methods are raised by `PhoneApplicationFrame`/`TransitionFrame`, so keep pages inside those frames.

Pass parameters with the frame overload:

```csharp
Frame.Navigate(
    new DetailPage(),
    new Dictionary<string, string> { { "id", "42" }, { "mode", "edit" } });
```

Read them during `OnNavigatedTo`:

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    string id = NavigationContext.QueryString["id"];
}
```

The port merges any URI query string and the supplied dictionary (dictionary values win) into `page.NavigationContext`. `NavigationContext.Uri` and `ExtraData` are also available. `PhoneApplicationPage.State` is retained per page instance by the frame and survives back navigation to that same page.

## 4. Migration of projection XAML

### 4.1 Original Silverlight form (does not compile in WPF)

```xml
<Grid>
    <Grid.Projection>
        <PlaneProjection RotationY="-35" />
    </Grid.Projection>
</Grid>
```

WPF `Grid` has no `Projection` property. There is no C#/XAML extension that can add a property element to a sealed framework type.

### 4.2 Recommended WPF form

```xml
<phone:ProjectionHost Width="320" Height="200">
    <Grid Background="#FF1F1F1F">
        <TextBlock Text="Projected content" Foreground="White" />
    </Grid>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection
            CenterOfRotationX="0.5"
            CenterOfRotationY="0.5"
            RotationY="-35" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>
```

The host uses a real `Viewport3D`/`Viewport2DVisual3D`, so the content remains interactive and is not a screenshot.

### 4.3 Attached-property form

```xml
<phone:ProjectionHost
    phone:ProjectionService.Projection="{StaticResource MyProjection}">
    <Grid>...</Grid>
</phone:ProjectionHost>
```

The same property works on any other element and wraps it on demand:

```xml
<Grid phone:ProjectionService.Projection="{StaticResource MyProjection}">...</Grid>
```

```csharp
myGrid.Projection = new PlaneProjection { RotationY = -35 };
myGrid.Projection = null;   // releases the on-demand wrapper
```

The on-demand wrapper requires the element to be a direct child of a `Panel`; item containers (`ListBoxItem`) and `ContentPresenter` are skipped because replacing them breaks item generation. The wrapper is released when the projection is cleared, so the visual tree ends up unchanged. Use an explicit `ProjectionHost` in templates or when the element is not a panel child.

### 4.4 Animations

Silverlight storyboards used:

```xml
Storyboard.TargetProperty="(UIElement.Projection).(PlaneProjection.RotationY)"
```

In WPF, animate the `PlaneProjection` object directly:

```csharp
DoubleAnimation animation = new DoubleAnimation
{
    From = -35,
    To = 0,
    Duration = TimeSpan.FromSeconds(0.35),
    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 }
};
projection.BeginAnimation(PlaneProjection.RotationYProperty, animation);
```

`Projection` derives from `Animatable`, so `BeginAnimation` works directly. A `Storyboard` can also target the element itself with the property path `(ProjectionService.Projection).(PlaneProjection.RotationY)`, as in Silverlight.

### 4.5 `Matrix3DProjection`

`ProjectionMatrix` is interpreted in normalized (0..1) space, matching Silverlight’s normalized projection convention, and converted to pixel space by the host. Verify any complex matrices visually.

## 5. Navigation transitions

The original page declarations work after changing the namespace:

```xml
<phone:TransitionService.NavigationInTransition>
    <phone:NavigationInTransition>
        <phone:NavigationInTransition.Backward>
            <phone:TurnstileTransition Mode="BackwardIn" />
        </phone:NavigationInTransition.Backward>
        <phone:NavigationInTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardIn" />
        </phone:NavigationInTransition.Forward>
    </phone:NavigationInTransition>
</phone:TransitionService.NavigationInTransition>

<phone:TransitionService.NavigationOutTransition>
    <phone:NavigationOutTransition>
        <phone:NavigationOutTransition.Backward>
            <phone:TurnstileTransition Mode="BackwardOut" />
        </phone:NavigationOutTransition.Backward>
        <phone:NavigationOutTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardOut" />
        </phone:NavigationOutTransition.Forward>
    </phone:NavigationOutTransition>
</phone:TransitionService.NavigationOutTransition>
```

Use `TransitionFrame` (not `PhoneApplicationFrame`) if you want these transitions. `TransitionFrame` presents pages through two `ProjectionHost` parts named `FirstContentPresenter` and `SecondContentPresenter`. Direct `Frame.Navigate(page)` calls run both entry and exit transitions.

If a transition factory throws, the exception propagates out of the navigation, exactly like the original toolkit — there is no fallback event, so validate custom `TransitionElement` implementations in your own code.

Every transition plays the original storyboard: the toolkit loads `Transitions/Storyboards/<Family><Mode>.xaml` from its own resources, so angles, key times and easing are unchanged. Two turnstile constants are the only sanctioned deviation (`CenterOfRotationX = -0.05` and a `1.2` duration scale); both are documented in `Transitions.cs` and can be reverted to the toolkit values `0` and `1`.

## 6. Control migration

### Namespace-only migration (typical)

These controls preserve their public surface; change `toolkit:` to `phone:` and the assembly in the namespace:

```xml
<phone:ToggleSwitch Header="Wi-Fi" />
<phone:Rating Width="220" Value="3" AllowHalfItemIncrement="True" />
<phone:PhoneTextBox Hint="Search" LengthIndicatorVisible="True" />
<phone:ListPicker Header="Choose" SelectedIndex="0" />
<phone:ExpanderView Header="Details">...</phone:ExpanderView>
<phone:HubTile Title="News" Message="Back" />
<phone:WrapPanel ItemWidth="80">...</phone:WrapPanel>
<phone:PerformanceProgressBar IsIndeterminate="True" />
<phone:ContextMenuService.ContextMenu>
    <phone:ContextMenu>
        <phone:MenuItem Header="Open" Click="OnOpen" />
        <phone:Separator />
    </phone:ContextMenu>
</phone:ContextMenuService.ContextMenu>
```

### Controls with host/behavior differences

| Control | Migration note |
|---|---|
| `ContextMenu` | Derives from WPF `ContextMenu`; open with right-click. `ContextMenuService.ContextMenu` is the attached property. |
| `CustomMessageBox` | Construct and call `Show()`. It is hosted in a WPF `Window`; `Dismiss()` raises the same events. |
| `DatePicker` / `TimePicker` | `Value`, `ValueString`, `ValueChanged`, `Header` are preserved. The picker UI is a WPF modal window, not the WP picker page; `PickerPageUri` is retained but not navigated. |
| `ListPicker` | Normal (expanded) mode is unchanged; full mode navigates to `ListPickerPage` through the host frame, using the pack URI in `PickerPageUri`. The page declares its Done/Cancel actions in an `ApplicationBar` with check/cancel icon buttons, exactly like the original (shown for multiple selection, hidden for single selection). |
| `ExpanderView` | Template parts are `Presenter`, `ExpanderPanel`, `ItemsCanvas`; height animation is performed in code. |
| `HubTile` | Timer-driven state cycle; `HubTileService.FreezeGroup/UnfreezeGroup` are supported. |
| `PhoneTextBox` | The inner TextBox uses a two-way text binding; hint/action/length indicator behavior is preserved. |
| `Rating` | The star items are generated in code and use `RatingItem`; template part names match the original. |
| `TiltEffect` | Apply `phone:TiltEffect.IsTiltEnabled="True"` to a container. Plain controls are temporarily wrapped in a `ProjectionHost`; list item containers are not wrapped. |
| `TurnstileFeatherEffect` | Apply `phone:TurnstileFeatherEffect.FeatheringIndex` to wrappable elements. Item containers/content presenters are skipped. |

## 7. Effects and gestures

### TiltEffect

```xml
<StackPanel phone:TiltEffect.IsTiltEnabled="True">
    <Button Content="Tilt me" />
</StackPanel>
```

A press creates a `PlaneProjection` and animates it. Mouse input is supported in addition to touch manipulation.

### TurnstileFeatherEffect

```xml
<TextBlock Text="First" phone:TurnstileFeatherEffect.FeatheringIndex="0" />
<TextBlock Text="Second" phone:TurnstileFeatherEffect.FeatheringIndex="1" />
```

Targets must be wrappable by a `ProjectionHost`. A target inside a `ListBoxItem` is skipped because replacing the item container would break item generation.

### Not migrated

- `SlideInEffect` depends on the WP `Pivot` control; not implemented.
- `GestureListener`/`GestureService` are not implemented. Use WPF mouse/touch/manipulation events or a WPF gesture library.
- Maps, background transfer, and speech are not implemented; define an application-specific service interface and keep platform-specific code out of shared UI.

## 8. System tray and application bar

`ApplicationBar` is fully ported: declare it in XAML (`<phone:PhoneApplicationPage.ApplicationBar><shell:ApplicationBar .../></phone:PhoneApplicationPage.ApplicationBar>`) or assign it in code, and the frame draws it at the bottom of the page, reserving the page space. Icon buttons need an `IconUri` (application-relative, for example `/Images/appbar.save.rest.png`) and a `Text`; a bar accepts at most 4 icon buttons and 50 menu items, and its items raise `Click`. `SystemTray.IsVisible` is an attached property and the state object is implemented, but the default frame template still does not paint the full Windows Phone status bar; host that chrome yourself if you need it.

## 9. Resource and theme migration

The original WP platform supplied `PhoneAccentBrush`, `PhoneForegroundBrush`, `PhoneBackgroundBrush`, `PhoneFontFamily*`, `PhoneFontSize*`, `PhoneText*Style`, etc. The port supplies these keys in `Themes/Generic.xaml`. If your app defined its own theme resources, merge them after the toolkit dictionary so your overrides win:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
    <ResourceDictionary Source="MyTheme.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## 10. Verification checklist

1. Project builds against `net10.0-windows` (or your chosen Windows TFM) with `UseWPF`.
2. `App.xaml` merges `Themes/Generic.xaml` if named Metro resources are used.
3. Pages derive from `PhoneApplicationPage` and are hosted in `PhoneApplicationFrame`/`TransitionFrame`.
4. Projection is assigned to an element (wrapped on demand) or to an explicit `ProjectionHost`; verify hit testing and input.
5. Navigation transitions run; watch for exceptions from invalid storyboard property values.
6. Controls render with the expected theme and template parts.
7. Unsupported modules are replaced with application-specific WPF implementations rather than assumed to work.

## 11. Known migration traps

- `xmlns:toolkit` still points at an old assembly; remove it and use `xmlns:phone`.
- Keep WPF’s `System.Windows.Controls.WrapPanel` and the port’s `Microsoft.Phone.Controls.WrapPanel` distinct; use the `phone:` prefix intentionally.
- `ContextMenu` is ambiguous if both `Microsoft.Phone.Controls` and `System.Windows.Controls` are imported; qualify the type in C#.
- `DatePicker` is ambiguous the same way; use an alias or qualify.
- Do not place interactive content inside a screenshot/VisualBrush projection; the port uses live `Viewport2DVisual3D`.
- Do not animate `Visibility` or `Margin` with string values in `DiscreteObjectKeyFrame`; use `<Visibility>` and `<syswin:Thickness>` objects (the port’s theme does this).
