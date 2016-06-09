namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net.Sockets;
    public static class SocketHelper
    {
        public static int DestorySocket(Socket socket)
        {
            int r = -1;
            try
            {
                //if (_socket.Connected)
                //{
                //    _socket.Disconnect(false);

                //}
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.DisconnectAsync(null);
                    
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
                r = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                r = -1;
            }
            return r;
        }
    }
}
