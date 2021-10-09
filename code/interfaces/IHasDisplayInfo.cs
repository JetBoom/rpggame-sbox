using Sandbox;


namespace RPG
{
	public interface IHasDisplayInfo
	{
		// TODO: Allow richer info display by passing some HUD panel here instead.
		public abstract string GetDisplayInfo();

		public float DisplayInfoDistance => 1000f;

		public bool DisplayInfoIsVisibleFrom( ref Vector3 eyePos, ref Vector3 hitPos ) => eyePos.DistToSqr( hitPos ) <= DisplayInfoDistance * DisplayInfoDistance;
	}
}
