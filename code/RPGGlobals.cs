namespace RPG
{
	public static class RPGGlobals
	{
		/// <summary>Players can select and prepare up to this many abilities, not counting common or inherent ones which are given automatically at all times.</summary>
		public static readonly int MaxSchoolAbilities = 8;
		/// <summary>After ending an ability, a global cooldown of this much is applied.</summary>
		public static readonly float GlobalStartCD = 0.5f;

		/// <summary>Players cannot go above this much damage resistance in any damage type.</summary>
		public static readonly float MaxPlayerResistance = 0.8f;
		//public static readonly float MinResistance = -2.0f;

		/// <summary>Players cannot have casting speed above this amount through buffs, items, etc.</summary>
		public static readonly float MaxCastingSpeed = 2.5f;
		/// <summary>Players cannot have casting speed below this amount through debuffs, items, etc.</summary>
		public static readonly float MinCastingSpeed = 0.25f;

		/// <summary>Player walking speed before buffs and items.</summary>
		public static readonly float BaseWalkSpeed = 250f;

		/// <summary>Every <see cref="HealthRegenRate"/> add this amount of Health to a player.</summary>
		public static readonly float HealthRegenAmount = 0.5f;
		/// <summary>Every this many seconds, add <see cref="HealthRegenAmount"/> Health to a player.</summary>
		public static readonly float HealthRegenRate = 3f;

		/// <summary>Player voice goes as far as this number, square rooted. This is a bit of a code optimization.</summary>
		public static readonly float VoiceDistSqr = 1000 * 1000;

		/// <summary>The world will auto-save every this many seconds.</summary>
		public static readonly float SaveWorldTime = 120f;

		/// <summary>Players stay in the world for this much time after disconnecting. If they reconnect then they repossess the already existing pawn.</summary>
		public static readonly float PlayerLogoutTime = 60f;

		/// <summary>Time to bleed out. The player will automatically go from dying to dead state if not revived in some way.</summary>
		public static readonly float PlayerBleedOutTime = 15f;
		/// <summary>Time to actually respawn. This is the time the screen fades to black and you're then finally given a respawn screen.</summary>
		public static readonly float PlayerRespawnTime = 3f;
	}
}
