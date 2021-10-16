using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public partial class LootContainerComponent : ContainerComponent
	{
		public override int MaxItems => Math.Max( 64, Items.Count );

		public override bool CanDeposit( RPGPlayer player, Item item )
		{
			// Loot bags aren't storage containers!
			return false;
		}
	}
}
