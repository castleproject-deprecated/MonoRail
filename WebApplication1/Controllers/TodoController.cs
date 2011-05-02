namespace WebApplication1.Controllers
{
    using Castle.MonoRail;
    using Models;

    public partial class TodoController
    {
        public ActionResult Index()
        {
            return null;
        }

        public ActionResult View(int id)
        {
            return null;
        }

        public ActionResult Edit(int id)
        {
            return null;
        }

        [HttpMethod(HttpVerb.Post)]
        public ActionResult Create(Model<Todo> todo)
        {
            return null;
        }

        [HttpMethod(HttpVerb.Put)]
        public ActionResult Update(Model<Todo> todo)
        {
            return null;
        }

        [HttpMethod(HttpVerb.Delete)]
        public ActionResult Delete(int id)
        {
            return null;
        }
    }
}