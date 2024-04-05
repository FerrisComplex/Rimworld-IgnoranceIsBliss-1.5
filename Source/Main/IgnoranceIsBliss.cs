using UnityEngine;
using Verse;

namespace DIgnoranceIsBliss
{
    
    internal class IgnoranceMod : Mod
    {
        
        public IgnoranceMod(ModContentPack content) : base(content)
        {
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
            Settings.WriteAll();
            base.WriteSettings();
        }
    }
}
