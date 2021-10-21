using Sandbox;


namespace RPG
{
	public partial class StaminaComponent : StatComponent
	{
	}

	public static class StaminaComponentExtend
	{
		public static StatComponent GetStaminaComponent( this Entity self ) => self.Components.Get<StaminaComponent>();

		public static float GetStamina( this Entity self ) => self.GetStat<StaminaComponent>();

		public static void SetStamina( this Entity self, float value ) => self.SetStat<StaminaComponent>( value );

		public static void AddStamina( this Entity self, float value ) => self.AddStat<StaminaComponent>( value );

		public static float GetStaminaMax( this Entity self ) => self.GetStatMax<StaminaComponent>();

		public static void SetStaminaMax( this Entity self, float value ) => self.SetStatMax<StaminaComponent>( value );

		public static float GetStaminaRate( this Entity self ) => self.GetStatRate<StaminaComponent>();

		public static void SetStaminaRate( this Entity self, float value ) => self.SetStatRate<StaminaComponent>( value );
	}
}
