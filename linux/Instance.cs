using System.IO.Pipes;
using Avalonia.Threading;

namespace TrayMixer.Linux;

public static class Instance
{
    static string PipeName => "traymixer-" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Environment.UserName)))[..16];
    public static void ActivateExisting()
    {
        try { using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly); client.Connect(2500); client.WriteByte(1); }
        catch (Exception ex) when (ex is IOException or TimeoutException or UnauthorizedAccessException) { Console.Error.WriteLine("TrayMixer is already running: " + ex.Message); }
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
                using var readDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                readDeadline.CancelAfter(TimeSpan.FromSeconds(2));
                if (await server.ReadAsync(buffer, readDeadline.Token) == 1 && buffer[0] == 1) Dispatcher.UIThread.Post(activate);
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                try { await Task.Delay(200, cancellation); }
                catch (OperationCanceledException) { return; }
            }
        }
    }
}
