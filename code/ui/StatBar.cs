using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;


namespace RPG.UI
{
	public partial class StatBar : Panel
	{
		public enum StatTypes
		{
			Health,
			Mana,
			Stamina,
		}
		public RPGLabel LabelCurrent { get; init; }
		public RPGLabel LabelMax { get; init; }
		public Panel Bar { get; init; }

		private float LastValue = 0.0f;
		private float LastMax = 100.0f;

		public Entity TargetEntity { get; set; }
		public bool TargetLocalPawn = false;

		private StatTypes _StatType = StatTypes.Health;
		public StatTypes StatType
		{
			get => _StatType;
			set
			{
				_StatType = value;
				foreach ( var type in Enum.GetNames<StatTypes>() )
					RemoveClass( type.ToLower() );

				AddClass( value.ToString().ToLower() );
			}
		}

		private float _BarWidth = 600.0f;
		public float BarWidth
		{
			get => _BarWidth;
			set
			{
				_BarWidth = value;
				UpdateSizes();
			}
		}

		public StatBar()
		{
			StyleSheet.Load( "/ui/StatBar.scss" );

			Bar = Add.Panel( "bar" );
			LabelCurrent = Add.RPGLabel( LastValue.ToString(), "current" );
			LabelMax = Add.RPGLabel( LastMax.ToString(), "max" );

			UpdateSizes();
			UpdateLabels();
		}

		private void UpdateSizes()
		{
			Style.Width = Length.Pixels( BarWidth );
			Style.Dirty();
			Bar.Style.Width = Length.Pixels( Math.Clamp( LastValue / LastMax, 0.0f, 1.0f ) * BarWidth );
			Bar.Style.Dirty();
		}

		private void UpdateLabels()
		{
			LabelCurrent.Text = LastValue.FloorToInt().ToString();
			LabelMax.Text = LastMax.CeilToInt().ToString();
		}

		private Entity GetTarget() => TargetLocalPawn ? Local.Pawn : TargetEntity;

		private float GetCurrent()
		{
			var target = GetTarget();
			if ( target.IsValid() )
			{
				switch ( StatType )
				{
					case StatTypes.Health:
						return target.GetHealth();
					case StatTypes.Stamina:
						return target.GetStamina();
					case StatTypes.Mana:
						return target.GetMana();
				}
			}

			return 0.0f;
		}

		private float GetMax()
		{
			var target = GetTarget();
			if ( target.IsValid() )
			{
				switch ( StatType )
				{
					case StatTypes.Health:
						return target.GetHealthMax();
					case StatTypes.Stamina:
						return target.GetStaminaMax();
					case StatTypes.Mana:
						return target.GetManaMax();
				}
			}

			return 100.0f;
		}

		public override void Tick()
		{
			base.Tick();

			var current = GetCurrent();
			var max = GetMax();
			if ( current == LastValue && max == LastMax ) return;

			LastValue = current;
			LastMax = max;

			SetClass( "full", current >= max );
			SetClass( "critical", current <= 0.2f );
			SetClass( "empty", current == 0.0f );

			UpdateSizes();
			UpdateLabels();
		}
	}
}
