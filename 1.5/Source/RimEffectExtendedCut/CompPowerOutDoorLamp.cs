using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaFurnitureExpanded;
using Verse;

namespace RimEffectExtendedCut
{
	public class CompPowerOutDoorLamp : ThingComp
	{
		public float storedEnergy;

		public CompGlowerExtended compGlowerExtended;

		private float SolarPower => Mathf.Lerp(0f, Props.maxSolarPowerGain, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

		protected float DesiredPowerOutput => SolarPower;

		private float RoofedPowerOutputFactor
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 item in parent.OccupiedRect())
				{
					num++;
					if (parent.Map.roofGrid.Roofed(item))
					{
						num2++;
					}
				}
				return (float)(num - num2) / (float)num;
			}
		}

		public float AmountCanAccept
		{
			get
			{
				if (parent.IsBrokenDown())
				{
					return 0f;
				}
				CompProperties_OutDoorLamp compProperties_OutDoorLamp = Props;
				return (compProperties_OutDoorLamp.storedEnergyMax - storedEnergy) / compProperties_OutDoorLamp.efficiency;
			}
		}

		public float StoredEnergy => storedEnergy;

		public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

		public CompProperties_OutDoorLamp Props => (CompProperties_OutDoorLamp)props;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			compGlowerExtended = parent.GetComp<CompGlowerExtended>();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
			CompProperties_OutDoorLamp compProperties_OutDoorLamp = Props;
			if (storedEnergy > compProperties_OutDoorLamp.storedEnergyMax)
			{
				storedEnergy = compProperties_OutDoorLamp.storedEnergyMax;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			float desiredPowerOutput = DesiredPowerOutput;
			AddEnergy(desiredPowerOutput * CompPower.WattsToWattDaysPerTick);
			if (compGlowerExtended.compGlower != null)
			{
				DrawPower(Mathf.Min(Props.selfCharging * CompPower.WattsToWattDaysPerTick, storedEnergy));
			}
			if (compGlowerExtended == null)
			{
				return;
			}
			int num = GenLocalDate.HourOfDay(parent.Map);
			if (num >= 8 && num <= 19)
			{
				if (compGlowerExtended.compGlower != null)
				{
					compGlowerExtended.RemoveGlower(parent.Map);
				}
			}
			else if (storedEnergy <= 0f)
			{
				if (compGlowerExtended.compGlower != null)
				{
					compGlowerExtended.RemoveGlower(parent.Map);
				}
			}
			else if (compGlowerExtended.compGlower == null)
			{
				compGlowerExtended.UpdateGlower(compGlowerExtended.currentColorInd);
			}
		}

		public void AddEnergy(float amount)
		{
			if (amount < 0f)
			{
				Log.Error("Cannot add negative energy " + amount);
				return;
			}
			if (amount > AmountCanAccept)
			{
				amount = AmountCanAccept;
			}
			amount *= Props.efficiency;
			storedEnergy += amount;
		}

		public void DrawPower(float amount)
		{
			storedEnergy -= amount;
			if (storedEnergy < 0f)
			{
				Log.Error("Drawing power we don't have from " + parent);
				storedEnergy = 0f;
			}
		}

		public void SetStoredEnergyPct(float pct)
		{
			pct = Mathf.Clamp01(pct);
			storedEnergy = Props.storedEnergyMax * pct;
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "Breakdown")
			{
				DrawPower(StoredEnergy);
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_OutDoorLamp compProperties_OutDoorLamp = Props;
			string text = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + compProperties_OutDoorLamp.storedEnergyMax.ToString("F0") + " Wd";
			text += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (compProperties_OutDoorLamp.efficiency * 100f).ToString("F0") + "%";
			if (storedEnergy > 0f)
			{
				text += "\n" + "SelfDischarging".Translate() + ": " + Props.selfCharging.ToString("F0") + " W";
			}
			return text;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Fill",
					action = delegate
					{
						SetStoredEnergyPct(1f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Empty",
					action = delegate
					{
						SetStoredEnergyPct(0f);
					}
				};
			}
		}
	}
}
