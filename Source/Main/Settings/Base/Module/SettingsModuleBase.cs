using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public abstract class SettingsModuleBase
{
    public void Look<T>(ref T value, string label, T defaultValue = default(T), bool forceSave = false) => Scribe_Values.Look(ref value, "DFerrisIgnorance_" + (_cachedModuleCatgory.NullOrEmpty() ? "MissingCategory_" : _cachedModuleCatgory.Replace(" ", "") + "_") + label.Replace(" ", ""), defaultValue, forceSave);
    public void LookDictionary<T,V>(ref Dictionary<T,V> value, string label, LookMode keyLookMode = LookMode.Value, LookMode valueLookMode = LookMode.Value)
    {
        // Null checks to avoid it leaving the result as null
        if (value == null) value = new Dictionary<T, V>();
        Scribe_Collections.Look(ref value, "DFerrisIgnorance_" + (_cachedModuleCatgory.NullOrEmpty() ? "MissingCategory_" : _cachedModuleCatgory.Replace(" ", "") + "_") + label.Replace(" ", ""), keyLookMode, valueLookMode);
        if (value == null) value = new Dictionary<T, V>();
    }

    public void LookList<T>(ref List<T> value, string label, LookMode mode = LookMode.Value)
    {
        if (value == null) value = new List<T>();
        Scribe_Collections.Look(ref value, "DFerrisIgnorance_" + (_cachedModuleCatgory.NullOrEmpty() ? "MissingCategory_" : _cachedModuleCatgory.Replace(" ", "") + "_") + label.Replace(" ", ""), mode);
        if (value == null) value = new List<T>();
    }
    
    
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
