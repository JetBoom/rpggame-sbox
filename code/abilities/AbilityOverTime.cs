using Sandbox;


namespace RPG
{
	/// <summary>
	/// An ability that has its effects at a point in time during the casting.
	/// Stat consumption and cooldown are done immediately and gameplay stuff is usually done before the EndTime.
	/// Ideal for melee abilities.
	/// </summary>
	[Library( "ability_overtime", Group = "base" )]
	public partial class AbilityOverTime : Ability
	{
		public virtual float ProcessFrac => 0.5f;
		public float ProcessTime => Caster.CastStartTime + (Caster.CastEndTime - Caster.CastStartTime) * ProcessFrac;

		public override bool ShouldCooldownImmediately => true;
		public override bool ShouldTakeStatsImmediately => true;
		public override bool CanNotCancel => true;
		public override bool CanNotBeHeld => true;
		public override bool ShouldDisplayProgress => false;

		public override bool CanCastWithNoWeapon => false;
		public override bool GetCanCastWithWeapon( ItemWeaponEntity wep ) => wep.WeaponData.TypeOfWeapon == WeaponType.Melee;

		protected bool Processed;

		public AbilityOverTime() : base()
		{
		}

		public override void Simulate( Client client )
		{
			if ( !Processed && Time.Now >= ProcessTime )
				OnProcess();

			base.Simulate( client );
		}

		protected virtual void OnProcess()
		{
			Processed = true;
		}

		public override void OnStart()
		{
			Processed = false;

			base.OnStart();
		}

		public override void OnFinish( AbilityResult result = AbilityResult.Success )
		{
			base.OnFinish( result );

			Processed = false;
		}
	}
}
