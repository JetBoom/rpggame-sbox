using Sandbox;


namespace RPG
{
	[Library( "status" )]
	public partial class StatusData : Asset
	{
		public static readonly string BasePath = "data/status/";
		public static readonly string PathSuffix = ".status";
		public static string FilePathFor( string name ) => $"{BasePath}{name}{PathSuffix}";
		public static StatusData ResourceFor( string name ) => FromPath<StatusData>( FilePathFor( name ) );

		public string DisplayName { get; set; } = "Status Name";
		public bool OnlyNetworkToOwner { get; set; } = false;
		public bool CanStack { get; set; } = true;
		public Material Icon { get; set; } = null;
		public float BaseDuration { get; set; } = 5f;
		public float BaseMagnitude { get; set; } = 1f;
	}
}
