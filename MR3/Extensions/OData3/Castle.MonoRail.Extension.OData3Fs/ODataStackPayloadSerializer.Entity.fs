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



    // Feed/Entry
    type EntitySerializer(odataWriter:ODataWriter) = 
        class 

            (* 
            WriteTopLevelElements
            string title = this.RequestDescription.TargetKind == RequestTargetKind.OpenProperty || this.RequestDescription.TargetSource != RequestTargetSource.Property ? this.RequestDescription.ContainerName : this.RequestDescription.Property.Name;
            this.odataWriter = this.messageWriter.CreateODataFeedWriter();
            bool needPop = this.PushSegmentForRoot();
            this.WriteFeedElements(expanded, elements, WebUtil.ElementType(this.RequestDescription.TargetResourceType), title, new Uri(this.RequestDescription.LastSegmentInfo.Identifier, UriKind.Relative), this.RequestDescription.ResultUri, hasMoved, true);
            this.PopSegmentName(needPop);            
            *)
            (* 
            WriteTopLevelElement
            ResourceType expectedType = this.RequestDescription.TargetSource == RequestTargetSource.EntitySet || this.RequestDescription.TargetSource == RequestTargetSource.ServiceOperation ? this.RequestDescription.TargetResourceType : this.RequestDescription.Property.ResourceType;
            bool needPop = this.PushSegmentForRoot();
            this.WriteEntry(expanded, element, false, expectedType);
            this.PopSegmentName(needPop);            
            *)

        let link_operations() = 
(* 
    private void PopulateODataOperations(object resourceInstance, bool resourceInstanceInFeed, ODataEntry entry, ResourceType actualResourceType)
    {
      ResourceType resourceType = this.CurrentContainer.ResourceType;
      IEnumerable<OperationWrapper> operationProjections = this.GetOperationProjections();
      IEnumerable<OperationWrapper> source = operationProjections != null ? Enumerable.Where<OperationWrapper>(operationProjections, (Func<OperationWrapper, bool>) (o => o.BindingParameter.ParameterType.IsAssignableFrom(actualResourceType))) : this.Service.ActionProvider.GetServiceActionsByBindingParameterType(this.Service.OperationContext, actualResourceType);
      if (!Enumerable.Any<OperationWrapper>(source))
        return;
      List<ODataAction> list = new List<ODataAction>();
      string containerName = this.Service.Provider.ContainerName;
      foreach (OperationWrapper serviceAction in source)
      {
        string segmentByResourceType = serviceAction.GetActionTitleSegmentByResourceType(actualResourceType, containerName);
        ResourceType parameterType = serviceAction.BindingParameter.ParameterType;
        string text = parameterType == resourceType || !resourceType.IsAssignableFrom(parameterType) ? segmentByResourceType : serviceAction.BindingParameter.ParameterType.FullName + "/" + segmentByResourceType;
        Uri uri1 = new Uri(entry.Id, UriKind.RelativeOrAbsolute);
        Uri operationMetadata = this.GetODataOperationMetadata(this.AbsoluteServiceUri, serviceAction.Name);
        Uri uri2 = RequestUriProcessor.AppendUnescapedSegment(uri1, text);
        ODataAction odataAction = new ODataAction();
        odataAction.Title = serviceAction.Name;
        odataAction.Metadata = operationMetadata;
        odataAction.Target = uri2;
        ODataAction actionToSerialize = odataAction;
        if (serviceAction.OperationParameterBindingKind == OperationParameterBindingKind.Always)
          list.Add(actionToSerialize);
        else if (this.Service.ActionProvider.AdvertiseServiceAction(this.Service.OperationContext, serviceAction, resourceInstance, resourceInstanceInFeed, ref actionToSerialize))
        {
          if (actionToSerialize == null)
            throw new DataServiceException(500, System.Data.Services.Strings.DataServiceActionProviderWrapper_AdvertiseServiceActionCannotReturnNullActionToSerialize);
          list.Add(actionToSerialize);
        }
      }
      if (!Enumerable.Any<ODataAction>((IEnumerable<ODataAction>) list))
        return;
      entry.Actions = (IEnumerable<ODataAction>) list;
    }
*)
            ()

        let getProperties() = 
    (* 
    private void WriteNavigationProperties(IExpandedResult expanded, object customObject, bool resourceInstanceInFeed, ResourceType currentResourceType, Uri absoluteUri, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
    {
      foreach (ResourceProperty property in projectionNodesForCurrentResourceType == null ? Enumerable.Where<ResourceProperty>(this.Provider.GetResourceSerializableProperties(this.CurrentContainer, currentResourceType), (Func<ResourceProperty, bool>) (p => p.TypeKind == ResourceTypeKind.EntityType)) : Enumerable.Select<ProjectionNode, ResourceProperty>(Enumerable.Where<ProjectionNode>(projectionNodesForCurrentResourceType, (Func<ProjectionNode, bool>) (p =>
      {
        if (p.Property != null)
          return p.Property.TypeKind == ResourceTypeKind.EntityType;
        else
          return false;
      })), (Func<ProjectionNode, ResourceProperty>) (p1 => p1.Property)))
      {
        Serializer.ResourcePropertyInfo navigationPropertyInfo = this.GetNavigationPropertyInfo(expanded, customObject, currentResourceType, property);
        ODataNavigationLink navigationLink = new ODataNavigationLink();
        navigationLink.Name = navigationPropertyInfo.Property.Name;
        navigationLink.IsCollection = new bool?(navigationPropertyInfo.Property.Kind == ResourcePropertyKind.ResourceSetReference);
        navigationLink.Url = Serializer.AppendEntryToUri(relativeUri, navigationLink.Name);
        this.odataWriter.WriteStart(navigationLink);
        if (navigationPropertyInfo.Expand)
        {
          object obj1 = navigationPropertyInfo.Value;
          IExpandedResult expanded1 = obj1 as IExpandedResult;
          object obj2 = expanded1 != null ? Serializer.GetExpandedElement(expanded1) : obj1;
          bool needPop = this.PushSegmentForProperty(navigationPropertyInfo.Property, currentResourceType, navigationPropertyInfo.ExpandedNode);
          if (this.CurrentContainer != null)
          {
            if (navigationPropertyInfo.Property.Kind == ResourcePropertyKind.ResourceSetReference)
            {
              IEnumerable enumerable;
              WebUtil.IsElementIEnumerable(obj2, out enumerable);
              IEnumerator enumerator = enumerable.GetEnumerator();
              try
              {
                bool hasMoved = enumerator.MoveNext();
                Uri absoluteUri1 = Serializer.AppendEntryToUri(absoluteUri, navigationLink.Name);
                this.WriteFeedElements(expanded1, enumerator, navigationPropertyInfo.Property.ResourceType, navigationLink.Name, navigationLink.Url, absoluteUri1, hasMoved, false);
              }
              catch
              {
                WebUtil.Dispose((object) enumerator);
                throw;
              }
            }
            else if (WebUtil.IsNullValue(obj2))
            {
              this.odataWriter.WriteStart((ODataEntry) null);
              this.odataWriter.WriteEnd();
            }
            else
              this.WriteEntry(expanded1, obj2, resourceInstanceInFeed, navigationPropertyInfo.Property.ResourceType);
          }
          this.PopSegmentName(needPop);
        }
        this.odataWriter.WriteEnd();
      }
    }

    private IEnumerable<ODataProperty> GetEntityProperties(object customObject, ResourceType currentResourceType, Uri relativeUri, IEnumerable<ProjectionNode> projectionNodesForCurrentResourceType)
    {
      this.RecurseEnter();
      try
      {
        if (projectionNodesForCurrentResourceType == null)
          return this.GetAllEntityProperties(customObject, currentResourceType, relativeUri);
        else
          return this.GetProjectedEntityProperties(customObject, currentResourceType, relativeUri, projectionNodesForCurrentResourceType);
      }
      finally
      {
        this.RecurseLeave();
      }
    }  
    
    private IEnumerable<ODataProperty> GetAllEntityProperties(object customObject, ResourceType currentResourceType, Uri relativeUri)
    {
      List<ODataProperty> list = new List<ODataProperty>(currentResourceType.Properties.Count);
      foreach (ResourceProperty property in this.Provider.GetResourceSerializableProperties(this.CurrentContainer, currentResourceType))
      {
        if (property.TypeKind != ResourceTypeKind.EntityType)
          list.Add(this.GetODataPropertyForEntityProperty(customObject, currentResourceType, relativeUri, property));
      }
      if (currentResourceType.IsOpenType)
      {
        foreach (KeyValuePair<string, object> keyValuePair in this.Provider.GetOpenPropertyValues(customObject))
        {
          string key = keyValuePair.Key;
          if (string.IsNullOrEmpty(key))
            throw new DataServiceException(500, System.Data.Services.Strings.Syndication_InvalidOpenPropertyName((object) currentResourceType.FullName));
          list.Add(this.GetODataPropertyForOpenProperty(key, keyValuePair.Value));
        }
        if (currentResourceType.HasEntityPropertyMappings && this.contentFormat != ODataFormat.VerboseJson)
        {
          HashSet<string> propertiesLookup = new HashSet<string>(Enumerable.Select<ODataProperty, string>((IEnumerable<ODataProperty>) list, (Func<ODataProperty, string>) (p => p.Name)));
          foreach (EpmSourcePathSegment sourcePathSegment in Enumerable.Where<EpmSourcePathSegment>((IEnumerable<EpmSourcePathSegment>) currentResourceType.EpmSourceTree.Root.SubProperties, (Func<EpmSourcePathSegment, bool>) (p => !propertiesLookup.Contains(p.PropertyName))))
            list.Add(this.GetODataPropertyForOpenProperty(sourcePathSegment.PropertyName, this.Provider.GetOpenPropertyValue(customObject, sourcePathSegment.PropertyName)));
        }
      }
      return (IEnumerable<ODataProperty>) list;
    }      
    *)  
            ()

        let write_feed_items () = 
           (* 
      ODataFeed feed = new ODataFeed();
      feed.Id = absoluteUri.AbsoluteUri;
      AtomFeedMetadata annotation = new AtomFeedMetadata();
      feed.SetAnnotation<AtomFeedMetadata>(annotation);
      annotation.Title = new AtomTextConstruct()
      {
        Text = title
      };
      annotation.SelfLink = new AtomLinkMetadata()
      {
        Href = relativeUri,
        Title = title
      };
      bool flag = false;
      if (topLevel && this.RequestDescription.CountOption == RequestQueryCountOption.Inline)
      {
        flag = this.contentFormat == ODataFormat.VerboseJson;
        if (!flag)
          feed.Count = new long?(this.RequestDescription.CountValue);
      }
      this.odataWriter.WriteStart(feed);
      try
      {
        object lastObject = (object) null;
        IExpandedResult skipTokenExpandedResult = (IExpandedResult) null;
        while (hasMoved)
        {
          object element = elements.Current;
          IExpandedResult skipToken = this.GetSkipToken(expanded);
          if (element != null)
          {
            IExpandedResult expandedResult = element as IExpandedResult;
            if (expandedResult != null)
            {
              expanded = expandedResult;
              element = Serializer.GetExpandedElement(expanded);
              skipToken = this.GetSkipToken(expanded);
            }
            this.WriteEntry(expanded, element, true, expectedType);
          }
          hasMoved = elements.MoveNext();
          lastObject = element;
          skipTokenExpandedResult = skipToken;
        }
        if (flag)
          feed.Count = new long?(this.RequestDescription.CountValue);
        if (this.NeedNextPageLink(elements))
          feed.NextPageLink = this.GetNextLinkUri(lastObject, skipTokenExpandedResult, absoluteUri);
      }
      finally
      {
        if (!topLevel)
          WebUtil.Dispose((object) elements);
      }
      this.odataWriter.WriteEnd();           
           *)
           ()

        let write_entry (element:obj) (edmType:IEdmType) = 

            let entry = ODataEntry()
            let annotation = AtomEntryMetadata()
            entry.SetAnnotation(annotation);

            // Uri id;
            // Uri idAndEditLink = Serializer.GetIdAndEditLink(element, actualResourceType, this.Provider, this.CurrentContainer, this.AbsoluteServiceUri, out id);
            // Uri relativeUri = new Uri(idAndEditLink.AbsoluteUri.Substring(this.AbsoluteServiceUri.AbsoluteUri.Length), UriKind.Relative);

            let name = edmType.FName

            entry.TypeName  <- name // actualResourceType.FullName
            entry.Id        <- "testing"  // id.AbsoluteUri
            // entry.EditLink  <- relativeUri
            annotation.EditLink <- AtomLinkMetadata(Title = name);

            // let etagValue = GetETagValue(element, actualResourceType)
            // entry.ETag = etagValue
            // PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType)

            odataWriter.WriteStart(entry)
            // WriteNavigationProperties(expanded, element, resourceInstanceInFeed, actualResourceType, idAndEditLink, relativeUri, enumerable);
            // Properties = this.GetEntityProperties(element, actualResourceType, relativeUri, enumerable);
            odataWriter.WriteEnd()
            odataWriter.Flush()

        (* 
          IEnumerable<ProjectionNode> enumerable = this.GetProjections();
          if (enumerable != null)
          {
            enumerable = Enumerable.Where(enumerable, (projectionNode => projectionNode.TargetResourceType.IsAssignableFrom(actualResourceType)));
            entry.SetAnnotation(new ProjectedPropertiesAnnotation(Enumerable.Select(enumerable, (p => p.PropertyName))));
          }
          entry.AssociationLinks = this.GetEntityAssociationLinks(actualResourceType, relativeUri, enumerable);
          this.PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType);
          this.odataWriter.WriteStart(entry);
          this.WriteNavigationProperties(expanded, element, resourceInstanceInFeed, actualResourceType, idAndEditLink, relativeUri, enumerable);
          entry.Properties = this.GetEntityProperties(element, actualResourceType, relativeUri, enumerable);
          this.odataWriter.WriteEnd();
        *)
            ()
        end

        member x.WriteEntry (element, elType) = 
            write_entry element elType