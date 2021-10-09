using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace RPG.UI
{
	public partial class HotBar : Panel
	{
		private readonly List<AbilityIcon> AbilityIcons = new();

		public HotBar() : base()
		{
			StyleSheet.Load( "/ui/HotBar.scss" );

			for ( int i = 0; i <= 9; ++i )
				AddAbility( i );
		}

		private void AddAbility( int slot )
		{
			var icon = AddChild<AbilityIcon>();
			icon.AbilitySlot = slot;
			AbilityIcons.Add( icon );
		}
	}
}
