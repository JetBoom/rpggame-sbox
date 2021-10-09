using Sandbox;


namespace RPG
{
	[Library( Group = "base" )]
	public abstract partial class ItemMelee : ItemWeapon
	{
	}

	[Library( Group = "base" )]
	public abstract partial class ItemMeleeEntity : ItemWeaponEntity
	{
		public virtual float BaseSwingTime => 1f;
		public virtual float SwingTime => BaseSwingTime / WeaponData.Speed;
		public virtual float HitAtFrac => 0.7f;
		public virtual DamageType DamageType => DamageType.Slashing;

		[Net, Predicted]
		public float StartSwingTime { get; protected set; }
		public float EndSwingTime => IsAttacking ? 0f : StartSwingTime + SwingTime;

		[Net, Predicted]
		protected bool HitProcessed { get; set; }

		public bool IsAttacking => StartSwingTime > 0f;
		float SwingProcessTime => StartSwingTime + SwingTime * HitAtFrac;

		public virtual string AttackSoundPath => "";
		public virtual string ProcessSoundPath => "";
		public virtual string HitFleshSoundPath => "";

		public override bool IsInUse => base.IsInUse || IsAttacking;

		public override void Simulate( Client client )
		{
			base.Simulate( client );

			if ( IsAttacking )
			{
				if ( !HitProcessed && Time.Now >= SwingProcessTime )
					ProcessAttack();
				else if ( Time.Now >= EndSwingTime )
				{
					StartSwingTime = 0f;
					HitProcessed = false;

					OnAttackEnded();
				}
			}
			else if ( Input.Down( InputButton.Attack1 ) )
				StartAttack();
		}

		public override bool CanActivate()
		{
			return base.CanActivate();
		}

		public override bool CanDeactivate()
		{
			return !IsAttacking && base.CanDeactivate();
		}

		public override bool CanStartAbility( Ability ability )
		{
			return !IsAttacking && base.CanStartAbility( ability );
		}

		public void StartAttack()
		{
			if ( IsAttacking ) return;

			StartSwingTime = Time.Now;
			HitProcessed = false;

			if ( Owner is RPGPlayer player )
				player.OnStartMeleeAttack( this );

			if ( !string.IsNullOrEmpty( AttackSoundPath ) )
				PlaySound( AttackSoundPath );

			OnAttackStarted();
		}

		protected void ProcessAttack()
		{
			HitProcessed = true;

			if ( !string.IsNullOrEmpty( ProcessSoundPath ) )
				PlaySound( ProcessSoundPath );

			var traceResults = Owner.FindTargetsInArc( WeaponData.Range, WeaponData.Arc );

			//bool hitWorld = false;
			bool hitFlesh = false;
			bool hitAny = false;

			var damage = GetDamage();

			foreach ( var tr in traceResults )
			{
				var ent = tr.Entity;
				if ( !ent.IsValid() ) continue;

				if ( ent.IsWorld )
				{
					//hitWorld = true;
				}
				else
				{
					var dmgInfo = new DamageInfo
					{
						Damage = damage,
						Attacker = Owner,
						Weapon = this,
						Flags = DamageFlags.Slash,
						Position = tr.EndPos,
						//Force = new Vector3( 100f ),
						BoneIndex = tr.Bone,
						Body = tr.Body,
						HitboxIndex = tr.HitboxIndex,
					};

					ent.TakeDamageType( dmgInfo, DamageType );

					if ( ent is Player )
						hitFlesh = true;
				}

				tr.Surface?.DoBulletImpact( tr );

				hitAny = true;
			}

			if ( hitFlesh )
			{
				if ( !string.IsNullOrEmpty( HitFleshSoundPath ) )
					PlaySound( HitFleshSoundPath );
			}
			else if ( hitAny )
			{
				// Some hit metal or something sound/effects...
			}
			else
				// Some miss sound...

				OnAttackProcessed();
		}

		public virtual void OnAttackStarted() { }
		public virtual void OnAttackEnded() { }
		public virtual void OnAttackProcessed() { }
	}
}
