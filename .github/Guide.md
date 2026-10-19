# 项目任务：将 Windows Phone Toolkit 完整移植为 WPF 兼容版 Community.Phone.Controls.Toolkit.WPF

## 一、项目目标

请将微软归档的 Windows Phone Toolkit 仓库移植为面向现代 WPF 的兼容实现，最终交付一个可以通过 NuGet 安装的包：

**NuGet Package ID：`Community.Phone.Controls.Toolkit.WPF`**

原始仓库：

D:\repos\repo-other\WindowsPhoneToolkit

开始实现之前，必须先完整检查仓库的实际内容，包括项目文件、源码、XAML、资源、样例、依赖和许可证，不得只根据 README 或文件名推断实现细节。

项目最终希望达到以下使用体验：

1. WPF 项目只需添加 `Community.Phone.Controls.Toolkit.WPF` 一个 NuGet 包。
2. 可以使用移植后的 Windows Phone Toolkit 控件、效果、导航转场和配套资源。
3. 尽可能保留原始 Windows Phone Toolkit 的公共 API、命名空间、属性名称、事件、依赖属性、命令、模板和 XAML 使用方式。
4. 对 WPF 中不存在或行为不同的 Windows Phone 平台基础设施，提供配套的 WPF 兼容实现，而不是简单删除相关代码或将其替换成外观相似的 WPF API。
5. 尽可能复刻 Windows Phone 的 Metro UI、触控反馈、TiltEffect、TurnstileFeatherEffect、3D Projection 和页面导航转场。
6. 对无法完全兼容的 API 明确说明差异、原因和推荐替代方式，不得声称尚未实现的功能已经兼容。

这不是简单的控件移植任务，而是一个 Windows Phone XAML API 到 WPF 的兼容性工程。

## 二、技术环境与总体约束

- 开发语言：C#。
- UI 框架：WPF。
- 优先使用现代 .NET Windows 桌面技术，默认以 `net10.0-windows` 作为目标框架；如有充分理由需要兼容其他 .NET 版本，请设计合理的多目标框架方案。
- 采用 SDK-style `.csproj`。
- 使用 WPF 原生依赖属性、路由事件、命令、数据绑定、模板和资源机制。
- 不依赖 Windows Phone SDK、Silverlight 运行时或已经废弃的 WP 开发工具。
- 不要求用户额外安装 Microsoft.Phone.dll。
- 所有必要的控件、样式、模板、资源、导航和转场实现都必须随 NuGet 包交付。
- 优先使用 WPF 原生实现；只有在原生 WPF 无法满足效果要求时，才考虑 Win32、D3D、Windows Composition 或其他底层技术。
- 禁止使用截图替代整个交互式控件树作为默认 3D 投影实现。
- 不要为了追求表面上的 API 兼容而破坏 WPF 原生控件的行为。
- 不得修改或重新定义 WPF 框架自身的 `System.Windows` 类型。
- 不得直接删除无法移植的功能来获得一个表面上可以编译的项目。
- 所有实现必须有真实代码、构建验证和测试证据。

## 三、第一阶段：完整审计原始仓库

首先检查整个仓库，至少包括：

- `Microsoft.Phone.Controls.Toolkit`
- `Microsoft.Phone.Controls.Toolkit.WP7`
- `Microsoft.Phone.Controls.Toolkit.WP8`
- `Microsoft.Phone.Controls.Toolkit.Localization`
- `PhoneToolkitSample`
- `PhoneToolkitSample8`
- 原始解决方案、所有项目文件、资源文件、主题、XAML、代码生成文件和测试相关文件。

必须实际阅读源代码，而不是仅仅列出文件。

### 3.1 建立完整的功能清单

逐项梳理以下功能，实际存在的其他功能也必须纳入清单：

- AutoCompleteBox
- ContextMenu
- CustomMessageBox
- DateTimeConverters
- DateTimePickers
- SlideInEffect
- TiltEffect
- TurnstileFeatherEffect
- ExpanderView
- HubTile
- ListPicker
- LongListMultiSelector
- Map extensions
- PhoneTextBox
- RatingControl
- ToggleSwitch
- TransferControl
- Navigation transitions
- WrapPanel
- WP7 相关的 LongListSelector、MultiSelect 等功能
- 所有其他公开控件、辅助类、扩展方法、枚举、转换器和行为。

对每个功能记录：

1. 原始命名空间、类型名称和继承关系。
2. 公共构造函数、属性、依赖属性、附加属性、事件、命令及方法。
3. 对外公开的 XAML 使用方式。
4. 使用的 WP/Silverlight 基础设施。
5. 使用的内部 Toolkit 类和其他组件。
6. 对主题、模板、系统资源、导航、输入、动画和生命周期的依赖。
7. WPF 中的可替代实现及行为差异。
8. 移植优先级、实现状态和测试要求。

产出 `docs/porting-inventory.md`，并建立一份可以持续更新的 API 兼容性清单。

不能只记录 README 中列出的组件，必须以实际源代码和公共 API 为准。

### 3.2 检查源代码和依赖关系

重点搜索并分析：

- 所有 `using Microsoft.Phone.*`
- 所有 `using System.Windows.*`
- 所有 Silverlight 特有类型和 API
- 所有平台相关的导航和页面生命周期调用
- 所有主题资源、静态资源、动态资源及隐式样式
- 所有动画 Storyboard 和 `TargetProperty` 路径
- 所有附加属性和行为
- 所有对 `Microsoft.Phone.dll`、系统主题和 PhoneApplicationFrame 的依赖。

建立依赖图，区分公共 API、内部实现和平台基础设施。

任何无法确定语义的 API，都必须查阅对应源码或可靠的 Microsoft 文档；不要根据名称猜测行为。

## 四、第二阶段：设计兼容层，而不是直接替换类型

为 WPF 建立一套完整的 Phone 兼容基础设施。具体命名空间应根据原始程序集的实际 API 进行设计，尽可能保留原始命名空间和类型名称。

建议将实现拆分为以下逻辑模块；可以在开发阶段使用多个项目，但最终应让用户只需安装一个主 NuGet 包即可获得全部必需功能。

### 4.1 Phone UI 基础设施

审计并按需实现：

- PhoneApplicationPage
- PhoneApplicationFrame
- 页面导航服务与导航事件
- 页面生命周期与导航状态
- 后退导航
- 导航参数与页面状态
- 导航转场
- PhoneApplicationService 对应的应用生命周期服务
- SystemTray 对应的系统状态栏兼容实现
- PhoneApplicationPage 的方向、可见区域及页面布局相关 API
- Phone 主题、系统字体、颜色和样式资源
- 原始 API 中实际存在的其他平台服务。

以上是需要审计的候选功能，不代表所有 API 都必须照搬。最终实现范围必须以源码及使用场景为依据。

### 4.2 重点处理 Frame 和 Navigation

WPF 已经存在 `System.Windows.Controls.Frame`，而 Windows Phone 的 `Microsoft.Phone.Controls.PhoneApplicationFrame` 有自己的页面导航、生命周期和转场行为。

必须明确区分：

- WPF 原生 Frame。
- WP PhoneApplicationFrame。
- Toolkit 中的 TransitionFrame 或其他导航转场类型。
- 页面实例管理、导航历史、返回行为和页面状态。

不要将 WP 的 PhoneApplicationFrame 简单映射成 WPF Frame 后就认为兼容完成。

请设计自己的导航兼容层，在尽量保留 WP 公共 API 的前提下，使用 WPF 原生控件与导航机制实现相应行为。

如果需要自定义 Frame、Page 或 NavigationService，应明确说明其命名空间和使用方式，并尽量让现有 WP 示例只需修改少量 XAML 命名空间或启动代码即可运行。

### 4.3 Projection / PlaneProjection：最高优先级基础设施

WP 的 `UIElement.Projection` 是整个 UI 的 3D 透视和部分转场效果的重要基础设施。WPF 原生 `UIElement` 没有对应的 `Projection` 属性。

必须设计一个真正的 WPF 投影兼容层，包括：

- `Projection` 基类。
- `PlaneProjection`。
- 如原始平台 API 或移植效果确实需要，进一步评估 `Matrix3DProjection` 或等价的矩阵投影能力。
- `RotationX`、`RotationY`、`RotationZ`。
- `CenterOfRotationX`、`CenterOfRotationY`、`CenterOfRotationZ`。
- `GlobalOffsetX`、`GlobalOffsetY`、`GlobalOffsetZ`。
- `LocalOffsetX`、`LocalOffsetY`、`LocalOffsetZ`。
- 其他经源码确认的投影属性和行为。
- 属性的依赖属性、动画、数据绑定和资源兼容。
- 对象尺寸变化时的旋转中心计算。
- 透视、坐标系、旋转顺序、偏移、裁剪、深度和背面显示。
- Turnstile、TurnstileFeatherEffect 以及其他依赖投影的效果。

请优先研究原始平台 API 的实际语义，再实现 WPF 渲染器。

评估并比较以下实现路线：

1. WPF 原生 3D 与 `Viewport2DVisual3D`。
2. WPF Visual 到 3D 平面的自定义渲染方案。
3. Windows Composition 或 D3D 等更底层的方案。

选择时必须考虑：

- 透视准确性。
- 任意 UIElement 和复杂控件的支持。
- 动画性能。
- 鼠标命中测试。
- 键盘焦点和文本输入。
- 鼠标捕获、拖放、滚轮和触控。
- 弹出窗口、ContextMenu、ToolTip 和 Popup。
- 渲染缓存、DPI 和窗口缩放。
- 多个投影对象的组合。
- 内存占用和资源释放。

不要默认采用截图或 VisualBrush 静态图像作为交互式 3D 控件的完整解决方案。

#### 必须明确 XAML 兼容边界

原始 WP XAML 可能写成：

```xml
<Grid.Projection>
    <PlaneProjection RotationY="-35" />
</Grid.Projection>
```

由于 WPF 原生 Grid 不存在 `Projection` 属性，不能仅通过 C# 扩展属性或附加属性，就让上述属性元素语法自动生效。

请明确区分：

1. 能够原样编译的 XAML。
2. 仅需修改命名空间或少量属性写法的 XAML。
3. 必须将原生控件替换为兼容包装控件的 XAML。
4. 无法在 WPF 中完全兼容的 API。

优先探索可维护的兼容方案，但不得修改 WPF 框架本身或虚构不存在的 C# 能力。

例如，必要时可提供：

```xml
<phone:ProjectionHost>
    <Grid>
        <!-- 原有 UI 内容 -->
    </Grid>
</phone:ProjectionHost>
```

或：

```xml
<Grid
    phone:ProjectionService.Projection="{StaticResource MyProjection}">
    <!-- 原有 UI 内容 -->
</Grid>
```

这些只是兼容方案示意，最终 API 设计应结合原始源码、WPF XAML 编译规则和实际动画需求确定。

目标是尽量降低迁移成本，而不是在文档中宣称无法实现的原样兼容。

## 五、第三阶段：迁移 Toolkit 控件与效果

对原始仓库中的每一个控件和效果逐项移植，尽量保留：

- 类型名称与命名空间。
- 依赖属性及其默认值。
- 只读依赖属性。
- 附加属性。
- 路由事件及事件参数。
- 命令与命令参数。
- 控件模板部件名称。
- TemplatePart 和 TemplateVisualState 契约。
- 数据绑定行为。
- 控件状态及状态切换。
- 主题资源和默认样式。
- 动画属性路径。
- 原始控件的公开方法与事件顺序。

WPF 与 Silverlight 的基类和 API 如果不同，应通过适配层实现原有语义，而不是盲目修改公共接口。

对于每个控件，至少完成：

1. 源码移植。
2. 编译。
3. 默认样式和模板。
4. 依赖属性、绑定和事件测试。
5. 基本交互测试。
6. 原始示例迁移或等价的 WPF 示例。
7. 已知差异文档。

如果某个控件依赖地图服务、系统弹窗或无法在 WPF 中直接提供的系统能力，应设计合理的接口与替代实现，并清楚标注限制。

## 六、第四阶段：完整迁移导航转场与 Turnstile

导航转场与投影属于核心功能，不得仅实现一个简单的 RotateTransform 就宣称移植成功。

重点研究：

- `TransitionFrame`
- `ITransition`
- 原始转场基类和转场管理逻辑
- 页面切换的前后内容管理
- 导航方向
- 页面离场与进入时序
- BitmapCache 等性能优化
- Slide、Rotate、Turnstile 等实际存在的转场
- `TurnstileFeatherEffect`
- `TiltEffect`
- 其他依赖投影、输入事件和页面状态的效果。

具体类型和功能以原始仓库为准，不要遗漏未在 README 中列出的实现。

要求：

- 正确处理导航开始、完成、取消和异常。
- 不重复执行或泄漏动画。
- 动画结束后恢复正确的输入状态。
- 正确释放旧页面及临时视觉资源。
- 支持窗口尺寸变化和 DPI 缩放。
- 保持尽可能接近 WP 的运动方向、透视、缓动、时序和触控反馈。

对 Turnstile，建立可重复的视觉对照测试场景。不要仅凭主观感觉确定动画效果已经还原。

## 七、第五阶段：命名空间与公共 API 策略

兼容性是本项目的核心目标，因此必须在实现前建立命名空间和 API 策略。

优先原则：

1. 原始 Toolkit 公共类型的命名空间和类型名尽可能保持不变。
2. 对应 WP 平台基础设施的兼容类型尽量保持原有名称。
3. 对必须使用不同命名空间的 WPF 适配实现，建立清晰一致的命名约定。
4. 避免与 WPF 的 `System.Windows.Controls.Frame`、`Page`、`ContextMenu` 等类型产生歧义。
5. 不要试图重新定义 `System.Windows.UIElement`、`System.Windows.Controls.Frame` 等 WPF 框架类型。
6. 在确实无法保持原始 API 时，提供迁移指南和兼容别名或包装类型。

建立 API 映射表，逐项记录：

- WP 原始类型或成员。
- 原始程序集及命名空间。
- WPF 对应类型。
- 最终兼容类型。
- 兼容级别。
- 是否需要修改 XAML。
- 是否需要修改 C# 代码。
- 是否存在行为差异。

兼容级别至少包括：

- 完全兼容。
- API 兼容但行为有差异。
- 需要少量迁移。
- 需要包装控件或替代实现。
- 不支持。

不要将名称相同误认为行为相同。

## 八、第六阶段：单一 NuGet 包交付

最终用户只需执行：

```powershell
dotnet add package Community.Phone.Controls.Toolkit.WPF
```

便能使用所有已交付的 Toolkit 控件、投影、主题、转场、导航兼容层及其必要依赖。

请完成：

- NuGet 包元数据。
- SDK-style 项目文件。
- 正确的依赖关系。
- 所有必要的程序集和资源。
- WPF 默认样式及 `Themes/Generic.xaml`。
- ResourceDictionary 与主题资源。
- 本地化资源。
- 必要的设计时资源。
- XML 文档注释。
- Source Link 或相应源码调试配置（如适用）。
- 包版本管理。
- LICENSE 和第三方许可说明。
- README、API 兼容矩阵及迁移文档。
- NuGet 打包、安装与消费测试。

内部可以采用多个程序集，但最终必须确保用户不需要逐个安装内部包。

如果拆分为多个程序集，必须验证它们都被正确纳入主包依赖图。不能只把主程序集发布到 NuGet，而遗漏主题、资源或基础设施程序集。

同时验证安装包的项目是否能直接运行，不依赖开发机中的额外文件或本地绝对路径。

## 九、第七阶段：测试体系

创建独立的测试项目和完整的示例应用。

至少包括以下测试：

### 9.1 构建与包测试

- 整个解决方案可以构建。
- NuGet 包可以生成。
- 使用全新 WPF 项目安装包后可以构建和运行。
- 不需要引用 WP SDK 或 Microsoft.Phone.dll。
- 所有必要资源能在运行时加载。

### 9.2 API 测试

- 公共类型及命名空间。
- 依赖属性注册与默认值。
- 依赖属性读写、绑定与动画。
- 事件和命令。
- 模板部件与视觉状态。
- 资源与样式查找。

### 9.3 Projection 测试

- 三个旋转轴。
- 旋转中心。
- GlobalOffset 和 LocalOffset。
- 不同控件尺寸。
- 透视与裁剪。
- 动画中的连续变化。
- 控件尺寸改变和 DPI 缩放。
- 多个投影对象。
- 输入命中测试和键盘焦点。

### 9.4 导航和转场测试

- 前进导航。
- 后退导航。
- 导航参数。
- 页面生命周期。
- 页面状态恢复。
- 转场开始与结束。
- 连续导航和快速重复触发。
- 导航期间的异常和资源释放。

### 9.5 控件交互测试

- TextBox 输入。
- Button 点击。
- ListPicker 选择。
- ContextMenu。
- Popup 与 ToolTip。
- 滚轮、鼠标捕获和键盘焦点。
- 数据绑定与命令执行。
- 控件模板替换。
- 主题切换。

无法自动化的视觉和交互测试应提供手动测试步骤及明确的预期结果。

## 十、第八阶段：持续集成和质量要求

建立可以重复运行的构建与测试流程。

推荐命令：

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
```

以上命令必须根据实际解决方案结构调整，不能为了符合示例而省略真实项目。

每完成一个功能模块，都应先构建、测试并更新兼容性清单，再继续下一模块。

不要一次性生成大量未经编译验证的代码。

不要使用空实现、返回默认值、吞掉异常或禁用测试来制造成功结果。

对于暂时无法完成的功能，应明确标注为未实现，并说明剩余工作。

## 十一、开发工作方式

请按以下顺序执行：

### Phase 0：仓库审计

- 检查仓库状态、分支和许可证。
- 阅读解决方案、全部项目文件和核心源码。
- 生成完整功能清单、依赖图和 API 兼容性矩阵。
- 确认原始项目的构建条件及无法直接复用的依赖。
- 输出架构方案和分阶段实施计划。

### Phase 1：最小可用兼容基础设施

优先完成：

- WPF 项目骨架。
- Phone 兼容命名空间。
- 基础页面与导航模型。
- 主题资源。
- Projection / PlaneProjection。
- 一个可运行的投影动画示例。
- 一个可运行的页面转场示例。
- NuGet 包的基本结构。

这一阶段必须能够构建并运行。

### Phase 2：Toolkit 控件迁移

按依赖关系从基础设施到具体控件逐步迁移。

每个控件完成后都应有编译验证和基本测试。

### Phase 3：完整导航与效果

完善页面导航、Turnstile、TiltEffect、其他转场和相关性能优化。

### Phase 4：单包集成

整合所有组件、样式、资源和必要依赖，验证只安装主 NuGet 包即可运行。

### Phase 5：兼容性与质量验收

运行完整测试，修复问题，更新文档，并给出实际完成情况。

在每个阶段结束时，汇报：

- 已阅读的源码范围。
- 已实现的功能。
- 实际构建和测试结果。
- 未完成的功能。
- 已知兼容差异。
- 下一阶段计划。

不要在尚未完成某个阶段时声称整个项目已经移植完成。

## 十二、交付物

最终交付：

1. 可构建的完整 WPF 解决方案。
2. `Community.Phone.Controls.Toolkit.WPF` NuGet 包。
3. 所有已移植的控件、效果、基础设施和资源。
4. 可运行的 WPF 示例应用。
5. 自动化测试项目。
6. `docs/porting-inventory.md`：原始功能清单。
7. `docs/api-compatibility.md`：API 兼容矩阵。
8. `docs/architecture.md`：架构设计。
9. `docs/migration-guide.md`：WP 到 WPF 的迁移指南。
10. `docs/known-limitations.md`：未实现功能和行为差异。
11. README、构建说明和 NuGet 使用示例。
12. 许可证及第三方依赖说明。

## 十三、验收标准

最终以真实结果为准，不以文档描述代替实现。

必须能够证明：

- 单一 NuGet 包可安装。
- 全新 WPF 项目可以使用包中的控件和资源。
- 已实现的公共 API 与兼容性矩阵一致。
- Projection 属性变化能驱动真实的 3D 视觉效果。
- Turnstile 等已声明支持的动画能够实际运行。
- 导航和转场具有可测试的生命周期与资源管理行为。
- 控件的基本交互没有被静态截图替代。
- 所有公开声明支持的功能都有构建、测试或明确的手动验证证据。
- 所有未完成或不兼容的部分均有文档说明。

