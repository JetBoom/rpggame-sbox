using Sandbox;
using System.Text.Json;

namespace RPG
{
	[Library( "item_book_ability" )]
	public partial class ItemBookAbility : Item
	{
		[Net]
		public string AbilityClass { get; set; }

		public override string GetDisplayInfo()
		{
			var data = AbilityManager.Get( AbilityClass );
			if ( data == null )
				return $"Book of {AbilityClass}";

			return $"Book of {data.DisplayName}";
		}

		public override void OnUsed( RPGPlayer player )
		{
			base.OnUsed( player );

			if ( string.IsNullOrEmpty( AbilityClass ) ) return;
			if ( player.UnlockedAbilities.Contains( AbilityClass ) ) return;

			var data = AbilityManager.Get( AbilityClass );
			if ( data != null || data.School > Schools.Common ) return;

			player.UnlockedAbilities.Add( AbilityClass );

			Delete();
		}

		public override void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
			base.Serialize( writer, options );

			writer.WriteString( nameof( AbilityClass ), AbilityClass );
		}

		public override bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			switch ( propertyName )
			{
				case nameof( AbilityClass ):
					reader.Read();
					AbilityClass = reader.GetString();
					return true;
			}

			return base.DeserializeProperty( propertyName, ref reader, options );
		}
	}

	[Library("ent_item_book_ability")]
	public partial class ItemBookAbilityEntity : ItemEntity
	{
	}
}
