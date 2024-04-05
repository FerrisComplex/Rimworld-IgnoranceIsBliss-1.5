using System;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;

namespace DIgnoranceIsBliss.Core_Patches
{
    internal class Patch_CanRun_Prefix
    {
        public static bool Prefix(ref bool __result, QuestScriptDef __instance)
        {
            if (SettingsHelper.LatestVersion.ChangeQuests && IgnoranceBase.questScriptDefs.TryGetValue(__instance.defName, out var tech) && !IgnoranceBase.TechIsEligibleForIncident(tech))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
