namespace Castle.MonoRail3.Primitives.Mvc
{
    // do we need sentinels between buckets to enforce a sane status? 
    // i.e. confirm expectations/validate assumptions
    public interface IControllerExecutionSink
    {
        IControllerExecutionSink Next { get; set; }

        void Invoke(ControllerExecutionContext executionCtx);
    }
}
