# ErosUI

基于 PromeRotation 的 ACR 通用 UI 框架。接入后你的 ACR 直接获得一套完整的界面：
设置窗口、QT 面板、热键面板、战斗控制条，以及日间/夜间双主题。

框架本身不包含任何职业逻辑。QT 开关表、热键按钮等内容全部由使用方注入，
框架不预置任何按钮或技能，接入什么就显示什么。

## 快速开始

1. 引用本工程（项目引用，或编译产物 `ErosUI.dll`）
2. 在 Rotation 构造函数最前面注入职业环境
3. 在生命周期回调里成对安装/卸载

```csharp
using ErosUI.Data;
using ErosUI.Helper;
using ErosUI.UI;

// 2. 注入职业环境
ErosUIJobEnv.Configure(
    jobTag: "SAM",
    jobName: "武士",
    qtAll: MyQT.All,                       // QT 开关表：键 → 默认值
    qtIsMetaKey: MyQT.IsMetaKey,           // 元键（不进面板）
    qtIsVisibleInMode: MyQT.IsVisibleInMode, // 高难/日随各自的可见性
    qtDefault: MyQT.Default,
    qtCascadeRules: MyQT.CascadeRules,     // QT 联动表
    hotkeyNames: [.. MyJobNames, .. CommonNames],
    buildHotkeys: b =>
    {
        b.Fixed("疾跑", 7571u, ActionType.OffGcd, ActionTargetType.Self);
        b.Execute("爆发药", new ExecuteLogic(() => { /* ... */ }));
    },
    rebuildHotkeys: ErosUIHotkeyUI.Rebuild,
    qtTab基础: [ ("aoe", "AOE 模式", 0u) ],
    author: "你的作者名");   // 见下文「作者身份」
```

```csharp
// 3. 生命周期：严格成对调用
public void OnEnterAcr()
{
    ErosUIFramework.Install();       // 注册全部窗口
    ErosUIHotkeyUI.Rebuild();        // 构建热键面板
    APIHelper.重建QT可见性();         // 按显隐配置注册 QT
}

public void OnExitAcr()
{
    ErosUIHotkeyUI.Uninstall();
    ErosUIFramework.Uninstall();
    ErosUISettings.Instance.Save();
    ErosUICommonSettings.Instance.Save();
}
```

## 模块组成

| 组件 | 说明 |
|------|------|
| `ErosUIFramework` | 框架门面：Install/Uninstall 注册全部窗口，面板开关入口 |
| `ErosUIJobEnv` | 注入接口：使用方与框架的唯一耦合面 |
| `Common/UI/SettingsWindowBase*` | 设置窗口（侧边栏页签布局 + 主题页） |
| `Common/UI/ErosUISettingsUI.cs` | 设置页内容（基础设置 / Hotkey / QT面板 三页） |
| `Common/UI/ErosUIQtPanelWindow.cs` | QT 悬浮面板（网格布局、拖拽排序、按模式记忆显隐） |
| `Common/UI/ErosUIHotkeyPanelWindow.cs` | 热键悬浮面板（图标、冷却、队列待发提示） |
| `Common/UI/CombatControlWindow*.cs` | 战斗控制条（运行/停手/主动攻击/设置入口） |
| `Common/UI/SimplePalette.cs` | 日间/夜间双主题色板，主题页可自定义主色 |
| `Common/Data/` | 配置持久化（通用 + 按职业分档的 JSON） |
| `Common/Helper/ErosUIHotkeyBuilder.cs` | 热键面板条目构建器 |
| `APIHelper` | QT 联动写入与可见性重建 |

## 依赖

- .NET 10.0 Windows
- [PromeRotation](https://github.com/kanyeishere/PRACR)（编译期走 `PromeRotation.SDK.CNGL` NuGet 包）
- Dalamud / ECommons / Lumina（运行时由游戏环境随宿主提供）

## 目录结构

```
ErosUI/
├── ErosUI.csproj
├── Common/
│   ├── APIHelper.cs
│   ├── Data/            （注入接口、设置持久化、主题模式）
│   ├── Helper/          （热键构建器）
│   └── UI/              （全部窗口与控件）
└── Resources/
    └── DefaultCommon.json   （首次使用的出厂配置）
```

## 作者身份（author）

框架内所有对外可见的「自称」统一为一个作者名字段，经 `Configure` 的 `author`
参数注入，**默认占位值为 `author`，使用方必须改成自己的作者名**。它决定：

- **配置目录**：`pluginConfigs\PromeRotation\Settings\ACRConfig\<作者>\`——因此必须与
  你 `RotationMetadata` 的 Author 参数一致，否则配置目录对不上
- **窗口标题前缀**：各窗口标题以 `{作者}` 开头（窗口名同时是宿主 WindowSystem 的
  键，改名会丢对应窗口的位置记忆）
- **日志前缀**：框架日志行统一为 `[<作者>] ...`

## 配置落盘位置

- 通用（主题等）：`pluginConfigs\PromeRotation\Settings\ACRConfig\<作者>\Common.json`
- 按职业：同目录 `{JobTag}.json`（如 `SAM.json`）
- 首次使用自动落出厂配置；职业分档默认配置由使用方按 `Default{JobTag}.json`
  自备嵌入资源（缺省走代码默认值）

## 许可证

MIT，见 [LICENSE](LICENSE)。
