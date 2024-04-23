using System;
using DFerrisIgnorance.Modules;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;

namespace DIgnoranceIsBliss.Core_Patches
{
    internal class Patch_CanRun_Prefix
    {
        public static bool Prefix(ref bool __result, QuestScriptDef __instance)
        {
            if (QuestSettings.ChangeQuests && !QuestSettings.IsEligableForQuest(__instance))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
