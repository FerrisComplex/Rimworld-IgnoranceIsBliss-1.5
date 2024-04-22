using Verse;

namespace FerrisPatches.Modules;

public class VEF_PipeSettings : SettingsModuleBase
{
    public static bool InfiniteDeepWellResources = false;
    public static bool InfiniteDrillResources = false;
  
    public override void DoTweakContents(Listing_Standard originalListing, string filter = "")
    {
        originalListing.DoSettingBool(filter, "Pipe Resources are Infinate", "All DeepWell Resources are now Inifinte", ref InfiniteDeepWellResources);
        originalListing.DoSettingBool(filter, "Drill Resources are Infinate", "All Drill Resources from VanillaExpanded Drills are now Infinite", ref InfiniteDrillResources);
    }

    public override void OnReset()
    {
        InfiniteDrillResources = false;
        InfiniteDrillResources = false;
    }


    public override void OnExposeData()
    {
        Look(ref InfiniteDeepWellResources, "InfiniteDeepWellResources", false);
        Look(ref InfiniteDrillResources, "InfiniteDrillResources", false);
    }
}
