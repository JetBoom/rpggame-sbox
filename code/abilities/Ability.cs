using Sandbox;
using System;
using System.Linq;


namespace RPG
{
	public enum AbilityResult
	{
		Success,
		Cancel,
		GenericFail,
		InsufficientStats,
		Silence,
	}

	/// <summary>An ability that can be given to something with an AbilityCasterComponent.</summary>
	[Library( "ability_base" )]
	public abstract partial class Ability : BaseNetworkable
	{
		private AbilityData _Data;
		public AbilityData Data => _Data == null ? _Data = AbilityData.ResourceFor( ClassInfo.Name ) : _Data;

		[Net]
		public Entity Owner { get; set; }

		public AbilityCasterComponent Caster => Owner?.GetAbilityCasterComponent();

		public bool IsAuthority => Owner != null && Owner.IsAuthority;

		[Net, Predicted, Local]
		public float CooldownUntil { get; set; }

		public void PutOnCooldown() => CooldownUntil = Math.Max( Time.Now + Data.BaseCooldown, CooldownUntil );
		public void ClearCooldown() => CooldownUntil = 0f;
		public float CooldownTimeLeft => Math.Max( CooldownUntil - Time.Now, 0f );
		public bool CooldownActive => CooldownTimeLeft > 0f;

		public bool IsCasting => Caster.CastingAbility == this && Caster.CastStartTime > 0f;

		public virtual bool ShouldCooldownImmediately => false;
		public virtual bool ShouldTakeStatsImmediately => false;
		public virtual bool CanNotCancel => false;
		public virtual bool CanNotBeHeld => false;
		public virtual bool ShouldDisplayProgress => true;
		public virtual float GlobalEndCooldown => 0.25f;

		public virtual bool CanCastWithNoWeapon => true;
		public virtual bool CanCastThroughSilence => Data.School <= Schools.Common;
		public virtual bool GetCanCastWithWeapon( ItemWeaponEntity wep ) => true;

		protected virtual string SuccessAnimParam => "b_attack";
		protected virtual string FailureAnimParam => null;

		private Sound LoopSound;

		public Ability() : base()
		{
			/*Data = AbilityManager.Get( ClassInfo.Name );
			if ( Data == null )
				Log.Error( $"Failed to load ability data for {ClassInfo.Name}!!" );*/
		}

		/// <summary>Called from within Pawn code. Starts the ability. If we can start or not is handled by the Pawn.</summary>
		public void Start()
		{
			Caster.CastStartTime = Time.Now;
			Caster.CastEndTime = Caster.CastStartTime + GetCastingTime();

			OnStart();
		}

		protected virtual void CreateCastingEffects()
		{
			Host.AssertClient();

			if ( !string.IsNullOrEmpty( Data.StartSoundPath ) )
				Sound.FromEntity( Data.StartSoundPath, Owner );

			if ( !string.IsNullOrEmpty( Data.LoopSoundPath ) )
			{
				LoopSound.Stop();
				LoopSound = Sound.FromEntity( Data.LoopSoundPath, Owner );
			}
		}

		public virtual void RemoveCastingEffects()
		{
			if ( !string.IsNullOrEmpty( Data.LoopSoundPath ) )
				LoopSound.Stop();
		}

		protected virtual void OnSuccess()
		{
			Assert.True( IsAuthority );

			PlaySuccessAnim();
		}

		protected virtual void PlaySuccessAnim()
		{
			if ( Owner is AnimEntity anim && !string.IsNullOrEmpty( SuccessAnimParam ) )
				anim.SetAnimBool( SuccessAnimParam, true );
		}

		protected virtual void OnFailure( AbilityResult reason = AbilityResult.GenericFail )
		{
			Assert.True( IsAuthority );

			PlayFailureAnim();
		}

		protected virtual void PlayFailureAnim()
		{
			if ( Owner is AnimEntity anim && !string.IsNullOrEmpty( SuccessAnimParam ) )
				anim.SetAnimBool( FailureAnimParam, true );
		}

		protected virtual void StartAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		/*protected virtual void StopAnimator( PawnAnimator anim )
		{
		}*/

		private void TakeStats()
		{
			Owner.AddMana( -GetManaCost() );
			Owner.AddStamina( -GetStaminaCost() );
		}

		public virtual bool GetCanBeCasted()
		{
			if ( Caster.IsCasting ) return false;
			if ( !GetOwnerCanAfford() ) return false;
			if ( CooldownActive ) return false;

			if ( !Owner.ActiveChild.IsValid() )
			{
				if ( !CanCastWithNoWeapon ) return false;
			}
			else if ( Owner.ActiveChild is ItemWeaponEntity wep )
			{
				if ( GetCanCastWithWeapon( wep ) ) return false;
			}

			return true;
		}

		public virtual bool GetOwnerCanAfford()
		{
			if ( Owner == null )
			{
				DebugOverlay.ScreenText( Host.IsServer ? "SERVER NULL" : "CLIENT NULL", 0.5f );
				return false;
			}
			var manaCost = GetManaCost();
			if ( manaCost > 0.0f && Owner.GetMana() < manaCost ) return false;
			var staminaCost = GetStaminaCost();
			if ( staminaCost > 0.0f && Owner.GetStamina() < staminaCost ) return false;

			return true;
		}

		public virtual float GetCastingTime()
		{
			float time = Data.BaseCastingTime;

			if ( Data.UseCastingSpeed )
				time /= Caster.CastingSpeed;

			/*if ( Owner.IsValid() )
			{
				if ( Data.UseMeleeWeaponSpeed && Owner.ActiveChild is ItemMelee melee )
					time /= melee.GetCastingSpeed();
				else if ( Data.UseBowWeaponSpeed && Owner.ActiveChild is ItemBow bow )
					time /= bow.GetCastingSpeed();
			}*/

			return time;
		}

		public virtual float GetManaCost()
		{
			// Maybe have mana efficiency later?
			return Data.BaseManaCost;
		}

		public virtual float GetStaminaCost()
		{
			// Maybe have stamina efficiency later?
			return Data.BaseStaminaCost;
		}

		public virtual void Simulate( Client client )
		{
			if ( !ShouldTakeStatsImmediately && !GetOwnerCanAfford() )
				OnFinish( AbilityResult.InsufficientStats );
			else if ( !CanCastThroughSilence && Caster.Silenced )
				OnFinish( AbilityResult.Silence );
			else if ( Data.CanCancel && Input.Down( InputButton.Reload ) && !CanNotCancel )
				OnFinish( AbilityResult.Cancel );
			else if ( Caster.CastEndTime > 0.0f && Time.Now >= Caster.CastEndTime )
			{
				var canBeHeld = Data.CanBeHeld && !CanNotBeHeld;
				if ( !canBeHeld || !Input.Down( InputButton.Attack2 ) )
					OnFinish( AbilityResult.Success );
			}
		}

		public virtual void OnStart()
		{
			if ( Host.IsClient )
				CreateCastingEffects();

			if ( Owner is RPGPlayer player )
			{
				var animator = player.GetActiveAnimator();
				if ( animator != null )
					StartAnimator( animator );
			}

			if ( IsAuthority )
			{
				if ( ShouldCooldownImmediately )
					PutOnCooldown();
				if ( ShouldTakeStatsImmediately )
					TakeStats();
			}
		}

		public void Finish( AbilityResult result = AbilityResult.GenericFail )
		{
			OnFinish( result );
		}

		public virtual void OnFinish( AbilityResult result = AbilityResult.Success )
		{
			if ( Host.IsClient )
				RemoveCastingEffects();

			if ( !IsAuthority ) return;

			if ( result == AbilityResult.Success )
			{
				if ( !ShouldCooldownImmediately )
					PutOnCooldown();
				if ( !ShouldTakeStatsImmediately )
					TakeStats();

				if ( Data.SuccessSoundPath != "" )
					Sound.FromEntity( Data.SuccessSoundPath, Owner );

				OnSuccess();

				Event.Run( "ability.success", this );
			}
			else if ( result == AbilityResult.Cancel )
				OnCancel();
			else
			{
				if ( Data.FailSoundPath != "" )
					Sound.FromEntity( Data.FailSoundPath, Owner );

				OnFailure( result );
			}

			Caster.CastStartTime = Caster.CastEndTime = 0.0f;
			if ( Caster.CastingAbility == this )
			{
				/*var animator = player.GetActiveAnimator();
				if ( animator != null )
					StopAnimator( animator );*/
				Caster.OnFinishAbility( this, result );
				Caster.ClearCastingAbility();
			}
		}

		protected virtual void OnCancel()
		{
			// Generally shouldn't be any behavior here but might as well have it.
		}

		public static void GiveAllCommon( Entity ent )
		{
			var caster = ent.GetAbilityCasterComponent();
			if ( caster == null ) return;

			caster.GiveAllCommon();
		}
	}
}
