using Sandbox;


namespace RPG
{
	[Library( "item" )]
	public partial class ItemData : Asset
	{
		public static readonly string BasePath = "data/items/";
		public static readonly string PathSuffix = ".item";
		public static string FilePathFor( string name ) => $"{BasePath}{name}{PathSuffix}";
		public static ItemData ResourceFor( string name ) => FromPath<ItemData>( FilePathFor( name ) );

		public string DisplayName { get; set; } = "Item Name";
		public string Description { get; set; } = "";
		public int MaxAmount { get; set; } = 1;
		public int BaseVendorValue { get; set; } = 0;
		public bool CanUseFromInventory { get; set; } = false;
		public string Model { get; set; } = "";
		public Material Icon { get; set; } = null;
		public float ModelScale { get; set; } = 1f;
		public bool IsBlessed { get; set; } = false;
	}
}
