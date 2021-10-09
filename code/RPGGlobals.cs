namespace RPG
{
	public static class RPGGlobals
	{
		public static readonly int MaxSchoolAbilities = 8;
		/// <summary>After ending an ability, a global cooldown of this much is applied.</summary>
		public static readonly float GlobalStartCD = 0.5f;

		public static readonly float MaxPlayerResistance = 0.8f;
		//public static readonly float MinResistance = -2.0f;

		public static readonly float MaxCastingSpeed = 2.5f;
		public static readonly float MinCastingSpeed = 0.25f;

		public static readonly float BaseWalkSpeed = 250f;

		public static readonly float HealthRegenAmount = 0.5f;
		public static readonly float HealthRegenRate = 3f;

		public static readonly float VoiceDistSqr = 1000 * 1000;

		public static readonly float SaveWorldTime = 120f;

		/// <summary>Time to actually respawn. This is when the screen fades to black and you're given a respawn screen.</summary>
		public static readonly float PlayerRespawnTime = 3f;
		/// <summary>Time to bleed out. The player will automatically go from dying to dead state.</summary>
		public static readonly float PlayerBleedOutTime = 15f;
	}
}
