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
	public class CompProperties_OutDoorLamp : CompProperties_Power
	{
		public float storedEnergyMax = 1000f;

		public float efficiency = 0.5f;

		public float selfCharging = 30f;

		public float maxSolarPowerGain = 300f;

		public CompProperties_OutDoorLamp()
		{
			compClass = typeof(CompPowerOutDoorLamp);
		}
	}
}
