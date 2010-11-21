namespace Castle.MonoRail
{
    using System;
    using System.Collections.Generic;

    public class Resource<T> where T : class
    {
        private readonly List<ResourceRelation> _resourceRelations = new List<ResourceRelation>();
        private readonly T _value;

        public Resource(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            _value = value;
        }

        public void AddRelation(ResourceRelation relation)
        {
            _resourceRelations.Add(relation);
        }

        public T Value { get { return _value; } }

        public IEnumerable<ResourceRelation> Relations { get { return _resourceRelations; } }
    }
}
