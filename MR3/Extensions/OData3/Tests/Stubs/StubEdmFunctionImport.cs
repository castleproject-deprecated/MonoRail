using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	class StubEdmFunctionImport : IEdmFunctionImport
	{
		public StubEdmFunctionImport()
		{
		}

		public string Name { get; set; }
		public bool IsSideEffecting { get; set; }
		public bool IsComposable { get; set; }
		public bool IsBindable { get; set; }
		public IEdmExpression EntitySet { get; set; }
		public IEdmTypeReference ReturnType { get; set; }
		public IEnumerable<IEdmFunctionParameter> Parameters { get; set; }

		public EdmContainerElementKind ContainerElementKind
		{
			get { return EdmContainerElementKind.FunctionImport; }
		}

		public IEdmEntityContainer Container { get; set; }

		public IEdmFunctionParameter FindParameter(string name)
		{
			return Parameters.Where(p => p.Name == name).SingleOrDefault();
		}
	}
}
