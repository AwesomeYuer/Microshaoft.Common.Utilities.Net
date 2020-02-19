#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    internal static class StreamExtensions
    {
        public static async Task<string> ReadAsStringAsync(this Stream stream,
            CancellationToken cancellationToken = default)
        {
            using (var tr = new StreamReader(stream))
            {
                return await tr.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
#endif