using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public static class ModuleUtils
{
    public static Color headerColor = new Color(1f, 1f, 1f);
    public static Color subHeaderColor = new Color(0.9f, 0.9f, 0.9f);
    public static Color settingColor = new Color(0.8f, 0.8f, 0.8f);

    private static bool filterMatches(string filter, string label, string description)
    {
        if (filter.NullOrEmpty()) return true;
        if (!label.NullOrEmpty() && label.ToLower().Contains(filter)) return true;
        //if (!description.NullOrEmpty() && description.ToLower().Contains(filter)) return true;
        return false;
    }


    public static void DoSettingBool(this Listing_Standard listing, string filter, string label, string description, ref bool value, bool gapLine = false, bool? enhanced = null)
    {
        if (!filterMatches(filter, label, description)) return;
        if (enhanced.GetValueOrDefault())
            listing.CheckboxEnhanced(label, description, ref value, null, null);
        else
            listing.CheckboxLabeled(label, ref value, description, 0f, 1f);
        if (gapLine) listing.GapLine(12f);
    }


    public static void DoSettingFloat(this Listing_Standard listing, string filter, string label, string description, ref float value, float min, float max, float? increment = null, bool percentage = false)
    {
        if (!filterMatches(filter, label, description)) return;
        listing.AddLabeledSlider((label.NullOrEmpty() ? "" : label + ": ") + (percentage ? value.ToStringPercent() : value.ToString("0.00")), ref value, min, max, "Min: " + (percentage ? min.ToStringPercent() : min.ToString()), "Max: " + (percentage ? max.ToStringPercent() : max.ToString()), increment ?? Mathf.Max(0.01f, (max - min) / 100f), false);
        if (!description.NullOrEmpty())
            listing.Note(description, GameFont.Tiny, new Color?(Color.gray));
    }


    public static void DoSettingInt(this Listing_Standard listing, string filter, string label, string description, ref int value, int min, int max, int? increment = null, bool dontShowValue = false)
    {
        if (!filterMatches(filter, label, description)) return;
        float f = (float)value;
        listing.AddLabeledSlider(dontShowValue ? label.NullOrEmpty() ? "" : label : ((label.NullOrEmpty() ? "" : label + ": ") + value.ToString()), ref f, (float)min, (float)max, string.Format("Min: {0}", min), string.Format("Max: {0}", max), (float)(increment ?? Mathf.RoundToInt(Mathf.Max(1f, (float)(max - min) / 100f))), false);
        value = Mathf.RoundToInt(f);

        if (!description.NullOrEmpty())
            listing.Note(description, GameFont.Tiny, new Color?(Color.gray));
    }


    public static void DoSettingIntRange(this Listing_Standard listing, string filter, string label, string description, ref IntRange value, int min, int max)
    {
        if (!filterMatches(filter, label, description)) return;
        listing.Label(label, -1f, null);
        if (!description.NullOrEmpty())
            listing.Note(description, GameFont.Tiny, null);
        listing.IntRange(ref value, min, max);
    }

    private static Dictionary<string, bool> toggleStatus = new Dictionary<string, bool>();

    public static void StartSection(this Listing_Standard listing, string label, string section, out bool toggle, Action setDefaults = null)
    {
        bool collapsedCategoryState = toggleStatus.GetValueOrDefault(section, false);
        listing.LabelBackedHeader(label, headerColor, ref collapsedCategoryState, GameFont.Medium, false);
        toggleStatus.SetOrAdd(section, collapsedCategoryState);
        if (setDefaults != null && !collapsedCategoryState && listing.ButtonTextLabeled("", "Restore Section Defaults", TextAnchor.UpperLeft, null, null))
            setDefaults();
        toggle = collapsedCategoryState;
    }


    public static void DoSection(this Listing_Standard listing, SectionDef def, string filter)
    {
        bool collapsedCategoryState = toggleStatus.GetValueOrDefault(def.Name, true);
        listing.LabelBackedHeader(def.Name, headerColor, ref collapsedCategoryState, GameFont.Medium, false, def.Tooltip);
        toggleStatus.SetOrAdd(def.Name, collapsedCategoryState);
        if (!collapsedCategoryState)
        {
            def.DoSectionContents(listing, filter);
            listing.GapLine(12f);
        }
    }


    public static void DoSubSection(this Listing_Standard listing, SubSectionDef def, string filter)
    {
        bool collapsedCategoryState = toggleStatus.GetValueOrDefault(def.Name, true);
        listing.LabelBackedHeader(def.Name, headerColor, ref collapsedCategoryState, GameFont.Small, false);
        toggleStatus.SetOrAdd(def.Name, collapsedCategoryState);
        if (!collapsedCategoryState)
        {
            listing.Note(def.Tooltip, GameFont.Tiny, null);
            foreach (var v in def.HeldTweaks.Where(x => x.IsUsable()))
                v.DoTweakContents(listing, filter);
        }
    }

    public static bool DoSubSectionDirect(this Listing_Standard listing, string name, ref bool collapsedCategoryState, string tooltipOverride = null)
    {
        listing.LabelBackedHeader(name, headerColor, ref collapsedCategoryState, GameFont.Small, false);
        if (!collapsedCategoryState && !tooltipOverride.NullOrEmpty())
            listing.Note(tooltipOverride, GameFont.Tiny, null);
        return collapsedCategoryState;
    }


    public static void TechLevelMenuWithExtraOption(this Listing_Standard listing, string extraOptionName, int extraIdValue, string name, string explanation, TechLevel value, float position, float buttonWidth, Action<TechLevel> action)
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


        if (action != null && Widgets.ButtonText(new Rect(position - buttonWidth, rect.y, buttonWidth, 29f), (int)value == extraIdValue ? extraOptionName : Enum.GetName(typeof(TechLevel), value), true, true, true, null))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption(extraOptionName, () => action((TechLevel)extraIdValue), MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));

            foreach (var enumValue in Enum.GetValues(typeof(TechLevel)))
                list.Add(new FloatMenuOption(Enum.GetName(typeof(TechLevel), (TechLevel)enumValue), () => action.Invoke((TechLevel)enumValue), MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
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


    public static void TechLevelMenu(this Listing_Standard listing, string name, string explanation, TechLevel value, float position, float buttonWidth, Action<TechLevel> action)
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


        
        if (action != null && Widgets.ButtonText(new Rect(position - buttonWidth, rect.y, buttonWidth, 29f), Enum.GetName(typeof(TechLevel), value), true, true, true, null))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (var enumValue in Enum.GetValues(typeof(TechLevel)))
                list.Add(new FloatMenuOption(Enum.GetName(typeof(TechLevel), (TechLevel)enumValue), () => action.Invoke((TechLevel)enumValue), MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
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
}
