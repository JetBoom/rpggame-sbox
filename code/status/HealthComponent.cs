using Sandbox;


namespace RPG
{
	/// <summary>Simple component that controls maximum health because Entity doesn't have one built-in.</summary>
	public partial class HealthComponent : EntityComponent
	{
		[Net]
		public float Max { get; set; }
	}

	public static class HealthComponentExtend
	{
		public static HealthComponent GetHealthComponent( this Entity self ) => self.Components.Get<HealthComponent>();

		public static float GetHealth( this Entity self )
		{
			return self.Health;
		}

		public static void SetHealth( this Entity self, float value )
		{
			var c = self.GetHealthComponent();
			if ( c != null )
			{
				// Things without a health component default to a max of 100 so only clamp if there is a health component.
				var max = self.GetHealthMax();
				if ( value > max )
					value = max;
			}

			self.Health = value;
		}

		public static void AddHealth( this Entity self, float value )
		{
			self.SetHealth( self.GetHealth() + value );
		}

		public static void SetHealthMax( this Entity self, float value )
		{
			var c = self.GetHealthComponent();
			if ( c != null )
				c.Max = value;

			if ( self.Health > value )
				self.Health = value;
		}

		public static float GetHealthMax( this Entity self )
		{
			var c = self.GetHealthComponent();
			if ( c != null )
				return c.Max;

			return 100f;
		}
	}
}
