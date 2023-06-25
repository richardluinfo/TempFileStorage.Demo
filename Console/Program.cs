using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StorageLib;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Start...");

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();

var containerBuilder = new ContainerBuilder();
containerBuilder.Populate(serviceCollection);
containerBuilder.RegisterType<LocalFileStreamService>()
    .As<ILocalFileStreamService>()
    .InstancePerDependency();

IContainer container = containerBuilder.Build();
var serviceProvider = new AutofacServiceProvider(container);

var streamService = serviceProvider.GetRequiredService<ILocalFileStreamService>();
byte[] bytes = await GetRandomBytes(4096 * 100_000);
Log.Information("Creating data. Byte size: {ByteSize}", bytes.Length);

string fileName = await streamService.SaveAsTempFile(bytes, default);

Log.Information("Temp File created: {FileName}", fileName);

await Task.Delay(10_000);
        
await using Stream stream = streamService.GetTempFileAsStream(fileName, default);
	   
StreamReader reader = new(stream);
	   
string result = await reader.ReadToEndAsync();

Log.Information("Result retrieved. Byte size: {ByteSize}", result.Length);

Log.Information("Ending...");


static Task<byte[]> GetRandomBytes(long size)
{
    Random rdm = new();
    byte[] bytes = new byte[size];
    rdm.NextBytes(bytes);
    return Task.FromResult(bytes);
}