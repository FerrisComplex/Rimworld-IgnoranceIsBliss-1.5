using System;
using System.Linq;
using DFerrisIgnorance;
using DIgnoranceIsBliss;
using RimWorld;
using Verse;

namespace IgnoranceIsBliss.Patches.Core;

public class Patch_OnInit
{
    public static void RegisterPatches()
    {
        Ferris.PatchHelper.RegisterPostfixPatch(typeof(ScribeLoader), "FinalizeLoading", null, typeof(Patch_OnInit));
    }

    public static void Postfix()
    {
        SettingsHelper.OnMapInitialization();
    }
}
