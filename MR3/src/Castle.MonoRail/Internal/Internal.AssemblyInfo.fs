
module AssemblyLevelDeclarations

open System.Reflection
open System.Security
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyVersion("0.0.1.0")>]
[<assembly: AssemblyFileVersion("0.0.1.0")>]
[<assembly: InternalsVisibleToAttribute("Castle.MonoRail.Tests") >]

[<assembly: AllowPartiallyTrustedCallers()>]
[<assembly: SecurityRules(SecurityRuleSet.Level2)>]
// http://msdn.microsoft.com/en-us/library/dd233102.aspx
//[<assembly: SecurityCritical()>] // means we're transparent but may expose critical api

do()