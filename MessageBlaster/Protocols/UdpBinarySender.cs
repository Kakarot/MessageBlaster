using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageBlaster.Protocols
{
    internal class UdpBinarySender
    {
        private string destination;
        private int port;

        private UdpClient client;
        internal UdpBinarySender(string destination, int port)
        {
            this.destination = destination;
            this.port = port;
        }

        internal void SendMessage(string message)
        {
            Console.WriteLine("Preparing to send UDP Binary Message");

            try
            {
                client = new UdpClient(destination, port);
                Console.WriteLine($"Successfully connected to {destination} on port# {port}\n");
                

                var bitMessage = Encoding.ASCII.GetBytes(message);
                client.Send(bitMessage, bitMessage.Length);
                Console.WriteLine($"Sent message to {destination} on port# {port}\n");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}\n", e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General exception occurred. Exception was: ${ex.Message}\n");
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
