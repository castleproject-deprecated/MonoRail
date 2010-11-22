namespace Castle.MonoRail
{
    using System;
    using Castle.MonoRail.Mvc.Typed;

    public class XmlResult : ActionResult
    {
        public override void Execute(ActionResultContext context, IMonoRailServices services)
        {
            throw new NotImplementedException();
        }
    }
}
