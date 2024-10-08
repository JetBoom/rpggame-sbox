﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


namespace RPG
{
	public partial class RPGPlayer : IUseStatusMods
	{
		protected bool NeedsStatusCalculation { get; set; }

		[Event.Hotload]
		public void InvalidateStatus() => NeedsStatusCalculation = true;

		public void CalculateStatuses()
		{
			//if ( IsServer ) return;

			NeedsStatusCalculation = false;

			StatusModifications mods = new();

			// Any ongoing status effects
			foreach ( var status in this.GetAllStatus() )
				status.ContributeModifications( mods );

			// Equipment such as the active weapon, worn armor, etc.
			var container = this.GetContainer();
			if ( container != null )
			{
				var equips = container.GetEquippedItems();
				foreach ( var equip in equips )
					equip.ContributeModifications( mods );
			}

			mods.FinishContributing();

			foreach ( var (modType, mod) in mods.Values )
			{
				switch ( modType )
				{
					case StatusModType.Speed:
						if ( Controller is RPGWalkController walk )
							walk.SetSpeed( mod.Alter( RPGGlobals.BaseWalkSpeed ) );
						break;
					case StatusModType.MaxHealth:
						this.SetHealthMax( mod.Alter( 60 + 40 * GetSkillFrac( SkillType.Vitality ) ) );
						break;
					case StatusModType.MaxStamina:
						this.SetStaminaMax( mod.Alter( 100f ) );
						break;
					case StatusModType.MaxMana:
						this.SetManaMax( mod.Alter( 60 + 40 * GetSkillFrac( SkillType.Energy ) ) );
						break;
					case StatusModType.ManaRate:
						this.SetManaRate( mod.Alter( 1f ) );
						break;
					case StatusModType.StaminaRate:
						this.SetStaminaRate( mod.Alter( 1f ) );
						break;
					case StatusModType.Silence:
						this.SetSilenced( mod.Alter( 0f, true ) > 0f );
						break;
					case StatusModType.CastingSpeed:
						this.SetCastingSpeed( Math.Clamp( mod.Alter( 1f, true ), RPGGlobals.MinCastingSpeed, RPGGlobals.MaxCastingSpeed ) );
						break;
					case StatusModType.ResistSlash:
						this.SetResistance( DamageType.Slashing, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistPierce:
						this.SetResistance( DamageType.Piercing, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistBlunt:
						this.SetResistance( DamageType.Blunt, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistFire:
						this.SetResistance( DamageType.Fire, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistCold:
						this.SetResistance( DamageType.Cold, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistShock:
						this.SetResistance( DamageType.Shock, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
					case StatusModType.ResistPoison:
						this.SetResistance( DamageType.Poison, Math.Min( mod.Alter( 0f, true ), RPGGlobals.MaxPlayerResistance ) );
						break;
				}
			}
		}
	}
}
