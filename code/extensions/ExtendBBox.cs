using System;


namespace RPG
{
	public static class ExtendBBox
	{
		public static Vector3 NearestPoint( this BBox bounds, Vector3 pos )
		{
			return new Vector3(
				Math.Clamp( pos.x, bounds.Mins.x, bounds.Maxs.x ),
				Math.Clamp( pos.y, bounds.Mins.y, bounds.Maxs.y ),
				Math.Clamp( pos.z, bounds.Mins.z, bounds.Maxs.z )
			);
		}
	}
}
