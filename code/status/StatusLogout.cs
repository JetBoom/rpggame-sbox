using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	[Library("status_logout", Group = "base")]
	public partial class StatusLogout : Status
	{
		public StatusLogout() : base()
		{
			if ( !Host.IsServer ) return;

			StartTime = Time.Now;
			Duration = RPGGlobals.PlayerLogoutTime;
		}

		protected override void OnTickServer()
		{
			if ( Enabled && Duration > 0f && Time.Now >= EndTime )
			{
				Enabled = false;

				if ( Entity is RPGPlayer player && player.IsValid() && player.Client == null )
				{
					RPGGame.RPGCurrent.SavePlayer( player );
					player.Delete();
				}
				else
					Remove();
			}
		}
	}
}
