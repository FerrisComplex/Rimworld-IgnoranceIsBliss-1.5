using System;
using HarmonyLib;
using RimWorld;

namespace DIgnoranceIsBliss.Core_Patches
{
    internal class Patch_FactionCanBeGroupSource_Postfix
    {
        public static void Postfix(Faction f, ref bool __result)
        {
            if(__result)
                __result = IgnoranceBase.FactionInEligibleTechRange(f);
        }
    }
}
