using ECommons.DalamudServices;
using ErosUI.Data;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace ErosUI.Helper;

// UI 框架自用的 QT 数据入口 —— 只收口带框架附加逻辑的少数能力：
// 联动写入（宿主 SetQt + 联动表）与可见性重建（按职业/模式注册 QT）。
// 其余与宿主等价的能力（读 QT / 屏幕提示 / 日志）不在此转发, 直接用宿主 SDK。
public static class APIHelper
{
    #region QT 读写

    // 设置 QT 开关状态：宿主 SetQt 为基座, 叠加当前职业环境的联动表批量写入
    public static void 设置QT(string qtKey, bool 值)
    {
        PromeSettings.Instance.SetQt(qtKey, 值);
        var dict = PromeSettings.Instance.QuickToggles;
        if (ErosUIJobEnv.QtCascadeRules.TryGetValue(qtKey, out var links))
            foreach ((string lk, bool inv) in links)
                dict[lk] = inv ? !值 : 值;
    }

    // 重建 QT 可见性：ClearQts 后只注册当前职业、当前模式可见的 QT。
    // 多职业共存: 同时清除 QuickToggles 里其它职业注册的 QT 键（宿主全局表, 不清会串台）。
    public static void 重建QT可见性()
    {
        var isHigh = ErosUISettings.Instance.IsHighEnd;
        var qt = PromeSettings.Instance.QuickToggles;
        var saved = new Dictionary<string, bool>();
        foreach (var (key, _) in ErosUIJobEnv.QtAll)
            if (qt.TryGetValue(key, out var v)) saved[key] = v;
        PromeSettings.Instance.ClearQts();
        foreach (var k in qt.Keys.ToList())
            if (!ErosUIJobEnv.QtAll.ContainsKey(k)) qt.Remove(k);
        // 按用户自定义顺序注册（QT面板页/悬浮面板拖拽调整; 未调整过 = QT 表定义顺序）,
        // 宿主 QuickToggles 键序与 ErosUI 悬浮面板保持同序
        foreach (var key in ErosUISettings.Instance.GetOrderedQtKeys())
        {
            if (!ErosUIJobEnv.QtIsVisibleInMode(key, isHigh)) continue;
            var val = saved.TryGetValue(key, out var sv) ? sv : ErosUIJobEnv.QtDefault(key);
            PromeSettings.Instance.AddQt(key, val);
            qt[key] = val;
        }
        // PR 的 ClearQts 会连 HiddenQts 一起清空（宿主实测），重建后必须立刻按显隐配置回填，
        // 否则 QT管理 里隐藏的按钮在每次模式切换/进本后全部复活
        ErosUISettings.Instance.SyncQtHiddenToPr();
    }

    #endregion
}
