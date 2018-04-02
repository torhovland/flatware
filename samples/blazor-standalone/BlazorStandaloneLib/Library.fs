namespace BlazorStandaloneLib

open Flatware

type Msg =
    | Increment of n : int

type Mdl = { count : int } with
    static member init = { count = 0 }

type MyComponent() =
    inherit FlatwareComponent<Msg, Mdl>()

    override this.Reduce(msg : Msg, mdl : Mdl) =
        match msg with
            | Increment n -> 
                { mdl with count = mdl.count + n }
