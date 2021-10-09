using Sandbox;


namespace RPG
{
	[Library( "ability_throwrock" )]
	public partial class AbilityThrowRock : AbilityOverTime
	{
		public override float ProcessFrac => 0.4f;

		public override bool CanCastWithNoWeapon => true;
		public override bool GetCanCastWithWeapon( ItemWeaponEntity wep ) => true;

		protected override void OnProcess()
		{
			base.OnProcess();

			if ( Owner is AnimEntity anim )
				anim.SetAnimBool( "b_attack", true );

			if ( Host.IsServer )
				Projectile.FireProjectileFrom( "projectile_thorn", Owner );
		}
	}
}
