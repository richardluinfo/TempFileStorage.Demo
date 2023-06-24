using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StorageLib;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();

var containerBuilder = new ContainerBuilder();
containerBuilder.Populate(serviceCollection);
containerBuilder.RegisterType<LocalFileStreamService>().As<ILocalFileStreamService>();

var container = containerBuilder.Build();
var serviceProvider = new AutofacServiceProvider(container);



ILocalFileStreamService streamService = serviceProvider.GetRequiredService<ILocalFileStreamService>();

Log.Information("Starting...");

byte[] bytes = await GetRandomBytes(4095 * 100);

Log.Information("Byte Size: {ByteSize}", bytes.Length);


await using Stream stream = await streamService.GetTempFileStream(bytes, default);
	
await Task.Delay(5000);
	
StreamReader reader = new(stream);
	
string result = await reader.ReadToEndAsync();
	
Log.Information("Result: {Result}", result);

Log.Information("Ending...");
await Log.CloseAndFlushAsync();


static Task<byte[]> GetRandomBytes(long size)
{
    Random rdm = new();
    byte[] bytes = new byte[size];
    rdm.NextBytes(bytes);
    return Task.FromResult(bytes);
}
