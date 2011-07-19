namespace Castle.MonoRail.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    public abstract class HelperTestsBase
    {
        protected IDictionary<string, object> _viewBag;
        protected ViewContext _ctx;
        protected HttpContextStub _httpCtx;
        protected StubServiceRegistry _serviceRegistry;
        protected StubModelMetadataProvider _modelProvider;
        protected HelperContext _helperContext;

        [SetUp]
        public virtual void Init()
        {
            var viewReq = new ViewRequest();
            Init(viewReq);
        }

        public void Init(ViewRequest viewReq)
        {
            _viewBag = new Dictionary<string, object>();
            _httpCtx = new HttpContextStub();
            _ctx = new ViewContext(_httpCtx, _viewBag, new object(), viewReq);
            _serviceRegistry = CreateStubServiceRegistry();
            _modelProvider = CreateMetadataProvider();
            _helperContext = new HelperContext(_ctx, _modelProvider, _serviceRegistry);
        }

        protected virtual StubModelMetadataProvider CreateMetadataProvider()
        {
            return new StubModelMetadataProvider(t => BuildMetadataFor(t, null));
        }

        protected virtual StubServiceRegistry CreateStubServiceRegistry()
        {
            return new StubServiceRegistry();
        }

        protected static ModelMetadata BuildMetadataFor<T>(Func<Dictionary<PropertyInfo, ModelMetadata>> buildDict)
        {
            return BuildMetadataFor(typeof (T), buildDict);
        }

        protected static ModelMetadata BuildMetadataFor(Type type, Func<Dictionary<PropertyInfo, ModelMetadata>> buildDict)
        {
            var dict = new Dictionary<PropertyInfo, ModelMetadata>();
            if (buildDict != null)
            {
                dict = buildDict();
            }
            foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!dict.ContainsKey(info))
                {
                    dict[info] = new ModelMetadata(type, info);
                }
            }

            return new ModelMetadata(type, null, dict);
        }
    }
}
