using Sandbox.UI;


namespace RPG.UI
{
	public partial class RPGHud : RootPanel
	{
		public static RPGHud Current { get; protected set; }
		public InteractiveRootPanel InteractiveRoot { get; protected set; }
		public ScreenMenuBar ScreenMenuTop { get; protected set; }
		public ScreenMenuBar ScreenMenuBottom { get; protected set; }

		public RPGHud()
		{
			Current = this;

			StyleSheet.Load( "/ui/RPGHud.scss" );

			AddChild<NameTags>();
			AddChild<CrosshairCanvas>();
			AddChild<DisplayInfo>();
			CrosshairCanvas.SetCrosshair( new StandardCrosshair() );
			AddChild<ActionProgressBar>();

			AddChild<ChatBox>();
			AddChild<VoiceList>();
			AddChild<KillFeed>();

			AddChild<StatBars>();
			AddChild<HotBar>();

			//AddChild<Scoreboard<ScoreboardEntry>>();

			InteractiveRoot = AddChild<InteractiveRootPanel>();

			ScreenMenuTop = InteractiveRoot.AddChild<ScreenMenuBar>();
			ScreenMenuTop.SetBottom( false );

			ScreenMenuBottom = InteractiveRoot.AddChild<ScreenMenuBar>();
			ScreenMenuBottom.SetBottom( true );
		}
	}
}
