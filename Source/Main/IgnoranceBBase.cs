using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DFerrisIgnorance;
using DFerrisIgnorance.Modules;
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
            if (TechnologyLevelSettings.UseHighestResearched)
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

            if (TechnologyLevelSettings.UsePercentResearched)
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

                        if ((float)num / (float)IgnoranceBase.strataDic[(TechLevel)j].Count >= TechnologyLevelSettings.PercentResearchNeeded)
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
                    return f.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Combat);
                return false;
            });
        }


        public static Faction GetRandomEligibleFaction()
        {
            IEnumerable<Faction> enumerable = from f in IgnoranceBase.HostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(FactionSettings.UpdateFactionTechLevel(f))
                select f;
            if (enumerable != null && enumerable.Count<Faction>() > 0)
                return enumerable.RandomElement<Faction>();
            

            return null;
        }


        public static IEnumerable<Faction> FactionsInRange()
        {
            return from f in IgnoranceBase.HostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(FactionSettings.UpdateFactionTechLevel(f))
                select f;
        }


        public static IEnumerable<Faction> TraderFactionsInRange()
        {
            return from f in IgnoranceBase.NonHostileFactions()
                where IgnoranceBase.TechIsEligibleForIncident(FactionSettings.UpdateFactionTechLevel(f))
                select f;
        }


        public static int NumFactionsInRange()
        {
            return IgnoranceBase.FactionsInRange().Count<Faction>();
        }


        public static IEnumerable<Faction> FactionsBelow(TechLevel tech)
        {
            return IgnoranceBase.HostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) < tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }


        public static IEnumerable<Faction> FactionsAbove(TechLevel tech)
        {
            return IgnoranceBase.HostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) > tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }


        public static IEnumerable<Faction> FactionsEqual(TechLevel tech)
        {
            return IgnoranceBase.HostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) == tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }


        public static IEnumerable<Faction> TraderFactionsBelow(TechLevel tech)
        {
            return IgnoranceBase.NonHostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) < tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }


        public static IEnumerable<Faction> TraderFactionsAbove(TechLevel tech)
        {
            return IgnoranceBase.NonHostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) > tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }


        public static IEnumerable<Faction> TraderFactionsEqual(TechLevel tech)
        {
            return IgnoranceBase.NonHostileFactions().Where(x => x != null && (FactionSettings.UpdateFactionTechLevel(x) == tech || IgnoranceBase.EmpireIsEligible(x) || IgnoranceBase.MechanoidsAreEligible(x)));
        }



        public static bool TechIsEligibleForIncident(TechLevel tech)
        {

            
            
            if (tech == TechLevel.Undefined)
            {
                return true;
            }

            if (TechnologyLevelSettings.UseFixedTechRange)
            {
                return (int)tech >= TechnologyLevelSettings.FixedRange.min && (int)tech <= TechnologyLevelSettings.FixedRange.max;
            }

            int playerTechLevel = (int)IgnoranceBase.PlayerTechLevel;
            if (playerTechLevel < (int)tech)
            {
                if (TechnologyLevelSettings.NumTechsAhead >= 0)
                {
                    return playerTechLevel + TechnologyLevelSettings.NumTechsAhead >= (int)tech;
                }
            }
            else if (playerTechLevel > (int)tech && TechnologyLevelSettings.NumTechsBehind >= 0)
            {
                return playerTechLevel - TechnologyLevelSettings.NumTechsBehind <= (int)tech;
            }

            return true;
        }




        

        public static bool FactionInEligibleTechRange(Faction f)
        {
            return IgnoranceBase.EmpireIsEligible(f) || IgnoranceBase.MechanoidsAreEligible(f) || IgnoranceBase.TechIsEligibleForIncident(FactionSettings.UpdateFactionTechLevel(f));
        }


        private static bool EmpireIsEligible(Faction f)
        {
            return f.def == FactionDefOf.Empire && FactionSettings.EmpireAlwaysEligable;
        }


        private static bool MechanoidsAreEligible(Faction f)
        {
            return f.def == FactionDefOf.Mechanoid && FactionSettings.MechanoidsAlwaysEligable;
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


        public static Dictionary<string, TechLevel> questScriptDefs = new Dictionary<string, TechLevel>();
        public static Dictionary<string, TechLevel> incidentDefNames = new Dictionary<string, TechLevel>();
        public static Dictionary<TechLevel, List<ResearchProjectDef>> strataDic = new Dictionary<TechLevel, List<ResearchProjectDef>>();
        private static TechLevel cachedTechLevel = TechLevel.Undefined;
    }
}
