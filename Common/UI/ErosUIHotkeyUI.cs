using ECommons.DalamudServices;
using ErosUI.Data;
using ErosUI.Helper;
using PromeRotation.UI.HotKey;

namespace ErosUI.UI;

// 热键面板 — 悬浮窗自绘（ErosUIHotkeyPanelWindow：圆角底/细描边/精确尺寸）。
// 面板条目完全由使用方经 ErosUIJobEnv.BuildHotkeys 注入（通用段/职业段均由 ACR 作者构建），
// 框架只负责窗口外壳、布局参数（每行数量/间距/缩放）、显隐与拖拽排序、设置页 Hotkey 显隐列表。
public static class ErosUIHotkeyUI
{
    private static ErosUIHotkeyPanelWindow? window;

    // (重)建热键面板：OnEnterAcr 与设置页布局/显隐改动后调用。全部隐藏时不注册空面板。
    public static void Rebuild()
    {
        Uninstall();

        var b = new ErosUIHotkeyBuilder(ErosUISettings.Instance);
        ErosUIJobEnv.BuildHotkeys?.Invoke(b);
        if (b.Entries.Count == 0) return;

        var s = ErosUISettings.Instance;
        window = new ErosUIHotkeyPanelWindow(
            b.Entries,
            s.HotkeyColumns,
            45f * s.HotkeyScalePercent / 100f,
            s.HotkeySpacing);
        try { PromeRotation.Plugin.Instance?.WindowSystem.AddWindow(window); }
        catch { /* 宿主未就绪 */ }
    }

    // 从宿主 WindowSystem 摘除热键面板（OnExitAcr 调用）。
    public static void Uninstall()
    {
        if (window == null) return;
        try { PromeRotation.Plugin.Instance?.WindowSystem.RemoveWindow(window); }
        catch (ArgumentException) { /* 窗口可能已被宿主移除 */ }
        window = null;
    }

    // 热键面板当前是否可见（未构建 = 不可见）。
    public static bool PanelVisible => window?.IsOpen ?? false;

    // 显示/隐藏热键面板（仅切显隐不重建；设置页「面板控制」用）。
    public static void SetPanelVisible(bool visible)
    {
        if (window == null) return;
        window.IsOpen = visible;
    }

}
