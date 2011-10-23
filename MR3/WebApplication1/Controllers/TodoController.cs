namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Castle.MonoRail;
    using Models;

    public partial class TodoController
    {
        public TodoController()
        {
        }

        public ActionResult Index()
        {
            var list = 
                new []
                {
                    new Todo() { Id = 1, Name = "take the garbage old", Email = "aaaha@mma.com" },
					new Todo() { Id = 2, Name = "test", Email = "ha@mma.com" },
                };

        	dynamic viewBag = new PropertyBag<IEnumerable<Todo>>();
        	viewBag.Title = "something";
        	viewBag.Model = list;

			return new ContentNegotiatedResult<IEnumerable<Todo>>(viewBag);
        }

        public ActionResult View(int id)
        {
            var todo = new Todo() {Name = "test", Email = "ha@mma.com"};
			return new ContentNegotiatedResult<Todo>(todo);
        }

        public ActionResult New()
        {
            return new ViewResult() { Model = new Todo() };
        }

        public ActionResult Edit(int id)
        {
            return new ViewResult() { Model = new Todo() { Name = "test", Email = "ha@mma.com" } };
        }

        // [ActionName("Delete")]
        public ActionResult DeleteConfirmation(int id)
        {
            return new ViewResult() { Model = new Todo() { Name = "test", Email = "ha@mma.com" } };
        }

        [HttpMethod(HttpVerb.Post)]
        public ActionResult Create(Model<Todo> todo)
        {
            if (todo.IsValid)
            {
                // create
                // repository.Add(todo.Value);
				return new ContentNegotiatedResult<Todo>(todo.Value) { StatusCode = HttpStatusCode.Created }; //{ RedirectTarget = Urls.View };
            }
            else
            {
				return new ContentNegotiatedResult<Todo>(todo.Value) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [HttpMethod(HttpVerb.Put)]
        public ActionResult Update(Model<Todo> todo)
        {
            if (todo.IsValid)
            {
                // update
                // repository.Update(todo.Value);
				return new ContentNegotiatedResult<Todo>(todo.Value); //{ RedirectTarget = Urls.View };
            }
            else
            {
				return new ContentNegotiatedResult<Todo>(todo.Value)
                           {
                                StatusCode = HttpStatusCode.BadRequest, RedirectBrowserTo = Urls.Edit.Get(1)
                           };
            }
        }

        [HttpMethod(HttpVerb.Delete)]
        public ActionResult Delete(int id)
        {
            // repository.Delete(id)
			return new ContentNegotiatedResult() { RedirectBrowserTo = Urls.Index.Get(), StatusCode = HttpStatusCode.NoContent };
        }
    }
}