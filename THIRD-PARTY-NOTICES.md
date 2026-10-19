# Third-party notices

[中文版](THIRD-PARTY-NOTICES_zh-CN.md)

## Windows Phone Toolkit (MS-PL)

This project is a compatibility port of the archived Windows Phone Toolkit:

- Original repository: Microsoft Windows Phone Toolkit (archived)
- Original license: Microsoft Public License (MS-PL)
- Original copyright: Microsoft Corporation

The port retains the original source-file headers where code was copied or adapted. The full MS-PL text is in [LICENSE](LICENSE).

## Runtime dependencies

The `Community.Phone.Controls.Toolkit.WPF` package has **no NuGet package dependencies**. It uses only the WPF/.NET framework assemblies that ship with .NET (`PresentationCore`, `PresentationFramework`, `WindowsBase`, and the base class library). It does not depend on:

- Windows Phone SDK
- Silverlight
- `Microsoft.Phone` / `Microsoft.Phone.Controls.Toolkit`
- `System.Windows.Interactivity`

## Test/build-only dependencies

The automated test project uses xUnit and the .NET test SDK. These packages are **not** included in the NuGet package and are not required by consumers:

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`

Their respective licenses apply to test execution only.

## Fonts and icons

The theme references Metro-era system fonts (`Segoe UI`, `Segoe UI Light`) that are provided by Windows. No font files or third-party icon assets are redistributed in the package.
