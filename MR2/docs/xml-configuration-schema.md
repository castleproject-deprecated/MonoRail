# XML Configuration Schema

Below is an XML Schema Definition that defines the valid structure of the MonoRail configuration XML. A fully populated [sample XML configuration](xml-configuration-sample.md) file is also available.

```xml
<?xml version="1.0" encoding="utf-8"?>
<xs:schema targetNamespace="urn:castle-monorail-configuration-2.0" xmlns="urn:castle-monorail-configuration-2.0"

attributeFormDefault="unqualified" elementFormDefault="qualified" xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:element name="monorail">
    <xs:complexType>
      <xs:all>
        <xs:element name="controllers" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="assembly" type="xs:string" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="url" minOccurs="0">
          <xs:complexType>
            <xs:attribute name="useExtensions" type="xs:boolean" use="required" />
          </xs:complexType>
        </xs:element>
        <xs:element name="viewEngines" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="add">
                <xs:complexType>
                  <xs:attribute name="type" type="xs:string" use="required" />
                  <xs:attribute name="xhtml" type="xs:boolean" use="optional" />
                </xs:complexType>
              </xs:element>
            </xs:sequence>
            <xs:attribute name="viewPathRoot" type="xs:string" use="optional" />
          </xs:complexType>
        </xs:element>
        <xs:element name="viewcomponents" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="assembly" type="xs:string" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="extensions" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="extension">
                <xs:complexType>
                  <xs:attribute name="type" type="xs:string" use="required" />
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="scaffold" minOccurs="0">
          <xs:complexType>
            <xs:attribute name="type" use="required" type="xs:string"/>
          </xs:complexType>
        </xs:element>
        <xs:element name="services" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element name="service" minOccurs="0" maxOccurs="unbounded">
                <xs:complexType>
                  <xs:attribute name="id" type="serviceIdType" use="required" />
                  <xs:attribute name="type" type="xs:string" use="required" />
                  <xs:attribute name="interface" type="xs:string" use="optional" />
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="additionalSources" minOccurs="0">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="assembly">
                <xs:complexType>
                  <xs:attribute name="name" type="xs:string" use="required" />
                  <xs:attribute name="namespace" type="xs:string" use="required" />
                </xs:complexType>
              </xs:element>
              <xs:element minOccurs="0" maxOccurs="unbounded" name="path">
                <xs:complexType>
                  <xs:attribute name="location" type="xs:string" use="required" />
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:all>
      <xs:attribute name="smtpHost" type="xs:string" use="optional" />
      <xs:attribute name="smtpPort" type="xs:integer" use="optional" />
      <xs:attribute name="smtpUsername" type="xs:string" use="optional" />
      <xs:attribute name="smtpPassword" type="xs:string" use="optional" />
      <xs:attribute name="smtpUseSsl" type="xs:boolean" use="optional" />
    </xs:complexType>
  </xs:element>
  <xs:simpleType name="serviceIdType">
    <xs:restriction base="xs:string">
      <xs:enumeration value="Custom" />
      <xs:enumeration value="ControllerFactory" />
      <xs:enumeration value="ViewEngine" />
      <xs:enumeration value="ViewSourceLoader" />
      <xs:enumeration value="ViewComponentFactory" />
      <xs:enumeration value="FilterFactory" />
      <xs:enumeration value="ResourceFactory" />
      <xs:enumeration value="EmailSender" />
      <xs:enumeration value="ControllerDescriptorProvider" />
      <xs:enumeration value="ResourceDescriptorProvider" />
      <xs:enumeration value="RescueDescriptorProvider" />
      <xs:enumeration value="LayoutDescriptorProvider" />
      <xs:enumeration value="HelperDescriptorProvider" />
      <xs:enumeration value="FilterDescriptorProvider" />
      <xs:enumeration value="EmailTemplateService" />
      <xs:enumeration value="ControllerTree" />
      <xs:enumeration value="CacheProvider" />
      <xs:enumeration value="ScaffoldingSupport" />
      <xs:enumeration value="ExecutorFactory" />
      <xs:enumeration value="TransformFilterDescriptorProvider" />
      <xs:enumeration value="TransformationFilterFactory" />
      <xs:enumeration value="ViewEngineManager" />
      <xs:enumeration value="UrlBuilder" />
      <xs:enumeration value="UrlTokenizer" />
      <xs:enumeration value="ServerUtility" />
      <xs:enumeration value="ValidatorRegistry" />
      <xs:enumeration value="AjaxProxyGenerator" />
    </xs:restriction>
  </xs:simpleType>
</xs:schema>
```