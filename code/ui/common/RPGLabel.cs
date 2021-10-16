using Sandbox.UI;
using Sandbox.UI.Construct;


namespace RPG
{
	public partial class RPGLabel : Label
	{
		public RPGLabel() : base()
		{
			StyleSheet.Load( "/ui/common/RPGLabel.scss" );
		}
	}

	public static class RPGLabelExtend
	{
		public static RPGLabel RPGLabel( this PanelCreator self, string text = null, string classname = null )
		{
			var control = self.panel.AddChild<RPGLabel>();

			if ( text != null )
				control.Text = text;

			if ( classname != null )
				control.AddClass( classname );

			return control;
		}
	}
}
