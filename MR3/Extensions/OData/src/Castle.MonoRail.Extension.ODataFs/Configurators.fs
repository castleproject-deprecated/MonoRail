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

namespace Castle.MonoRail.OData

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Data.Services.Common
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection


    [<AllowNullLiteral>]
    type EntitySetConfig(entitySetName:string, entityName:string, source, targetType:Type) = 
        let _entMapAttrs : List<EntityPropertyMappingAttribute> = List()
        let _customPropInfo = Dictionary<PropertyInfo, PropConfigurator>()
        let mutable _entityName = entityName
        member x.TargetType = targetType
        member x.EntitySetName : string = entitySetName
        member x.EntityName with get() = _entityName and set(v) = _entityName <- v
        member x.Source : IQueryable = source
        member internal x.EntityPropertyAttributes : List<EntityPropertyMappingAttribute> = _entMapAttrs
        member internal x.CustomPropConfig = _customPropInfo

    and [<AbstractClass>]
        PropConfigurator(mappedType:Type) = 
        class
            abstract member GetValue : instance:obj * source:obj -> obj
            abstract member SetValue : instance:obj * value:obj -> unit
            member x.MappedType = mappedType
        end

    and TypedPropConfigurator<'TSource,'TTarget>
                             (getter:Func<'TSource, 'TTarget>, 
                              setter:Func<'TTarget, 'TSource>) = 
        class
            inherit PropConfigurator(typeof<'TTarget>)

            override x.GetValue(instance, source) = 
                null

            override x.SetValue(instance, value) = 
                ()
        end

    and EntitySetConfigurator<'a>(entitySetName, entityName, source:IQueryable<'a>) = 
        inherit EntitySetConfig(entitySetName, entityName, source, typeof<'a>)
        
        member x.TypedSource = source

        member x.AddAttribute( att:EntityPropertyMappingAttribute ) = 
            x.EntityPropertyAttributes.Add att
            x

        member x.WithEntityName(name) = 
            x.EntityName <- name
            x

        member x.ForProperty<'TSource,'TTarget>(propSelector:Expression<Func<'a, 'TSource>>, 
                                                getter:Func<'TSource, 'TTarget>, 
                                                setter:Func<'TTarget, 'TSource>) = 
            let propInfo = RefHelpers.lastpropinfo_from_exp(propSelector)
            if propInfo = null then raise(ArgumentException())
            let config = TypedPropConfigurator(getter, setter)
            x.CustomPropConfig.[propInfo] <- config
            x

