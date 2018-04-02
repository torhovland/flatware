namespace BlazorStandaloneLib

open System
open Microsoft.AspNetCore.Blazor.Components
open Flatware

type Model = { count : int } with
    static member init = { count = 0 }

type Msg =
    | Increment

type FlatwareComponent() =
    inherit BlazorComponent()

    [<Inject>]
    member val f : FlatwareContainer<Model> = FlatwareContainer(Model.init) with get, set

    member this.Dispatch(msg : Msg) =
        let oldMdl = this.f.mdl
        let newMdl =
            match msg with
            | Increment -> 
                { oldMdl with count = oldMdl.count + 1 }
        this.f.mdl <- newMdl
        this.StateHasChanged()
