using Sandbox;
using System;
using System.Collections.Generic;


namespace RPG
{
	public partial class RPGPlayer
	{
		public static readonly int MaxSkill = 100; // If this is changed to 1000 later then have to change byte to ushort

		[Net, Local, Change]
		public List<byte> Skills { get; init; } = new();

		[Net, Change]
		public uint TotalXP { get; set; } = 0;
		[Net, Local]
		public uint UsedXP { get; set; } = 0;
		public int AvailableXP => (int)TotalXP - (int)UsedXP;
		public int Level => 1 + (TotalXP / 1000f).FloorToInt();

		private int PreviousLevel = 1;
		private void OnTotalXPChanged()
		{
			if ( Local.Pawn != this ) return;

			var newLevel = Level;
			if ( PreviousLevel == newLevel ) return;

			PreviousLevel = newLevel;

			// TODO: Update XP bar or somethin..
		}

		public int GetSkill( SkillType type )
		{
			var idx = (int)type;
			return idx >= 0 && idx < Skills.Count ? Skills[idx] : 0;
		}

		public float GetSkillFrac( SkillType type ) => (float)GetSkill( type ) / MaxSkill;

		public void SetSkill( SkillType type, int skill )
		{
			skill = Math.Clamp( skill, 0, MaxSkill );
			Skills[(int)type] = (byte)skill;

			InvalidateStatus();
		}

		public void SetSkill( int skillId, int skill )
		{
			if ( skillId >= 0 && skillId < SkillData.NumSkills )
				SetSkill( (SkillType)skillId, skill );
		}

		private void OnSkillsChanged()
		{
			//TODO: Change UI elements
		}

		[ServerCmd]
		public static void ClientUpgradeSkill( int skillId )
		{
			if ( ConsoleSystem.Caller?.Pawn is not RPGPlayer player || !player.IsValid() ) return;
			if ( !player.IsAlive() ) return;
			if ( skillId < 0 || skillId >= Enum.GetValues<SkillType>().Length ) return;

			var type = (SkillType)skillId;
			var data = SkillData.GetData( type );
			var current = player.GetSkill( type );
			var cost = SkillData.GetSkillIncreaseCost( type, player.GetSkill( type ) );

			if ( data != null && data.Trainable && current < MaxSkill && player.AvailableXP >= cost )
			{
				player.SetSkill( type, current + 1 );
				player.UsedXP += (uint)cost;
			}
		}

		/*[ServerCmd( "rpg_testskill" )]
		public static void TestSkill()
		{
			if ( ConsoleSystem.Caller?.Pawn is RPGPlayer player )
			{
				player.SetSkill( SkillType.Vitality, MaxSkill * 0.9f );
			}
		}*/
	}
}
