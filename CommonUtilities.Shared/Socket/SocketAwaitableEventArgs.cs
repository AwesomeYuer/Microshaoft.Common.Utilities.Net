#if NETCOREAPP2_X
namespace Microshaoft.Sockets
{
    using System;
    using System.IO.Pipelines;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public class SocketAwaitableEventArgs : SocketAsyncEventArgs, ICriticalNotifyCompletion
    {
        private static readonly Action _callbackCompleted = () => { };

        private readonly PipeScheduler _ioScheduler;

        private Action _callback;

        public SocketAwaitableEventArgs(PipeScheduler ioScheduler)
        {
            _ioScheduler = ioScheduler;
        }

        public SocketAwaitableEventArgs GetAwaiter() => this;
        public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);

        public int GetResult()
        {
            _callback = null;

            if (SocketError != SocketError.Success)
            {
                ThrowSocketException(SocketError);
            }

            return BytesTransferred;

            void ThrowSocketException(SocketError e)
            {
                throw new SocketException((int)e);
            }
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback, _callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
            {
                Task.Run(continuation);
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Complete()
        {
            OnCompleted(this);
        }

        protected override void OnCompleted(SocketAsyncEventArgs _)
        {
            var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);

            if (continuation != null)
            {
                _ioScheduler.Schedule(state => ((Action)state)(), continuation);
            }
        }
    }

    public class SocketReceiver : IDisposable
    {
        Socket _socket;
        SocketAwaitableEventArgs _awaitableEventArgs;

        public SocketReceiver(Socket socket, PipeScheduler scheduler)
        {
            _socket = socket;
            _awaitableEventArgs = new SocketAwaitableEventArgs(scheduler);
        }
        public SocketAwaitableEventArgs WaitForDataAsync()
        {
            _awaitableEventArgs.SetBuffer(Array.Empty<byte>(), 0, 0);

            if (!_socket.ReceiveAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }

        public SocketAwaitableEventArgs ReceiveAsync(Memory<byte> buffer)
        {
            _awaitableEventArgs.SetBuffer(buffer);

            if (!_socket.ReceiveAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }

        public void Dispose()
        {
            _awaitableEventArgs.Dispose();
        }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            var ipe = new IPEndPoint(IPAddress.Any, 5000);

            using (var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ipe);
                socket.Listen(512);

                Console.WriteLine("Waiting to accept");
                var acceptSocket = await socket.AcceptAsync();

                // On *nix platforms, Sockets already dispatches to the ThreadPool.
                // Yes, the IOQueues are still used for the PipeSchedulers. This is intentional.
                // https://github.com/aspnet/KestrelHttpServer/issues/2573
                var receiver = new SocketReceiver(acceptSocket, PipeScheduler.Inline);
                // Ensure we have some reasonable amount of buffer space
                var buffer = new Memory<byte>(new byte[2048]);

                while (true)
                {
                    Console.WriteLine("\n\nWaiting for data");

                    // Wait for data before allocating a buffer.
                    // REPRO: comment out the following two lines to observe the behaviour change on macOS
                    var waitedBytes = await receiver.WaitForDataAsync();
                    Console.WriteLine($"Waited for {waitedBytes} bytes of data");

                    var bytesReceived = await receiver.ReceiveAsync(buffer);
                    Console.WriteLine($"Received {bytesReceived} bytes of data");
                    Console.WriteLine(Encoding.ASCII.GetString(buffer.Span.Slice(0, bytesReceived)));
                }
            }
        }
    }
}
#endif