namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Castle.MonoRail;
    using Models;

    // []
    public partial class TodoController
    {
        public TodoController()
        {
        }

        public ActionResult Index()
        {
            var list = 
                new Todo[]
                {
                    new Todo() { Name = "test", Email = "ha@mma.com" },
                };

            return new ContentResult<IEnumerable<Todo>>(list);
        }

        public ActionResult View(int id)
        {
            var todo = new Todo() {Name = "test", Email = "ha@mma.com"};
            return new ContentResult<Todo>(todo); //.When(MimeType.Xhtml, () => new ViewResult() { ViewName = ""});
            // return new ViewResult() { Model = new Todo() { Name = "test", Email = "ha@mma.com" } };
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
                return new ContentResult<Todo>(todo.Value) { StatusCode = HttpStatusCode.Created }; //{ RedirectTarget = Urls.View };
            }
            else
            {
                return new ContentResult<Todo>(todo.Value) { StatusCode = HttpStatusCode.BadRequest };
            }
//            catch (Exception) // should be managed by filter
//            {
//                return new ContentResult() { StatusCode = HttpStatusCode.InternalServerError };
//            }
        }

        [HttpMethod(HttpVerb.Put)]
        public ActionResult Update(Model<Todo> todo)
        {
            if (todo.IsValid)
            {
                // update
                // repository.Update(todo.Value);
                return new ContentResult<Todo>(todo.Value); //{ RedirectTarget = Urls.View };
            }
            else
            {
                return new ContentResult<Todo>(todo.Value)
                           {
                                StatusCode = HttpStatusCode.BadRequest, RedirectBrowserTo = Urls.Edit(1)
                           };
            }

//            catch (Exception)
//                return new ContentResult() { StatusCode = HttpStatusCode.InternalServerError };
        }

        [HttpMethod(HttpVerb.Delete)]
        public ActionResult Delete(int id)
        {
            // repository.Delete(id)
            return new ContentResult() {RedirectBrowserTo = Urls.Index(), StatusCode = HttpStatusCode.NoContent};
        }
    }
}