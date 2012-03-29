namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using System.Reflection;

	public class ODataMetadataBuilder
	{
		private readonly ODataModel _model;
		private readonly Dictionary<Type, ResourceType> _knownTypes = new Dictionary<Type, ResourceType>();
		private readonly Dictionary<Type, string> _type2Name;
		private readonly string _schemaNamespace;

		public ODataMetadataBuilder(ODataModel model)
		{
			_model = model;
			_type2Name = model.Entities.ToDictionary(e => e._entityType, e => e._entityName);
			_schemaNamespace = model.SchemaNamespace;
		}

		public IEnumerable<ResourceType> Build()
		{
			var resourceTypes = new List<ResourceType>();

			foreach (var ent in _model.Entities)
			{
				var entityRT = BuildResourceTypeForEntity(ent);
				resourceTypes.Add(entityRT);
			}

			return resourceTypes;
		}

		private ResourceType BuildResourceTypeForEntity(EntitySetDefinition ent)
		{
			var resType = BuildResourceType(ent._entityType);

			if (resType == null || resType.ResourceTypeKind != ResourceTypeKind.EntityType)
			{
				throw new Exception("Unexpected return from BuildResourceType");
			}

			return resType;
		}

		private ResourceType BuildResourceType(Type type)
		{
			if (type.IsValueType || !type.IsVisible || type.IsArray || type.IsPointer || type.IsCOMObject || type.IsInterface || 
				type == typeof(IntPtr) || type == typeof(UIntPtr) || type == typeof(char) || type == typeof(TimeSpan) || 
				type == typeof(DateTimeOffset) || type == typeof(Uri) || type.IsEnum) 
			{
				return null; 
			}

			if (_knownTypes.ContainsKey(type))
			{
				return _knownTypes[type];
			}

			// note: no support for hierarchies of resource types yet

			var kind = ResourceTypeKind.ComplexType;
			if (HasKeyPropertyOrNonPrimitiveProperty(type))
			{
				kind = ResourceTypeKind.EntityType;
			}

			var entityName = ResolveEntityName(type);
			var resourceType = new ResourceType(type, kind, null, _schemaNamespace, entityName, false);
			_knownTypes[type] = resourceType;

			var props = BuildResourceProperties(type);
			foreach (var prop in props)
			{
				resourceType.AddProperty(prop);	
			}

			return resourceType;
		}

		private static bool HasKeyPropertyOrNonPrimitiveProperty(Type type)
		{
			return 
				type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
					.Any(p => p.IsDefined(typeof (KeyAttribute), true) || !IsPrimitive(p.PropertyType));
		}

		private string ResolveEntityName(Type type)
		{
			string name;
			if (!_type2Name.TryGetValue(type, out name))
			{
				name = type.Name;
			}
			return name;
		}

		private IEnumerable<ResourceProperty> BuildResourceProperties(Type entType)
		{
			var properties = new List<ResourceProperty>();

			foreach (var resourceProperty in entType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
			{
				if (!resourceProperty.CanRead || resourceProperty.GetIndexParameters().Length != 0)
				{
					continue;
				}

				var propType = resourceProperty.PropertyType;
				var isCollectionType = false;
				ResourcePropertyKind kind = 0;

				var resolvedType = ResolveType(propType);
				if (resolvedType == null)
				{
					var collectionElement = TryResolveCollectionElement(propType);
					if (collectionElement != null)
					{
						resolvedType = ResolveType(collectionElement);
						isCollectionType = true;
					}
				}

				if (resolvedType == null)
				{
					resolvedType = BuildResourceType(propType);
				}

				if (resolvedType == null)
				{
					throw new Exception("Could not resolve ResType for " + propType);
				}

				kind = ResolvePropertyKindFromType(resolvedType, isCollectionType, resourceProperty);

				var property = new ResourceProperty(resourceProperty.Name, kind, resolvedType);
				properties.Add(property);
			}

			return properties;
		}

		private static bool IsPrimitive(Type type)
		{
			return ResourceType.GetPrimitiveResourceType(type) != null;
		}

		private static ResourcePropertyKind ResolvePropertyKindFromType(ResourceType resourceType, bool isCollection, PropertyInfo propertyInfo)
		{
			ResourcePropertyKind kind = (0);

			if (resourceType.ResourceTypeKind == ResourceTypeKind.Primitive)
			{
				kind = ResourcePropertyKind.Primitive;

				if (propertyInfo.IsDefined(typeof(KeyAttribute), true))
				{
					kind |= ResourcePropertyKind.Key;
				}
			}
			else if (resourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
			{
				kind = ResourcePropertyKind.ComplexType;
			}
			else if (resourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
			{
				kind = isCollection ? 
					ResourcePropertyKind.ResourceSetReference : 
					ResourcePropertyKind.ResourceReference;
			}
			else if (resourceType.ResourceTypeKind == ResourceTypeKind.MultiValue)
			{
				throw new NotSupportedException("ResourceTypeKind.MultiValue");
			}

			return kind;
		}

		private static Type TryResolveCollectionElement(Type type)
		{
			var interType = type.GetInterface(typeof (IEnumerable<>).Name, false);

			if (type.IsGenericType && interType != null)
			{
				return type.GetGenericArguments()[0];
			}
			return null;
		}

		private ResourceType ResolveType(Type type)
		{
			var resourceType = ResourceType.GetPrimitiveResourceType(type);

			if (resourceType != null) return resourceType;

			_knownTypes.TryGetValue(type, out resourceType);

			return resourceType;
		}
	}
}