using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	static class EdmExtensions
	{

		public static IEdmCollectionType AsCollectionRef(this IEdmEntityType type)
		{
			return (IEdmCollectionType) EdmCoreModel.GetCollection(new EdmEntityTypeReference(type, false)).Definition;
		}
	}
}
