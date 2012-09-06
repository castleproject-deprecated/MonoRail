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

namespace Castle.MonoRail.OData.Internal

    open System
    open System.Collections
    open System.Collections.Specialized
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.OData.Atom



    type ODataStackPayloadSerializer(edmModel:IEdmModel, serviceUri:Uri) = 
        inherit PayloadSerializer()

        let createSetting (serviceUri) (format) = 
            let messageWriterSettings = 
                ODataMessageWriterSettings(BaseUri = serviceUri,
                                            Version = Nullable(Microsoft.Data.OData.ODataVersion.V3),
                                            Indent = true,
                                            CheckCharacters = false,
                                            DisableMessageStreamDisposal = false )
            messageWriterSettings.SetContentType(format)
            // messageWriterSettings.SetContentType(acceptHeaderValue, acceptCharSetHeaderValue);
            messageWriterSettings


        override x.SerializeServiceDoc (request, response) =
            let build_coll_info (set:IEdmEntitySet) = 
                let info = ODataResourceCollectionInfo(Url = Uri(set.Name, UriKind.Relative))
                info.SetAnnotation(AtomResourceCollectionMetadata(Title = AtomTextConstruct(Text = set.Name) ))
                info

            let settings = createSetting request.Url ODataFormat.Metadata
            use writer = new ODataMessageWriter(response, settings, edmModel)
            let sets = edmModel.EntityContainers() |> Seq.collect (fun c -> c.EntitySets())
            let coll = sets |> Seq.map build_coll_info

            let workspace = ODataWorkspace(Collections = coll)

            writer.WriteServiceDocument(workspace) 

        override x.SerializeMetadata (request, response) =
            response.SetHeader("Content-Type", "application/xml")

            let settings = createSetting request.Url ODataFormat.Metadata
            use writer = new ODataMessageWriter(response, settings, edmModel)
            writer.WriteMetadataDocument()

        (*
        override x.SerializeMany (models, edmType, request, response) = 

            ()
        
        override x.SerializeSingle (model, edmType, request, response) = 

            
            ()
        
        override x.SerializeValue (value, edmType, request, response) = 
            ()
        *)

        override x.Deserialize (edmType, request) = 
            null


