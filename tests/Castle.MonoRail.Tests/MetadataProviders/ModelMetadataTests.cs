#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Tests.MetadataProviders
{
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class ModelMetadataTests
    {
        private ModelMetadata modelForType;

        [SetUp]
        public void BuildMetadata()
        {
            var properties = typeof(School).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var prop2Metadata = new Dictionary<PropertyInfo, ModelMetadata>();
            foreach (var prop in properties)
            {
                prop2Metadata[prop] = new ModelMetadata(typeof(School), prop);
            }

            modelForType = new ModelMetadata(typeof(School), null, prop2Metadata);
        }

        [Test]
        public void CanGetPropertyMetadataForExistingProperty()
        {
            var propMetadata = modelForType.GetPropertyMetadata(typeof(School).GetProperty("Name"));
            Assert.IsNotNull(propMetadata);
        }

        [Test, ExpectedException]
        public void GetPropertyMetadataForNonExistingProperty_ThrowsException()
        {
            modelForType.GetPropertyMetadata(typeof(School).GetProperty("Age"));
        }

        [Test]
        public void GetValue_ReturnsCurrentValue()
        {
            var propMetadata = modelForType.GetPropertyMetadata(typeof(School).GetProperty("Name"));
            var result = propMetadata.GetValue(new School() {Name = "Giordano"});
            Assert.AreEqual("Giordano", result);
        }


        class School
        {
            public string Name { get; set; }
        }

    }
}
