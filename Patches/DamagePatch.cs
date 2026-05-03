using Godot;
using HarmonyLib;
using JmcModLib.Utils;
using MathTheSpire.Core;
using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MathTheSpire.Patches
{
    public static class ModSettings
    {
        public static int FontSize = 28;

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

        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Lazy<PropertyInfo?> CreatureCombatStateProperty = new(() =>
            typeof(Creature).GetProperty("CombatState", InstanceFlags));

        private static readonly Lazy<PropertyInfo?> CombatManagerInstanceProperty = new(() =>
            typeof(CombatManager).GetProperty("Instance", StaticFlags));

        private static readonly Lazy<MethodInfo?> CombatManagerDebugOnlyGetStateMethod = new(() =>
            typeof(CombatManager)
                .GetMethods(InstanceFlags)
                .FirstOrDefault(method => method.Name == "DebugOnlyGetState" && method.GetParameters().Length == 0));

        private static bool _combatStateLookupWarningLogged;

        private static CombatState? GetCombatStateCompat(Creature? playerCreature)
        {
            // 0.103: Creature.CombatState 的 getter 签名是 CombatState? get_CombatState()。
            // 0.104 Beta: getter 签名变成 ICombatState? get_CombatState()。
            // CLR 的方法签名包含返回类型，所以无论编译面向哪个版本，直接写
            // playerCreature.CombatState 都会在另一个版本里触发 MissingMethodException。
            // CombatManager.DebugOnlyGetState() 在 0.103 和 0.104 里仍然都是 CombatState?，
            // 因此优先从这里取；同时保持反射回退，避免静态绑定旧 getter。
            try
            {
                var combatManager = CombatManagerInstanceProperty.Value?.GetValue(null);
                if (combatManager != null && CombatManagerDebugOnlyGetStateMethod.Value is { } debugOnlyGetStateMethod)
                {
                    if (debugOnlyGetStateMethod.Invoke(combatManager, null) is CombatState managerState)
                    {
                        return managerState;
                    }
                }
            }
            catch (Exception ex)
            {
                LogCombatStateLookupWarningOnce($"通过 CombatManager.DebugOnlyGetState 获取战斗状态失败，尝试回退到 Creature.CombatState: {ex.Message}");
            }

            try
            {
                if (playerCreature != null && CreatureCombatStateProperty.Value is { } creatureCombatStateProperty)
                {
                    if (creatureCombatStateProperty.GetValue(playerCreature) is CombatState creatureCombatState)
                    {
                        return creatureCombatState;
                    }
                }
            }
            catch (Exception ex)
            {
                LogCombatStateLookupWarningOnce($"通过 Creature.CombatState 获取战斗状态失败: {ex.Message}");
            }

            return null;
        }

        private static void LogCombatStateLookupWarningOnce(string message)
        {
            if (_combatStateLookupWarningLogged) return;

            _combatStateLookupWarningLogged = true;
            ModLogger.Warn(message);
        }

        private static void UpdateDamageText(Creature playerCreature, Label damageLabel)
        {
            try
            {
                if (!GodotObject.IsInstanceValid(damageLabel)) return;

                var combatState = GetCombatStateCompat(playerCreature);

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
                        damageLabel.AddThemeColorOverride("font_color", MathTheSpireSettings.LethalDamageColor);
                    }
                    else if (actualDamage == 0)
                    {
                        damageLabel.AddThemeColorOverride("font_color", MathTheSpireSettings.SafeDamageColor);
                    }
                    else
                    {
                        damageLabel.AddThemeColorOverride("font_color", MathTheSpireSettings.WarningDamageColor);
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
