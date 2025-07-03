using Verse;

namespace MediaProps
{
	[HotSwappable]
	public abstract class MediaBuilding : Building
	{
		public abstract string[] AcceptableExtensions { get; }

		public string selectedFilePath;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref selectedFilePath, "selectedFilePath", string.Empty);
		}

	}
}