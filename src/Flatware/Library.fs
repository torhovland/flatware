namespace Flatware

open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Blazor.Components
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks

type FlatwareContainer<'msg, 'mdl>(mdl : 'mdl) =
    let mutable mdl = mdl
    let onChangeEvent = new Event<unit>()

    member this.Mdl 
        with get() = mdl 
        and private set(value) =
            mdl <- value
            onChangeEvent.Trigger()

    [<CLIEvent>]
    member this.OnChange = onChangeEvent.Publish

    member this.DispatchAsync(reducer : _ -> Task<'mdl>) =
        task {
            let! newMdl = reducer()
            this.Mdl <- newMdl
            printfn "Updated state."
        }

[<AbstractClass>]
type FlatwareComponent<'msg, 'mdl>() =
    inherit BlazorComponent()

    [<Inject>]
    member val F = Unchecked.defaultof<FlatwareContainer<'msg, 'mdl>> with get, set

    // Necessary due to FS0491
    member this.StateHasChanged() =
        base.StateHasChanged()

    member this.OnChangeHandler =
        Handler<unit>(fun _ _ -> this.StateHasChanged())

    override this.OnInit() =
        this.F.add_OnChange this.OnChangeHandler
        base.OnInit()
    
    member this.Dispose() =
        this.F.remove_OnChange this.OnChangeHandler

    abstract member ReduceAsync : 'msg * 'mdl -> Task<'mdl>

    member this.DispatchAsync(msg) =
        this.F.DispatchAsync(fun () -> this.ReduceAsync(msg, this.F.Mdl))

[<Extension>]
type ServiceCollectionExtensions =
    [<Extension>]
    static member AddFlatware<'msg, 'mdl>(configure : IServiceCollection, initialMdl) =
        let f = FlatwareContainer<'msg, 'mdl>(initialMdl)
        configure.Add(ServiceDescriptor.Singleton<FlatwareContainer<'msg, 'mdl>>(f));
        printfn "Flatware configured"
