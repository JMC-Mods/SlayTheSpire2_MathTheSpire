using Godot;
using HarmonyLib;
using MathTheSpire.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MathTheSpire.Patches
{
    public static class ModSettings
    {
        public static int FontSize = 28;

        // 致命伤害 (如果破盾后的实际伤害 >= 当前血量)
        public static Color LethalColor = new(1.0f, 0.2f, 0.2f, 1.0f);

        // 警告/部分格挡 (破盾后会掉血，但不致死)
        public static Color WarningColor = new(0.4f, 0.7f, 1.0f, 1.0f);

        // 完全安全 (护甲 >= 伤害)
        public static Color SafeColor = new(0.4f, 1.0f, 0.6f, 1.0f);

        // 字体描边大小和颜色
        public static int OutlineSize = 6;
        public static Color OutlineColor = new(0, 0, 0, 1.0f); // 纯黑描边

        // Y轴高度微调 (往上是负数，往下是正数。
        public static float HeightOffset = -350f;
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
    public static class Creature_CreateVisuals_Patch
    {
        public static void Postfix(Creature __instance, ref NCreatureVisuals? __result)
        {
            if (__result != null && __instance.IsPlayer)
            {
                try
                {
                    var damageLabel = new Label
                    {
                        Text = "",
                        HorizontalAlignment = HorizontalAlignment.Center, // 文本居中对齐
                        VerticalAlignment = VerticalAlignment.Center,
                        ZIndex = 2,

                        CustomMinimumSize = new Vector2(200, 50)
                    };

                    damageLabel.AddThemeFontSizeOverride("font_size", ModSettings.FontSize);

                    damageLabel.AddThemeColorOverride("font_outline_color", ModSettings.OutlineColor);
                    damageLabel.AddThemeConstantOverride("outline_size", ModSettings.OutlineSize);

                    __result.AddChild(damageLabel);

                    damageLabel.Position = new Vector2(-100, ModSettings.HeightOffset);

                    var updateTimer = new Godot.Timer
                    {
                        WaitTime = 0.1,
                        Autostart = true
                    };

                    updateTimer.Timeout += () =>
                    {
                        UpdateDamageText(__instance, damageLabel);
                    };

                    __result.AddChild(updateTimer);
                }
                catch (Exception ex)
                {
                    ModLogger.Error($"挂载 UI 时发生异常: {ex.Message}");
                }
            }
        }

        private static void UpdateDamageText(Creature playerCreature, Label damageLabel)
        {
            try
            {
                if (!GodotObject.IsInstanceValid(damageLabel)) return;

                var combatState = playerCreature?.CombatState ?? CombatManager.Instance?.DebugOnlyGetState();

                if (combatState == null || playerCreature == null || playerCreature.IsDead)
                {
                    damageLabel.Text = "";
                    damageLabel.Visible = false;
                    return;
                }

                damageLabel.Visible = true;
                int totalIncomingDamage = 0;
                var targets = new List<Creature> { playerCreature };

                if (combatState.HittableEnemies != null)
                {
                    foreach (var enemy in combatState.HittableEnemies)
                    {
                        if (enemy == null || enemy.IsDead || enemy.Monster == null) continue;

                        var nextMove = enemy.Monster.NextMove;
                        if (nextMove != null && nextMove.Intents != null)
                        {
                            foreach (var intent in nextMove.Intents)
                            {
                                if (intent is AttackIntent attackIntent)
                                {
                                    totalIncomingDamage += attackIntent.GetTotalDamage(targets, enemy);
                                }
                            }
                        }
                    }
                }

                if (totalIncomingDamage > 0)
                {
                    // 获取玩家当前护甲和当前血量
                    int currentBlock = playerCreature.Block;
                    int currentHp = playerCreature.CurrentHp;

                    // 计算实际受伤
                    int actualDamage = Math.Max(0, totalIncomingDamage - currentBlock);

                    if (currentBlock > 0)
                    {
                        damageLabel.Text = $"{totalIncomingDamage} ({actualDamage})";
                    }
                    else
                    {
                        damageLabel.Text = $"{totalIncomingDamage}";
                    }

                    if (actualDamage >= currentHp)
                    {
                        damageLabel.AddThemeColorOverride("font_color", ModSettings.LethalColor);
                    }
                    else if (actualDamage == 0)
                    {
                        damageLabel.AddThemeColorOverride("font_color", ModSettings.SafeColor);
                    }
                    else
                    {
                        damageLabel.AddThemeColorOverride("font_color", ModSettings.WarningColor);
                    }
                }
                else
                {
                    damageLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                ModLogger.Error($"伤害计算错误: {ex.Message}");
            }
        }
    }
}