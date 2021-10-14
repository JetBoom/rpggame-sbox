using Sandbox;
using System.Runtime.Serialization;
using System.Text.Json;


namespace RPG
{
	public enum EquipSlot : int
	{
		Weapon,
		Head,
		Top,
		Bottom,
		Amulet,
	}

	[Library( Group = "base" )]
	public abstract partial class ItemEquippable : Item
	{
		[Net]
		public bool IsEquipped { get; set; }

		public virtual EquipSlot Slot => EquipSlot.Head;

		public override bool EntityShouldExistInContainer => true;

		/*public override void OnDroppedFromInventory( RPGPlayer player, Entity oldOwner )
		{
			IsEquipped = false;

			base.OnDroppedFromInventory( player, oldOwner );
		}*/

		public virtual void ContributeModifications( StatusModifications mods )
		{
		}
	}

	/// <summary>An item which can be put in an equipment slot.</summary>
	[Library( Group = "base" )]
	public abstract partial class ItemEquippableEntity : ItemEntity
	{
		public ItemEquippable ItemEquippable => Item as ItemEquippable;

		public override void ActiveStart( Entity ent )
		{
			ItemEquippable.IsEquipped = true;

			if ( ent is IUseStatusMods modder )
				modder.InvalidateStatus();

			base.ActiveStart( ent );
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			ItemEquippable.IsEquipped = false;

			if ( ent is IUseStatusMods modder )
				modder.InvalidateStatus();

			base.ActiveEnd( ent, dropped );
		}

		public override TransmitType GetDesiredTransmitType()
		{
			if ( ItemEquippable.IsEquipped && Owner is RPGPlayer player && player.IsValid() )
				return TransmitType.Default;

			return base.GetDesiredTransmitType();
		}
	}
}
