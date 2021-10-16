using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace RPG.UI
{
	public partial class InteractiveRootPanel : Panel
	{
		public static readonly InputButton Button = InputButton.Score;

		public bool Open { get; private set; }

		private bool ButtonPressed;

		public InteractiveRootPanel() : base()
		{
			StyleSheet.Load( "/ui/InteractiveRootPanel.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			// Have to do this convoluted stuff because Input.Pressed does not work in UI Tick functions.

			bool buttonDown = Input.Down( Button );
			if ( !buttonDown )
			{
				ButtonPressed = false;
				return;
			}

			if ( ButtonPressed ) return;
			ButtonPressed = true;

			Open = !Open;

			SetClass( "open", Open );

			if ( Open )
				ContainerPanel.UpdateLocalContainerPanel();
		}
	}
}
