﻿using BMTournamentPrize.Models;
using HarmonyLib;
using SandBox.TournamentMissions.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BMTweakCollection.Patches
{
    [HarmonyPatch(typeof(TournamentBehavior), "OnPlayerWinMatch")]
#pragma warning disable RCS1102 // Make class static.
    public class TournamentBehaviourPatchCharLevels
#pragma warning restore RCS1102 // Make class static.
    {
        static bool Prefix(TournamentBehavior __instance, ref TournamentParticipant[] ____participants)
        {
            int numHeroLevels = 0;
            int bonusMoney = 0;

            foreach (var p in ____participants)
            {
                if (p.Character.IsHero && p.Character.HeroObject != null && p.Character.HeroObject != Hero.MainHero)
                {
                    numHeroLevels += p.Character.HeroObject.Level;
                }
            }
            bonusMoney = numHeroLevels * BMTournamentPrizeConfiguration.Instance.TournamentBonusMoneyBaseNamedCharLevel;
            typeof(TournamentBehavior).GetProperty("OverallExpectedDenars").SetValue(__instance, __instance.OverallExpectedDenars + bonusMoney);
            return true;
        }

        static bool Prepare()
        {
            return BMTournamentPrizeConfiguration.Instance.TournamentBonusMoneyBaseNamedCharLevel > 0;
        }
    }

    [HarmonyPatch(typeof(TournamentBehavior), "CalculateBet")]
    public class TournamentBehaviorPatchCalculateBet
    {
        public static bool Prefix(ref TournamentBehavior __instance)
        {

            //var tb = Traverse.Create(__instance);
            
            if (__instance.IsPlayerParticipating)
            {
                if (__instance.CurrentRound.CurrentMatch == null)
                {
                    //__instance.BetOdd = 0f;
                    //tb.Field("BetOdd").SetValue(0f);
                    typeof(TournamentBehavior).GetProperty("BetOdd").SetValue(__instance, 0f);
                    
                    return false;
                }
                if (__instance.IsPlayerEliminated || !__instance.IsPlayerParticipating)
                {
                    // __instance.OverallExpectedDenars = 0;
                    //this.BetOdd = 0f;
                    // tb.Field("BetOdd").SetValue(0f);
                    // tb.Field("OverallExpectedDenars").SetValue(0);
                    typeof(TournamentBehavior).GetProperty("BetOdd").SetValue(__instance, 0f);
                    typeof(TournamentBehavior).GetProperty("OverallExpectedDenars").SetValue(__instance, 0);

                    return false;
                }
                float single = 50f;
                float matchScore = 0f;
                float playerTeamScore = 0f;
                TournamentMatch[] matches = __instance.CurrentRound.Matches;
                for (int i = 0; i < (int)matches.Length; i++)
                {
                    foreach (TournamentTeam team in matches[i].Teams)
                    {
                        float level = 0f;
                        foreach (TournamentParticipant participant in team.Participants)
                        {
                            //level += (float)participant.Character.Level;
                            level += (float)participant.Character.Level;                            
                            //If they are a named hero, increase their score a bit
                            if (participant.Character.IsHero)
                            {
                                level += 50f;
                            }
                            //Tack on armor values
                            level += participant.Character.GetArmArmorSum()*2 + participant.Character.GetBodyArmorSum()*3 + participant.Character.GetLegArmorSum()*2;
                            //Get skills based 
                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Bow) 
                                + (float)participant.Character.GetSkillValue(DefaultSkills.OneHanded)
                                + (float)participant.Character.GetSkillValue(DefaultSkills.TwoHanded)
                                + (float)participant.Character.GetSkillValue(DefaultSkills.Throwing)
                                + (float)participant.Character.GetSkillValue(DefaultSkills.Polearm) 
                                + (float)participant.Character.GetSkillValue(DefaultSkills.Riding);
                            //bool hasBow, hasTwoH, hasOneH, hasHorse = false;

                            //Get skills based on equipment
                            //Unfortunately we don't know the equipped match equipment at the betting stage so have to use just all their skills for now
                            /*
                            for (var ie = 0; ie < 5; ie++)
                            {
                                var slotEquipment = participant.MatchEquipment.GetEquipmentFromSlot((EquipmentIndex)ie);
                                if (slotEquipment.Item != null)
                                {
                                    switch (slotEquipment.Item.ItemType)
                                    {
                                        case ItemObject.ItemTypeEnum.Bow:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Bow);
                                            break;
                                        case ItemObject.ItemTypeEnum.OneHandedWeapon:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.OneHanded);
                                            break;
                                        case ItemObject.ItemTypeEnum.Thrown:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Throwing);
                                            break;
                                        case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.TwoHanded);
                                            break;
                                        case ItemObject.ItemTypeEnum.Polearm:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Polearm);
                                            break;
                                        case ItemObject.ItemTypeEnum.Crossbow:
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Crossbow);
                                            break;

                                    }
                                }
                            }

                            if (participant.MatchEquipment.Horse.Item != null)
                            {                              
                                            level += (float)participant.Character.GetSkillValue(DefaultSkills.Riding);                              
                            }
                            */
                        }
                        if (team.Participants.Any<TournamentParticipant>((TournamentParticipant x) => x.Character == CharacterObject.PlayerCharacter))
                        {
                            level += single;  //Human player gets an additional score increase
                            playerTeamScore = level;
                        }
                        matchScore += level;
                    }
                }
                float single3 = MathF.Clamp((float)Math.Sqrt((double)(matchScore / playerTeamScore)), 1.01f, BMTournamentPrizeConfiguration.Instance.MaximumBetOdds);
                //tb.Field("BetOdd").SetValue(Math.Min(single3, BMTournamentPrizeConfiguration.Instance.MaximumBetOdds));                
                typeof(TournamentBehavior).GetProperty("BetOdd").SetValue(__instance, Math.Min(single3, BMTournamentPrizeConfiguration.Instance.MaximumBetOdds)); 
            }
            return false;
        }

        static bool Prepare()
        {
            return BMTournamentPrizeConfiguration.Instance.OppenentDifficultyAffectsOdds;
        }
    }


}