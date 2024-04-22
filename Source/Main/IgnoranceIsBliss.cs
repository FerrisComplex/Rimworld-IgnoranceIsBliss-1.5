using DFerrisIgnorance;
using UnityEngine;
using Verse;

namespace DIgnoranceIsBliss
{
    
    internal class IgnoranceMod : Mod
    {
        
        public IgnoranceMod(ModContentPack content) : base(content)
        {
            SettingsHelper.RegisterSettingModules(); // Register our modules first!
            SettingsHelper.LatestVersion = (base.GetSettings<Settings>() ?? new Settings());
        }

        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DrawSettings(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        
        public override string SettingsCategory()
        {
            return "Ignorance Is Bliss";
        }

        
        public override void WriteSettings()
        {
            foreach (var allCategory in SettingsHelper.AllCategories)
                allCategory.OnPreSave();
            foreach (var allCategory in SettingsHelper.AllCategories)
                allCategory.OnDoSave();
            base.WriteSettings();
            foreach (var allCategory in SettingsHelper.AllCategories)
                allCategory.OnPostSave();
        }
    }
}
