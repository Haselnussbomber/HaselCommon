using System.Diagnostics.CodeAnalysis;
using Lumina.Data;

namespace HaselCommon.Extensions.Dalamud;

public static class IDataManagerExtensions
{
    extension(IDataManager dataManager)
    {
        public bool TryGetFile<T>(string path, [NotNullWhen(returnValue: true)] out T? file, out Exception? exception) where T : FileResource
        {
            try
            {
                file = dataManager.GetFile<T>(path);
                exception = null;
                return file != null;
            }
            catch (Exception ex)
            {
                exception = ex;
                file = null;
                return false;
            }
        }

        public bool TryGetFile<T>(string path, [NotNullWhen(returnValue: true)] out T? file) where T : FileResource
        {
            var result = TryGetFile(dataManager, path, out file, out var exception);

            if (!result && ServiceLocator.TryGetService<ILogger<IDataManager>>(out var logger) && logger.IsEnabled(LogLevel.Error))
                logger.LogError(exception, "Unexpected exception while getting file {path}", path);

            return result;
        }
    }
}
