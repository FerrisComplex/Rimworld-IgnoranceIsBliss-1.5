using System.Collections.Generic;
using DFerrisIgnorance.Compatability;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public class SubSectionDef
{
    
    public List<SettingsModuleBase> HeldTweaks = new List<SettingsModuleBase>();
    public readonly List<SubSectionDef> HeldSubSections = new List<SubSectionDef>();
    public string Name { get; private set; }
    public string Tooltip { get; private set; }
    public Texture2D IconTexture => Requirements == null ? null : Requirements.Icon;
    public string IconLocation => Requirements == null ? null : Requirements.IconTextureLocation;
    
    public readonly RequirementData Requirements;
    
    private readonly bool _isUsableRaw;
    public SubSectionDef(string name, string tooltip = "", RequirementData requirements = null)
    {
        Name = name;
        Tooltip = tooltip == null ? "" : tooltip;
        _isUsableRaw = requirements == null || requirements.EnsureValid();
        Requirements = requirements;
    }
    
    public SubSectionDef AddTweak(SettingsModuleBase tweak)
    {
        HeldTweaks.Add(tweak);
        return this;
    }
    

    public SubSectionDef AddSubSection(SubSectionDef section)
    {
        if(section != this)
            HeldSubSections.Add(section);
        return this;
    }
    
    public void OnPreSave()
    {
        foreach (var v in HeldTweaks)
            v.OnPreSave();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnPreSave();
    }
    public void OnDoSave()
    {
        foreach (var v in HeldTweaks)
            v.OnDoSave();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnDoSave();
    }
    public void OnPostSave()
    {
        foreach (var v in HeldTweaks)
            v.OnPostSave();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnPostSave();
    }
    
    public void OnExposeData()
    {
        foreach (var v in HeldTweaks)
            v.OnExposeData();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnExposeData();
    }
    
    public virtual void OnGameInitialization()
    {
        foreach (var v in HeldTweaks)
            v.OnGameInitialization();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnGameInitialization();
    }
    
    public virtual void OnMapInitialization()
    {
        foreach (var v in HeldTweaks)
            v.OnMapInitialization();
        foreach (var v in HeldSubSections)
            if(v != this)
                v.OnMapInitialization();
    }

    
    public bool IsUsable() => _isUsableRaw && HeldTweaks.Any(x => x.IsUsable());
    public bool MatchesFilter(string filter) => filter.NullOrEmpty() || Name.ToLower().Contains(filter) || Tooltip.ToLower().Contains(filter);

    public virtual void DoSectionContents(Listing_Standard listing, string filter)
    {
        if (listing.ButtonTextLabeled("", "Restore Section Defaults", TextAnchor.UpperLeft, null, null))
        {
            this.DoSectionRestore();
            Messages.Message(Name + " tweaks restored to defaults. Restart required to take full effect!", MessageTypeDefOf.CautionInput, true);
        }

        if (!HeldTweaks.NullOrEmpty())
        {
            foreach(var tweakDef in HeldTweaks)
                if (tweakDef.IsUsable())
                    tweakDef.DoTweakContents(listing, filter);
        }

        if (!HeldSubSections.NullOrEmpty())
        {
            foreach(var subSectionDef in HeldSubSections)
                if (subSectionDef != this && subSectionDef.IsUsable() && subSectionDef.MatchesFilter(filter))
                    listing.DoSubSection(subSectionDef, filter);
        }
    }
    
    public void DoSectionRestore()
    {
        
        foreach(var v in HeldTweaks)
            v.OnReset();
        foreach(var v in HeldSubSections)
            if(v != this && v.IsUsable())  
                v.DoSectionRestore();
    }
}
