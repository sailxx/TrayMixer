using System.IO.Pipes;
using Avalonia.Threading;

namespace TrayMixer.Linux;

public static class Instance
{
    static string PipeName => "traymixer-" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Environment.UserName)))[..16];
    public static void ActivateExisting()
    {
        try { using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out); client.Connect(2500); client.WriteByte(1); }
        catch (Exception ex) when (ex is IOException or TimeoutException) { Console.Error.WriteLine("TrayMixer is already running: " + ex.Message); }
    }
    public static async Task Listen(Action activate, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await server.WaitForConnectionAsync(cancellation);
                var buffer = new byte[1];
                if (await server.ReadAsync(buffer, cancellation) > 0) Dispatcher.UIThread.Post(activate);
            }
            catch (OperationCanceledException) { return; }
            catch (IOException) { await Task.Delay(200, cancellation); }
        }
    }
}
