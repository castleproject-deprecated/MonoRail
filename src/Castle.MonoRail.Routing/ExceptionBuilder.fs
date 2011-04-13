module ExceptionBuilder

    let ArgumentNull name = 
        sprintf "The argument %s is required. It cannot be null or empty" name

    let UnexpectedNull name = 
        sprintf "Looks like something went very wrong. We expected to have a value for '%s' but it's actually null" name

    let NotImplemented =
        "You tried to use a functionality that has not been implemented. Looks like a great opportunity to contribute!"

    let UnexpectedToken name = 
        sprintf "Error parsing the route matching expression. We hit an unexpected token '%s' which indicates the expression is probably wrong. If it's not, its a bug" name

    let UnexpectedEndTokenStream = 
        "Unexpected end of token stream - I don't think the route path is well-formed"