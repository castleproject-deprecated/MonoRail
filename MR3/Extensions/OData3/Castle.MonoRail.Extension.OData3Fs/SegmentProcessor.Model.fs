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
    open System.IO
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library


    module ProcessorUtils = 
        begin
            let internal createSetting (serviceUri) (format) = 
                let messageWriterSettings = 
                    ODataMessageWriterSettings(BaseUri = serviceUri,
                                               Version = Nullable(Microsoft.Data.OData.ODataVersion.V3),
                                               Indent = true,
                                               CheckCharacters = false,
                                               DisableMessageStreamDisposal = false
                                              )
                
                messageWriterSettings.SetContentType(format)
                // messageWriterSettings.EnableWcfDataServicesServerBehavior(provider.IsV1Provider);
                // messageWriterSettings.SetContentType(acceptHeaderValue, acceptCharSetHeaderValue);
                messageWriterSettings


        end


    [<AbstractClass;AllowNullLiteral>]
    type ODataSegmentProcessor (model:IEdmModel) =  
     
        abstract member Process : op:RequestOperation * request:ODataRequestMessage * response:ODataResponseMessage -> ResponseToSend


    type MetadataProcessor(model) = 
        inherit ODataSegmentProcessor(model)

        override x.Process (op, request, response) = 
            match op with 
            | RequestOperation.Get ->
                response.SetHeader("Content-Type", "application/xml")

                let settings = ProcessorUtils.createSetting request.Url ODataFormat.Metadata
                use writer = new ODataMessageWriter(response, settings, model)
                writer.WriteMetadataDocument()

            | _ -> failwithf "Unsupported operation %O at this level" op

            emptyResponse