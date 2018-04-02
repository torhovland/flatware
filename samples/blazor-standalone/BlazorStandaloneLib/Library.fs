namespace BlazorStandaloneLib

open System
open System.Net.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Blazor
open Microsoft.AspNetCore.Blazor.Components
open FSharp.Control.Tasks
open Flatware

type Msg =
    | Increment of n : int
    | LoadWeather

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

type Mdl = { count : int; forecasts : WeatherForecast list } with
    static member init = { count = 0; forecasts = [] }

type MyAppComponent() =
    inherit FlatwareComponent<Msg, Mdl>()

    [<Inject>]
    member val http = Unchecked.defaultof<HttpClient> with get, set

    override this.ReduceAsync(msg : Msg, mdl : Mdl) =
        task {
            match msg with
                | Increment n -> 
                    printfn "Loading weather ..."
                    let! forecasts = this.http.GetJsonAsync<WeatherForecast[]>("/sample-data/weather.json") |> Async.AwaitTask
                    printfn "Done loading weather."
                    return { mdl with count = mdl.count + n; forecasts = Array.toList forecasts }
                | LoadWeather -> 
                    let! forecasts = this.http.GetJsonAsync<WeatherForecast list>("/sample-data/weather.json") |> Async.AwaitTask
                    return { mdl with forecasts = forecasts }
        }