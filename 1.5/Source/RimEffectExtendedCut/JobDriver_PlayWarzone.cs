using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimEffectExtendedCut
{
	public class JobDriver_PlayWarzone : JobDriver
	{
		public Building_WarzoneTable Building_Warzone => base.TargetA.Thing as Building_WarzoneTable;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			base.TargetB.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RE_DefOf.RE_Play_WarzoneSecondPlayer, Building_Warzone, pawn), JobTag.Misc);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(Building_Warzone.GetFirstSpot(), PathEndMode.OnCell);
			Toil doPlay = new Toil();
			doPlay.tickAction = delegate
			{
				pawn.skills.Learn(SkillDefOf.Intellectual, 0.02f);
				pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None);
				Building_WarzoneTable building_Warzone = Building_Warzone;
				if (!building_Warzone.InUse && base.TargetB.Pawn.Position == building_Warzone.GetSecondSpot())
				{
					building_Warzone.StartPlay(pawn);
				}
				else if (building_Warzone.InUse)
				{
					if (building_Warzone.LastTurn)
					{
						if (Rand.Bool)
						{
							building_Warzone.SelectWinner(pawn, base.TargetB.Pawn);
						}
						else
						{
							building_Warzone.SelectWinner(base.TargetB.Pawn, pawn);
						}
					}
					else if (building_Warzone.IsGameFinished)
					{
						if (base.TargetB.Pawn.jobs.curDriver is JobDriver_PlayWarzoneSecondPlayer jobDriver_PlayWarzoneSecondPlayer)
						{
							jobDriver_PlayWarzoneSecondPlayer.endGame = true;
						}
						building_Warzone.StopPlay();
						ReadyForNextToil();
					}
				}
			};
			doPlay.defaultCompleteMode = ToilCompleteMode.Never;
			doPlay.activeSkill = () => SkillDefOf.Intellectual;
			doPlay.socialMode = RandomSocialMode.Normal;
			doPlay.AddFinishAction(delegate
			{
				Building_Warzone.StopPlay();
			});
			doPlay.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			yield return doPlay;
		}
	}
}
