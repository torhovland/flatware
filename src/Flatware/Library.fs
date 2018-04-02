namespace Flatware

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Blazor.Components
open Microsoft.Extensions.DependencyInjection

type FlatwareContainer<'msg, 'mdl>(mdl : 'mdl) =
    member val mdl : 'mdl = (mdl : 'mdl) with get, set

[<AbstractClass>]
type FlatwareComponent<'msg, 'mdl>() =
    inherit BlazorComponent()

    [<Inject>]
    member val f : FlatwareContainer<'msg, 'mdl> = Unchecked.defaultof<FlatwareContainer<'msg, 'mdl>> with get, set

    abstract member Reduce : 'msg * 'mdl -> 'mdl

    member this.Dispatch(msg : 'msg) =
        let oldMdl = this.f.mdl
        let newMdl = this.Reduce(msg, oldMdl)
        this.f.mdl <- newMdl
        this.StateHasChanged()

[<Extension>]
type ServiceCollectionExtensions =
    [<Extension>]
    static member AddFlatware<'msg, 'mdl>(_this : IServiceCollection, initialMdl : 'mdl) =
        let f = FlatwareContainer<'msg, 'mdl>(initialMdl)
        _this.Add(ServiceDescriptor.Singleton<FlatwareContainer<'msg, 'mdl>>(f));
        printfn "Flatware configured"
