using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace DIgnoranceIsBliss.Core_Patches
{
    [StaticConstructorOnStartup]
    public static class IgnoranceBase
    {
        public static TechLevel PlayerTechLevel
        {
            get
            {
                if (IgnoranceBase.cachedTechLevel == TechLevel.Undefined)
                {
                    IgnoranceBase.cachedTechLevel = IgnoranceBase.GetPlayerTech();
                }

                return IgnoranceBase.cachedTechLevel;
            }
            set
            {
                if (IgnoranceBase.cachedTechLevel == TechLevel.Undefined)
                {
                }

                IgnoranceBase.cachedTechLevel = value;
            }
        }


        static IgnoranceBase()
        {
            foreach (ResearchProjectDef researchProjectDef in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (!IgnoranceBase.strataDic.ContainsKey(researchProjectDef.techLevel))
                {
                    IgnoranceBase.strataDic.Add(researchProjectDef.techLevel, new List<ResearchProjectDef>());
                }

                IgnoranceBase.strataDic[researchProjectDef.techLevel].Add(researchProjectDef);
            }
        }


        public static void WarnIfNoFactions()
        {
            if (IgnoranceBase.NumFactionsInRange() <= 0)
            {
                Messages.Message("DNoValidFactions".Translate(), null, MessageTypeDefOf.NegativeEvent, false);
            }
        }


        public static TechLevel GetPlayerTech()
        {
            if (SettingsHelper.LatestVersion.UseHighestResearched)
            {
                for (int i = 7; i > 0; i--)
                {
                    if (IgnoranceBase.strataDic.ContainsKey((TechLevel)i))
                    {
                        using (List<ResearchProjectDef>.Enumerator enumerator = IgnoranceBase.strataDic[(TechLevel)i].GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.IsFinished)
                                {
                                    return (TechLevel)i;
                                }
                            }
                        }
                    }
                }

                return TechLevel.Animal;
            }

            if (SettingsHelper.LatestVersion.UsePercentResearched)
            {
                int num = 0;
                for (int j = 7; j > 0; j--)
                {
                    if (IgnoranceBase.strataDic.ContainsKey((TechLevel)j))
                    {
                        using (List<ResearchProjectDef>.Enumerator enumerator = IgnoranceBase.strataDic[(TechLevel)j].GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.IsFinished)
                                {
                                    num++;
                                }
                            }
                        }

                        if ((float)num / (float)IgnoranceBase.strataDic[(TechLevel)j].Count >= SettingsHelper.LatestVersion.PercentResearchNeeded)
                        {
                            return (TechLevel)j;
                        }
                    }
                }

                return TechLevel.Animal;
            }

            return Faction.OfPlayer.def.techLevel;
        }


        public static IEnumerable<Faction> HostileFactions()
        {
            return Find.FactionManager.AllFactions.Where(delegate(Faction f)
            {
                if (!f.IsPlayer && !f.defeated && f.HostileTo(Faction.OfPlayer) && f.def.pawnGroupMakers != null)
                {
                    return f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat);
                }

                return false;
            });
        }


        public static IEnumerable<Faction> NonHostileFactions()
        {
            return Find.FactionManager.AllFactions.Where(delegate(Faction f)
            {
                if (!f.IsPlayer && !f.defeated && !f.HostileTo(Faction.OfPlayer) && f.def.pawnGroupMakers != null)
                {
                    return f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat);
                }

                return false;
            });
        }


        public static Faction GetRandomEligibleFaction()
        {
            IEnumerable<Faction> enumerable = from f in IgnoranceBase.HostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f))
                select f;
            if (enumerable != null && enumerable.Count<Faction>() > 0)
                return enumerable.RandomElement<Faction>();
            

            return null;
        }


        public static IEnumerable<Faction> FactionsInRange()
        {
            return from f in IgnoranceBase.HostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f))
                select f;
        }


        public static IEnumerable<Faction> TraderFactionsInRange()
        {
            return from f in IgnoranceBase.NonHostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f))
                select f;
        }


        public static int NumFactionsInRange()
        {
            return IgnoranceBase.FactionsInRange().Count<Faction>();
        }


        public static IEnumerable<Faction> FactionsBelow(TechLevel tech)
        {
            return from f in IgnoranceBase.HostileFactions()
                where f.def.techLevel < tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }


        public static IEnumerable<Faction> FactionsAbove(TechLevel tech)
        {
            return from f in IgnoranceBase.HostileFactions()
                where f.def.techLevel > tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }


        public static IEnumerable<Faction> FactionsEqual(TechLevel tech)
        {
            return from f in IgnoranceBase.HostileFactions()
                where f.def.techLevel == tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }


        public static IEnumerable<Faction> TraderFactionsBelow(TechLevel tech)
        {
            return from f in IgnoranceBase.NonHostileFactions()
                where f.def.techLevel < tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }


        public static IEnumerable<Faction> TraderFactionsAbove(TechLevel tech)
        {
            return from f in IgnoranceBase.NonHostileFactions()
                where f.def.techLevel > tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }


        public static IEnumerable<Faction> TraderFactionsEqual(TechLevel tech)
        {
            return from f in IgnoranceBase.NonHostileFactions()
                where f.def.techLevel == tech && (IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f)) || IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f))
                select f;
        }

        public static TechLevel getFactionTechLevel(Faction faction)
        {
            return updateFactionTechLevel(faction);
        }

        public static bool TechIsEligibleForIncident(TechLevel tech)
        {

            
            
            if (tech == TechLevel.Undefined)
            {
                return true;
            }

            if (SettingsHelper.LatestVersion.UseFixedTechRange)
            {
                return (int)tech >= SettingsHelper.LatestVersion.FixedRange.min && (int)tech <= SettingsHelper.LatestVersion.FixedRange.max;
            }

            int playerTechLevel = (int)IgnoranceBase.PlayerTechLevel;
            if (playerTechLevel < (int)tech)
            {
                if (SettingsHelper.LatestVersion.NumTechsAhead >= 0)
                {
                    return playerTechLevel + SettingsHelper.LatestVersion.NumTechsAhead >= (int)tech;
                }
            }
            else if (playerTechLevel > (int)tech && SettingsHelper.LatestVersion.NumTechsBehind >= 0)
            {
                return playerTechLevel - SettingsHelper.LatestVersion.NumTechsBehind <= (int)tech;
            }

            return true;
        }


        public static TechLevel updateFactionTechLevel(Faction faction)
        {
            if (SettingsHelper.LatestVersion.ChangeVoidTechLevel)
            {
                switch (faction.def.defName)
                {
                    case "rh2_nerotonin4_horde":
                        return (TechLevel)Settings.N4RaidTechnologyLevel;
                    case "rh_voidfactionbase":
                    case "rh_factionbase_void":
                    case "rh_void":
                        return (TechLevel)Settings.VoidTechTechnologyLevel;
                }
            }

            return faction.def.techLevel;
        }
        

        public static bool FactionInEligibleTechRange(Faction f)
        {
            return IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f) || IgnoranceBase.TechIsEligibleForIncident(getFactionTechLevel(f));
        }


        private static bool EmpireIsEligible(Faction f)
        {
            return f.def == FactionDefOf.Empire && SettingsHelper.LatestVersion.EmpireIsAlwaysEligible;
        }


        private static bool MechanoidsAreEligible(Faction f)
        {
            return f.def == FactionDefOf.Mechanoid && SettingsHelper.LatestVersion.MechanoidsAreAlwaysEligible;
        }


        public static Dictionary<Type, TechLevel> incidentWorkers = new Dictionary<Type, TechLevel>
        {
            {
                typeof(IncidentWorker_MechCluster),
                TechLevel.Spacer
            },
            {
                typeof(IncidentWorker_CrashedShipPart),
                TechLevel.Spacer
            },
            {
                typeof(IncidentWorker_Infestation),
                TechLevel.Animal
            },
            {
                typeof(IncidentWorker_DeepDrillInfestation),
                TechLevel.Animal
            }
        };


        public static Dictionary<string, TechLevel> questScriptDefs = new Dictionary<string, TechLevel>
        {
            {
                "ThreatReward_MechPods_MiscReward",
                TechLevel.Ultra
            }
        };


        public static bool IsTechEligableForEvent(string eventName)
        {
            if (incidentDefNames.ContainsKey(eventName))
                return TechIsEligibleForIncident(incidentDefNames.TryGetValue(eventName));
            // Ensure void is ultratech ffs
            if (SettingsHelper.LatestVersion.ChangeVoidTechLevel)
            {
                switch (eventName)
                {
                    case "VOID_N4Manhunter_RedZone":
                    case "VOID_N4Manhunter_DeathRow":
                    case "VOID_N4Event":
                    case "VOID_VolatileLeaper_ShipPartCrash":
                    case "VOID_BlackTitan_ShipPartCrash":
                    case "VOID_DevilHound_ShipPartCrash":
                    case "VOID_DefoliatorShipPartCrash":
                        return TechIsEligibleForIncident((TechLevel)Settings.N4EventTechnologyLevel);
                    case "VOID_DroneEvent":
                    case "VOID_RedDevil_NegativeEvent":
                    case "VOID_NegativeEvent":
                    case "VOID_Black_NegativeEvent":
                    case "Void_Stalker":
                    case "VOID_Endgame_NegativeEvent":
                    case "VOID_MechanoidShip":
                    case "Void_PlanetKiller": // fuck u void
                        return TechIsEligibleForIncident((TechLevel)Settings.VoidTechTechnologyLevel);
                }
            }

            return true; // default is allowed
        }


        public static Dictionary<string, TechLevel> incidentDefNames = new Dictionary<string, TechLevel>
        {
            {
                "CrystalloidShipPartCrash",
                TechLevel.Ultra
            },
            {
                "RatkinTunnel_Guerrilla",
                TechLevel.Industrial
            },
            {
                "RatkinTunnel_Thief",
                TechLevel.Industrial
            },
            {
                "VFEM_BlackKnight",
                TechLevel.Medieval
            },
            {
                "PsychicEmitterActivationSW",
                TechLevel.Spacer
            },
            {
                "WeaponsCachePodCrashSW",
                TechLevel.Spacer
            },
            {
                "AA_Incident_BlackHive",
                TechLevel.Spacer
            }
        };


        public static Dictionary<TechLevel, List<ResearchProjectDef>> strataDic = new Dictionary<TechLevel, List<ResearchProjectDef>>();


        private static TechLevel cachedTechLevel = TechLevel.Undefined;
    }
}
