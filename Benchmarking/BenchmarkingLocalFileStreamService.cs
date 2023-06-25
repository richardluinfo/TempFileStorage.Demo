using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Microsoft.Extensions.DependencyInjection;
using StorageLib;

namespace Benchmarking;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[Config(typeof(AntiVirusFriendlyConfig))]
public class BenchmarkingLocalFileStreamService
{
    private const long NumberOfBytes = 4095 * 10000;
    private const int Delay = 500;
    private IServiceProvider _serviceProvider;
    
    [GlobalSetup]
    public void GlobalSetup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(serviceCollection);
        containerBuilder.RegisterType<LocalFileStreamService>()
            .As<ILocalFileStreamService>()
            .InstancePerDependency();

        IContainer container = containerBuilder.Build();
        _serviceProvider = new AutofacServiceProvider(container);
    }
    
    [Benchmark]
    public async Task BenchmarkGetTempFileStream()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);

        string fileName = await streamService.SaveAsTempFile(bytes, default);

        await Task.Delay(Delay);
        
        await using Stream stream = await streamService.GetTempFileStream(fileName, default);
	   
        StreamReader reader = new(stream);
	   
        _ = await reader.ReadToEndAsync();
    }
    
    [Benchmark]
    public async Task BenchmarkNoTempFileStream()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);
        
        await Task.Delay(Delay);
        
        await using Stream stream = await streamService.NoTempFileStream(bytes, default);
        stream.Seek(0, SeekOrigin.Begin);
        StreamReader reader = new(stream);
	
        _ = await reader.ReadToEndAsync();
    }
    
    [GlobalCleanup]
    public void GlobalCleanup()
    {
        //Write your cleanup logic here
    }

    private static Task<byte[]> GetRandomBytes(long size)
    {
        Random rdm = new();
        byte[] bytes = new byte[size];
        rdm.NextBytes(bytes);
        return Task.FromResult(bytes);
    }
}

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(Job.MediumRun
            .WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}