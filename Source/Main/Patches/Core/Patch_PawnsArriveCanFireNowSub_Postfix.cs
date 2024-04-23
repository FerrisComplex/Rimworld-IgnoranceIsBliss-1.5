using System;
using DFerrisIgnorance.Modules;
using HarmonyLib;
using RimWorld;

namespace DIgnoranceIsBliss.Core_Patches
{
    internal class Patch_PawnsArriveCanFireNowSub_Postfix
    {
        public static void Postfix(ref IncidentParms parms, ref bool __result)
        {
            if (!__result || parms.faction == null || IgnoranceBase.FactionInEligibleTechRange(parms.faction)) return;
            Faction randomEligibleFaction = IgnoranceBase.GetRandomEligibleFaction();
            if (randomEligibleFaction != null) parms.faction = randomEligibleFaction;
            
        }
    }
}
