using System;
using System.Collections.Generic;
using DIgnoranceIsBliss.Core_Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIgnoranceIsBliss;

internal class Settings : ModSettings
{
    
    public void Reset()
    {
        this.ChangeQuests = changeQuests;
        this.UseHighestResearched = useHighestResearched;
        this.UsePercentResearched = usePercentResearched;
        this.PercentResearchNeeded = percentResearchNeeded;
        this.UseActualTechLevel = useActualTechLevel;
        this.UseFixedTechRange = useFixedTechRange;
        this.FixedRange = fixedRange;
        this.NumTechsBehind = numTechsBehind;
        this.NumTechsAhead = numTechsAhead;
        this.EmpireIsAlwaysEligible = empireIsAlwaysEligible;
        this.MechanoidsAreAlwaysEligible = mechanoidsAreAlwaysEligible;
        this.DebugOutput = debugOutput;
        this.ChangeVoidTechLevel = ChangeVoidTechLevelDefault;
        VoidTechTechnologyLevel = (int)TechLevel.Spacer;
        N4EventTechnologyLevel = (int)TechLevel.Medieval;
        N4RaidTechnologyLevel = (int)TechLevel.Spacer;
    }


    public static int VoidTechTechnologyLevel = (int)TechLevel.Ultra;
    public static int N4EventTechnologyLevel = (int)TechLevel.Medieval;
    public static int N4RaidTechnologyLevel = (int)TechLevel.Spacer;
    

    public override void ExposeData()
    {
        Scribe_Values.Look<bool>(ref this.ChangeQuests, "changeQuests", changeQuests, false);
        Scribe_Values.Look<bool>(ref this.UseHighestResearched, "useHighestResearched", useHighestResearched, false);
        Scribe_Values.Look<bool>(ref this.UsePercentResearched, "usePercentResearched", usePercentResearched, false);
        Scribe_Values.Look<float>(ref this.PercentResearchNeeded, "percentResearchNeeded", percentResearchNeeded, false);
        Scribe_Values.Look<bool>(ref this.UseActualTechLevel, "useActualTechLevel", useActualTechLevel, false);
        Scribe_Values.Look<bool>(ref this.UseFixedTechRange, "useFixedTechRange", useFixedTechRange, false);
        Scribe_Values.Look<IntRange>(ref this.FixedRange, "fixedRange", fixedRange, false);
        Scribe_Values.Look<int>(ref this.NumTechsBehind, "numTechsBehind", numTechsBehind, false);
        Scribe_Values.Look<int>(ref this.NumTechsAhead, "numTechsAhead", numTechsAhead, false);
        Scribe_Values.Look<bool>(ref this.EmpireIsAlwaysEligible, "empireIsAlwaysEligible", empireIsAlwaysEligible, false);
        Scribe_Values.Look<bool>(ref this.MechanoidsAreAlwaysEligible, "mechanoidsAreAlwaysEligible", mechanoidsAreAlwaysEligible, false);
        Scribe_Values.Look<bool>(ref this.ChangeVoidTechLevel, "ChangeVoidStorytellerTechLevel", ChangeVoidTechLevelDefault, false);
        Scribe_Values.Look<int>(ref VoidTechTechnologyLevel, "VoidTechTechnologyLevel", (int)TechLevel.Spacer, false);
        Scribe_Values.Look<int>(ref N4EventTechnologyLevel, "VoidTechTechnologyLevel", (int)TechLevel.Medieval, false);
        Scribe_Values.Look<int>(ref N4RaidTechnologyLevel, "VoidTechTechnologyLevel", (int)TechLevel.Spacer, false);
        Scribe_Values.Look<bool>(ref this.DebugOutput, "debugOutput", debugOutput, false);
        base.ExposeData();
    }


    public static void WriteAll()
    {
        if (SettingsHelper.LatestVersion.UseHighestResearched)
        {
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
        }
        else if (SettingsHelper.LatestVersion.UsePercentResearched)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
        }
        else if (SettingsHelper.LatestVersion.UseActualTechLevel)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = false;
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
        }
        else if (SettingsHelper.LatestVersion.UseFixedTechRange)
        {
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseHighestResearched = false;
        }
        
        if (Current.Game != null)
        {
            IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
            IgnoranceBase.WarnIfNoFactions();
        }
    }

    public static string getTechLevelName(int techlevel)
    {
        if (techlevel == 0) return "";
        if (techlevel <= 0 || techlevel >= techLabels.Count) return "Unknown(" + techlevel + ")";
        return techLabels[techlevel].Translate();
    }
    

    public static void DrawSettings(Rect rect)
    {
        rect.xMin += 20f;
        rect.yMax -= 20f;
        Listing_Standard listing_Standard = new Listing_Standard(GameFont.Small);
        new Rect(rect.x, rect.y, rect.width, rect.height);
        Rect rect2 = new Rect(0f, 0f, rect.width - 30f, 1000f);
        Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
        listing_Standard.Begin(rect2);
        listing_Standard.Gap(12f);
        listing_Standard.Gap(12f);
        string text = "Your calculated tech level: ";
        if (SettingsHelper.LatestVersion.UseFixedTechRange)
        {
            text += "N/A (fixed tech range)";
        }
        else if (Current.Game != null)
        {
            text += Enum.GetName(typeof(TechLevel), IgnoranceBase.PlayerTechLevel);
        }
        else
        {
            text += "Not in game";
        }

        float height = listing_Standard.Label(text, -1f, null).height;
        if (Current.Game != null)
        {
            if (!SettingsHelper.LatestVersion.UseFixedTechRange)
            {
                TechLevel playerTechLevel = IgnoranceBase.PlayerTechLevel;
                listing_Standard.Label("Eligible hostile factions below your tech: " + FactionsToString(IgnoranceBase.FactionsBelow(playerTechLevel)), -1f, null);
                listing_Standard.Label("Eligible hostile factions equivalent to your tech: " + FactionsToString(IgnoranceBase.FactionsEqual(playerTechLevel)), -1f, null);
                listing_Standard.Label("Eligible hostile factions above your tech: " + FactionsToString(IgnoranceBase.FactionsAbove(playerTechLevel)), -1f, null);
            }
            else
            {
                listing_Standard.Label("Eligible, hostile factions in range: " + FactionsToString(IgnoranceBase.FactionsInRange()), -1f, null);
            }

            listing_Standard.Gap(12f);
            listing_Standard.GapLine(12f);
            if (!SettingsHelper.LatestVersion.UseFixedTechRange)
            {
                TechLevel playerTechLevel2 = IgnoranceBase.PlayerTechLevel;
                listing_Standard.Label("Eligible non-hostile factions below your tech: " + FactionsToString(IgnoranceBase.TraderFactionsBelow(playerTechLevel2)), -1f, null);
                listing_Standard.Label("Eligible non-hostile factions equivalent to your tech: " + FactionsToString(IgnoranceBase.TraderFactionsEqual(playerTechLevel2)), -1f, null);
                listing_Standard.Label("Eligible non-hostile factions above your tech: " + FactionsToString(IgnoranceBase.TraderFactionsAbove(playerTechLevel2)), -1f, null);
            }
            else
            {
                listing_Standard.Label("Eligible, non-hostile factions in range: " + FactionsToString(IgnoranceBase.TraderFactionsInRange()), -1f, null);
            }
        }

        listing_Standard.Gap(12f);
        listing_Standard.GapLine(12f);
        listing_Standard.CheckboxLabeled("Restrict quest threats", ref SettingsHelper.LatestVersion.ChangeQuests, "Substitute quest factions for a faction in range. Will not change the quest description, but an appropriate faction will arrive.", 0f, 1f);
        listing_Standard.GapLine(12f);
        listing_Standard.Label("Method by which this mod will calculate your tech level for raids and incidents:", -1f, null);
        bool flag = listing_Standard.RadioButton("Fixed range", SettingsHelper.LatestVersion.UseFixedTechRange, 0f, "Will not dynamically update with the game state", null);
        if (SettingsHelper.LatestVersion.UseFixedTechRange)
        {
            Widgets.IntRange(listing_Standard.GetRect(height, 1f).LeftPartPixels(450f), 999, ref SettingsHelper.LatestVersion.FixedRange, 1, 7, getTechLevelName(SettingsHelper.LatestVersion.FixedRange.min) + " - " + getTechLevelName(SettingsHelper.LatestVersion.FixedRange.max) , 0);
        }

        listing_Standard.Gap(12f);
        bool flag2 = listing_Standard.RadioButton("Highest tech researched", SettingsHelper.LatestVersion.UseHighestResearched, 0f, "If you have even one tech in a tech level researched, you will be considered that tech for the purpose of raids.", null);
        listing_Standard.Gap(12f);
        bool flag3 = listing_Standard.RadioButton("Tech completion of a certain percent", SettingsHelper.LatestVersion.UsePercentResearched, 0f, "Once you research a certain % of a tech level's available technologies, you will be considered that tech level for the purpose of raids.", null);
        if (SettingsHelper.LatestVersion.UsePercentResearched)
        {
            SettingsHelper.LatestVersion.PercentResearchNeeded = Mathf.Clamp(SettingsHelper.LatestVersion.PercentResearchNeeded, 0.05f, 1f);
            Rect rect3 = listing_Standard.GetRect(height, 1f).LeftPartPixels(450f);
            SettingsHelper.LatestVersion.PercentResearchNeeded = Widgets.HorizontalSlider(rect3, SettingsHelper.LatestVersion.PercentResearchNeeded, 0.05f, 1f, false, Mathf.RoundToInt(SettingsHelper.LatestVersion.PercentResearchNeeded * 100f).ToString() + "%", null, null, 0.05f);
        }
        else
        {
            listing_Standard.Gap(12f);
        }

        listing_Standard.Gap(12f);
        bool flag4 = listing_Standard.RadioButton("Actual colonist tech level", SettingsHelper.LatestVersion.UseActualTechLevel, 0f, "Not recommended unless you have a mod to increase your tech level somehow.", null);
        if (!SettingsHelper.LatestVersion.UseFixedTechRange)
        {
            listing_Standard.GapLine(12f);
            listing_Standard.Label("Please note that there are 7 tech levels in the game by default.", -1f, null);
            listing_Standard.Label("Also keep in mind that there are NO medieval factions in the vanilla game.", -1f, null);
            listing_Standard.Label("(This means tribal starts will only fight other tribes at the start with default settings)", -1f, null);
            listing_Standard.Gap(12f);
            listing_Standard.Label("Maximum difference between your calculated tech level and enemy faction's (-1 is any):", -1f, null);
            listing_Standard.Gap(12f);
            Rect rect4 = listing_Standard.GetRect(height, 1f).LeftPartPixels(450f);
            SettingsHelper.LatestVersion.NumTechsBehind = Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, (float)SettingsHelper.LatestVersion.NumTechsBehind, -1f, 7f, false, OffsetString(SettingsHelper.LatestVersion.NumTechsBehind) + " behind", null, null, 1f));
            listing_Standard.Gap(12f);
            Rect rect5 = listing_Standard.GetRect(height, 1f).LeftPartPixels(450f);
            SettingsHelper.LatestVersion.NumTechsAhead = Mathf.RoundToInt(Widgets.HorizontalSlider(rect5, (float)SettingsHelper.LatestVersion.NumTechsAhead, -1f, 7f, false, OffsetString(SettingsHelper.LatestVersion.NumTechsAhead) + " ahead", null, null, 1f));
        }

        if (flag2 && flag2 != SettingsHelper.LatestVersion.UseHighestResearched)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = true;
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
            if (Current.Game != null)
            {
                IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
            }
        }
        else if (flag3 && flag3 != SettingsHelper.LatestVersion.UsePercentResearched)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = false;
            SettingsHelper.LatestVersion.UsePercentResearched = true;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
            if (Current.Game != null)
            {
                IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
            }
        }
        else if (flag4 && flag4 != SettingsHelper.LatestVersion.UseActualTechLevel)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = false;
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = true;
            SettingsHelper.LatestVersion.UseFixedTechRange = false;
            if (Current.Game != null)
            {
                IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
            }
        }
        else if (flag && flag != SettingsHelper.LatestVersion.UseFixedTechRange)
        {
            SettingsHelper.LatestVersion.UseHighestResearched = false;
            SettingsHelper.LatestVersion.UsePercentResearched = false;
            SettingsHelper.LatestVersion.UseActualTechLevel = false;
            SettingsHelper.LatestVersion.UseFixedTechRange = true;
            if (Current.Game != null)
            {
                IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
            }
        }

        listing_Standard.Gap(12f);
        listing_Standard.GapLine(12f);
        listing_Standard.CheckboxLabeled("Empire is always eligible", ref SettingsHelper.LatestVersion.EmpireIsAlwaysEligible, "Will not prevent Empire from coming even if your tech level is low (you will be getting their traders if neutral/friends or raids otherwise)", 0f, 1f);
        listing_Standard.CheckboxLabeled("Mechanoids are always eligible", ref SettingsHelper.LatestVersion.MechanoidsAreAlwaysEligible, "Will not prevent Mechanoids from coming even if your tech level is low", 0f, 1f);
        if (LoadedModManager.RunningModsListForReading.Any(x => x.Name.EqualsIgnoreCase("[RH2] V.O.I.D. Storyteller") || x.Name.EqualsIgnoreCase("[RH2] Faction: V.O.I.D.")))
        {
            listing_Standard.CheckboxLabeled("Change V.O.I.D Events/Faction to Tech Level", ref SettingsHelper.LatestVersion.ChangeVoidTechLevel, "By Default this mod will not block V.O.I.D as they are set as a super low tech level!\nThis does not stop FORCED events such as capturing a prisoner or the starting event from happening!", 0f, 1f);
            VoidTechTechnologyLevel = (int)Math.Round(listing_Standard.SliderLabeled("V.O.I.D Actual Tech Level: " + (VoidTechTechnologyLevel == 0 ? "Always Eligable To Attack" : getTechLevelName(VoidTechTechnologyLevel)), VoidTechTechnologyLevel, (int)TechLevel.Animal, (int)TechLevel.Archotech, 0.5f, "The tech level to mark void events as"));
            N4EventTechnologyLevel = (int)Math.Round(listing_Standard.SliderLabeled("V.O.I.D N4 Events Tech Level: " + (N4EventTechnologyLevel == 0 ? "Always Eligable To Attack" : getTechLevelName(N4EventTechnologyLevel)), N4EventTechnologyLevel, (int)TechLevel.Animal, (int)TechLevel.Archotech, 0.5f, "The tech level to mark void N4 events as"));
            N4RaidTechnologyLevel = (int)Math.Round(listing_Standard.SliderLabeled("V.O.I.D N4 Raids Tech Level: " + (N4RaidTechnologyLevel == 0 ? "Always Eligable To Attack" : getTechLevelName(N4RaidTechnologyLevel)), N4RaidTechnologyLevel, (int)TechLevel.Animal, (int)TechLevel.Archotech, 0.5f, "The tech level to mark void N4 faction as"));
        }

        listing_Standard.CheckboxLabeled("Debug output? Don't use unless necessary", ref SettingsHelper.LatestVersion.DebugOutput, "May dump some extra data to console", 0f, 1f);
        listing_Standard.GapLine(12f);
        if (listing_Standard.ButtonText("Default Settings", null, 1f))
        {
            SettingsHelper.LatestVersion.Reset();
        }

        listing_Standard.GapLine(12f);
        listing_Standard.End();
        Widgets.EndScrollView();
    }


    public static string OffsetString(int num)
    {
        if (num < 0)
        {
            return "Any number";
        }

        return num.ToString();
    }


    public static string FactionsToString(IEnumerable<Faction> factions)
    {
        string text = "";
        foreach (Faction faction in factions)
        {
            if (text == "")
            {
                text = faction.Name;
            }
            else
            {
                text = text + ", " + faction.Name;
            }
        }

        if (text == "")
        {
            text = "[none]";
        }

        return text;
    }


    public Settings()
    {
    }


    // Note: this type is marked as 'beforefieldinit'.
    static Settings()
    {
    }


    private static Vector2 scrollPosition = Vector2.zero;


    private static readonly List<string> techLabels = new List<string>()
    {
        "",
        "Animal",
        "Neolithic",
        "Medieval",
        "Industrial",
        "Spacer",
        "Ultra",
        "Archotech"
    };


    private static readonly bool changeQuests = true;


    private static readonly bool useHighestResearched = false;


    private static readonly bool usePercentResearched = true;


    private static readonly float percentResearchNeeded = 0.75f;


    private static readonly bool useActualTechLevel = false;


    private static readonly bool useFixedTechRange = false;


    private static readonly IntRange fixedRange = new IntRange(1, 7);


    private static readonly int numTechsBehind = 1;


    private static readonly int numTechsAhead = 2;


    private static readonly bool empireIsAlwaysEligible = true;


    private static readonly bool mechanoidsAreAlwaysEligible = false;

    private static readonly bool ChangeVoidTechLevelDefault = true;
    
    
    private static readonly bool debugOutput = false;

    

    

    public bool ChangeQuests = changeQuests;


    public bool UseHighestResearched = useHighestResearched;


    public bool UsePercentResearched = usePercentResearched;


    public float PercentResearchNeeded = percentResearchNeeded;


    public bool UseActualTechLevel = useActualTechLevel;


    public bool UseFixedTechRange = useFixedTechRange;


    public IntRange FixedRange = fixedRange;


    public int NumTechsBehind = numTechsBehind;


    public int NumTechsAhead = numTechsAhead;


    public bool EmpireIsAlwaysEligible = empireIsAlwaysEligible;


    public bool MechanoidsAreAlwaysEligible = mechanoidsAreAlwaysEligible;

    public bool ChangeVoidTechLevel = ChangeVoidTechLevelDefault;


    public bool DebugOutput = debugOutput;
}
