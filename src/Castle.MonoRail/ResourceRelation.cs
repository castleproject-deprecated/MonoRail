namespace Castle.MonoRail
{
    using System;

    public class ResourceRelation
    {
        public string Name { get; private set; }
        public Uri Resource { get; private set; }

        public ResourceRelation(string name, Uri resource)
        {
            Name = name;
            Resource = resource;
        }
    }
}