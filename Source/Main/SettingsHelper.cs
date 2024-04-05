using System;

namespace DIgnoranceIsBliss
{
    
    internal static class SettingsHelper
    {
        
        public static void Reset()
        {
            SettingsHelper.LatestVersion.Reset();
        }

        
        public static Settings LatestVersion;
    }
}
