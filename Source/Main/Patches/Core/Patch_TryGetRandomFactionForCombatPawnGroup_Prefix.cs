using System;
using System.Runtime.CompilerServices;
using DIgnoranceIsBliss.Core_Patches;
using HarmonyLib;
using RimWorld;

namespace DIgnoranceIsBliss
{
    internal class Patch_TryGetRandomFactionForCombatPawnGroup_Prefix
    {
        public static bool Prefix(ref Predicate<Faction> validator)
        {
            if (validator == null)
                validator = IgnoranceBase.FactionInEligibleTechRange;
            return true;
        }
    }
}
