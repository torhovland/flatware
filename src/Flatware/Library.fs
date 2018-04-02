namespace Flatware

open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection

type FlatwareContainer<'model>(mdl : 'model) =
    member val mdl : 'model = (mdl : 'model) with get, set

[<Extension>]
type ServiceCollectionExtensions =
    [<Extension>]
    static member AddFlatware<'model>(_this : IServiceCollection, initialMdl : 'model) =
        let f = FlatwareContainer<'model>(initialMdl)
        _this.Add(ServiceDescriptor.Singleton<FlatwareContainer<'model>>(f));
        printfn "Flatware configured"
