using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public partial interface IAssetPaths
	{
		public static string GetBasePath() => "";
		public static string GetPathSuffix() => "";
		public static string FilePathFor( string name ) => $"{GetBasePath()}{name}{GetPathSuffix()}";
	}
}
