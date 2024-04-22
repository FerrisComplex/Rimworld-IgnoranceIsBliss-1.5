using UnityEngine;
using Verse;

namespace DFerrisIgnorance;

internal class Settings : ModSettings
{
    

    public static bool DebugLog = false;
    

    
    public override void ExposeData()
    {
       
        Scribe_Values.Look<bool>(ref DebugLog, "DFerrisIgnorance_Core_DebugLog", false, false);
        SettingsHelper.OnExposeData();
        base.ExposeData();
        
       
    }
    
    


    public static void DrawSettings(Rect rect)
    {
        SettingsHelper.DoSettingsWindowContents(rect);
    }
    

    


    
    
}
