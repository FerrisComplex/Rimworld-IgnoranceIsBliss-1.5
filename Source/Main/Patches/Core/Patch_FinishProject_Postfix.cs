using System;
using HarmonyLib;
using RimWorld;

namespace DIgnoranceIsBliss.Core_Patches
{
    internal class Patch_FinishProject_Postfix
    {
        public static void Postfix()
        {
            IgnoranceBase.PlayerTechLevel = IgnoranceBase.GetPlayerTech();
        }
    }
}
