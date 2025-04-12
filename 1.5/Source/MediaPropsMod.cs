using System;
using HarmonyLib;
using Verse;

namespace MediaProps
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}
	public class MediaPropsMod : Mod
	{
		public MediaPropsMod(ModContentPack pack) : base(pack)
		{
			new Harmony("MediaPropsMod").PatchAll();
		}
	}
}