

module Assertions 

    open System

    let internal ArgNotNullOrEmpty(s:string) = 
        if (s = null || String.Empty = s) then
            raise (ArgumentNullException())


    let internal ArgNotNull(obj) = 
        if (obj = null) then 
            raise (ArgumentNullException())

    let internal NotImplemented() = 
        raise (NotImplementedException())
