using Sandbox;


namespace RPG
{
	public partial class StatComponent : EntityComponent
	{
		[Net, Local]
		protected float _Rate { get; set; }

		[Net, Predicted, Local]
		protected float Base { get; set; }

		[Net, Predicted, Local]
		protected float LastChange { get; set; }

		[Net, Local]
		protected float _Max { get; set; }

		public float Rate
		{
			get => _Rate;
			set
			{
				Base = Value;
				LastChange = Time.Now;
				_Rate = value;
			}
		}

		public float Value
		{
			get => MathX.Clamp( Base + Rate * (Time.Now - LastChange), 0.0f, Max );
			set
			{
				Base = value;
				LastChange = Time.Now;
			}
		}

		public float Max
		{
			get => _Max;
			set
			{
				var oldMax = _Max;
				_Max = value;

				if ( value > oldMax && oldMax > 0f && Value > oldMax )
					Value = oldMax;
			}
		}
	}

	public static class StatComponentExtend
	{
		public static float GetStat<T>( this Entity self ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			return c == null ? 0f : c.Value;
		}

		public static void SetStat<T>( this Entity self, float value ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			if ( c != null )
				c.Value = value;
		}

		public static void AddStat<T>( this Entity self, float value ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			if ( c != null )
				c.Value += value;
		}

		public static float GetStatMax<T>( this Entity self ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			return c == null ? 0f : c.Max;
		}

		public static void SetStatMax<T>( this Entity self, float value ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			if ( c != null )
				c.Max = value;
		}

		public static float GetStatRate<T>( this Entity self ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			return c == null ? 0f : c.Rate;
		}

		public static void SetStatRate<T>( this Entity self, float value ) where T : StatComponent
		{
			var c = self.Components.Get<T>();
			if ( c != null )
				c.Rate = value;
		}
	}
}
