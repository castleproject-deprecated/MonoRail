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

        let createSettingsForFormat (serviceUri) (format) = 
            let messageWriterSettings = 
                ODataMessageWriterSettings(BaseUri = serviceUri,
                                            Version = Nullable(Microsoft.Data.OData.ODataVersion.V3),
                                            Indent = true,
                                            CheckCharacters = false,
                                            DisableMessageStreamDisposal = false,
                                            MetadataDocumentUri = serviceUri )
            messageWriterSettings.SetContentType(format)
            messageWriterSettings

        let createSettingsForRequest (serviceUri) (request:IODataRequestMessage) = 
            let acceptHeaderValue, acceptCharSetHeaderValue = (request.GetHeader("Accept")), (request.GetHeader("Accept-charset"))
            let messageWriterSettings = 
                ODataMessageWriterSettings(BaseUri = serviceUri,
                                            Version = Nullable(Microsoft.Data.OData.ODataVersion.V3),
                                            Indent = true,
                                            CheckCharacters = false,
                                            DisableMessageStreamDisposal = false,
                                            MetadataDocumentUri = serviceUri )
            let acceptCharSetHeaderValue = 
                if (String.IsNullOrEmpty(acceptCharSetHeaderValue) || acceptCharSetHeaderValue = "*")
                then "UTF-8" else acceptCharSetHeaderValue
            messageWriterSettings.SetContentType(acceptHeaderValue, acceptCharSetHeaderValue);
            messageWriterSettings

        let createWriter (request:IODataRequestMessage) (response:IODataResponseMessage) = 
            let settings = createSettingsForRequest request.Url request
            new ODataMessageWriter(response, settings)
            
        let createReaderSettings (serviceUri) = 
            let readerSettings = 
                ODataMessageReaderSettings(BaseUri = serviceUri,
                                           CheckCharacters = false,
                                           DisableMessageStreamDisposal = true,
                                           DisablePrimitiveTypeConversion = false,
                                           MaxProtocolVersion = ODataVersion.V3
                                          )
            readerSettings.MessageQuotas.MaxReceivedMessageSize <- Int64.MaxValue
            readerSettings

        override x.SerializeError (exc, request, response) = 
            use writer = createWriter request response
            let odataError = ODataError(Message = exc.Message, InnerError = ODataInnerError(exc))
            writer.WriteError (odataError, true)

        override x.SerializeServiceDoc (request, response) =
            let build_coll_info (set:IEdmEntitySet) = 
                let info = ODataResourceCollectionInfo(Url = Uri(set.Name, UriKind.Relative))
                info.SetAnnotation(AtomResourceCollectionMetadata(Title = AtomTextConstruct(Text = set.Name) ))
                info
            let settings = createSettingsForFormat request.Url ODataFormat.Atom
            use writer = new ODataMessageWriter(response, settings, edmModel)
            let sets = edmModel.EntityContainers() |> Seq.collect (fun c -> c.EntitySets())
            let coll = sets |> Seq.map build_coll_info
            let workspace = ODataWorkspace(Collections = coll)
            writer.WriteServiceDocument(workspace)

        override x.SerializeMetadata (request, response) =
            response.SetHeader("Content-Type", "application/xml")

            let settings = createSettingsForFormat request.Url ODataFormat.Metadata
            use writer = new ODataMessageWriter(response, settings, edmModel)
            writer.WriteMetadataDocument()

        override x.SerializeFeed (models, edmEntSet, edmEntType, request, response) = 
            use writer = createWriter request response
            let serializer = EntitySerializer( writer )
            serializer.WriteFeed (edmEntSet, models, edmEntType.Definition :?> IEdmEntityType)

        override x.SerializeEntry (model, edmEntSet, edmEntType, request, response) = 
            use writer = createWriter request response
            let serializer = EntitySerializer( writer )
            serializer.WriteEntry (edmEntSet, model, edmEntType.Definition :?> IEdmEntityType)

        override x.SerializeCollection (models, edmType, request:IODataRequestMessage, response:IODataResponseMessage) = 
            use writer = createWriter request response
            let serializer = NonEntitySerializer( writer )
            serializer.WriteCollection(models, edmType)

        override x.SerializeProperty (model:obj, edmType, request, response) = 
            use writer = createWriter request response
            let serializer = NonEntitySerializer( writer )
            serializer.WriteProperty(model, edmType)

        override x.SerializeValue (value:obj, edmType, request, response) = 
            use writer = createWriter request response
            let serializer = TextSerializer(writer)
            serializer.WriteValue (value, edmType)

        override x.Deserialize (edmType, request) =
            System.Diagnostics.Debug.Assert(edmType.IsEntity())
            
            let deserializer = EntityDeserializer()
            let isMerge = request.Method === "MERGE"
            let instance = Activator.CreateInstance( edmType.Definition.TargetType )

            let reader = new StreamReader(request.GetStream(), Encoding.UTF8)
            deserializer.ReadEntry (instance, edmType.Definition :?> IEdmEntityType, reader, isMerge)



