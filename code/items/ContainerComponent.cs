using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RPG.UI;


namespace RPG
{
	/// <summary>
	/// A container component for an entity. Anything that can have items stored inside of it should have one of these.<br/>
	/// To setup something with a container it should override and call Container?.OnChildAdded( entity ) and Container?.OnChildRemoved( entity )<br/>
	/// </summary>
	[Library]
	public partial class ContainerComponent : EntityComponent
	{
		/// <summary>Max item count.</summary>
		public virtual int MaxItems => 25;

		[Net, Change( nameof( OnItemsChanged ) )]
		protected List<Item> Items { get; set; } = new();

		[Net]
		public int NetworkIdentity { get; protected set; }

		/// <summary>All valid items in the container right now.</summary>
		public IReadOnlyList<Item> ItemList => (IReadOnlyList<Item>)Items; //Items.Where( item => item.IsValid() ).ToList();
		/// <summary>All valid items of a specific type in the container right now.</summary>
		/// <remarks>Unlike the generic-type version of this method, only exact types are returned.</remarks>
		public IReadOnlyList<Item> GetItemsOfType( string classname ) => Items.Where( item => item.ClassInfo.Name == classname ).ToList();
		/// <summary>All valid items of a specific type in the container right now.</summary>
		/// <remarks>This WILL return subclasses of type T.</remarks>
		public IReadOnlyList<T> GetItemsOfType<T>() where T : Item
		{
			List<T> list = new();
			foreach ( var item in Items )
			{
				if ( item is T )
					list.Add( item as T );
			}
			return list;
		}
		/// <summary>All valid, currently equipped items.</summary>
		public IReadOnlyList<ItemEquippable> GetEquippedItems()
		{
			List<ItemEquippable> equips = new();
			foreach ( var item in Items )
			{
				if ( item is ItemEquippable equip && equip.IsEquipped )
					equips.Add( equip );
			}
			return equips;
		}

		public int Count => Items.Count;

		public List<ItemEntity> ItemEntities { get; init; } = new();

		public RPGPlayer Player => Entity is RPGPlayer player ? player : null;

		/// <summary>The container owner's active ItemEntity, if there is one.</summary>
		public ItemEntity ActiveItemEntity => Entity?.ActiveChild as ItemEntity;
		/// <summary>The container owner's active ItemEntity's Item if there is one.</summary>
		public Item ActiveItem => ActiveItemEntity?.Item;

		/// <summary>Utility to see how much money this container has.</summary>
		public int Money => GetAmount<ItemMoney>();

		public ContainerComponent() : base()
		{
			if ( Host.IsServer )
				NetworkIdentity = RPGGame.NewNetworkIdentity();
		}

		protected virtual void OnItemsChanged()
		{
			if ( !Host.IsClient ) return;

			Log.Info( "Items changed" );

			var panel = ContainerPanel.GetPanelFor( NetworkIdentity );
			if ( panel != null )
				panel.UpdateAll();
		}

		public Item GetItemOfType( string classname )
		{
			//return Items.Find( item => item.ClassInfo.Name == classname );
			foreach ( var item in Items )
			{
				if ( item.ClassInfo.Name == classname )
					return item;
			}

			return null;
		}

		public bool RemoveItem( Item item )
		{
			var index = Items.IndexOf( item );
			if ( index == -1 ) return false;

			if ( item.Container == this )
				item.Container = null;

			if ( item.ItemEntity != null )
				ItemEntities.Remove( item.ItemEntity );

			Items.Remove( item );

			return true;
		}

		public int FindFreeIndex() => Items.IndexOf( null );

		public void ChangeItemIndex( Item item, int index )
		{
			if ( item.Container != this ) return;

			var currentIndex = Items.IndexOf( item );
			if ( currentIndex == -1 ) return;

			if ( index < -1 || index >= Items.Count )
				throw new Exception( $"ChangeItemTile: Item {item} index {index} is out of range [-1 - {Items.Count - 1}]" );

			if ( index == -1 )
			{
				index = FindFreeIndex();
				if ( index == -1 )
					return;
			}

			var existingItem = Items[index];

			Items[index] = item;
			Items[currentIndex] = existingItem;
		}

		/// <summary>A child has been added to the Owner (player). Add it to the container if it's an ItemEntity.</summary>
		public void OnChildAdded( Entity child )
		{
			if ( child is not ItemEntity itemEntity || !itemEntity.EntityShouldExistInContainer ) return;

			if ( ItemEntities.Contains( itemEntity ) )
				throw new Exception( "Trying to add to inventory multiple times. This is gated by Entity:OnChildAdded and should never happen!" );

			/*if ( !itemEntity.EntityShouldExistInContainer )
				throw new Exception( "Trying to add to inventory container an ItemEntity with EntityShouldExistInContainer = false. This should never happen!" );*/

			if ( Host.IsServer && !Items.Contains( itemEntity.Item ) )
				throw new Exception( $"An ItemEntity was added to {this} but the Item wasn't in the item list. You cannot call SetParent directly on ItemEntity!" );

			ItemEntities.Add( itemEntity );

			if ( itemEntity.IsValid() )
				itemEntity.RefreshState();

			Player?.InvalidateStatus();

			return;
		}

		/// <summary>A child has been removed from our Owner.</summary>
		public void OnChildRemoved( Entity child )
		{
			if ( child is not ItemEntity itemEntity ) return;

			if ( ItemEntities.Remove( itemEntity ) )
			{
				if ( itemEntity.IsValid() )
					itemEntity.RefreshState();

				Player?.InvalidateStatus();
			}
		}

		public Item GetItemByNetId( int netid )
		{
			//return Items.Find( item => item.NetworkIdent == netid );
			foreach ( var item in Items )
			{
				if ( item.NetworkIdentity == netid )
					return item;
			}

			return null;
		}

		/// <summary>Delete every item we have.</summary>
		public void DeleteContents()
		{
			Host.AssertServer();

			List<Item> toDelete = new();
			foreach ( var item in Items )
				toDelete.Add( item );

			foreach ( var item in toDelete )
				RemoveItem( item );

			Player?.InvalidateStatus();
		}

		/// <summary>Can this player move the items around, in to, or out of this container?</summary>
		public virtual bool CanManageItems( RPGPlayer player )
		{
			return player.GetContainer() == this;
		}

		/// <summary>A player is trying to drop this item.</summary>
		public ItemEntity PlayerTryDrop( RPGPlayer player, Item item, int amount = -1 )
		{
			if ( !Host.IsServer ) return null;

			if ( !Contains( item ) || item.Amount < 1 ) return null;

			if ( !CanManageItems( player ) ) return null;

			// Drop all by default.
			if ( amount == -1 || amount > item.Amount || item.Data.MaxAmount == 1 )
				amount = item.Amount;

			var ent = item.ToEntity();
			if ( !ent.IsValid() ) return null;

			if ( ent.Parent == Entity && Entity.IsValid() )
			{
				// Entity already existed and was already inside of us.
				RemoveItem( item );

				ent.Parent = null;
				ent.Owner = null;
			}
			else if ( amount == item.Amount )
			{
				// Dropping the entire stack of this item.
				RemoveItem( item );
			}
			else
			{
				// Dropping an incomplete stack, so we have to make a clone of this item...
				// Note: We don't support cloning unique properties on stackable items
				var cloneItem = Library.Create<Item>( item.ClassInfo.Name );
				cloneItem.Amount = amount;
				item.Amount -= amount;

				ent.Item = cloneItem;
			}

			ent.Transform = GetDropTransform();
			ent.RefreshState();

			return ent;
		}

		public Transform GetDropTransform()
		{
			Transform transform = new();

			if ( Player.IsValid() )
			{
				transform.Position = Player.EyePos;
				transform.Rotation = Player.EyeRot;
			}
			else if ( Entity.IsValid() )
			{
				transform.Position = Entity.WorldSpaceBounds.Center.WithZ( Entity.WorldSpaceBounds.Maxs.z + 1f );
				transform.Rotation = Entity.Rotation;
			}

			return transform;
		}

		public bool Contains( Item item ) => Items.Contains( item );
		public bool Contains( ItemEntity itemEntity ) => ItemEntities.Contains( itemEntity );

		public bool Contains( string itemClassName, int checkAmount = 1 )
		{
			var count = 0;

			var items = GetItemsOfType( itemClassName );
			foreach ( var item in items )
			{
				count += item.Amount;
				if ( count >= checkAmount )
					return true;
			}

			return false;
		}

		/// <summary>Make this entity the active one</summary>
		public bool SetActive( ItemEquippableEntity ent, bool force = false )
		{
			var current = ActiveItemEntity;

			if ( !Entity.IsValid() ) return false;
			if ( current == ent ) return true;
			if ( !Contains( ent ) ) return false;

			if ( !force )
			{
				if ( Player.IsValid() && Player.GetIsCasting() ) return false;
				if ( !ent.CanActivate() ) return false;
				if ( current.IsValid() && !current.CanDeactivate() ) return false;
			}

			Entity.ActiveChild = ent;

			return true;
		}

		public bool CanFit( Item item ) => GetHowManyCanFit( item ) >= item.Amount;

		public bool CanFit( string itemClassName ) => GetHowManyCanFit( itemClassName ) >= 1;

		public int GetFreeInventorySlots()
		{
			/*int count = 0;
			for ( int i = 0; i < Items.Count; ++i )
			{
				if ( Items[i] == null )
					++count;
			}
			return count;*/
			return MaxItems - Items.Count;
		}

		public int GetHowManyCanFit( Item item ) => GetHowManyCanFit( item.ClassInfo.Name );

		public int GetHowManyCanFit( string itemClassName )
		{
			var data = ItemManager.Get( itemClassName );
			if ( data == null ) return 0;

			var freeInventorySlots = GetFreeInventorySlots();

			// If this is a single stack item, we can fit as many slots are left in the inventory.
			if ( data.MaxAmount == 1 )
				return freeInventorySlots;

			// Find stacks with enough space.
			int fit = 0;

			fit += freeInventorySlots * data.MaxAmount;

			// Find any existing items with a stack less than its max. We can fill those in.
			var items = GetItemsOfType( itemClassName );
			foreach ( var existingItem in items )
			{
				if ( existingItem.Amount < data.MaxAmount )
					fit += data.MaxAmount - existingItem.Amount;
			}

			return fit;
		}

		public int GetAmount( string itemClassName )
		{
			int amount = 0;

			var items = GetItemsOfType( itemClassName );
			foreach ( var item in items )
				amount += item.Amount;

			return amount;
		}

		public int GetAmount<T>() where T : Item
		{
			int amount = 0;

			var items = GetItemsOfType<T>();
			foreach ( var item in items )
				amount += item.Amount;

			return amount;
		}

		/// <summary>Add this item to the inventory.</summary>
		public bool AddItem( Item item, bool onlyAddEntireStack = false/*, bool ignoreCapacityLimits = false*/ )
		{
			Host.AssertServer();

			if ( item.Container == this )
				return true;

			int maxAmount = item.Data.MaxAmount;

			// Can skip a lot of cycles here.
			if ( maxAmount == 1 )
			{
				if ( /*ignoreCapacityLimits ||*/ CanFit( item ) )
				{
					item.Container?.RemoveItem( item );
					Items.Add( item );
					item.Container = this;

					return true;
				}

				return false;
			}

			int canFit = GetHowManyCanFit( item );

			if ( onlyAddEntireStack && canFit < item.Amount )
				return false;

			string className = item.ClassInfo.Name;
			int newAmount = item.Amount;

			// Pull stacks from the item in to already existing stacks in the inventory.
			var items = GetItemsOfType( className );
			foreach ( var myItem in items )
			{
				if ( myItem.Amount < maxAmount )
				{
					int toadd = Math.Min( maxAmount - myItem.Amount, newAmount );
					myItem.Amount += toadd;
					newAmount -= toadd;

					if ( newAmount <= 0 ) break;
				}
			}

			item.Amount = newAmount;

			if ( newAmount >= 1 )
			{
				// Pull the remaining stacks in as the existing item.
				item.Container?.RemoveItem( item );
				Items.Add( item );
				item.Container = this;
			}
			else
				item.Delete();

			return true;
		}

		public bool AddItem( string classname, bool onlyAddEntireStack = false/*, bool ignoreCapacityLimits = false*/)
		{
			return AddItem( classname, 1, onlyAddEntireStack/*, ignoreCapacityLimits*/ );
		}

		public bool AddItem( string classname, int amount, bool onlyAddEntireStack = false/*, bool ignoreCapacityLimits = false*/ )
		{
			var item = Library.Create<Item>( classname );
			if ( item != null )
			{
				item.Amount = amount;
				var added = AddItem( item, onlyAddEntireStack/*, ignoreCapacityLimits*/ );
				if ( item.Container != this )
					item.Delete();

				return added;
			}

			return false;
		}
	}

	public static class ContainerExtend
	{
		public static ContainerComponent GetContainer( this Entity self ) => self.Components.Get<ContainerComponent>();

		public static bool HasContainer( this Entity self ) => self.GetContainer() != null;
	}
}
