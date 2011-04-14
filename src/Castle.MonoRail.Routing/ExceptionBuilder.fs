module ExceptionBuilder
    
    open System

    let internal ArgumentNull name = 
        sprintf "The argument %s is required. It cannot be null or empty" name

    let internal UnexpectedNull name = 
        sprintf "Looks like something went very wrong. We expected to have a value for '%s' but it's actually null" name

    let internal NotImplemented =
        "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

    let internal UnexpectedToken name = 
        sprintf "Error parsing the route matching expression. We hit an unexpected token '%s' which indicates the expression is probably wrong. If it's not, its a bug" name

    let internal UnexpectedEndTokenStream = 
        "Unexpected end of token stream - I don't think the route path is well-formed"

    let internal RaiseNotImplemented() = 
        raise (NotImplementedException(NotImplemented))
