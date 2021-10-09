using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;


namespace RPG
{
	public partial class RPGGame
	{
		private bool WorldLoaded;
		private float NextSave = 0f;

		private static JsonSerializerOptions _SerializerOptions;
		public static JsonSerializerOptions SerializerOptions
		{
			get
			{
				if ( _SerializerOptions == null )
				{
					_SerializerOptions = new();
					_SerializerOptions.Converters.Add( new JsonConverterListItem() );
					_SerializerOptions.Converters.Add( new JsonConverterItem() );
					_SerializerOptions.Converters.Add( new JsonConverterListEntity() );
					_SerializerOptions.Converters.Add( new JsonConverterEntity() );
				}

				return _SerializerOptions;
			}
		}

		public static string GetPlayerSaveFile( RPGPlayer player ) => $"player_{player.LastOwnerSteamId}.json";
		public static string GetPlayerSaveFile( Client client ) => $"player_{client.SteamId}.json";
		public static string GetWorldSaveFile() => $"world_{Global.MapName}.json";

		public RPGPlayer LoadPlayer( Client client )
		{
			// Possess an already existing pawn, if there is one.
			foreach ( var existingPawn in RPGPlayer.AllPlayers )
			{
				if ( existingPawn.LastOwnerSteamId == client.SteamId && existingPawn.Client == null && existingPawn.IsValid() )
				{
					client.Pawn = existingPawn;
					return existingPawn;
				}
			}

			RPGPlayer pawn = null;
			Entity ent = null;

			// Load player from a file.
			try
			{
				var fileName = GetPlayerSaveFile( client );
				if ( FileSystem.Data.FileExists( fileName ) )
				{
					SerializeEnt.StartLoad();

					var json = FileSystem.Data.ReadAllText( fileName );
					//ReadOnlySpan<byte> jsonUtf8 = Encoding.UTF8.GetBytes( json );
					ent = JsonSerializer.Deserialize<Entity>( json, SerializerOptions );
					if ( ent is not RPGPlayer player )
						throw new Exception( $"Loaded player for {client.SteamId} is not an RPGPlayer." );

					pawn = player;
					pawn.LoadOrRespawn();
				}
			}
			catch ( Exception e )
			{
				if ( ent.IsValid() )
					ent.Delete();

				Log.Warning( e.Message );
			}
			finally
			{
				SerializeEnt.EndLoad();
			}

			// Otherwise, make a new player.
			if ( pawn == null )
			{
				pawn = new RPGPlayer();
				pawn.Respawn();
			}

			pawn.LastOwnerSteamId = client.SteamId;
			client.Pawn = pawn;

			return pawn;
		}

		public void SavePlayer( RPGPlayer player )
		{
			if ( !player.IsValid() || player.LastOwnerSteamId == 0 ) return;

			try
			{
				var json = JsonSerializer.Serialize( player, SerializerOptions );
				FileSystem.Data.WriteAllText( GetPlayerSaveFile( player ), json );
			}
			catch ( Exception e )
			{
				Log.Warning( e.Message );
			}
		}

		public void SavePlayer( Client client )
		{
			if ( client.Pawn is RPGPlayer player )
				SavePlayer( player );
		}

		public void SaveWorld()
		{
			Host.AssertServer();

			NextSave = RealTime.Now + RPGGlobals.SaveWorldTime;

			if ( !WorldLoaded || Global.IsClosing ) return;

			Log.Info( $"Starting SaveWorld with {Client.All.Count} client(s) and {RPGPlayer.AllPlayers.Count} player pawn(s)" );

			// Save player in separate files.
			foreach ( var pl in RPGPlayer.AllPlayers )
				SavePlayer( pl );

			var ents = All.Where( ent => ent is ISavedEntity sav && sav.ShouldPersistInWorldFile() ).ToList();
			try
			{
				var json = JsonSerializer.Serialize( ents, SerializerOptions );

				FileSystem.Data.WriteAllText( GetWorldSaveFile(), json );

				Log.Info( $"SaveWorld done with {ents.Count} saved entities." );
			}
			catch ( Exception e )
			{
				Log.Error( $"Failed to SaveWorld: {e.Message}" );
			}
		}

		public void LoadWorld()
		{
			Host.AssertServer();

			if ( WorldLoaded ) return;

			Log.Info( "Loading world..." );

			SerializeEnt.StartLoad();

			try
			{
				var json = FileSystem.Data.ReadAllText( GetWorldSaveFile() );
				//ReadOnlySpan<byte> jsonUtf8 = Encoding.UTF8.GetBytes( json );
				var ents = JsonSerializer.Deserialize<List<Entity>>( json, SerializerOptions );

				Log.Info( $"Loaded {ents.Count} world entities." );
			}
			catch ( Exception e )
			{
				Log.Warning( e.Message );
			}

			SerializeEnt.EndLoad();

			WorldLoaded = true;
		}

		[Event.Tick.Server]
		public void SaveWorldTimer()
		{
			if ( NextSave == 0f )
				NextSave = RealTime.Now + RPGGlobals.SaveWorldTime;
			else if ( RealTime.Now >= NextSave )
				SaveWorld();
		}

		/*[ServerCmd( "testsaving" )]
		public static void TestSaving()
		{
			if ( ConsoleSystem.Caller?.Pawn is RPGPlayer player )
				(Current as RPGGame).SavePlayer( player );
		}*/

		[ServerCmd( "saveworld" )]
		public static void ServerCmdSaveWorld()
		{
			if ( Global.IsDedicatedServer ) return;

			(Current as RPGGame).SaveWorld();
		}
	}
}
