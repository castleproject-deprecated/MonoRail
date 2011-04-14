
module ExceptionBuilder
    
    open System

    let private NotImplemented = 
        "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

    let internal RaiseNotImplemented() = 
        raise (NotImplementedException(NotImplemented))

    let internal RaiseArgumentNull name = 
        let msg = sprintf "The argument %s is required. It cannot be null or empty" name
        raise (ArgumentNullException(msg))
        
