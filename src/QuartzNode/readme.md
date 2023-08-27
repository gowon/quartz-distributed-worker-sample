# QuartzNode

## Notes

### Design

Bootstrapping still follows the Generic Host pattern.

A single worker node application owns a single 

### Quartz

`IJobs` do not need to be registered in Dependency Injection to be instantiated in Quartz.NET. When jobs are set as "durable", they are stored in the JobStore with the job type full name. The `MicrosoftDependencyInjectionJobFactory` makes use of the `ActivatorUtilities.CreateFactory` method to dynamically generate unregistered jobs by matching dependencies to the type's constructor. This method is used to hydrate Controllers in ASP.NET Core MVC. They are effectively transient services.

- <https://github.com/quartznet/quartznet/discussions/1303#discussioncomment-1345608>
- <https://digitteck.com/dotnet/csharp/net-core-serviceprovider-activatorutilities-objectfactory/>
- <https://onthedrift.com/posts/activator-utilities/>

## References

- <https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/>

