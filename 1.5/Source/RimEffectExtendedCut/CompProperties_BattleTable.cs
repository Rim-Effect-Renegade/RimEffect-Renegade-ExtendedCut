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
	public class CompProperties_BattleTable : CompProperties_Power
	{
		public List<IntVec3> interactionCellOffsets;

		public CompProperties_BattleTable()
		{
			compClass = typeof(CompBattleTable);
		}
	}
}
