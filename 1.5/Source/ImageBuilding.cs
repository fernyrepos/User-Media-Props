using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace MediaProps
{
	[HotSwappable]
	public class ImageBuilding : MediaBuilding
	{
		public float drawPosOffsetX;
		public float drawPosOffsetY;
		public float drawSizeX = 1f; // Fixed width for the graphic
		public float drawSizeY = 1f; // Fixed height for the graphic
		public AltitudeLayer altitudeLayer = AltitudeLayer.Item; // Default AltitudeLayer
		public override string[] AcceptableExtensions => ModContentLoader<Texture2D>.AcceptableExtensionsTexture;
		public Graphic customGraphic;  // Made public for external access
		private Graphic cachedDefaultGraphic;
		private string lastFilePath;
		private float lastDrawSizeX;
		private float lastDrawSizeY;

		public override Vector3 DrawPos
		{
			get
			{
				// Apply offsets to the base position and use altitudeLayer
				Vector3 basePos = base.DrawPos;
				basePos.x += drawPosOffsetX;
				basePos.z += drawPosOffsetY;
				basePos.y = altitudeLayer.AltitudeFor(); // Set the altitude based on the selected layer
				return basePos;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				// Check if a custom image path is set and valid
				if (!selectedFilePath.NullOrEmpty() && File.Exists(selectedFilePath))
				{
					if (customGraphic == null || customGraphic.MatSingle.mainTexture == null 
					|| selectedFilePath != lastFilePath || drawSizeX != lastDrawSizeX 
					|| drawSizeY != lastDrawSizeY)
					{
						customGraphic = CreateCustomGraphic(selectedFilePath);
						lastFilePath = selectedFilePath;
						lastDrawSizeX = drawSizeX;
						lastDrawSizeY = drawSizeY;
					}
					return customGraphic;
				}

				// No custom image; use cached default graphic with adjusted size
				if (cachedDefaultGraphic == null || drawSizeX != lastDrawSizeX || drawSizeY != lastDrawSizeY)
				{
					cachedDefaultGraphic = CreateDefaultGraphicWithSize();
					lastDrawSizeX = drawSizeX;
					lastDrawSizeY = drawSizeY;
				}

				return cachedDefaultGraphic;
			}
		}

		private Graphic CreateCustomGraphic(string filePath)
		{
			// Load the image as a Texture2D
			byte[] fileData = File.ReadAllBytes(filePath);
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(fileData);

			Vector2 graphicDrawSize = new Vector2(drawSizeX, drawSizeY);
			Graphic graphic = GetInner<Graphic_Single>(new GraphicRequest(typeof(Graphic_Single),
			texture, ShaderDatabase.Cutout,
			graphicDrawSize, Color.white, Color.white, null, 0, null, null));
			graphic.MatSingle.mainTexture = texture;
			return graphic;
		}

		private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
		{
			req.color = (Color32)req.color;
			req.colorTwo = (Color32)req.colorTwo;
			req.renderQueue = ((req.renderQueue == 0 && req.graphicData != null) ? req.graphicData.renderQueue : req.renderQueue);
			var value = new T();
			value.Init(req);
			return (T)value;
		}

		private Graphic CreateDefaultGraphicWithSize()
		{
			// Use def.graphicData.texPath as the default path
			string texPath = def.graphicData?.texPath;

			// Apply drawSizeX and drawSizeY to the default graphic size
			Vector2 graphicDrawSize = new Vector2(drawSizeX, drawSizeY);

			return GraphicDatabase.Get<Graphic_Single>(texPath, ShaderDatabase.Cutout, graphicDrawSize, Color.white);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (var gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_Action
			{
				defaultLabel = "Set image",
				icon = ContentFinder<Texture2D>.Get("ConfigButton"),
				action = delegate
				{
					Find.WindowStack.Add(new Dialog_GraphicManipulator(this));  // Use Dialog_GraphicManipulator
				}
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref drawPosOffsetX, "drawPosOffsetX", 0f);
			Scribe_Values.Look(ref drawPosOffsetY, "drawPosOffsetY", 0f);
			Scribe_Values.Look(ref drawSizeX, "drawSizeX", 1f);
			Scribe_Values.Look(ref drawSizeY, "drawSizeY", 1f);
			Scribe_Values.Look(ref altitudeLayer, "altitudeLayer", AltitudeLayer.Item); // Save and load the altitude layer
		}
	}
}
