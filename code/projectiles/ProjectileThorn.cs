using Sandbox;


namespace RPG
{
	[Library( "projectile_thorn" )]
	public partial class ProjectileThorn : ProjectileDamage
	{
		protected override string SoundHit => "projectile.thorn.hit";
		protected override string SoundExpire => "projectile.generic.expire";

		private readonly Model ThornModel = Model.Load( "models/rust_props/small_junk/can.vmdl" );

		public ProjectileThorn() : base()
		{
			Damage = 5f;
			DamageRadius = 0f;
			Speed = 2000f;
			LifeTime = 3f;
			UseHitboxes = false;
			HitOnExpire = false;
		}

		public override void Spawn()
		{
			SetModel( ThornModel );
			EnableDrawing = true;

			base.Spawn();
		}
	}
}
