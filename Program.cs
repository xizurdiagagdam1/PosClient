﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PosClient
{
    public class Message
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }
        public string Stamp { get; set; }

        public override string ToString()
        {
            return $"From: {From}\nTo: {To}\n{Msg}\nStamp: {Stamp}";
        }
    }

    public class Client
    {
        public static string ip = "127.0.0.1";
        public static int port = 14300;
        public static int TAM = 1024;

        public static IPAddress GetLocalIpAddress()
        {
            List<IPAddress> ipAddressList = new List<IPAddress>();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            int t = ipHostInfo.AddressList.Length;
            string ip;
            for (int i = 0; i < t; i++)
            {
                ip = ipHostInfo.AddressList[i].ToString();
                if (ip.Contains(".") && !ip.Equals("127.0.0.1")) ipAddressList.Add(ipHostInfo.AddressList[i]);
            }
            if (ipAddressList.Count > 0)
            {
                return ipAddressList[0];//devuelve la primera posible
            }
            return null;
        }

        public static void ReadServerIpPort()
        {
            string s;
            System.Console.WriteLine("Datos del servidor: ");
            string defIp = GetLocalIpAddress().ToString();
            System.Console.Write("Dir. IP [{0}]: ", defIp);
            s = Console.ReadLine();
            if ((s.Length > 0) && (s.Replace(".", "").Length == s.Length - 3))
            {
                ip = s;
            }
            else
            {
                ip = defIp;
            }
            System.Console.Write("PUERTO [{0}]: ", port);
            s = Console.ReadLine();
            if (Int32.TryParse(s, out int i))
            {
                port = i;
            }
        }

        public static void PrintOptionMenu()
        {
            System.Console.WriteLine("====================");
            System.Console.WriteLine("        MENU        ");
            System.Console.WriteLine("====================");
            System.Console.WriteLine("0: Exit");
            System.Console.WriteLine("1: Chequear correo");
            System.Console.WriteLine("2: Obtener mensaje");
            System.Console.WriteLine("3: Escribir mensaje");
        }

        public static int ReadOption()
        {
            string s = null;
            while (true)
            {
                System.Console.Write("Opción [0-3]: ");
                s = Console.ReadLine();
                if (Int32.TryParse(s, out int i))
                {
                    if ((i >= 0) && (i <= 3))
                    {
                        return i;
                    }
                }
            }
        }

        public static Socket Connect()
        {
            IPAddress ipAddress = System.Net.IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(remoteEP);

            return socket;
        }

        public static void Disconnect(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public static void Send(Socket socket, Message message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            Stream stream = new MemoryStream();
            serializer.Serialize(stream, message);
            byte[] byteData = ((MemoryStream)stream).ToArray();
            // string xml = Encoding.ASCII.GetString(byteData, 0, byteData.Length);
            // Console.WriteLine(xml);//Imprime el texto enviado
            int bytesSent = socket.Send(byteData);
        }

        public static Message Receive(Socket socket)
        {
            byte[] bytes = new byte[TAM];
            int bytesRec = socket.Receive(bytes);
            string xml = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            // Console.WriteLine(xml);//Imprime el texto recibido
            byte[] byteArray = Encoding.ASCII.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);
            Message response = (Message)new XmlSerializer(typeof(Message)).Deserialize(stream);
            return response;
        }

        public static void Process(int option)
        {
            switch (option)
            {
                case 1:
                    ChequearCorreo();
                    break;
                case 2:
                    ObtenerMensaje();
                    break;
                case 3:
                    EscribirMensaje();
                    break;
            }
        }

        public static void ChequearCorreo()
        {
            System.Console.WriteLine("--------------------");
            System.Console.WriteLine("1: Chequear correo  ");
            System.Console.WriteLine("--------------------");
            System.Console.Write("From: ");
            string f = Console.ReadLine();

            // TODO: Chequear Correo
            Socket socket = Connect();
            Message mensaje = new Message { From = f, To = "0", Msg = "LIST", Stamp = "Client" };
            Send(socket, mensaje);
            Message recived = Receive(socket);
            Console.WriteLine("................\n" + recived.ToString());
            Disconnect(socket);
        }

        public static void ObtenerMensaje()
        {
            System.Console.WriteLine("--------------------");
            System.Console.WriteLine("2: Obtener mensaje  ");
            System.Console.WriteLine("--------------------");
            System.Console.Write("From: ");
            string f = Console.ReadLine();
            System.Console.Write("Num.: ");
            string n = Console.ReadLine();

            // TODO: Obtener Mensaje
            Socket socket = Connect();
            Message mensaje = new Message { From = f, To = "0", Msg = "RETR " + n, Stamp = "Client" };
            Send(socket, mensaje);
            Message recived = Receive(socket);
            Console.WriteLine("................\n" + recived.ToString());
            Disconnect(socket);
        }

        public static void EscribirMensaje()
        {
            System.Console.WriteLine("--------------------");
            System.Console.WriteLine("3: Escribir mensaje ");
            System.Console.WriteLine("--------------------");
            System.Console.Write("From: ");
            string f = Console.ReadLine();
            System.Console.Write("To: ");
            string t = Console.ReadLine();
            System.Console.Write("Msg: ");
            string m = Console.ReadLine();

            // TODO: Escribir Mensaje
            Socket socket = Connect();
            Message mensaje = new Message { From = f, To = t, Msg = m, Stamp = "Client" };
            Send(socket, mensaje);
            Message recived = Receive(socket);
            Console.WriteLine("................\n" + recived.ToString());
            Disconnect(socket);
        }

        public static int Main(String[] args)
        {
            ReadServerIpPort();
            while (true)
            {
                PrintOptionMenu();
                int opt = ReadOption();
                if (opt == 0) break;
                Process(opt);
            }
            System.Console.WriteLine("FIN.");
            return 0;
        }
    }
}