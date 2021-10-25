using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public class DisplayInfoPanel : Panel
	{
		public static DisplayInfoPanel Instance;

		private Label LabelName;

		public DisplayInfoPanel() : base()
		{
			StyleSheet.Load( "/ui/DisplayInfoPanel.scss" );

			LabelName = Add.Label( "", "name" );
		}

		public DisplayInfoPanel( IHasDisplayInfo info )
		{
			LabelName.SetText( info.GetDisplayName() );

			info.WriteDisplayInfo( this );
		}
	}
}
