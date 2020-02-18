#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Multipart
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    public interface IMultipart : IDisposable
    {
        Task CopyToAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
#endif