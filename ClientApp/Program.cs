using System;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        TcpClient client = new TcpClient();
        client.Connect("127.0.0.1", 5000); // Aynı bilgisayarda test için

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Gelen mesaj: " + message);

        stream.Close();
        client.Close();
    }
}

