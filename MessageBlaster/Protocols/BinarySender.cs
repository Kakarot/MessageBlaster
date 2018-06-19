using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageBlaster.Protocols
{
    internal class BinarySender
    {
        private string destination;
        private int port;

        private TcpClient client;
        internal BinarySender(string destination, int port)
        {
            this.destination = destination;
            this.port = port;
        }

        internal void SendMessage(string message)
        {
            Console.WriteLine("Preparing to send TCP Binary Message");

            try
            {
                client = new TcpClient(destination, port);
                Console.WriteLine($"Successfully connected to {destination} on port# {port}\n");
                NetworkStream stream = client.GetStream();
                var sr = new StreamReader(stream);
                var sw = new BinaryWriter(stream);
                

                Int32 responseLength = message.Length;
                var workArray = new List<byte>();

                // GetBytes returns "response.length" in a byte format
                byte[] originalResponseLengthAsByteArray = BitConverter.GetBytes(responseLength);

                // workArray is now storing the length of "response" in the first 4 bytes
                workArray.AddRange(originalResponseLengthAsByteArray);



                var bitMessage = Encoding.ASCII.GetBytes(message);
                workArray.InsertRange(Marshal.SizeOf(responseLength), bitMessage);

                byte[] myByteArray = workArray.ToArray();

                Console.WriteLine($"The size of the message is {responseLength} bytes\n");
                sw.Write(myByteArray);

                sw.Flush();

                if (sr != null && !sr.EndOfStream)
                {
                    var receiveBuffer = new char[client.ReceiveBufferSize];
                    sr.Read(receiveBuffer, 0, client.ReceiveBufferSize);
                    Console.WriteLine("    Response " +
                                     Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(receiveBuffer)).TrimEnd('\0'));
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}\n", e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP listner on server abruptly closed socket stream. Exception was: ${ex.Message}\n");
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        /// <summary>
        /// Maps the specified structure to a memory block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        internal byte[] StructureToByteArray<T>(ref T item)
            where T : struct
        {
            int size = Marshal.SizeOf(item);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(item, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}
