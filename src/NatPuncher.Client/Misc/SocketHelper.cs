using System.Net.Sockets;

namespace NatPuncher.Client.Misc;

public static class SocketHelper
{
    public static Socket CreateSocketWithAddressReusing()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        return socket;
    }
}