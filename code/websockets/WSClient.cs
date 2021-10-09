using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace RPG
{
	public static class WSClient
	{
		private static WebSocket socket;

		public static bool IsConnected => socket != null && socket.IsConnected;

		public static async void Connect()
		{
			if ( socket != null && socket.IsConnected ) return;

			if ( socket != null )
				socket.Dispose();

			socket = new();

			await socket.Connect( "wss://localhost:1281" );

			if ( socket.IsConnected )
			{
				socket.OnMessageReceived += OnMessageReceived;
				socket.OnDisconnected += OnDisconnected;
			}
			else
			{
				Connect();
			}
		}

		public static async void Send( string message )
		{
			if ( socket == null || !socket.IsConnected ) return;

			await socket.Send( message );
		}

		private static void OnMessageReceived( string message )
		{
			Log.Info( message );
		}

		private static void OnDisconnected( int status, string reason )
		{
			Log.Warning( $"WS disconnected with code {status}: {reason}" );
			Connect();
		}
	}
}
