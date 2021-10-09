﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


namespace RPG
{
	public partial class RPGPlayer : Player
	{
		protected static readonly List<RPGPlayer> _AllPlayers = new();
		public static IReadOnlyList<RPGPlayer> AllPlayers => _AllPlayers;

		public ulong LastOwnerSteamId { get; set; }

		private static readonly InputButton[] InputSlotKeys = {
			InputButton.Slot1,
			InputButton.Slot2,
			InputButton.Slot3,
			InputButton.Slot4,
			InputButton.Slot5,
			InputButton.Slot6,
			InputButton.Slot7,
			InputButton.Slot8,
			InputButton.Slot9,
			InputButton.Slot0,
		};

		[Net]
		public float HealthMax { get; set; } = 100f;

		public DamageInfo LastDamageInfo { get; protected set; }
		public float LastDamage { get; protected set; }
		public DamageType LastDamageType { get; protected set; }
		public float LastDamageTime { get; protected set; }

		private TimeSince TimeSinceHealthRegen;

		[Net]
		public TimeSince TimeSinceIncapacitated { get; protected set; }

		[Net]
		public TimeSince TimeSinceKilled { get; protected set; }

		public RPGPlayer() : base()
		{
			_AllPlayers.Add( this );

			//Health = HealthMax = 60.0f;

			if ( IsServer )
			{
				Components.GetOrCreate<AbilityCasterComponent>();
				Components.GetOrCreate<ContainerComponent>();
				Components.GetOrCreate<ManaComponent>();
				Components.GetOrCreate<StaminaComponent>();
				Components.GetOrCreate<ResistancesComponent>();
			}

			for ( int i = 0; i < SkillData.NumSkills; ++i )
				Skills.Add( 0 );
		}

		public void ChangeLifeState( LifeState state )
		{
			LifeState = state;
			SetupController();
		}

		public virtual void SetupController()
		{
			if ( LifeState == LifeState.Alive )
			{
				Controller = new RPGWalkController();
				(Controller as RPGWalkController).SetSpeed( RPGGlobals.BaseWalkSpeed );
			}
			else if ( LifeState == LifeState.Dying )
				Controller = new IncapacitatedController();
			else
				Controller = null;
		}

		public virtual void LoadOrRespawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			ChangeLifeState( LifeState.Alive );

			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			CreateHull();
			WaterLevel.Clear();

			Health = HealthMax;

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			for ( int i = 0; i < SkillData.NumSkills; ++i )
				SetSkill( (SkillType)i, 0 );

			Ability.GiveAllCommon( this );

			/*var cost = 0;
			for ( int i = 0; i < MaxSkill; i++ )
			{
				var c = GetSkillIncreaseCost( SkillType.Energy, i );
				cost += c;
				if ( i % 10 == 0 ) Log.Info( $"Cost to {i} would be {cost} and at {i} would be {c}" );
			}
			Log.Info( $"Cost would be {cost}" );*/

			PostSpawn();
		}

		public override void Respawn()
		{
			LoadOrRespawn();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			//DeleteAbilities();
			RemoveAllStatus();

			Health = HealthMax;
			Velocity = Vector3.Zero;
			WaterLevel.Clear();

			CreateHull();

			Game.Current?.MoveToSpawnpoint( this );

			PostSpawn();
		}

		public virtual void PostSpawn()
		{
			ResetInterpolation();
			InvalidateStatus();

			/*if ( Host.IsServer )
			{
				for ( int i = 0; i < this.GetContainer().MaxItems - this.GetContainer().Count; ++i )
					this.GetContainer().AddItem( "item_money" );
			}*/
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( LifeState == LifeState.Dead ) return;

			LastDamageInfo = info;
			LastDamage = info.Damage;
			LastDamageTime = Time.Now;

			if ( IsServer )
				ClientRpcTookDamage( To.Single( this ), info.Damage, LastDamageType );

			this.ProceduralHitReaction( info );

			base.TakeDamage( info );
		}

		public void TakeDamageType( DamageInfo info, DamageType damagetype )
		{
			if ( LifeState == LifeState.Dead ) return;

			info.Damage *= this.GetDamageScale( damagetype );
			LastDamageType = damagetype;

			TakeDamage( info );
		}

		[ClientRpc]
		public void ClientRpcTookDamage( float damage, DamageType damagetype )
		{
			LastDamage = damage;
			LastDamageType = damagetype;
			LastDamageTime = Time.Now;
		}

		public override void OnChildAdded( Entity child )
		{
			base.OnChildAdded( child );

			this.GetContainer()?.OnChildAdded( child );

			InvalidateStatus();
		}

		public override void OnChildRemoved( Entity child )
		{
			base.OnChildRemoved( child );

			this.GetContainer()?.OnChildRemoved( child );

			InvalidateStatus();
		}

		protected override void OnComponentAdded( EntityComponent component )
		{
			base.OnComponentAdded( component );

			if ( component is Status )
				InvalidateStatus();
		}

		protected override void OnComponentRemoved( EntityComponent component )
		{
			base.OnComponentRemoved( component );

			if ( component is Status )
				InvalidateStatus();
		}

		private void DebugHud()
		{
			if ( !Client.IsListenServerHost ) return;

			int line = 1;
			var col = Color.Orange;
			var pos = Vector2.Zero;
			if ( IsServer )
			{
				DebugOverlay.ScreenText( pos, line++, col, "== Server ==" );
			}
			else
			{
				pos.x += 400;
				col = Color.Green;
				DebugOverlay.ScreenText( pos, line++, col, "== Client ==" );
			}

			DebugOverlay.ScreenText( pos, line++, col, $"NetworkIdent: {NetworkIdent}" );

			DebugOverlay.ScreenText( pos, line++, col, $"User level: {Client?.GetUserLevel()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Permissions: {Client?.GetPermissions()}" );

			DebugOverlay.ScreenText( pos, line++, col, $"Health max: {HealthMax}" );

			/*DebugOverlay.ScreenText( pos, line++, col, $"Mana component: {Components.Get<ManaComponent>()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Mana max: {this.GetManaMax()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Mana rate: {this.GetManaRate()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Mana current: {this.GetMana()}" );

			DebugOverlay.ScreenText( pos, line++, col, $"Stamina component: {Components.Get<StaminaComponent>()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Stamina max: {this.GetStaminaMax()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Stamina rate: {this.GetStaminaRate()}" );
			DebugOverlay.ScreenText( pos, line++, col, $"Stamina current: {this.GetStamina()}" );*/

			var inv = this.GetContainer();
			if ( inv != null )
			{
				DebugOverlay.ScreenText( pos, line++, col, $"{inv}#{inv.NetworkIdentity} ({inv.ItemList.Count} / {inv.MaxItems})" );
				foreach ( var item in inv.ItemList )
					DebugOverlay.ScreenText( pos, line++, col, $"{item.ClassInfo.Name}#{item.NetworkIdentity}{(item.Amount == 1 ? "" : $" ({item.Amount})")}" );
			}

			var caster = this.GetAbilityCasterComponent();
			if ( caster != null )
			{
				DebugOverlay.ScreenText( pos, line++, col, $"{caster} ({caster.Abilities.Count})" );
				DebugOverlay.ScreenText( pos, line++, col, $"Casting ability: {caster.CastingAbility}" );
				foreach ( var ability in caster.Abilities )
					DebugOverlay.ScreenText( pos, line++, col, $"{ability.ClassInfo.Name}" );
			}
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			DebugHud();
		}

		public override void Simulate( Client cl )
		{
			DebugHud();

			if ( NeedsStatusCalculation )
				CalculateStatuses();

			if ( IsServer )
			{
				if ( LifeState == LifeState.Alive )
				{
					if ( TimeSinceHealthRegen >= RPGGlobals.HealthRegenRate )
					{
						TimeSinceHealthRegen = 0f;

						var health = Health;
						if ( health < HealthMax && health > 0f )
							Health = MathF.Min( HealthMax, health + RPGGlobals.HealthRegenAmount );
					}
				}
				else if ( LifeState == LifeState.Dying )
				{
					if ( TimeSinceIncapacitated >= RPGGlobals.PlayerBleedOutTime )
						OnKilled();
				}
				else if ( LifeState == LifeState.Dead )
				{
					if ( TimeSinceKilled > RPGGlobals.PlayerRespawnTime )
						LifeState = LifeState.Respawnable;
				}
				else if ( LifeState == LifeState.Respawnable )
				{
					// TODO: Make this a menu that pops up later.
					if ( Input.Pressed( InputButton.Attack1 ) )
						ServerCmdRespawn();
				}
			}

			if ( IsClient )
			{
				for ( int i = 0; i < InputSlotKeys.Length; ++i )
				{
					if ( Input.Pressed( InputSlotKeys[i] ) )
						OnPressHotKey( i );
				}
			}

			//UpdatePhysicsHull();

			SimulateController( cl );

			this.GetCastingAbility()?.Simulate( cl );

			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Pressed( InputButton.Attack2 ) )
				this.GetAbilityCasterComponent()?.TryStartAbility( cl.GetClientData( "ability_current", "ability_thorn" ) );
		}

		protected void SimulateController( Client cl )
		{
			var controller = GetActiveController();
			if ( controller != null )
				controller.Simulate( cl, this, GetActiveAnimator() );
		}

		public override void OnActiveChildChanged( Entity previous, Entity next )
		{
			base.OnActiveChildChanged( previous, next );

			InvalidateStatus();
		}

		protected void OnPressHotKey( int slot )
		{
			if ( slot < 0 || slot >= this.GetAbilityCount() || this.GetAbilityCount() == 0 ) return;

			ConsoleSystem.Run( "ability_current", this.GetAbilityCasterComponent().GetAbilityInSlot( slot )?.ClassInfo.Name ?? "" );
		}

		/// <summary>Called when the player is knocked down and put in the dying state.</summary>
		public virtual void OnIncapacitated()
		{
			ChangeLifeState( LifeState.Dying );
			TimeSinceIncapacitated = 0f;

			// Do this for now. Later might want to make it like ganking instead of just hitting them again.
			Health = Math.Min( HealthMax, 50f );

			RPGGame.RPGCurrent?.OnIncapacitated( this );

			StopUsing();

			PlaySound( "player.death" );

			this.GetCastingAbility()?.Finish( AbilityResult.GenericFail );

			//this.GetContainer()?.SetActive( null, true );
		}

		/// <summary>Called when health reaches 0.</summary>
		public override void OnKilled()
		{
			// If we're Alive, set to incapacitated instead.
			if ( LifeState == LifeState.Alive && Client.IsValid() )
			{
				OnIncapacitated();
				return;
			}

			ChangeLifeState( LifeState.Dead );
			TimeSinceKilled = 0f;

			this.GetCastingAbility()?.Finish( AbilityResult.GenericFail );
			RemoveAllStatus();

			RPGGame.RPGCurrent?.OnKilled( this );

			//BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
			/*LastCamera = MainCamera;
			MainCamera = new SpectateRagdollCamera();
			Camera = MainCamera;*/

			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			//this.GetInventoryComponent()?.MakeLootPack();
		}

		public virtual void OnStartMeleeAttack( ItemMeleeEntity weapon )
		{
			// TODO: Play some aggression noises?
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_AllPlayers.Remove( this );
		}

		public bool IsBusy()
		{
			if ( this.GetIsCasting() ) return true;
			if ( ActiveChild is ItemWeaponEntity weapon && weapon.IsValid() && weapon.IsInUse ) return true;

			return false;
		}

		[ServerCmd]
		public static void ServerCmdRespawn()
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( player.LifeState != LifeState.Respawnable ) return;

			Log.Info( $"{player} asked to respawn." );

			player.Respawn();
		}
	}
}
