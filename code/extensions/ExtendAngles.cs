using Sandbox;


namespace RPG
{
	public static class ExtendAngles
	{
		public static Angles AnglesFromCSV( this string str )
		{
			var split = str.Split( "," );
			if ( split.Length == 3 )
				return new Angles( split[0].ToFloat(), split[1].ToFloat(), split[2].ToFloat() );

			return Angles.Zero;
		}

		public static Angles ToAngles( this string str ) => AnglesFromCSV( str );
		public static string ToCSV( this Angles ang ) => $"{ang.pitch},{ang.yaw},{ang.roll}";
	}
}
