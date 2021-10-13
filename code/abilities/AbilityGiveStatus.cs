using Sandbox;


namespace RPG
{
	[Library("ability_base_givestatus", Group = "base")]
	public abstract partial class AbilityGiveStatus : Ability
	{
		protected virtual Status CreateStatus() => null;

		protected override void OnSuccess()
		{
			base.OnSuccess();

			if ( Host.IsServer )
				CreateStatus();
		}
	}
}
