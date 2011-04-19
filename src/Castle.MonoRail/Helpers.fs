
module Helpers
    
    open System
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open Castle.MonoRail.Extensibility

    let private guard_load_types (asm:Assembly) =
        try
            asm.GetTypes()
        with
        | :? ReflectionTypeLoadException as exn -> 
            exn.Types

    // produces non-null seq of types
    let typesInAssembly (asm:Assembly) f =
        seq { 
                let types = guard_load_types(asm)
                for t in types do
                    if (t <> null && f(t)) then 
                        yield t
            }

    let order_lazy_set (set:IEnumerable<Lazy<'a, IComponentOrder>>) = 
        System.Linq.Enumerable.OrderBy(set, (fun e -> e.Metadata.Order)) :> IEnumerable<Lazy<'a, IComponentOrder>>


