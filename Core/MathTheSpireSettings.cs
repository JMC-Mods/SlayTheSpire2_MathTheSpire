using Godot;
using JmcModLib.Config;
using JmcModLib.Config.UI;

namespace MathTheSpire.Core;

public static class MathTheSpireSettings
{
    internal const int MinDamageLabelFontSize = 12;
    internal const int MaxDamageLabelFontSize = 96;
    internal const int MinDamageLabelOffsetX = -600;
    internal const int MaxDamageLabelOffsetX = 600;
    internal const int MinDamageLabelOffsetY = -800;
    internal const int MaxDamageLabelOffsetY = 200;

    private const string DamageLabelGroup = "damage_label";
    private const string DamageColorGroup = "damage_colors";

    [UIIntSlider(MinDamageLabelFontSize, MaxDamageLabelFontSize)]
    [Config(
        "伤害数字字号",
        group: DamageLabelGroup,
        Description = "头顶伤害数字在 1080p 下的基准字号，实际显示会根据分辨率和 DPI 自动缩放。",
        Key = "damage_label.font_size",
        Order = 10)]
    public static int DamageLabelFontSize = 28;

    [UIIntSlider(MinDamageLabelOffsetX, MaxDamageLabelOffsetX)]
    [Config(
        "伤害数字水平位置",
        group: DamageLabelGroup,
        Description = "头顶伤害数字相对角色中心的水平偏移。负数向左，正数向右，实际显示会自动适配 DPI。",
        Key = "damage_label.offset_x",
        Order = 20)]
    public static int DamageLabelOffsetX = 0;

    [UIIntSlider(MinDamageLabelOffsetY, MaxDamageLabelOffsetY)]
    [Config(
        "伤害数字垂直位置",
        group: DamageLabelGroup,
        Description = "头顶伤害数字相对角色节点的垂直位置。负数向上，正数向下，实际显示会自动适配 DPI。",
        Key = "damage_label.offset_y",
        Order = 30)]
    public static int DamageLabelOffsetY = -350;

    [UIColor("#FF3333", "#E84B4B", "#FF6B6B", Palette = UIColorPalette.Game, AllowAlpha = false)]
    [Config(
        "致命伤害颜色",
        group: DamageColorGroup,
        Description = "当未被格挡的实际伤害会导致玩家死亡时使用的颜色。修改后会立即用于下一次伤害数字刷新。",
        Key = "damage_colors.lethal",
        Order = 10)]
    public static Color LethalDamageColor = new(1.0f, 0.2f, 0.2f, 1.0f);

    [UIColor("#66B3FF", "#5AA9E6", "#7CC7FF", Palette = UIColorPalette.Game, AllowAlpha = false)]
    [Config(
        "受伤警告颜色",
        group: DamageColorGroup,
        Description = "当护甲不足但不会致死时使用的颜色。修改后会立即用于下一次伤害数字刷新。",
        Key = "damage_colors.warning",
        Order = 20)]
    public static Color WarningDamageColor = new(0.4f, 0.7f, 1.0f, 1.0f);

    [UIColor("#66FF99", "#6EDC7F", "#8DFFB1", Palette = UIColorPalette.Game, AllowAlpha = false)]
    [Config(
        "安全格挡颜色",
        group: DamageColorGroup,
        Description = "当当前护甲可以完全格挡即将受到的伤害时使用的颜色。修改后会立即用于下一次伤害数字刷新。",
        Key = "damage_colors.safe",
        Order = 30)]
    public static Color SafeDamageColor = new(0.4f, 1.0f, 0.6f, 1.0f);
}
