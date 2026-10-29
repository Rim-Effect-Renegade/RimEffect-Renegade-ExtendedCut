using HarmonyLib;
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
	[HarmonyPatch(typeof(WatchBuildingUtility), "GetWatchCellRect")]
	public static class Patch_WatchBuildingUtility_GetWatchCellRect
	{
		[HarmonyPrefix]
		public static bool Prefix(ref CellRect __result, ThingDef def, IntVec3 center, Rot4 rot, int watchRot)
		{
			if (def == RE_DefOf.RE_HolovisionTable)
			{
				__result = GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(2);
				return false;
			}
			return true;
		}
	}
}
