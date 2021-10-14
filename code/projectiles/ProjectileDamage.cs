using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public abstract partial class ProjectileDamage : Projectile
	{
		public virtual float BaseDamage => 10f;
		public virtual float DamageRadius => 0f;
		public virtual DamageType DamageType => DamageType.Generic;
		public virtual DamageFlags DamageFlags => DamageFlags.Blunt;
		public virtual bool RadiusOnlyAffectPlayers => false;

		public ProjectileDamage() : base()
		{
		}

		protected override void OnHit( ref TraceResult tr )
		{
			base.OnHit( ref tr );

			if ( DamageRadius <= 0f )
			{
				if ( tr.Entity.IsValid() )
					ApplyDamage( ref tr, tr.Entity, BaseDamage );
			}
			else
			{
				ApplyRadialDamage( ref tr, BaseDamage );
			}
		}

		protected virtual void ApplyDamage( ref TraceResult tr, Entity entity, float damage )
		{
			var info = DamageInfo.Generic( damage )
			.UsingTraceResult( tr )
			.WithForce( Vector3.Zero )
			.WithFlag( DamageFlags )
			.WithWeapon( this )
			.WithAttacker( Owner );

			entity.TakeDamageType( info, DamageType );
		}

		protected virtual void ApplyRadialDamage( ref TraceResult tr, float damage )
		{
			Vector3 hitPos = tr.EndPos;

			var targets = GetRadialTargets( hitPos, DamageRadius, RadiusOnlyAffectPlayers );

			DebugOverlay.Sphere( hitPos, DamageRadius, Color.Red, true, 1f );

			foreach ( var ent in targets )
			{
				Vector3 entPos = ent.NearestPoint( hitPos );
				float magnitude = 1f - ent.Position.Distance( hitPos ) / DamageRadius;
				if ( magnitude < 0.15f ) magnitude = 0.15f;

				DebugOverlay.Text( entPos, $"{damage} x {magnitude} ({DamageType})", Color.Red, 1f );

				ApplyDamage( ref tr, ent, damage * magnitude );
			}
		}

		public IEnumerable<Entity> GetRadialTargets( Vector3 pos, float radius, bool onlyPlayers = false, bool throughWalls = false )
		{
			return Physics.GetEntitiesInSphere( pos, radius )
			.Where( ent =>
			{
				if ( ent == this || ent.GetHealth() <= 0f ) return false;
				if ( onlyPlayers && !(ent is RPGPlayer) ) return false;
				if ( !throughWalls && ent.IsRadialDamageVisible( pos, this ) ) return false;

				return true;
			} );
		}
	}
}
