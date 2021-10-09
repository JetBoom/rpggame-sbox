using Sandbox;
using System.Collections.Generic;


namespace RPG
{
	public partial class AbilityManager
	{
		private static readonly List<AbilityData> Abilities = new();
		private static readonly Dictionary<string, AbilityData> AbilitiesByName = new();

		private static void Add( string name )
		{
			var path = $"data/abilities/{name}.ability";
			var srvorcl = Host.IsServer ? "SERVER" : "CLIENT";
			var data = Resource.FromPath<AbilityData>( path );
			if ( data != null )
			{
				Abilities.Add( data );
				AbilitiesByName[name] = data;
				//Log.Info( $"{srvorcl} loaded {path}." );
			}
			else
				Log.Warning( $"{srvorcl} couldn't load {path}!!" );
		}

		public static List<AbilityData> GetAll() => Abilities;

		public static AbilityData Get( string classname )
		{
			return AbilitiesByName.ContainsKey( classname ) ? AbilitiesByName[classname] : null;
		}

		[Event.Hotload]
		public static void Init()
		{
			Abilities.Clear();
			AbilitiesByName.Clear();

			var classes = Library.GetAllAttributes<Ability>();
			foreach ( var classInfo in classes )
			{
				if ( classInfo.Name.StartsWith( "ability_base" ) || classInfo.Group == "base" ) continue;

				Add( classInfo.Name );
			}
		}
	}
}
