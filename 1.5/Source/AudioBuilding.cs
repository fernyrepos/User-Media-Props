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
			// Only setup ambient audio if not configured to play on incident
			if (!playOnIncident)
			{
				SetupAudio();
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.Maintain();
			}
		}

		SoundDef customSoundDef;
		// Sets up the regular, potentially looping audio sustainer
		public void SetupAudio()
		{
			// If this building plays on incident, don't set up ambient audio
			if (playOnIncident)
			{
				// Ensure any existing ambient sustainer is stopped
				if (sustainer != null)
				{
					sustainer.End();
					sustainer = null;
				}
				return;
			}

			// End existing sustainer before setting up a new one
			if (sustainer != null)
			{
				sustainer.End();
				sustainer = null;
			}

			if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
			{
				return; // No file selected or file doesn't exist
			}

			// Generate a unique defName using ThingIDNumber for the sustainer sound
			string uniqueDefName = "CustomSustainer_AudioBuilding_" + this.thingIDNumber;

			// Create a custom SoundDef for the sustainer
			customSoundDef = new SoundDef // Reuse the class field if needed elsewhere, otherwise could be local
			{
				defName = uniqueDefName,
				context = ignoreTimeAndPosition ? SoundContext.Any : SoundContext.MapOnly,
				sustain = true, // Sustainers always sustain
				priorityMode = ignoreTimeAndPosition ? VoicePriorityMode.PrioritizeNewest : VoicePriorityMode.PrioritizeNearest,
				sustainFadeoutTime = 0.1f,
			};

			// Configure the SubSoundDef for the sustainer
			SubSoundDef subSoundDef = new SubSoundDef
			{
				muteWhenPaused = !ignoreTimeAndPosition,
				tempoAffectedByGameSpeed = false,
				onCamera = ignoreTimeAndPosition,
				sustainLoop = true, // Sustainers generally loop
				sustainAttack = 0.1f,
				volumeRange = new FloatRange(volume, volume),
				distRange = distanceRange
			};

			// Add a DynamicAudioGrain
			DynamicAudioGrain dynamicGrain = new DynamicAudioGrain
			{
				filePath = selectedFilePath
			};

			// Assign grain and SubSoundDef
			subSoundDef.grains = new List<AudioGrain> { dynamicGrain };
			customSoundDef.subSounds = new List<SubSoundDef> { subSoundDef };
			customSoundDef.ResolveReferences();

			// Determine SoundInfo for the sustainer
			SoundInfo soundInfo = ignoreTimeAndPosition
				? SoundInfo.OnCamera(MaintenanceType.PerTick)
				: SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.PerTick);

			// Spawn the sustainer
			sustainer = customSoundDef.TrySpawnSustainer(soundInfo);
			if (sustainer != null)
			{
				sustainer.info.volumeFactor = volume; // Apply volume to the sustainer instance
			}
			else
			{
				Log.Error($"Failed to spawn sustainer for custom sound '{uniqueDefName}'.");
			}
			// Maintain is handled in Tick
		}

		// Plays the configured sound once, typically for incidents
		public void PlayIncidentSound()
		{
			if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
			{
				return; // No file selected or file doesn't exist
			}

			// Generate a unique defName using ThingIDNumber for the one-shot sound
			// Append "OneShot" to avoid potential conflicts with sustainer defName if SetupAudio is called later
			string uniqueDefName = "CustomOneShot_AudioBuilding_" + this.thingIDNumber;

			// Create a temporary SoundDef specifically for one-shot playback
			SoundDef oneShotSoundDef = new SoundDef
			{
				defName = uniqueDefName,
				context = ignoreTimeAndPosition ? SoundContext.Any : SoundContext.MapOnly,
				sustain = false, // One-shot sounds do not sustain
				priorityMode = ignoreTimeAndPosition ? VoicePriorityMode.PrioritizeNewest : VoicePriorityMode.PrioritizeNearest,
			};

			// Configure the SubSoundDef for one-shot
			SubSoundDef subSoundDef = new SubSoundDef
			{
				muteWhenPaused = !ignoreTimeAndPosition,
				tempoAffectedByGameSpeed = false,
				onCamera = ignoreTimeAndPosition,
				sustainLoop = false, // Ensure no looping
				volumeRange = new FloatRange(volume, volume),
				distRange = distanceRange
			};

			// Add a DynamicAudioGrain
			DynamicAudioGrain dynamicGrain = new DynamicAudioGrain
			{
				filePath = selectedFilePath
			};

			// Assign grain and SubSoundDef
			subSoundDef.grains = new List<AudioGrain> { dynamicGrain };
			oneShotSoundDef.subSounds = new List<SubSoundDef> { subSoundDef };
			oneShotSoundDef.ResolveReferences();

			// Determine SoundInfo for one-shot
			SoundInfo soundInfo = ignoreTimeAndPosition
				? SoundInfo.OnCamera(MaintenanceType.None) // No maintenance needed for one-shot
				: SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);

			// Apply volume setting directly to SoundInfo for PlayOneShot
			soundInfo.volumeFactor = volume;

			// Play the sound once
			oneShotSoundDef.PlayOneShot(soundInfo);
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
			Scribe_Defs.Look(ref selectedIncidentDef, "selectedIncidentDef");
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
