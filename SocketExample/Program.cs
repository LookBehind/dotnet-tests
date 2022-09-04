// See https://aka.ms/new-console-template for more information

using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

#pragma warning disable CS4014

var validGetRequestFirstLine = new Memory<byte>(Encoding.ASCII.GetBytes("GET /status HTTP/1.1\r\n"));
var validGetResponse = new Memory<byte>(Encoding.ASCII.GetBytes(
    "HTTP/1.1 200\r\n" +
    "Content-Length: 0\r\n" +
    "\r\n"));
var invalidGetResponse = new Memory<byte>(Encoding.ASCII.GetBytes(
    "HTTP/1.1 500\r\n" +
    "Content-Length: 0\r\n" +
    "\r\n"));

var cts = new CancellationTokenSource();

Task.Factory.StartNew(async () =>
{
    var memPool = MemoryPool<byte>.Shared;

    var listener = new TcpListener(IPAddress.Loopback, 1111);
    listener.Start();
    while (true)
    {
        var socket = await listener.AcceptSocketAsync(cts.Token);
        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

        var requestBuffer = memPool.Rent(4096);

        var socketState = new Tuple<IMemoryOwner<byte>, Socket>(requestBuffer, socket);
        socket
            .ReceiveAsync(requestBuffer.Memory, SocketFlags.None, cts.Token)
            .AsTask()
            .ContinueWith(async (readTask, state) =>
            {
                var (buf, sock) = (state as Tuple<IMemoryOwner<byte>, Socket>)!;
                try
                {
                    var _ = await readTask;

                    if (buf.Memory[..validGetRequestFirstLine.Length].Span
                        .SequenceEqual(validGetRequestFirstLine.Span))
                    {
                        await sock.SendAsync(validGetResponse, SocketFlags.None, cts.Token);
                    }
                    else
                    {
                        await sock.SendAsync(invalidGetResponse, SocketFlags.None, cts.Token);
                    }
                }
                catch (SocketException se) when (se is {SocketErrorCode: SocketError.ConnectionReset})
                {
                    // ignored
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    buf!.Dispose();
                    sock.Dispose();
                }
            }, socketState, cts.Token);
    }
}, cts.Token);

Console.WriteLine($"Listening on socket {IPAddress.Loopback}:1111, hit any key to cancel");
Console.ReadKey();
cts.Cancel();
await Task.Delay(1000);