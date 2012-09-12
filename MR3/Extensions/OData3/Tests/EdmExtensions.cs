using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	static class EdmExtensions
	{

		public static IEdmCollectionTypeReference AsCollectionRef(this IEdmEntityTypeReference type)
		{
			return EdmCoreModel.GetCollection(type);
		}
	}
}
