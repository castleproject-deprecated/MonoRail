namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Data.Services.Providers;
	using System.Linq;

	public class EntitySetDefinition
	{
		internal readonly Type _entityType;
		internal readonly string _entityName;
		internal readonly IQueryable _source;
		internal readonly EntitySetPermission _permissions;

		public EntitySetDefinition(Type entityType, string entityName, IQueryable source, EntitySetPermission permissions)
		{
			_entityType = entityType;
			_entityName = entityName;
			_source = source;
			_permissions = permissions;
		}

		public ResourceSetWrapper Wrapper { get; set; }
	}
}