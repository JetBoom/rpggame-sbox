using Sandbox;


namespace RPG
{
	public enum Schools : uint
	{
		Hidden,
		Common,
		Fire,
	}

	[Library( "ability" )/*, AutoGenerate*/]
	public partial class AbilityData : Asset
	{
		public static readonly string BasePath = "data/abilities/";
		public static readonly string PathSuffix = ".ability";
		public static string FilePathFor( string name ) => $"{BasePath}{name}{PathSuffix}";
		public static AbilityData ResourceFor( string name ) => FromPath<AbilityData>( FilePathFor( name ) );

		//[Property]
		public string DisplayName { get; set; } = "Ability Name";
		//[Property]
		public Schools School { get; set; } = Schools.Hidden;
		//[Property]
		public string Description { get; set; } = "No description.";
		//[Property]
		public int XPCost { get; set; } = 0;
		//[Property]
		public bool UseCastingSpeed { get; set; } = false;
		//[Property]
		public bool UseMeleeWeaponSpeed { get; set; } = false;
		//[Property]
		public bool UseBowWeaponSpeed { get; set; } = false;
		//[Property]
		public int AbilitySlotsUsed { get; set; } = 1;
		//[Property]
		public bool IsUltimate { get; set; } = false;
		//[Property]
		public Material Icon { get; set; } = null;
		//[Property]
		public float BaseManaCost { get; set; } = 10f;
		//[Property]
		public float BaseStaminaCost { get; set; } = 0f;
		//[Property]
		public float BaseCastingTime { get; set; } = 1f;
		//[Property]
		public float BaseCooldown { get; set; } = 3f;
		//[Property]
		public bool CanBeHeld { get; set; } = true;
		//[Property]
		public bool CanCancel { get; set; } = true;
		//[Property]
		public string StartSoundPath { get; set; } = "";
		//[Property, ResourceType( "sound" )]
		public string LoopSoundPath { get; set; } = "";
		//[Property, ResourceType( "sound" )]
		public string SuccessSoundPath { get; set; } = "";
		//[Property, ResourceType( "sound" )]
		public string FailSoundPath { get; set; } = "";
	}
}
