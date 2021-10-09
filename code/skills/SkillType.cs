using Sandbox;
using System.Collections.Generic;


namespace RPG
{
	public enum SkillType : int
	{
		Vitality,
		Energy,
		Fire,
	}

	public partial class SkillData : Asset
	{
		public static readonly Dictionary<SkillType, SkillData> Data = new()
		{
			[SkillType.Vitality] = new()
			{
				DisplayName = "Vitality",
				Description = "Raises maximum health.",
			},
			[SkillType.Energy] = new()
			{
				DisplayName = "Energy",
				Description = "Raises maximum mana.",
			},
			[SkillType.Fire] = new()
			{
				DisplayName = "Fire Magic",
				Description = "Allows learning of more complex Fire Magic abilities as well as increasing their effectiveness.",
			},
		};

		public static int NumSkills => Data.Count;

		public string DisplayName { get; set; } = "";
		public string Description { get; set; } = "";
		/// <summary>Can this skill be upgraded with XP points?</summary>
		public bool Trainable { get; set; } = true;

		public static SkillData GetData( SkillType type )
		{
			if ( Data.TryGetValue( type, out SkillData data ) )
				return data;

			return null;
		}

		/// <summary>Cost to increase this skill by 1</summary>
		public static int GetSkillIncreaseCost( SkillType type, int atSkill )
		{
			// TODO: Make a better formula than this. It has to have an inverse function too.
			//return (int)Math.Ceiling( Math.Pow( (atSkill + 1) * 2, 1.1 ) );
			return atSkill;
		}

		public static string GetDisplayName( SkillType type ) => GetData( type ).DisplayName;
	}
}
