using Sandbox;
using System;


namespace RPG
{
	public partial class ResistancesComponent : EntityComponent
	{
		private float[] Resistances { get; init; }

		public ResistancesComponent() : base()
		{
			Resistances = new float[Enum.GetValues<DamageType>().Length];
		}

		public void Set( DamageType type, float value ) => Resistances[(int)type] = value;
		public float Get( DamageType type ) => Resistances[(int)type];
	}

	public static class ResistancesComponentExtend
	{
		public static ResistancesComponent GetResistancesComponent( this Entity self ) => self.Components.Get<ResistancesComponent>();

		public static float GetResistance( this Entity self, DamageType damagetype )
		{
			var c = self.GetResistancesComponent();
			if ( c != null )
				return c.Get( damagetype );

			return 0f;
		}

		public static void SetResistance( this Entity self, DamageType damagetype, float resist )
		{
			var c = self.GetResistancesComponent();
			if ( c != null )
				c.Set( damagetype, resist );
		}

		public static void TakeDamageType( this Entity self, DamageInfo info, DamageType damagetype )
		{
			info.Damage *= self.GetDamageScale( damagetype );

			self.TakeDamage( info );
		}

		public static float GetDamageScale( this Entity self, DamageType damagetype )
		{
			return 1f - self.GetResistance( damagetype );
		}
	}
}
