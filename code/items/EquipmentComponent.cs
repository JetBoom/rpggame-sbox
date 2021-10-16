using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Should this really be a separate component? Probably not.

namespace RPG
{
	/// <summary>Required component for anything that allows equipping of weapons, armor, and other items.</summary>
	/// <remarks>This is different than a <seealso cref="ContainerComponent"/>, which is for storing inactive items.</remarks>
	[Library]
	public partial class EquipmentComponent : EntityComponent
	{
		[Net, Change( nameof( OnEquipmentChanged ) )]
		protected List<ItemEquippable> Equipment { get; set; } = new();

		/// <summary>All items handled by this component, ie all items equipped by the owner.</summary>
		public IReadOnlyList<ItemEquippable> EquipmentList => (IReadOnlyList<ItemEquippable>)Equipment;

		private void OnEquipmentChanged()
		{
			if ( Entity is IUseStatusMods modder )
				modder.InvalidateStatus();
		}
	}

	public static class EquipmentComponentStatic
	{
		public static EquipmentComponent GetEquipmentComponent( this Entity self ) => self.Components.Get<EquipmentComponent>();

		public static bool HasEquipment( this Entity self ) => self.GetEquipmentComponent() != null;
	}
}
