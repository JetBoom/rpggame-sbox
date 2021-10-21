using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public partial class ProjectileMovement
	{
		public Projectile Projectile { get; init; }

		public ProjectileMovement( Projectile proj )
		{
			Projectile = proj;
		}

		public void Move()
		{
			var vel = Projectile.Rotation.Forward * Projectile.Speed;
			Projectile.Velocity = vel;
			//Projectile.Position += vel * Time.Delta;
		}
	}
}
