namespace Flatware

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Blazor.Components
open Microsoft.Extensions.DependencyInjection

type FlatwareContainer<'msg, 'mdl>(mdl : 'mdl) =
    member val mdl = mdl with get, set

[<AbstractClass>]
type FlatwareComponent<'msg, 'mdl>() =
    inherit BlazorComponent()

    [<Inject>]
    member val f = Unchecked.defaultof<FlatwareContainer<'msg, 'mdl>> with get, set

    abstract member Reduce : 'msg * 'mdl -> 'mdl

    member this.Dispatch(msg) =
        this.f.mdl <- this.Reduce(msg, this.f.mdl)
        this.StateHasChanged()

[<Extension>]
type ServiceCollectionExtensions =
    [<Extension>]
    static member AddFlatware<'msg, 'mdl>(configure : IServiceCollection, initialMdl) =
        let f = FlatwareContainer<'msg, 'mdl>(initialMdl)
        configure.Add(ServiceDescriptor.Singleton<FlatwareContainer<'msg, 'mdl>>(f));
        printfn "Flatware configured"
