using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DIgnoranceIsBliss.Core_Patches
{
	internal class Patch_UsableIncidentsInCategory_Postfix
	{
		public static void Postfix(ref IEnumerable<IncidentDef> __result)
		{
			if (__result == null) return;
			__result = from x in __result where x != null && ((!IgnoranceBase.incidentWorkers.ContainsKey(x.workerClass) || IgnoranceBase.TechIsEligibleForIncident(IgnoranceBase.incidentWorkers.TryGetValue(x.workerClass))) && (IgnoranceBase.IsTechEligableForEvent(x.defName))) select x;
		}
	}
}
