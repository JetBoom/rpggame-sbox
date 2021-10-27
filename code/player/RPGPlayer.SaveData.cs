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
		}

		private async void GoToSpawnIn( int ms = 10 )
		{
			await GameTask.Delay( ms );
			if ( this.IsValid() )
				Game.Current?.MoveToSpawnpoint( this );
		}
	}
}
