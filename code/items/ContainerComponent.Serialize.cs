using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPG
{
	public partial class ContainerComponent : IJsonSerializable
	{
		public virtual void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
			if ( Count > 0 )
			{
				var itemListConverter = options.GetConverter( typeof( List<Item> ) ) as JsonConverterListItem;

				writer.WritePropertyName( nameof( Items ) );
				itemListConverter.Write( writer, Items.ToList(), options );
			}
		}

		public virtual bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			switch ( propertyName )
			{
				case "Items":
					var itemListConverter = options.GetConverter( typeof( List<Item> ) ) as JsonConverterListItem;
					reader.Read();
					Items.Clear();
					var newItems = itemListConverter.Read( ref reader, typeof( List<Item> ), options );
					foreach ( var item in newItems )
					{
						item.Container = this;
						Items.Add( item );
					}
					return true;
			}

			return false;
		}

		/*public virtual void SerializeMemory( BinaryWriter writer )
		{
			writer.Write( (ushort)Count );

			foreach ( var item in Items )
			{
				writer.Write( item.NetId );
				writer.Write( item.ClassInfo.Name );
				item.SerializeMemory( writer );
			}
		}

		public virtual void DeserializeMemory( BinaryReader reader )
		{
			var count = reader.ReadUInt16();

			HashSet<int> keep = new();

			for ( int i = 0; i < count; ++i )
			{
				var netid = reader.ReadInt32();
				var className = reader.ReadString();

				keep.Add( netid );

				var item = Item.GetItemByNetId( netid );
				if ( item == null )
				{
					item = Library.Create<Item>( className );
					item.TransferTo( this );
				}

				item.DeserializeMemory( reader );
			}

			List<Item> toRemove = new();
			foreach ( var item in Items )
			{
				if ( !keep.Contains( item.NetId ) )
					toRemove.Add( item );
			}

			foreach ( var item in toRemove )
			{
				if ( item.Container == this )
					item.Delete();
			}
		}*/
	}

	public class JsonConverterContainer : JsonConverter<ContainerComponent>
	{
		public override ContainerComponent Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( reader.TokenType != JsonTokenType.StartObject )
				throw new JsonException( "First token was not a StartObject!" );

			ContainerComponent container = null;

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

							container = Library.Create<ContainerComponent>( className );
							if ( container == null )
							{
								Log.Warning( $"Failed to Deserialize on something with $class \"${className}\". Does it not have a matching Library attribute? Skipping!" );
								reader.Skip();
								return null;
							}

							break;
						default:
							try
							{
								container.DeserializeProperty( propertyName, ref reader, options );
							}
							catch ( Exception readException )
							{
								Log.Warning( readException.Message );
							}

							break;
					}
				}
			}

			return container;
		}

		public override void Write( Utf8JsonWriter writer, ContainerComponent container, JsonSerializerOptions options )
		{
			writer.WriteStartObject();
			writer.WriteString( "$class", container.GetType().Name );
			container.Serialize( writer, options );
			writer.WriteEndObject();
		}

		public override bool CanConvert( Type typeToConvert )
		{
			return typeToConvert == typeof( ContainerComponent ) || typeToConvert.IsSubclassOf( typeof( ContainerComponent ) );
		}
	}
}
