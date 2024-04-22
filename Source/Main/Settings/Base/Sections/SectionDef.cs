using System;
using System.Collections.Generic;
using System.Linq;
using DFerrisIgnorance.Compatability;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public class SectionDef
{
    
    public string Name { get; private set; }
    public string Tooltip { get; private set; }
    public Texture2D IconTexture => Requirements == null ? null : Requirements.Icon;
    public string IconLocation => Requirements == null ? null : Requirements.IconTextureLocation;
    
    public readonly RequirementData Requirements;
    public readonly List<SettingsModuleBase> HeldTweaks = new List<SettingsModuleBase>();
    public readonly List<SubSectionDef> HeldSubSections = new List<SubSectionDef>();
    private bool _sortTweaks = true;
    private readonly bool _isUsableRaw;

    public SectionDef(string name, string tooltip = "", RequirementData requirements = null)
    {
        Name = name;
        Tooltip = tooltip == null ? "" : tooltip;
        _isUsableRaw = requirements == null || requirements.EnsureValid();
        Requirements = requirements;
    }
    
    public SectionDef AddTweak(SettingsModuleBase tweak)
    {
        HeldTweaks.Add(tweak);
        return this;
    }
    

    public SectionDef AddSubSection(SubSectionDef section)
    {
        HeldSubSections.Add(section);
        return this;
    }
    
    public SectionDef SetSorting(bool enabled = true)
    {
        this._sortTweaks = enabled;
        return this;
    }

    public bool IsUsable() => _isUsableRaw && HeldTweaks.Any(x => x.IsUsable());
    public bool MatchesFilter(string filter) => filter.NullOrEmpty() || Name.ToLower().Contains(filter) || Tooltip.ToLower().Contains(filter);

    public void DoSectionRestore()
    {
        foreach(var v in HeldTweaks)
            v.OnReset();
        foreach(var v in HeldSubSections)
            if(v.IsUsable())
                v.DoSectionRestore();
    }

    public void OnPreSave()
    {
        foreach (var v in HeldTweaks)
            v.OnPreSave();
        foreach (var v in HeldSubSections)
            v.OnPreSave();
    }
    public void OnDoSave()
    {
        foreach (var v in HeldTweaks)
            v.OnDoSave();
        foreach (var v in HeldSubSections)
            v.OnDoSave();
    }
    public void OnPostSave()
    {
        foreach (var v in HeldTweaks)
            v.OnPostSave();
        foreach (var v in HeldSubSections)
            v.OnPostSave();
    }

    public void OnExposeData()
    {
        foreach (var v in HeldTweaks)
            v.OnExposeData();
        foreach (var v in HeldSubSections)
            v.OnExposeData();
    }
    
    public virtual void OnGameInitialization()
    {
        foreach (var v in HeldTweaks)
            v.OnGameInitialization();
        foreach (var v in HeldSubSections)
                v.OnGameInitialization();
    }
    
    public virtual void OnMapInitialization()
    {
        foreach (var v in HeldTweaks)
            v.OnMapInitialization();
        foreach (var v in HeldSubSections)
                v.OnMapInitialization();
    }
    
    public virtual void DoSectionContents(Listing_Standard listing, string filter)
    {
        if (listing.ButtonTextLabeled("", "Restore Section Defaults", TextAnchor.UpperLeft, null, null))
        {
            this.DoSectionRestore();
            Messages.Message(Name + " tweaks restored to defaults. Restart required to take full effect!", MessageTypeDefOf.CautionInput, true);
        }

        if (!HeldTweaks.NullOrEmpty())
        {
            var list = !_sortTweaks ? this.HeldTweaks : this.HeldTweaks.OrderBy(t => t.DrawOrder).ToList();
            for (int k = 0; k < list.Count; k++)
            {
                var tweakDef = list[k];
                if (tweakDef.IsUsable())
                    tweakDef.DoTweakContents(listing, filter);
            }
        }

        if (!HeldSubSections.NullOrEmpty())
        {
            for (int j = 0; j < HeldSubSections.Count; j++)
            {
                var SubSectionDef = HeldSubSections[j];
                if (SubSectionDef.IsUsable() && SubSectionDef.MatchesFilter(filter))
                    listing.DoSubSection(SubSectionDef, filter);
            }
        }
    }
    
}
