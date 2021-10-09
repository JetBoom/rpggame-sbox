using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace RPG.UI
{
	public partial class ContainerPanel : Panel
	{
		private static readonly Dictionary<int, ContainerPanel> Panels = new();
		//public static ContainerPanel LocalContainerPanel { get; protected set; }

		public int NetId { get; protected set; }
		public ContainerComponent Container { get; protected set; }

		protected Label LabelName;

		public static ContainerPanel GetPanelFor( int netid )
		{
			return Panels.TryGetValue( netid, out ContainerPanel panel ) ? panel : null;
		}

		public static ContainerPanel GetOrCreatePanelFor( int netid )
		{
			var panel = GetPanelFor( netid );
			if ( panel == null )
			{
				panel = RPGHudEntity.Current.RootPanel.AddChild<ContainerPanel>();
				panel.NetId = netid;
				Panels.Add( netid, panel );
			}

			return panel;
		}

		public ContainerPanel()
		{
			StyleSheet.Load( "/ui/ContainerPanel.scss" );

			LabelName = Add.Label( "", "name" );

			UpdateAll();
		}

		public void UpdateAll()
		{
			LabelName.SetText( $"Container for NetID {NetId}" );
		}

		/*public override void Tick()
		{
		}*/
	}
}
