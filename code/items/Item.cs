using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;


namespace RPG
{
	[Library( Group = "base" )]
	public abstract partial class Item : BaseNetworkable, IJsonSerializable
	{
		//public static readonly Dictionary<int, Item> AllItems = new();

		private ItemData _Data;
		public ItemData Data => _Data == null ? _Data = Resource.FromPath<ItemData>( $"data/items/{ClassInfo.Name}.item" ) : _Data;

		public ContainerComponent Container { get; set; }
		public ItemEntity ItemEntity;
		public Entity OwnerEntity => Container?.Entity;
		public RPGPlayer OwnerPlayer => Container?.Player;

		[Net]
		protected ushort _Amount { get; set; } = 1;
		public int Amount
		{
			get => _Amount;
			set
			{
				_Amount = (ushort)value;
			}
		}

		[Net]
		public int NetworkIdentity { get; protected set; }

		/*private ushort _Amount { get; set; } = 1;
		public ushort Amount
		{
			get => _Amount;
			set
			{
				if ( _Amount != value )
				{
					_Amount = value;

					if ( value <= 0 )
						Delete();
				}
			}
		}*/

		public Item()
		{
			if ( Host.IsServer )
				NetworkIdentity = RPGGame.NewNetworkIdentity();
		}

		public Client Client
		{
			get
			{
				var pl = OwnerPlayer;
				if ( pl.IsValid() )
					return pl.Client;

				return null;
			}
		}

		/// <summary>Spawns an ItemEntity and then sets its Item to this. If the entity already exists then just returns the already existing entity.</summary>
		public ItemEntity ToEntity( bool removeFromContainer = true )
		{
			Host.AssertServer();

			if ( ItemEntity.IsValid() ) return ItemEntity;

			var ent = Library.Create<ItemEntity>( $"ent_{ClassInfo.Name}" );
			if ( ent.IsValid() )
			{
				if ( removeFromContainer )
					Container?.RemoveItem( this );
				ent.Item = this;
			}

			return ent;
		}

		public void Delete()
		{
			Host.AssertServer();

			ItemEntity?.Delete();

			Container?.RemoveItem( this );
		}

		public virtual void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
			if ( Amount != 1 )
				writer.WriteNumber( nameof( Amount ), Amount );
		}

		public virtual bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			switch ( propertyName )
			{
				case "Amount":
					reader.Read();
					Amount = reader.GetInt32();
					return true;
			}

			return false;
		}

		public static Item FindIn( Entity ent, int netid )
		{
			if ( !ent.IsValid() ) return null;

			var container = ent.GetContainer();
			if ( container != null )
			{
				var item = container.GetItemByNetId( netid );
				if ( item != null ) return item;
			}

			if ( ent is ItemEntity itement && itement.Item?.NetworkIdentity == netid )
				return itement.Item;

			return null;
		}

		/*public virtual void SerializeMemory( BinaryWriter writer )
		{
			//writer.Write( NetId );

			if ( Data.MaxAmount != 1 )
				writer.Write( (ushort)Amount );

			writer.Write( TileIndex );
		}

		public virtual void DeserializeMemory( BinaryReader reader )
		{
			//var oldNetId = NetId;
			//NetId = reader.ReadInt32();

			// Kinda hacky but if the netid changes for some reason we have to update the dictionary for it.
			//if ( Host.IsClient )
			//{
			//	if ( NetId != oldNetId )
			//	{
			//		var oldItem = GetItemByNetId( oldNetId );
			//		if ( oldItem == this )
			//			AllItems.Remove( oldNetId );
			//	}
			//	AllItems[NetId] = this;
			//}

			if ( Data.MaxAmount != 1 )
				Amount = reader.ReadUInt16();

			TileIndex = reader.ReadByte();
		}*/
	}

	public static class ItemExtend
	{
	}
}
