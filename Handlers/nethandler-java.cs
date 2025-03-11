using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

class NetHandler_Java {
    private const int PORT = 25565;

    public static void StartServer() {
        TcpListener server = new TcpListener(IPAddress.Any, PORT);
        server.Start();
        Console.WriteLine($"[JAVA] Server started on port {PORT}");

        while (true) {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client) {
        NetworkStream stream = client.GetStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);

        try {
            int packetLength = ReadVarInt(reader);
            int packetID = ReadVarInt(reader);

            if (packetID == 0x00) { // Handshake packet
                int protocolVersion = ReadVarInt(reader);
                string serverAddress = ReadString(reader);
                ushort serverPort = (ushort)reader.ReadUInt16();
                int nextState = ReadVarInt(reader);

                Console.WriteLine($"[JAVA] Handshake from {client.Client.RemoteEndPoint}: Protocol {protocolVersion}, NextState {nextState}");

                if (nextState == 1) { // Status (Server Ping)
                    HandleStatus(writer);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"[ERROR] {ex.Message}");
        } finally {
            client.Close();
        }
    }

    private static void HandleStatus(BinaryWriter writer) {
        string jsonResponse = "{\"version\":{\"name\":\"My C# Server\",\"protocol\":756},\"players\":{\"max\":100,\"online\":0},\"description\":{\"text\":\"Welcome to my C# Minecraft Server!\"}}";

        WriteVarInt(writer, jsonResponse.Length + 1);
        writer.Write((byte)0x00);
        WriteString(writer, jsonResponse);
        writer.Flush();
    }

    private static int ReadVarInt(BinaryReader reader) {
        int numRead = 0, result = 0;
        byte read;
        do {
            read = reader.ReadByte();
            int value = read & 0b01111111;
            result |= value << (7 * numRead);
            numRead++;
            if (numRead > 5) throw new Exception("VarInt is too big!");
        } while ((read & 0b10000000) != 0);
        return result;
    }

    private static void WriteVarInt(BinaryWriter writer, int value) {
        while ((value & -128) != 0) {
            writer.Write((byte)((value & 127) | 128));
            value >>= 7;
        }
        writer.Write((byte)value);
    }

    private static string ReadString(BinaryReader reader) {
        int length = ReadVarInt(reader);
        byte[] data = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(data);
    }

    private static void WriteString(BinaryWriter writer, string value) {
        byte[] data = Encoding.UTF8.GetBytes(value);
        WriteVarInt(writer, data.Length);
        writer.Write(data);
    }
}
