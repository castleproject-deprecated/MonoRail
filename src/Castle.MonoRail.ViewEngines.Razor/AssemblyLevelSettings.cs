using System.Security;
using System.Web;
using Castle.MonoRail.ViewEngines.Razor;

// default in v4.0
//[assembly: SecurityTransparent]
// allows safe critical and critical code. 
//[assembly: AllowPartiallyTrustedCallers]

[assembly: PreApplicationStartMethod(typeof(RazorViewEngine), "Initialize")]

