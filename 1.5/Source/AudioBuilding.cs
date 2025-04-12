using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace MediaProps
{
	[HotSwappable]
	public class AudioBuilding : MediaBuilding
	{
		public float volume = 50f;
		public FloatRange distanceRange = new FloatRange(10f, 20f);
		public bool isInvisible = false;
		public bool ignoreTimeAndPosition = false;
		public bool playOnIncident = false;
		public IncidentDef selectedIncidentDef;
		private Sustainer sustainer;

		public override string[] AcceptableExtensions => ModContentLoader<Texture2D>.AcceptableExtensionsAudio;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			SetupAudio();
		}

		public override void Tick()
		{
			base.Tick();
			if (sustainer != null)
			{
				if (sustainer.Ended)
				{
					SoundInfo soundInfo = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.PerTick);
					sustainer = customSoundDef.TrySpawnSustainer(soundInfo);
					if (sustainer != null)
					{
						sustainer.info.volumeFactor = volume;
					}
				}
				sustainer.Maintain();
			}
		}

		SoundDef customSoundDef;
		public void SetupAudio(bool forcePlay = false)
		{
			if (playOnIncident && !forcePlay)
			{
				return; // Return early if playOnIncident is enabled and not forced
			}
			if (sustainer != null)
			{
				sustainer.End();
			}

			if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
			{
				// Generate a unique defName using ThingIDNumber
				string uniqueDefName = "CustomSound_AudioBuilding_" + this.thingIDNumber;

				// Create a custom SoundDef with the specified sustain properties
				customSoundDef = new SoundDef
				{
					defName = uniqueDefName,
					context = ignoreTimeAndPosition ? SoundContext.Any : SoundContext.MapOnly, // Conditional context
					sustain = true,
					priorityMode = ignoreTimeAndPosition ? VoicePriorityMode.PrioritizeNewest : VoicePriorityMode.PrioritizeNearest,
					sustainFadeoutTime = 0.1f,  // Fade-out when stopping the sound
				};

				// Configure the SubSoundDef with specified properties
				SubSoundDef subSoundDef = new SubSoundDef
				{
					muteWhenPaused = ignoreTimeAndPosition ? false : true,
					tempoAffectedByGameSpeed = false,
					onCamera = ignoreTimeAndPosition ? true : false,
					sustainLoop = true,
					sustainAttack = 0.1f,       // Optional fade-in time
					volumeRange = new FloatRange(volume, volume),  // Use the volume field for dynamic control
					distRange = distanceRange           // Audible distance from 10 to 20 units
				};

				// Add a DynamicAudioGrain with the selected file path
				DynamicAudioGrain dynamicGrain = new DynamicAudioGrain
				{
					filePath = selectedFilePath
				};

				// Assign the grain and add SubSoundDef to SoundDef
				subSoundDef.grains = new List<AudioGrain> { dynamicGrain };
				customSoundDef.subSounds = new List<SubSoundDef> { subSoundDef };
				customSoundDef.ResolveReferences();
				// Use SoundInfo to play sound at the building's location
				SoundInfo soundInfo = ignoreTimeAndPosition ? SoundInfo.OnCamera( MaintenanceType.PerTick) : SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.PerTick);

				// Spawn the sustainer using the custom SoundDef
				sustainer = customSoundDef.TrySpawnSustainer(soundInfo);
				if (sustainer != null)
				{
					sustainer.info.volumeFactor = volume;
				}
				else
				{
					Log.Error($"Failed to spawn sustainer for custom sound '{uniqueDefName}'.");
				}
				sustainer.Maintain();
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			// Only draw the building if it is not set to invisible
			if (!isInvisible)
			{
				base.DrawAt(drawLoc, flip);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (var gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			// "Set Audio" Gizmo to open configuration dialog
			yield return new Command_Action
			{
				defaultLabel = "Set audio",
				icon = ContentFinder<Texture2D>.Get("AudioConfigButton"),
				action = () => Find.WindowStack.Add(new Dialog_AudioConfig(this))
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref volume, "volume", 50f);
			Scribe_Values.Look(ref distanceRange, "distanceRange", new FloatRange(10f, 20f));
			Scribe_Values.Look(ref isInvisible, "isInvisible", false);
			Scribe_Values.Look(ref ignoreTimeAndPosition, "ignoreTimeAndPosition", false); // Save new field
			Scribe_Values.Look(ref playOnIncident, "playOnIncident", false);
			Scribe_Defs.Look(ref selectedIncidentDef, "selectedIncidentDef"); // Updated to Scribe_Defs.Look
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			// Clean up the sustainer if it exists
			if (sustainer != null)
			{
				sustainer.End();
				sustainer = null;
			}
		}
	}
}
