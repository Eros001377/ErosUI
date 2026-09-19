using ErosUI.Data;
using PromeRotation.Data;
using PromeRotation.UI.HotKey;

namespace ErosUI.Helper;

// 热键面板构建器：产出 (名称, IHotkey) 条目列表，交 ErosUIHotkeyPanelWindow 自绘
// （自绘外壳，不走宿主 HotkeyPanel 默认皮肤）。
// 统一按 HiddenHotkeys 过滤。图标约定：
// 固定技能按钮由 PAction 的技能 id 自动取游戏内图标；
// 逻辑按钮传 iconActionID（游戏内动作图标）或 customIconPath（PR 本体 Resources 资源 / 绝对路径 tex、png）。
public sealed class ErosUIHotkeyBuilder
{
    private readonly HashSet<string> hidden;

    // 构建产物：条目顺序 = 注册顺序 = 面板排布顺序。GameIcon = 游戏内原始图标 id（物品等无动作条目的图标来源），GameIconHQ = 是否取 HQ 品质图标（hq/ 子目录）。
    public List<(string Name, IHotkey Hotkey, uint GameIcon, bool GameIconHQ)> Entries { get; } = new();

    public ErosUIHotkeyBuilder(ErosUISettings settings)
    {
        hidden = new HashSet<string>(settings.HiddenHotkeys);
    }

    // 固定技能按钮：点击原样进热键队列，图标 = 游戏内技能图标。
    public void Fixed(string name, uint spell, ActionType type, ActionTargetType target)
    {
        if (!hidden.Contains(name))
            Entries.Add((name, new ActionHotkey(new PAction(spell, type, target)), 0, false));
    }

    // 自定义逻辑按钮：iconActionID = 游戏内动作 id（图标/冷却/充能显示来源）；
    // gameIconID = 游戏内原始图标 id（物品等不在 Action 表的条目用，经 GetFromGameIcon 直取，优先于 iconActionID）；
    // gameIconHQ = 图标取 HQ 品质（游戏内 hq/ 子目录）。
    public void Execute(string name, IHotkeyLogic logic, uint iconActionID = 0, string? customIconPath = null, uint gameIconID = 0, bool gameIconHQ = false)
    {
        if (!hidden.Contains(name))
            Entries.Add((name, new DelegateHotkey(logic, iconActionID, customIconPath), gameIconID, gameIconHQ));
    }
}
