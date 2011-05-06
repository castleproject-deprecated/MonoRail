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

namespace Castle.MonoRail.Serializers

    open System.IO
    open System.Web
    open Newtonsoft.Json

    [<Interface>]
    type public IModelSerializer<'a> = 
        abstract member Serialize : model:'a * contentType:string * writer:System.IO.TextWriter -> unit
        abstract member Deserialize : prefix:string * request:HttpRequest -> 'a


    type JsonSerializer<'a>() = 
        
        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                // very inneficient for large models
                let content = Newtonsoft.Json.JsonConvert.SerializeObject(model)
                writer.Write content

            member x.Deserialize (prefix, request) = 
                // very inneficient for large inputs
                let reader = new StreamReader(request.InputStream)
                let content = reader.ReadToEnd()
                Newtonsoft.Json.JsonConvert.DeserializeObject<'a>(content)


    type XmlSerializer<'a>() = 

        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                
                ()

            member x.Deserialize (prefix, request) = 
                Unchecked.defaultof<'a>


    type FormBasedSerializer<'a>() = 

        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                ()

            member x.Deserialize (prefix, request) = 
                Unchecked.defaultof<'a>
