using System.IO;
using System.Linq;
namespace MediaProps
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
	using Verse;

	[HotSwappable]
	public class Dialog_GraphicManipulator : Window
	{
		private ImageBuilding imageBuilding;
		private Vector2 drawPosOffset;
		private Vector2 drawSize;
		private Texture2D previewImage;
		private AltitudeLayer selectedAltitudeLayer;

		public Dialog_GraphicManipulator(ImageBuilding imageBuilding)
		{
			this.imageBuilding = imageBuilding;
			doCloseButton = true;
			doCloseX = true;
			forcePause = true;
			draggable = true;

			// Initialize with the current properties of the building
			drawPosOffset = new Vector2(imageBuilding.drawPosOffsetX, imageBuilding.drawPosOffsetY);
			drawSize = new Vector2(imageBuilding.drawSizeX, imageBuilding.drawSizeY);
			selectedAltitudeLayer = imageBuilding.altitudeLayer; // Assuming altitudeLayer is defined in ImageBuilding

			LoadPreviewImage(imageBuilding.selectedFilePath); // Load initial image preview using selectedFilePath
		}

		public override Vector2 InitialSize => new Vector2(600f, 700f);

		public override void DoWindowContents(Rect inRect)
		{
			float y = 0f;

			// Draw Position Offset
			Widgets.Label(new Rect(0, y, 200f, 30f), "Draw Position Offset:");
			y += 35f;
			drawPosOffset.x = Widgets.HorizontalSlider(new Rect(0, y, inRect.width, 30f), drawPosOffset.x, -5f, 5f, label: $"X Offset: {drawPosOffset.x:F2}");
			y += 35f;
			drawPosOffset.y = Widgets.HorizontalSlider(new Rect(0, y, inRect.width, 30f), drawPosOffset.y, -5f, 5f, label: $"Y Offset: {drawPosOffset.y:F2}");
			y += 45f;

			// Draw Size
			Widgets.Label(new Rect(0, y, 200f, 30f), "Draw Size:");
			y += 35f;
			drawSize.x = Widgets.HorizontalSlider(new Rect(0, y, inRect.width, 30f), drawSize.x, 0.1f, 10f, label: $"Width: {drawSize.x:F2}");
			y += 35f;
			drawSize.y = Widgets.HorizontalSlider(new Rect(0, y, inRect.width, 30f), drawSize.y, 0.1f, 10f, label: $"Height: {drawSize.y:F2}");
			y += 45f;

			// Altitude Layer Selector
			Widgets.Label(new Rect(0, y, 200f, 30f), "Altitude Layer:");
			y += 35f;
			if (Widgets.ButtonText(new Rect(0, y, inRect.width - 20f, 30f), selectedAltitudeLayer.ToString()))
			{
				List<FloatMenuOption> options = Enum.GetValues(typeof(AltitudeLayer))
					.Cast<AltitudeLayer>()
					.Select(layer => new FloatMenuOption(layer.ToString(), () => selectedAltitudeLayer = layer))
					.ToList();
				Find.WindowStack.Add(new FloatMenu(options));
			}
			y += 45f;

			// Import Image Button
			if (Widgets.ButtonText(new Rect(0, y, 200f, 30f), "Import Image"))
			{
				Find.WindowStack.Add(new Dialog_FileSelector(imageBuilding)
				{
					doCloseX = true,
					onSelectAction = OnImageSelected
				});
			}
			y += 45f;

			// Image Preview
			if (previewImage != null)
			{
				Rect previewRect = new Rect(0, y, 200f, 200f);
				GUI.DrawTexture(previewRect, previewImage, ScaleMode.ScaleToFit);
				Widgets.Label(new Rect(210f, y + 90f, inRect.width - 210f, 30f), "Image Preview");
			}

			// Apply button to save settings
			if (Widgets.ButtonText(new Rect(inRect.width - 120f, inRect.height - 35f, 100f, 30f), "Apply"))
			{
				ApplySettings();
			}
		}

		private void OnImageSelected()
		{
			if (!string.IsNullOrEmpty(imageBuilding.selectedFilePath))
			{
				LoadPreviewImage(imageBuilding.selectedFilePath);
			}
		}

		private void LoadPreviewImage(string imagePath)
		{
			if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
			{
				byte[] imageData = File.ReadAllBytes(imagePath);
				previewImage = new Texture2D(2, 2);
				previewImage.LoadImage(imageData);
			}
			else
			{
				// Use the building's current graphic texture if no custom image is set
				previewImage = imageBuilding.Graphic.MatSingle.mainTexture as Texture2D;
			}
		}

		private void ApplySettings()
		{
			imageBuilding.drawPosOffsetX = drawPosOffset.x;
			imageBuilding.drawPosOffsetY = drawPosOffset.y;
			imageBuilding.drawSizeX = drawSize.x;
			imageBuilding.drawSizeY = drawSize.y;
			imageBuilding.altitudeLayer = selectedAltitudeLayer; // Assuming altitudeLayer is defined in ImageBuilding
			imageBuilding.customGraphic = null;
		}
	}
}
