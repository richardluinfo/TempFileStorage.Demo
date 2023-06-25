using Microsoft.Extensions.Logging;

namespace StorageLib;

public interface ILocalFileStreamService
{
    Task<string> SaveAsTempFile(byte[] bytes, CancellationToken cancellationToken);
    Task<byte[]?> GetTempFileAsBytes(string fileName, CancellationToken cancellationToken);
    Task<string?> GetTempFileAsString(string fileName, CancellationToken cancellationToken);
    Stream GetTempFileAsStream(string fileName, CancellationToken cancellationToken);
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

    public async Task<byte[]?> GetTempFileAsBytes(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                _logger.LogError("File {FileName} doesn't not exist", fileName);
                return default;
            }

            await using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.DeleteOnClose);
            
            byte[] bytes = new byte[stream.Length];

            _ = await stream.ReadAsync(bytes.AsMemory(0, (int)stream.Length), cancellationToken);

            return bytes;
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

    public async Task<string?> GetTempFileAsString(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                _logger.LogError("File {FileName} doesn't not exist", fileName);
                return default;
            }
            
            await using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.DeleteOnClose);

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using StreamReader reader = new(stream);

            return await reader.ReadToEndAsync(cancellationToken);
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

    public Stream GetTempFileAsStream(string fileName, CancellationToken cancellationToken)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogError("File {FileName} doesn't not exist", fileName);
            throw new FileNotFoundException();
        }
            
        FileStream stream = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.DeleteOnClose);

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return stream;
    }

    public async Task<Stream> NoTempFileStream(byte[] bytes, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var stream = new MemoryStream(bytes);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}