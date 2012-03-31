namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq
    open System.Text
    open System.Web
    open Castle.MonoRail.OData
    open Castle.MonoRail.Extension.OData


    /// Entry point for exposing EntitySets through OData
    [<AbstractClass>]
    type ODataController<'T when 'T :> ODataModel>(model:'T) =  
        
        member x.Model = model
        member internal x.MetadataProvider = model :> IDataServiceMetadataProvider

        member x.Process(GreedyMatch:string, response:HttpResponseBase, request:HttpRequestBase) = 
            
            let qs = request.Url.Query

            let segments = SegmentParser.parse (GreedyMatch, qs, model)

            // let settings = ODataWriterSettings( BaseUri = Uri("http://localhost/something"), Version = ODataVersion.V2 )
            let writer = response.Output
            MetadataSerializer.serialize(writer, DataServiceMetadataProviderWrapper(x.MetadataProvider), Encoding.UTF8)

            // serializer.GenerateMetadata(MetadataEdmSchemaVersion.Version2Dot0);

            EmptyResult.Instance
