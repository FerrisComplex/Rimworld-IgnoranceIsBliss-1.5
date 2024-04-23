using System;
using System.Collections.Generic;
using System.Linq;
using DIgnoranceIsBliss.Core_Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance.Modules;

public class FactionSettings : SettingsModuleBase
{
    private static FactionSettings settings;
    private Dictionary<string, int> ManualRequirements = new Dictionary<string, int>();


    public FactionSettings()
    {
        settings = this;
    }

    public static bool ModifyFactionTechLevels = false;
    public static bool ModifiesRealTechLevels = false;
    public static bool ModModificationsAllowed = true;
    private static readonly Dictionary<ModContentPack, bool> IsCollapsed = new Dictionary<ModContentPack, bool>();
    private static bool _isInvalidCollapsed = true;


    public static bool MechanoidsAlwaysEligable = true;
    public static bool EmpireAlwaysEligable = true;
    
    public static TechLevel UpdateFactionTechLevel(Faction faction)
    {
        if (settings != null && settings.ManualRequirements.TryGetValue(faction.def.defName, out var value) && value >= 0 && value <= 7) return (TechLevel)value;
        return faction.def.techLevel;
    }
    
    
    public static void TechMenuFactions(Listing_Standard listing, string name, TechLevel defaultValue, string explanation, TechLevel value, float position, float buttonWidth, Action<TechLevel> action)
    {
        float curHeight = listing.CurHeight;
        Rect rect = listing.GetRect(Text.LineHeight + listing.verticalSpacing, 1f);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect, name);
        Text.Anchor = TextAnchor.MiddleRight;
        // Assume -1 = right side, -2 = left side
        if (position == -1)
            position = rect.x + rect.width;
        else if (position == -2)
            position = rect.x + buttonWidth;


        
        if (action != null && Widgets.ButtonText(new Rect(position - buttonWidth, rect.y, buttonWidth, 29f), Enum.GetName(typeof(TechLevel), value) + (value == defaultValue ? " (Default)" : ""), true, true, true, null))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (var enumValue in Enum.GetValues(typeof(TechLevel)))
                list.Add(new FloatMenuOption(Enum.GetName(typeof(TechLevel), (TechLevel)enumValue) + (value == defaultValue ? " (Default)" : ""), () => action.Invoke( ((TechLevel)enumValue == defaultValue) ? (TechLevel)128 : (TechLevel)enumValue), MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
            Find.WindowStack.Add(new FloatMenu(list));
        }

        Text.Anchor = anchor;
        Text.Font = GameFont.Tiny;
        listing.ColumnWidth -= 34f;
        GUI.color = Color.gray;
        listing.Label(explanation, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        rect = listing.GetRect(0f, 1f);
        rect.height = listing.CurHeight - curHeight;
        rect.y -= rect.height;
        GUI.color = Color.white;
        listing.Gap(6f);
    }
    

    public override void DoTweakContents(Listing_Standard originalListing, string filter = "")
    {
        
        originalListing.DoSettingBool(filter, "Mechanoids are Always Eligable", "Allows Mechanoids to always be able to raid/attack you regardless of tech!", ref MechanoidsAlwaysEligable);
        originalListing.DoSettingBool(filter, "The Empire is Always Eligable", "Allows Empire to always be able to raid/attack you regardless of tech!", ref EmpireAlwaysEligable);
        originalListing.DoSettingBool(filter, "Allow Other Mods to Inject TechLevel defaults into Factions", "Allow mod compatability to run, allowing mod devs to assign their own tech levels to factions\nChanging values in the bellow setting can override their defaults!", ref ModModificationsAllowed);
        originalListing.Gap();
        originalListing.DoSettingBool(filter, "Modify TechLevel of Certain Factions", "Allows Changing Specific Factions, ie changing V.O.I.D N4 Mutants from neolithic to count as spacer", ref ModifyFactionTechLevels);
        originalListing.Gap();
        //originalListing.DoSettingBool(filter, "Modify Actual Faction TechLevel", "If enabled this changes the actual tech level of said faction! I do not recommend using this as it can cause things like ideology equipment to not be equipable or weapons not be usable causing pawns to spam drop their items when they visit or not be able to generate and lag like crazy when it attempts it!", ref ModifiesRealTechLevels);
        
        if (ModifyFactionTechLevels)
        {
            originalListing.Gap();
            if (originalListing.ButtonTextLabeled("", "Restore Section Defaults", TextAnchor.UpperLeft, null, null))
            {
                OnReset();
                Messages.Message("TechLevel By Faction tweaks restored to defaults.", MessageTypeDefOf.CautionInput, true);
            }

            var lastIndex = -1;
            bool shouldSkip = false;

            List<FactionDef> invalidDefs = new List<FactionDef>();
            
            float buttonSize = Text.CalcSize("Industrial (Default)").x + 30;

            foreach (var v in DefDatabase<FactionDef>.AllDefs.OrderBy(x => x != null && x.modContentPack != null && x.modContentPack.IsCoreMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null && x.modContentPack.IsOfficialMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null ? x.modContentPack.loadOrder : int.MaxValue - 1))
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
                var reference = ManualRequirements.TryGetValue(v.defName, (int)v.techLevel);
                TechMenuFactions(originalListing, v.LabelCap.NullOrEmpty() ? v.label.NullOrEmpty() ? v.defName : v.label : v.LabelCap, v.techLevel, v.description.NullOrEmpty() ? "No Description" : v.description, (TechLevel)reference, -1, buttonSize, (x) =>
                {
                    if ((int)x == 128)
                    {
                        ManualRequirements.Remove(v.defName);
                        
                    }
                    else
                    {
                        ManualRequirements.SetOrAdd(v.defName, (int)x);
                    }
                });
            }

            originalListing.Gap();
            bool collapsedCategoryState_ = _isInvalidCollapsed;
            shouldSkip = originalListing.DoSubSectionDirect("XML Patch Def", ref collapsedCategoryState_);
            _isInvalidCollapsed = collapsedCategoryState_;
            foreach (var v in invalidDefs)
            {
                if (shouldSkip) continue;
                var reference = ManualRequirements.TryGetValue(v.defName, (int)v.techLevel);
                TechMenuFactions(originalListing, v.LabelCap.NullOrEmpty() ? v.label.NullOrEmpty() ? v.defName : v.label : v.LabelCap, v.techLevel, v.description.NullOrEmpty() ? "No Description" : v.description, (TechLevel)reference, -1, buttonSize, (x) =>
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
        ModifyFactionTechLevels = false;
        ModifiesRealTechLevels = false;
        EmpireAlwaysEligable = true;
        ModModificationsAllowed = true;
        MechanoidsAlwaysEligable = true;
        ManualRequirements.Clear();
    }


    public override void OnExposeData()
    {
        Look(ref EmpireAlwaysEligable, "EmpireAlwaysEligable", true);
        Look(ref MechanoidsAlwaysEligable, "MechanoidsAlwaysEligable", true);
        Look(ref ModifiesRealTechLevels, "ModifiesRealTechLevels", false);
        Look(ref ModifyFactionTechLevels, "ModifyFactionTechLevels", false);
        Look(ref ModModificationsAllowed, "ModModificationsAllowed", true);

        foreach (var v in DefDatabase<FactionDef>.AllDefs.OrderBy(x => x != null && x.modContentPack != null && x.modContentPack.IsCoreMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null && x.modContentPack.IsOfficialMod ? 0 : 1).ThenBy(x => x != null && x.modContentPack != null ? x.modContentPack.loadOrder : int.MaxValue - 1))
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
