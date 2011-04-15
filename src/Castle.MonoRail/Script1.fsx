

// module Monads
    
open System.Reflection



type internal MaybeBuilder() =

    let doneA = 

    let bindA v f = 
        if v <> Unchecked.defaultof<_> then
            f(x)


    let rec whileA exp rest = 
        if (exp) then
            bindA rest, (fun () -> whileA exp rest)
        else 
            doneA

    let rec forA (s:seq<_>), f =
        whileA       


    // member this.While(exp, rest) 

    // seq<'a> * ('a -> M<'b>) -> M<'b>
    // member this.For(s:seq<_>, f) =
        
    member this.Bind(x, f) =
        match x with
        | Some(x) when x >= 0 && x <= 100 -> f(x)
        | _ -> None
    member this.Delay(f) = delay f
    member this.Return(x) = Some x
 

// let internal maybe = MaybeBuilder();;

