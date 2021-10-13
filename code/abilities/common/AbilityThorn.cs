using Sandbox;


namespace RPG
{
	[Library( "ability_thorn" )]
	public partial class AbilityThorn : AbilityShootProjectile
	{
		protected override string ProjectileClass => "projectile_thorn";
	}
}
