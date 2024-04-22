using System;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public abstract class SettingsModuleBase
{
    public void Look<T>(ref T value, string label, T defaultValue = default(T), bool forceSave = false) => Scribe_Values.Look(ref value, "DFerrisIgnorance_" + (_cachedModuleCatgory.NullOrEmpty() ? "MissingCategory_" : _cachedModuleCatgory.Replace(" ", "") + "_") + label.Replace(" ", ""), defaultValue, forceSave);

    
    
    public bool IsModded { get; private set; }
    public int DrawOrder { get; private set; }
    private SectionDef _sectionDefHolder = null;
    private readonly string _cachedModuleCatgory;

    public SectionDef BuildSingletonSection(string sectionName, string sectionTooltip = "")
    {
        if (_sectionDefHolder != null) return _sectionDefHolder;
        var section = new SectionDef(sectionName, sectionTooltip);
        section.AddTweak(this);
        _sectionDefHolder = section;
        return section;
    }

    public SettingsModuleBase()
    {
        _cachedModuleCatgory = this.GetType().Name;;
    }
    
    public virtual void OnDoSave() {}
    public virtual bool IsUsable() { return true; }
    public virtual void OnGameInitialization() {}
    public virtual void OnMapInitialization() {}
    public virtual void OnPreSave() {}
    public virtual void OnPostSave(){}
    
    public abstract void DoTweakContents(Listing_Standard originalListing, string filter = "");
    public abstract void OnReset();
    public abstract void OnExposeData();
}
