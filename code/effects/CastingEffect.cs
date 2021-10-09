using Sandbox;
using System;


namespace RPG
{
	public partial class CastingEffect : ModelEntity
	{
		protected virtual string StartSoundPath => "";
		protected virtual string LoopSoundPath => "";

		private Sound LoopSound;

		public CastingEffect() : base()
		{
		}

		public override void Spawn()
		{
			base.Spawn();

			if ( StartSoundPath != "" )
				Sound.FromEntity( StartSoundPath, this );

			if ( LoopSoundPath != "" )
				LoopSound = Sound.FromEntity( LoopSoundPath, this );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( LoopSoundPath != "" )
				LoopSound.Stop();
		}
	}
}
