using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimEffectExtendedCut
{
	public class Building_WarzoneTable : Building
	{
		private CompPowerTrader powerComp;

		private CompBattleTable compBattleTable;

		private bool inUse;

		private int curGameTick;

		public BattleSetDef curBattleSetDef;

		private int curGameStageInd;

		private Material battleMat;

		private bool dirty;

		private int nextTurnTick;

		private BattleSetStep winningBattleSet;

		private bool winnerIsDetermined;

		private Pawn user;

		public bool CanUse => user == null && powerComp.PowerOn;

		public bool InUse
		{
			get
			{
				bool flag = user != null && powerComp.PowerOn && user.Spawned && !user.Dead && user.CurJob?.targetA.Thing == this;
				if (inUse != flag && !flag)
				{
					inUse = flag;
					StopPlay();
				}
				return flag;
			}
		}

		public Material BattleMaterial
		{
			get
			{
				if (!winnerIsDetermined && ((object)battleMat == null || dirty))
				{
					BattleSetStep battleSetStep = curBattleSetDef.battleSetTextures[curGameStageInd];
					battleMat = battleSetStep.graphicData.Graphic.MatAt(base.Rotation);
					dirty = false;
					if (battleSetStep.soundDef != null)
					{
						battleSetStep.soundDef.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
					}
				}
				else if (winningBattleSet != null && winnerIsDetermined && dirty)
				{
					battleMat = winningBattleSet.graphicData.Graphic.MatAt(base.Rotation);
					dirty = false;
					if (winningBattleSet.soundDef != null)
					{
						winningBattleSet.soundDef.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
					}
				}
				return battleMat;
			}
		}

		public bool IsGameFinished => winnerIsDetermined && Find.TickManager.TicksGame >= nextTurnTick;

		public bool LastTurn => curGameStageInd == curBattleSetDef.battleSetTextures.Count - 1 && !winnerIsDetermined && Find.TickManager.TicksGame >= nextTurnTick;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref curBattleSetDef, "curBattleSetDef");
			Scribe_Values.Look(ref curGameTick, "curGameTick", 0);
			Scribe_Values.Look(ref curGameStageInd, "curGameStageInd", 0);
			Scribe_Values.Look(ref nextTurnTick, "nextTurnTick", 0);
			Scribe_Values.Look(ref winnerIsDetermined, "winnerIsDetermined", defaultValue: false);
			Scribe_Deep.Look(ref winningBattleSet, "winningBattleSet");
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			compBattleTable = GetComp<CompBattleTable>();
			if (curBattleSetDef == null)
			{
				curBattleSetDef = RE_DefOf.RE_FirstContactWarBattleSet;
			}
		}

		public IntVec3 GetFirstSpot()
		{
			if (compBattleTable == null)
			{
				compBattleTable = GetComp<CompBattleTable>();
			}
			IntVec3 orig = compBattleTable.Props.interactionCellOffsets[0];
			IntVec3 intVec = orig.RotatedBy(base.Rotation);
			return base.Position + intVec;
		}

		public IntVec3 GetSecondSpot()
		{
			if (compBattleTable == null)
			{
				compBattleTable = GetComp<CompBattleTable>();
			}
			IntVec3 orig = compBattleTable.Props.interactionCellOffsets[1];
			IntVec3 intVec = orig.RotatedBy(base.Rotation);
			return base.Position + intVec;
		}

		public void StartPlay(Pawn user)
		{
			this.user = user;
			inUse = true;
			curGameStageInd = 0;
			nextTurnTick = Find.TickManager.TicksGame + curBattleSetDef.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
			winnerIsDetermined = false;
			winningBattleSet = null;
			battleMat = null;
			dirty = true;
		}

		public void StopPlay()
		{
			user = null;
			inUse = false;
			curGameStageInd = 0;
		}

		public void SelectWinner(Pawn winner, Pawn loser)
		{
			if (winner == user)
			{
				winningBattleSet = Enumerable.FirstOrDefault(curBattleSetDef.winningTextures, (BattleSetStep x) => x.playerA == BattleCondition.Win);
			}
			else
			{
				winningBattleSet = Enumerable.FirstOrDefault(curBattleSetDef.winningTextures, (BattleSetStep x) => x.playerB == BattleCondition.Win);
			}
			nextTurnTick = Find.TickManager.TicksGame + winningBattleSet.ticksInterval.RandomInRange;
			dirty = true;
			winnerIsDetermined = true;
			if (winningBattleSet.playerWonThought != null)
			{
				winner.needs?.mood?.thoughts?.memories?.TryGainMemory(winningBattleSet.playerWonThought);
			}
			if (winningBattleSet.playerLoseThought != null)
			{
				loser.needs?.mood?.thoughts?.memories?.TryGainMemory(winningBattleSet.playerWonThought);
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (InUse)
			{
				Vector3 drawPos = DrawPos;
				drawPos.y += 1f;
				Vector3 s = (base.Rotation.IsHorizontal ? new Vector3(def.graphicData.drawSize.y, 1f, def.graphicData.drawSize.x) : new Vector3(def.graphicData.drawSize.x, 1f, def.graphicData.drawSize.y));
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawPos, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, BattleMaterial, 0);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (InUse && Find.TickManager.TicksGame >= nextTurnTick && !winnerIsDetermined && curGameStageInd < curBattleSetDef.battleSetTextures.Count - 1)
			{
				curGameStageInd++;
				nextTurnTick = Find.TickManager.TicksGame + curBattleSetDef.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
				dirty = true;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_Action
			{
				defaultLabel = "RE.SelectBattleSet".Translate(curBattleSetDef.label),
				defaultDesc = "RE.SelectBattleSetDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Icons/SelectBattleset"),
				action = delegate
				{
					FloatMenu window = new FloatMenu(GetBattleSetOptions().ToList());
					Find.WindowStack.Add(window);
				}
			};
		}

		private IEnumerable<FloatMenuOption> GetBattleSetOptions()
		{
			foreach (BattleSetDef def in DefDatabase<BattleSetDef>.AllDefs)
			{
				yield return new FloatMenuOption(def.label, delegate
				{
					curBattleSetDef = def;
					curGameStageInd = 0;
				});
			}
		}
	}
}
