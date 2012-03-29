
namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq
    open Castle.MonoRail.OData
    open Castle.MonoRail.Extension.OData

    /// Access point to entities to be exposed by a single odata endpoint
    type ODataModel() = 
        class
            let mutable _schemaNs : string = null
            let _entities = List<EntitySetConfig>()

            let _resourcesets  = lazy ( ResourceMetadataBuilder.build(_schemaNs, _entities) 
                                        |> Seq.map (fun rt -> ResourceSet(rt.Name, rt) ) )
            let _resourcetypes = lazy ( _resourcesets.Force() |> Seq.map (fun (e:ResourceSet) -> e.ResourceType) )

            member x.SchemaNamespace with get() = _schemaNs and set(v) = _schemaNs <- v

            member x.EntitySet<'a>(entityName:string, source:IQueryable<'a>) = 
                if _resourcesets.IsValueCreated then raise(InvalidOperationException("Model is frozen since ResourceSets were built"))
                let cfg = EntitySetConfigurator(entityName, source)
                _entities.Add cfg
                cfg

            member x.Entities : EntitySetConfig seq = upcast _entities 

            member internal x.ResourceSets  = _resourcesets.Force()
            member internal x.ResourceTypes = _resourcetypes.Force()

            member internal x.GetResourceType(name) = 
                x.ResourceTypes 
                |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )

        end

    
        


