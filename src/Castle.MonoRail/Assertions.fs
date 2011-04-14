module Assertions

    open System

    let internal ArgNotNull (obj, name:string) = 
        if (obj = null) then 
            ExceptionBuilder.RaiseArgumentNull(name)


