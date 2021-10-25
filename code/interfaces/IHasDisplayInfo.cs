using Sandbox;


namespace RPG
{
	public interface IHasDisplayInfo
	{
		public abstract string GetDisplayName();

		public void WriteDisplayInfo( DisplayInfoPanel panel ) { }

		public float DisplayInfoDistance => 1000f;

		public bool DisplayInfoIsVisibleFrom( ref Vector3 eyePos, ref Vector3 hitPos ) => eyePos.DistToSqr( hitPos ) <= DisplayInfoDistance * DisplayInfoDistance;
	}
}
