using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


namespace RPG.UI
{
	public partial class StatBars : Panel
	{
		public StatBar HealthStatBar { get; protected set; }
		public StatBar StaminaStatBar { get; protected set; }
		public StatBar ManaStatBar { get; protected set; }

		public StatBars()
		{
			StyleSheet.Load( "/ui/StatBars.scss" );
			AddClass( "statbars_position" );

			HealthStatBar = AddChild<StatBar>();
			HealthStatBar.TargetLocalPawn = true;
			HealthStatBar.StatType = StatBar.StatTypes.Health;
			HealthStatBar.BarWidth = 400.0f;

			StaminaStatBar = AddChild<StatBar>();
			StaminaStatBar.TargetLocalPawn = true;
			StaminaStatBar.StatType = StatBar.StatTypes.Stamina;
			StaminaStatBar.BarWidth = 400.0f;

			ManaStatBar = AddChild<StatBar>();
			ManaStatBar.TargetLocalPawn = true;
			ManaStatBar.StatType = StatBar.StatTypes.Mana;
			ManaStatBar.BarWidth = 400.0f;
		}
	}
}
