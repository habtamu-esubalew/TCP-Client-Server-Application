using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpServer
{
    private  static TcpListener? server;
    private static CancellationTokenSource? cancellationTokenSource;

    public static void Main()
    {
        int port = 5000;
        server = new TcpListener(IPAddress.Any, port);
        cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        server.Start();
        Console.WriteLine("Server started on port: " + port);

        // Listen for incoming client connections in a separate thread
        Thread listenerThread = new (() => ListenForClients(cancellationToken));
        listenerThread.Start();

        // Wait for the server to be stopped
        Console.WriteLine("Press Enter to stop the server.");
        Console.ReadLine();
        StopServer();
    }

    private static void ListenForClients(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Block until a client connects to the server
                if (server!.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Client connected successfuly.");

                    // Handle the client connection in a separate thread
                    Thread clientThread = new(new ParameterizedThreadStart(HandleClient!));
                    clientThread.Start(client);
                }
                else
                {
                    // Sleep briefly to avoid 100% CPU usage while waiting for clients
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while accepting new client: " + ex.Message);
            }
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient tcpClient = (TcpClient)obj;
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string convertedMessage=message!.ToUpper();  
                Console.WriteLine("Received command: " + convertedMessage);

                // Process the command and send the appropriate response
                string response = ProcessCommand(convertedMessage);
                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Sent response: " + response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while handling client: " + ex.Message);
        }
        finally
        {
            // Close the connection when done
            tcpClient.Close();
            Console.WriteLine("Client Disconnected.");
        }
    }

    private static string ProcessCommand(string command)
    {
        return command.Trim() switch
        {
            "GET_TEMP" => "Temperature: 25.3 °C",
            "GET_HUMIDITY" => "Humidity: 29.6%",
            "GET_WIND" => "Wind: 17 km/h",
            "GET_STATUS" => "Status: Active",
            _ => "Unknown command.",
        };
    }

    private static void StopServer()
    {
        //isRunning = false;
        cancellationTokenSource!.Cancel(); // Cancel the started operation
        server!.Stop(); // Stop the server from accepting new clients
        Console.WriteLine("Server stopped.");
    }
}

