namespace WebApplication1
{
    using System.ComponentModel.Composition;
    using Castle.MonoRail;
    using Castle.MonoRail.Serialization;

    [Export(typeof(IModuleStarter))]
    public class ModuleStarter : IModuleStarter
    {
        [Import]
        public ModelSerializerResolver SerializerResolver { get; set; }

        [Import]
        public ModelHypertextProcessorResolver HPResolver { get; set; }

        public void Initialize()
        {

            // Configure whatever you want here (data annotation provider, validation provider, serializers)
            // SerializerResolver.Register<>();
        }
    }
}