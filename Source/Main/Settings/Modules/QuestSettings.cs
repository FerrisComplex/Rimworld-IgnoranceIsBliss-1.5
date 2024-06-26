﻿using System;
using System.Collections.Generic;
using System.Linq;
using DIgnoranceIsBliss;
using DIgnoranceIsBliss.Core_Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance.Modules;

public class QuestSettings : SettingsModuleBase
{
    private Dictionary<string, int> ManualRequirements = new Dictionary<string, int>();
    
    private Dictionary<string, int> DefaultValuesInternal = new Dictionary<string, int>()
    {
        {
            "ThreatReward_MechPods_MiscReward",
            (int)TechLevel.Ultra
        }
    };

    private static QuestSettings settings;
    
    public static bool IsEligableForQuest(QuestScriptDef def)
    {
        if (def == null) return true;
        if (settings != null && settings.ManualRequirements.TryGetValue(def.defName, out var value) && value >= 0 && value <= 7) return IgnoranceBase.TechIsEligibleForIncident((TechLevel)value);
        if (settings != null && settings.DefaultValuesInternal.TryGetValue(def.defName, out var value2) && value2 >= 0 && value2 <= 7) return IgnoranceBase.TechIsEligibleForIncident((TechLevel)value2);
        return !IgnoranceBase.questScriptDefs.TryGetValue(def.defName, out var tech) || IgnoranceBase.TechIsEligibleForIncident(tech);
    }

    public QuestSettings()
    {
        settings = this;
    }
    
    // QuestScriptDef
    // IgnoranceBase.questScriptDefs.TryGetValue(__instance.defName, out var tech) && !IgnoranceBase.TechIsEligibleForIncident(tech))

    public static bool showMenu = false;
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
        originalListing.DoSettingBool(filter, "Show Menu", "Shows the menu, this is hidden by default cause im lazy to build a collapasable menu inside a calapsable menu!", ref showMenu);
        originalListing.Gap();
        if (showMenu)
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
        ChangeQuests = true;
        ModModificationsAllowed = true;
        ManualRequirements.Clear();
        ManualRequirements.AddRange(DefaultValuesInternal);
    }

    

    public override void OnExposeData()
    {
        Look(ref ChangeQuests, "ChangeQuests", true);
        Look(ref ModModificationsAllowed, "ModModificationsAllowed", true);
        LookDictionary(ref ManualRequirements, "ManualRequirements");
    }
}
