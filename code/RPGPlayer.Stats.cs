using Sandbox;

/* Q: Why have 3 networked floats for one stat?!
 * A: You send all 3 whenever we need to add or subtract the stat.
 *    BUT, you no longer need to send ANYTHING for regeneration and that regeneration is granular to a single game frame.
 *    With a big player count, you're saving A LOT of network data and avoiding awkwardly having stats regenerating every second instead of every frame.
 *    50 players x 50 network receivers x 3 types of stats x 32 bits (yes I know floats are compressed) = 240 Kb saved each second
 */

namespace RPG
{
	/*public struct Stat
	{
		public float Base;
		public float Rate;
		public float LastChange;

		public Stat()
		{
			Base = 0.0f;
			Rate = 0.0f;
			LastChange = Time.Now;
		}
	};*/

	public partial class RPGPlayer
	{
		/*[Net, Predicted, Local]
		public Stat ManaStat { get; set; }

		[Net, Predicted, Local]
		public Stat StaminaStat { get; set; }*/

		/*[Net, Local]
		public float _ManaRate { get; private set; } = 0.0f;
		[Net, Local]
		public float _StaminaRate { get; private set; } = 0.0f;

		[Net, Predicted, Local]
		public float ManaBase { get; set; }
		[Net, Predicted, Local]
		public float StaminaBase { get; set; }

		[Net, Predicted, Local]
		public float ManaLastChange { get; set; }
		[Net, Predicted, Local]
		public float StaminaLastChange { get; set; }

		[Net, Local]
		public float _ManaMax { get; set; } = 100.0f;
		[Net, Local]
		public float _StaminaMax { get; set; } = 100.0f;

		public float ManaRate
		{
			get => _ManaRate;
			set
			{
				ManaBase = Mana;
				ManaLastChange = Time.Now;
				_ManaRate = value;
			}
		}

		public float StaminaRate
		{
			get => _StaminaRate;
			set
			{
				StaminaBase = Stamina;
				StaminaLastChange = Time.Now;
				_StaminaRate = value;
			}
		}

		public float Mana
		{
			get => MathX.Clamp( ManaBase + ManaRate * (Time.Now - ManaLastChange), 0.0f, ManaMax );
			set
			{
				ManaBase = value;
				ManaLastChange = Time.Now;
			}
		}

		public float Stamina
		{
			get => MathX.Clamp( StaminaBase + StaminaRate * (Time.Now - StaminaLastChange), 0.0f, StaminaMax );
			set
			{
				StaminaBase = value;
				StaminaLastChange = Time.Now;
			}
		}

		public float ManaMax
		{
			get => _ManaMax;
			set
			{
				var oldMax = _ManaMax;
				_ManaMax = value;

				if ( value > oldMax && oldMax > 0f && Mana > oldMax )
					Mana = oldMax;
			}
		}

		public float StaminaMax
		{
			get => _StaminaMax;
			set
			{
				var oldMax = _StaminaMax;
				_StaminaMax = value;

				if ( value > oldMax && oldMax > 0f && Stamina > oldMax )
					Stamina = oldMax;
			}
		}*/
	}
}
