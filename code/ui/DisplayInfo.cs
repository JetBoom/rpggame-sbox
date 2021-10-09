using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

// TODO: Make it so we have a Dictionary<Entity, DisplayInfo> so we can have multiple panels at once.

namespace RPG.UI
{
	public partial class DisplayInfo : Panel
	{
		public static DisplayInfo Current { get; protected set; }

		public Vector3 Position { get; protected set; }

		public Panel Canvas { get; init; }
		public Label LabelInfo { get; init; }

		public DisplayInfo() : base()
		{
			Current = this;

			StyleSheet.Load( "/ui/DisplayInfo.scss" );

			Canvas = Add.Panel( "canvas" );

			LabelInfo = Canvas.Add.Label( "", "info" );
		}

		public void SetInfo( string info )
		{
			LabelInfo.SetText( info );

			AddClass( "visible" );
		}

		public void ClearInfo()
		{
			RemoveClass( "visible" );
		}

		public void UpdatePosition( Vector3 worldPos )
		{
			Position = worldPos;

			var screenPos = worldPos.ToScreen();

			Style.Left = Length.Fraction( screenPos.x );
			Style.Top = Length.Fraction( screenPos.y );

			var transform = new PanelTransform();
			//transform.AddTranslateY( Length.Fraction( 1.0f ) );
			transform.AddTranslateX( Length.Fraction( -0.5f ) );

			Style.Transform = transform;
			Style.Dirty();
		}

		public override void Tick()
		{
			base.Tick();

			var icam = Game.Current?.FindActiveCamera();
			if ( icam is not Camera cam ) return;

			var eyePos = cam.Pos;
			var endPos = eyePos + cam.Rot.Forward * 10240f;

			var tr = Trace.Ray( eyePos, endPos )
				.Ignore( Local.Pawn )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.EntitiesOnly()
				.Size( 2f )
				.Run();

			var hitEnt = tr.Entity;

			if ( hitEnt is IHasDisplayInfo disp && disp.DisplayInfoIsVisibleFrom( ref eyePos, ref tr.EndPos ) )
			{
				SetInfo( disp.GetDisplayInfo() );

				var bounds = hitEnt.WorldSpaceBounds;
				UpdatePosition( bounds.Center.WithZ( bounds.Mins.z - 8f ) );
			}
			else
				ClearInfo();
		}
	}
}
