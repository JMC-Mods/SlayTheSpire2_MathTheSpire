using Godot;
using HarmonyLib;
using JmcModLib.Utils;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.Screens;
using System.Reflection;

namespace MathTheSpire.Patches;

[HarmonyPatch(typeof(NDeckViewScreen), "_Ready")]
public static class CardRemovalPricePatch
{
    private const string PriceLabelNodeName = "MathTheSpireCardRemovalPriceLabel";
    private const string PriceTextKey = "EXTENSION.MATHTHESPIRE.DECK_VIEW.CARD_REMOVAL_PRICE";
    private const string UnavailableTextKey = "EXTENSION.MATHTHESPIRE.DECK_VIEW.CARD_REMOVAL_UNAVAILABLE";
    private const int PriceLabelWidth = 280;
    private const int PriceLabelHeight = 46;
    private const int PriceLabelOffsetX = 18;
    private const int PriceLabelVerticalSpacing = 12;
    private const int PriceLabelFontSize = 24;
    private const int PriceLabelOutlineSize = 4;

    private static readonly Color PriceLabelOutlineColor = new(0f, 0f, 0f, 0.95f);
    private static readonly Lazy<FieldInfo?> DeckViewPlayerField = new(() =>
        typeof(NDeckViewScreen).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic));

    private static bool _playerLookupWarningLogged;

    public static void Postfix(NDeckViewScreen __instance)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(__instance))
            {
                return;
            }

            if (TryGetDeckViewPlayer(__instance) is not { } player)
            {
                return;
            }

            var backButton = __instance.GetNodeOrNull<Control>("BackButton");
            if (backButton == null)
            {
                ModLogger.Warn("添加删牌价格提示失败：找不到牌组界面的返回按钮节点。");
                return;
            }

            if (__instance.GetNodeOrNull<Label>(PriceLabelNodeName) != null)
            {
                return;
            }

            var priceLabel = CreatePriceLabel(player, __instance, backButton);
            __instance.AddChild(priceLabel);
        }
        catch (Exception ex)
        {
            ModLogger.Error($"添加删牌价格提示时发生异常: {ex.Message}");
        }
    }

    private static Player? TryGetDeckViewPlayer(NDeckViewScreen screen)
    {
        try
        {
            return DeckViewPlayerField.Value?.GetValue(screen) as Player;
        }
        catch (Exception ex)
        {
            LogPlayerLookupWarningOnce($"读取牌组界面玩家信息失败，跳过删牌价格提示: {ex.Message}");
            return null;
        }
    }

    private static void LogPlayerLookupWarningOnce(string message)
    {
        if (_playerLookupWarningLogged) return;

        _playerLookupWarningLogged = true;
        ModLogger.Warn(message);
    }

    private static Label CreatePriceLabel(Player player, Control screen, Control backButton)
    {
        var priceLabel = new Label
        {
            Name = PriceLabelNodeName,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ClipText = true,
            ZIndex = 10
        };

        priceLabel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        priceLabel.Position = CalculatePriceLabelPosition(screen, backButton);
        priceLabel.Size = new Vector2(PriceLabelWidth, PriceLabelHeight);
        priceLabel.AddThemeFontSizeOverride(ThemeConstants.Label.FontSize, PriceLabelFontSize);
        priceLabel.AddThemeColorOverride(ThemeConstants.Label.FontOutlineColor, PriceLabelOutlineColor);
        priceLabel.AddThemeConstantOverride("outline_size", PriceLabelOutlineSize);

        UpdatePriceLabel(player, priceLabel);
        return priceLabel;
    }

    private static Vector2 CalculatePriceLabelPosition(Control screen, Control backButton)
    {
        var screenTopLeft = screen.GetGlobalRect().Position;
        var buttonTopLeft = backButton.GetGlobalRect().Position;

        return buttonTopLeft - screenTopLeft + new Vector2(
            PriceLabelOffsetX,
            -PriceLabelHeight - PriceLabelVerticalSpacing);
    }

    private static void UpdatePriceLabel(Player player, Label priceLabel)
    {
        if (!Hook.ShouldAllowMerchantCardRemoval(player.RunState, player))
        {
            priceLabel.Text = L10n.Resolve(
                UnavailableTextKey,
                "删牌不可用",
                L10n.DefaultTable,
                typeof(MainFile).Assembly);
            priceLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, StsColors.red);
            return;
        }

        var removalEntry = new MerchantCardRemovalEntry(player);
        int price = removalEntry.Cost;

        priceLabel.Text = L10n.Resolve(
            PriceTextKey,
            $"下次删牌：{price}",
            L10n.DefaultTable,
            typeof(MainFile).Assembly,
            locString => locString.Add("Price", (decimal)price));
        priceLabel.AddThemeColorOverride(
            ThemeConstants.Label.FontColor,
            price <= player.Gold ? StsColors.cream : StsColors.red);
    }
}
