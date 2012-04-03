namespace Castle.MonoRail.Extension.OData

open System.Data.OData
open System.Text.RegularExpressions

module Constants =
    begin
        let SegmentKeyRegex = Regex(@"^([a-zA-Z0-9]+)\((\d+)\)$", RegexOptions.Singleline ||| RegexOptions.CultureInvariant ||| RegexOptions.Compiled)
    end