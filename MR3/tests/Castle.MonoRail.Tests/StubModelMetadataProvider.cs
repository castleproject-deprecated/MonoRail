#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Tests
{
    using System;
    using System.Collections.Generic;

    public class StubModelMetadataProvider : ModelMetadataProvider
    {
        private Func<Type, ModelMetadata> _creator;
        private Dictionary<Type, ModelMetadata> _type2Meta = new Dictionary<Type, ModelMetadata>();

        public StubModelMetadataProvider(Func<Type, ModelMetadata> creator)
        {
            _creator = creator;
        }

        public Dictionary<Type, ModelMetadata> Type2Meta
        {
            get { return _type2Meta; }
        }

        public override ModelMetadata Create(Type type)
        {
            ModelMetadata meta;
            if (_type2Meta.TryGetValue(type, out meta))
            {
                return meta;
            }

            if (_creator != null)
                return _creator(type);
            else
                return new ModelMetadata(type);
        }
    }
}
