using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	[Library( "status_givestats", Group = "base" )]
	public abstract partial class StatusGiveStats : Status
	{
		public virtual int TotalStacks => 5;
		public virtual float TickTime => 1f;
		public virtual float HealthToGive => 0f;
		public virtual float StaminaToGive => 0f;
		public virtual float ManaToGive => 0f;

		private int StacksRemaining;
		private TimeSince TimeSinceLastTick;

		public StatusGiveStats() : base()
		{
			StacksRemaining = TotalStacks;
		}

		protected override void OnTickServer()
		{
			if ( TimeSinceLastTick < TickTime ) return;

			TimeSinceLastTick = 0f;

			if ( StacksRemaining-- > 0 )
			{
				if ( Entity.IsValid() )
				{
					var magnitude = GetMagnitude() / TotalStacks;
					if ( HealthToGive > 0f )
						Entity.Health = Math.Min( Entity.Health + HealthToGive * magnitude, Entity.HealthMax );
					if ( StaminaToGive > 0f )
						Entity.AddStamina( StaminaToGive * magnitude );
					if ( ManaToGive > 0f )
						Entity.AddMana( ManaToGive * magnitude );
				}
			}
			else
				Remove();
		}
	}
}
