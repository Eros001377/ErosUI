using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace ErosUI.UI;

// 窗口内自绘背景的叠序辅助。
// 背景必须画进窗口自己的 draw list（首个内容），窗口叠序才正确——盖住房身后的窗口内容;
// 画进 viewport 背景层（GetBackgroundDrawList）会让背景沉到所有窗口内容之下,
// 两个窗口重叠时内容互相穿透（实测: 热键面板叠在设置窗上能看到设置窗的文字）。
// 但 Dalamud 模糊垫底的 PrependBlurBehind 会顶掉窗口 list 的首个 draw cmd（实测踩坑,
// 原 SettingsWindowBase.DrawWindowBackground 注释）——先垫一条零面积全透明牺牲帧占住 cmd[0]。
internal static class ErosUILayer
{
    // 零面积全透明矩形: 只产生一个退化 draw cmd, 不承担任何视觉
    public static void 垫牺牲帧(ImDrawListPtr drawList) =>
        drawList.AddRectFilled(Vector2.Zero, Vector2.Zero, 0u);
}
