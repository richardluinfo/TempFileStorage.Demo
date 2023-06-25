using Microsoft.Extensions.Logging;

namespace StorageLib;

public interface ILocalFileStreamService
{
    Task<string> SaveAsTempFile(byte[] bytes, CancellationToken cancellationToken);
    Task<Stream> GetTempFileStream(string fileName, CancellationToken cancellationToken);
    Task<Stream> NoTempFileStream(byte[] bytes, CancellationToken cancellationToken);
}

public class LocalFileStreamService : ILocalFileStreamService
{
    private readonly ILogger<LocalFileStreamService> _logger;

    public LocalFileStreamService(ILogger<LocalFileStreamService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveAsTempFile(byte[] bytes, CancellationToken cancellationToken)
    {
        string tempFileName = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(tempFileName, bytes, cancellationToken);
            return tempFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write temp file. FileName: {FileName}", tempFileName);
            
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            
            throw;
        }
    }

    public async Task<Stream> GetTempFileStream(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.DeleteOnClose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read temp file. FileName: {FileName}", fileName);
            
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            
            throw;
        }
    }

    public async Task<Stream> NoTempFileStream(byte[] bytes, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return new MemoryStream(bytes);
    }
}