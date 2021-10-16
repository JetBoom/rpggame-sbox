using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG.UI
{
	public partial class ScreenMenuBar : Panel
	{
		public ScreenMenuBar() : base()
		{
			StyleSheet.Load( "/ui/ScreenMenuBar.scss" );
		}

		public void SetBottom( bool bottom ) => SetClass( "bottom", bottom );
	}
}
