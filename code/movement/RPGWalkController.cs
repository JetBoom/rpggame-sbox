using Sandbox;


namespace RPG
{
	public partial class RPGWalkController : WalkController
	{
		public static readonly float SideMoveDot = 0.5f;
		public static readonly float BackMoveDot = 0.0f;

		public float SideSpeedMultiplier { get; set; } = 0.8f;
		public float BackSpeedMultiplier { get; set; } = 0.666f;

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

			//   Player.SetAnimParam( "forward", Input.Forward );
			//   Player.SetAnimParam( "sideward", Input.Right );
			//   Player.SetAnimParam( "wishspeed", wishspeed );
			//    Player.SetAnimParam( "walkspeed_scale", 2.0f / 190.0f );
			//   Player.SetAnimParam( "runspeed_scale", 2.0f / 320.0f );

			//  DebugOverlay.Text( 0, Pos + Vector3.Up * 100, $"forward: {Input.Forward}\nsideward: {Input.Right}" );

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
			//if ( !player->CanJump() )
			//    return false;


			/*
			if ( player->m_flWaterJumpTime )
			{
				player->m_flWaterJumpTime -= gpGlobals->frametime();
				if ( player->m_flWaterJumpTime < 0 )
					player->m_flWaterJumpTime = 0;

				return false;
			}*/



			// If we are in the water most of the way...
			if ( Swimming )
			{
				// swimming, not jumping
				ClearGroundEntity();

				Velocity = Velocity.WithZ( 100 );

				// play swimming sound
				//  if ( player->m_flSwimSoundTime <= 0 )
				{
					// Don't play sound again for 1 second
					//   player->m_flSwimSoundTime = 1000;
					//   PlaySwimSound();
				}

				return;
			}

			if ( GroundEntity == null )
				return;

			// Crippled?
			if ( DefaultSpeed < 32f )
				return;

			/*
			if ( player->m_Local.m_bDucking && (player->GetFlags() & FL_DUCKING) )
				return false;
			*/

			/*
			// Still updating the eye position.
			if ( player->m_Local.m_nDuckJumpTimeMsecs > 0u )
				return false;
			*/

			ClearGroundEntity();

			// player->PlayStepSound( (Vector &)mv->GetAbsOrigin(), player->m_pSurfaceData, 1.0, true );

			// MoveHelper()->PlayerSetAnimation( PLAYER_JUMP );

			float flGroundFactor = 1.0f;
			//if ( player->m_pSurfaceData )
			{
				//   flGroundFactor = g_pPhysicsQuery->GetGameSurfaceproperties( player->m_pSurfaceData )->m_flJumpFactor;
			}

			float flMul = 268.3281572999747f * 1.2f;

			float startz = Velocity.z;

			if ( Duck.IsActive )
				flMul *= 0.8f;

			Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );

			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			// mv->m_outJumpVel.z += mv->m_vecVelocity[2] - startz;
			// mv->m_outStepHeight += 0.15f;

			// don't jump again until released
			//mv->m_nOldButtons |= IN_JUMP;

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
