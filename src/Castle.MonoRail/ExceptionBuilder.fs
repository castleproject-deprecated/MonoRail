
module internal ExceptionBuilder
    
    open System

    let private NotImplemented = 
        "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

    let internal RaiseNotImplemented() = 
        raise (NotImplementedException(NotImplemented))

