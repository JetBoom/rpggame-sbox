using Sandbox;


namespace RPG
{
	[Library( "ability_healself" )]
	public partial class AbilityHealSelf : AbilityGiveStatus
	{
		protected override Status CreateStatus() => Owner.AddStatus<StatusHealSelf>();
	}
}
