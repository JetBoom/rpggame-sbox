using Sandbox;
using System.Collections.Generic;
using System.Text.Json;


namespace RPG
{
	public static class SerializeEnt
	{
		private static readonly Dictionary<int, Entity> SavedNetId = new();
		private static readonly Dictionary<Entity, int> SavedOwnerNetId = new();
		private static readonly Dictionary<Entity, int> SavedParentNetId = new();

		public static void SetLoadWorldNetId( Entity ent, int netId ) => SavedNetId[netId] = ent;
		public static void SetLoadWorldOwner( Entity ent, int ownerId ) => SavedOwnerNetId[ent] = ownerId;
		public static void SetLoadWorldParent( Entity ent, int parentId ) => SavedParentNetId[ent] = parentId;

		private static void SetupParents()
		{
			foreach ( (Entity ent, int ownerNetId) in SavedOwnerNetId )
			{
				if ( SavedNetId.TryGetValue( ownerNetId, out Entity owner ) )
				{
					ent.Owner = owner;
				}
				else
					Log.Warning( $"Entity {ent.ClassInfo.Name}:{ent.NetworkIdent} had a $owner but no loaded entity had a matching $netid!" );
			}

			foreach ( (Entity ent, int parentNetId) in SavedParentNetId )
			{
				if ( SavedNetId.TryGetValue( parentNetId, out Entity parent ) )
				{
					ent.Position = parent.WorldSpaceBounds.Center;
					if ( ent is ItemEntity itemEntity )
						itemEntity.ParentToItemOwner();
					else
						ent.SetParent( parent, null, Transform.Zero );
				}
				else
					Log.Warning( $"Entity {ent.ClassInfo.Name}:{ent.NetworkIdent} had a $parent but no loaded entity had a matching $netid!" );
			}
		}

		public static void StartLoad()
		{
			SavedNetId.Clear();
			SavedOwnerNetId.Clear();
			SavedParentNetId.Clear();
		}

		public static void EndLoad()
		{
			SetupParents();

			SavedNetId.Clear();
			SavedOwnerNetId.Clear();
			SavedParentNetId.Clear();
		}

		public static void WriteVector3( this Utf8JsonWriter writer, string name, Vector3 vec ) => writer.WriteString( name, vec.ToCSV() );

		public static void WriteAngles( this Utf8JsonWriter writer, string name, Angles ang ) => writer.WriteString( name, ang.ToCSV() );
	}
}
