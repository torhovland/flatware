# Flatware for Blazor

Flatware is a state management library for [Blazor](https://github.com/aspnet/Blazor), similar to [Elm](http://elm-lang.org/) and [Redux](https://redux.js.org/). It has the following features:

- Implements a one-way model-update-view architecture, by many considered to be [more robust and easier to reason about](https://www.exclamationlabs.com/blog/the-case-for-unidirectional-data-flow/) than a two-way data binding as found in Angular. 
- Application state is kept in a single state store. This opens up for advanced features such as [undo/redo](https://github.com/elm-community/undo-redo), [hydration of application state](https://github.com/rt2zz/redux-persist), [time-traveling debuggers](http://debug.elm-lang.org/), [isomorphic apps](https://hackernoon.com/isomorphic-universal-boilerplate-react-redux-server-rendering-tutorial-example-webpack-compenent-6e22106ae285) with shared .NET code on the frontend and backend, etc.
- Any Blazor component that is upgraded to a Flatware component will subscribe to changes in the state store and automatically update its view, so you don't have to worry about calling `StateHasChanged()`.
- The view engine is Razor, just like Blazor without Flatware. This combines the power of a templating engine with the familiarity of HTML, like [JSX](https://reactjs.org/docs/introducing-jsx.html), and is much less alien than the view languages used in Elm and [Fable](http://fable.io/). The Blazor pages themselves become very simple, with just presentational content, references to state in the model, and dispatching of application messages.
- Flatware uses F#, which means you write your model, your application messages and your reducer logic in F#. While this will doubtlessly put off some C# developers, F# has some really useful language features. The [discriminated union types](https://fsharpforfunandprofit.com/posts/discriminated-unions/) are perfect for designing type-safe application messages, and [the `with` keyword in record types](https://fsharpforfunandprofit.com/posts/records/) makes it simple to work with immutable types in your reducer logic. Not to mention that a model with many small types can be created with much less ceremony. F# lends itself well to [type driven development](https://fsharpforfunandprofit.com/series/designing-with-types.html). The Blazor project itself and the Razor pages must be C#.

## Getting started

1. Assuming you have Visual Studio 15.7 or newer and the Blazor tooling installed, create a new standalone Blazor project.

2. Add a .NET Standard F# class library to the solution.

3. Add a reference from the Blazor project to the F# project.

4. Add the Flatware NuGet package to both projects.

5. In `Library.fs`, add your message type, model types, and your component base class with the reducer logic:

```fsharp
open System
open System.Net.Http
open Microsoft.AspNetCore.Blazor
open Microsoft.AspNetCore.Blazor.Components
open FSharp.Control.Tasks
open Flatware

type MyMsg =
    | Increment of n : int
    | LoadWeather

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

type MyMdl = { Count : int; Forecasts : WeatherForecast list } with
    static member Init = { Count = 0; Forecasts = [] }

type MyAppComponent() =
    inherit FlatwareComponent<MyMsg, MyMdl>()

    [<Inject>]
    member val Http = null : HttpClient with get, set

    override this.ReduceAsync(msg : MyMsg, mdl : MyMdl) =
        task {
            match msg with
                | Increment n -> 
                    return { mdl with Count = mdl.Count + n }
                | LoadWeather -> 
                    let! forecasts = this.Http.GetJsonAsync<WeatherForecast[]>("/sample-data/weather.json") |> Async.AwaitTask
                    return { mdl with Forecasts = Array.toList forecasts }
        }
```

6. Open `Program.cs` and configure Flatware in the `BrowserServiceProvider`:

```csharp
configure.AddFlatware<MyMsg, MyMdl>(MyMdl.Init);
```

You will need to add

```csharp
using Flatware;
using ClassLibrary1;
```

at the top, assuming your F# library was called `ClassLibrary1`.

7. The architecture is now ready for use in your Blazor pages. Open `Counter.cshtml`. Remove the entire `@functions` block. Change the header to:

```
@page "/counter"
@inherits MyAppComponent
@using ClassLibrary1
```

Replace

```
<p>Current count: @currentCount</p>
```

with 

```
<p>Current count: @Mdl.Count</p>
```

Also replace

```
<button @onclick(IncrementCount)>Click me</button>
```

with

```
<button @onclick(() => DispatchAsync(MyMsg.NewIncrement(3)))>Click me</button>
```

8. In `FetchData.cshtml`, do the same change to the header and remove the `@functions` block.

Replace both occurrences of `forecasts` with `Mdl.Forecasts`.

9. All that remains is that the application message `LoadWeather` needs to be dispatched from somewhere. It could be from a button, or it could be from an `OnInitAsync()` method in the Blazor page. But it seems more natural to load the weather data when the application starts, which is why we'll change `App.cshtml` to the following:

```
@inherits MyAppComponent
@using ClassLibrary1
<!--
    Configuring this here is temporary. Later we'll move the app config
    into Program.cs, and it won't be necessary to specify AppAssembly.
-->
<Router AppAssembly=typeof(Program).Assembly />

@functions
{
    protected override async Task OnInitAsync()
    {
        await this.DispatchAsync(MyMsg.LoadWeather);
    }
}
```

## Contributing

Flatware is definitely experimental at the moment, and you should expect breaking changes. But I'd be very interested in discussing the design and potential features. Please open an issue if you have any particular topic in mind.
