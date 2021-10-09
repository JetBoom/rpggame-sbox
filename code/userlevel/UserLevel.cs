using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;


namespace RPG
{
	public struct LevelPermissions
	{
		public UserLevel UserLevel;
		public Permissions Permissions;
	}

	public enum UserLevel : byte
	{
		User = 0,
		Moderator,
		Admin,
		SuperAdmin,
	}

	[Flags]
	public enum Permissions : ushort
	{
		None = 0,
		Kick = 1,
		Ban,
		Mute,
		TeleportSelf,
		TeleportOther,
		Kill,
		SaveWorld,
	}

	public partial class UserPermissionsComponent : EntityComponent
	{
		[Net] public LevelPermissions UserPermissions { get; set; }

		public UserLevel GetUserLevel() => UserPermissions.UserLevel;
		public Permissions GetPermissions() => UserPermissions.Permissions;
	}

	public static class UserPermissions
	{
		public static bool Loaded { get; private set; }
		private static Dictionary<ulong, LevelPermissions> PermissionsMap = new();

		public static void Load()
		{
			Host.AssertServer();

			try
			{
				if ( FileSystem.Data.FileExists( "userlevels.json" ) )
				{
					JsonSerializerOptions options = new();
					options.Converters.Add( new JsonConverterLevelPermissions() );

					var json = FileSystem.Data.ReadAllText( "userlevels.json" );
					PermissionsMap = JsonSerializer.Deserialize<Dictionary<ulong, LevelPermissions>>( json, options );
				}

				Loaded = true;
			}
			catch
			{
				Loaded = false;
			}
			finally
			{
				foreach ( var client in Client.All )
					client.UpdateUserPermissionsComponent();
			}
		}

		public static void Save()
		{
			Host.AssertServer();

			if ( !Loaded ) return;

			try
			{
				/*foreach ( var (k, v ) in PermissionsMap )
					Log.Info( $"{k} = {v.Permissions} & {v.UserLevel}" );*/

				JsonSerializerOptions options = new();
				options.Converters.Add( new JsonConverterLevelPermissions() );

				var json = JsonSerializer.Serialize( PermissionsMap, options );
				FileSystem.Data.WriteAllText( "userlevels.json", json );
			}
			catch (Exception e)
			{
				Log.Error( e.Message );
			}
		}

		public static UserPermissionsComponent GetUserPermissionsComponent( this Client client ) => client.Components.Get<UserPermissionsComponent>();

		public static void UpdateUserPermissionsComponent( this Client client )
		{
			var has = PermissionsMap.TryGetValue( client.SteamId, out LevelPermissions perms );
			var c = client.GetUserPermissionsComponent();

			if ( has && perms.Permissions == 0 && perms.UserLevel == 0 )
				has = false;

			if ( has )
			{
				c = client.Components.GetOrCreate<UserPermissionsComponent>();
				c.UserPermissions = perms;
			}
			else if ( c != null )
				c.Remove();
		}

		public static UserLevel GetUserLevel( this Client client )
		{
			var c = client.GetUserPermissionsComponent();
			if ( c != null )
				return c.GetUserLevel();

			return UserLevel.User;
		}

		public static Permissions GetPermissions( this Client client )
		{
			var c = client.GetUserPermissionsComponent();
			if ( c != null )
				return c.GetPermissions();

			return 0;
		}

		public static void SetPermissions( this Client client, LevelPermissions permissions )
		{
			PermissionsMap.Remove( client.SteamId );
			if ( permissions.UserLevel != 0 || permissions.Permissions != 0 )
				PermissionsMap.Add( client.SteamId, permissions );

			Save();

			client.UpdateUserPermissionsComponent();
		}

		public static bool IsUserLevel( this Client client, UserLevel level ) => client.GetUserLevel() >= level;

		public static bool HasPermission( this Client client, Permissions flag ) => client.GetPermissions().HasFlag( flag );
	}
}
