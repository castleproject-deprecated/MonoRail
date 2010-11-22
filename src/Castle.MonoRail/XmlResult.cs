namespace Castle.MonoRail
{
    using System;
    using Hosting.Mvc.Typed;

    public class XmlResult : ActionResult
    {
        public override void Execute(ActionResultContext context, IMonoRailServices services)
        {
            throw new NotImplementedException();
        }
    }
}
