namespace WebApplication1.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Castle.MonoRail;
    using WebApplication1.Models;

    public partial class UserController
    {
        public ActionResult Index()
        {
            var list =
                new[]
                {
                    new SiteUser() { Id = 1, Name = "take the garbage old", Email = "aaaha@mma.com" },
					new SiteUser() { Id = 2, Name = "test", Email = "ha@mma.com" },
                };

            dynamic viewBag = new PropertyBag<IEnumerable<SiteUser>>();
            viewBag.Title = "something";
            viewBag.Model = list;

            return new ContentNegotiatedResult<IEnumerable<SiteUser>>(viewBag);
        }

        public ActionResult View(int id)
        {
            var todo = new SiteUser() { Name = "test", Email = "ha@mma.com" };
            return new ContentNegotiatedResult<SiteUser>(todo);
        }

        public ActionResult New()
        {
            return new ViewResult() { Model = new SiteUser() };
        }

        public ActionResult Edit(int id)
        {
            return new ViewResult() { Model = new SiteUser() { Name = "test", Email = "ha@mma.com" } };
        }

        // [ActionName("Delete")]
        public ActionResult DeleteConfirmation(int id)
        {
            return new ViewResult() { Model = new SiteUser() { Name = "test", Email = "ha@mma.com" } };
        }

        // [HttpMethod(HttpVerb.Post)]
        public ActionResult PostCreate(Model<SiteUser> todo)
        {
            if (todo.IsValid)
            {
                // create
                // repository.Add(todo.Value);
                return new ContentNegotiatedResult<SiteUser>(todo.Value) { StatusCode = HttpStatusCode.Created }; //{ RedirectTarget = Urls.View };
            }
            else
            {
                return new ContentNegotiatedResult<SiteUser>(todo.Value) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        // [HttpMethod(HttpVerb.Put)]
        public ActionResult PutUpdate(Model<SiteUser> todo)
        {
            if (todo.IsValid)
            {
                // update
                return new ContentNegotiatedResult<SiteUser>(todo.Value); //{ RedirectTarget = Urls.View };
            }
            else
            {
                return new ContentNegotiatedResult<SiteUser>(todo.Value)
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    RedirectBrowserTo = Urls.Edit.Get(1)
                };
            }
        }

        [HttpMethod(HttpVerb.Delete)]
        public ActionResult Remove(int id)
        {
            // repository.Delete(id)
            return new ContentNegotiatedResult()
                       {
                           RedirectBrowserTo = Urls.Index.Get(), 
                           StatusCode = HttpStatusCode.NoContent
                       };
        }

		public ActionResult Post_Role(string name)
		{
			return new HttpResult(HttpStatusCode.OK);
		}

//		public ActionResult Delete_Role(int id)
//		{
//			return new HttpResult(HttpStatusCode.OK);
//		}
    }
}