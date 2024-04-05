using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DIgnoranceIsBliss.Core_Patches;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace DIgnoranceIsBliss
{

    public class HarmonyPatches : Mod
    {


        
        private void PatchVanillaRimworld(Harmony harmony)
        {
            Ferris.PatchHelper.RegisterPrefixPatch(typeof(PawnGroupMakerUtility), "TryGetRandomFactionForCombatPawnGroup", null, typeof(Patch_TryGetRandomFactionForCombatPawnGroup_Prefix));
            Ferris.PatchHelper.RegisterPrefixPatch(typeof(QuestScriptDef), "CanRun", new[] { typeof(Slate) }, typeof(Patch_CanRun_Prefix));
            
            Ferris.PatchHelper.RegisterPostfixPatch(typeof(IncidentWorker_PawnsArrive), "FactionCanBeGroupSource", null, typeof(Patch_FactionCanBeGroupSource_Postfix));
            Ferris.PatchHelper.RegisterPostfixPatch(typeof(ResearchManager), "FinishProject", null, typeof(Patch_FinishProject_Postfix));
            Ferris.PatchHelper.RegisterPostfixPatch(typeof(IncidentWorker_PawnsArrive), "CanFireNowSub", null, typeof(Patch_PawnsArriveCanFireNowSub_Postfix));
            Ferris.PatchHelper.RegisterPostfixPatch(typeof(StorytellerComp), "UsableIncidentsInCategory", new[] {typeof(IncidentCategoryDef), typeof(Func<IncidentDef, IncidentParms>) }, typeof(Patch_UsableIncidentsInCategory_Postfix));
        }


        private void PatchWinstonWaves(Harmony harmony)
        {
            if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name.EqualsIgnoreCase("Winston Waves"))) return;
            D.Text("Found Winston Waves was loaded, Attempting to Load Patches");
            D.Error("This is not yet patched yet, please DM ferris about updating these patches!");
        }

        private void PatchRimWar(Harmony harmony)
        {
            if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name.EqualsIgnoreCase("RimWar"))) return;
            D.Text("Found RimWar was loaded, Attempting to Load Patches");
            D.Error("This is not yet patched yet, please DM ferris about updating these patches!");
        }



        public HarmonyPatches(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("io.github.dametri.ignorance");
            var executingAssembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(executingAssembly);
            PatchVanillaRimworld(harmony);
            PatchWinstonWaves(harmony);
            PatchRimWar(harmony);
            
            Ferris.PatchHelper.ProcessRegisteredPatches(harmony);
        }

    }

}
