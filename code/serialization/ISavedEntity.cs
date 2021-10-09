using Sandbox;


namespace RPG
{
	public interface ISavedEntity : IJsonSerializable
	{
		/// <summary>Should we save in the SaveWorld pass? Probably want to be false for anything that's saved in its own files (like players).</summary>
		public bool ShouldPersistInWorldFile()
		{
			// Follow behavior of Root entity, if we have one.
			if ( this is Entity ent )
			{
				if ( ent.Root != this && ent.Root is ISavedEntity ser )
					return ser.ShouldPersistInWorldFile();

				return ent.IsValid();
			}

			return false;
		}
	}
}
