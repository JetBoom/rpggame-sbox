using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace RPG.UI
{
	public partial class ContainerPanel : Panel
	{
		private static readonly Dictionary<ContainerComponent, ContainerPanel> ContainerPanels = new();
		//public static ContainerPanel LocalContainerPanel { get; protected set; }

		public int NetId { get; protected set; }
		public ContainerComponent Container { get; protected set; }

		protected Label LabelName;

		public static ContainerPanel GetPanelFor( ContainerComponent container )
		{
			return ContainerPanels.TryGetValue( container, out ContainerPanel panel ) ? panel : null;
		}

		public static ContainerPanel GetOrCreatePanelFor( ContainerComponent container )
		{
			var panel = GetPanelFor( container );
			if ( panel == null )
			{
				panel = RPGHud.Current.InteractiveRoot.AddChild<ContainerPanel>();
				panel.Container = container;

				ContainerPanels.Add( container, panel );
			}

			return panel;
		}

		public static void UpdateLocalContainerPanel()
		{
			var localContainer = Local.Pawn?.GetContainer();
			if ( localContainer != null )
			{
				var panel = GetOrCreatePanelFor( localContainer );
				panel.UpdateAll();
			}
		}

		public ContainerPanel()
		{
			StyleSheet.Load( "/ui/ContainerPanel.scss" );

			LabelName = Add.Label( "", "name" );
		}

		public override void Delete( bool immediate = false )
		{
			ContainerPanels.Remove( Container );
			base.Delete( immediate );
		}

		public void UpdateAll()
		{
			LabelName.SetText( $"Container for NetID {Container.NetworkIdentity}" );
		}

		/*public override void Tick()
		{
		}*/
	}
}
