using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace RPG
{
	[Library( Group = "base" )]
	public abstract partial class ItemEntity : ModelEntity, IHasDisplayInfo, ISavedEntity
	{
		public string ItemClassName => ClassInfo.Name[4..];

		private ItemData _Data;
		public ItemData Data => _Data == null ? _Data = ItemData.ResourceFor( ClassInfo.Name ) : _Data;

		[Net]
		public Item Item { get; set; }

		public virtual bool ShouldBoneMerge => false;

		public virtual bool IsInUse => false;

		public virtual string AttachmentName => null;
		public virtual Transform AttachmentTransform => Transform.Zero;

		public Entity ItemOwner => Item?.OwnerEntity;
		public RPGPlayer ItemPlayerOwner => Item?.OwnerPlayer;

		public override void Spawn()
		{
			base.Spawn();

			/*if ( Item == null && Host.IsServer )
				Item = Library.Create<Item>( ItemClassName );*/

			Scale = Data.ModelScale;
			if ( Data.Model == null )
				SetupPhysicsFromAABB( PhysicsMotionType.Dynamic, new Vector3( -16f ), new Vector3( 16f ) );
			else
			{
				SetModel( Data.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			}

			CollisionGroup = CollisionGroup.Debris;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;

			RefreshState();
		}

		public void RefreshState()
		{
			RefreshDrawing();
			RefreshTransmitState();
		}

		// IHasDisplayInfo
		public virtual string GetDisplayInfo()
		{
			string info = Data.DisplayName;

			if ( Item?.Amount > 1 )
				info = $"{info} ({Item.Amount})";

			return info;
		}

		protected void RefreshDrawing()
		{
			var owner = ItemOwner;

			if ( owner.IsValid() )
			{
				Position = owner.WorldSpaceBounds.Center;
				MoveType = MoveType.None;
				EnableAllCollisions = false;
				EnableDrawing = false;
			}
			else
			{
				MoveType = MoveType.Physics;
				EnableDrawing = true;
				EnableAllCollisions = true;
			}

			ParentToItemOwner();
		}

		public void ParentToItemOwner()
		{
			var owner = ItemOwner;

			Owner = owner;
			if ( Parent != owner )
			{
				if ( owner == null )
					SetParent( null );
				else if ( ShouldBoneMerge )
					SetParent( owner, true );
				else
					SetParent( owner, AttachmentName, AttachmentTransform );
			}
		}

		protected void RefreshTransmitState()
		{
			Transmit = GetDesiredTransmitType();
		}

		/// <summary>What transmit state <b>should</b> we be? Can be overridden for custom behavior (equippable items).</summary>
		/// <remarks>Actually setting the transmit state is done with <seealso cref="RefreshTransmitState"/></remarks>
		public virtual TransmitType GetDesiredTransmitType()
		{
			if ( !Owner.IsValid() ) return TransmitType.Default;

			if ( Owner.ActiveChild == this ) return TransmitType.Default;

			if ( Owner is RPGPlayer ) return TransmitType.Owner;

			/*if ( Owner is IContainer )
				return TransmitType.Default;*/

			return TransmitType.Never;
		}

		public virtual bool CanActivate()
		{
			return false;
		}

		public virtual bool CanDeactivate()
		{
			return true;
		}

		public override void ActiveStart( Entity ent )
		{
			if ( IsServer )
				RefreshState();
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			if ( IsServer )
				RefreshState();
		}

		public virtual bool CanBeUsedBy( RPGPlayer player )
		{
			if ( !this.IsValid() ) return false;
			if ( !Data.CanUseFromInventory ) return false;
			if ( player != ItemOwner ) return false;
			return true;
		}

		public virtual bool CanBeDroppedBy( RPGPlayer player )
		{
			if ( !this.IsValid() ) return false;
			if ( !ItemOwner.IsValid() ) return false;
			if ( !IsInUse ) return false;
			if ( player != ItemOwner ) return false;
			return true;
		}

		public virtual bool OnUseFromInventory( RPGPlayer player )
		{
			Host.AssertServer();

			return true;
		}

		/// <summary>Drop this item. If a null player is given then it will use the current owner.</summary>
		/// <remarks>Server Only</remarks>
		/// <param name="player">Who is dropping us?</param>
		/*public bool OnDropFromInventory( RPGPlayer player = null )
		{
			Host.AssertServer();

			Entity oldOwner = ItemOwner;
			if ( oldOwner == null ) return true;

			Owner = null;

			if ( player == null && oldOwner is RPGPlayer )
				player = oldOwner as RPGPlayer;

			if ( player.IsValid() )
			{
				Position = player.EyePos;
				Rotation = player.EyeRot;
			}

			RefreshState();

			if ( player.IsValid() )
				Velocity = player.Velocity * 0.75f;

			OnDroppedFromInventory( player, oldOwner );

			return true;
		}

		/// <summary>We successfully dropped and are now in the world.</summary>
		/// <remarks>Server Only</remarks>
		/// <param name="player">Who dropped us.</param>
		/// <param name="oldOwner">Our previous owner.</param>
		public virtual void OnDroppedFromInventory( RPGPlayer player, Entity oldOwner ) { }*/

		public virtual bool ShouldPersistInWorldFile()
		{
			// Root entity isn't in world file? We shouldn't be either.
			if ( Root != this )
				return Root is ISavedEntity sav && sav.ShouldPersistInWorldFile();

			return true;
		}

		public virtual void Serialize( Utf8JsonWriter writer, JsonSerializerOptions options )
		{
			if ( Item != null )
			{
				writer.WritePropertyName( nameof( Item ) );
				writer.WriteStartObject();
				Item.Serialize( writer, options );
				writer.WriteEndObject();
			}
		}

		public virtual bool DeserializeProperty( string propertyName, ref Utf8JsonReader reader, JsonSerializerOptions options )
		{
			if ( propertyName == nameof( Item ) )
			{
				var itemConverter = options.GetConverter( typeof( Item ) ) as JsonConverterItem;

				reader.Read();
				Item = itemConverter.Read( ref reader, typeof( Item ), options );
			}

			return false;
		}
	}
}
