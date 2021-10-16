using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace RPG.UI
{
	public partial class ContainerPanel : Panel
	{
		private static readonly Dictionary<int, ContainerPanel> ContainerPanels = new();
		//public static ContainerPanel LocalContainerPanel { get; protected set; }

		public int NetId { get; protected set; }
		public ContainerComponent Container { get; protected set; }

		protected RPGLabel LabelName;

		public static ContainerPanel GetPanelFor( int netid )
		{
			return ContainerPanels.TryGetValue( netid, out ContainerPanel panel ) ? panel : null;
		}

		public static ContainerPanel GetOrCreatePanelFor( int netid )
		{
			var panel = GetPanelFor( netid );
			if ( panel == null )
			{
				panel = RPGHudEntity.Current.InteractiveRoot.AddChild<ContainerPanel>();
				panel.NetId = netid;

				ContainerPanels.Add( netid, panel );
			}

			return panel;
		}

		public static void UpdateLocalContainerPanel()
		{
			var localContainer = Local.Pawn?.GetContainer();
			if ( localContainer != null )
			{
				var panel = GetOrCreatePanelFor( localContainer.NetworkIdentity );
				panel.UpdateAll( localContainer );
			}
		}

		public ContainerPanel()
		{
			StyleSheet.Load( "/ui/ContainerPanel.scss" );

			LabelName = Add.RPGLabel( "", "name" );
		}

		public override void Delete( bool immediate = false )
		{
			ContainerPanels.Remove( NetId );
			base.Delete( immediate );
		}

		public void UpdateAll( ContainerComponent container )
		{
			LabelName.SetText( $"Container for NetID {container.NetworkIdentity}" );
		}

		/*public override void Tick()
		{
		}*/
	}
}
