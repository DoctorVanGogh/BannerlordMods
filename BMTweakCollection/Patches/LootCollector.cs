﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BMTweakCollection.Patches
{

    //public static class LootCollectorInfo
    //{
        //public static Type ClassType = Type.GetType("TaleWorlds.CampaignSystem.LootCollector, TaleWorlds.CampaignSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
    //}


  // [HarmonyPatch(Type.GetType("TaleWorlds.CampaignSystem.LootCollector, TaleWorlds.CampaignSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"))]
  
   // [HarmonyPatch(LootCollectorInfo.ClassType, "GetXpFromHit")]
    public class LootCollectorPatch
    {
        // make sure DoPatching() is called at start either by
        // the mod loader or by your injector
        internal static Type LootCollectorType;

        public static void DoPatching()
        {
            var harmony = new Harmony("com.example.patch");
            LootCollectorType = Type.GetType("TaleWorlds.CampaignSystem.LootCollector, TaleWorlds.CampaignSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            var mOriginal = AccessTools.Method(LootCollectorType, "GiveShareOfLootToParty");
            var prefix = typeof(LootCollectorPatch).GetMethod("GiveShareOfLootToPartyPre");

            // in general, add null checks here (new HarmonyMethod() does it for you too)

            harmony.Patch(mOriginal, new HarmonyMethod(prefix));
        }

    
        internal static bool GiveShareOfLootToPartyPre(PartyBase partyToReceiveLoot, PartyBase winnerParty, float lootAmount, 
            ref TroopRoster ___LootedMembers,
            ref ItemRoster ___LootedItems,
            ref TroopRoster ___LootedPrisoners,
            ref TroopRoster ___CasualtiesInBattle
            )
        {
            bool flag = winnerParty == PartyBase.MainParty;
            List<TroopRosterElement> troopRosterElements = new List<TroopRosterElement>();
            foreach (TroopRosterElement lootedMember in ___LootedMembers)
            {
                int number = lootedMember.Number;
                CharacterObject character = lootedMember.Character;
                for (int i = 0; i < number; i++)
                {
                    if (MBRandom.RandomFloat < lootAmount)
                    {
                        TroopRosterElement troopRosterElement = new TroopRosterElement(character)
                        {
                            Number = 1,
                            WoundedNumber = 1
                        };
                        troopRosterElements.Add(troopRosterElement);
                    }
                }
            }
            foreach (TroopRosterElement troopRosterElement1 in troopRosterElements)
            {
                ___LootedMembers.AddToCounts(troopRosterElement1.Character, -1, false, 0, 0, true, -1);
            }
            foreach (TroopRosterElement troopRosterElement2 in troopRosterElements)
            {
                if (!troopRosterElement2.Character.IsHero)
                {
                    partyToReceiveLoot.PrisonRoster.AddToCounts(troopRosterElement2.Character, troopRosterElement2.Number, false, 0, 0, true, -1);
                }
                else if (!partyToReceiveLoot.IsMobile)
                {
                    TakePrisonerAction.Apply(partyToReceiveLoot, troopRosterElement2.Character.HeroObject);
                }
                else
                {
                    TakePrisonerAction.Apply(partyToReceiveLoot, troopRosterElement2.Character.HeroObject);
                }
            }
            ICollection<ItemRosterElement> itemRosterElements = new List<ItemRosterElement>();
            for (int j = ___LootedItems.Count<ItemRosterElement>() - 1; j >= 0; j--)
            {
                ItemRosterElement elementCopyAtIndex = ___LootedItems.GetElementCopyAtIndex(j);
                int num = 0;
                EquipmentElement equipmentElement = elementCopyAtIndex.EquipmentElement;
                ItemObject item = equipmentElement.Item;
                equipmentElement = elementCopyAtIndex.EquipmentElement;
                ItemRosterElement itemRosterElement = new ItemRosterElement(item, 1, equipmentElement.ItemModifier);
                for (int k = 0; k < elementCopyAtIndex.Amount; k++)
                {
                    if (MBRandom.RandomFloat < lootAmount)
                    {
                        itemRosterElements.Add(itemRosterElement);
                        num++;
                    }
                }
                ___LootedItems.AddToCounts(itemRosterElement, -num, true);
            }
            partyToReceiveLoot.ItemRoster.Add(itemRosterElements);
            for (int l = ___LootedPrisoners.Count<TroopRosterElement>() - 1; l >= 0; l--)
            {
                int elementNumber = ___LootedPrisoners.GetElementNumber(l);
                CharacterObject characterAtIndex = ___LootedPrisoners.GetCharacterAtIndex(l);
                int num1 = 0;
                for (int m = 0; m < elementNumber; m++)
                {
                    if (MBRandom.RandomFloat < lootAmount)
                    {
                        partyToReceiveLoot.MemberRoster.AddToCounts(characterAtIndex, 1, false, 0, 0, true, -1);
                        num1++;
                    }
                }
                ___LootedPrisoners.AddToCounts(characterAtIndex, -num1, false, 0, 0, true, -1);
            }
            ICollection<TroopRosterElement> troopRosterElements1 = new List<TroopRosterElement>();
            for (int n = ___CasualtiesInBattle.Count<TroopRosterElement>() - 1; n >= 0; n--)
            {
                int elementNumber1 = ___CasualtiesInBattle.GetElementNumber(n);
                CharacterObject characterObject = ___CasualtiesInBattle.GetCharacterAtIndex(n);
                int num2 = 0;
                TroopRosterElement troopRosterElement3 = new TroopRosterElement(characterObject);
                for (int o = 0; o < elementNumber1; o++)
                {
                    if (MBRandom.RandomFloat < lootAmount)
                    {
                        troopRosterElements1.Add(troopRosterElement3);
                        num2++;
                    }
                }
                ___CasualtiesInBattle.AddToCounts(characterObject, -num2, false, 0, 0, true, -1);
            }
            ExplainedNumber explainedNumber = new ExplainedNumber(1f, null);
            if (winnerParty.MobileParty != null && winnerParty.MobileParty.Leader != null)
            {
                SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Roguery, DefaultSkillEffects.RogueryLootBonus, winnerParty.MobileParty.Leader, ref explainedNumber, true);
            }
            if (flag)
            {
                //IEnumerable<ItemRosterElement> itemRosterElements1 = this.LootCasualties(troopRosterElements1, explainedNumber.ResultNumber);
                LootCollectorType.GetMethod("LootCasualties").Invoke()
                partyToReceiveLoot.ItemRoster.Add(itemRosterElements1);
            }
            else if (partyToReceiveLoot.LeaderHero != null)
            {


               // int gold = LootCollectorPatch.ConvertLootToGold(this.LootCasualties(troopRosterElements1, 0.5f));
                gold = MBMath.Round((float)gold * 0.5f * explainedNumber.ResultNumber);
                GiveGoldAction.ApplyBetweenCharacters(null, partyToReceiveLoot.LeaderHero, gold, false);
                return false;
            }

            return false;
        }

        private static int ConvertLootToGold(IEnumerable<ItemRosterElement> lootedItemsRecoveredFromCasualties)
        {
            int num = 0;
            foreach (ItemRosterElement lootedItemsRecoveredFromCasualty in lootedItemsRecoveredFromCasualties)
            {
                int amount = lootedItemsRecoveredFromCasualty.Amount;
                EquipmentElement equipmentElement = lootedItemsRecoveredFromCasualty.EquipmentElement;
                num = num + amount * MBMath.Round((float)equipmentElement.GetBaseValue() * 0.5f);
            }
            return num;
        }

        private IEnumerable<ItemRosterElement> LootCasualties(ICollection<TroopRosterElement> shareFromCasualties, float lootFactor)
        {
            EquipmentElement equipmentElement;
            ItemModifier randomModifierWithTarget;
            ItemModifier itemModifier;
            ItemRoster itemRosters = new ItemRoster();
            List<EquipmentElement> equipmentElements = new List<EquipmentElement>();
            foreach (TroopRosterElement shareFromCasualty in shareFromCasualties)
            {
                for (int i = 0; i < 1; i++)
                {
                    Equipment randomEquipment = LootCollector.GetRandomEquipment(shareFromCasualty.Character);
                    equipmentElements.Clear();
                    int num = MBRandom.RoundRandomized(lootFactor);
                    for (int j = 0; j < num; j++)
                    {
                        float expectedLootedItemValue = ItemHelper.GetExpectedLootedItemValue(shareFromCasualty.Character);
                        EquipmentElement randomItem = randomEquipment.GetRandomItem(expectedLootedItemValue);
                        if (randomItem.Item != null && !randomItem.Item.NotMerchandise && equipmentElements.Count<EquipmentElement>((EquipmentElement x) => x.Item.Type == randomItem.Item.Type) == 0)
                        {
                            equipmentElements.Add(randomItem);
                        }
                    }
                    for (int k = 0; k < equipmentElements.Count; k++)
                    {
                        EquipmentElement item = equipmentElements[k];
                        ItemRosterElement itemRosterElement = new ItemRosterElement(item.Item, 1, null);
                        float single = ItemHelper.GetExpectedLootedItemValue(shareFromCasualty.Character);
                        EquipmentElement equipmentElement1 = itemRosterElement.EquipmentElement;
                        if (!equipmentElement1.Item.HasHorseComponent)
                        {
                            equipmentElement1 = itemRosterElement.EquipmentElement;
                            if (equipmentElement1.Item.HasArmorComponent)
                            {
                                equipmentElement1 = itemRosterElement.EquipmentElement;
                                equipmentElement = itemRosterElement.EquipmentElement;
                                ItemModifierGroup itemModifierGroup = equipmentElement.Item.ArmorComponent.ItemModifierGroup;
                                if (itemModifierGroup != null)
                                {
                                    equipmentElement = itemRosterElement.EquipmentElement;
                                    randomModifierWithTarget = itemModifierGroup.GetRandomModifierWithTarget(single / (float)equipmentElement.GetBaseValue(), 1f);
                                }
                                else
                                {
                                    randomModifierWithTarget = null;
                                }
                                equipmentElement1.SetModifier(randomModifierWithTarget);
                            }
                        }
                        else
                        {
                            equipmentElement1 = itemRosterElement.EquipmentElement;
                            equipmentElement = itemRosterElement.EquipmentElement;
                            ItemModifierGroup itemModifierGroup1 = equipmentElement.Item.HorseComponent.ItemModifierGroup;
                            if (itemModifierGroup1 != null)
                            {
                                equipmentElement = itemRosterElement.EquipmentElement;
                                itemModifier = itemModifierGroup1.GetRandomModifierWithTarget(single / (float)equipmentElement.GetBaseValue(), 1f);
                            }
                            else
                            {
                                itemModifier = null;
                            }
                            equipmentElement1.SetModifier(itemModifier);
                        }
                        itemRosters.Add(itemRosterElement);
                    }
                }
            }
            return itemRosters;
        }
    }
}
