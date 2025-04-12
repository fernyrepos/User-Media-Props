using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace MediaProps
{
	[HotSwappable]
	public class Dialog_AudioConfig : Window
	{
		private readonly AudioBuilding audioBuilding;
		private float volume;
		private bool isInvisible;
		private bool ignoreTimeAndPosition;
		private string selectedFilePath;
		private FloatRange distanceRange;
		private bool localPlayOnIncident;
		private IncidentDef localSelectedIncidentDef;
		private List<IncidentDef> incidentDefs;

		public Dialog_AudioConfig(AudioBuilding audioBuilding)
		{
			this.audioBuilding = audioBuilding;
			volume = audioBuilding.volume;
			isInvisible = audioBuilding.isInvisible;
			ignoreTimeAndPosition = audioBuilding.ignoreTimeAndPosition;
			selectedFilePath = audioBuilding.selectedFilePath;
			distanceRange = audioBuilding.distanceRange;
			localPlayOnIncident = audioBuilding.playOnIncident;
			localSelectedIncidentDef = audioBuilding.selectedIncidentDef;
			incidentDefs = DefDatabase<IncidentDef>.AllDefsListForReading;
			doCloseButton = true;
			doCloseX = true;
			forcePause = true;
			draggable = true;
		}

		public override Vector2 InitialSize => new Vector2(400f, 450);

		public override void DoWindowContents(Rect inRect)
		{
			float y = 0f;
			// Volume Slider
			volume = Widgets.HorizontalSlider(new Rect(0, y , inRect.width - 20f, 30f), volume, 0f, 100f, true, $"Volume: {volume:F2}", null, null, 0.1f);
			y += 35f;
			// Invisibility Toggle
			Widgets.CheckboxLabeled(new Rect(0, y, inRect.width, 30f), "Invisible", ref isInvisible);
			y += 30f;
			// Ignore Time and Position Toggle
			Widgets.CheckboxLabeled(new Rect(0, y, inRect.width, 30f), "Ignore Time and Position", ref ignoreTimeAndPosition);
			y += 30f;
			// Play on Incident Toggle
			Widgets.CheckboxLabeled(new Rect(0, y, inRect.width, 30f), "Play on Incident?", ref localPlayOnIncident);
			y += 30f;

			// Incident Def Dropdown
			Widgets.Label(new Rect(0, y, inRect.width - 20f, 30f), "Incident:");
			y += 30f;

			string dropdownLabel = localSelectedIncidentDef?.label ?? "Select Incident"; // Default label
			if (Widgets.ButtonText(new Rect(0, y, inRect.width - 20f, 30f), dropdownLabel))
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();
				foreach (IncidentDef incidentDef in incidentDefs)
				{
					options.Add(new FloatMenuOption(incidentDef.label, () =>
					{
						localSelectedIncidentDef = incidentDef;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(options));
			}
			y += 35f;

			// Distance Range Slider
			Widgets.Label(new Rect(0, y, inRect.width - 20f, 30f), "Distance Range:");
			y += 35f;
			Widgets.FloatRange(new Rect(0, y, inRect.width - 20f, 30f), id: 1234, ref distanceRange, min: 1f, max: 100f); // Use ref keyword
			y += 45f;

			// Audio Path Button
			if (Widgets.ButtonText(new Rect(0, y, inRect.width - 20f, 30f), "Set Audio File"))
			{
				OpenFileSelector();
			}
			y += 45f;

			// Display selected audio file path
			Widgets.Label(new Rect(0, y, inRect.width - 20f, 40f), $"Audio Path: {selectedFilePath}");
			y += 35f;

			// Apply button
			if (Widgets.ButtonText(new Rect(inRect.width - 100f, inRect.height - 35f, 100f, 30f), "Apply"))
			{
				ApplySettings();
				Close();
			}
		}

		private void OpenFileSelector()
		{
			Find.WindowStack.Add(new Dialog_FileSelector(audioBuilding)
			{
				doCloseX = true,
				onSelectAction = OnAudioSelected
			});
		}

		private void OnAudioSelected()
		{
			selectedFilePath = audioBuilding.selectedFilePath;
		}

		private void ApplySettings()
		{
			audioBuilding.volume = volume;
			audioBuilding.isInvisible = isInvisible;
			audioBuilding.selectedFilePath = selectedFilePath;
			audioBuilding.distanceRange = distanceRange;
			audioBuilding.ignoreTimeAndPosition = ignoreTimeAndPosition; // Apply new setting
			audioBuilding.playOnIncident = localPlayOnIncident;
			audioBuilding.selectedIncidentDef = localSelectedIncidentDef; // Changed to selectedIncidentDef
			audioBuilding.SetupAudio();
		}
	}
}
