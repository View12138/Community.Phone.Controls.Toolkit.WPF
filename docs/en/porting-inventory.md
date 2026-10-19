# Porting inventory
This document is the Phase 0 audit of the archived Windows Phone Toolkit at `D:\repos\repo-other\WindowsPhoneToolkit`. It was produced by reading the solution, project files, shared source, WP7/WP8 specific source, XAML themes, localization resources, and samples. It records the actual public types found in source; it is not based on the README alone.
## Repository audit summary
| Item | Result |
|---|---|
| Original branch / commit | `master`, `4e4fd5d` (Adding README) |
| License | Microsoft Public License (MS-PL) |
| Solutions | `PhoneToolkit.sln`, `Microsoft.Phone.Controls.Toolkit.WP7.sln` |
| Toolkit projects | `Microsoft.Phone.Controls.Toolkit` (shared source), `Microsoft.Phone.Controls.Toolkit.WP7`, `Microsoft.Phone.Controls.Toolkit.WP8`, `Microsoft.Phone.Controls.Toolkit.Localization` |
| Sample projects | `PhoneToolkitSample` (WP7), `PhoneToolkitSample8` (WP8) |
| Source files read | 245 `.cs`, 82 `.xaml`, 56 `.resx`, 51 `.xlf`, 21 `.png`, 16 `.jpg` |
| Original target frameworks | WP7.1 (Silverlight 4 profile), WP8 (Silverlight 8) |
| Original platform dependencies | `Microsoft.Phone`, `Microsoft.Phone.Controls`, `Microsoft.Phone.Interop`, `Microsoft.Xna.Framework`, `System.Device`, `Windows.Phone.Speech`, `Microsoft.Phone.Maps` |
| Original theme | `Microsoft.Phone.Controls.Toolkit.WP8\Themes\Generic.xaml` (150 KB), `WP7` variant (152 KB) |
| Original localized resources | `Microsoft.Phone.Controls.Toolkit.Localization\LocalizedResources\ControlResources.*` for 51 cultures |
| WP7/WP8 differences | WP7 has its own `GestureListener`/`ListPickerPage`; WP8 adds `LongListMultiSelector`, maps, speech, and updated picker pages. Shared files are linked into both projects. |

### Build constraints confirmed from project files
- The original projects are non-SDK-style MSBuild projects targeting Windows Phone and Silverlight; they cannot be built with the modern .NET SDK as-is.
- They require `Microsoft.Phone.dll`, `Microsoft.Phone.Controls.dll`, WP SDK tooling, and XNA assemblies. None of these are referenced by this WPF port.
- `Generic.xaml` references WP platform resource keys (`PhoneAccentBrush`, `PhoneFontFamily*`, `PhoneFontSize*`, `PhoneBorderThickness`, etc.) and WP platform types; those resources are re-created in this port.
- The original sample is a WP Silverlight application, not a WPF application; a new WPF sample was created for runtime evidence.

## Public type inventory
Status legend: **Implemented** = source port exists in this repository; **Partial** = implemented with documented differences; **WPF native** = use the WPF framework type; **Not implemented** = no compatible implementation yet.
| Namespace | Kind | Type | Original source | Status | Notes |
|---|---|---|---|---|---|
| Microsoft.Phone.Controls | class | AutoCompleteBox | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/AutoCompleteBox.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | enum | AutoCompleteFilterMode | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/AutoCompleteFilterMode.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | ContextMenu | Microsoft.Phone.Controls.Toolkit/ContextMenu/ContextMenu.cs | Partial | Derives from WPF ContextMenu/MenuItem separator types; opening is triggered by right-click. |
| Microsoft.Phone.Controls | class | ContextMenuService | Microsoft.Phone.Controls.Toolkit/ContextMenu/ContextMenuService.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | CultureInfoExtensions | Microsoft.Phone.Controls.Toolkit/Common/CultureInfoExtensions.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | CustomMessageBox | Microsoft.Phone.Controls.Toolkit/CustomMessageBox/CustomMessageBox.cs | Partial | Hosted in a WPF Window instead of a platform Popup; visual layout is rebuilt in code. |
| Microsoft.Phone.Controls | enum | CustomMessageBoxResult | Microsoft.Phone.Controls.Toolkit/CustomMessageBox/CustomMessageBoxResult.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DailyDateTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/DailyDateTimeConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DatePicker | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DatePicker.cs | Partial | Uses a WPF DatePicker in a modal Window instead of the WP picker page. |
| Microsoft.Phone.Controls | class | DatePickerPage | Microsoft.Phone.Controls.Toolkit.WP8/DateTimePickers/DatePickerPage.xaml.cs<br>Microsoft.Phone.Controls.Toolkit/DateTimePickers/DatePickerPage.xaml.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | DateTimePickerBase | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerBase.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DateTimePickerResources | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerResources.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DateTimeValueChangedEventArgs | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimeValueChangedEventArgs.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DismissedEventArgs | Microsoft.Phone.Controls.Toolkit/CustomMessageBox/DismissedEventArgs.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DismissingEventArgs | Microsoft.Phone.Controls.Toolkit/CustomMessageBox/DismissingEventArgs.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | DragCompletedGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | DragDeltaGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | DragStartedGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | ExpanderView | Microsoft.Phone.Controls.Toolkit/ExpanderView/ExpanderView.cs | Partial | Uses WPF HeaderedItemsControl; height animation and visual states are simplified. |
| Microsoft.Phone.Controls | enum | ExpansionMode | Microsoft.Phone.Controls.Toolkit/ListPicker/ExpansionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | FeatheredTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/FeatheredTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | FlickGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | FullViewDateTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/FullViewDateTimeConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | GestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | GestureListener | Microsoft.Phone.Controls.Toolkit.WP7/Input/GestureListenerStatic.cs<br>Microsoft.Phone.Controls.Toolkit.WP8/Input/GestureListenerStatic.cs<br>Microsoft.Phone.Controls.Toolkit/Input/GestureListener.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | GestureService | Microsoft.Phone.Controls.Toolkit.WP7/Input/GestureService.cs<br>Microsoft.Phone.Controls.Toolkit.WP8/Input/GestureService.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | GroupViewClosingEventArgs | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | GroupViewOpenedEventArgs | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | HourlyDateTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/HourlyDateTimeConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | HubTile | Microsoft.Phone.Controls.Toolkit/HubTile/HubTile.cs | Partial | Timer-driven state cycle; animation timing is approximate. |
| Microsoft.Phone.Controls | class | HubTileService | Microsoft.Phone.Controls.Toolkit/HubTile/HubTileService.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | interface | ISelectionAdapter | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/ISelectionAdapter.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | interface | ITransition | Microsoft.Phone.Controls.Toolkit/Transitions/ITransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | LengthConverter | Microsoft.Phone.Controls.Toolkit/Common/LengthConverter.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | LinkUnlinkEventArgs | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | ListPicker | Microsoft.Phone.Controls.Toolkit/ListPicker/ListPicker.cs | Partial | Popup or modal Window instead of ListPickerPage; PickerPageUri is retained but not navigated. |
| Microsoft.Phone.Controls | class | ListPickerItem | Microsoft.Phone.Controls.Toolkit/ListPicker/ListPickerItem.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | ListPickerMode | Microsoft.Phone.Controls.Toolkit/ListPicker/ListPickerMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | ListPickerPage | Microsoft.Phone.Controls.Toolkit.WP7/ListPicker/ListPickerPage.xaml.cs<br>Microsoft.Phone.Controls.Toolkit.WP8/ListPicker/ListPickerPage.xaml.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | ListViewDateTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/ListViewDateTimeConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | LockablePivot | Microsoft.Phone.Controls.Toolkit/Pivot/LockablePivot.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | LongListMultiSelector | Microsoft.Phone.Controls.Toolkit.WP8/LongListMultiSelector/LongListMultiSelector.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | LongListMultiSelectorItem | Microsoft.Phone.Controls.Toolkit.WP8/LongListMultiSelector/LongListMultiSelectorItem.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | LongListSelector | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelector.cs<br>Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorGroup.cs<br>Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItemsControl.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | LongListSelectorItem | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItem.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | enum | LongListSelectorItemType | Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItemType.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | MenuItem | Microsoft.Phone.Controls.Toolkit/ContextMenu/MenuItem.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | MultiTouchGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | MultiselectItem | Microsoft.Phone.Controls.Toolkit/MultiselectList/MultiselectItem.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | MultiselectList | Microsoft.Phone.Controls.Toolkit/MultiselectList/MultiselectList.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | NavigationInTransition | Microsoft.Phone.Controls.Toolkit/Transitions/NavigationInTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | NavigationOutTransition | Microsoft.Phone.Controls.Toolkit/Transitions/NavigationOutTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | NavigationTransition | Microsoft.Phone.Controls.Toolkit/Transitions/NavigationTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | NullToVisibilityConverter | Microsoft.Phone.Controls.Toolkit/TransferControl/NullToVisibilityConverter.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | OffOnConverter | Microsoft.Phone.Controls.Toolkit/ToggleSwitch/OffOnConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | PerformanceProgressBar | Microsoft.Phone.Controls.Toolkit/ProgressBar/PerformanceProgressBar.cs | Partial | Indeterminate template only; fade-out is a simple opacity animation. |
| Microsoft.Phone.Controls | class | PhoneTextBox | Microsoft.Phone.Controls.Toolkit/PhoneTextBox/PhoneTextBox.cs | Implemented | Hint, action icon, length indicator, DP surface preserved. |
| Microsoft.Phone.Controls | class | PinchGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | PinchStartedGestureEventArgs | Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | PopulatedEventArgs | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/PopulatedEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | PopulatingEventArgs | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/PopulatingEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | Rating | Microsoft.Phone.Controls.Toolkit/RatingControl/Rating.cs | Implemented | Original DP surface and template parts; star items generated in code. |
| Microsoft.Phone.Controls | class | RatingItem | Microsoft.Phone.Controls.Toolkit/RatingControl/RatingItem.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | RecurringDaysPicker | Microsoft.Phone.Controls.Toolkit/DateTimePickers/RecurringDaysPicker.cs | Partial | Built on the WPF ListPicker; summarization matches the original rules. |
| Microsoft.Phone.Controls | class | RelativeTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/RelativeTimeConverter.cs | Implemented | Localized elapsed-time strings from ControlResources. |
| Microsoft.Phone.Controls | class | RollTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RollTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | RotateTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RotateTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | RotateTransitionMode | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RotateTransitionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | RoutedPropertyChangingEventArgs | Microsoft.Phone.Controls.Toolkit/Common/RoutedPropertyChangingEventArgs.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | SelectorSelectionAdapter | Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/SelectorSelectionAdapter.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | Separator | Microsoft.Phone.Controls.Toolkit/ContextMenu/Separator.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | SlideInEffect | Microsoft.Phone.Controls.Toolkit/Effects/SlideInEffect.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | SlideTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SlideTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | SlideTransitionMode | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SlideTransitionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | SpeechRecognizedEventArgs | Microsoft.Phone.Controls.Toolkit.WP8/SpeechTextBox/SpeechRecognizedEventArgs.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | SpeechTextBox | Microsoft.Phone.Controls.Toolkit.WP8/SpeechTextBox/SpeechTextBox.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | SwivelTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SwivelTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | SwivelTransitionMode | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SwivelTransitionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | ThreadDateTimeConverter | Microsoft.Phone.Controls.Toolkit/DateTimeConverters/ThreadDateTimeConverter.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | TileSize | Microsoft.Phone.Controls.Toolkit/HubTile/HubTile.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TileSizeToHeightConverter | Microsoft.Phone.Controls.Toolkit/HubTile/HubTileConverters.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TileSizeToWidthConverter | Microsoft.Phone.Controls.Toolkit/HubTile/HubTileConverters.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TiltEffect | Microsoft.Phone.Controls.Toolkit/Effects/TiltEffect.cs | Partial | Wraps plain controls in ProjectionHost and supports mouse + touch; list-item containers are not wrapped. |
| Microsoft.Phone.Controls | class | TimePicker | Microsoft.Phone.Controls.Toolkit/DateTimePickers/TimePicker.cs | Partial | Uses hour/minute ComboBoxes in a modal Window instead of the WP picker page. |
| Microsoft.Phone.Controls | class | TimePickerPage | Microsoft.Phone.Controls.Toolkit.WP8/DateTimePickers/TimePickerPage.xaml.cs<br>Microsoft.Phone.Controls.Toolkit/DateTimePickers/TimePickerPage.xaml.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | ToggleSwitch | Microsoft.Phone.Controls.Toolkit/ToggleSwitch/ToggleSwitch.cs | Implemented | Original DP/event surface; WPF template uses ToggleSwitchButton. |
| Microsoft.Phone.Controls | class | TransferControl | Microsoft.Phone.Controls.Toolkit/TransferControl/TransferControl.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | TransferMonitor | Microsoft.Phone.Controls.Toolkit/TransferControl/TransferMonitor.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | enum | TransferRequestState | Microsoft.Phone.Controls.Toolkit/TransferControl/TransferRequestState.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | enum | TransferType | Microsoft.Phone.Controls.Toolkit/TransferControl/TransferType.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls | class | Transition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/Transition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TransitionElement | Microsoft.Phone.Controls.Toolkit/Transitions/TransitionElement.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TransitionFrame | Microsoft.Phone.Controls.Toolkit/Transitions/TransitionFrame.cs | Partial | Uses ProjectionHost template parts instead of ContentPresenter; transitions are programmatic. |
| Microsoft.Phone.Controls | class | TransitionService | Microsoft.Phone.Controls.Toolkit/Transitions/TransitionService.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TurnstileFeatherEffect | Microsoft.Phone.Controls.Toolkit/Effects/TurnstileFeatherEffect.cs | Partial | Requires each target to be wrappable by ProjectionHost; ListBoxItem/ContentPresenter targets are skipped. |
| Microsoft.Phone.Controls | class | TurnstileFeatherTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileFeatherTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | TurnstileFeatherTransitionMode | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileFeatherTransitionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | TurnstileTransition | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileTransition.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | enum | TurnstileTransitionMode | Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileTransitionMode.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | WeakReferenceHelper | Microsoft.Phone.Controls.Toolkit/Common/WeakReferenceHelper.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls | class | WrapPanel | Microsoft.Phone.Controls.Toolkit/WrapPanel/WrapPanel.cs | Implemented | ItemHeight/ItemWidth/Orientation and original layout algorithm. |
| Microsoft.Phone.Controls.LocalizedResources | class | ControlResources | Microsoft.Phone.Controls.Toolkit.Localization/LocalizedResources/ControlResources.Designer.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls.Primitives | class | ClipToBounds | Microsoft.Phone.Controls.Toolkit/Primitives/ClipToBounds.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls.Primitives | class | DateTimePickerPageBase | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerPageBase.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls.Primitives | class | DateTimeWrapper | Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimeWrapper.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls.Primitives | interface | IDateTimePickerPage | Microsoft.Phone.Controls.Toolkit/DateTimePickers/IDateTimePickerPage.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls.Primitives | interface | ILoopingSelectorDataSource | Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelectorDataSource.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls.Primitives | class | LoopingSelector | Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelector.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls.Primitives | class | LoopingSelectorItem | Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelectorItem.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls.Primitives | class | MenuBase | Microsoft.Phone.Controls.Toolkit/ContextMenu/MenuBase.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Controls.Primitives | class | RelativeAnimatingContentControl | Microsoft.Phone.Controls.Toolkit/ProgressBar/RelativeAnimatingContentControl.cs | Partial | Content host only; magic-number relative animation rewriting is not ported. |
| Microsoft.Phone.Controls.Primitives | class | TemplatedListBox | Microsoft.Phone.Controls.Toolkit/LongListSelector/TemplatedListBox.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Controls.Primitives | class | ToggleSwitchButton | Microsoft.Phone.Controls.Toolkit/Primitives/ToggleSwitchButton.cs | Implemented | Implemented and covered by build/tests. |
| Microsoft.Phone.Maps.Services | class | QueryExtensions | Microsoft.Phone.Controls.Toolkit.WP8/Maps/QueryExtensions.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | MapChild | Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChild.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | MapChildCollection | Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChildCollection.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | MapChildControl | Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChildControl.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | MapExtensions | Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapExtensions.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | MapItemsControl | Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapItemsControl.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | Pushpin | Microsoft.Phone.Controls.Toolkit.WP8/Maps/PushPin.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Microsoft.Phone.Maps.Toolkit | class | UserLocationMarker | Microsoft.Phone.Controls.Toolkit.WP8/Maps/UserLocationMarker.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| System.Windows | class | HierarchicalDataTemplate | Microsoft.Phone.Controls.Toolkit/Data/HierarchicalDataTemplate.cs | WPF native | Use the WPF System.Windows implementation; the original type is not redefined. |
| System.Windows.Controls | class | HeaderedItemsControl | Microsoft.Phone.Controls.Toolkit/HeaderedItemsControl/HeaderedItemsControl.cs | WPF native | Use the WPF System.Windows implementation; the original type is not redefined. |
| System.Windows.Controls | class | TemplatedVisualTreeExtensions | Microsoft.Phone.Controls.Toolkit/Common/TemplatedVisualTreeExtensions.cs | WPF native | Use the WPF System.Windows implementation; the original type is not redefined. |
| System.Windows.Controls | class | TimeTypeConverter | Microsoft.Phone.Controls.Toolkit/Common/TimeTypeConverter.cs | Not implemented | No compatible implementation in this preview; see known limitations. |
| Windows.Devices.Geolocation | class | GeocoordinateExtensions | Microsoft.Phone.Controls.Toolkit.WP8/Maps/GeocoordinateExtensions.cs | Not implemented | No compatible implementation in this preview; see known limitations. |

## Port-only compatibility types

These types do not exist with the same namespace in the original toolkit; they are WPF compatibility surfaces for platform concepts.

| Port type | Purpose | Compatibility notes |
|---|---|---|
| `Microsoft.Phone.Controls.NavigationContext` | Supplies `Uri`, `QueryString`, and `ExtraData` to pages | Original platform type is `System.Windows.Navigation.NavigationContext`; WPF has no equivalent. |
| `Microsoft.Phone.Controls.ProjectionHost` | Applies a projection to arbitrary WPF content | Replaces the missing `UIElement.Projection` property. |
| `Microsoft.Phone.Controls.ProjectionService` | Attached projection and on-demand host wrapping | New WPF adapter; namespaced with the port. |
| `Microsoft.Phone.Controls.TransitionFrame.TransitionFailed` / `TransitionFailedEventArgs` | Reports a transition factory/animation failure and fallback | New diagnostic surface; not present in the original. |
| `PhoneApplicationFrame.Navigate(PhoneApplicationPage, IDictionary<string,string>)` | Passes page parameters without a URI | New WPF-friendly overload; original used platform navigation URIs. |

## Module coverage
| Module | Source files | Public surface | Port status |
|---|---|---|---|
| Projection / 3D | No original toolkit type; platform `UIElement.Projection`/`PlaneProjection` used by effects and transitions | Projection, PlaneProjection, Matrix3DProjection, ProjectionHost, ProjectionService | Implemented (real Viewport2DVisual3D rendering; tests) |
| Navigation / page model | WP platform `PhoneApplicationFrame`, `PhoneApplicationPage`, `PhoneApplicationService`, `SystemTray` usage | PhoneApplicationFrame, PhoneApplicationPage, NavigationContext, lifecycle events, state, orientation, app service, system tray state | Implemented subset (WPF Frame/Page; params/state/lifecycle tested; system chrome and idle policy differ) |
| Navigation transitions | `Transitions\*` (19 XAML storyboards, 36 C# files) | ITransition, Transition, TransitionElement, NavigationTransition, TransitionFrame, Slide/Rotate/Swivel/Turnstile/Roll/Feather | Implemented (programmatic storyboards; automatic entry/exit; real projection animation; failure fallback; tests) |
| Effects | `Effects\*` | TiltEffect, SlideInEffect, TurnstileFeatherEffect | TiltEffect + TurnstileFeatherEffect implemented with real projection and automated tests; SlideInEffect not implemented (requires Pivot) |
| AutoCompleteBox | `AutoCompleteBox\*` (11 files, ~174 KB) | AutoCompleteBox, filters, selection adapters, popup helper | Not implemented |
| ContextMenu | `ContextMenu\*` (5 files, ~74 KB) | ContextMenu, ContextMenuService, MenuBase, MenuItem, Separator | Partial (WPF ContextMenu-derived) |
| CustomMessageBox | `CustomMessageBox\*` (4 files) | CustomMessageBox, result, Dismissing/Dismissed | Implemented (WPF Window host) |
| DateTimeConverters | `DateTimeConverters\*` (7 files) | 6 converters + DateTimeFormatHelper | Implemented |
| DateTimePickers | `DateTimePickers\*` (12 files) | DatePicker, TimePicker, DateTimePickerBase, RecurringDaysPicker, pages, LoopingSelector dependencies | Partial (WPF DatePicker/TimePicker windows; picker pages and LoopingSelector not ported) |
| ExpanderView | `ExpanderView\*` | ExpanderView | Implemented (WPF HeaderedItemsControl; simplified height animation) |
| HubTile | `HubTile\*` (3 files) | HubTile, HubTileService, TileSize, converters | Implemented (timer-driven, approximate timing) |
| ListPicker | `ListPicker\*` (4 files) | ListPicker, ListPickerItem, modes, ExpansionMode | Partial (popup/modal Window; PickerPageUri retained) |
| LongListSelector (WP7) | `LongListSelector\*` (8 files, ~89 KB) | LongListSelector, item types, group, templated list box | Not implemented |
| LongListMultiSelector (WP8) | `LongListMultiSelector\*` (2 files, ~57 KB) | LongListMultiSelector, item | Not implemented |
| Maps (WP8) | `Maps\*` (14 files, ~76 KB) | MapChild, MapExtensions, MapItemsControl, Pushpin, UserLocationMarker, query helpers | Not implemented (requires a map provider) |
| MultiselectList | `MultiselectList\*` (2 files) | MultiselectList, MultiselectItem | Not implemented |
| PhoneTextBox | `PhoneTextBox\*` | PhoneTextBox | Implemented |
| Pivot helper | `Pivot\LockablePivot.cs` | LockablePivot | Not implemented |
| Primitives | `Primitives\*` (5 files) | ClipToBounds, LoopingSelector, LoopingSelectorItem, ToggleSwitchButton | ClipToBounds + ToggleSwitchButton implemented; LoopingSelector not implemented |
| ProgressBar | `ProgressBar\*` (2 files) | PerformanceProgressBar, RelativeAnimatingContentControl | Partial (indeterminate template; relative animation rewriting not ported) |
| Rating | `RatingControl\*` (2 files) | Rating, RatingItem | Implemented |
| ToggleSwitch | `ToggleSwitch\*` (2 files) | ToggleSwitch, OffOnConverter | Implemented |
| TransferControl | `TransferControl\*` (5 files) | TransferControl, TransferMonitor, states/types, converter | Not implemented (requires WP background transfer service) |
| WrapPanel | `WrapPanel\*` (2 files) | WrapPanel | Implemented |
| SpeechTextBox (WP8) | `SpeechTextBox\*` (2 files) | SpeechTextBox, SpeechRecognizedEventArgs | Not implemented (requires Windows Phone speech APIs) |
| Gesture input | `Input\*` (shared + WP7/WP8 variants) | GestureListener, GestureService, gesture event args | Not implemented (WPF manipulation/touch events differ) |
| Localization | `Microsoft.Phone.Controls.Toolkit.Localization` | ControlResources and 51 culture resources | Implemented (resources copied and embedded) |
| Common helpers | `Common\*` (25 files) | CultureInfoExtensions, LengthConverter, RoutedPropertyChangingEventArgs, TimeTypeConverter, WeakReferenceHelper, extensions, visual state helpers | Partially ported (CultureInfoExtensions, RoutedPropertyChangingEventArgs, WeakReferenceHelper); other helpers are internal or not ported |

## Dependency graph (original)
```text
PhoneToolkit.sln
├─ Microsoft.Phone.Controls.Toolkit.WP8.csproj (assembly Microsoft.Phone.Controls.Toolkit)
│  ├─ links all shared Microsoft.Phone.Controls.Toolkit\**\*.cs
│  ├─ links Microsoft.Phone.Controls.Toolkit.Localization\LocalizedResources\ControlResources.Designer.cs
│  ├─ WP8-only: LongListMultiSelector, Maps, SpeechTextBox, GestureService, picker pages
│  └─ references Microsoft.Phone, Microsoft.Phone.Controls (platform)
├─ Microsoft.Phone.Controls.Toolkit.WP7.csproj
│  ├─ links shared source + WP7-only GestureListener/GestureService/ListPickerPage
│  └─ references Microsoft.Phone, Microsoft.Phone.Controls, Microsoft.Xna.Framework
├─ Microsoft.Phone.Controls.Toolkit.Localization.csproj (resx/xlf -> satellite resources)
├─ PhoneToolkitSample.WP7.csproj -> Toolkit.WP7
└─ PhoneToolkitSample.WP8.csproj -> Toolkit.WP8
```
Internal dependency clusters: `Transitions` depends on `Effects` (feathering), `Primitives` and `Common`; `DateTimePickers` depends on `LoopingSelector`, `ListPicker` and localization; `ListPicker` depends on `ContextMenu`, `Transitions`, `Primitives` and localization; `TiltEffect` depends on `Primitives`/`LongListSelector` (WP8); `Maps` depends on `Microsoft.Phone.Maps.Controls` and `System.Device.Location`; `TransferControl` depends on `Microsoft.Phone.BackgroundTransfer`.

## WPF platform infrastructure that had to be designed
- `UIElement.Projection` and `PlaneProjection` do not exist in WPF. The port supplies `Microsoft.Phone.Controls.Projection`, `PlaneProjection`, `Matrix3DProjection`, `ProjectionService`, and `ProjectionHost`. WPF XAML cannot add a property to `System.Windows.Controls.Grid`; wrappers are required for in-place projection.
- WP `PhoneApplicationFrame`, `PhoneApplicationPage`, `PhoneApplicationService`, `SystemTray`, and `ApplicationBar` are platform types, not toolkit types. The port supplies compatible WPF types in `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`.
- WP `Manipulation*`/`Gesture*` APIs and `Application.Current.RootVisual` differ from WPF. Gesture-based components are not ported.
- WP `BitmapCache`, `VisualStateManager`, and Storyboards exist in WPF but their property paths differ; transition storyboards were converted to code so they can target the compatible `PlaneProjection` directly.

## Original theme and resources
- The WP8 `Generic.xaml` defines implicit styles for 25 control types and uses 40+ WP platform resource keys. It also defines storyboards and templates for `Rating`, `ListPicker`, `DatePicker`, `TimePicker`, `AutoCompleteBox`, `ContextMenu`, `MenuItem`, `Separator`, `ToggleSwitch`, `PhoneTextBox`, `ExpanderView`, `HubTile`, `CustomMessageBox`, `TransitionFrame`, `LongListMultiSelector`, `Pushpin`, and `UserLocationMarker`.
- The port re-creates the keys used by the implemented controls in `src/Community.Phone.Controls.Toolkit.WPF/Themes/Generic.xaml` (Metro palette, fonts, sizes, text styles, button style, frame templates, and per-control templates).
- The 51 `.xlf` files are localization exchange files; 51 `.resx` files were copied into the WPF project. The English base plus satellite resources are embedded in the single NuGet package. The `qps-ploc` pseudo-locale is omitted because NuGet normalizes its culture casing and warns on the package.

## Highest-priority compatibility risks found in audit
1. Projection cannot be injected into an arbitrary WPF `UIElement`; a `ProjectionHost` wrapper or an effects-specific wrapper is required. This affects TiltEffect, TurnstileFeatherEffect, and user XAML migration.
2. `PhoneApplicationFrame`/`TransitionFrame` cannot be a drop-in WPF `Frame` without changing navigation content presentation; the port uses custom templates and page lifecycle raises from the frame.
3. WP-specific services (maps, background transfer, speech, Live Tiles, system tray chrome) have no WPF equivalent. They are either replaced with interfaces or documented as unsupported.
4. LongListSelector/LongListMultiSelector/AutoCompleteBox are large, virtualized controls tied to Silverlight item container lifecycles. They are the largest remaining work items.

## XAML usage observed in the original samples
| Element / attached property | Sample occurrences | Notes |
|---|---|---|
| `toolkit:TurnstileTransition` | 112 | Dominant navigation transition family in both samples. |
| `toolkit:NavigationOutTransition` | 90 | Used as the page NavigationOutTransition root. |
| `toolkit:NavigationInTransition` | 90 | Used as the page NavigationInTransition root. |
| `toolkit:TransitionService` | 60 | Attached transition declarations. |
| `phone:PhoneApplicationPage` | 55 | Page base type; project-specific namespace mapping. |
| `toolkit:HubTile` | 19 | Animated tiles on the hub sample. |
| `toolkit:MenuItem` | 14 | Context menu items. |
| `toolkit:ListPicker` | 14 | Transition/mode pickers and general item picker samples. |
| `toolkit:LongListSelector` | 8 | WP7 list control; not ported. |
| `toolkit:TurnstileFeatherTransition` | 8 | Feathered page transitions. |
| `toolkit:PhoneTextBox` | 6 | Text input samples. |
| `toolkit:LongListMultiSelector` | 6 | WP8 multi-select list; not ported. |
| `toolkit:MultiselectList` | 6 | WP7 multi-select list; not ported. |
| `toolkit:ToggleSwitch` | 5 | Toggle samples. |
| `toolkit:ExpanderView` | 5 | Expander sample. |
| `toolkit:ContextMenu/ContextMenuService` | 8 | Context menu samples. |
| `toolkit:AutoCompleteBox` | 3 | Auto-complete sample; not ported. |
| `toolkit:WrapPanel` | 2 | Layout sample. |
| `toolkit:Rating` | 1 | Rating sample. |
| `toolkit:DatePicker/TimePicker` | 2 | Date/time picker samples. |
| `toolkit:PerformanceProgressBar` | 1 | Indeterminate progress sample. |
| `toolkit:GestureService/GestureListener` | 1 | Gesture sample; not ported. |
| `toolkit:TiltEffect.IsTiltEnabled` | 19 | Applied to page or list containers. |
| `toolkit:TurnstileFeatherEffect.FeatheringIndex` | 9 | Per-element staggering index. |
| `toolkit:SlideInEffect.LineIndex` | 3 | Pivot-based effect; not ported. |
| `toolkit:TiltEffect.SuppressTilt` | 1 | Explicit suppression on a control. |
| `maptk:Pushpin/MapItemsControl/MapExtensions/UserLocationMarker` | 5 | Map sample; not ported. |
| `maps:Map` | 1 | Requires Microsoft.Phone.Maps; not ported. |
| `shell:ApplicationBar/ApplicationBarIconButton/ApplicationBarMenuItem` | 20 | Application bar sample usage; API model implemented. |

## Appendix: public members observed in source
The following lists are derived from the actual source files. They are intentionally verbose and include duplicated WP7/WP8 variants. Members may use Silverlight-only base types; the compatibility matrix records the final WPF decision for each implemented type.

### Microsoft.Phone.Controls.AutoCompleteBox (class)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/AutoCompleteBox.cs`
- `public int MinimumPrefixLength public int MinimumPrefixLength`
- `public static readonly DependencyProperty MinimumPrefixLengthProperty = DependencyProperty.Register( "MinimumPrefixLength", typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(1, OnMinimumPrefixLengthPropertyChanged));`
- `public int MinimumPrefixLength #region public int MinimumPopulateDelay public int MinimumPopulateDelay`
- `public static readonly DependencyProperty MinimumPopulateDelayProperty = DependencyProperty.Register( "MinimumPopulateDelay", typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(OnMinimumPopulateDelayPropertyChanged));`
- `public int MinimumPopulateDelay #region public bool IsTextCompletionEnabled public bool IsTextCompletionEnabled`
- `public static readonly DependencyProperty IsTextCompletionEnabledProperty = DependencyProperty.Register( "IsTextCompletionEnabled", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, null));`
- `public bool IsTextCompletionEnabled #region public DataTemplate ItemTemplate public DataTemplate ItemTemplate`
- `public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register( "ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));`
- `public DataTemplate ItemTemplate #region public Style ItemContainerStyle public Style ItemContainerStyle`
- `public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register( ElementItemContainerStyle, typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null, null));`
- `public Style ItemContainerStyle #region public Style TextBoxStyle public Style TextBoxStyle`
- `public static readonly DependencyProperty TextBoxStyleProperty = DependencyProperty.Register( ElementTextBoxStyle, typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null));`
- `public Style TextBoxStyle #region public double MaxDropDownHeight public double MaxDropDownHeight`
- `public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register( "MaxDropDownHeight", typeof(double), typeof(AutoCompleteBox), new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));`
- `public double MaxDropDownHeight #region public bool IsDropDownOpen public bool IsDropDownOpen`
- `public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register( "IsDropDownOpen", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, OnIsDropDownOpenPropertyChanged));`
- `public bool IsDropDownOpen #region public IEnumerable ItemsSource public IEnumerable ItemsSource`
- `public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof(IEnumerable), typeof(AutoCompleteBox), new PropertyMetadata(OnItemsSourcePropertyChanged));`
- `public IEnumerable ItemsSource #region public object SelectedItem public object SelectedItem`
- `public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( "SelectedItem", typeof(object), typeof(AutoCompleteBox), new PropertyMetadata(OnSelectedItemPropertyChanged));`
- `public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent( "SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteBox));`
- `public object SelectedItem #region public string Text public string Text`
- `public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, OnTextPropertyChanged));`
- `public string Text #region public string SearchText public string SearchText`
- `public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register( "SearchText", typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, OnSearchTextPropertyChanged));`
- `public string SearchText #region public AutoCompleteFilterMode FilterMode public AutoCompleteFilterMode FilterMode`
- `public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register( "FilterMode", typeof(AutoCompleteFilterMode), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteFilterMode.StartsWith, OnFilterModePropertyChanged));`
- `public AutoCompleteFilterMode FilterMode #region public AutoCompleteFilterPredicate ItemFilter public AutoCompleteFilterPredicate<object> ItemFilter`
- `public static readonly DependencyProperty ItemFilterProperty = DependencyProperty.Register( "ItemFilter", typeof(AutoCompleteFilterPredicate<object>), typeof(AutoCompleteBox), new PropertyMetadata(OnItemFilterPropertyChanged));`
- `public AutoCompleteFilterPredicate ItemFilter #region public AutoCompleteStringFilterPredicate TextFilter public AutoCompleteFilterPredicate<string> TextFilter`
- `public static readonly DependencyProperty TextFilterProperty = DependencyProperty.Register( "TextFilter", typeof(AutoCompleteFilterPredicate<string>), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith)));`
- `public AutoCompleteStringFilterPredicate TextFilter #if WINDOWS_PHONE #region public InputScope InputScope public InputScope InputScope`
- `public static readonly DependencyProperty InputScopeProperty = DependencyProperty.Register( "InputScope", typeof(InputScope), typeof(AutoCompleteBox), null);`
- `public InputScope InputScope #endif #region Template parts private PopupHelper DropDownPopup`
- `public event RoutedEventHandler TextChanged;`
- `public event PopulatingEventHandler Populating;`
- `public event PopulatedEventHandler Populated;`
- `public event RoutedPropertyChangingEventHandler<bool> DropDownOpening;`
- `public event RoutedPropertyChangedEventHandler<bool> DropDownOpened;`
- `public event RoutedPropertyChangingEventHandler<bool> DropDownClosing;`
- `public event RoutedPropertyChangedEventHandler<bool> DropDownClosed;`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public Binding ValueMemberBinding`
- `public string ValueMemberPath`
- `public AutoCompleteBox()`
- `public override void OnApplyTemplate()`
- `public void PopulateComplete()`

### Microsoft.Phone.Controls.AutoCompleteFilterMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/AutoCompleteFilterMode.cs`

### Microsoft.Phone.Controls.ContextMenu (class)
Source: `Microsoft.Phone.Controls.Toolkit/ContextMenu/ContextMenu.cs`
- `public DependencyObject Owner`
- `public bool IsZoomEnabled`
- `public static readonly DependencyProperty IsZoomEnabledProperty = DependencyProperty.Register( "IsZoomEnabled", typeof(bool), typeof(ContextMenu), new PropertyMetadata(true));`
- `public bool IsFadeEnabled`
- `public static readonly DependencyProperty IsFadeEnabledProperty = DependencyProperty.Register( "IsFadeEnabled", typeof(bool), typeof(ContextMenu), new PropertyMetadata(true));`
- `public double VerticalOffset`
- `public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register( "VerticalOffset", typeof(double), typeof(ContextMenu), new PropertyMetadata(0.0, OnVerticalOffsetChanged));`
- `public bool IsOpen`
- `public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof(bool), typeof(ContextMenu), new PropertyMetadata(false, OnIsOpenChanged));`
- `public Rect? RegionOfInterest`
- `public static readonly DependencyProperty RegionOfInterestProperty = DependencyProperty.Register("RegionOfInterest", typeof(Rect?), typeof(ContextMenu), null);`
- `public event RoutedEventHandler Opened;`
- `public event RoutedEventHandler Closed;`
- `public ContextMenu()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.ContextMenuService (class)
Source: `Microsoft.Phone.Controls.Toolkit/ContextMenu/ContextMenuService.cs`
- `public static ContextMenu GetContextMenu(DependencyObject element)`
- `public static void SetContextMenu(DependencyObject element, ContextMenu value)`
- `public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.RegisterAttached( "ContextMenu", typeof(ContextMenu), typeof(ContextMenuService), new PropertyMetadata(null, OnContextMenuChanged));`

### Microsoft.Phone.Controls.CultureInfoExtensions (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/CultureInfoExtensions.cs`
- `public methods", MessageId = "0", Justification = "Extension method.")] public static ReadOnlyCollection<string> Weekdays(this CultureInfo culture)`
- `public methods", MessageId = "0", Justification = "Extension method.")] public static ReadOnlyCollection<string> Weekends(this CultureInfo culture)`

### Microsoft.Phone.Controls.CustomMessageBox (class)
Source: `Microsoft.Phone.Controls.Toolkit/CustomMessageBox/CustomMessageBox.cs`
- `public event EventHandler<DismissingEventArgs> Dismissing;`
- `public event EventHandler<DismissedEventArgs> Dismissed;`
- `public string Title`
- `public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnTitlePropertyChanged));`
- `public string Caption`
- `public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnCaptionPropertyChanged));`
- `public string Message`
- `public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnMessagePropertyChanged));`
- `public Object LeftButtonContent`
- `public static readonly DependencyProperty LeftButtonContentProperty = DependencyProperty.Register("LeftButtonContent", typeof(Object), typeof(CustomMessageBox), new PropertyMetadata(null, OnLeftButtonContentPropertyChanged));`
- `public Object RightButtonContent`
- `public static readonly DependencyProperty RightButtonContentProperty = DependencyProperty.Register("RightButtonContent", typeof(Object), typeof(CustomMessageBox), new PropertyMetadata(null, OnRightButtonContentPropertyChanged));`
- `public bool IsLeftButtonEnabled`
- `public static readonly DependencyProperty IsLeftButtonEnabledProperty = DependencyProperty.Register("IsLeftButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));`
- `public bool IsRightButtonEnabled`
- `public static readonly DependencyProperty IsRightButtonEnabledProperty = DependencyProperty.Register("IsRightButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));`
- `public bool IsFullScreen`
- `public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(false, OnIsFullScreenPropertyChanged));`
- `public override void OnApplyTemplate()`
- `public CustomMessageBox() : base()`
- `public void Show()`
- `public void Dismiss()`

### Microsoft.Phone.Controls.CustomMessageBoxResult (enum)
Source: `Microsoft.Phone.Controls.Toolkit/CustomMessageBox/CustomMessageBoxResult.cs`

### Microsoft.Phone.Controls.DailyDateTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/DailyDateTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.DatePicker (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DatePicker.cs`
- `public DatePicker()`

### Microsoft.Phone.Controls.DatePickerPage (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/DateTimePickers/DatePickerPage.xaml.cs`, `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DatePickerPage.xaml.cs`
- `public DatePickerPage()`
- `public override void SetFlowDirection(FlowDirection flowDirection)`

### Microsoft.Phone.Controls.DateTimePickerBase (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerBase.cs`
- `public event EventHandler<DateTimeValueChangedEventArgs> ValueChanged;`
- `public DateTime? Value`
- `public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof(DateTime?), typeof(DateTimePickerBase), new PropertyMetadata(null, OnValueChanged));`
- `public string ValueString`
- `public static readonly DependencyProperty ValueStringProperty = DependencyProperty.Register( "ValueString", typeof(string), typeof(DateTimePickerBase), null);`
- `public string ValueStringFormat`
- `public static readonly DependencyProperty ValueStringFormatProperty = DependencyProperty.Register( "ValueStringFormat", typeof(string), typeof(DateTimePickerBase), new PropertyMetadata(null, OnValueStringFormatChanged));`
- `public object Header`
- `public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register( "Header", typeof(object), typeof(DateTimePickerBase), null);`
- `public DataTemplate HeaderTemplate`
- `public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register( "HeaderTemplate", typeof(DataTemplate), typeof(DateTimePickerBase), null);`
- `public Uri PickerPageUri`
- `public static readonly DependencyProperty PickerPageUriProperty = DependencyProperty.Register( "PickerPageUri", typeof(Uri), typeof(DateTimePickerBase), null);`
- `public DateTimePickerBase()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.DateTimePickerResources (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerResources.cs`
- `public static string DatePickerTitle`
- `public static string TimePickerTitle`

### Microsoft.Phone.Controls.DateTimeValueChangedEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimeValueChangedEventArgs.cs`
- `public DateTimeValueChangedEventArgs(DateTime? oldDateTime, DateTime? newDateTime)`
- `public DateTime? OldDateTime`
- `public DateTime? NewDateTime`

### Microsoft.Phone.Controls.DismissedEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/CustomMessageBox/DismissedEventArgs.cs`
- `public DismissedEventArgs(CustomMessageBoxResult result)`
- `public CustomMessageBoxResult Result`

### Microsoft.Phone.Controls.DismissingEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/CustomMessageBox/DismissingEventArgs.cs`
- `public DismissingEventArgs(CustomMessageBoxResult result)`
- `public CustomMessageBoxResult Result`

### Microsoft.Phone.Controls.DragCompletedGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public double HorizontalChange`
- `public double VerticalChange`
- `public Orientation Direction`
- `public double HorizontalVelocity`
- `public double VerticalVelocity`

### Microsoft.Phone.Controls.DragDeltaGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public double HorizontalChange`
- `public double VerticalChange`
- `public Orientation Direction`

### Microsoft.Phone.Controls.DragStartedGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public Orientation Direction`

### Microsoft.Phone.Controls.ExpanderView (class)
Source: `Microsoft.Phone.Controls.Toolkit/ExpanderView/ExpanderView.cs`
- `public const string ExpansionStates = "ExpansionStates";`
- `public const string ExpandabilityStates = "ExpandabilityStates";`
- `public const string CollapsedState = "Collapsed";`
- `public const string ExpandedState = "Expanded";`
- `public const string ExpandableState = "Expandable";`
- `public const string NonExpandableState = "NonExpandable";`
- `public event RoutedEventHandler Expanded;`
- `public event RoutedEventHandler Collapsed;`
- `public object Expander`
- `public static readonly DependencyProperty ExpanderProperty = DependencyProperty.Register("Expander", typeof(object), typeof(ExpanderView), new PropertyMetadata(null, OnExpanderPropertyChanged));`
- `public DataTemplate ExpanderTemplate`
- `public static readonly DependencyProperty ExpanderTemplateProperty = DependencyProperty.Register("ExpanderTemplate", typeof(DataTemplate), typeof(ExpanderView), new PropertyMetadata(null, OnExpanderTemplatePropertyChanged));`
- `public object NonExpandableHeader`
- `public static readonly DependencyProperty NonExpandableHeaderProperty = DependencyProperty.Register("NonExpandableHeader", typeof(object), typeof(ExpanderView), new PropertyMetadata(null, OnNonExpandableHeaderPropertyChanged));`
- `public DataTemplate NonExpandableHeaderTemplate`
- `public static readonly DependencyProperty NonExpandableHeaderTemplateProperty = DependencyProperty.Register("NonExpandableHeaderTemplate", typeof(DataTemplate), typeof(ExpanderView), new PropertyMetadata(null, OnNonExpandableHeaderTemplatePropertyChanged));`
- `public bool IsExpanded`
- `public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false, OnIsExpandedPropertyChanged));`
- `public bool HasItems`
- `public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register("HasItems", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false, null));`
- `public bool IsNonExpandable`
- `public static readonly DependencyProperty IsNonExpandableProperty = DependencyProperty.Register("IsNonExpandable", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false, OnIsNonExpandablePropertyChanged));`
- `public override void OnApplyTemplate()`
- `public ExpanderView()`

### Microsoft.Phone.Controls.ExpansionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/ListPicker/ExpansionMode.cs`

### Microsoft.Phone.Controls.FeatheredTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/FeatheredTransition.cs`
- `public FeatheredTransition(UIElement element, Storyboard storyboard, TurnstileFeatherTransitionMode mode, TimeSpan? beginTime) : base(element, storyboard)`
- `public override void Begin()`

### Microsoft.Phone.Controls.FlickGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public double HorizontalVelocity`
- `public double VerticalVelocity`
- `public double Angle`
- `public Orientation Direction`

### Microsoft.Phone.Controls.FullViewDateTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/FullViewDateTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.GestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public object OriginalSource`
- `public bool Handled`
- `public Point GetPosition(UIElement relativeTo)`

### Microsoft.Phone.Controls.GestureListener (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP7/Input/GestureListenerStatic.cs`, `Microsoft.Phone.Controls.Toolkit.WP8/Input/GestureListenerStatic.cs`, `Microsoft.Phone.Controls.Toolkit/Input/GestureListener.cs`
- `public event EventHandler<GestureEventArgs> GestureBegin;`
- `public event EventHandler<GestureEventArgs> GestureCompleted;`
- `public event EventHandler<GestureEventArgs> Tap;`
- `public event EventHandler<GestureEventArgs> DoubleTap;`
- `public event EventHandler<GestureEventArgs> Hold;`
- `public event EventHandler<DragStartedGestureEventArgs> DragStarted;`
- `public event EventHandler<DragDeltaGestureEventArgs> DragDelta;`
- `public event EventHandler<DragCompletedGestureEventArgs> DragCompleted;`
- `public event EventHandler<FlickGestureEventArgs> Flick;`
- `public event EventHandler<PinchStartedGestureEventArgs> PinchStarted;`
- `public event EventHandler<PinchGestureEventArgs> PinchDelta;`
- `public event EventHandler<PinchGestureEventArgs> PinchCompleted;`

### Microsoft.Phone.Controls.GestureService (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP7/Input/GestureService.cs`, `Microsoft.Phone.Controls.Toolkit.WP8/Input/GestureService.cs`
- `public static GestureListener GetGestureListener(DependencyObject obj)`
- `public static void SetGestureListener(DependencyObject obj, GestureListener value)`
- `public static readonly DependencyProperty GestureListenerProperty = DependencyProperty.RegisterAttached("GestureListener", typeof(GestureListener), typeof(GestureService), new PropertyMetadata(null));`

### Microsoft.Phone.Controls.GroupViewClosingEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs`
- `public ItemsControl ItemsControl`
- `public object SelectedGroup`
- `public bool Cancel`

### Microsoft.Phone.Controls.GroupViewOpenedEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs`
- `public ItemsControl ItemsControl`

### Microsoft.Phone.Controls.HourlyDateTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/HourlyDateTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.HubTile (class)
Source: `Microsoft.Phone.Controls.Toolkit/HubTile/HubTile.cs`
- `public ImageSource Source`
- `public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(HubTile), new PropertyMetadata(null));`
- `public string Title`
- `public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTitleChanged)));`
- `public string Notification`
- `public static readonly DependencyProperty NotificationProperty = DependencyProperty.Register("Notification", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnBackContentChanged)));`
- `public string Message`
- `public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnBackContentChanged)));`
- `public bool DisplayNotification`
- `public static readonly DependencyProperty DisplayNotificationProperty = DependencyProperty.Register("DisplayNotification", typeof(bool), typeof(HubTile), new PropertyMetadata(false, new PropertyChangedCallback(OnBackContentChanged)));`
- `public bool IsFrozen`
- `public static readonly DependencyProperty IsFrozenProperty = DependencyProperty.Register("IsFrozen", typeof(bool), typeof(HubTile), new PropertyMetadata(false, new PropertyChangedCallback(OnIsFrozenChanged)));`
- `public string GroupTag`
- `public static readonly DependencyProperty GroupTagProperty = DependencyProperty.Register("GroupTag", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty));`
- `public TileSize Size`
- `public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(TileSize), typeof(HubTile), new PropertyMetadata(TileSize.Default, OnSizeChanged));`
- `public override void OnApplyTemplate()`
- `public HubTile()`

### Microsoft.Phone.Controls.HubTileService (class)
Source: `Microsoft.Phone.Controls.Toolkit/HubTile/HubTileService.cs`
- `public static void FreezeHubTile(HubTile tile)`
- `public static void UnfreezeHubTile(HubTile tile)`
- `public static void FreezeGroup(string group)`
- `public static void UnfreezeGroup(string group)`

### Microsoft.Phone.Controls.ISelectionAdapter (interface)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/ISelectionAdapter.cs`

### Microsoft.Phone.Controls.ITransition (interface)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/ITransition.cs`

### Microsoft.Phone.Controls.LengthConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/LengthConverter.cs`
- `public LengthConverter()`
- `public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)`
- `public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)`
- `public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)`
- `public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)`

### Microsoft.Phone.Controls.LinkUnlinkEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorEventArgs.cs`
- `public LinkUnlinkEventArgs(ContentPresenter contentPresenter)`
- `public ContentPresenter ContentPresenter`

### Microsoft.Phone.Controls.ListPicker (class)
Source: `Microsoft.Phone.Controls.Toolkit/ListPicker/ListPicker.cs`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public Func<IList, string> SummaryForSelectedItemsDelegate`
- `public static readonly DependencyProperty SummaryForSelectedItemsDelegateProperty = DependencyProperty.Register("SummaryForSelectedItemsDelegate", typeof(Func<IList, string>), typeof(ListPicker), null);`
- `public ListPickerMode ListPickerMode`
- `public static readonly DependencyProperty ListPickerModeProperty = DependencyProperty.Register("ListPickerMode", typeof(ListPickerMode), typeof(ListPicker), new PropertyMetadata(ListPickerMode.Normal, OnListPickerModeChanged));`
- `public int SelectedIndex`
- `public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ListPicker), new PropertyMetadata(-1, OnSelectedIndexChanged));`
- `public object SelectedItem`
- `public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListPicker), new PropertyMetadata(null, OnSelectedItemChanged));`
- `public DataTemplate FullModeItemTemplate`
- `public static readonly DependencyProperty FullModeItemTemplateProperty = DependencyProperty.Register("FullModeItemTemplate", typeof(DataTemplate), typeof(ListPicker), new PropertyMetadata(null, OnShadowOrFullModeItemTemplateChanged));`
- `public object Header`
- `public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ListPicker), null);`
- `public DataTemplate HeaderTemplate`
- `public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ListPicker), null);`
- `public object FullModeHeader`
- `public static readonly DependencyProperty FullModeHeaderProperty = DependencyProperty.Register("FullModeHeader", typeof(object), typeof(ListPicker), null);`
- `public int ItemCountThreshold`
- `public static readonly DependencyProperty ItemCountThresholdProperty = DependencyProperty.Register("ItemCountThreshold", typeof(int), typeof(ListPicker), new PropertyMetadata(5, OnItemCountThresholdChanged));`
- `public Uri PickerPageUri`
- `public static readonly DependencyProperty PickerPageUriProperty = DependencyProperty.Register( "PickerPageUri", typeof(Uri), typeof(ListPicker), null);`
- `public ExpansionMode ExpansionMode`
- `public static readonly DependencyProperty ExpansionModeProperty = DependencyProperty.Register( "ExpansionMode", typeof(ExpansionMode), typeof(ListPicker), new PropertyMetadata(ExpansionMode.ExpansionAllowed, null) );`
- `public SelectionMode SelectionMode`
- `public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register( "SelectionMode", typeof(SelectionMode), typeof(ListPicker), new PropertyMetadata(SelectionMode.Single, OnSelectionModeChanged) );`
- `public IList SelectedItems`
- `public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register( "SelectedItems", typeof(IList), typeof(ListPicker), new PropertyMetadata(OnSelectedItemsChanged) );`
- `public ListPicker()`
- `public override void OnApplyTemplate()`
- `public bool Open()`

### Microsoft.Phone.Controls.ListPickerItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/ListPicker/ListPickerItem.cs`
- `public ListPickerItem()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.ListPickerMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/ListPicker/ListPickerMode.cs`

### Microsoft.Phone.Controls.ListPickerPage (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP7/ListPicker/ListPickerPage.xaml.cs`, `Microsoft.Phone.Controls.Toolkit.WP8/ListPicker/ListPickerPage.xaml.cs`
- `public string HeaderText`
- `public IList Items`
- `public SelectionMode SelectionMode`
- `public object SelectedItem`
- `public IList SelectedItems`
- `public DataTemplate FullModeItemTemplate`
- `public ListPickerPage()`

### Microsoft.Phone.Controls.ListViewDateTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/ListViewDateTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.LockablePivot (class)
Source: `Microsoft.Phone.Controls.Toolkit/Pivot/LockablePivot.cs`
- `public bool IsLocked public bool IsLocked`
- `public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register( "IsLocked", typeof(bool), typeof(LockablePivot), new PropertyMetadata(false, OnIsLockedPropertyChanged));`
- `public bool IsLocked private struct HeaderAnimationInfo`
- `public LockablePivot()`

### Microsoft.Phone.Controls.LongListMultiSelector (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/LongListMultiSelector/LongListMultiSelector.cs`
- `public ManipulationState ManipulationState`
- `public Size GridCellSize`
- `public static readonly DependencyProperty GridCellSizeProperty = DependencyProperty.Register("GridCellSize", typeof(Size), typeof(LongListMultiSelector), new PropertyMetadata(Size.Empty));`
- `public DataTemplate GroupFooterTemplate`
- `public static readonly DependencyProperty GroupFooterTemplateProperty = DependencyProperty.Register("GroupFooterTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public DataTemplate GroupHeaderTemplate`
- `public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register("GroupHeaderTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public bool HideEmptyGroups`
- `public static readonly DependencyProperty HideEmptyGroupsProperty = DependencyProperty.Register("HideEmptyGroups", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false));`
- `public bool IsGroupingEnabled`
- `public static readonly DependencyProperty IsGroupingEnabledProperty = DependencyProperty.Register("IsGroupingEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false));`
- `public Style ItemContainerStyle`
- `public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemContainerStylePropertyChanged));`
- `public IList ItemsSource`
- `public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemsSourcePropertyChanged));`
- `public DataTemplate ItemTemplate`
- `public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public DataTemplate ItemInfoTemplate`
- `public static readonly DependencyProperty ItemInfoTemplateProperty = DependencyProperty.Register("ItemInfoTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null, OnItemInfoTemplatePropertyChanged));`
- `public Style JumpListStyle`
- `public static readonly DependencyProperty JumpListStyleProperty = DependencyProperty.Register("JumpListStyle", typeof(Style), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public LongListSelectorLayoutMode LayoutMode`
- `public object ListFooter`
- `public static readonly DependencyProperty ListFooterProperty = DependencyProperty.Register("ListFooter", typeof(object), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public DataTemplate ListFooterTemplate`
- `public static readonly DependencyProperty ListFooterTemplateProperty = DependencyProperty.Register("ListFooterTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public object ListHeader`
- `public static readonly DependencyProperty ListHeaderProperty = DependencyProperty.Register("ListHeader", typeof(object), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public DataTemplate ListHeaderTemplate`
- `public static readonly DependencyProperty ListHeaderTemplateProperty = DependencyProperty.Register("ListHeaderTemplate", typeof(DataTemplate), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public IList SelectedItems`
- `public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IList), typeof(LongListMultiSelector), new PropertyMetadata(null));`
- `public bool IsSelectionEnabled`
- `public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false, OnIsSelectionEnabledPropertyChanged));`
- `public bool EnforceIsSelectionEnabled`
- `public static readonly DependencyProperty EnforceIsSelectionEnabledProperty = DependencyProperty.Register("EnforceIsSelectionEnabled", typeof(bool), typeof(LongListMultiSelector), new PropertyMetadata(false, OnEnforceIsSelectionEnabledPropertyChanged));`
- `public event EventHandler<ItemRealizationEventArgs> ItemRealized;`
- `public event EventHandler<ItemRealizationEventArgs> ItemUnrealized;`
- `public event EventHandler JumpListClosed;`
- `public event EventHandler JumpListOpening;`
- `public event EventHandler ManipulationStateChanged;`
- `public event PropertyChangedEventHandler PropertyChanged;`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public event DependencyPropertyChangedEventHandler IsSelectionEnabledChanged;`
- `public LongListMultiSelector()`
- `public override void OnApplyTemplate()`
- `public object ContainerFromItem(object item)`
- `public void ScrollTo(object item)`

### Microsoft.Phone.Controls.LongListMultiSelectorItem (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/LongListMultiSelector/LongListMultiSelectorItem.cs`
- `public object ContentInfo`
- `public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register("ContentInfo", typeof(object), typeof(LongListMultiSelectorItem), new PropertyMetadata(null));`
- `public DataTemplate ContentInfoTemplate`
- `public static readonly DependencyProperty ContentInfoTemplateProperty = DependencyProperty.Register("ContentInfoTemplate", typeof(DataTemplate), typeof(LongListMultiSelectorItem), new PropertyMetadata(null));`
- `public bool IsSelected`
- `public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(LongListMultiSelectorItem), new PropertyMetadata(false, OnIsSelectedPropertyChanged));`
- `public double HintPanelHeight`
- `public static readonly DependencyProperty HintPanelHeightProperty = DependencyProperty.Register("HintPanelHeight", typeof(double), typeof(LongListMultiSelectorItem), new PropertyMetadata(Double.NaN, OnHintPanelHeightPropertyChanged));`
- `public event EventHandler IsSelectedChanged;`
- `public LongListMultiSelectorItem()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.LongListSelector (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelector.cs`, `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorGroup.cs`, `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItemsControl.cs`
- `public bool IsFlatList`
- `public setter.")] [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Backward compatible public setter.")] [Obsolete("IsBouncy is always set to true.")] public bool IsBouncy`
- `public bool IsScrolling`
- `public bool IsStretching`
- `public IEnumerable ItemsSource`
- `public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(LongListSelector), new PropertyMetadata(null, OnItemsSourceChanged));`
- `public object SelectedItem`
- `public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(LongListSelector), new PropertyMetadata(null, OnSelectedItemChanged));`
- `public object ListHeader`
- `public static readonly DependencyProperty ListHeaderProperty = DependencyProperty.Register("ListHeader", typeof(object), typeof(LongListSelector), new PropertyMetadata(null));`
- `public DataTemplate ListHeaderTemplate`
- `public static readonly DependencyProperty ListHeaderTemplateProperty = DependencyProperty.Register("ListHeaderTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null, OnDataTemplateChanged));`
- `public object ListFooter`
- `public static readonly DependencyProperty ListFooterProperty = DependencyProperty.Register("ListFooter", typeof(object), typeof(LongListSelector), new PropertyMetadata(null));`
- `public DataTemplate ListFooterTemplate`
- `public static readonly DependencyProperty ListFooterTemplateProperty = DependencyProperty.Register("ListFooterTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null, OnDataTemplateChanged));`
- `public DataTemplate GroupHeaderTemplate`
- `public static readonly DependencyProperty GroupHeaderProperty = DependencyProperty.Register("GroupHeaderTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null, OnDataTemplateChanged));`
- `public DataTemplate GroupFooterTemplate`
- `public static readonly DependencyProperty GroupFooterProperty = DependencyProperty.Register("GroupFooterTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null, OnDataTemplateChanged));`
- `public DataTemplate ItemTemplate`
- `public static readonly DependencyProperty ItemsTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null, OnDataTemplateChanged));`
- `public bool DisplayAllGroups`
- `public static readonly DependencyProperty DisplayAllGroupsProperty = DependencyProperty.Register("DisplayAllGroups", typeof(bool), typeof(LongListSelector), new PropertyMetadata(false, OnDisplayAllGroupsChanged));`
- `public DataTemplate GroupItemTemplate`
- `public static readonly DependencyProperty GroupItemTemplateProperty = DependencyProperty.Register("GroupItemTemplate", typeof(DataTemplate), typeof(LongListSelector), new PropertyMetadata(null));`
- `public ItemsPanelTemplate GroupItemsPanel`
- `public static readonly DependencyProperty GroupItemsPanelProperty = DependencyProperty.Register("GroupItemsPanel", typeof(ItemsPanelTemplate), typeof(LongListSelector), new PropertyMetadata(null));`
- `public double BufferSize`
- `public static readonly DependencyProperty BufferSizeProperty = DependencyProperty.Register("BufferSize", typeof(double), typeof(LongListSelector), new PropertyMetadata(BufferSizeDefault, OnBufferSizeChanged));`
- `public double MaximumFlickVelocity`
- `public static readonly DependencyProperty MaximumFlickVelocityProperty = DependencyProperty.Register("MaximumFlickVelocity", typeof(double), typeof(LongListSelector), new PropertyMetadata(MotionParameters.MaximumSpeed));`
- `public bool ShowListHeader`
- `public static readonly DependencyProperty ShowListHeaderProperty = DependencyProperty.Register("ShowListHeader", typeof(bool), typeof(LongListSelector), new PropertyMetadata(true, OnShowListHeaderChanged));`
- `public bool ShowListFooter`
- `public static readonly DependencyProperty ShowListFooterProperty = DependencyProperty.Register("ShowListFooter", typeof(bool), typeof(LongListSelector), new PropertyMetadata(true, OnShowListFooterChanged));`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public event EventHandler ScrollingStarted;`
- `public event EventHandler ScrollingCompleted;`
- `public event EventHandler<GroupViewOpenedEventArgs> GroupViewOpened;`
- `public event EventHandler<GroupViewClosingEventArgs> GroupViewClosing;`
- `public event EventHandler<LinkUnlinkEventArgs> Link;`
- `public event EventHandler<LinkUnlinkEventArgs> Unlink;`
- `public event EventHandler StretchingBottom;`
- `public event EventHandler StretchingCompleted;`
- `public event EventHandler StretchingTop;`
- `public LongListSelector()`
- `public void ScrollTo(object item)`
- `public void ScrollToGroup(object group)`
- `public void DisplayGroupView()`
- `public void CloseGroupView()`
- `public void AnimateTo(object item)`
- `public ICollection<Object> GetItemsWithContainers(bool onlyItemsInView, bool getContainers)`
- `public ICollection<Object> GetItemsInView()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.LongListSelectorItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItem.cs`
- `public LongListSelectorItemType ItemType`
- `public object Group`
- `public object Item`

### Microsoft.Phone.Controls.LongListSelectorItemType (enum)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/LongListSelectorItemType.cs`

### Microsoft.Phone.Controls.MenuItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/ContextMenu/MenuItem.cs`
- `public event RoutedEventHandler Click;`
- `public ICommand Command`
- `public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( "Command", typeof(ICommand), typeof(MenuItem), new PropertyMetadata(null, OnCommandChanged));`
- `public object CommandParameter`
- `public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register( "CommandParameter", typeof(object), typeof(MenuItem), new PropertyMetadata(null, OnCommandParameterChanged));`
- `public MenuItem()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.MultiTouchGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public Point GetPosition(UIElement relativeTo, int index)`

### Microsoft.Phone.Controls.MultiselectItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/MultiselectList/MultiselectItem.cs`
- `public event RoutedEventHandler Selected;`
- `public event RoutedEventHandler Unselected;`
- `public bool IsSelected`
- `public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(MultiselectItem), new PropertyMetadata(false, OnIsSelectedPropertyChanged));`
- `public double HintPanelHeight`
- `public static readonly DependencyProperty HintPanelHeightProperty = DependencyProperty.Register("HintPanelHeight", typeof(double), typeof(MultiselectItem), new PropertyMetadata(Double.NaN, null));`
- `public object ContentInfo`
- `public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register("ContentInfo", typeof(object), typeof(MultiselectItem), new PropertyMetadata(null, OnContentInfoPropertyChanged));`
- `public DataTemplate ContentInfoTemplate`
- `public static readonly DependencyProperty ContentInfoTemplateProperty = DependencyProperty.Register("ContentInfoTemplate", typeof(DataTemplate), typeof(MultiselectItem), new PropertyMetadata(null, OnContentInfoTemplatePropertyChanged));`
- `public override void OnApplyTemplate()`
- `public MultiselectItem()`

### Microsoft.Phone.Controls.MultiselectList (class)
Source: `Microsoft.Phone.Controls.Toolkit/MultiselectList/MultiselectList.cs`
- `public IList SelectedItems`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public event DependencyPropertyChangedEventHandler IsSelectionEnabledChanged;`
- `public bool IsSelectionEnabled`
- `public static readonly DependencyProperty IsInSelectionModeProperty = DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(MultiselectList), new PropertyMetadata(false, OnIsSelectionEnabledPropertyChanged));`
- `public DataTemplate ItemInfoTemplate`
- `public static readonly DependencyProperty ItemInfoTemplateProperty = DependencyProperty.Register("ItemInfoTemplate", typeof(DataTemplate), typeof(MultiselectList), new PropertyMetadata(null, null));`
- `public Style ItemContainerStyle`
- `public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(MultiselectList), new PropertyMetadata(null, null));`
- `public MultiselectList()`

### Microsoft.Phone.Controls.NavigationInTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/NavigationInTransition.cs`

### Microsoft.Phone.Controls.NavigationOutTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/NavigationOutTransition.cs`

### Microsoft.Phone.Controls.NavigationTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/NavigationTransition.cs`
- `public static readonly DependencyProperty BackwardProperty = DependencyProperty.Register("Backward", typeof(TransitionElement), typeof(NavigationTransition), null);`
- `public static readonly DependencyProperty ForwardProperty = DependencyProperty.Register("Forward", typeof(TransitionElement), typeof(NavigationTransition), null);`
- `public event RoutedEventHandler BeginTransition;`
- `public event RoutedEventHandler EndTransition;`
- `public TransitionElement Backward`
- `public TransitionElement Forward`

### Microsoft.Phone.Controls.NullToVisibilityConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/TransferControl/NullToVisibilityConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.OffOnConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/ToggleSwitch/OffOnConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.PerformanceProgressBar (class)
Source: `Microsoft.Phone.Controls.Toolkit/ProgressBar/PerformanceProgressBar.cs`
- `public bool ActualIsIndeterminate public bool ActualIsIndeterminate`
- `public static readonly DependencyProperty ActualIsIndeterminateProperty = DependencyProperty.Register( "ActualIsIndeterminate", typeof(bool), typeof(PerformanceProgressBar), new PropertyMetadata(false));`
- `public bool ActualIsIndeterminate #region public bool IsIndeterminate public bool IsIndeterminate`
- `public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register( "IsIndeterminate", typeof(bool), typeof(PerformanceProgressBar), new PropertyMetadata(false, OnIsIndeterminatePropertyChanged));`
- `public bool IsIndeterminate public PerformanceProgressBar() : base()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.PhoneTextBox (class)
Source: `Microsoft.Phone.Controls.Toolkit/PhoneTextBox/PhoneTextBox.cs`
- `public static readonly DependencyProperty HintProperty = DependencyProperty.Register("Hint", typeof(string), typeof(PhoneTextBox), new PropertyMetadata( new PropertyChangedCallback(OnHintPropertyChanged) ) );`
- `public string Hint`
- `public static readonly DependencyProperty HintStyleProperty = DependencyProperty.Register("HintStyle", typeof(Style), typeof(PhoneTextBox), null);`
- `public Style HintStyle`
- `public static readonly DependencyProperty ActualHintVisibilityProperty = DependencyProperty.Register("ActualHintVisibility", typeof(Visibility), typeof(PhoneTextBox), null);`
- `public Visibility ActualHintVisibility`
- `public static readonly DependencyProperty LengthIndicatorVisibleProperty = DependencyProperty.Register("LengthIndicatorVisible", typeof(Boolean), typeof(PhoneTextBox), null);`
- `public Boolean LengthIndicatorVisible`
- `public static readonly DependencyProperty LengthIndicatorThresholdProperty = DependencyProperty.Register("LengthIndicatorThreshold", typeof(int), typeof(PhoneTextBox), new PropertyMetadata( new PropertyChangedCallback(OnLengthIndicatorThresholdChanged) ) );`
- `public int LengthIndicatorThreshold`
- `public static readonly DependencyProperty DisplayedMaxLengthProperty = DependencyProperty.Register("DisplayedMaxLength", typeof(int), typeof(PhoneTextBox), new PropertyMetadata( new PropertyChangedCallback(DisplayedMaxLengthChanged) ) );`
- `public int DisplayedMaxLength`
- `public static readonly DependencyProperty ActionIconProperty = DependencyProperty.Register("ActionIcon", typeof(ImageSource), typeof(PhoneTextBox), new PropertyMetadata( new PropertyChangedCallback(OnActionIconChanged) ) );`
- `public ImageSource ActionIcon`
- `public bool HidesActionItemWhenEmpty`
- `public static readonly DependencyProperty HidesActionItemWhenEmptyProperty = DependencyProperty.Register("HidesActionItemWhenEmpty", typeof(bool), typeof(PhoneTextBox), new PropertyMetadata(false, OnActionIconChanged));`
- `public event EventHandler ActionIconTapped;`
- `public PhoneTextBox()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.PinchGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public double DistanceRatio`
- `public double TotalAngleDelta`

### Microsoft.Phone.Controls.PinchStartedGestureEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Input/GestureHelperEventArgs.cs`
- `public double Distance`
- `public double Angle`

### Microsoft.Phone.Controls.PopulatedEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/PopulatedEventArgs.cs`
- `public IEnumerable Data`
- `public PopulatedEventArgs(IEnumerable data)`

### Microsoft.Phone.Controls.PopulatingEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/PopulatingEventArgs.cs`
- `public string Parameter`
- `public bool Cancel`
- `public PopulatingEventArgs(string parameter)`

### Microsoft.Phone.Controls.Rating (class)
Source: `Microsoft.Phone.Controls.Toolkit/RatingControl/Rating.cs`
- `public event EventHandler ValueChanged;`
- `public Rating()`
- `public override void OnApplyTemplate()`
- `public Style FilledItemStyle public Style FilledItemStyle`
- `public static readonly DependencyProperty FilledItemStyleProperty = DependencyProperty.Register("FilledItemStyle", typeof(Style), typeof(Rating), new PropertyMetadata(OnFilledItemStyleChanged));`
- `public style UnfilledItemStyle public Style UnfilledItemStyle`
- `public static readonly DependencyProperty UnfilledItemStyleProperty = DependencyProperty.Register("UnfilledItemStyle", typeof(Style), typeof(Rating), new PropertyMetadata(OnUnfilledItemStyleChanged));`
- `public int RatingItemCount public int RatingItemCount`
- `public static readonly DependencyProperty RatingItemCountProperty = DependencyProperty.Register("RatingItemCount", typeof(int), typeof(Rating), new PropertyMetadata(5, OnRatingItemCountChanged));`
- `public double Value [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Property traditionally named value")] public double Value`
- `public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Rating), new PropertyMetadata(0.0, OnValueChanged));`
- `public boolean ReadOnly public bool ReadOnly`
- `public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(Rating), new PropertyMetadata(false));`
- `public boolean AllowHalfItemIncrement public bool AllowHalfItemIncrement`
- `public static readonly DependencyProperty AllowHalfItemIncrementProperty = DependencyProperty.Register("AllowHalfItemIncrement", typeof(bool), typeof(Rating), new PropertyMetadata(false));`
- `public boolean AllowSelectingZero public bool AllowSelectingZero`
- `public static readonly DependencyProperty AllowSelectingZeroProperty = DependencyProperty.Register("AllowSelectingZero", typeof(bool), typeof(Rating), new PropertyMetadata(false));`
- `public boolean ShowSelectionHelper public bool ShowSelectionHelper`
- `public static readonly DependencyProperty ShowSelectionHelperProperty = DependencyProperty.Register("ShowSelectionHelperItems", typeof(bool), typeof(Rating), new PropertyMetadata(false));`
- `public Orientation Orientation public Orientation Orientation`
- `public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Rating), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));`

### Microsoft.Phone.Controls.RatingItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/RatingControl/RatingItem.cs`
- `public RatingItem()`
- `public double StrokeThickness public double StrokeThickness`
- `public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThinkness", typeof(double), typeof(RatingItem), null);`

### Microsoft.Phone.Controls.RecurringDaysPicker (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/RecurringDaysPicker.cs`
- `public RecurringDaysPicker()`

### Microsoft.Phone.Controls.RelativeTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/RelativeTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.RollTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RollTransition.cs`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.RotateTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RotateTransition.cs`
- `public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(RotateTransitionMode), typeof(RotateTransition), null);`
- `public RotateTransitionMode Mode`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.RotateTransitionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/RotateTransitionMode.cs`

### Microsoft.Phone.Controls.RoutedPropertyChangingEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/RoutedPropertyChangingEventArgs.cs`
- `public DependencyProperty Property`
- `public T OldValue`
- `public T NewValue`
- `public bool IsCancelable`
- `public bool Cancel`
- `public bool InCoercion`
- `public RoutedPropertyChangingEventArgs( DependencyProperty property, T oldValue, T newValue, bool isCancelable)`

### Microsoft.Phone.Controls.SelectorSelectionAdapter (class)
Source: `Microsoft.Phone.Controls.Toolkit/AutoCompleteBox/SelectorSelectionAdapter.cs`
- `public Selector SelectorControl`
- `public event SelectionChangedEventHandler SelectionChanged;`
- `public event RoutedEventHandler Commit;`
- `public event RoutedEventHandler Cancel;`
- `public SelectorSelectionAdapter()`
- `public SelectorSelectionAdapter(Selector selector)`
- `public object SelectedItem`
- `public IEnumerable ItemsSource`
- `public void HandleKeyDown(KeyEventArgs e)`
- `public AutomationPeer CreateAutomationPeer()`

### Microsoft.Phone.Controls.Separator (class)
Source: `Microsoft.Phone.Controls.Toolkit/ContextMenu/Separator.cs`
- `public Separator()`

### Microsoft.Phone.Controls.SlideInEffect (class)
Source: `Microsoft.Phone.Controls.Toolkit/Effects/SlideInEffect.cs`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static int GetLineIndex(DependencyObject obj)`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static void SetLineIndex(DependencyObject obj, int value)`
- `public static readonly DependencyProperty LineIndexProperty = DependencyProperty.RegisterAttached("LineIndex", typeof(int), typeof(SlideInEffect), new PropertyMetadata(-1, OnLineIndexPropertyChanged));`

### Microsoft.Phone.Controls.SlideTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SlideTransition.cs`
- `public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(SlideTransitionMode), typeof(SlideTransition), null);`
- `public SlideTransitionMode Mode`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.SlideTransitionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SlideTransitionMode.cs`

### Microsoft.Phone.Controls.SpeechRecognizedEventArgs (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/SpeechTextBox/SpeechRecognizedEventArgs.cs`
- `public SpeechRecognizedEventArgs(SpeechRecognitionResult result)`
- `public SpeechRecognitionResult Result`
- `public bool Canceled`

### Microsoft.Phone.Controls.SpeechTextBox (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/SpeechTextBox/SpeechTextBox.cs`
- `public SpeechTextBox()`
- `public override void OnApplyTemplate()`
- `public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;`

### Microsoft.Phone.Controls.SwivelTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SwivelTransition.cs`
- `public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(SwivelTransitionMode), typeof(SwivelTransition), null);`
- `public SwivelTransitionMode Mode`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.SwivelTransitionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/SwivelTransitionMode.cs`

### Microsoft.Phone.Controls.ThreadDateTimeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimeConverters/ThreadDateTimeConverter.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.TileSize (enum)
Source: `Microsoft.Phone.Controls.Toolkit/HubTile/HubTile.cs`

### Microsoft.Phone.Controls.TileSizeToHeightConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/HubTile/HubTileConverters.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.TileSizeToWidthConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/HubTile/HubTileConverters.cs`
- `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Microsoft.Phone.Controls.TiltEffect (class)
Source: `Microsoft.Phone.Controls.Toolkit/Effects/TiltEffect.cs`
- `public static bool UseLogarithmicEase`
- `public static List<Type> TiltableItems`
- `public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.RegisterAttached( "IsTiltEnabled", typeof(bool), typeof(TiltEffect), new PropertyMetadata(OnIsTiltEnabledChanged) );`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static bool GetIsTiltEnabled(DependencyObject source)`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static void SetIsTiltEnabled(DependencyObject source, bool value)`
- `public static readonly DependencyProperty SuppressTiltProperty = DependencyProperty.RegisterAttached( "SuppressTilt", typeof(bool), typeof(TiltEffect), null );`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static bool GetSuppressTilt(DependencyObject source)`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static void SetSuppressTilt(DependencyObject source, bool value)`

### Microsoft.Phone.Controls.TimePicker (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/TimePicker.cs`
- `public TimePicker()`

### Microsoft.Phone.Controls.TimePickerPage (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/DateTimePickers/TimePickerPage.xaml.cs`, `Microsoft.Phone.Controls.Toolkit/DateTimePickers/TimePickerPage.xaml.cs`
- `public TimePickerPage()`
- `public override void SetFlowDirection(FlowDirection flowDirection)`

### Microsoft.Phone.Controls.ToggleSwitch (class)
Source: `Microsoft.Phone.Controls.Toolkit/ToggleSwitch/ToggleSwitch.cs`
- `public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ToggleSwitch), new PropertyMetadata(null));`
- `public object Header`
- `public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ToggleSwitch), new PropertyMetadata(null));`
- `public DataTemplate HeaderTemplate`
- `public static readonly DependencyProperty SwitchForegroundProperty = DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitch), null);`
- `public Brush SwitchForeground`
- `public bool? IsChecked`
- `public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ToggleSwitch), new PropertyMetadata(false, OnIsCheckedChanged));`
- `public event EventHandler<RoutedEventArgs> Checked;`
- `public event EventHandler<RoutedEventArgs> Unchecked;`
- `public event EventHandler<RoutedEventArgs> Indeterminate;`
- `public event EventHandler<RoutedEventArgs> Click;`
- `public ToggleSwitch()`
- `public override void OnApplyTemplate()`
- `public override string ToString()`

### Microsoft.Phone.Controls.TransferControl (class)
Source: `Microsoft.Phone.Controls.Toolkit/TransferControl/TransferControl.cs`
- `public bool AutoHide`
- `public object Header`
- `public DataTemplate HeaderTemplate`
- `public TransferMonitor Monitor`
- `public Uri Icon`
- `public bool IsContextMenuEnabled`
- `public Style ProgressBarStyle`
- `public Brush StatusTextBrush`
- `public TransferControl()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.TransferMonitor (class)
Source: `Microsoft.Phone.Controls.Toolkit/TransferControl/TransferMonitor.cs`
- `public event EventHandler<BackgroundTransferEventArgs> ProgressChanged;`
- `public event EventHandler<BackgroundTransferEventArgs> Started;`
- `public event EventHandler<BackgroundTransferEventArgs> Failed;`
- `public event EventHandler<BackgroundTransferEventArgs> Complete;`
- `public TransferType TransferType`
- `public string ErrorMessage`
- `public string Name`
- `public double PercentComplete`
- `public TransferRequestState State`
- `public string StatusText`
- `public long BytesTransferred`
- `public long TotalBytesToTransfer`
- `public bool IsProgressIndeterminate`
- `public TransferMonitor(BackgroundTransferRequest request) : this(request, "")`
- `public TransferMonitor(BackgroundTransferRequest request, string name)`
- `public void RequestStart()`
- `public void RequestCancel()`
- `public event PropertyChangedEventHandler PropertyChanged;`

### Microsoft.Phone.Controls.TransferRequestState (enum)
Source: `Microsoft.Phone.Controls.Toolkit/TransferControl/TransferRequestState.cs`

### Microsoft.Phone.Controls.TransferType (enum)
Source: `Microsoft.Phone.Controls.Toolkit/TransferControl/TransferType.cs`

### Microsoft.Phone.Controls.Transition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/Transition.cs`
- `public event EventHandler Completed;`
- `public Transition(UIElement element, Storyboard storyboard)`
- `public virtual void Begin()`
- `public ClockState GetCurrentState()`
- `public TimeSpan GetCurrentTime()`
- `public void Pause()`
- `public void Resume()`
- `public void Seek(TimeSpan offset)`
- `public void SeekAlignedToLastTick(TimeSpan offset)`
- `public void SkipToFill()`
- `public void Stop()`

### Microsoft.Phone.Controls.TransitionElement (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/TransitionElement.cs`
- `public abstract ITransition GetTransition(UIElement element);`

### Microsoft.Phone.Controls.TransitionFrame (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/TransitionFrame.cs`
- `public TransitionFrame() : base()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.TransitionService (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/TransitionService.cs`
- `public static readonly DependencyProperty NavigationInTransitionProperty = DependencyProperty.RegisterAttached("NavigationInTransition", typeof(NavigationInTransition), typeof(TransitionService), null);`
- `public static readonly DependencyProperty NavigationOutTransitionProperty = DependencyProperty.RegisterAttached("NavigationOutTransition", typeof(NavigationOutTransition), typeof(TransitionService), null);`
- `public static NavigationInTransition GetNavigationInTransition(UIElement element)`
- `public static NavigationOutTransition GetNavigationOutTransition(UIElement element)`
- `public static void SetNavigationInTransition(UIElement element, NavigationInTransition value)`
- `public static void SetNavigationOutTransition(UIElement element, NavigationOutTransition value)`

### Microsoft.Phone.Controls.TurnstileFeatherEffect (class)
Source: `Microsoft.Phone.Controls.Toolkit/Effects/TurnstileFeatherEffect.cs`
- `public static IList<Type> NonPermittedTypes`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static int GetFeatheringIndex(DependencyObject obj)`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static void SetFeatheringIndex(DependencyObject obj, int value)`
- `public static readonly DependencyProperty FeatheringIndexProperty = DependencyProperty.RegisterAttached("FeatheringIndex", typeof(int), typeof(TurnstileFeatherEffect), new PropertyMetadata(-1, OnFeatheringIndexPropertyChanged));`

### Microsoft.Phone.Controls.TurnstileFeatherTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileFeatherTransition.cs`
- `public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(TurnstileFeatherTransitionMode), typeof(TurnstileFeatherTransition), null);`
- `public TurnstileFeatherTransitionMode Mode`
- `public static readonly DependencyProperty BeginTimeProperty = DependencyProperty.Register("BeginTime", typeof(TimeSpan?), typeof(TurnstileFeatherTransition), new PropertyMetadata(TimeSpan.Zero));`
- `public TimeSpan? BeginTime`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.TurnstileFeatherTransitionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileFeatherTransitionMode.cs`

### Microsoft.Phone.Controls.TurnstileTransition (class)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileTransition.cs`
- `public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(TurnstileTransitionMode), typeof(TurnstileTransition), null);`
- `public TurnstileTransitionMode Mode`
- `public override ITransition GetTransition(UIElement element)`

### Microsoft.Phone.Controls.TurnstileTransitionMode (enum)
Source: `Microsoft.Phone.Controls.Toolkit/Transitions/Transitions/TurnstileTransitionMode.cs`

### Microsoft.Phone.Controls.WeakReferenceHelper (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/WeakReferenceHelper.cs`
- `public static bool ContainsTarget(IEnumerable<WeakReference> references, object target)`
- `public static bool TryRemoveTarget(IList<WeakReference> references, object target)`
- `public static void RemoveNullTargetReferences(IList<WeakReference> references)`

### Microsoft.Phone.Controls.WrapPanel (class)
Source: `Microsoft.Phone.Controls.Toolkit/WrapPanel/WrapPanel.cs`
- `public double ItemHeight [TypeConverter(typeof(LengthConverter))] public double ItemHeight`
- `public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register( "ItemHeight", typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));`
- `public double ItemHeight #region public double ItemWidth [TypeConverter(typeof(LengthConverter))] public double ItemWidth`
- `public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register( "ItemWidth", typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));`
- `public double ItemWidth #region public Orientation Orientation public Orientation Orientation`
- `public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register( "Orientation", typeof(Orientation), typeof(WrapPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));`
- `public Orientation Orientation public WrapPanel()`

### Microsoft.Phone.Controls.LocalizedResources.ControlResources (class)
Source: `Microsoft.Phone.Controls.Toolkit.Localization/LocalizedResources/ControlResources.Designer.cs`
- `public static global::System.Resources.ResourceManager ResourceManager`
- `public static global::System.Globalization.CultureInfo Culture`
- `public static string AboutAMinuteAgo`
- `public static string AboutAMonthAgo`
- `public static string AboutAnHourAgo`
- `public static string AboutAWeekAgo`
- `public static string AlarmRepeatsOn`
- `public static string Byte`
- `public static string Cancel`
- `public static string DatePickerTitle`
- `public static string DateTimePickerCancelText`
- `public static string DateTimePickerDoneText`
- `public static string Gigabyte`
- `public static string Kilobyte`
- `public static string last_Friday`
- `public static string last_Monday`
- `public static string last_Saturday`
- `public static string last_Sunday`
- `public static string last_Thursday`
- `public static string last_Tuesday`
- `public static string last_Wednesday`
- `public static string Megabyte`
- `public static string No`
- `public static string Off`
- `public static string On`
- `public static string on_Friday`
- `public static string on_Monday`
- `public static string on_Saturday`
- `public static string on_Sunday`
- `public static string on_Thursday`
- `public static string on_Tuesday`
- `public static string on_Wednesday`
- `public static string OverAYearAgo`
- `public static string PartOfWhole`
- `public static string Pause`
- `public static string RepeatsEveryDay`
- `public static string RepeatsOnlyOnce`
- `public static string RepeatsOnWeekdays`
- `public static string RepeatsOnWeekends`
- `public static string Resume`
- `public static string StatusCancelled`
- `public static string StatusComplete`
- `public static string StatusDownloading`
- `public static string StatusFailed`
- `public static string StatusPaused`
- `public static string StatusPending`
- `public static string StatusUnknown`
- `public static string StatusUploading`
- `public static string StatusWaiting`
- `public static string StatusWaitingForExternalPower`
- `public static string StatusWaitingForExternalPowerDueToBatterySaverMode`
- `public static string StatusWaitingForNonVoiceBlockingNetwork`
- `public static string StatusWaitingForWiFi`
- `public static string TimePickerTitle`
- `public static string XHoursAgo_2To4`
- `public static string XHoursAgo_EndsIn1Not11`
- `public static string XHoursAgo_EndsIn2To4Not12To14`
- `public static string XHoursAgo_Other`
- `public static string XMinutesAgo_2To4`
- `public static string XMinutesAgo_EndsIn1Not11`
- `public static string XMinutesAgo_EndsIn2To4Not12To14`
- `public static string XMinutesAgo_Other`
- `public static string XMonthsAgo_2To4`
- `public static string XMonthsAgo_5To12`
- `public static string XSecondsAgo_2To4`
- `public static string XSecondsAgo_EndsIn1Not11`
- `public static string XSecondsAgo_EndsIn2To4Not12To14`
- `public static string XSecondsAgo_Other`
- `public static string XWeeksAgo_2To4`
- `public static string Yes`

### Microsoft.Phone.Controls.Primitives.ClipToBounds (class)
Source: `Microsoft.Phone.Controls.Toolkit/Primitives/ClipToBounds.cs`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static bool GetIsEnabled(DependencyObject obj)`
- `public methods", MessageId = "0", Justification = "Standard pattern.")] public static void SetIsEnabled(DependencyObject obj, bool value)`
- `public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ClipToBounds), new PropertyMetadata(false, OnIsEnabledPropertyChanged));`

### Microsoft.Phone.Controls.Primitives.DateTimePickerPageBase (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimePickerPageBase.cs`
- `public DateTime? Value`
- `public abstract void SetFlowDirection(FlowDirection flowDirection);`

### Microsoft.Phone.Controls.Primitives.DateTimeWrapper (class)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/DateTimeWrapper.cs`
- `public DateTime DateTime`
- `public string YearNumber`
- `public string MonthNumber`
- `public string MonthName`
- `public string DayNumber`
- `public string DayName`
- `public string HourNumber`
- `public string MinuteNumber`
- `public string AmPmString`
- `public DateTimeWrapper(DateTime dateTime)`
- `public static bool CurrentCultureUsesTwentyFourHourClock()`

### Microsoft.Phone.Controls.Primitives.IDateTimePickerPage (interface)
Source: `Microsoft.Phone.Controls.Toolkit/DateTimePickers/IDateTimePickerPage.cs`

### Microsoft.Phone.Controls.Primitives.ILoopingSelectorDataSource (interface)
Source: `Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelectorDataSource.cs`

### Microsoft.Phone.Controls.Primitives.LoopingSelector (class)
Source: `Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelector.cs`
- `public ILoopingSelectorDataSource DataSource`
- `public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ILoopingSelectorDataSource), typeof(LoopingSelector), new PropertyMetadata(null, OnDataSourceChanged));`
- `public DataTemplate ItemTemplate`
- `public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LoopingSelector), new PropertyMetadata(null, OnItemTemplateChanged));`
- `public Size ItemSize`
- `public Thickness ItemMargin`
- `public LoopingSelector()`
- `public bool IsExpanded`
- `public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(LoopingSelector), new PropertyMetadata(false, OnIsExpandedChanged));`
- `public event DependencyPropertyChangedEventHandler IsExpandedChanged;`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.Primitives.LoopingSelectorItem (class)
Source: `Microsoft.Phone.Controls.Toolkit/Primitives/LoopingSelectorItem.cs`
- `public LoopingSelectorItem()`
- `public event EventHandler<EventArgs> Click;`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Controls.Primitives.MenuBase (class)
Source: `Microsoft.Phone.Controls.Toolkit/ContextMenu/MenuBase.cs`
- `public Style ItemContainerStyle`
- `public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register( "ItemContainerStyle", typeof(Style), typeof(MenuBase), null);`

### Microsoft.Phone.Controls.Primitives.RelativeAnimatingContentControl (class)
Source: `Microsoft.Phone.Controls.Toolkit/ProgressBar/RelativeAnimatingContentControl.cs`
- `public RelativeAnimatingContentControl()`

### Microsoft.Phone.Controls.Primitives.TemplatedListBox (class)
Source: `Microsoft.Phone.Controls.Toolkit/LongListSelector/TemplatedListBox.cs`
- `public DataTemplate ListHeaderTemplate`
- `public DataTemplate ListFooterTemplate`
- `public DataTemplate GroupHeaderTemplate`
- `public DataTemplate GroupFooterTemplate`
- `public event EventHandler<LinkUnlinkEventArgs> Link;`
- `public event EventHandler<LinkUnlinkEventArgs> Unlink;`

### Microsoft.Phone.Controls.Primitives.ToggleSwitchButton (class)
Source: `Microsoft.Phone.Controls.Toolkit/Primitives/ToggleSwitchButton.cs`
- `public static readonly DependencyProperty SwitchForegroundProperty = DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitchButton), new PropertyMetadata(null));`
- `public Brush SwitchForeground`
- `public ToggleSwitchButton()`
- `public override void OnApplyTemplate()`

### Microsoft.Phone.Maps.Services.QueryExtensions (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/QueryExtensions.cs`
- `public static Task<IList<MapLocation>> GetMapLocationsAsync(this GeocodeQuery geocodeQuery)`
- `public static Task<IList<MapLocation>> GetMapLocationsAsync(this ReverseGeocodeQuery reverseGeocodeQuery)`
- `public static Task<Route> GetRouteAsync(this RouteQuery routeQuery)`

### Microsoft.Phone.Maps.Toolkit.MapChild (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChild.cs`
- `public static readonly DependencyProperty GeoCoordinateProperty = DependencyProperty.RegisterAttached( "GeoCoordinate", typeof(object), typeof(MapChild), null);`
- `public static readonly DependencyProperty PositionOriginProperty = DependencyProperty.RegisterAttached( "PositionOrigin", typeof(Point), typeof(MapChild), null);`
- `public static GeoCoordinate GetGeoCoordinate(DependencyObject element)`
- `public static void SetGeoCoordinate(DependencyObject element, GeoCoordinate value)`
- `public static Point GetPositionOrigin(DependencyObject element)`
- `public static void SetPositionOrigin(DependencyObject element, Point value)`

### Microsoft.Phone.Maps.Toolkit.MapChildCollection (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChildCollection.cs`
- `public MapChildCollection()`
- `public void Add(DependencyObject dependencyObject, GeoCoordinate geoCoordinate)`
- `public void Add(DependencyObject dependencyObject, GeoCoordinate geoCoordinate, Point positionOrigin)`

### Microsoft.Phone.Maps.Toolkit.MapChildControl (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapChildControl.cs`
- `public static readonly DependencyProperty GeoCoordinateProperty = DependencyProperty.Register( "GeoCoordinate", typeof(GeoCoordinate), typeof(MapChildControl), new PropertyMetadata(OnGeoCoordinateChangedCallback));`
- `public static readonly DependencyProperty PositionOriginProperty = DependencyProperty.Register( "PositionOrigin", typeof(Point), typeof(MapChildControl), new PropertyMetadata(OnPositionOriginChangedCallback));`
- `public GeoCoordinate GeoCoordinate`
- `public Point PositionOrigin`

### Microsoft.Phone.Maps.Toolkit.MapExtensions (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapExtensions.cs`
- `public static readonly DependencyProperty ChildrenProperty = DependencyProperty.RegisterAttached( "Children", typeof(ObservableCollection<DependencyObject>), typeof(MapExtensions), null);`
- `public static ObservableCollection<DependencyObject> GetChildren(Map element)`
- `public static void Add(this ObservableCollection<DependencyObject> childrenCollection, DependencyObject dependencyObject, GeoCoordinate geoCoordinate)`
- `public static void Add(this ObservableCollection<DependencyObject> childrenCollection, DependencyObject dependencyObject, GeoCoordinate geoCoordinate, Point positionOrigin)`

### Microsoft.Phone.Maps.Toolkit.MapItemsControl (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/MapItemsControl.cs`
- `public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof(IEnumerable), typeof(MapItemsControl), new PropertyMetadata(OnItemsSourceChanged));`
- `public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register( "ItemTemplate", typeof(DataTemplate), typeof(MapItemsControl), new PropertyMetadata(OnItemTemplateChanged));`
- `public static readonly DependencyProperty NameProperty = DependencyProperty.Register( "ItemTemplate", typeof(string), typeof(MapItemsControl), null);`
- `public MapItemsControl()`
- `public MapChildCollection Items`
- `public string Name`
- `public IEnumerable ItemsSource`
- `public DataTemplate ItemTemplate`

### Microsoft.Phone.Maps.Toolkit.Pushpin (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/PushPin.cs`
- `public Pushpin()`

### Microsoft.Phone.Maps.Toolkit.UserLocationMarker (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/UserLocationMarker.cs`
- `public UserLocationMarker()`

### System.Windows.HierarchicalDataTemplate (class)
Source: `Microsoft.Phone.Controls.Toolkit/Data/HierarchicalDataTemplate.cs`
- `public Binding ItemsSource`
- `public DataTemplate ItemTemplate private DataTemplate _itemTemplate;`
- `public DataTemplate ItemTemplate`
- `public DataTemplate ItemTemplate #region public Style ItemContainerStyle private Style _itemContainerStyle;`
- `public Style ItemContainerStyle`
- `public Style ItemContainerStyle public HierarchicalDataTemplate()`

### System.Windows.Controls.HeaderedItemsControl (class)
Source: `Microsoft.Phone.Controls.Toolkit/HeaderedItemsControl/HeaderedItemsControl.cs`
- `public object Header public object Header`
- `public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register( "Header", typeof(object), typeof(HeaderedItemsControl), new PropertyMetadata(OnHeaderPropertyChanged));`
- `public object Header #region public DataTemplate HeaderTemplate public DataTemplate HeaderTemplate`
- `public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register( "HeaderTemplate", typeof(DataTemplate), typeof(HeaderedItemsControl), new PropertyMetadata(OnHeaderTemplatePropertyChanged));`
- `public DataTemplate HeaderTemplate #region public Style ItemContainerStyle public Style ItemContainerStyle`
- `public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register( "ItemContainerStyle", typeof(Style), typeof(HeaderedItemsControl), new PropertyMetadata(null, OnItemContainerStylePropertyChanged));`
- `public Style ItemContainerStyle internal ItemsControlHelper ItemsControlHelper`
- `public HeaderedItemsControl()`
- `public override void OnApplyTemplate()`

### System.Windows.Controls.TemplatedVisualTreeExtensions (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/TemplatedVisualTreeExtensions.cs`

### System.Windows.Controls.TimeTypeConverter (class)
Source: `Microsoft.Phone.Controls.Toolkit/Common/TimeTypeConverter.cs`
- `public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)`
- `public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)`
- `public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)`
- `public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)`

### Windows.Devices.Geolocation.GeocoordinateExtensions (class)
Source: `Microsoft.Phone.Controls.Toolkit.WP8/Maps/GeocoordinateExtensions.cs`
- `public static GeoCoordinate ToGeoCoordinate(this Geocoordinate geocoordinate)`

## Verification summary

Verified against the current working tree on 2026-10-19:

| Evidence | Result |
|---|---|
| `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | 0 warnings, 0 errors |
| `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release --no-build` | 57 passed, 0 failed |
| `build\verify-package.ps1` | Package packed; fresh isolated consumer restored/built with 0 warnings and ran successfully |
| Sample process startup | Window remained alive after 5 s; no `%TEMP%\community-phone-toolkit-sample-error.txt` |
| Sample UI automation | Opened the detail page, verified the detail page loaded, returned to the home page, no startup error |
| Package contents | Main assembly, XML docs, 50 satellite resource assemblies, theme/BAML, README, LICENSE, third-party notices, docs |

The tests cover projection math and rendering, high-DPI rendering, projection animation, hit testing/focus, control templates/interaction, converters, navigation parameters/state/back cancellation/rapid navigation, entry+exit transitions, transition failure fallback, TiltEffect, TurnstileFeatherEffect, system tray/application service state, and package consumption. Visual parity and physical touch remain documented manual checks; see `known-limitations.md`.
