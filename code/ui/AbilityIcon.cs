using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;


namespace RPG.UI
{
	public partial class AbilityIcon : Panel
	{
		public Label LabelName { get; init; }
		public Label LabelCooldown { get; init; }
		public Image ImageIcon { get; init; }

		public int AbilitySlot { get; set; } = -1;
		public string AbilityClass { get; set; } = "";

		public AbilityIcon()
		{
			StyleSheet.Load( "/ui/AbilityIcon.scss" );

			LabelName = Add.Label( "", "name" );
			LabelCooldown = Add.Label( "", "cooldown" );

			UpdateCooldown();
		}

		public Ability GetAbility()
		{
			if ( Local.Pawn is RPGPlayer player )
				return AbilitySlot >= 0 ? player.GetAbilityInSlot( AbilitySlot ) : player.GetAbility( AbilityClass );

			return null;
		}

		public string GetAbilityClass()
		{
			if ( AbilitySlot >= 0 )
			{
				if ( Local.Pawn is RPGPlayer player )
					return player.GetAbilityInSlot( AbilitySlot ).ClassInfo.Name;
			}

			return AbilityClass;
		}

		protected void UpdateCooldown()
		{
			if ( Local.Pawn is not RPGPlayer ) return;

			Ability ability = GetAbility();
			float cooldown;
			bool selected;

			if ( ability != null )
			{
				cooldown = ability.CooldownTimeLeft;
				selected = Local.Client.GetClientData( "ability_current" ) == ability.ClassInfo.Name;
				LabelName.SetText( ability.Data.DisplayName );
				//DebugOverlay.ScreenText( Vector2.Up * (20 + 20 * AbilitySlot), 0, Color.Green, $"{AbilitySlot}: {ability.ClassInfo.Name} , Cooldown = {cooldown} , Selected = {selected}" );
			}
			else
			{
				cooldown = 0f;
				selected = false;
				LabelName.SetText( "" );
				//DebugOverlay.ScreenText( Vector2.Up * (20 + 20 * AbilitySlot), 0, Color.Red, $"No ability in slot {AbilitySlot}" );
			}

			SetClass( "oncooldown", cooldown > 0f );
			SetClass( "selected", selected );

			if ( cooldown > 0f )
			{
				if ( cooldown <= 3f )
					cooldown = (float)Math.Round( cooldown, 1 );
				else
					cooldown = (float)Math.Ceiling( cooldown );
				LabelCooldown.SetText( $"{cooldown}" );
			}
			else
				LabelCooldown.SetText( "" );
		}

		protected void UpdateIcon()
		{
		}

		public override void Tick()
		{
			UpdateCooldown();
		}
	}
}
