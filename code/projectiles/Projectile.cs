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
		/// <summary>Travel speed in m/s</summary>
		public virtual float Speed => 800f;
		/// <summary>How big we are for hit detection purposes.</summary>
		public virtual float Size => 0f;
		/// <summary>Test traces/collisions on hitboxes, otherwise bounding boxes.</summary>
		public virtual bool UseHitboxes => false;
		/// <summary>If true then call the Hit function if our lifetime runs out. For example, if this is an explosive then explode if we run out of time.</summary>
		/// <remarks>OnExpire will not be called if we hit due to this.</remarks>
		public virtual bool HitOnExpire => false;
		/// <summary>Time before expiring if we don't hit anything.</summary>
		public virtual float LifeTime => 5f;

		protected virtual ProjectileMovement CreateMovement() => new ProjectileMovement( this );

		protected virtual string SoundHit => null;
		protected virtual string SoundExpire => null;

		protected ProjectileMovement Movement;

		protected Vector3 LastPosition;
		protected bool Hit;
		protected TimeSince TimeSinceCreation;

		public float LifeTimeLeft => LifeTime - TimeSinceCreation;

		public Projectile() : base()
		{
			Predictable = false;

			TimeSinceCreation = 0f;

			Movement = CreateMovement();
		}

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, Math.Max( Size, 1.0f ) );
			PhysicsEnabled = true;
			PhysicsBody.GravityEnabled = false;
			PhysicsBody.DragEnabled = false;
			PhysicsBody.AngularDamping = 0f;
			PhysicsBody.AngularDrag = PhysicsBody.LinearDrag = 0f;
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
				.Radius( Size )
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
				if ( TimeSinceCreation >= LifeTime )
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
			if ( !string.IsNullOrEmpty( SoundHit ) )
				Sound.FromWorld( SoundHit, tr.EndPos );
		}

		protected virtual void OnExpire()
		{
			if ( !string.IsNullOrEmpty( SoundExpire ) )
				Sound.FromWorld( SoundExpire, Position );
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
