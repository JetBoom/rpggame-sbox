using Sandbox;


namespace RPG
{
	[Library( "projectile_thorn" )]
	public partial class ProjectileThorn : ProjectileDamaging
	{
		public override float DamageRadius => 0f;
		public override float Speed => 2000f;
		public override bool UseHitboxes => false;
		public override bool HitOnExpire => false;
		public override float BaseDamage => 5f;

		protected override float LifeTime => 3f;

		protected override string SoundHit => "projectile.thorn.hit";
		protected override string SoundExpire => "projectile.generic.expire";

		private static readonly Model ThornModel = Model.Load( "models/wooden_box_1.vmdl" ); //Model.Load( "models/rust_props/small_junk/can.vmdl" );

		public ProjectileThorn() : base()
		{
		}

		public override void Spawn()
		{
			SetModel( ThornModel );
			EnableDrawing = true;

			base.Spawn();
		}
	}
}
