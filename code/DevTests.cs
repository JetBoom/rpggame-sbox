using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace RPG.Dev
{
	public static partial class DevTests
	{
		[ServerCmd( "test_givestone" )]
		public static void ServerCmdTestGiveStone()
		{
			if ( Global.IsDedicatedServer ) return;

			if ( ConsoleSystem.Caller?.Pawn is RPGPlayer player && player.IsValid() )
				player.GetContainer().AddItem( "item_stone" );
		}

		/*[ServerCmd( "teststream" )]
		public static void TestStream()
		{
			MemoryStream stream = new();
			using ( BinaryWriter writer = new( stream, System.Text.Encoding.UTF8 ) )
			{
				writer.Write( 3 );
				writer.Write( "Hello" );
				writer.Write( Vector3.Up * 30f );
			}

			TestStreamReceive( stream.ToArray() );

			stream.Dispose();
		}

		[ClientRpc]
		public static void TestStreamReceive( byte[] data )
		{
			MemoryStream stream = new( data );
			using ( BinaryReader reader = new( stream, System.Text.Encoding.UTF8 ) )
			{
				Log.Info( reader.ReadInt32() );
				Log.Info( reader.ReadString() );
				Log.Info( reader.ReadVector3() );
			}

			stream.Dispose();
		}*/
	}
}
