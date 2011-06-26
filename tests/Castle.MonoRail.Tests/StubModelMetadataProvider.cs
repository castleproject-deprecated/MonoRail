namespace Castle.MonoRail.Tests
{
    using System;

    class StubModelMetadataProvider : ModelMetadataProvider
    {
        public override ModelMetadata Create(Type type)
        {
            return new ModelMetadata();
        }
    }
}
