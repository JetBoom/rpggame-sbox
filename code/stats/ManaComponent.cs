using Sandbox;


namespace RPG
{
	public partial class ManaComponent : StatComponent
	{
	}

	public static class ManaComponentExtend
	{
		public static StatComponent GetManaComponent( this Entity self ) => self.Components.Get<ManaComponent>();

		public static float GetMana( this Entity self ) => self.GetStat<ManaComponent>();

		public static void SetMana( this Entity self, float value ) => self.SetStat<ManaComponent>( value );

		public static void AddMana( this Entity self, float value ) => self.AddStat<ManaComponent>( value );

		public static float GetManaMax( this Entity self ) => self.GetStatMax<ManaComponent>();

		public static void SetManaMax( this Entity self, float value ) => self.SetStatMax<ManaComponent>( value );

		public static float GetManaRate( this Entity self ) => self.GetStatRate<ManaComponent>();

		public static void SetManaRate( this Entity self, float value ) => self.SetStatRate<ManaComponent>( value );
	}
}
