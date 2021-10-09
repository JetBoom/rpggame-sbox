using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public partial class AbilityCasterComponent : EntityComponent
	{
		[Net]
		public List<Ability> Abilities { get; protected set; } = new();

		[Net, Predicted, Change( nameof( OnCastingAbilityChanged ) )]
		protected byte CastingAbilityID { get; set; } = byte.MaxValue;
		public Ability CastingAbility
		{
			get => Abilities.ElementAtOrDefault( CastingAbilityID );
			protected set
			{
				if ( value != null )
				{
					var index = Abilities.FindIndex( ability => ability == value );
					if ( index < 0 )
						CastingAbilityID = byte.MaxValue;
					else
						CastingAbilityID = (byte)index;
				}
				else
					CastingAbilityID = byte.MaxValue;
			}
		}
		public bool IsCasting => CastingAbility != null;

		[Net, Predicted]
		public float CastStartTime { get; set; }

		[Net, Predicted]
		public float CastEndTime { get; set; }

		[Net, Predicted]
		public float GlobalAbilityCooldown { get; set; }
		public bool IsOnGlobalCooldown => Time.Now < GlobalAbilityCooldown;

		[Net]
		public bool Silenced { get; set; }

		[Net]
		public float CastingSpeed { get; set; } = 1f;

		[ConVar.ClientData( "ability_current" )]
		public string SelectedAbilityName { get; set; } = "";

		public IReadOnlyList<Ability> SchoolAbilities => Abilities.FindAll( ability => ability.Data.School > Schools.Common );
		public int SchoolAbilityCount => SchoolAbilities.Count;

		private Ability LastCastingAbility;
		private void OnCastingAbilityChanged()
		{
			if ( Entity != null && Entity.IsAuthority ) return;

			if ( LastCastingAbility == CastingAbility ) return;

			LastCastingAbility?.OnFinish();
			CastingAbility?.OnStart();

			LastCastingAbility = CastingAbility;
		}

		public virtual void OnFinishAbility( Ability ability, AbilityResult result )
		{
			GlobalAbilityCooldown = Math.Max( Time.Now + ability.GlobalEndCooldown, GlobalAbilityCooldown );
		}

		public virtual bool CanAddAbility( string abilityName )
		{
			if ( HasAbilityOfType( abilityName ) ) return false;

			var data = AbilityManager.Get( abilityName );
			if ( data == null ) return false;

			if ( (int)data.School > (int)Schools.Common && SchoolAbilityCount >= RPGGlobals.MaxSchoolAbilities ) return false;

			return true;
		}

		public void DeleteAbilities()
		{
			Host.AssertServer();

			CastingAbility?.Finish( AbilityResult.GenericFail );

			Abilities.Clear();
		}

		public void RemoveAbility( string abilityName )
		{
			Host.AssertServer();

			var ability = GetAbility( abilityName );
			if ( ability == null ) return;

			if ( CastingAbility == ability )
				ability.Finish( AbilityResult.GenericFail );

			Abilities.Remove( ability );
		}

		public virtual Ability GetAbilityInSlot( int i ) => Abilities.ElementAtOrDefault( i );

		public Ability GetAbility( string abilityName ) => Abilities.Find( ability => ability.ClassInfo.Name == abilityName );

		public virtual Ability AddAbility( string abilityName )
		{
			Host.AssertServer();

			if ( !CanAddAbility( abilityName ) )
				return null;

			var attribute = Library.GetAttribute( abilityName );
			if ( attribute == null )
				return null;

			var ability = Library.Create<Ability>( abilityName );
			if ( ability == null ) return null;

			ability.Owner = Entity;
			Abilities.Add( ability );

			return ability;
		}

		public bool HasAbilityOfType<T>() => Abilities.FindIndex( ability => ability is T ) >= 0;

		public bool HasAbilityOfType( string abilityName ) => GetAbility( abilityName ) != null;

		public void ClearCastingAbility() => CastingAbility = null;

		public bool TryStartAbility( Ability ability )
		{
			if ( !CanStartAbility( ability ) ) return false;

			CastingAbility = ability;

			if ( RPGGlobals.GlobalStartCD > 0.0f )
				GlobalAbilityCooldown = Math.Max( Time.Now + RPGGlobals.GlobalStartCD, GlobalAbilityCooldown );

			ability.Start();

			return true;
		}

		public bool TryStartAbility( string abilityName )
		{
			//Log.Info( $"TryStartAbility {abilityName} on {IsServer ? "Server" : "Client"}" );
			var ability = GetAbility( abilityName );
			if ( ability != null )
				return TryStartAbility( ability );

			return false;
		}

		public bool CanStartAbility( Ability ability )
		{
			return
				ability != null
				&& !IsCasting
				&& Entity.IsAlive()
				&& ability.Caster == this
				&& Time.Now > GlobalAbilityCooldown
				&& ability.GetCanBeCasted()
				&& !(Entity.ActiveChild is ItemWeaponEntity weapon && !weapon.CanStartAbility( ability ))
			;
		}

		public bool IsAnyAbilityOnCooldown()
		{
			foreach ( var ability in Abilities )
				if ( ability.CooldownActive ) return true;

			return false;
		}

		public void GiveAllCommon()
		{
			Host.AssertServer();

			var commonAbilities = AbilityManager.GetAll().Where( data => data.School == Schools.Common );
			foreach ( var abilityInfo in commonAbilities )
				AddAbility( abilityInfo.Name );
		}
	}

	public static class AbilityCasterComponentExtend
	{
		public static AbilityCasterComponent GetAbilityCasterComponent( this Entity self ) => self.Components.Get<AbilityCasterComponent>();

		public static int GetAbilityCount( this Entity self )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return 0;

			return c.Abilities.Count;
		}

		public static bool GetIsCasting( this Entity self )
		{
			return self.GetCastingAbility() != null;
		}

		public static Ability GetCastingAbility( this Entity self )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return null;

			return c.CastingAbility;
		}

		public static void SetSilenced( this Entity self, bool silenced )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return;

			c.Silenced = silenced;
		}

		public static void SetCastingSpeed( this Entity self, float speed )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return;

			c.CastingSpeed = speed;
		}

		public static bool IsOnGlobalAbilityCooldown( this Entity self )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return false;

			return c.IsOnGlobalCooldown;
		}

		public static Ability GetAbilityInSlot( this Entity self, int slot )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return null;

			return c.GetAbilityInSlot( slot );
		}

		public static Ability GetAbility( this Entity self, string classname )
		{
			var c = self.GetAbilityCasterComponent();
			if ( c == null ) return null;

			return c.GetAbility( classname );
		}
	}
}
