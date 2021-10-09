using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{
	public class JsonConverterListItem : JsonConverter<List<Item>>
	{
		public override List<Item> Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartArray )
				throw new JsonException( "First token was not a StartArray!" );

			var converter = options.GetConverter( typeof( Item ) ) as JsonConverterItem;

			List<Item> list = new();

			while ( reader.Read() )
			{
				if ( reader.TokenType == JsonTokenType.EndArray ) break;
				if ( reader.TokenType == JsonTokenType.StartObject )
					list.Add( converter.Read( ref reader, typeof( Item ), options ) );
			}

			return list;
		}

		public override void Write( Utf8JsonWriter writer, List<Item> list, JsonSerializerOptions options )
		{
			var converter = options.GetConverter( typeof( Item ) ) as JsonConverterItem;

			writer.WriteStartArray();

			for ( int i = 0; i < list.Count; ++i )
				converter.Write( writer, list[i], options );

			writer.WriteEndArray();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( List<Item> );
		}
	}
}
