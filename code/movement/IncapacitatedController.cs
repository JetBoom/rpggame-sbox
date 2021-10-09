using Sandbox;


namespace RPG
{
	public partial class IncapacitatedDuck : Duck
	{
		public IncapacitatedDuck( BasePlayerController controller ) : base( controller )
		{
		}

		public override void PreTick()
		{
		}
	}

	public partial class IncapacitatedController : RPGWalkController
	{
		public IncapacitatedController() : base()
		{
			DefaultSpeed
			= WalkSpeed
			= SprintSpeed
			= AirAcceleration
			= AirControl
			= 0f;

			EyeHeight = 16.0f;

			Duck = new IncapacitatedDuck( this );
		}

		public override float GetWishSpeed()
		{
			return 0f;
		}

		public override void CheckJumpButton()
		{
		}
	}
}
