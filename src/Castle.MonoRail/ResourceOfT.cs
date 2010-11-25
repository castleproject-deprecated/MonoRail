#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

namespace Castle.MonoRail
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    // this needs some more thought
    [DataContract(IsReference = true, Name = "Resource")]
    public class Resource<T> where T : class
    {
        private readonly List<ResourceRelation> _resourceRelations = new List<ResourceRelation>();
        private T _value;

        public Resource(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _value = value;
        }

        public void AddRelation(ResourceRelation relation)
        {
            if (relation == null)
                throw new ArgumentNullException("relation");

            _resourceRelations.Add(relation);
        }

        [DataMember]
        public string Name { get { return typeof(T).Name; } set { ; } }

        [DataMember]
        public T Value { get { return _value; } protected set { _value = value; } }

        [DataMember]
        public IEnumerable<ResourceRelation> Relations { get { return _resourceRelations; } }
    }
}
