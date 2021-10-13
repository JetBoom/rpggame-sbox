using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public abstract partial class Projectile : ModelEntity
	{
		public float Speed { get; set; } = 800f;
		public float Radius { get; set; } = 0f;
		public float LifeTime { get; set; } = 5f;
		public bool UseHitboxes { get; set; } = false;
		public bool HitOnExpire { get; set; } = false;

		protected virtual ProjectileMovement CreateMovement() => new ProjectileMovement( this );

		protected ProjectileMovement Movement;

		private Vector3 LastPosition;
		private bool Hit;
		private float TimeLeft;

		public Projectile() : base()
		{
			Predictable = false;

			TimeLeft = LifeTime;

			Movement = CreateMovement();
		}

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, Math.Max( Radius, 1.0f ) );
			PhysicsEnabled = true;
			PhysicsBody.GravityEnabled = false;
			PhysicsBody.DragEnabled = false;
			PhysicsBody.AngularDamping = 0f;
			PhysicsBody.AngularDrag = 0f;
			PhysicsBody.EnableAutoSleeping = false;
			UsePhysicsCollision = false;
			EnableAllCollisions = false;
		}

		[Event.Tick.Server]
		public virtual void OnTickServer()
		{
			if ( Hit ) return;

			Movement?.Move();

			//DebugOverlay.Sphere( Position, 1f, Color.Green, true, 1f );

			var tr = Trace
				.Ray( LastPosition, Position )
				.Radius( Radius )
				.UseHitboxes( UseHitboxes )
				.WorldAndEntities()
				.HitLayer( CollisionLayer.Solid )
				.Ignore( Owner )
				.Ignore( this )
				.Run();

			LastPosition = Position;

			if ( tr.Hit )
			{
				Hit = true;
				Position = tr.EndPos;
				OnHit( ref tr );
				Delete();
			}
			else
			{
				TimeLeft -= Time.Delta;
				if ( TimeLeft <= 0f )
				{
					if ( HitOnExpire )
					{
						Hit = true;
						Position = tr.EndPos;
						OnHit( ref tr );
						Delete();
					}
					else
					{
						Hit = true;
						OnExpire();
						Delete();
					}
				}
			}
		}

		protected virtual void OnHit( ref TraceResult tr )
		{
		}

		protected virtual void OnExpire()
		{
		}

		public static Projectile CreateProjectile( string className ) => Library.Create<Projectile>( className );

		public static Projectile FireProjectileFrom( string className, Entity owner, Vector3 origin, Rotation rot )
		{
			var proj = CreateProjectile( className );
			if ( proj.IsValid() )
			{
				proj.Owner = owner;
				proj.Position = proj.LastPosition = origin;
				proj.Rotation = rot;
				proj.Velocity = rot.Forward * proj.Speed;
			}

			return proj;
		}

		public static Projectile FireProjectileFrom( string className, Entity owner )
			=> FireProjectileFrom( className, owner, owner.EyePos, owner.EyeRot );
		public static Projectile FireProjectileFrom( string className, Entity owner, Vector3 direction )
			=> FireProjectileFrom( className, owner, owner.EyePos, direction.Normal );
		public static Projectile FireProjectileFrom( string className, Entity owner, Vector3 origin, Vector3 direction )
			=> FireProjectileFrom( className, owner, origin, direction.EulerAngles.ToRotation() );
	}
}
