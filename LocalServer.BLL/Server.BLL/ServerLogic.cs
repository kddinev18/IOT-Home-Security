﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Collections.Generic;
using System.Timers;
using System;
using System.Data.SqlClient;
using System.Net.Http;
using LocalServer.BLL.DataManipulation.BLL;

namespace LocalServer.BLL.Server.BLL
{
    public class ServerLogic
    {
        private static TcpListener _tcpListener;
        private static List<TcpClient> _clients = new List<TcpClient>();

        private static byte[] _data = new byte[16777216];

        private int _port;
        private static int _success = 0;
        private static int _error = 1;
        private static DatabaseInitialiser _databaseInitialiser;
        public ServerLogic(int port, long deleteTiemr)
        {
            _port = port;
            _databaseInitialiser = new DatabaseInitialiser(deleteTiemr);
        }
        public void ServerSetUp()
        {
            try
            {
                ClientHandlingLogic.DatabaseInitialiser = _databaseInitialiser;
                UserAuthenticationLogic.DatabaseInitialiser = _databaseInitialiser;
                UserModifierLogic.DatabaseInitialiser = _databaseInitialiser;
                DeviceModificationLogic.DatabaseInitialiser = _databaseInitialiser;
                RoleModificationLogic.DatabaseInitialiser = _databaseInitialiser;
                PermissionModifierLogic.DatabaseInitialiser = _databaseInitialiser;
                GlobalServerComunicationLogic.DatabaseInitialiser = _databaseInitialiser;

                _tcpListener = new TcpListener(IPAddress.Any, _port);
                // Starts the server
                _tcpListener.Start();
                // Starts accepting clients
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptClients), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ServerShutDown();
            }
        }

        public void ServerShutDown()
        {
            // Stops the server
            if (_tcpListener != null)
                _tcpListener.Stop();
            foreach (TcpClient client in _clients)
            {
                DisconnectClient(client);
            }
            _tcpListener = null;
        }

        public static void AcceptClients(IAsyncResult asyncResult)
        {
            // Newly connection client
            TcpClient client = null;
            bool accepted = false;
            try
            {
                // Connect the client
                if (_tcpListener is not null)
                {
                    client = _tcpListener.EndAcceptTcpClient(asyncResult);
                    Console.WriteLine("Client connected with IP {0}", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    accepted = ClientHandlingLogic.AddClients(client);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (accepted)
            {
                // Add the client newly connect client into the _clients list
                _clients.Add(client);
                // Begin recieving bytes from the client
                client.Client.BeginReceive(_data, 0, _data.Length, SocketFlags.None, new AsyncCallback(ReciveClientInput), client);
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptClients), null);
            }
            else if (_tcpListener is not null)
            {
                DisconnectClient(client);
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptClients), null);
            }
        }

        public static void ReciveClientInput(IAsyncResult asyncResult)
        {
            TcpClient client = asyncResult.AsyncState as TcpClient;
            int reciever;
            try
            {
                // How many bytes has the user sent
                reciever = client.Client.EndReceive(asyncResult);
                // If the bytes are - disconnect the client
                if (reciever == 0)
                {
                    DisconnectClient(client);
                    return;
                }
                // Get the data
                ClientHandlingLogic.HandleClientInput(Encoding.ASCII.GetString(_data).Replace("\0", string.Empty), ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), _clients);
            }
            catch (Exception ex)
            {
                string response = $"{_error}|{ex.Message}";
                // send data to the client
                try
                {
                    client.Client.Send(Encoding.ASCII.GetBytes(response));
                }
                catch (Exception) 
                {
                    DisconnectClient(client);  
                }
            }
            finally
            {
                FlushBuffer();
            }
            try
            {
                client.Client.BeginReceive(_data, 0, _data.Length, SocketFlags.None, new AsyncCallback(ReciveClientInput), client);
            }
            catch (Exception) { }
        }

        // Clear the buffer
        public static void FlushBuffer()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        public static void DisconnectClient(TcpClient client)
        {
            Console.WriteLine("Client disconnected");
            client.Client.Shutdown(SocketShutdown.Both);
            client.Client.Close();
            _clients.Remove(client);
            client = null;
        }

        public static TcpClient GetClient(string ipAddress)
        {
            return _clients.Where(client => ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() == ipAddress).First();
        }
    }
}