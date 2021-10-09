using Sandbox;


namespace RPG
{
	[Library( "item_battleaxe" )]
	public partial class ItemBattleAxe : ItemMelee { }

	[Library( "ent_item_battleaxe" )]
	public partial class ItemBattleAxeEntity : ItemMeleeEntity
	{
		public override string AttackSoundPath => "";
		public override string ProcessSoundPath => "";
		public override string HitFleshSoundPath => "";
	}
}
