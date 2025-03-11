using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;

class NetHandler {
    public static void StartServer() {
        TcpListener server = new TcpListener(IPAddress.Any, 25565);
        server.Start();
        Console.WriteLine("Server started on port 25565");

        while (true) {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("New connection!");
            // Handle client in another method/thread
        }
    }
}
