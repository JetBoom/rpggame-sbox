using Sandbox;


namespace RPG
{
	[Library( "ability_healself" )]
	public partial class AbilityHealSelf : Ability
	{
		protected override void OnSuccess()
		{
			base.OnSuccess();

			if ( Owner is AnimEntity anim )
				anim.SetAnimBool( "b_attack", true );

			if ( Host.IsServer )
			{
				if ( Owner is RPGPlayer player )
					player.AddStatus<StatusHealSelf>();
			}
		}
	}
}
