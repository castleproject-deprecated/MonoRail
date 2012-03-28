namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.Data.Services.Providers;
	using System.Linq;

	public abstract class ODataModel
	{
		private readonly List<EntitySetDefinition> _entities = new List<EntitySetDefinition>();
		private readonly Lazy<IEnumerable<ResourceType>> _resourceTypes;
		private readonly Lazy<IEnumerable<ResourceSet>> _resourceSets;

		protected ODataModel()
		{
			_resourceTypes = new Lazy<IEnumerable<ResourceType>>(BuildResourceTypes);
			_resourceSets = new Lazy<IEnumerable<ResourceSet>>(BuildResourceSets);
		}

		public string SchemaNamespace { get; set; }

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

		protected internal IEnumerable<ResourceType> ResourceTypes 
		{
			get { return _resourceTypes.Value; } 
		}

		protected internal IEnumerable<ResourceSet> ResourceSets
		{
			get { return _resourceSets.Value; }
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

		private IEnumerable<ResourceType> BuildResourceTypes()
		{
			return new ODataMetadataBuilder(this).Build();
		}

		private IEnumerable<ResourceSet> BuildResourceSets()
		{
			return ResourceTypes.Select(rt => new ResourceSet(rt.Name, rt));
		}
	}
}
