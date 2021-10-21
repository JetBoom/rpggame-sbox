using Sandbox;
using System.Collections.Generic;


namespace RPG
{
	public partial class ItemManager
	{
		private static readonly List<ItemData> Items = new();
		private static readonly Dictionary<string, ItemData> ItemsByName = new();
		private static readonly Dictionary<int, string> NetworkIDForClass = new();
		private static readonly Dictionary<string, int> ClassForNetworkID = new();

		private static void Add( string name )
		{
			var path = $"data/items/{name}.item";
			var srvorcl = Host.IsServer ? "SERVER" : "CLIENT";
			var data = Resource.FromPath<ItemData>( path );

			var netid = NetworkIDForClass.Count;
			ClassForNetworkID.Add( name, netid );
			NetworkIDForClass.Add( netid, name );

			if ( data != null )
			{
				Items.Add( data );
				ItemsByName[name] = data;
				//Log.Info( $"{srvorcl} loaded {path}." );
			}
			else
				Log.Warning( $"{srvorcl} couldn't load {path}!!" );
		}

		public static List<ItemData> GetAll() => Items;

		public static ItemData Get( string classname )
		{
			if ( ItemsByName.TryGetValue( classname, out ItemData data ) )
				return data;
			return null;
		}

		public static string GetClassFromNetworkID( int itemId )
		{
			return NetworkIDForClass.TryGetValue( itemId, out string classname ) ? classname : "";
		}

		public static int GetNetworkIDForClass( string classname )
		{
			return ClassForNetworkID.TryGetValue( classname, out int netid ) ? netid : -1;
		}

		[Event.Hotload]
		public static void Init()
		{
			Items.Clear();
			ItemsByName.Clear();
			NetworkIDForClass.Clear();
			ClassForNetworkID.Clear();

			var classes = Library.GetAllAttributes<Item>();
			foreach ( var classInfo in classes )
			{
				if ( classInfo.Group == "base" ) continue;

				Add( classInfo.Name );

				if ( !classInfo.Name.StartsWith("item_") )
					Log.Warning( $"Item naming convention not being followed. {classInfo.Name} does not start with item_!" );
				if ( Library.Get<ItemEntity>( "ent_" + classInfo.Name ) == null )
					Log.Warning( $"Item naming convention not being followed. {classInfo.Name} does not have a matching ent_{classInfo.Name}!" );
			}

			/*if ( Host.IsServer )
			{
				try
				{
					if ( FileSystem.Data.FileExists( "itemguid.txt" ) )
					{
						uint lastguid = FileSystem.Data.ReadAllText( "itemguid.txt" ).ToUInt( 0 ) + 1;
						if ( lastguid == 1 ) throw new System.Exception( "Item GUID cant be decoded or is 0!" );
					}
				}
				catch
				{
					Item.CorruptedGUID = true;
				}
			}*/
		}
	}
}
