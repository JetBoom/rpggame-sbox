using Sandbox;
using Sandbox.UI;
using RPG.UI;


namespace RPG
{
	public partial class RPGGame : Game
	{
		private static int NetworkIdentityIncrement = 0;

		public static int NewNetworkIdentity() => ++NetworkIdentityIncrement;

		public RPGHud HUD { get; init; }

		private bool ManagersLoaded;

		public static RPGGame RPGCurrent { get; private set; }

		public RPGGame()
		{
			RPGCurrent = this;

			if ( IsServer )
			{
				InitManagers();

				UserPermissions.Load();
			}

			if ( IsClient )
				HUD = new RPGHud();
		}

		private void InitManagers()
		{
			ManagersLoaded = true;

			ItemManager.Init();
			AbilityManager.Init();
			StatusManager.Init();
		}

		[Event.Tick.Client]
		protected void OnTickClient()
		{
			if ( !ManagersLoaded )
			{
				// hacky way to do this since no other method I can think of.
				var data = Resource.FromPath<ItemData>( "data/items/item_stone.item" );
				if ( data != null )
					InitManagers();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			LoadPlayer( client );

			UserPermissions.UpdateUserPermissionsComponent( client );
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			Log.Info( $"\"{cl.Name}\" has left the game ({reason})" );
			ChatBox.AddInformation( To.Everyone, $"{cl.Name} has left ({reason})", $"avatar:{cl.SteamId}" );

			SavePlayer( cl );

			if ( cl.Pawn.IsValid() )
			{
				if ( RPGGlobals.PlayerLogoutTime > 0f && cl.Pawn is RPGPlayer player )
				{
					player.AddStatus<StatusLogout>();
				}
				else
				{
					cl.Pawn.Delete();
					cl.Pawn = null;
				}
			}
		}

		public override bool CanHearPlayerVoice( Client source, Client dest )
		{
			var sp = source.Pawn;
			var dp = dest.Pawn;
			return sp != null && dp != null && (sp.Position - dp.Position).LengthSquared <= RPGGlobals.VoiceDistSqr;
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			if ( IsServer )
				LoadWorld();
		}

		public override void Shutdown()
		{
			if ( IsServer )
				SaveWorld();

			base.Shutdown();
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was killed" );
		}

		public virtual void OnIncapacitated( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was incapacitated" );
		}

		public virtual void OnIncapacitated( Entity pawn )
		{
			Host.AssertServer();

			if ( pawn.Client != null )
			{
				OnIncapacitated( pawn.Client, pawn );
				return;
			}
		}
	}
}
