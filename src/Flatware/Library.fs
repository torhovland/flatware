namespace Flatware

open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Blazor.Components
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks

type FlatwareContainer<'msg, 'mdl>(mdl : 'mdl) =
    member val mdl = mdl with get, set

[<AbstractClass>]
type FlatwareComponent<'msg, 'mdl>() =
    inherit BlazorComponent()

    [<Inject>]
    member val f = Unchecked.defaultof<FlatwareContainer<'msg, 'mdl>> with get, set

    abstract member ReduceAsync : 'msg * 'mdl -> Task<'mdl>

    // Necessary due to FS0491
    member this.StateHasChanged() =
        base.StateHasChanged()
        
    member this.DispatchAsync(msg) =
        task {
            let! newMdl = this.ReduceAsync(msg, this.f.mdl)
            this.f.mdl <- newMdl
            this.StateHasChanged()
            printfn "Updated state."
        }

[<Extension>]
type ServiceCollectionExtensions =
    [<Extension>]
    static member AddFlatware<'msg, 'mdl>(configure : IServiceCollection, initialMdl) =
        let f = FlatwareContainer<'msg, 'mdl>(initialMdl)
        configure.Add(ServiceDescriptor.Singleton<FlatwareContainer<'msg, 'mdl>>(f));
        printfn "Flatware configured"
