using Sandbox;
using System.Text;
using System.IO;


namespace RPG
{
	/// <summary>Something with a Library attribute that can be serialized in to a byte array.</summary>
	public interface ISerializable
	{
		public void SerializeMemory( BinaryWriter writer )
		{
		}

		public void DeserializeMemory( BinaryReader reader )
		{
		}

		public void DeserializeMemory( byte[] bytes )
		{
			var stream = new MemoryStream( bytes );
			using ( BinaryReader reader = new( stream, Encoding.UTF8 ) )
			{
				DeserializeMemory( reader );
			}

			stream.Dispose();
		}

		public static T Create<T>( string name, BinaryReader reader = null ) where T : ISerializable
		{
			var obj = Library.Create<T>( name );

			if ( reader != null )
				obj.DeserializeMemory( reader );

			return obj;
		}
	}
}
