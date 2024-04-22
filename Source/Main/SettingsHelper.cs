using System;
using System.Collections.Generic;
using System.Linq;
using DFerrisIgnorance.Compatability;
using DFerrisIgnorance.Modules;
using DIgnoranceIsBliss;
using RimWorld;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance
{ internal static class SettingsHelper
    {
        public static readonly List<CategoryDef> AllCategories = new List<CategoryDef>();
        public static CategoryDef CurrentCategory = null;
        public static Settings LatestVersion;

        public static CategoryDef VanillaCategory;
                
        public static QuickSearchWidget quickSearchWidget = new QuickSearchWidget();
        public static Vector2 optionsScrollPosition;
        public static float optionsViewRectHeight;
        public static string filter;


        public static void OnMapInitialization()
        {
            foreach(var v in AllCategories)
                v.OnMapInitialization();
        }

        public static void OnGameInitialization()
        {
            foreach(var v in AllCategories)
                v.OnGameInitialization();
        }
        
        public static void RegisterSettingModules()
        {

            D.Text("Attempting to register settings");
            if (AllCategories.Any()) return;

            VanillaCategory = new TechnologyLevelSettings("Ignorance is Bliss", "Settings for Ignorance is bliss, no restarts are required for anything in this mod!");
            AllCategories.Add(VanillaCategory);
            CurrentCategory = VanillaCategory;
        }

        public static void OnExposeData()
        {
            foreach (var allCategory in SettingsHelper.AllCategories)
                allCategory.OnExposeData();
        }
        

        public static void DoSettingsWindowContents(Rect inRect)
        {
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
            Widgets.BeginScrollView(inRect, ref optionsScrollPosition, viewRect, true);
            Listing_Standard listing_Standard = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
            listing_Standard.Begin(rect);
            DoOptionsCategoryContents(listing_Standard);
            optionsViewRectHeight = listing_Standard.CurHeight;
            listing_Standard.End();
            Widgets.EndScrollView();
        }

        private static bool ensureSafety()
        {
            if (CurrentCategory == null) CurrentCategory = (VanillaCategory == null ? AllCategories.FirstOrDefault() : VanillaCategory);
            return CurrentCategory != null;
        }
        
        private static void DoOptionsCategoryContents(Listing_Standard listing)
        {
            
            
            if (ensureSafety())
            {
                listing.SettingsCategoryDropdown("Current Page", "Setting descriptions are in tooltips. The Searchbar only works for the currently selected page.", ref CurrentCategory, listing.ColumnWidth);
                Rect rect = listing.GetRect(30f, 1f);
                quickSearchWidget.OnGUI(rect, null, null);
                filter = quickSearchWidget.filter.Text.ToLower();
                listing.GapLine(12f);
                CurrentCategory.DoCategoryContents(listing, filter);
            }
        }
        
    }
}
