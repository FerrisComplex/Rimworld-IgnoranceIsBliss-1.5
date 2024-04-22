using Verse;

namespace FerrisPatches.Modules;

public class NoWhiteHairSettings : SettingsModuleBase
{
    public static bool DisableHairColorChange = false;
  
    public override void DoTweakContents(Listing_Standard originalListing, string filter = "")
    {
        originalListing.DoSettingBool(filter, "Elder Trial Does not Change Hair Color", "(Requires Reboot) Makes Hair color not white after elder trial surgery", ref DisableHairColorChange);
    }

    public override void OnReset()
    {
        DisableHairColorChange = false;
    }


    public override void OnExposeData()
    {
        Look(ref DisableHairColorChange, "DisableHairColorChange", false);
    }
}
