using Sandbox;


namespace RPG
{
	[Library( "status_raisecon" )]
	public partial class StatusRaiseCon : Status
	{
		public override void ContributeModifications( StatusModifications mods )
		{
			mods.Add( new StatusMod( StatusModType.MaxHealth, StatusModOperation.Add, GetMagnitude() ) );
		}
	}
}
