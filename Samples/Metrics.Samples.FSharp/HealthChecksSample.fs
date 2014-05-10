namespace Metrics.Samples.FSharp

open Metrics
open Metrics.Core
open System

module HealthChecksSample =
    [<AllowNullLiteral>]
    type IDatabase =
        abstract member Ping : unit -> unit
    
    type DatabaseHealthCheck (database : IDatabase) as this =
        inherit HealthCheck ("DatabaseCheck")

        do
            HealthChecks.RegisterHealthCheck this

        override this.Check () : HealthCheckResult =
            // exceptions will be caught and 
            // the result will be unhealthy
            database.Ping ()
            HealthCheckResult.Healthy ()

    let CheckDbIsConnected () : unit = ()

    let GetFreeDiskSpace () : int = 100

    let RegisterHealthChecks () =
        ignore <| new DatabaseHealthCheck null

        HealthChecks.RegisterHealthCheck ("DatabaseConnected",
            fun () ->
                CheckDbIsConnected ()
                "Database Connection OK"
        )

        HealthChecks.RegisterHealthCheck ("DiskSpace",
            fun () ->
                let freeDiskSpace = GetFreeDiskSpace ()

                if (freeDiskSpace <= 512) then
                    HealthCheckResult.Unhealthy ("Not enough disk space: {0}", freeDiskSpace)
                else
                    HealthCheckResult.Unhealthy ("Disk space ok: {0}", freeDiskSpace)
        )

        let SampleOperation () =
            try
                try
                    raise (InvalidCastException "Sample error")
                with
                | x -> raise (FormatException ("Another Error", x));
            with
            | x -> raise (InvalidOperationException("operation went south", x))
            ()

        let AggregateSampleOperation () =
            try
                SampleOperation ()
            with
            | x1 ->
                try
                    SampleOperation()
                with
                | x2 ->
                    raise (AggregateException (seq { yield x1; yield x2 }))
            ()

        HealthChecks.RegisterHealthCheck ("SampleOperation", SampleOperation)
        HealthChecks.RegisterHealthCheck ("AggregateSampleOperation", AggregateSampleOperation)

        