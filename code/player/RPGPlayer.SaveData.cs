using Sandbox;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;


namespace RPG
{
	public partial class RPGPlayer : ISavedEntity
	{
		public bool ShouldPersistInWorldFile() => false;

		public virtual bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			switch ( propertyName )
			{
				case "Map":
					reader.Read();
					if ( reader.GetString() != Global.MapName )
						GoToSpawnIn();
					break;
				case nameof( TotalXP ):
					reader.Read();
					TotalXP = reader.GetUInt32();
					break;
				case nameof( UsedXP ):
					reader.Read();
					UsedXP = reader.GetUInt32();
					break;
				case nameof( Skills ):
					int skillId = 0;

					while ( reader.Read() )
					{
						if ( reader.TokenType == JsonTokenType.StartArray ) continue;
						if ( reader.TokenType == JsonTokenType.EndArray ) break;
						if ( reader.TokenType == JsonTokenType.Number )
							SetSkill( skillId++, reader.GetInt32() );
					}

					break;
				case nameof( UnlockedAbilities ):
					UnlockedAbilities.Clear();

					while ( reader.Read() )
					{
						if ( reader.TokenType == JsonTokenType.StartArray ) continue;
						if ( reader.TokenType == JsonTokenType.EndArray ) break;
						if ( reader.TokenType == JsonTokenType.String )
							UnlockedAbilities.Add( reader.GetString() );
					}

					break;
				case "ContainerItems":
					var container = this.GetContainer();
					if ( container == null ) break;

					var converter = options.GetConverter( typeof( Item ) ) as JsonConverterItem;

					while ( reader.Read() )
					{
						if ( reader.TokenType == JsonTokenType.StartArray ) continue;
						if ( reader.TokenType == JsonTokenType.EndArray ) break;
						if ( reader.TokenType == JsonTokenType.StartObject )
						{
							var item = converter.Read( ref reader, typeof( Item ), options );
							if ( item != null )
								container.AddItem( item );
						}
					}

					break;
				/*case "ContainerEntityItems":
					var entconverter = options.GetConverter( typeof( Entity ) ) as JsonConverterEntity;

					while ( reader.Read() )
					{
						if ( reader.TokenType == JsonTokenType.StartArray ) continue;
						if ( reader.TokenType == JsonTokenType.EndArray ) break;
						if ( reader.TokenType == JsonTokenType.StartObject )
						{
							var itemEntity = entconverter.Read( ref reader, typeof( Entity ), options );
							if ( itemEntity.IsValid() )
							{
								itemEntity.Position = WorldSpaceBounds.Center;
								itemEntity.Owner = this;
								itemEntity.SetParent( this, null, Transform.Zero );
							}
						}
					}

					break;*/
				default:
					return false;
			}

			return true;
		}

		public virtual void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
			writer.WriteString( "Map", Global.MapName );

			writer.WriteNumber( nameof( TotalXP ), TotalXP );
			writer.WriteNumber( nameof( UsedXP ), UsedXP );

			writer.WritePropertyName( nameof( Skills ) );
			writer.WriteStartArray();
			for ( int i = 0; i < Skills.Count; ++i )
				writer.WriteNumberValue( Skills[i] );
			writer.WriteEndArray();

			if ( UnlockedAbilities.Count > 0 )
			{
				writer.WritePropertyName( nameof( UnlockedAbilities ) );
				writer.WriteStartArray();
				for ( int i = 0; i < UnlockedAbilities.Count; ++i )
					writer.WriteStringValue( UnlockedAbilities[i] );
				writer.WriteEndArray();
			}

			var container = this.GetContainer();
			if ( container != null && container.Count > 0 )
			{
				var items = container.ItemList;
				//var ents = container.ItemEntities;

				var itemConverter = options.GetConverter( typeof( Item ) ) as JsonConverterItem;

				writer.WritePropertyName( "ContainerItems" );
				writer.WriteStartArray();
				foreach ( var item in items )
				{
					if ( !item.ItemEntity.IsValid() )
						itemConverter.Write( writer, item, options );
				}
				writer.WriteEndArray();

				/*if ( ents.Count > 0 )
				{
					var entityConverter = options.GetConverter( typeof( Entity ) ) as JsonConverterEntity;

					writer.WritePropertyName( "ContainerEntityItems" );
					writer.WriteStartArray();
					foreach ( var ent in ents )
						entityConverter.Write( writer, ent, options );
					writer.WriteEndArray();
				}*/
			}
		}

		private async void GoToSpawnIn( int ms = 10 )
		{
			await GameTask.Delay( ms );
			if ( this.IsValid() )
				Game.Current?.MoveToSpawnpoint( this );
		}
	}
}
