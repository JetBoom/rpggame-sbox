using System.Text.Json;


namespace RPG
{
	/// <summary>Something with a Library attribute that can be serialized in to json.</summary>
	public interface IJsonSerializable
	{
		/// <summary>Put class-specific data in to <paramref name="info"/>.</summary>
		public void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
		}

		/// <summary>Class has been constructed and now we're loading data in to it.</summary>
		/// <returns>false: this custom property is unknown.</returns>
		public bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			return false;
		}
	}
}
