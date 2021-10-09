using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;


namespace RPG.UI
{
	public partial class ActionProgressBar : Panel
	{
		public static ActionProgressBar Current { get; protected set; }

		public Label LabelTimeLeft { get; init; }
		public Label LabelAction { get; init; }
		public Panel Bar { get; init; }

		public ActionProgressBar()
		{
			Current = this;

			StyleSheet.Load( "/ui/ActionProgressBar.scss" );

			var transform = new PanelTransform();
			transform.AddTranslateX( Length.Percent( -50 ) );
			Style.Transform = transform;
			Style.Dirty();

			Bar = Add.Panel( "bar" );

			LabelTimeLeft = Add.Label( "", "timeleft" );
			LabelAction = Add.Label( "", "action" );
		}

		public void SetProgress( float progress, float maxprogress = 1f, string action = "" )
		{
			progress = Math.Clamp( progress, 0f, maxprogress );

			LabelAction.SetText( action );

			Bar.Style.Width = Length.Percent( progress / maxprogress * 100f );
			Bar.Style.Dirty();
		}

		public override void Tick()
		{
			base.Tick();

			var hidden = true;

			var caster = Local.Pawn?.GetAbilityCasterComponent();
			if ( caster != null )
			{
				var ability = caster.CastingAbility;
				if ( ability != null && ability.ShouldDisplayProgress )
				{
					SetProgress( Time.Now - caster.CastStartTime, ability.Data.BaseCastingTime, ability.Data.DisplayName );
					hidden = false;
				}
			}

			SetClass( "hidden", hidden );
		}
	}
}
