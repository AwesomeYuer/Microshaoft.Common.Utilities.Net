#if NETCOREAPP2_X
namespace Microshaoft
{
    using Newtonsoft.Json;
    using System.Buffers;
    using System.IO;
    public static partial class JsonHelper
    {
        public static JsonTextReader CreateJsonTextReader(this TextReader target)
        {
            var reader = new JsonTextReader(target);
            reader.ArrayPool = JsonArrayPool<char>.Shared;

            // Don't close the input, leave closing to the caller
            reader.CloseInput = false;

            return reader;
        }

        public static JsonTextWriter CreateJsonTextWriter(this TextWriter target)
        {
            var writer = new JsonTextWriter(target);
            writer.ArrayPool = JsonArrayPool<char>.Shared;
            // Don't close the output, leave closing to the caller
            writer.CloseOutput = false;

            // SignalR will always write a complete JSON response
            // This setting will prevent an error during writing be hidden by another error writing on dispose
            writer.AutoCompleteOnClose = false;

            return writer;
        }


        private class JsonArrayPool<T> : IArrayPool<T>
        {
            private readonly ArrayPool<T> _inner;

            internal static readonly JsonArrayPool<T> Shared = new JsonArrayPool<T>(ArrayPool<T>.Shared);

            public JsonArrayPool(ArrayPool<T> inner)
            {
                _inner = inner;
            }

            public T[] Rent(int minimumLength)
            {
                return _inner.Rent(minimumLength);
            }

            public void Return(T[] array)
            {
                _inner.Return(array);
            }
        }
    }
}
#endif