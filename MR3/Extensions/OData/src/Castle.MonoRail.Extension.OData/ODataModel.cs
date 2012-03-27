namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.Data.Services.Providers;
	using System.Linq;

	

	public abstract class ODataModel
	{
		public string SchemaNamespace { get; set; }

		private readonly List<EntitySetDefinition> _entities = new List<EntitySetDefinition>();

		public EntitySetConfig<T> EntitySet<T>(string entityName, IQueryable<T> source, EntitySetPermission permissions)
		{
			var config = new EntitySetConfig<T>(entityName, source, permissions);

			_entities.Add(new EntitySetDefinition(typeof(T), entityName, source, permissions));

			return config;
		}

		public IEnumerable<EntitySetDefinition> Entities
		{
			get { return _entities; }
		}

		public class EntitySetConfig<T>
		{
			public string EntityName { get; private set; }
			public IQueryable<T> Source { get; private set; }
			public EntitySetPermission Permissions { get; private set; }

			public EntitySetConfig(string entityName, IQueryable<T> source, EntitySetPermission permissions)
			{
				EntityName = entityName;
				Source = source;
				Permissions = permissions;
			}

			public EntitySetConfig<T> Authorization()
			{
				return this;
			}
		}
	}
}
