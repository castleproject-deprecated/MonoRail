module File1

// need to process segments from an endpoint
// Example localhost/vpath/odata.svc/Products(1)/Categories

// http://odata.research.microsoft.com/FAQ.aspx
// http://services.odata.org/%28S%28zjtwckq5iumy0qno2wbf413y%29%29/OData/OData.svc/


// http://odata.netflix.com/v2/Catalog/
// http://odata.netflix.com/v2/Catalog/$metadata
// http://odata.netflix.com/v2/Catalog/Movies


// http://vancouverdataservice.cloudapp.net/v1/

(*
   enum RequestTargetKind
  {
    Nothing,
    ServiceDirectory,           localhost/vpath/odata.svc/
    Resource,
    ComplexObject,
    Primitive,
    PrimitiveValue,
    Metadata,                   localhost/vpath/odata.svc/$metadata
    VoidServiceOperation,
    Batch,
    Link,
    OpenProperty,
    OpenPropertyValue,
    MediaResource,
  }



        if (other == null)
        {
          segment2 = RequestUriProcessor.CreateFirstSegment(service, identifier, checkRights, str, segments.Length == 1, out crossReferencingUrl);
        }
        else
        {
          if (other.TargetKind == RequestTargetKind.Batch || other.TargetKind == RequestTargetKind.Metadata || (other.TargetKind == RequestTargetKind.PrimitiveValue || other.TargetKind == RequestTargetKind.VoidServiceOperation) || (other.TargetKind == RequestTargetKind.OpenPropertyValue || other.TargetKind == RequestTargetKind.MediaResource))
            throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_MustBeLeafSegment((object) other.Identifier));
          if (isAfterLink && identifier != "$count")
            throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_CannotSpecifyAfterPostLinkSegment((object) identifier, (object) "$links"));
          if (other.TargetKind == RequestTargetKind.Primitive)
          {
            if (identifier != "$value")
              throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_ValueSegmentAfterScalarPropertySegment((object) other.Identifier, (object) identifier));
            System.Data.Services.WebUtil.CheckSyntaxValid(!segmentIdentifier);
            segment2 = new SegmentInfo(other);
            segment2.Identifier = identifier;
            segment2.SingleResult = true;
            segment2.TargetKind = RequestTargetKind.PrimitiveValue;
          }
          else if (other.TargetKind == RequestTargetKind.Resource && other.SingleResult && identifier == "$links")
          {
            segment2 = new SegmentInfo(other);
            segment2.Identifier = identifier;
            segment2.TargetKind = RequestTargetKind.Link;
          }
          else
          {
            isAfterLink = other.TargetKind == RequestTargetKind.Link;
            if (other.Operation != null && (other.Operation.ResultKind == ServiceOperationResultKind.Enumeration || other.Operation.ResultKind == ServiceOperationResultKind.DirectValue))
              throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_IEnumerableServiceOperationsCannotBeFurtherComposed((object) other.Identifier));
            if (!other.SingleResult && identifier != "$count")
              throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_CannotQueryCollections((object) other.Identifier));
            segment2 = new SegmentInfo();
            segment2.Identifier = identifier;
            segment2.TargetSource = RequestTargetSource.Property;
            segment2.ProjectedProperty = other.TargetResourceType != null ? other.TargetResourceType.TryResolvePropertyName(identifier) : (ResourceProperty) null;
            if (identifier == "$count")
            {
              if (other.TargetKind != RequestTargetKind.Resource)
                throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CountNotSupported((object) other.Identifier));
              if (other.SingleResult)
                throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CannotQuerySingletons((object) other.Identifier, (object) identifier));
              if (service.OperationContext.Host.AstoriaHttpVerb != AstoriaVerbs.GET)
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_RequestVerbCannotCountError);
              segment2.RequestEnumerable = other.RequestEnumerable;
              segment2.SingleResult = true;
              segment2.TargetKind = RequestTargetKind.PrimitiveValue;
              segment2.TargetResourceType = other.TargetResourceType;
              segment2.TargetContainer = other.TargetContainer;
            }
            else if (identifier == "$value" && (other.TargetKind == RequestTargetKind.OpenProperty || other.TargetKind == RequestTargetKind.Resource))
            {
              segment2.RequestEnumerable = other.RequestEnumerable;
              segment2.SingleResult = true;
              segment2.TargetResourceType = other.TargetResourceType;
              if (other.TargetKind == RequestTargetKind.OpenProperty)
              {
                segment2.TargetKind = RequestTargetKind.OpenPropertyValue;
              }
              else
              {
                segment2.TargetKind = RequestTargetKind.MediaResource;
                RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
              }
            }
            else if (segment2.ProjectedProperty == null)
            {
              if (other.TargetResourceType != null)
                System.Data.Services.WebUtil.CheckResourceExists(other.TargetResourceType.IsOpenType, segment2.Identifier);
              if (other.TargetKind == RequestTargetKind.Link || segmentIdentifier || service.OperationContext.Host.AstoriaHttpVerb == AstoriaVerbs.POST)
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes((object) segment2.Identifier));
              segment2.TargetResourceType = (ResourceType) null;
              segment2.TargetKind = RequestTargetKind.OpenProperty;
              segment2.SingleResult = true;
              if (!crossReferencingUrl)
                segment2.RequestQueryable = RequestUriProcessor.SelectOpenProperty(other.RequestQueryable, identifier);
            }
            else
            {
              segment2.TargetResourceType = segment2.ProjectedProperty.ResourceType;
              ResourcePropertyKind kind = segment2.ProjectedProperty.Kind;
              segment2.SingleResult = kind != ResourcePropertyKind.ResourceSetReference;
              if (!crossReferencingUrl)
                segment2.RequestQueryable = !segment2.ProjectedProperty.CanReflectOnInstanceTypeProperty ? (segment2.SingleResult ? RequestUriProcessor.SelectLateBoundProperty(other.RequestQueryable, segment2.ProjectedProperty) : RequestUriProcessor.SelectLateBoundPropertyMultiple(other.RequestQueryable, segment2.ProjectedProperty)) : (segment2.SingleResult ? RequestUriProcessor.SelectElement(other.RequestQueryable, segment2.ProjectedProperty) : RequestUriProcessor.SelectMultiple(other.RequestQueryable, segment2.ProjectedProperty));
              if (other.TargetKind == RequestTargetKind.Link && segment2.ProjectedProperty.TypeKind != ResourceTypeKind.EntityType)
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_LinkSegmentMustBeFollowedByEntitySegment((object) identifier, (object) "$links"));
              switch (kind)
              {
                case ResourcePropertyKind.ComplexType:
                  segment2.TargetKind = RequestTargetKind.ComplexObject;
                  break;
                case ResourcePropertyKind.ResourceReference:
                case ResourcePropertyKind.ResourceSetReference:
                  segment2.TargetKind = RequestTargetKind.Resource;
                  segment2.TargetContainer = service.Provider.GetContainer(other.TargetContainer, other.TargetResourceType, segment2.ProjectedProperty);
                  if (segment2.TargetContainer == null)
                    throw DataServiceException.CreateResourceNotFound(segment2.ProjectedProperty.Name);
                  else
                    break;
                default:
                  segment2.TargetKind = RequestTargetKind.Primitive;
                  break;
              }
              if (segmentIdentifier)
              {
                System.Data.Services.WebUtil.CheckSyntaxValid(!segment2.SingleResult);
                if (crossReferencingUrl)
                  throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ResourceCanBeCrossReferencedOnlyForBindOperation);
                RequestUriProcessor.ComposeQuery(str, segment2);
              }
              if (segment2.TargetContainer != null)
              {
                if (checkRights)
                  DataServiceConfiguration.CheckResourceRightsForRead(segment2.TargetContainer, segment2.SingleResult);
                if (!crossReferencingUrl && RequestUriProcessor.ShouldRequestQuery(service, index == segments.Length - 1, isAfterLink, str))
                  segment2.RequestQueryable = DataServiceConfiguration.ComposeResourceContainer(service, segment2.TargetContainer, segment2.RequestQueryable);
              }
            }
          }
        }
        segmentInfoArray[index] = segment2;
        other = segment2;
      }
      if (segments.Length != 0 && other.TargetKind == RequestTargetKind.Link)
        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_MissingSegmentAfterLink((object) "$links"));
      else
        return segmentInfoArray;
    }



    private static SegmentInfo CreateFirstSegment(IDataService service, string identifier, bool checkRights, string queryPortion, bool isLastSegment, out bool crossReferencingUrl)
    {
      crossReferencingUrl = false;
      SegmentInfo segment = new SegmentInfo();
      segment.Identifier = identifier;
      if (segment.Identifier == "$metadata")
      {
        System.Data.Services.WebUtil.CheckSyntaxValid(queryPortion == null);
        segment.TargetKind = RequestTargetKind.Metadata;
        return segment;
      }
      else if (segment.Identifier == "$batch")
      {
        System.Data.Services.WebUtil.CheckSyntaxValid(queryPortion == null);
        segment.TargetKind = RequestTargetKind.Batch;
        return segment;
      }
      else
      {
        if (segment.Identifier == "$count")
          throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CountOnRoot);
        segment.Operation = service.Provider.TryResolveServiceOperation(segment.Identifier);
        if (segment.Operation != null)
        {
          segment.TargetSource = RequestTargetSource.ServiceOperation;
          if (service.OperationContext.RequestMethod != segment.Operation.Method)
            throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.RequestUriProcessor_MethodNotAllowed, segment.Operation.Method);
          segment.TargetContainer = segment.Operation.ResourceSet;
          segment.TargetResourceType = segment.Operation.ResultKind == ServiceOperationResultKind.Void ? (ResourceType) null : segment.Operation.ResultType;
          segment.OperationParameters = RequestUriProcessor.ReadOperationParameters(service.OperationContext.Host, segment.Operation);
          switch (segment.Operation.ResultKind)
          {
            case ServiceOperationResultKind.DirectValue:
            case ServiceOperationResultKind.Enumeration:
              object obj;
              try
              {
                obj = service.Provider.InvokeServiceOperation(segment.Operation, segment.OperationParameters);
              }
              catch (TargetInvocationException ex)
              {
                ErrorHandler.HandleTargetInvocationException(ex);
                throw;
              }
              segment.SingleResult = segment.Operation.ResultKind == ServiceOperationResultKind.DirectValue;
              System.Data.Services.WebUtil.CheckResourceExists(segment.SingleResult || obj != null, segment.Identifier);
              SegmentInfo segmentInfo = segment;
              IEnumerable enumerable;
              if (!segment.SingleResult)
                enumerable = (IEnumerable) obj;
              else
                enumerable = (IEnumerable) new object[1]
                {
                  obj
                };
              segmentInfo.RequestEnumerable = enumerable;
              segment.TargetResourceType = segment.Operation.ResultType;
              segment.TargetKind = RequestUriProcessor.TargetKindFromType(segment.TargetResourceType);
              System.Data.Services.WebUtil.CheckSyntaxValid(queryPortion == null);
              RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
              break;
            case ServiceOperationResultKind.QueryWithMultipleResults:
            case ServiceOperationResultKind.QueryWithSingleResult:
              try
              {
                segment.RequestQueryable = (IQueryable) service.Provider.InvokeServiceOperation(segment.Operation, segment.OperationParameters);
              }
              catch (TargetInvocationException ex)
              {
                ErrorHandler.HandleTargetInvocationException(ex);
                throw;
              }
              System.Data.Services.WebUtil.CheckResourceExists(segment.RequestQueryable != null, segment.Identifier);
              segment.SingleResult = segment.Operation.ResultKind == ServiceOperationResultKind.QueryWithSingleResult;
              break;
            default:
              segment.TargetKind = RequestTargetKind.VoidServiceOperation;
              break;
          }
          if (segment.RequestQueryable != null)
          {
            segment.TargetKind = RequestUriProcessor.TargetKindFromType(segment.TargetResourceType);
            if (queryPortion != null)
            {
              System.Data.Services.WebUtil.CheckSyntaxValid(!segment.SingleResult);
              RequestUriProcessor.ComposeQuery(queryPortion, segment);
            }
          }
          if (checkRights)
            DataServiceConfiguration.CheckServiceRights(segment.Operation, segment.SingleResult);
          return segment;
        }
        else
        {
          SegmentInfo segmentForContentId = service.GetSegmentForContentId(segment.Identifier);
          if (segmentForContentId != null)
          {
            segmentForContentId.Identifier = segment.Identifier;
            crossReferencingUrl = true;
            return segmentForContentId;
          }
          else
          {
            ResourceSetWrapper resourceSetWrapper = service.Provider.TryResolveResourceSet(segment.Identifier);
            System.Data.Services.WebUtil.CheckResourceExists(resourceSetWrapper != null, segment.Identifier);
            if (RequestUriProcessor.ShouldRequestQuery(service, isLastSegment, false, queryPortion))
            {
              segment.RequestQueryable = service.Provider.GetQueryRootForResourceSet(resourceSetWrapper);
              System.Data.Services.WebUtil.CheckResourceExists(segment.RequestQueryable != null, segment.Identifier);
            }
            segment.TargetContainer = resourceSetWrapper;
            segment.TargetResourceType = resourceSetWrapper.ResourceType;
            segment.TargetSource = RequestTargetSource.EntitySet;
            segment.TargetKind = RequestTargetKind.Resource;
            segment.SingleResult = false;
            if (queryPortion != null)
              RequestUriProcessor.ComposeQuery(queryPortion, segment);
            if (checkRights)
              DataServiceConfiguration.CheckResourceRightsForRead(resourceSetWrapper, segment.SingleResult);
            if (segment.RequestQueryable != null)
              segment.RequestQueryable = DataServiceConfiguration.ComposeResourceContainer(service, resourceSetWrapper, segment.RequestQueryable);
            return segment;
          }
        }
      }
    }
*)