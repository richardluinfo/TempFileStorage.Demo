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
[ReturnValueValidator(failOnError: true)]
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
    public async Task<bool> GetTempFileAsBytes()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);

        string fileName = await streamService.SaveAsTempFile(bytes, default);

        await Task.Delay(Delay);

        byte[]? result = await streamService.GetTempFileAsBytes(fileName, default);

        return result?.Length > 1_000;
    }
    
    [Benchmark]
    public async Task<bool> GetTempFileAsString()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);

        string fileName = await streamService.SaveAsTempFile(bytes, default);

        await Task.Delay(Delay);

        string? result = await streamService.GetTempFileAsString(fileName, default);

        return result?.Length > 1_000;
    }
    
    [Benchmark]
    public async Task<bool> GetTempFileAsStream()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);

        string fileName = await streamService.SaveAsTempFile(bytes, default);

        await Task.Delay(Delay);

        await using Stream result = streamService.GetTempFileAsStream(fileName, default);

        return result.Length > 1_000;
    }
    
    [Benchmark]
    public async Task<bool> NoTempFileStream()
    {
        var streamService = _serviceProvider.GetRequiredService<ILocalFileStreamService>();
        byte[] bytes = await GetRandomBytes(NumberOfBytes);
        
        await Task.Delay(Delay);
        
        await using Stream stream = await streamService.NoTempFileStream(bytes, default);
        
        byte[] result = new byte[stream.Length];

        _ = await stream.ReadAsync(result.AsMemory(0, (int)stream.Length), default);

        return result.Length > 1_000;
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