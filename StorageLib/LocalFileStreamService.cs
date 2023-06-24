using Microsoft.Extensions.Logging;

namespace StorageLib;

public interface ILocalFileStreamService
{
    Task<Stream> GetTempFileStream(byte[] bytes, CancellationToken cancellationToken);
}

public class LocalFileStreamService : ILocalFileStreamService
{
    private readonly ILogger<LocalFileStreamService> _logger;

    public LocalFileStreamService(ILogger<LocalFileStreamService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> GetTempFileStream(byte[] bytes, CancellationToken cancellationToken)
    {
        string tempFileName = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(tempFileName, bytes, cancellationToken);

            Stream stream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.DeleteOnClose);

            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write/read temp files. FileName: {FileName}", tempFileName);
            
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            
            throw;
        }
    }
}