using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace MediaProps.HarmonyPatches
{
    [HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
    public static class IncidentWorker_Patch
    {
        static void Prefix(IncidentParms parms, IncidentWorker __instance)
        {
            Map map = parms?.target as Map ?? Find.CurrentMap;
            if (map == null) return;

            List<AudioBuilding> audioBuildings = new List<AudioBuilding>();
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (thing is AudioBuilding audioBuilding)
                {
                    audioBuildings.Add(audioBuilding);
                }
            }
            if (audioBuildings.Count == 0) return;

            foreach (AudioBuilding audioBuilding in audioBuildings)
            {
                if (audioBuilding.playOnIncident && audioBuilding.selectedIncidentDef == __instance.def)
                {
                    audioBuilding.SetupAudio(forcePlay: true);
                }
            }
        }
    }
}