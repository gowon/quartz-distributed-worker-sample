# QuartzNode

## Design

Bootstrapping still follows the Generic Host pattern. By default, the application instantiates a single instance of an IScheduler, and attaches it's disposal to the application lifetime.

References:

- <https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/>

## Quartz

### Dependency Injection

`IJobs` do not need to be registered in Dependency Injection to be instantiated in Quartz.NET. When jobs are set as "durable", they are stored in the JobStore with the job type full name. The `MicrosoftDependencyInjectionJobFactory` makes use of the `ActivatorUtilities.CreateFactory` method to dynamically generate unregistered jobs by matching dependencies to the type's constructor. This method is used to hydrate Controllers in ASP.NET Core MVC.

This means that if the Job Type has been loaded into the worker's application context (eg. package dependency), then the worker will be able to execute that job. There is no ability to opt-in or opt-out of certain job types during container registration. If you wanted to prevent this behavior, then you would need to replace `MicrosoftDependencyInjectionJobFactory` with a custom implementation that removed the use of `ActivatorUtilities.CreateFactory`.

- <https://github.com/quartznet/quartznet/discussions/1303#discussioncomment-1345608>
- <https://digitteck.com/dotnet/csharp/net-core-serviceprovider-activatorutilities-objectfactory/>
- <https://onthedrift.com/posts/activator-utilities/>

### Metrics

To facilitate service elasticity, we would like to gauge the current job capacity of the scheduler in the instance; this can be determined by dividing the thread pool size by the current number of jobs in execution. `SchedulerMetricsListener` is hooked into the application host IScheduler instance to create that gauge metric when `IScheduler` has been configured with a thread pool size greater than 0 (instances set to Orchestrator Mode have their thread pool set to 0 automatically). Those metrics are then captured by the OpenTelemetry middleware, and exposed via the Prometheus endpoint (`/metrics`). This generates the `quartznode_active_job_load` metric:

```text
# HELP quartznode_active_job_load (percent) The percentage of total jobs in the thread pool currently in execution. (ObservableGauge`1)
# TYPE quartznode_active_job_load gauge
quartznode_active_job_load 0
```

The `quartznode_active_job_load` metric could be averaged across all worker nodes to determine if the set of nodes should be scaled up or down.

References:

- <https://www.thorsten-hans.com/instrumenting-dotnet-apps-with-opentelemetry/#custom-metrics>
- <https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-2/>
- <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation>

### Interrupting Jobs in a Clustered Environment

Use an MQ backplane to be able to emit a Terminate/Interrupt command, and have all the nodes subscribe to this channel. The node owning the job will be able to terminate the instance; no side effect for the other jobs. This could also be used for other things like pausing/resuming schedulers. These controls could be managed outside of the application with a Grafana Dashboard to combine the data from the Quartz Database (`qrtz_fired_triggers`), Prometheus (`quartznode_active_job_load`) and widgets to push commands to Rabbit MQ using REST API calls.

- <https://stackoverflow.com/a/68783974/7644876>
  - <https://funprojects.blog/2019/11/08/rabbitmq-rest-api/>
- <https://grafana.com/grafana/plugins/cloudspout-button-panel/>
  - <https://github.com/cloudspout/cloudspout-button-panel/issues/100#issuecomment-1644327411>
