using Sandbox;


namespace RPG
{
	public partial class RPGWalkController : WalkController
	{
		public static readonly float SideMoveDot = 0.5f;
		public static readonly float BackMoveDot = 0.0f;
		public static readonly float JumpPower = 320f;
		public static readonly float JumpDuckPowerMul = 0.8f;
		public static readonly float SwimUpSpeed = 100f;

		public float SideSpeedMultiplier = 0.8f;
		public float BackSpeedMultiplier = 0.666f;

		public RPGWalkController() : base()
		{
			DefaultSpeed = WalkSpeed = SprintSpeed = RPGGlobals.BaseWalkSpeed;
			Acceleration = 7.0f;
			AirAcceleration = 7.0f;
			AirControl = 20.0f;
			//SideSpeedMultiplier = 0.8f;
			//BackSpeedMultiplier = 0.666f;
			//FallSoundZ = -30.0f;
			//GroundFriction = 4.0f;
			//StopSpeed = 100.0f;
			//Size = 20.0f;
			//DistEpsilon = 0.03125f;
			//GroundAngle = 46.0f;
			//Bounce = 0.0f;
			//MoveFriction = 1.0f;
			//StepSize = 18.0f;
			//MaxNonJumpVelocity = 140.0f;
			//BodyGirth = 32.0f;
			//BodyHeight = 72.0f;
			//EyeHeight = 64.0f;
			//Gravity = 800.0f;
		}

		public void SetSpeed( float speed )
		{
			DefaultSpeed = WalkSpeed = SprintSpeed = speed;
		}

		public override void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			if ( !Duck.IsActive )
			{
				var rot = EyeRot.Angles();
				rot.pitch = 0.0f;

				var dot = wishdir.Dot( rot.Direction );
				if ( dot < BackMoveDot )
					wishspeed *= BackSpeedMultiplier;
				else if ( dot < SideMoveDot )
					wishspeed *= SideSpeedMultiplier;
			}

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishspeed;

			Velocity = Velocity.WithZ( 0 );
			Accelerate( wishdir, wishspeed, 0, Acceleration );
			Velocity = Velocity.WithZ( 0 );

			/*Player.SetAnimParam( "forward", Input.Forward );
			Player.SetAnimParam( "sideward", Input.Right );
			Player.SetAnimParam( "wishspeed", wishspeed );
			Player.SetAnimParam( "walkspeed_scale", 2.0f / 190.0f );
			Player.SetAnimParam( "runspeed_scale", 2.0f / 320.0f );
			DebugOverlay.Text( 0, Pos + Vector3.Up * 100, $"forward: {Input.Forward}\nsideward: {Input.Right}" );*/

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.LengthSquared < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination
				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

				var pm = TraceBBox( Position, dest );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPos;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		public override void CheckJumpButton()
		{
			// TODO: Add some way to jump out of water while near an edge.
			if ( Swimming )
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ( SwimUpSpeed );
				return;
			}

			if ( GroundEntity == null )
				return;

			// Crippled?
			if ( DefaultSpeed < 32f )
				return;

			ClearGroundEntity();

			// player->PlayStepSound( (Vector &)mv->GetAbsOrigin(), player->m_pSurfaceData, 1.0, true );

			float power = JumpPower;
			if ( Duck.IsActive )
				power *= JumpDuckPowerMul;

			Velocity = Velocity.WithZ( Velocity.z + power - Gravity / 2f * Time.Delta );

			AddEvent( "jump" );
		}

		/*private static RPGWalkController GetControllerFromCommand()
		{
			var player = ConsoleSystem.Caller?.Pawn as RPGPlayer;
			return player?.Controller as RPGWalkController;
		}

		[ServerCmd("rpg_test_accel")]
		public static void ClientTestAccel( string strArg )
		{
			var val = strArg.ToInt( 0 );
			var controller = GetControllerFromCommand();
			if (controller != null)
				controller.Acceleration = val;
		}

		[ServerCmd( "rpg_test_airaccel" )]
		public static void ClientTestAirAccel( string strArg )
		{
			var val = strArg.ToInt( 0 );
			var controller = GetControllerFromCommand();
			if ( controller != null )
				controller.AirAcceleration = val;
		}

		[ServerCmd( "rpg_test_aircontrol" )]
		public static void ClientTestAirControl( string strArg )
		{
			var val = strArg.ToInt( 0 );
			var controller = GetControllerFromCommand();
			if ( controller != null )
				controller.AirControl = val;
		}*/
	}
}
