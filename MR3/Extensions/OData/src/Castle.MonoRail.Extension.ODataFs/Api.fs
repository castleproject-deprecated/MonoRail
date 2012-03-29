
namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq

    /// Access point to entities to be exposed by a single odata endpoint
    type ODataModel() = 
        class
            let mutable _schemaNs : string = null
            let _entities = List<EntitySetConfig>()
            let _resourcesets  = lazy ( _entities |> Seq.map (fun e -> e.ResourceSet) )
            let _resourcetypes = lazy ( _resourcesets.Force() |> Seq.map (fun (e:ResourceSet) -> e.ResourceType) )

            member x.SchemaNamespace with get() = _schemaNs and set(v) = _schemaNs <- v

            member x.EntitySet(entityName:string, source) = 
                let cfg = EntitySetConfig(entityName, source)
                _entities.Add cfg
                cfg

            member internal x.ResourceSets  = _resourcesets.Force()
            member internal x.ResourceTypes = _resourcetypes.Force()

            member internal x.GetResourceType(name) = 
                x.ResourceSets 
                |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )

        end

    and EntitySetConfig(entityName, source:IQueryable<_>) = 
        
        let mutable _resSet : ResourceSet = null

        member x.EntityName : string = entityName
        member x.Source = source
        member internal x.ResourceSet with get() = _resSet and set(v) = _resSet <- v 