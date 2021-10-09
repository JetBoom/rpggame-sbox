using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{
	public class JsonConverterLevelPermissions : JsonConverter<LevelPermissions>
	{
		public override LevelPermissions Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartObject )
				throw new JsonException( "First token was not a StartObject!" );

			UserLevel level = 0;
			Permissions permissions = 0;

			while ( reader.Read() )
			{
				if ( reader.TokenType == JsonTokenType.EndObject ) break;

				if ( reader.TokenType == JsonTokenType.PropertyName )
				{
					var propertyName = reader.GetString();

					switch ( propertyName )
					{
						case "level":
							reader.Read();
							level = (UserLevel)reader.GetByte();
							break;
						case "permissions":
							reader.Read();
							permissions = (Permissions)reader.GetUInt16();
							break;
					}
				}
			}

			return new LevelPermissions() { Permissions = permissions, UserLevel = level };
		}

		public override void Write( Utf8JsonWriter writer, LevelPermissions obj, JsonSerializerOptions options )
		{
			writer.WriteStartObject();
			writer.WriteNumber( "level", (byte)obj.UserLevel );
			writer.WriteNumber( "permissions", (ushort)obj.Permissions );
			writer.WriteEndObject();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( LevelPermissions ) || typeToConvert.IsSubclassOf( typeof( LevelPermissions ) );
		}
	}
}
