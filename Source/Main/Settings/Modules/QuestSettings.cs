using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance.Modules;

public class QuestSettings : SettingsModuleBase
{
    private Dictionary<string, int> ManualRequirements = new Dictionary<string, int>()
    {
        {
            "ThreatReward_MechPods_MiscReward",
            (int)TechLevel.Ultra
        }
    };


    public static bool ModifyQuestTechLevels = false;
    public static bool ChangeQuests = true;
    public static bool ModModificationsAllowed = true;

    private static readonly Dictionary<ModContentPack, bool> IsCollapsed = new Dictionary<ModContentPack, bool>();
    private static bool _isInvalidCollapsed = true;


    public override void DoTweakContents(Listing_Standard originalListing, string filter = "")
    {
        originalListing.DoSettingBool(filter, "Restrict Quest threads", "Substitute quest factions for a faction in range. Will not change the quest description, but an appropriate faction will arrive.", ref ChangeQuests);
        originalListing.DoSettingBool(filter, "Allow Other Mods to Inject TechLevel defaults into Quests", "Allow mod compatability to run, allowing mod devs to assign their own tech levels to quests\nChanging values in the bellow setting can override their defaults!", ref ModModificationsAllowed);
        originalListing.Gap();
        originalListing.Gap();
        originalListing.Label("Note: If the quest contains any faction or pawn references");
        originalListing.Label("The system will automatically restrict that quests pawns to be within your allowed tech range while not showing that in the menus!");
        originalListing.Label("Otherwise events can fire regardless of TechLevel, The default value is neolithic for ALL quests outside the above logic");
        originalListing.Label("Changing the settings will set the required TechLevel to the highest value between the system and your change");
        originalListing.Gap();
        originalListing.DoSettingBool(filter, "Modify TechLevel of Certain Quests", "Allows Changing Specific Quests, ie changing V.O.I.D PlanetCracker event to only happen when above a certain tech level", ref ModifyQuestTechLevels);
        originalListing.Gap();
        if (ModifyQuestTechLevels)
        {
            originalListing.Gap();
            if (originalListing.ButtonTextLabeled("", "Restore Section Defaults", TextAnchor.UpperLeft, null, null))
            {
                OnReset();
                Messages.Message("TechLevel By Project tweaks restored to defaults.", MessageTypeDefOf.CautionInput, true);
            }

            var lastIndex = -1;
            bool shouldSkip = false;

            List<QuestScriptDef> invalidDefs = new List<QuestScriptDef>();

            float buttonSize = Text.CalcSize("Not Forced TechLevel").x + 30;

            foreach (var v in DefDatabase<QuestScriptDef>.AllDefs.OrderBy(x => x != null && x.modContentPack != null && x.modContentPack.IsCoreMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null && x.modContentPack.IsOfficialMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null ? x.modContentPack.loadOrder : int.MaxValue - 1))
            {
                if (v == null) continue;
                if (v.modContentPack == null)
                {
                    invalidDefs.Add(v);
                    continue;
                }


                if (lastIndex != v.modContentPack.loadOrder)
                {
                    lastIndex = v.modContentPack.loadOrder;
                    originalListing.Gap();
                    bool collapsedCategoryState = IsCollapsed.GetValueOrDefault(v.modContentPack, true);
                    shouldSkip = originalListing.DoSubSectionDirect(v.modContentPack.Name.NullOrEmpty() ? v.modContentPack.PackageId.NullOrEmpty() ? "Unknown(" + v.defName + ")" : v.modContentPack.PackageId : v.modContentPack.Name, ref collapsedCategoryState);
                    IsCollapsed.SetOrAdd(v.modContentPack, collapsedCategoryState);
                }

                if (shouldSkip) continue;
                var reference = ManualRequirements.TryGetValue(v.defName, 128);
                originalListing.TechLevelMenuWithExtraOption("Not Forced TechLevel", 128, v.LabelCap.NullOrEmpty() ? v.label.NullOrEmpty() ? v.defName : v.label : v.LabelCap, v.description.NullOrEmpty() ? "No Description" : v.description, (TechLevel)reference, -1, buttonSize, (x) =>
                {
                    if ((int)x == 128)
                        ManualRequirements.Remove(v.defName);
                    else
                        ManualRequirements.SetOrAdd(v.defName, (int)x);
                });
            }

            
            
            originalListing.Gap();
            bool collapsedCategoryState_ = _isInvalidCollapsed;
            shouldSkip = originalListing.DoSubSectionDirect("XML Patch Def", ref collapsedCategoryState_);
            _isInvalidCollapsed = collapsedCategoryState_;
            foreach (var v in invalidDefs)
            {
                if (shouldSkip) continue;
                var reference = ManualRequirements.TryGetValue(v.defName, 128);
                originalListing.TechLevelMenuWithExtraOption("Not Forced TechLevel", 128, v.LabelCap.NullOrEmpty() ? v.label.NullOrEmpty() ? v.defName : v.label : v.LabelCap, v.description.NullOrEmpty() ? "No Description" : v.description, (TechLevel)reference, -1, buttonSize, (x) =>
                {
                    if ((int)x == 128)
                        ManualRequirements.Remove(v.defName);
                    else
                        ManualRequirements.SetOrAdd(v.defName, (int)x);
                });
            }

            originalListing.Gap();
        }
    }

    public override void OnReset()
    {
        ModifyQuestTechLevels = true;
        ChangeQuests = true;
        ModModificationsAllowed = true;
        ManualRequirements.Clear();
    }


    public override void OnExposeData()
    {
        Look(ref ModifyQuestTechLevels, "ModifyQuestTechLevels", false);
        Look(ref ChangeQuests, "ChangeQuests", true);
        Look(ref ModModificationsAllowed, "ModModificationsAllowed", true);
        
        foreach (var v in DefDatabase<QuestScriptDef>.AllDefs.OrderBy(x => x != null && x.modContentPack != null && x.modContentPack.IsCoreMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null && x.modContentPack.IsOfficialMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null ? x.modContentPack.loadOrder : int.MaxValue - 1))
        {
            var value = ManualRequirements.TryGetValue(v.defName, out var valueResult) ? valueResult : -999;
            Look(ref value, "ManualRequirements." + v.defName, value);
            if (value == -999)
                ManualRequirements.Remove(v.defName);
            else
                ManualRequirements.SetOrAdd(v.defName, value);
        }
    }
}
