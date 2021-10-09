using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{

	public class JsonConverterListEntity : JsonConverter<List<Entity>>
	{
		public override List<Entity> Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartArray )
				throw new JsonException( "First token was not a StartArray!" );

			var entityConverter = options.GetConverter( typeof( Entity ) ) as JsonConverterEntity;

			List<Entity> list = new();

			while ( reader.Read() )
			{
				if ( reader.TokenType == JsonTokenType.EndArray ) break;
				if ( reader.TokenType == JsonTokenType.StartObject )
					list.Add( entityConverter.Read( ref reader, typeof( Entity ), options ) );
			}

			return list;
		}

		public override void Write( Utf8JsonWriter writer, List<Entity> list, JsonSerializerOptions options )
		{
			var converter = options.GetConverter( typeof( Entity ) ) as JsonConverterEntity;

			writer.WriteStartArray();

			for ( int i = 0; i < list.Count; ++i )
				converter.Write( writer, list[i], options );

			writer.WriteEndArray();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( List<Entity> );
		}
	}
}
