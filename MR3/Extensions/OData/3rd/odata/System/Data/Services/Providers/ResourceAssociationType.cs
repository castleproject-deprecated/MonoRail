namespace System.Data.Services.Providers
{
	public class ResourceAssociationTypeEnd
	{
		private readonly string name;
		private readonly ResourceType resourceType;
		private readonly ResourceProperty resourceProperty;
		private readonly ResourceProperty fromProperty;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public ResourceType ResourceType
		{
			get
			{
				return this.resourceType;
			}
		}

		public ResourceProperty ResourceProperty
		{
			get
			{
				return this.resourceProperty;
			}
		}

		public string Multiplicity
		{
			get
			{
				if (this.fromProperty != null && this.fromProperty.Kind == ResourcePropertyKind.ResourceReference)
					return "0..1";
				else
					return "*";
			}
		}

		public ResourceAssociationTypeEnd(string name, ResourceType resourceType, ResourceProperty resourceProperty, ResourceProperty fromProperty)
		{
			this.name = name;
			this.resourceType = resourceType;
			this.resourceProperty = resourceProperty;
			this.fromProperty = fromProperty;
		}
	}

	public class ResourceAssociationType
	{
		private readonly string fullName;
		private readonly string name;
		private readonly ResourceAssociationTypeEnd end1;
		private readonly ResourceAssociationTypeEnd end2;

		public string FullName
		{
			get
			{
				return this.fullName;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public ResourceAssociationTypeEnd End1
		{
			get
			{
				return this.end1;
			}
		}

		public ResourceAssociationTypeEnd End2
		{
			get
			{
				return this.end2;
			}
		}

		public ResourceAssociationType(string name, string namespaceName, ResourceAssociationTypeEnd end1, ResourceAssociationTypeEnd end2)
		{
			this.name = name;
			this.fullName = namespaceName + "." + name;
			this.end1 = end1;
			this.end2 = end2;
		}

		public ResourceAssociationTypeEnd GetResourceAssociationTypeEnd(ResourceType resourceType, ResourceProperty resourceProperty)
		{
			ResourceAssociationTypeEnd[] associationTypeEndArray = new ResourceAssociationTypeEnd[2]
	  {
		this.end1,
		this.end2
	  };
			foreach (ResourceAssociationTypeEnd associationTypeEnd in associationTypeEndArray)
			{
				if (associationTypeEnd.ResourceType == resourceType && associationTypeEnd.ResourceProperty == resourceProperty)
					return associationTypeEnd;
			}
			return (ResourceAssociationTypeEnd)null;
		}

		public ResourceAssociationTypeEnd GetRelatedResourceAssociationSetEnd(ResourceType resourceType, ResourceProperty resourceProperty)
		{
			ResourceAssociationTypeEnd associationTypeEnd1 = this.GetResourceAssociationTypeEnd(resourceType, resourceProperty);
			if (associationTypeEnd1 != null)
			{
				ResourceAssociationTypeEnd[] associationTypeEndArray = new ResourceAssociationTypeEnd[2]
		{
		  this.end1,
		  this.end2
		};
				foreach (ResourceAssociationTypeEnd associationTypeEnd2 in associationTypeEndArray)
				{
					if (associationTypeEnd2 != associationTypeEnd1)
						return associationTypeEnd2;
				}
			}
			return (ResourceAssociationTypeEnd)null;
		}
	}
}
