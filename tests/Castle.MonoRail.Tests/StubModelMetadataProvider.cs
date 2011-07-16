namespace Castle.MonoRail.Tests
{
    using System;

    class StubModelMetadataProvider : ModelMetadataProvider
    {
        private Func<Type, ModelMetadata> _creator;

        public StubModelMetadataProvider(Func<Type, ModelMetadata> creator)
        {
            _creator = creator;
        }

        public override ModelMetadata Create(Type type)
        {
            if (_creator != null)
                return _creator(type);
            else
                return new ModelMetadata(type);
        }
    }
}
