using Sandbox;


namespace RPG
{
	public enum WeaponType : int
	{
		Melee,
		Bow,
		//Magic,
	}

	[Library( "weapon" )]
	public partial class WeaponData : Asset
	{
		public static readonly string BasePath = "data/weapons/";
		public static readonly string PathSuffix = ".weapon";
		public static string FilePathFor( string name ) => $"{BasePath}{name}{PathSuffix}";
		public static WeaponData ResourceFor( string name ) => FromPath<WeaponData>( FilePathFor( name ) );

		public WeaponType TypeOfWeapon { get; set; } = WeaponType.Melee;
		public string ViewModel { get; set; } = "";
		public float BaseDamage { get; set; } = 10f;
		public float Range { get; set; } = 32f;
		public float Arc { get; set; } = 0.5f;
		public float Speed { get; set; } = 1f;
	}


	[Library( Group = "base" )]
	public abstract partial class ItemWeapon : ItemEquippable
	{
		public override EquipSlot Slot => EquipSlot.Weapon;
	}

	/// <summary>An equippable item which can also be slotted as the ActiveChild of a player.</summary>
	[Library( Group = "base" )]
	public abstract partial class ItemWeaponEntity : ItemEquippableEntity
	{
		private WeaponData _WeaponData;
		public WeaponData WeaponData => _WeaponData == null ? _WeaponData = WeaponData.ResourceFor( ClassInfo.Name ) : _WeaponData;

		public BaseViewModel ViewModelEntity { get; protected set; }

		public virtual float AfterDelay => 0.1f;

		//public override bool ShouldBoneMerge => true;
		public override string AttachmentName => "hold_R";

		public virtual ModelEntity EffectEntity => ( IsFirstPersonMode && ViewModelEntity.IsValid() ) ? ViewModelEntity : this;

		public override bool IsInUse => Owner.IsValid() && Owner.GetIsCasting();

		public ItemWeaponEntity() : base()
		{
		}

		public override void Spawn()
		{
			base.Spawn();

			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
		}

		/*public override void Simulate( Client client )
		{
			if ( Owner.IsValid() && Input.Down( InputButton.Reload ) )
				Reload();

			if ( Owner.IsValid() && Input.Down( InputButton.Attack1 ) )
				AttackPrimary();
		}*/

		public override bool CanActivate()
		{
			return true;
		}

		public override bool CanDeactivate()
		{
			if ( Owner is RPGPlayer player && !player.IsOnGlobalAbilityCooldown() ) return false;

			return true;
		}

		public virtual bool CanStartAbility( Ability ability )
		{
			return true;
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			EnableDrawing = true;

			if ( ent is RPGPlayer player )
			{
				var animator = player.GetActiveAnimator();
				if ( animator != null )
					SimulateAnimator( animator );
			}

			if ( ent is IUseStatusMods modder )
				modder.InvalidateStatus();

			//
			// If we're the local player (clientside) create viewmodel
			// and any HUD elements that this weapon wants
			//
			if ( IsLocalPawn )
			{
				DestroyViewModel();
				//DestroyHudElements();

				CreateViewModel();
				//CreateHudElements();

				ViewModelEntity?.SetAnimBool( "deploy", true );
			}
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );

			if ( ent is IUseStatusMods modder )
				modder.InvalidateStatus();

			// If we're just holstering, then hide us
			if ( !dropped )
				EnableDrawing = false;

			if ( IsClient )
			{
				DestroyViewModel();
				//DestroyHudElements();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( IsClient && ViewModelEntity.IsValid() )
			{
				DestroyViewModel();
				//DestroyHudElements();
			}
		}

		public virtual void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( WeaponData.ViewModel ) )
				return;

			DestroyViewModel();

			ViewModelEntity = new BaseViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( WeaponData.ViewModel );
		}

		public virtual void DestroyViewModel()
		{
			Host.AssertClient();

			ViewModelEntity?.Delete();
			ViewModelEntity = null;
		}

		public virtual void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 1 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		public virtual float GetDamage()
		{
			// TODO: Scale by skill
			return WeaponData.BaseDamage;
		}
	}
}
