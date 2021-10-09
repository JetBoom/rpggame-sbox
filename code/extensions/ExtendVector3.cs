using Sandbox;


namespace RPG
{
	public static class ExtendVector3
	{
		public static float DistToSqr( this Vector3 vec1, Vector3 vec2 )
		{
			return (vec1 - vec2).LengthSquared;
		}

		public static Vector3 Vector3FromCSV( this string str )
		{
			var split = str.Split( "," );
			if ( split.Length == 3 )
				return new Vector3( split[0].ToFloat(), split[1].ToFloat(), split[2].ToFloat() );

			return Vector3.Zero;
		}

		public static Vector3 ToVector( this string str ) => Vector3FromCSV( str );
		public static string ToCSV( this Vector3 vec ) => $"{vec.x},{vec.y},{vec.z}";
	}
}
