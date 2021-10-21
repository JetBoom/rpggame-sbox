using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace RPG
{
	public partial class Item
	{
		/// <summary>The transient network ID for this item. Is only serialized in memory, not saved to files.<br />
		/// Used mainly for RPC calls.</summary>
		/*public int NetId;
		private static int NextNetId;

		protected static readonly List<Client> emptyClients = new();

		/// <summary>Who needs to know about our network updates?</summary>
		public IReadOnlyList<Client> GetNetVisible()
		{
			// If there is an ItemEntity, the ItemEntity will have the info stored on it as a [Net] property.
			// There's no need to network with this method if so.
			if ( ItemEntity.IsValid() )
				return emptyClients;

			List<Client> clients = new();

			if ( Client.IsValid() )
				clients.Add( Client );

			// TODO: Whoever is viewing our container?

			return clients;
		}

		/// <summary>Call this whenever anything networkable about this item changes.</summary>
		public void InvalidateNetState()
		{
			if ( !Host.IsServer ) return;

			ItemsThatNeedNetUpdates.Add( this );
		}

		private void NetworkUpdate()
		{
			var pvs = GetNetVisible();

			// No one cares about us?
			if ( pvs.Count == 0 && !ItemEntity.IsValid() ) return;

			MemoryStream stream = new();
			using ( BinaryWriter writer = new( stream, Encoding.UTF8 ) )
			{
				writer.Write( NetId );
				writer.Write( OwnerEntity.IsValid() ? OwnerEntity.NetworkIdent : 0 );
				writer.Write( ClassInfo.Name ); // TODO: Can probably be tokenized to a number (index from ItemManager)

				SerializeMemory( writer );
			}

			var bytes = stream.ToArray();

			stream.Dispose();

			if ( ItemEntity.IsValid() )
				ItemEntity.ItemSerialized = bytes;
			else
				ClientRpcNetworkUpdate( To.Multiple( pvs ), bytes );
		}

		public static void UpdateItemFromBinary( byte[] data, Item item = null )
		{
			MemoryStream stream = new( data );
			using ( BinaryReader reader = new( stream, Encoding.UTF8 ) )
			{
				var netid = reader.ReadInt32();
				var ownerNetworkIdent = reader.ReadInt32();
				var className = reader.ReadString();

				if ( item == null )
					item = Item.GetItemByNetId( netid );

				// If we're getting updates for an item we don't know about, well we have to make it first then.
				if ( item == null )
				{
					item = Library.Create<Item>( className );
					item.NetId = netid;
				}

				// Make sure we're in the right container.
				ContainerComponent targetContainer = null;
				if ( ownerNetworkIdent != 0 && Entity.FindByIndex( ownerNetworkIdent ) is IHasContainer entOwner )
					targetContainer = entOwner.Container;
				item.TransferTo( targetContainer );

				// Let the item handle any custom stuff.
				item.DeserializeMemory( reader );
			}

			stream.Dispose();
		}

		[ClientRpc]
		public static void ClientRpcNetworkUpdate( byte[] data )
		{
			UpdateItemFromBinary( data );
		}

		private static readonly HashSet<Item> ItemsThatNeedNetUpdates = new();

		[Event.Tick.Server]
		public static void AllItemsNetworkingTick()
		{
			if ( ItemsThatNeedNetUpdates.Count == 0 ) return;

			foreach ( var item in ItemsThatNeedNetUpdates )
			{
				if ( item.IsValid )
					item.NetworkUpdate();
			}

			ItemsThatNeedNetUpdates.Clear();
		}

		public static Item GetItemByNetId( int netid )
		{
			if ( AllItems.TryGetValue( netid, out Item item ) )
				return item;

			return null;
		}*/

		[ServerCmd]
		public static void ServerCmdUseItem( Entity ent, int itemNetId )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.IsAlive() ) return;

			var container = ent.GetContainer();
			if ( container == null ) return;

			var item = FindIn( ent, itemNetId );
			if ( item == null ) return;

			Log.Info( $"// TODO: Item {item.ClassInfo.Name}#{itemNetId} was used by {player.Client.Name}." );

			container.PlayerTryUse( player, item );
		}

		[ServerCmd]
		public static void ServerCmdEquipItem( int itemNetId, bool shouldEquip )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.IsAlive() ) return;

			var container = player.GetContainer();
			if ( container == null ) return;

			var item = FindIn( player, itemNetId );
			if ( item == null || item is not ItemEquippable equip ) return;

			container.EquipItem( equip, shouldEquip );

			Log.Info( $"// Item {equip.ClassInfo.Name}#{itemNetId} was {(shouldEquip ? "" : "un")}equipped by {player.Client.Name}." );
		}

		[ServerCmd]
		public static void ServerCmdDropItem( Entity ent, int itemNetId, int amount )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.IsAlive() ) return;

			var item = FindIn( ent, itemNetId );
			if ( item == null || item.Container == null ) return;

			item.Container.PlayerTryDrop( player, item, amount < 1 ? -1 : Math.Min( amount, item.Amount ) );
		}

		[ServerCmd]
		public static void ServerCmdMoveItem( Entity sourceEntity, int itemNetId, Entity targetEntity, int tile )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.IsAlive() ) return;

			if ( !sourceEntity.IsValid() || !targetEntity.IsValid() ) return;

			var sourceContainer = targetEntity.GetContainer();
			if ( sourceContainer == null || !sourceContainer.CanManageItems( player ) ) return;

			var targetContainer = targetEntity.GetContainer();
			if ( targetContainer == null || !targetContainer.CanManageItems( player ) ) return;

			var item = FindIn( sourceEntity, itemNetId );
			if ( item == null ) return;

			if ( !sourceContainer.CanWithdraw( player, item ) || !targetContainer.CanDeposit( player, item ) ) return;

			if ( targetContainer.AddItem( item, true ) )
			{
				targetContainer.ChangeItemIndex( item, tile );

				Log.Info( $"// TODO: Item {item.ClassInfo.Name}#{itemNetId} in {sourceEntity} move to {targetEntity} in to tile {tile} by {player.Client.Name}." );
			}
		}

		[ServerCmd( "rpg_use_item" )]
		public static void ServerCmdUseItemType( string itemType )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;

			if ( !player.IsAlive() ) return;

			var container = player.GetContainer();
			if ( container == null ) return;

			var item = container.GetItemOfType( itemType );
			if ( item == null ) return;

			Log.Info( $"// TODO: Item {item.ClassInfo.Name}#{item.NetworkIdentity} was by-type used by {player.Client.Name}." );
		}
	}
}
