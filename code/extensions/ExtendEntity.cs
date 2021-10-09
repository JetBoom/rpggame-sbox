using Sandbox;
using System.Collections.Generic;
using System.Linq;


namespace RPG
{
	public static class ExtendEntity
	{
		public static bool IsAlive( this Entity self )
		{
			return self.LifeState == LifeState.Alive;
		}

		public static Vector3 NearestPoint( this Entity ent, Vector3 hitPos )
		{
			if ( ent is ModelEntity modelEnt && modelEnt.PhysicsBody.IsValid() )
				return modelEnt.PhysicsBody.FindClosestPoint( hitPos );

			return ent.WorldSpaceBounds.NearestPoint( hitPos );
		}

		public static bool IsRadialDamageVisible( this Entity ent, Vector3 hitPos, Entity ignore = null )
		{
			var trace = Trace
				.Ray( ent.EyePos, hitPos )
				.WorldAndEntities()
				.HitLayer( CollisionLayer.Solid )
				.HitLayer( CollisionLayer.Player, false )
				.Ignore( ent );

			if ( ignore != null ) trace = trace.Ignore( ignore );

			// 3 chances to to see if there's any way to damage them.

			if ( !trace.Run().Hit ) return true;

			trace.FromTo( ent.NearestPoint( hitPos ), hitPos );
			if ( !trace.Run().Hit ) return true;

			trace.FromTo( ent.WorldSpaceBounds.Center, hitPos );
			if ( !trace.Run().Hit ) return true;

			return false;
		}

		public static IEnumerable<TraceResult> FindTargetsInArc( this Entity ent, float range, float mindot = 0.5f )
		{
			List<TraceResult> Hits = new();

			var fudge = ent.WorldSpaceBounds.Size.x / 2f;
			var normal = ent.EyeRot.Forward;
			var startPos = ent.EyePos - normal * fudge;
			var endPos = ent.EyePos + normal * range;

			var trace = Trace.Ray( startPos, endPos )
				.Ignore( ent )
				.Radius( range )
				.HitLayer( CollisionLayer.Solid, true )
				.UseHitboxes( true )
				.WorldOnly();

			DebugOverlay.Sphere( startPos, range, Color.White, true, 2f );
			DebugOverlay.Sphere( endPos, range, Color.White, true, 2f );
			DebugOverlay.Line( startPos, endPos, Color.White, 2f, true );

			TraceResult tr;

			do
			{
				tr = trace.Run();

				if ( tr.Hit )
				{
					trace.Ignore( tr.Entity );

					var impactpoint = tr.EndPos.WithZ( startPos.z );
					var hittonormal = (impactpoint - startPos).Normal;
					if ( normal.Dot( hittonormal ) >= mindot && tr.Entity.IsRadialDamageVisible( startPos, ent ) )
					{
						Hits.Add( tr );
						DebugOverlay.Sphere( tr.EndPos, 6f, Color.Red, true, 2f );
						DebugOverlay.Line( tr.EndPos + tr.Normal * 16f, Color.Red, 2f, true );
					}
				}

				trace.EntitiesOnly();
			} while ( tr.Hit && Hits.Count < 5 );

			return Hits;
		}
	}
}
