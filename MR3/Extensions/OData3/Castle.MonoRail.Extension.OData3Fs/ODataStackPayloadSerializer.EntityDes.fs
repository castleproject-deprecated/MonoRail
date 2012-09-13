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
    open System.Reflection
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.OData.Atom
    open Newtonsoft.Json


    // Feed/Entry
    type EntityDeserializer() = 
        
        let read_item (rt:IEdmEntityType) target (reader:TextReader)  = 
            
            use jsonReader = new JsonTextReader(reader)
            let instance = 
                if target = null 
                then Activator.CreateInstance rt.TargetType
                else target

            // the two formats we support
            // odata verbose json:
            // { "d": { Prop: a, Prop2: 2 } }
            // standard json:
            // { Prop: a, Prop2: 2 }

            let readUpToPropertyStart () = 
                let doContinue = ref (jsonReader.TokenType <> JsonToken.PropertyName)
                while !doContinue && jsonReader.Read() do
                    if jsonReader.TokenType = JsonToken.PropertyName && jsonReader.Value.ToString() <> "d" then
                        doContinue := false

            let rec rec_read_object (instance) (rt:IEdmType) = 
                
                System.Diagnostics.Debug.Assert ( rt.IsComplex || rt.IsEntity )

                readUpToPropertyStart ()
                
                let doContinue = ref true
                while !doContinue do
                    if jsonReader.TokenType = JsonToken.PropertyName then 
                        
                        let properties = (rt :?> IEdmStructuredType).Properties()
                                
                        let matchingProperties = 
                            properties |> Seq.tryFind (fun p -> p.Name = jsonReader.Value.ToString())

                        match matchingProperties with
                        | Some prop -> 

                            process_read_property prop instance doContinue

                            doContinue := jsonReader.Read()

                        | _ -> failwithf "Property not found on model %s: %O" rt.FName jsonReader.Value 

                    else
                        doContinue := jsonReader.TokenType <> JsonToken.EndObject && jsonReader.Read()
            
            and process_read_property (prop:IEdmProperty) (instance:obj) doContinue = 
                let propType = prop.Type

                match prop.PropertyKind with 
                | EdmPropertyKind.Structural -> 
                    
                    let typedProp = prop :?> TypedEdmStructuralProperty

                    match propType.TypeKind() with 
                    | EdmTypeKind.Enum
                    | EdmTypeKind.Primitive ->
                        jsonReader.Read() |> ignore
                        let value = jsonReader.Value
                        if value <> null then
                            let sanitizedVal = System.Convert.ChangeType(value, prop.Type.Definition.TargetType)
                            typedProp.SetValue(instance, sanitizedVal)
                        else typedProp.SetValue(instance,  null)

                    | EdmTypeKind.Complex ->
                        
                        doContinue := jsonReader.Read()
                        if !doContinue = true then
                            if jsonReader.TokenType = JsonToken.Null then
                                typedProp.SetValue(instance, null)

                            elif jsonReader.TokenType = JsonToken.StartObject then
                                let inner = 
                                    let existinval = typedProp.GetValue(instance)
                                    if existinval = null then
                                        let newVal = System.Activator.CreateInstance prop.Type.Definition.TargetType
                                        newVal
                                    else existinval

                                rec_read_object inner prop.Type.Definition
                                typedProp.SetValue(instance, inner)

                            else failwithf "Unexpected json node type %O" jsonReader.TokenType
                        
                    | EdmTypeKind.Collection ->
                        let list = typedProp.GetValue(instance)
                        if list = null then 
                            failwithf "Null collection property. Please set a default value for property %s on type %s" prop.Name rt.TargetType.FullName

                        // empty the collection, since this is expected to be a HTTP PUT
                        list?Clear() |> ignore

                        doContinue := jsonReader.Read()
                        if !doContinue = true then
                            if jsonReader.TokenType = JsonToken.Null then
                                // nothing to do, since it was cleared already
                                ()  
                            elif jsonReader.TokenType = JsonToken.StartArray then

                                while (jsonReader.Read() && jsonReader.TokenType = JsonToken.StartObject) do
                                    let inner = System.Activator.CreateInstance prop.Type.Definition.TargetType
                                    rec_read_object inner prop.Type.Definition
                                    list?Add(inner) |> ignore
                            
                            else failwithf "Unexpected json node type %O" jsonReader.TokenType

                            typedProp.SetValue(instance, list)


                    | _ -> ()

                | EdmPropertyKind.Navigation -> 

                    let typedProp = prop :?> TypedEdmNavigationProperty

                    match propType.TypeKind() with 
                    | EdmTypeKind.Entity ->
                        ()
                    | EdmTypeKind.EntityReference ->
                        ()
                    | _ -> ()

                | _ -> failwithf "Property not found on model %s: %O" rt.FName jsonReader.Value 


            rec_read_object instance rt 

        member x.ReadEntry (elType:IEdmEntityType, reader) = 
            let instance = Activator.CreateInstance( elType.TargetType )
            read_item elType instance reader
            instance
