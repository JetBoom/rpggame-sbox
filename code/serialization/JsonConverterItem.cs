using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{
	public class JsonConverterItem : JsonConverter<Item>
	{
		public override Item Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartObject )
				throw new JsonException( "First token was not a StartObject!" );

			Item item = null;

			while ( reader.Read() )
			{
				if ( reader.TokenType == JsonTokenType.EndObject ) break;

				if ( reader.TokenType == JsonTokenType.PropertyName )
				{
					var propertyName = reader.GetString();

					switch ( propertyName )
					{
						case "$class":
							reader.Read();
							string className = reader.GetString();

							item = Library.Create<Item>( className );
							if ( item == null )
							{
								Log.Warning( $"Failed to Deserialize on something with $class \"${className}\". Does it not have a matching Library attribute? Skipping!" );
								reader.Skip();
								return null;
							}

							break;
						default:
							try
							{
								item.DeserializeProperty( propertyName, ref reader, options );
							}
							catch ( Exception readException )
							{
								Log.Warning( readException.Message );
							}

							break;
					}
				}
			}

			return item;
		}

		public override void Write( Utf8JsonWriter writer, Item item, JsonSerializerOptions options )
		{
			writer.WriteStartObject();
			writer.WriteString( "$class", item.ClassInfo.Name );
			item.Serialize( writer, options );
			writer.WriteEndObject();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( Item ) || typeToConvert.IsSubclassOf( typeof( Item ) );
		}
	}
}
