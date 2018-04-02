namespace BlazorStandaloneLib

open Flatware

type Msg =
    | Increment

type Mdl = { count : int } with
    static member init = { count = 0 }

type MyComponent() =
    inherit FlatwareComponent<Msg, Mdl>()

    override this.Reduce(msg : Msg, mdl : Mdl) =
        match msg with
            | Increment -> 
                { mdl with count = mdl.count + 1 }
