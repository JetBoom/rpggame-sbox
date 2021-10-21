using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace RPG
{
	public partial class RPGPlayer
	{
		[Net, Local]
		public List<string> UnlockedAbilities { get; set; } = new();

		public virtual bool CanSwitchAbilities()
		{
			var caster = this.GetAbilityCasterComponent();
			if ( caster == null ) return false;

			return this.IsAlive()
				&& !caster.IsAnyAbilityOnCooldown()
				&& !IsBusy()
				&& Velocity.LengthSquared < 1f
			;
		}

		[ClientCmd( "rpg_ability_slot" )]
		public static void ClientCmdAbilitySlot( string strArg )
		{
			if ( Local.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			var val = strArg.ToInt( -1 );
			if ( val < 0 || val >= player.GetAbilityCount() || player.GetAbilityCount() == 0 ) return;

			ConsoleSystem.Run( "ability_current", player.GetAbilityCasterComponent().Abilities[val].ClassInfo.Name );
		}

		[ServerCmd]
		public static void ServerCmdChangeAbilities( string strArg )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.CanSwitchAbilities() ) return;

			var component = player.GetAbilityCasterComponent();
			if ( component == null ) return;

			var abilityClasses = strArg.Split( "," );

			int remaining = RPGGlobals.MaxSchoolAbilities;
			var has = new HashSet<string>();
			bool hasUltimate = false;

			foreach ( var abilityClass in abilityClasses )
			{
				// Trying to equip something that doesn't exist?
				var data = AbilityManager.Get( abilityClass );
				if ( data == null ) return;

				// Trying to equip the same ability twice?
				if ( has.Contains( abilityClass ) ) return;
				has.Add( abilityClass );

				// Trying to equip a common or hidden ability?
				if ( data.School <= Schools.Common ) return;

				// Trying to equip too many?
				remaining -= data.AbilitySlotsUsed;
				if ( remaining < 0 ) return;

				// Trying to equip something we haven't learned?
				if ( !player.UnlockedAbilities.Contains( abilityClass ) ) return;

				// Trying to equip more than one ultimate?
				if ( data.IsUltimate )
				{
					if ( hasUltimate ) return;
					hasUltimate = true;
				}
			}

			// Guess it all passes.
			// TODO: Make this happen over time later...
			component.DeleteAbilities();
			foreach ( var abilityClass in abilityClasses )
				component.AddAbility( abilityClass );
			Ability.GiveAllCommon( player );
		}
	}
}
