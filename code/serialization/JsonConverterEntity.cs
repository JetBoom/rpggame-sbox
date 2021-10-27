using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{
	public class JsonConverterEntity : JsonConverter<Entity>
	{
		public override Entity Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartObject )
				throw new JsonException( "First token was not a StartObject!" );

			Entity ent = null;

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

							ent = Library.Create<Entity>( className );
							if ( ent == null )
							{
								Log.Warning( $"Failed to Deserialize on something with $class \"${className}\". Does it not have a matching Library attribute? Skipping!" );
								reader.Skip();
								return null;
							}

							break;
						case "$owner":
							reader.Read();
							SerializeEnt.SetLoadWorldOwner( ent, reader.GetInt32() );
							break;
						case "$parent":
							reader.Read();
							SerializeEnt.SetLoadWorldParent( ent, reader.GetInt32() );
							break;
						case "$netid":
							reader.Read();
							SerializeEnt.SetLoadWorldNetId( ent, reader.GetInt32() );
							break;
						case "Position":
							reader.Read();
							ent.Position = reader.GetString().ToVector();
							break;
						case "Rotation":
							reader.Read();
							ent.Rotation = reader.GetString().ToAngles().ToRotation();
							break;
						case "Velocity":
							reader.Read();
							ent.Velocity = reader.GetString().ToVector();
							break;
						case "LifeState":
							reader.Read();
							try
							{
								ent.LifeState = Enum.Parse<LifeState>( reader.GetString() );
							}
							catch { }
							break;
						case "Health":
							reader.Read();
							ent.SetHealth( (float)reader.GetDecimal() );
							break;
						case "Mana":
							reader.Read();
							ent.SetMana( (float)reader.GetDecimal() );
							break;
						case "Stamina":
							reader.Read();
							ent.SetStamina( (float)reader.GetDecimal() );
							break;
						case "ContainerItems":
							var container = ent.GetContainer();
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
						default:
							// Non-standard property? Let the entity itself try to handle it.
							if ( ent is IJsonSerializable ser )
							{
								try
								{
									ser.DeserializeProperty( propertyName, ref reader, options );
								}
								catch ( Exception readException )
								{
									Log.Warning( readException.Message );
								}
							}

							break;
					}
				}
			}

			return ent;
		}

		public override void Write( Utf8JsonWriter writer, Entity ent, JsonSerializerOptions options )
		{
			writer.WriteStartObject();

			// Write some standard properties everything needs.

			writer.WriteString( "$class", ent.ClassInfo.Name );
			writer.WriteNumber( "$netid", ent.NetworkIdent );

			if ( ent.Owner.IsValid() && ent.Owner is IJsonSerializable )
				writer.WriteNumber( "$owner", ent.Owner.NetworkIdent );

			if ( ent.Parent.IsValid() && ent.Parent is IJsonSerializable )
				writer.WriteNumber( "$parent", ent.Parent.NetworkIdent );
			else
			{
				writer.WriteVector3( "Position", ent.Position );
				writer.WriteAngles( "Rotation", ent.Rotation.Angles() );

				if ( ent.Velocity.LengthSquared >= 1f )
					writer.WriteVector3( "Velocity", ent.Velocity );
			}

			if ( ent.LifeState != LifeState.Alive )
				writer.WriteString( "LifeState", ent.LifeState.ToString() );

			if ( ent.GetHealth() > 0f )
				writer.WriteNumber( "Health", ent.Health );

			var mana = ent.GetMana();
			if ( mana > 0f )
				writer.WriteNumber( "Mana", mana );

			var stamina = ent.GetStamina();
			if ( stamina > 0f )
				writer.WriteNumber( "Stamina", stamina );

			var container = ent.GetContainer();
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
			}

			// Let the entity serialize stuff now. Polymorphic serialization!
			if ( ent is IJsonSerializable ser )
				ser.Serialize( writer, options );

			writer.WriteEndObject();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( Entity ) || typeToConvert.IsSubclassOf( typeof( Entity ) );
		}
	}
}
