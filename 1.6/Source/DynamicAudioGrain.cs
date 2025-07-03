using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MediaProps
{
    public class DynamicAudioGrain : AudioGrain
    {
        [NoTranslate]
        public string filePath = "";

        public override IEnumerable<ResolvedGrain> GetResolvedGrains()
        {
            AudioClip audioClip = LoadAudioClip(filePath);
            if (audioClip != null)
            {
                yield return new ResolvedGrain_Clip(audioClip);
            }
            else
            {
                Log.Error($"DynamicAudioGrain: Could not load AudioClip at path '{filePath}'");
            }
        }

        private AudioClip LoadAudioClip(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("DynamicAudioGrain: filePath is empty or null.");
                return null;
            }

            AudioClip clip = RuntimeAudioClipLoader.Manager.Load(path);

            if (clip == null)
            {
                Log.Error($"DynamicAudioGrain: Failed to load AudioClip from '{path}'. Ensure the path is correct.");
            }
            return clip;
        }
    }
}
