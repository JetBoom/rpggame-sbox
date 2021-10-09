using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	[Library( "status_healself" )]
	public partial class StatusHealSelf : StatusGiveStats
	{
		public override int TotalStacks => 5;
		public override float HealthToGive => 1f;
	}
}
