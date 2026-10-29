using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimEffectExtendedCut
{
	public class JoyGiver_PlayWarzone : JoyGiver_InteractBuilding
	{
		private static HashSet<JobTag> workTags = new HashSet<JobTag>
	{
		JobTag.Misc,
		JobTag.MiscWork,
		JobTag.Fieldwork
	};

		public override bool CanDoDuringGathering => true;

		public override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			if (!(t is Building_WarzoneTable building_WarzoneTable) || !building_WarzoneTable.CanUse || building_WarzoneTable.InUse || !pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				return null;
			}
			Pawn pawn2 = FindCompanion(pawn);
			if (pawn2 != null)
			{
				return JobMaker.MakeJob(def.jobDef, building_WarzoneTable, pawn2);
			}
			return null;
		}

		public Pawn FindCompanion(Pawn initiator)
		{
			IEnumerable<Pawn> source = from candidate in initiator.Map.mapPawns.SpawnedPawnsInFaction(initiator.Faction)
									   where candidate != initiator && BasePawnValidator(candidate) && MemberValidator(candidate) && PawnsCanGatherTogether(initiator, candidate)
									   select candidate;
			if (source.Any() && source.TryRandomElementByWeight((Pawn x) => SortCandidatesBy(initiator, x), out var result))
			{
				return result;
			}
			return null;
		}

		public bool MemberValidator(Pawn pawn)
		{
			return !workTags.Contains(pawn.mindState.lastJobTag);
		}

		public bool PawnsCanGatherTogether(Pawn organizer, Pawn companion)
		{
			return companion.relations.OpinionOf(organizer) >= 0 && organizer.relations.OpinionOf(companion) >= 0;
		}

		public float SortCandidatesBy(Pawn organizer, Pawn candidate)
		{
			return organizer.relations.OpinionOf(candidate);
		}

		public bool BasePawnValidator(Pawn pawn)
		{
			return pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState && pawn.GetLord() == null && (pawn.timetable == null || pawn.timetable.CurrentAssignment.allowJoy) && !pawn.Drafted;
		}
	}
}
