module Assertions 

    open System
    open ExceptionBuilder

    let internal ArgNotNullOrEmpty(s:string, name:string) = 
        if (s = null || String.Empty = s) then
            raise (ArgumentNullException(ArgumentNull(name)))

    let internal ArgNotNull (obj, name:string) = 
        if (obj = null) then 
            raise (ArgumentNullException(ArgumentNull(name)))

    let internal NotImplemented() = 
        raise (NotImplementedException(NotImplemented))

    let internal IsNotNull (obj, name:string) = 
        if (obj = null) then 
            raise (InvalidOperationException(UnexpectedNull(name)))