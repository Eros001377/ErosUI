# ErosUI

PromeRotation ACR 通用 UI 框架。从 ErosACR 的界面层剥离而来，提供设置窗口、QT 面板、热键面板、
战斗控制条与日间/夜间双主题，供 ACR 作者直接接入自己的轮转工程。

框架不含任何职业逻辑与预置按钮：QT 表、热键条目、设置页职业分节全部由使用方注入。

## 组成

| 模块 | 文件 | 说明 |
| --- | --- | --- |
| 窗口门面 | `Common/UI/ErosUIFramework.cs` | Install/Uninstall 注册全部窗口进宿主 WindowSystem |
| 设置窗口 | `Common/UI/SettingsWindowBase*.cs` | 侧边栏布局 + 末位「主题」页（夜间/日间），子类声明页签 |
| 设置页内容 | `Common/UI/ErosUISettingsUI.cs` | 基础设置 / Hotkey / QT面板 三页的绘制 |
| QT 悬浮面板 | `Common/UI/ErosUIQtPanelWindow.cs` | 网格布局、右键拖拽排序、显隐按模式分套 |
| 热键悬浮面板 | `Common/UI/ErosUIHotkeyPanelWindow.cs` + `ErosUIHotkeyUI.cs` | 图标/冷却/队列待发渲染，条目经构建器注入 |
| 战斗控制条 | `Common/UI/CombatControlWindow*.cs` | 运行/停手/主动攻击/设置入口，位置持久化 |
| 主题与色板 | `Common/UI/SimplePalette.cs` 等 | 日间/夜间双主题、通用主题主色（主题页调色） |
| 配置持久化 | `Common/Data/ErosUISettings.cs` / `ErosUICommonSettings.cs` | 按职业分档 JSON + 通用 Common.json |
| 注入接口 | `Common/Data/ErosUIJobEnv.cs` | 使用方与框架的唯一耦合面 |

## 接入方式

1. 引用本工程（项目引用或编译产物 `ErosUI.dll`），宿主 SDK 仍按使用方工程的引用提供。

2. 在你的 Rotation 构造函数最前面注入职业环境：

```csharp
using ErosUI.Data;
using ErosUI.Helper;
using ErosUI.UI;

ErosUIJobEnv.Configure(
    jobTag: "SAM",
    jobName: "武士",
    qtAll: MyQT.All,                       // 键 → 默认值
    qtIsMetaKey: MyQT.IsMetaKey,           // 元键（不进面板）
    qtIsVisibleInMode: MyQT.IsVisibleInMode, // 高难/日随可见性
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
    author: "你的作者名");   // 见下节「作者身份」
```

3. 在宿主生命周期回调里成对安装/卸载：

```csharp
public void OnEnterAcr()
{
    ErosUIFramework.Install();       // 注册全部窗口
    ErosUIHotkeyUI.Rebuild();      // 构建热键面板
    APIHelper.重建QT可见性();     // 按显隐配置注册 QT
}

public void OnExitAcr()
{
    ErosUIFramework.Uninstall();
    ErosUIHotkeyUI.Uninstall();
}
```

聊天命令注册（`/{你的ACR名}`）属于使用方个性化内容，不在框架内——官方模板
[PromeRotation/RotationTemplate](https://github.com/PromeRotation/RotationTemplate) 的 MacroManager
即为现成实现，按模板内 TODO 改命令名后随你的工程使用。

## 作者身份（author）

框架内所有对外可见的「自称」统一收敛为 `ErosUIJobEnv.作者` 一个字段，经 `Configure` 的 `author`
参数注入，**默认占位值为 `author`，使用方必须改成自己的作者名**。它同时决定：

- **配置目录**：宿主约定 `pluginConfigs\PromeRotation\Settings\ACRConfig\<作者>\`——因此必须与
  你 `RotationMetadata` 的第三参数（Author）一致，否则配置目录对不上
- **窗口标题前缀**：控制条 / 设置窗 / QT 面板 / 热键面板的标题以 `{作者}` 开头（同时是宿主
  WindowSystem 的窗口名，改名会丢对应窗口的位置记忆）
- **日志前缀**：框架日志行统一为 `[<作者>] ...`

## 接入示例

`examples/ErosUIDemo` 是一个最小可用的武士演示 ACR（空轮转、完整 UI 接入），
可直接编译部署到游戏目录验证框架流程，本 README 的接入代码即出自它。

## 配置落盘位置

- 通用（主题等）：`pluginConfigs\PromeRotation\Settings\ACRConfig\<作者>\Common.json`
- 按职业：同目录 `{JobTag}.json`（如 `SAM.json`）
- 首次使用落内嵌出厂配置 `Resources\DefaultCommon.json`；职业分档默认配置由使用方按
  `Default{JobTag}.json` 自备嵌入资源（缺省走代码默认值）

## 剥离记录

本工程由 ErosACR 剥离而来：`ErosUIJobEnv` 收敛为纯注入接口。

## 许可证

MIT，见 [LICENSE](LICENSE)。
