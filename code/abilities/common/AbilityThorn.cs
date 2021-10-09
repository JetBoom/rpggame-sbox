using Sandbox;


namespace RPG
{
	[Library( "ability_thorn" )]
	public partial class AbilityThorn : Ability
	{
		protected override void OnSuccess()
		{
			base.OnSuccess();

			if ( Owner is AnimEntity anim )
				anim.SetAnimBool( "b_attack", true );

			if ( Host.IsServer )
				Projectile.FireProjectileFrom( "projectile_thorn", Owner );
		}
	}
}
