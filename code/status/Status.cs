using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RPG
{
	[Library( "status_base", Group = "base" )]
	public abstract partial class Status : EntityComponent
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
		public static Status AddStatus<T>( this Entity self, float magnitude = 1f ) where T : Status, new()
		{
			Host.AssertServer();

			var usemods = self as IUseStatusMods;

			var existing = self.GetStatus<T>();
			if ( existing != null && !existing.Data.CanStack )
			{
				existing.Magnitude = MathF.Max( magnitude, existing.Magnitude );
				existing.StartTime = Time.Now;
				existing.Duration = existing.Data.BaseDuration;

				if ( usemods != null )
					usemods.InvalidateStatus();

				return existing;
			}

			var status = self.Components.Create<T>();
			if ( status == null ) return null;

			status.Magnitude = magnitude;
			status.StartTime = Time.Now;
			status.Duration = status.Data.BaseDuration;

			if ( usemods != null )
				usemods.InvalidateStatus();

			return status;
		}

		public static bool HasStatus<T>( this Entity self ) where T : Status
		{
			return self.GetStatus<T>() != null;
		}

		public static Status GetStatus<T>( this Entity self ) where T : Status
		{
			return self.Components.Get<T>();
		}

		public static IEnumerable<Status> GetAllStatus( this Entity self )
		{
			return self.Components.GetAll<Status>();
		}

		public static void RemoveAllStatus( this Entity self )
		{
			var statuses = self.GetAllStatus();
			foreach ( var status in statuses )
				self.Components.Remove( status );
		}

		public static bool RemoveStatus<T>( this Entity self ) where T : Status
		{
			var status = self.GetStatus<T>();
			if ( status != null )
			{
				status.Enabled = false;
				status.Remove();
				return true;
			}

			return false;
		}
	}
}
