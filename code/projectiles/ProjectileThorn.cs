using Sandbox;


namespace RPG
{
	[Library( "projectile_thorn" )]
	public partial class ProjectileThorn : ProjectileDamage
	{
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

		protected override void OnHit( ref TraceResult tr )
		{
			base.OnHit( ref tr );

			Sound.FromWorld( "projectile.thorn.hit", tr.EndPos );
		}

		protected override void OnExpire()
		{
			base.OnExpire();

			Sound.FromWorld( "projectile.generic.expire", Position );
		}
	}
}
