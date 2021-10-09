using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	public enum StatusModType : int
	{
		None,
		Speed,
		MaxHealth,
		MaxMana,
		ManaRate,
		MaxStamina,
		StaminaRate,
		Silence,
		CastingSpeed,
		ResistSlash,
		ResistPierce,
		ResistBlunt,
		ResistFire,
		ResistCold,
		ResistShock,
		ResistPoison,
	}

	public enum StatusModOperation : int
	{
		None,
		Add,
		Multiply,
		Limit,
	}

	public struct StatusMod
	{
		public StatusModType Type { get; init; }
		public StatusModOperation Op { get; init; }
		public float Magnitude { get; init; }

		public StatusMod( StatusModType type, StatusModOperation op, float magnitude )
		{
			Type = type;
			Op = op;
			Magnitude = magnitude;
		}
	}

	public struct StatusModCalculation
	{
		public float Extra;
		public float MultiplyBy;
		public float LimitTo;
		public bool HasLimit;

		public void AddExtra( float amount ) => Extra += amount;
		public void AddMultiplier( float amount ) => MultiplyBy += amount;
		public void AddLimiter( float amount )
		{
			if ( HasLimit )
			{
				LimitTo = Math.Min( amount, LimitTo );
			}
			else
			{
				LimitTo = amount;
				HasLimit = true;
			}
		}

		public float Alter( float val = 0f, bool additionFirst = false )
		{
			if ( additionFirst )
			{
				val += Extra;
				val *= MultiplyBy;
			}
			else
			{
				val *= MultiplyBy;
				val += Extra;
			}

			if ( HasLimit )
				val = Math.Min( val, LimitTo );

			return val;
		}

		public float Alter( bool additionFirst = false ) => Alter( 0f, additionFirst );
	}

	public class StatusModifications
	{
		public List<StatusMod> List;
		public Dictionary<StatusModType, StatusModCalculation> Values;

		public StatusModifications()
		{
			List = new();
			Values = new();
		}

		public void Add( StatusMod mod ) => List.Add( mod );

		public void LimitSpeed( float speed = 0f ) => List.Add( new StatusMod( StatusModType.Speed, StatusModOperation.Limit, speed ) );

		public void Silence() => List.Add( new StatusMod( StatusModType.Silence, StatusModOperation.Add, 1f ) );

		public void FinishContributing()
		{
			// These are dummies just to make sure we can undo the effects of a status type that has no remaining statuses.
			foreach ( var type in Enum.GetValues<StatusModType>() )
				Add( new StatusMod( type, StatusModOperation.None, 0f ) );

			List.Sort( delegate ( StatusMod mod1, StatusMod mod2 )
			{
				return mod1.Op.CompareTo( mod2.Op );
			} );

			foreach ( var mod in List )
			{
				StatusModCalculation calc;
				if ( Values.ContainsKey( mod.Type ) )
					calc = Values[mod.Type];
				else
				{
					calc = new StatusModCalculation()
					{
						Extra = 0f,
						MultiplyBy = 1f,
					};
				}

				if ( mod.Op == StatusModOperation.Add )
					calc.AddExtra( mod.Magnitude );
				else if ( mod.Op == StatusModOperation.Multiply )
					calc.AddMultiplier( mod.Magnitude );
				else if ( mod.Op == StatusModOperation.Limit )
					calc.AddLimiter( mod.Magnitude );

				Values[mod.Type] = calc;
			}
		}
	}
}
