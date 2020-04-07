﻿using BMTournamentPrizes.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames;
using TournamentLib.Models;

namespace BMTournamentPrizes.Patch
{

    [HarmonyPatch(typeof(TournamentManager), "AddTournament")]
    public class TournamentManagerPatchAddTournament1
    {
        public static void Prefex(TournamentGame game)
        {
            TournamentPrizeExpansion.Instance.ClearTournamentPrizes(game.Town.Settlement.StringId);
        }
        static bool Prepare()
        {
            return (TournamentConfiguration.Instance.PrizeConfiguration.EnableConfigReloadRealTime || TournamentConfiguration.Instance.PrizeConfiguration.EnablePrizeSelection);
        }
    }

    [HarmonyPatch(typeof(TournamentManager), "ResolveTournament")]
    public class TournamentManagerPatchResolveTournament1
    {
        public static void Prefex(TournamentGame tournament)
        {
            TournamentPrizeExpansion.Instance.ClearTournamentPrizes(tournament.Town.Settlement.StringId);
        }
        static bool Prepare()
        {
            return (TournamentConfiguration.Instance.PrizeConfiguration.EnableConfigReloadRealTime || TournamentConfiguration.Instance.PrizeConfiguration.EnablePrizeSelection);
        }
    }
}
