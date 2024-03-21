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
	[StaticConstructorOnStartup]
	public class Bullet_ChakramLauncher : Projectile_Explosive
	{
		public Material groundMat;

		public new static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);

		public bool onGround;

		public new float ArcHeightFactor
		{
			get
			{
				float num = def.projectile.arcHeightFactor;
				float num2 = (destination - origin).MagnitudeHorizontalSquared();
				if (num * num > num2 * 0.2f * 0.2f)
				{
					num = Mathf.Sqrt(num2) * 0.2f;
				}
				return num;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			GraphicRequest graphicRequest = new GraphicRequest(def.graphicData.graphicClass, def.graphicData.texPath + "_Ground", def.graphicData.shaderType.Shader, def.graphicData.drawSize, def.graphicData.color, def.graphicData.colorTwo, def.graphicData, 0, null, null);
			MaterialRequest req = default(MaterialRequest);
			req.mainTex = ContentFinder<Texture2D>.Get(graphicRequest.path);
			req.shader = graphicRequest.shader;
			req.color = graphicRequest.color;
			req.colorTwo = graphicRequest.colorTwo;
			req.renderQueue = graphicRequest.renderQueue;
			req.shaderParameters = graphicRequest.shaderParameters;
			groundMat = MaterialPool.MatFrom(req);
		}

		public override void Draw()
		{
			float num = ArcHeightFactor * GenMath.InverseParabola(base.DistanceCoveredFraction);
			Vector3 drawPos = DrawPos;
			Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
			if (def.projectile.shadowSize > 0f)
			{
				DrawShadow(drawPos, num);
			}
			if (onGround)
			{
				Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), position, ExactRotation, groundMat, 0);
			}
			else
			{
				Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), position, ExactRotation, def.DrawMatSingle, 0);
			}
			Comps_PostDraw();
		}

		public new void DrawShadow(Vector3 drawLoc, float height)
		{
			if (!(shadowMaterial == null))
			{
				float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
				Vector3 s = new Vector3(num, 1f, num);
				Vector3 vector = new Vector3(0f, -0.01f, 0f);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawLoc + vector, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Map != null && onGround)
			{
				List<Pawn> list = base.Position.GetThingList(base.Map).OfType<Pawn>().ToList();
				for (int num = list.Count - 1; num >= 0; num--)
				{
					list[num].TryAttachFire(Rand.Range(0.3f, 0.6f));
				}
			}
		}

		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			base.Impact(hitThing);
			onGround = true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref onGround, "onGround", defaultValue: false);
		}
	}
}
