using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;

namespace Castle.MonoRail.Extension.OData3.Tests
{
    class StubEdmFunctionParameter : IEdmFunctionParameter
    {
        public StubEdmFunctionParameter()
        {
            this.Mode = EdmFunctionParameterMode.In;
        }

        public string Name { get; internal set; }
        public IEdmTypeReference Type { get; internal set; }
        public IEdmFunctionBase DeclaringFunction { get; internal set; }
        public EdmFunctionParameterMode Mode { get; internal set; }
    }

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
