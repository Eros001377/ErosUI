using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ErosUI.Data;

namespace ErosUI.UI;

// ErosUI 门面：窗口注册进宿主的 WindowSystem（PR 统一绘制与生命周期托管），
// Install/Uninstall 严格成对放在 OnEnterAcr/OnExitAcr。
// PR 本体面板隐藏：HidePrPanels（UI 线程 1 秒节流，控制条 PreDraw 触发）
public static class ErosUIFramework
{
    private static CombatControlWindow? control;
    private static ErosUISettingsWindow? settings;
    private static ErosUIQtPanelWindow? qtPanel;

    // 注册全部窗口到宿主 WindowSystem（幂等：先卸再装）
    public static void Install()
    {
        Uninstall();
        var ws = PromeRotation.Plugin.Instance?.WindowSystem;
        if (ws == null)
        {
            Svc.Log.Info($"[{ErosUIJobEnv.作者}] 无法访问宿主 WindowSystem，UI 未注册");
            return;
        }

        control = new CombatControlWindow(
            $"{ErosUIJobEnv.作者}{ErosUIJobEnv.JobTag} 控制",
            ToggleSettings,
            () => 限制存档位置(ErosUISettings.Instance.控制条位置),
            pos =>
            {
                ErosUISettings.Instance.控制条位置 = pos;
                ErosUISettings.Instance.Save();
            });
        settings = new ErosUISettingsWindow();
        ws.AddWindow(control);
        ws.AddWindow(settings);
        qtPanel = new ErosUIQtPanelWindow();
        ws.AddWindow(qtPanel);
        qtPanel.IsOpen = true;   // 默认打开
    }

    // 从宿主 WindowSystem 摘除全部窗口（吞 ArgumentException：窗口可能已被宿主移除）
    public static void Uninstall()
    {
        var ws = PromeRotation.Plugin.Instance?.WindowSystem;
        if (ws == null) { control = null; settings = null; qtPanel = null; return; }

        if (control != null)
        {
            try { ws.RemoveWindow(control); } catch (ArgumentException) { }
            control = null;
        }
        if (settings != null)
        {
            try { ws.RemoveWindow(settings); } catch (ArgumentException) { }
            settings = null;
        }
        if (qtPanel != null)
        {
            try { ws.RemoveWindow(qtPanel); } catch (ArgumentException) { }
            qtPanel = null;
        }
    }

    // 开关 QT 面板悬浮窗
    public static void ToggleQtPanel()
    {
        if (qtPanel == null) return;
        qtPanel.IsOpen = !qtPanel.IsOpen;
    }

    // QT 或热键悬浮面板任一可见（设置页「面板控制」按钮状态依据）
    public static bool 任一面板可见 => (qtPanel?.IsOpen ?? false) || ErosUIHotkeyUI.PanelVisible;

    // 一键显示/隐藏 QT 面板与热键面板（设置页「面板控制」按钮）。
    // 仅切显隐不重建窗口。
    public static void SetPanelsVisible(bool visible)
    {
        if (qtPanel != null) qtPanel.IsOpen = visible;
        ErosUIHotkeyUI.SetPanelVisible(visible);
    }

    // 打开设置窗口
    public static void OpenSettings()
    {
        if (settings != null) settings.IsOpen = true;
    }

    // 开/关设置窗口（战斗控制条设置按钮：开着时再点一次关闭）
    public static void ToggleSettings()
    {
        if (settings == null) return;
        settings.IsOpen ^= true;
    }

    // PR DrawSettings 回调内嵌入口：一个打开设置窗口的按钮
    public static void DrawSettingsEntry()
    {
        if (ImGui.Button($"打开 {ErosUIJobEnv.作者} 设置窗口"))
            OpenSettings();
    }

    // ============================================================
    // === PR 本体面板隐藏（UI 线程节流压制，1 秒一次） ===
    // ============================================================
    private static long _上次HidePrPanels;

    // 隐藏 PR 本体的战斗控制/QT 面板。
    // 必须在 UI 线程调用（控制条 PreDraw 内节流触发）——
    // 切勿挂 Framework.Update：跨线程改写 WindowSystem.IsOpen 会与渲染竞争，导致全游戏 ImGui 交互假死（实测踩坑）。
    // force=true 绕过 1 秒节流：切职业时旧控制条刚卸、宿主会重新弹出本体面板，
    // 常规节流调用会被上次 PreDraw 的节流窗口吞掉导致闪现，OnEnterAcr 必须强制执行。
    public static void HidePrPanels(bool force = false)
    {
        var now = Environment.TickCount64;
        if (!force && now - _上次HidePrPanels < 1000) return;
        _上次HidePrPanels = now;
        try { PromeRotation.Plugin.Instance?.CloseQtWindow(); } catch { /* 宿主未就绪 */ }
    }

    // 存档位置钳制到屏幕工作区内（读档超界/换分辨率兜底）
    private static Vector2 限制存档位置(Vector2? saved)
    {
        if (saved is not Vector2 v) return new Vector2(40f, 220f);
        var vp = ImGui.GetMainViewport();
        var min = vp.WorkPos + new Vector2(4f);
        var max = vp.WorkPos + vp.WorkSize - new Vector2(364f, 74f);
        if (max.X < min.X) max.X = min.X;
        if (max.Y < min.Y) max.Y = min.Y;
        return new Vector2(Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y));
    }
}
