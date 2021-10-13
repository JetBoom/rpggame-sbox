using Sandbox;


namespace RPG
{
	[Library("ability_base_shootprojectile", Group = "base")]
	public abstract partial class AbilityShootProjectile : Ability
	{
		protected virtual string ProjectileClass => null;

		protected override void OnSuccess()
		{
			base.OnSuccess();

			if ( Host.IsServer && !string.IsNullOrEmpty( ProjectileClass ) )
				Projectile.FireProjectileFrom( ProjectileClass, Owner );
		}
	}
}
