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

namespace Castle.MonoRail.Extension.OData.Serialization

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Xml
open System.IO
open System.Text
open System.ServiceModel.Syndication
open System.Data.OData
open System.Data.Services.Providers
open System.Data.Services.Common

module AtomSerialization = 
    begin
        let private emptyPerson = SyndicationPerson(null, String.Empty, null)
        let private qualifiedNullAttribute = XmlQualifiedName("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
        let private qualifiedDataWebPrefix = XmlQualifiedName("d", "http://www.w3.org/2000/xmlns/")
        let private qualifiedDataWebMetadataPrefix = XmlQualifiedName("m", "http://www.w3.org/2000/xmlns/")
        let private linkRelResource = Uri("http://schemas.microsoft.com/ado/2007/08/dataservices/related/")
        let private categoryScheme = "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"

        type AtomSerializer() = 
            class 
                inherit Serializer() 


            end


        let CreateDeserializer () = 
            { new Deserializer() with 
                override x.DeserializeMany (rt, reader, enc) = 
                    raise(NotImplementedException())
                override x.DeserializeSingle (rt, reader, enc) = 
                    read_item rt reader enc
            }

    end
