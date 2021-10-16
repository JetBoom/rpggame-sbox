using Sandbox;
using Sandbox.UI;
using RPG.UI;


namespace RPG
{
	public partial class RPGHudEntity : HudEntity<RootPanel>
	{
		public static RPGHudEntity Current { get; protected set; }
		public InteractiveRootPanel InteractiveRoot { get; protected set; }

		public RPGHudEntity()
		{
			Current = this;

			if ( !IsClient ) return;

			//RootPanel.StyleSheet.Load( "/ui/rpghud.scss" );
			//RootPanel.SetTemplate( "/rpghud.html" );

			RootPanel.AddChild<NameTags>();
			RootPanel.AddChild<CrosshairCanvas>();
			RootPanel.AddChild<DisplayInfo>();
			CrosshairCanvas.SetCrosshair( new StandardCrosshair() );
			RootPanel.AddChild<ActionProgressBar>();

			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<KillFeed>();

			RootPanel.AddChild<StatBars>();
			RootPanel.AddChild<HotBar>();

			//RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();

			InteractiveRoot = RootPanel.AddChild<InteractiveRootPanel>();
		}
	}
}
