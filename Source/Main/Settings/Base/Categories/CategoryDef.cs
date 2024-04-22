using System;
using System.Collections.Generic;
using DFerrisIgnorance.Compatability;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

public class CategoryDef
{
    public void Look<T>(ref T value, string label, T defaultValue = default(T), bool forceSave = false) => Scribe_Values.Look(ref value, "DFerrisIgnorance_" + (Name.NullOrEmpty() ? "MissingCategory_" : Name.Replace(" ", "") + "_") + label.Replace(" ", ""), defaultValue, forceSave);

    
    public string Name { get; private set; }
    public Texture2D IconTexture => Requirements == null ? null : Requirements.Icon;
    public string IconLocation => Requirements == null ? null : Requirements.IconTextureLocation;
    public string Tooltip { get; private set; }
    private readonly bool _isUsableRaw;
    public readonly List<SectionDef> HeldSections = new List<SectionDef>();
    public int OrderID = 9999;
    public readonly RequirementData Requirements;
    
    public CategoryDef(string name, string tooltip = "", RequirementData requirements = null)
    {
        Name = name;
        Tooltip = tooltip == null ? "" : tooltip;
        _isUsableRaw = requirements == null || requirements.EnsureValid();
        Requirements = requirements;
    }


    public FloatMenuOption GetMenuWithIcon(Action action)
    {
        if (IconTexture != null) return new FloatMenuOption(Name, action, IconTexture, Color.white);    
        
        return new FloatMenuOption(Name, action, MenuOptionPriority.Default);
    }
    
    

    public CategoryDef AddSection(SectionDef section)
    {
        HeldSections.Add(section);
        return this;
    }
    
    public bool IsUsable() => _isUsableRaw && HeldSections.Any(x => x.IsUsable());
    public CategoryDef SetOrderId(int id)
    {
        OrderID = id;
        return this;
    }


    public virtual void OnPreSave()
    {
        foreach (var v in HeldSections)
            v.OnPreSave();
    }
    public virtual void OnDoSave()
    {
        foreach (var v in HeldSections)
            v.OnDoSave();
    }
    public virtual void OnPostSave()
    {
        foreach (var v in HeldSections)
            v.OnPostSave();
    }
    
    public virtual void OnExposeData()
    {
        foreach (var v in HeldSections)
            v.OnExposeData();
    }
    
    public virtual void OnGameInitialization()
    {
        foreach (var v in HeldSections)
            v.OnGameInitialization();
    }
    
    public virtual void OnMapInitialization()
    {
        foreach (var v in HeldSections)
            v.OnMapInitialization();
    }

   

    public virtual void DoCategoryContents(Listing_Standard listing, string filter)
    {
        if (!this.HeldSections.NullOrEmpty())
        {
            foreach (SectionDef def in this.HeldSections)
                if (def.MatchesFilter(filter))
                    listing.DoSection(def, filter);
        }
    }
}
