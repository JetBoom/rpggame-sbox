using Sandbox;


namespace RPG
{
	[Library( "ability_raisecon" )]
	public partial class AbilityRaiseCon : AbilityGiveStatus
	{
		protected override Status CreateStatus() => Owner.AddStatus<StatusRaiseCon>();
	}
}
