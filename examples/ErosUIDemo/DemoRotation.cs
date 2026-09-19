using ErosUI.Data;
using ErosUI.Helper;
using ErosUI.UI;
using ECommons.ExcelServices;
using PromeRotation.Data;
using PromeRotation.Rotation;

namespace ErosUIDemo;

// ErosUI 框架接入演示：最小可用的武士 ACR——无任何轮转逻辑,
// 只验证 Configure 注入 + 窗口生命周期 + QT/热键面板的完整接入流程。
// 重要: IRotationLifecycle 必须实现在本类上（宿主只检查 IRotation 实例本身,
// 挂在 EventHandler 上宿主永远不会调用）。
[RotationMetadata((uint)Job.SAM, "ErosUI 演示 武士", "ErosUIDemo", "1.0.0.0")]
public class DemoRotation : IRotation, IRotationLifecycle
{
    public string RotationName => "ErosUIDemo";
    public uint JobId => (uint)Job.SAM;

    private readonly DemoEventHandler eventHandler = new();
    public IRotationEventHandler GetEventHandler() => eventHandler;

    // QT 表（唯一数据源; 宿主经此注册全部开关）
    public static IReadOnlyDictionary<string, bool> QtList => QT.All;

    public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>();

    // 演示 QT 表：三个开关, 「高难专用」演示按模式显隐
    internal static class QT
    {
        public static readonly Dictionary<string, bool> All = new()
        {
            ["演示开关A"] = true,
            ["演示开关B"] = false,
            ["高难专用"] = true,
        };

        public static bool IsVisibleInMode(string key, bool highEnd) => key != "高难专用" || highEnd;
    }

    public DemoRotation()
    {
        // 框架注入：职业身份 + QT 数据源 + 热键构建 + 作者身份（详见 ErosUI README）
        ErosUIJobEnv.Configure(
            jobTag: "SAM",
            jobName: "武士",
            qtAll: QT.All,
            qtIsMetaKey: _ => false,
            qtIsVisibleInMode: QT.IsVisibleInMode,
            qtDefault: k => QT.All.GetValueOrDefault(k),
            qtCascadeRules: new Dictionary<string, (string, bool)[]>(),
            hotkeyNames: ["疾跑"],
            buildHotkeys: b => b.Fixed("疾跑", 7571u, ActionType.OffGcd, ActionTargetType.Self),
            rebuildHotkeys: ErosUIHotkeyUI.Rebuild,
            author: "ErosUIDemo");
    }

    // === 轮转出口（演示全空） ===
    public IOpener? GetOpener() => null;
    public PAction? NextAlways() => null;
    public PAction? NextGcd() => null;
    public PAction? NextOffGcd() => null;
    public void UpdateDebugStatus() { }

    // === PR 本体 UI 委托：设置页入口给一个打开框架设置窗的按钮 ===
    public void DrawSettings() => ErosUIFramework.DrawSettingsEntry();
    public void DrawQTs() { }

    // === 生命周期：框架装卸与设置落盘严格成对 ===
    public void OnEnterAcr()
    {
        ErosUIFramework.Install();       // 注册全部窗口
        ErosUIHotkeyUI.Rebuild();     // 构建热键面板
        APIHelper.重建QT可见性();      // 按显隐配置注册 QT
    }

    public void OnExitAcr()
    {
        ErosUIHotkeyUI.Uninstall();
        ErosUIFramework.Uninstall();
        ErosUISettings.Instance.Save();
        ErosUICommonSettings.Instance.Save();
    }
}

// 事件回调全空：演示不包含战斗逻辑
public class DemoEventHandler : IRotationEventHandler
{
    public void OnUpdate() { }
    public void OnOutOfBattleUpdate() { }
    public void OnBattleStarted() { }
    public void OnBattleUpdate() { }
    public void OnBattleEnded() { }
    public void OnTerritoryChanged(ushort territoryId) { }
    public void OnNoTarget() { }
}
