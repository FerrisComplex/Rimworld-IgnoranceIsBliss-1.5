using System.Collections.Generic;
using DFerrisIgnorance.Compatability;
using DIgnoranceIsBliss;
using DIgnoranceIsBliss.Core_Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance.Modules;

public class TechnologyLevelSettings : CategoryDef
{
    private EventSettings eventSettings = new EventSettings();
    private FactionSettings factionSettings = new FactionSettings();
    private QuestSettings questSettings = new QuestSettings();


    public TechnologyLevelSettings(string name, string tooltip = "", RequirementData requirements = null) : base(name, tooltip, requirements)
    {
    }

    private static readonly List<string> techLabels = new List<string>
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

    public static bool UseHighestResearched => SelectedTechMethod == 1;
    public static bool UsePercentResearched => SelectedTechMethod == 2;
    public static bool UseActualTechLevel => SelectedTechMethod == 3;
    public static bool UseFixedTechRange => SelectedTechMethod == 0;

    public static string getTechLevelName(int techlevel)
    {
        if (techlevel == 0) return "";
        if (techlevel <= 0 || techlevel >= techLabels.Count) return "Unknown(" + techlevel + ")";
        return techLabels[techlevel].Translate();
    }

    private static string OffsetString(int num)
    {
        if (num < 0) return "Any number";
        return num.ToString();
    }

    private static string FactionsToString(IEnumerable<Faction> factions)
    {
        string text = "";
        foreach (Faction faction in factions)
            text += (text.NullOrEmpty() ? "" : ", ") + faction.Name;
        if (text.NullOrEmpty()) return "[none]";
        return text;
    }


    private static int SelectedTechMethod = 0;
    public static float PercentResearchNeeded = 0.75f;
    public static IntRange FixedRange = new IntRange(1, 7);
    public static int NumTechsBehind = 1;
    public static int NumTechsAhead = 2;


    public override void OnDoSave()
    {
        this.eventSettings.OnDoSave();
        this.factionSettings.OnDoSave();
        this.questSettings.OnDoSave();
    }
    
    public override void OnPreSave()
    {
        this.eventSettings.OnPreSave();
        this.factionSettings.OnPreSave();
        this.questSettings.OnPreSave();
    }
    
    public override void OnPostSave()
    {
        this.eventSettings.OnPostSave();
        this.factionSettings.OnPostSave();
        this.questSettings.OnPostSave();
    }
    
    public override void OnGameInitialization()
    {
        this.eventSettings.OnGameInitialization();
        this.factionSettings.OnGameInitialization();
        this.questSettings.OnGameInitialization();
    }
    
    public override void OnMapInitialization()
    {
        // Looping queue to ensure our techlevel is correct
        if (!hasQueued)
        {
            hasQueued = true;
            TechnologyQueueSetup();
        }
        
        // Add a delay to loading the techlevel to allow for techadvancing to actually set the tech level first
        Ferris.QueueHelper.AddAction(() =>
        {
            if (Current.Game != null && !UseHighestResearched) IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
        }, 60);
        
        
        
        this.eventSettings.OnMapInitialization();
        this.factionSettings.OnMapInitialization();
        this.questSettings.OnMapInitialization();
    }

    private static bool hasQueued = false;
    private static void TechnologyQueueSetup()
    {
        if (Current.Game != null && UseActualTechLevel) IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
        Ferris.QueueHelper.AddAction(TechnologyQueueSetup, 2500);
    }
    

    public override void DoCategoryContents(Listing_Standard originalListing, string filter)
    {
        var calculatedTextLevel = "";
        if (UseFixedTechRange) calculatedTextLevel = "N/A (Fixed Tech Range)";
        else if (Current.Game == null) calculatedTextLevel = "N/A (Not in Game!)";
        else calculatedTextLevel = getTechLevelName((int)IgnoranceBase.PlayerTechLevel);

        originalListing.Label("Your calculated tech level: " + calculatedTextLevel);
        if (Current.Game != null)
        {
            var level = IgnoranceBase.PlayerTechLevel;
            if (!UseFixedTechRange)
            {
                originalListing.Label("Eligable hostile factions below your tech: " + FactionsToString(IgnoranceBase.FactionsBelow(level)));
                originalListing.Label("Eligible hostile factions equivalent to your tech: " + FactionsToString(IgnoranceBase.FactionsEqual(level)));
                originalListing.Label("Eligible hostile factions above your tech: " + FactionsToString(IgnoranceBase.FactionsAbove(level)));
            }
            else
            {
                originalListing.Label("Eligible hostile factions above your tech: " + FactionsToString(IgnoranceBase.FactionsInRange()));
            }

            originalListing.Gap();
            originalListing.GapLine();


            if (!UseFixedTechRange)
            {
                originalListing.Label("Eligible non-hostile factions below your tech: " + FactionsToString(IgnoranceBase.TraderFactionsBelow(level)));
                originalListing.Label("Eligible non-hostile factions equivalent to your tech: " + FactionsToString(IgnoranceBase.TraderFactionsEqual(level)));
                originalListing.Label("Eligible non-hostile factions above your tech: " + FactionsToString(IgnoranceBase.TraderFactionsAbove(level)));
            }
            else
            {
                originalListing.Label("Eligible, non-hostile factions in range: " + FactionsToString(IgnoranceBase.TraderFactionsInRange()));
            }
        }
        else
        {
            if (!UseFixedTechRange)
            {
                originalListing.Label("Eligable hostile factions below your tech: " + "[N/A (Not In Game)]");
                originalListing.Label("Eligible hostile factions equivalent to your tech: " + "[N/A (Not In Game)]");
                originalListing.Label("Eligible hostile factions above your tech: " + "[N/A (Not In Game)]");
            }
            else
            {
                originalListing.Label("Eligible hostile factions above your tech: " + "[N/A (Not In Game)]");
            }

            originalListing.Gap();
            originalListing.GapLine();


            if (!UseFixedTechRange)
            {
                originalListing.Label("Eligible non-hostile factions below your tech: " + "[N/A (Not In Game)]");
                originalListing.Label("Eligible non-hostile factions equivalent to your tech: " + "[N/A (Not In Game)]");
                originalListing.Label("Eligible non-hostile factions above your tech: " + "[N/A (Not In Game)]");
            }
            else
            {
                originalListing.Label("Eligible, non-hostile factions in range: " + "[N/A (Not In Game)]");
            }
        }

        originalListing.Gap();
        originalListing.GapLine();


        List<EnhancedListingStandard.RadioButtonLabel<int>> methods = new List<EnhancedListingStandard.RadioButtonLabel<int>>();

        methods.Add(new EnhancedListingStandard.RadioButtonLabel<int>("Fixed Range", 0, "Will not dynamically update with the game state", (x) =>
        {
            x.DoSettingIntRange(filter, getTechLevelName(FixedRange.min) + " - " + getTechLevelName(FixedRange.max), "The range of your allowed events to happen", ref FixedRange, 1, 7);
            originalListing.Gap();
        }));

        methods.Add(new EnhancedListingStandard.RadioButtonLabel<int>("Highest Tech Researched", 1, "If you have even one tech in a tech level researched, you will be considered that tech for the purpose of raids."));
        methods.Add(new EnhancedListingStandard.RadioButtonLabel<int>("Tech completion of a certain percent", 2, "Once you research a certain % of a tech level's available technologies, you will be considered that tech level for the purpose of raids.", (x) =>
        {
            x.DoSettingFloat(null, "Completion Percentage per Level:", "", ref PercentResearchNeeded, 0.05f, 1f, 0.0001f, true);
            originalListing.Gap();
        }));
        
        methods.Add(new EnhancedListingStandard.RadioButtonLabel<int>("Actual Colonist Tech Level", 3, "This requires another mod to edit your tech level\nI hugely recommend Tech Advancing"));
        originalListing.AddLabeledRadioList("Method by which this mod will calculate your tech level for raids and incidents", methods, ref SelectedTechMethod);

        if (!UseFixedTechRange)
        {
            originalListing.Gap();
            originalListing.GapLine();
            originalListing.Label("Please note that there are 7 tech levels in the game by default.");
            originalListing.Label("Also keep in mind that there are NO medieval factions in the vanilla game.");
            originalListing.Label("(This means tribal starts will only fight other tribes at the start with default settings)");
            originalListing.Gap();
            originalListing.Label("Maximum difference between your calculated tech level and enemy faction's (-1 is any):");
            originalListing.DoSettingInt(filter, OffsetString(NumTechsBehind) + " behind", "", ref NumTechsBehind, -1, 7, null, true);
            originalListing.DoSettingInt(filter, OffsetString(NumTechsAhead) + " ahead", "", ref NumTechsAhead, -1, 7, null, true);
        }

        if (!UseHighestResearched && Current.Game != null)
            IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();

        originalListing.Gap();
        originalListing.GapLine();
        
        // Faction Handlers
        factionSettings.DoTweakContents(originalListing, filter);
        
        originalListing.Gap();
        originalListing.GapLine();
        
        // Quest Handlers (same as eventSettings)
        questSettings.DoTweakContents(originalListing, filter);
        
        originalListing.Gap();
        originalListing.GapLine();
        // Event/Incident Handleres
        eventSettings.DoTweakContents(originalListing, filter);

        // Anything extra
        if (!this.HeldSections.NullOrEmpty())
        {
            foreach (SectionDef def in this.HeldSections)
                if (def.MatchesFilter(filter))
                    originalListing.DoSection(def, filter);
        }
    }
    
    
    public override void OnExposeData()
    {
        Look(ref SelectedTechMethod, "SelectedTechMethod", 0);
        Look(ref PercentResearchNeeded, "PercentResearchNeeded", 0.75f);
        Look(ref FixedRange, "FixedRange", new IntRange(1,7));
        Look(ref NumTechsBehind, "NumTechsBehind", 1);
        Look(ref NumTechsAhead, "NumTechsAhead", 2);
        
        this.eventSettings.OnExposeData();
        this.factionSettings.OnExposeData();
        this.questSettings.OnExposeData();
    }
}
