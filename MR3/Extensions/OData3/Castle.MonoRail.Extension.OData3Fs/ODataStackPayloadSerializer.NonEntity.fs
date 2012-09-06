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



    // Property / EntityReferenceLink / EntityReferenceLinks / Collection
    type NonEntitySerializer() = 
        class 
        (* 
private readonly ODataMessageWriter writer;
    private ODataCollectionWriter collectionWriter;

    internal NonEntitySerializer(RequestDescription requestDescription, Uri absoluteServiceUri, IDataService service, ODataMessageWriter messageWriter)
      : base(requestDescription, absoluteServiceUri, service, (string) null)
    {
      this.writer = messageWriter;
    }

    internal override void Flush()
    {
      if (this.collectionWriter == null)
        return;
      this.collectionWriter.Flush();
    }

    protected override void WriteTopLevelElement(IExpandedResult expandedResult, object element)
    {
      string containerName = this.ComputeContainerName();
      if (this.RequestDescription.LinkUri)
      {
        bool needPop = this.PushSegmentForRoot();
        this.WriteLink(element);
        this.PopSegmentName(needPop);
      }
      else
      {
        ResourceType propertyResourceType = element != null ? (this.RequestDescription.TargetKind == RequestTargetKind.Collection ? this.RequestDescription.TargetResourceType : WebUtil.GetResourceType(this.Provider, element)) : (this.RequestDescription.TargetKind == RequestTargetKind.OpenProperty ? ResourceType.PrimitiveStringResourceType : this.RequestDescription.TargetResourceType);
        if (propertyResourceType == null)
          throw new InvalidOperationException(System.Data.Services.Strings.Serializer_UnsupportedTopLevelType((object) element.GetType()));
        this.writer.WriteProperty(new ODataProperty()
        {
          Name = containerName,
          Value = this.GetPropertyValue(containerName, propertyResourceType, element, false)
        });
      }
    }

    protected override void WriteTopLevelElements(IExpandedResult expanded, IEnumerator elements, bool hasMoved)
    {
      if (this.RequestDescription.LinkUri)
      {
        bool needPop = this.PushSegmentForRoot();
        this.WriteLinkCollection(elements, hasMoved);
        this.PopSegmentName(needPop);
      }
      else
      {
        this.collectionWriter = this.writer.CreateODataCollectionWriter();
        this.collectionWriter.WriteStart(new ODataCollectionStart()
        {
          Name = this.ComputeContainerName()
        });
        for (; hasMoved; hasMoved = elements.MoveNext())
        {
          object current = elements.Current;
          ResourceType propertyResourceType = current == null ? this.RequestDescription.TargetResourceType : WebUtil.GetResourceType(this.Provider, current);
          if (propertyResourceType == null)
            throw new InvalidOperationException(System.Data.Services.Strings.Serializer_UnsupportedTopLevelType((object) current.GetType()));
          this.collectionWriter.WriteItem(this.GetPropertyValue("element", propertyResourceType, current, false));
        }
        this.collectionWriter.WriteEnd();
        this.collectionWriter.Flush();
      }
    }

    private void WriteLink(object element)
    {
      this.IncrementSegmentResultCount();
      this.writer.WriteEntityReferenceLink(new ODataEntityReferenceLink()
      {
        Url = this.GetEntityUri(element)
      });
    }

    private void WriteLinkCollection(IEnumerator elements, bool hasMoved)
    {
      ODataEntityReferenceLinks entityReferenceLinks = new ODataEntityReferenceLinks();
      if (this.RequestDescription.CountOption == RequestQueryCountOption.Inline)
        entityReferenceLinks.Count = new long?(this.RequestDescription.CountValue);
      entityReferenceLinks.Links = this.GetLinksCollection(elements, hasMoved, entityReferenceLinks);
      this.writer.WriteEntityReferenceLinks(entityReferenceLinks);
    }

    private IEnumerable<ODataEntityReferenceLink> GetLinksCollection(IEnumerator elements, bool hasMoved, ODataEntityReferenceLinks linksCollection)
    {
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      object lastObject = (object) null;
      IExpandedResult lastExpandedSkipToken = (IExpandedResult) null;
      if (hasMoved)
      {
        object element = elements.Current;
        IExpandedResult skipToken = (IExpandedResult) null;
        if (element != null)
        {
          IExpandedResult expanded = element as IExpandedResult;
          if (expanded != null)
          {
            element = Serializer.GetExpandedElement(expanded);
            skipToken = this.GetSkipToken(expanded);
          }
        }
        this.IncrementSegmentResultCount();
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E2__current = new ODataEntityReferenceLink()
        {
          Url = this.GetEntityUri(element)
        };
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E1__state = 1;
        return true;
      }
      else
      {
        if (this.NeedNextPageLink(elements))
          linksCollection.NextPageLink = this.GetNextLinkUri(lastObject, lastExpandedSkipToken, this.RequestDescription.ResultUri);
        return false;
      }
    }

    private Uri GetEntityUri(object element)
    {
      ResourceType primitiveResourceType = WebUtil.GetNonPrimitiveResourceType(this.Provider, element);
      return Serializer.GetEditLink(element, primitiveResourceType, this.Provider, this.CurrentContainer, this.AbsoluteServiceUri);
    }

    private string ComputeContainerName()
    {
      if (DataServiceActionProviderWrapper.IsServiceActionSegment(this.RequestDescription.LastSegmentInfo))
      {
        bool nameIsContainerQualified;
        return this.Provider.GetNameFromContainerQualifiedName(this.RequestDescription.ContainerName, out nameIsContainerQualified);
      }
      else
        return this.RequestDescription.ContainerName;
    }        
        *)
        end
    