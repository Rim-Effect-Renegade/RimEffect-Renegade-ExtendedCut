using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimEffectExtendedCut
{
	public class BattleSetStep : IExposable
	{
		public IntRange ticksInterval;

		public BattleCondition playerA;

		public BattleCondition playerB;

		public ThoughtDef playerWonThought;

		public ThoughtDef playerLoseThought;

		public GraphicData graphicData;

		public SoundDef soundDef;

		public void ExposeData()
		{
			Scribe_Values.Look(ref ticksInterval, "ticksInterval");
			Scribe_Values.Look(ref playerA, "playerA", BattleCondition.None);
			Scribe_Values.Look(ref playerB, "playerB", BattleCondition.None);
		}
	}
}
