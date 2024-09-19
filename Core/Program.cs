namespace Core;

using Microsoft.Extensions.Hosting;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        builder.Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://localhost:6666");
            });
}

/* todo:

1 - add a function that the intervieww will read and explain what it does in plain words
2 - add one more controller with other endpoints
3 - come up with issues that the interviewee will have to solve such as:
    a - null reference
    b - not protect non-existing path
    c - race condition
    d - inefficiency
    e - deadlock
    f - passing more data via socket than possible (see Startup)
    g - not handling exceptions

*/