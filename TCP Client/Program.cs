using System;
using System.Net.Sockets;
using System.Text;

class TcpClientApp
{
    public static void Main()
    {
        string serverIp = "127.0.0.1"; // Server IP address (localhost for demo purposes)
        int port = 5000;
        bool keepRunning = true; 

        while (keepRunning)
        {
            if (!IsServerRunning(serverIp, port))
            {
                Console.WriteLine("Server is not running. Please contact the admin.");
                Console.WriteLine("Type 'Exit' to QUIT.");
                string? command = Console.ReadLine();
                if (command?.ToLower() == "exit")
                {
                    keepRunning = false;
                }
                continue;
            }

            try
            {
                // Connect to the server
                using TcpClient client = new(serverIp, port);
                using NetworkStream stream = client.GetStream();

                Console.WriteLine("Connected to the server.");

                while (true)
                {
                    Console.WriteLine("Enter command (GET_TEMP, GET_HUMIDITY, GET_WIND, GET_STATUS) or type 'Exit' to QUIT:");
                    string? inputCommand = Console.ReadLine();
                    if (inputCommand?.ToLower() == "exit")
                    {
                        keepRunning = false; // Set flag to false to exit outer loop
                        break; // Exit the inner loop
                    }

                    // Send the command to the server
                    byte[] commandBytes = Encoding.ASCII.GetBytes(inputCommand!);
                    stream.Write(commandBytes, 0, commandBytes.Length);

                    // Read the server's response
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Server Response: " + response);
                }

                // Close the connection when done
                Console.WriteLine("Disconnected from the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        Console.WriteLine("Exiting...");
        Environment.Exit(0); // Ensures the app terminates immediately
    }

    // Method to check if the server is running
    private static bool IsServerRunning(string serverIp, int port)
    {
        try
        {
            using TcpClient client = new();
            client.Connect(serverIp, port);
            return true; // If connection succeeds, the server is running
        }
        catch
        {
            return false; // If connection fails, the server is not running
        }
    }
}
