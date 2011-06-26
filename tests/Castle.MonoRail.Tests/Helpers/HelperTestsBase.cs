namespace Castle.MonoRail.Tests.Helpers
{
    using System.Collections.Generic;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    public abstract class HelperTestsBase
    {
        protected IDictionary<string, object> _viewBag;
        protected ViewContext _ctx;
        protected HttpContextStub _httpCtx;

        [SetUp]
        public virtual void Init()
        {
            _viewBag = new Dictionary<string, object>();
            var viewReq = new ViewRequest();
            _httpCtx = new HttpContextStub();
            _ctx = new ViewContext(_httpCtx, _viewBag, new object(), viewReq);

        }
    }
}
