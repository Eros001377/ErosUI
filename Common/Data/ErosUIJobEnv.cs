using ErosUI.Helper;
using PromeRotation.Data;

namespace ErosUI.Data;

// 职业环境注入点 —— UI 框架与具体 ACR 的唯一耦合面。
// 使用方在进入 ACR 生命周期（Rotation 构造最前 / OnEnterAcr 前）调用 Configure 注入职业身份与数据源；
// 框架层（设置快照 / 悬浮面板 / 宏命令）一律读本类, 不引用任何职业数据源。
// 生命周期约定：宿主每切必 new Rotation 实例, 构造注入 = 激活职业总与运行环境一致;
// 不同职业的注入不会同时活跃, 静态字段读写无并发问题。
public static class ErosUIJobEnv
{
    // 窗口名/绘制 ID 后缀（如 SAM / RPR）, 保证宿主 WindowSystem 与绘制 ID 不跨职业冲突
    public static string JobTag { get; private set; } = "SAM";

    // 职业中文名（设置窗标题「xx 设置」用）
    public static string JobName { get; private set; } = "武士";

    // 作者身份（占位默认「author」, 使用方必须经 Configure 改成自己的作者名, 见 README「作者身份」）。
    // 用途: 宿主配置目录约定 Settings\ACRConfig\<作者>（与 RotationMetadata 的 Author 一致）、
    // 窗口标题前缀与日志前缀的品牌字样——一个字段管全部对外可见的自称。
    public static string 作者 { get; private set; } = "author";

    // === QT 数据源（键 → 默认值 及其元数据; 全部由使用方注入, 缺省为空表） ===
    public static IReadOnlyDictionary<string, bool> QtAll { get; private set; } = new Dictionary<string, bool>();
    public static Func<string, bool> QtIsMetaKey { get; private set; } = _ => false;
    public static Func<string, bool, bool> QtIsVisibleInMode { get; private set; } = (_, _) => true;
    public static Func<string, bool> QtDefault { get; private set; } = _ => false;
    public static IReadOnlyDictionary<string, (string key, bool invert)[]> QtCascadeRules { get; private set; }
        = new Dictionary<string, (string, bool)[]>();

    // 热键面板按钮名全集（设置页 Hotkey 显隐列表来源; 由使用方注入）
    public static string[] HotkeyNames { get; private set; } = Array.Empty<string>();

    // 热键面板按钮构建（框架不预置任何按钮, 全部条目由使用方经本委托注册）
    public static System.Action<ErosUIHotkeyBuilder>? BuildHotkeys { get; private set; }

    // 热键面板重建回调（设置页布局/显隐改动后调用; 使用方一般传 ErosUIHotkeyUI.Rebuild）
    public static System.Action? RebuildHotkeys { get; private set; }

    // === QT 设置页三个页签的开关清单（key, label, requiresSkillID=0 不校验技能） ===
    public static (string key, string label, uint skill)[] QtTab基础 { get; private set; } = Array.Empty<(string, string, uint)>();
    public static (string key, string label, uint skill)[] QtTab技能 { get; private set; } = Array.Empty<(string, string, uint)>();
    public static (string key, string label, uint skill)[] QtTab资源 { get; private set; } = Array.Empty<(string, string, uint)>();

    // 注入本职业的全部环境（Rotation 构造函数最前面调用）
    public static void Configure(
        string jobTag,
        string jobName,
        IReadOnlyDictionary<string, bool> qtAll,
        Func<string, bool> qtIsMetaKey,
        Func<string, bool, bool> qtIsVisibleInMode,
        Func<string, bool> qtDefault,
        IReadOnlyDictionary<string, (string key, bool invert)[]> qtCascadeRules,
        string[] hotkeyNames,
        System.Action<ErosUIHotkeyBuilder>? buildHotkeys,
        System.Action? rebuildHotkeys = null,
        (string key, string label, uint skill)[]? qtTab基础 = null,
        (string key, string label, uint skill)[]? qtTab技能 = null,
        (string key, string label, uint skill)[]? qtTab资源 = null,
        string? author = null)
    {
        JobTag = jobTag;
        JobName = jobName;
        if (author != null) 作者 = author;
        QtAll = qtAll;
        QtIsMetaKey = qtIsMetaKey;
        QtIsVisibleInMode = qtIsVisibleInMode;
        QtDefault = qtDefault;
        QtCascadeRules = qtCascadeRules;
        HotkeyNames = hotkeyNames;
        BuildHotkeys = buildHotkeys;
        RebuildHotkeys = rebuildHotkeys;
        if (qtTab基础 != null) QtTab基础 = qtTab基础;
        if (qtTab技能 != null) QtTab技能 = qtTab技能;
        if (qtTab资源 != null) QtTab资源 = qtTab资源;
    }
}
