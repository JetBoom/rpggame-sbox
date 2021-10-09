using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	[Library( "status_base", Group = "base" )]
	public abstract partial class Status : EntityComponent<RPGPlayer>
	{
		[Net]
		public float Magnitude { get; set; } = 1f;
		[Net]
		public float StartTime { get; set; } = 0f;
		[Net]
		public float Duration { get; set; } = 0f;

		public float EndTime => StartTime + Duration;
		public float TimeLeft => Math.Max( 0f, EndTime - Time.Now );

		private StatusData _Data;
		public StatusData Data => _Data == null ? _Data = StatusData.ResourceFor( ClassInfo.Name ) : _Data;

		public Status() : base()
		{
			if ( !Host.IsServer ) return;

			StartTime = Time.Now;

			if ( Data != null )
				Duration = Data.BaseDuration;
		}

		public virtual void Simulate( Client cl )
		{
		}

		public virtual float GetMagnitude()
		{
			return Data.BaseMagnitude * Magnitude;
		}

		public virtual void ContributeModifications( StatusModifications mods )
		{
		}

		[Event.Tick.Server]
		protected virtual void OnTickServer()
		{
			if ( Duration > 0f && Time.Now >= EndTime )
				Remove();
		}
	}

	public static class StatusExtend
	{

	}
}
