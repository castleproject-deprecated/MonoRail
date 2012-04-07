namespace Castle.MonoRail.Extension.OData

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Xml
open System.IO
open System.Text
open System.ServiceModel.Syndication
open System.Data.OData
open System.Data.Services.Providers

module AtomSerialization = 
    begin
        let internal emptyPerson = SyndicationPerson(null, String.Empty, null)

        // xml:base="http://services.odata.org/OData/OData.svc/" 
        // xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices" 
        // xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" 
        
        let internal build_item (ob) (baseUri:Uri) (rt:ResourceType) = 
            (*<entry>
                <id>http://services.odata.org/OData/OData.svc/Products(0)</id>
                <title type="text">Bread</title>
                <summary type="text">Whole grain bread</summary>
                <updated>2012-04-07T09:59:19Z</updated>
                <author>
                  <name />
                </author>
                <link rel="edit" title="Product" href="Products(0)" />
                <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Category" type="application/atom+xml;type=entry" title="Category" href="Products(0)/Category" />
                <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Supplier" type="application/atom+xml;type=entry" title="Supplier" href="Products(0)/Supplier" />
                <category term="[namespace].Product" scheme="http://schemas.microsoft.com/ado/2007/08/dataservices/scheme" />
                <content type="application/xml">
                  <m:properties>
                    <d:ID m:type="Edm.Int32">0</d:ID>
                    <d:ReleaseDate m:type="Edm.DateTime">1992-01-01T00:00:00</d:ReleaseDate>
                    <d:DiscontinuedDate m:type="Edm.DateTime" m:null="true" />
                    <d:Rating m:type="Edm.Int32">4</d:Rating>
                    <d:Price m:type="Edm.Decimal">2.5</d:Price>
                  </m:properties>
                </content>
              </entry> *)
            let item = SyndicationItem()
            let resUri = Uri(rt.Name, UriKind.Relative)
            item.Title <- TextSyndicationContent(rt.Name)
            item.Id    <- Uri(baseUri, resUri).AbsoluteUri
            // item.Links.Add(SyndicationLink(Uri(rt.Name, UriKind.Relative), "self", rt.Name, null, 0L))
            item.Authors.Add emptyPerson

            item

        let internal write_items (baseUri:Uri) (rt:ResourceType) (items:IEnumerable) (writer:TextWriter) (enc:Encoding) = 

            let resUri = Uri(rt.Name, UriKind.Relative)

            let syndicationItems = 
                let lst = List<SyndicationItem>()
                for item in items do
                    lst.Add (build_item item baseUri rt)
                lst

            let feed = SyndicationFeed(syndicationItems)

            feed.Title <- TextSyndicationContent(rt.Name)
            feed.Id    <- Uri(baseUri, resUri).AbsoluteUri
            feed.Links.Add(SyndicationLink(Uri(rt.Name, UriKind.Relative), "self", rt.Name, null, 0L))

            

            let xmlWriter = SerializerCommons.create_xmlwriter writer enc



            feed.GetAtom10Formatter().WriteTo( xmlWriter )
            xmlWriter.Flush()


        let CreateDeserializer () = null

        let CreateSerializer () = 
            { new Serializer() with 
                  override x.SerializeMany(baseUri, rt, items, writer, enc) = 
                      write_items baseUri rt items writer enc
                      
            }

    end
