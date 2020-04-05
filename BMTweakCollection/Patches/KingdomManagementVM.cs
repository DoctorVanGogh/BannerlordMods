﻿using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerlordTweaks.Patches
{
    [HarmonyPatch(typeof(KingdomManagementVM), "ExecuteLeaveKingdom")]
    public class ExecuteLeaveKingdomPatch
    {
        static bool Prefix(KingdomManagementVM __instance)
        {
            try
            {
                if (Clan.PlayerClan.IsUnderMercenaryService)
                {
                    InformationManager.ShowInquiry(new InquiryData("Ending Mercenary Contract",
                        $"Your mercenary contract will end and you will no longer be affiliated with {Clan.PlayerClan.Kingdom.Name.ToString()}.",
                        true, true, new TextObject("{=5Unqsx3N}Confirm").ToString(),
                        GameTexts.FindText("str_cancel").ToString(),
                        () =>
                        {
                            ChangeKingdomAction.ApplyByLeaveKingdomAsMercenaryForNoPayment(Clan.PlayerClan, Clan.PlayerClan.Kingdom);
                            typeof(KingdomManagementVM).GetMethod("ExecuteClose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            .Invoke(__instance, null);
                        },
                        null));
                    return false;
                }
            }
            catch (Exception ex)
            {
             FileLog.Log($"An error occurred in the leaving Kingdom as mercenary fix:\n\n{ex.ToString()}");
            }
            return true;
        }
    }
}
