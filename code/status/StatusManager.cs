using Sandbox;
using System.Collections.Generic;


namespace RPG
{
	public partial class StatusManager
	{
		private static readonly List<StatusData> List = new();
		private static readonly Dictionary<string, StatusData> Map = new();

		public static List<StatusData> GetAll() => List;

		public static StatusData Get( string classname ) => Map.ContainsKey( classname ) ? Map[classname] : null;

		private static void Add( string name )
		{
			var path = $"data/status/{name}.status";
			var srvorcl = Host.IsServer ? "SERVER" : "CLIENT";
			var data = Resource.FromPath<StatusData>( path );
			if ( data != null )
			{
				List.Add( data );
				Map[name] = data;
				//Log.Info( $"{srvorcl} loaded {path}." );
			}
			else
				Log.Warning( $"{srvorcl} couldn't load {path}!!" );
		}

		[Event.Hotload]
		public static void Init()
		{
			List.Clear();
			Map.Clear();

			var classes = Library.GetAllAttributes<Status>();
			foreach ( var classInfo in classes )
			{
				if ( classInfo.Name.StartsWith( "status_base" ) || classInfo.Group == "base" ) continue;

				Add( classInfo.Name );
			}
		}
	}
}
