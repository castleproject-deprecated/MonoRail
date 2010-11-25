namespace Castle.MonoRail.Mvc.Rest
{
    using System;
    using System.IO;

    public abstract class FormatSerializer
    {
        public abstract void Serialize(object data, Stream output);

        public abstract object Deserialize(Type modelType, Stream input);
    }
}
