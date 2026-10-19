# Community.Phone.Controls.Toolkit.WPF

A WPF compatibility port of the archived **Windows Phone Toolkit**, delivered as one NuGet package. The port preserves the original `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` namespaces and API shapes where WPF allows it, and implements real 3D projection, navigation transitions, and Metro-styled controls with WPF primitives.

[中文文档：README_zh-CN.md](README_zh-CN.md)

> **Status: preview (0.1.0-preview.1).** The implemented feature set is verified by build, automated tests, a runnable sample, and a fresh NuGet consumer test. Unsupported modules are listed explicitly in [docs/en/known-limitations.md](docs/en/known-limitations.md); they are not stubbed as if they worked.

## Features

- **Projection** — `Projection`, `PlaneProjection`, `Matrix3DProjection`, `ProjectionHost`, `ProjectionService`. Real `Viewport3D` + `Viewport2DVisual3D` rendering, hit testing, keyboard focus, and animation.
- **Navigation** — `PhoneApplicationFrame`, `PhoneApplicationPage`, `NavigationContext`, page state, orientation, back-key handling, and page lifecycle dispatch.
- **Transitions** — `TransitionFrame`, `ITransition`, `NavigationInTransition`/`NavigationOutTransition`, and Roll, Rotate, Slide, Swivel, Turnstile, and TurnstileFeather families (19 original storyboards).
- **Effects** — `TiltEffect` and `TurnstileFeatherEffect` backed by the real projection pipeline.
- **Controls** — `WrapPanel`, `ToggleSwitch`, `Rating`, `PhoneTextBox`, `ExpanderView`, `ListPicker`, `HubTile`, `ContextMenu`, `CustomMessageBox`, `DatePicker`, `TimePicker`, `RecurringDaysPicker`, `PerformanceProgressBar`, and related converters/helpers.
- **Localization** — 50 real cultures plus the neutral resource, generated from the original designer/resources.
- **Theme** — Metro palette, fonts, text styles, frame templates, and control templates in `Themes/Generic.xaml`. The theme also supplies implicit WP styles for common WPF controls (`Button`, `TextBox`, `ListBox`/`ListBoxItem`, `ComboBox`/`ComboBoxItem`, `CheckBox`, `RadioButton`, `ScrollBar`, `ContextMenu`/`MenuItem`, `ProgressBar`, and `ToolTip`).

## Requirements

- Windows
- .NET SDK 10.0 or later
- A WPF project targeting `net10.0-windows` (or a compatible Windows TFM)

## Install

```powershell
dotnet add package Community.Phone.Controls.Toolkit.WPF
```

The package ID is `Community.Phone.Controls.Toolkit.WPF`. The assembly name is the same; the root namespace is `Microsoft.Phone.Controls`.

## Quick start

Merge the toolkit theme so named Metro resources are available to your application XAML:

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary
                    Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Host pages in a `TransitionFrame` and derive pages from `PhoneApplicationPage`:

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF">
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

## Projection

WPF has no `UIElement.Projection` property. Use a `ProjectionHost`; the content stays live and interactive:

```xml
<phone:ProjectionHost Width="320" Height="200">
    <Border Background="#FF1F1F1F">
        <TextBlock Text="Hello, Metro" Foreground="White" />
    </Border>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection RotationY="-35" RotationX="8" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>
```

`PlaneProjection` is an `Animatable`; animate it directly:

```csharp
projection.BeginAnimation(
    PlaneProjection.RotationYProperty,
    new DoubleAnimation(-80, 0, TimeSpan.FromMilliseconds(350))
    {
        EasingFunction = new ExponentialEase
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 6
        }
    });
```

## Navigation parameters and state

```csharp
Frame.Navigate(
    new DetailPage(),
    new Dictionary<string, string> { { "id", "42" } });
```

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    string id = NavigationContext.QueryString["id"];
}
```

`PhoneApplicationPage.State` is retained by the frame for the page instance and survives back navigation to that page.

## Transitions

```xml
<phone:TransitionService.NavigationInTransition>
    <phone:NavigationInTransition>
        <phone:NavigationInTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardIn" />
        </phone:NavigationInTransition.Forward>
    </phone:NavigationInTransition>
</phone:TransitionService.NavigationInTransition>
```

Use `TransitionFrame`, not the plain `PhoneApplicationFrame`, for automatic entry/exit transitions. A broken custom transition is reported through `TransitionFrame.TransitionFailed` and falls back to an untransitioned, interactive navigation.

## Build, test, pack

```powershell
dotnet restore
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet test Community.Phone.Controls.Toolkit.slnx -c Release
dotnet pack src\Community.Phone.Controls.Toolkit.WPF\Community.Phone.Controls.Toolkit.WPF.csproj -c Release
```

The package is written to `artifacts/packages`.

### Package consumption test

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
```

This packs the library, restores a fresh WPF consumer from an isolated local feed and cache, builds it with zero warnings, runs it, and verifies that the package theme resources and controls load.

## Verification status

- Release build: 0 warnings, 0 errors (re-verified 2026-10-19).
- Automated tests: 72 passed, 0 failed (projection, controls, converters, effects, navigation/transitions, and shell APIs). Interaction tests cover the 2D→3D projection switch during animation, XAML-faithful perspective geometry (999 DIP depth, 1:1 identity mapping, `RotationY` direction), entry/exit Turnstile storyboards, feathered transitions, TiltEffect staying on its own control, ToggleSwitch thumb animation, Rating mouse input, and PhoneTextBox focus/editing.
- Package consumer: fresh project restored from an isolated local feed/cache, built with 0 warnings, and ran successfully with package controls and theme resources.
- Sample: launched successfully; UI automation opened every sample (Controls, Projection, Transitions → target page → back, Feathering, Menus) and returned to the sample index each time, with no startup error.
- Per-module status and evidence: [docs/porting-status.md](docs/porting-status.md). Latest handoff: [docs/handoff.md](docs/handoff.md).

## Run the sample

```powershell
dotnet run --project src\Community.Phone.Controls.Toolkit.WPF.Sample
```

The sample mirrors the original `PhoneToolkitSample8` structure: a `MainPage` lists the samples and each one is its own page — `ControlsPage` (WPF base controls with WP styles plus all implemented toolkit controls), `ProjectionPage`, `TransitionsPage` (pick any transition family, including feathering, and navigate with it), `FeatherPage`, and `MenusPage` (ContextMenu, CustomMessageBox, ApplicationBar/SystemTray state). Startup failures are written to `%TEMP%\community-phone-toolkit-sample-error.txt`.

## Documentation

English documentation lives under `docs/en/`; Chinese documentation lives under `docs/zh-CN/`.

- [Handoff — start here in a new session](docs/handoff.md) (latest verified state, uncommitted work, next steps)
- [Porting status](docs/porting-status.md) (verified / implemented-unverified / not implemented, per module)
- [Original feature audit](docs/en/porting-inventory.md)
- [Architecture](docs/en/architecture.md)
- [API compatibility matrix](docs/en/api-compatibility.md)
- [Migration guide](docs/en/migration-guide.md)
- [Known limitations](docs/en/known-limitations.md)
- [Third-party notices](THIRD-PARTY-NOTICES.md)
- [中文文档索引](docs/zh-CN/README.md) / [中文 README](README_zh-CN.md)

## Unsupported modules

The preview does **not** implement `AutoCompleteBox`, `LongListSelector`, `LongListMultiSelector`, `MultiselectList`, map extensions, `TransferControl`, `SpeechTextBox`, `LockablePivot`, `SlideInEffect`, `GestureListener`/`GestureService`, or looping-selector/picker pages. See [known limitations](docs/en/known-limitations.md) for rationale and alternatives. These are not claimed as compatible.

## License and attribution

The port is distributed under the **Microsoft Public License (MS-PL)**. See [LICENSE](LICENSE). Original toolkit source copyright Microsoft Corporation; port changes copyright their contributors. The package has no Windows Phone SDK, Silverlight, `Microsoft.Phone`, or `System.Windows.Interactivity` dependency.
