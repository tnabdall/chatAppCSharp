using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace chatServer
{
    class Server
    {
        // Key is user id
        private static Dictionary<int,ConnectionInfo> connections = new Dictionary<int,ConnectionInfo>();
        // Key is chat server id
        private static Dictionary<int,ChatServer> chatServers = new Dictionary<int, ChatServer>();

        static void Main(string[] args)
        {
            //Trigger the method PrintIncomingMessage when a packet of type 'Message' is received
            //We expect the incoming object to be a string which we state explicitly by using <string>
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("ConnectionRequest", ConnectNewUser);
            NetworkComms.AppendGlobalIncomingPacketHandler<string[]>("OpenServer", OpenNewChatServer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("connectMe", ConnectToChatServer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string[]>("newMessage", AddMessageChatServer);

            // Close connection and open connection array for new connection
            NetworkComms.AppendGlobalConnectionCloseHandler(closeConnection);
            //Start listening for incoming connections
            Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));

            //Print out the IPs and ports we are now listening on
            Console.WriteLine("Server listening for TCP connection on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);

            //Let the user close the server
            Console.WriteLine("\nPress any key to close server.");
            Console.ReadKey(true);

            //We have used NetworkComms so we should ensure that we correctly call shutdown
            NetworkComms.Shutdown();
        }

        private static void AddMessageChatServer(PacketHeader header, Connection connection, string[] info)
        {
            ConnectionInfo user = connections[int.Parse(info[0])];
            ChatServer cs = chatServers[int.Parse(info[1])];
            cs.writeMessage(user.username + ": " + info[2]);
        }

        // Connects user to existing chat server
        private static void ConnectToChatServer(PacketHeader header, Connection connection, string connectionString)
        {
            ChatServer cs = chatServers[int.Parse(connectionString.Split('|').First())];
            ConnectionInfo user = connections[int.Parse(connectionString.Split('|').Last())];
            user.joinServer(cs);            
            // Should open chat server and print log to window

        }

        // Creates a new chat server and invites requested users to server
        // First element in requested users is the sending users id
        private static void OpenNewChatServer(PacketHeader header, Connection connection, string[] requestedUsers)
        {
            // Open a new chat server and connect current user
            ChatServer newServer = new ChatServer();
            chatServers[newServer.chatServerId] = newServer;
            ConnectionInfo requestingUser = connections[int.Parse(requestedUsers[0])];
            newServer.connectUser(requestingUser);

            connection.SendObject("chatServerOpened", newServer.chatServerId);

            // Sends a request to each user if they would like to connect to the chat server
            foreach (KeyValuePair<int, ConnectionInfo> connectedUser in connections)
            {               
                for (int i = 1; i < requestedUsers.Count(); i++)
                {
                    if (requestedUsers[i] == connectedUser.Value.username)
                    {
                        connectedUser.Value.connection.SendObject("userConnectionRequest", newServer.chatServerId + "|User " + requestingUser.username + " would like to chat. Accept?");
                    }
                }
            }

        }

        // Connects user to chat server and sends the chat log
        private static void ConnectToChatServer(PacketHeader header, Connection connection, string[] info)
        {
            Console.WriteLine("New user joining chat server: " + int.Parse(info[1]));
            // Get users connection and chat server
            int userId = int.Parse(info[0]);
            int chatId = int.Parse(info[1]);
            ChatServer cs = chatServers[chatId];
            ConnectionInfo userConnection = connections[userId];

            // Connect user to chat server
            userConnection.joinServer(cs);

            // Send user current chat server log
            connection.SendObject("ChatLog", cs.ChatLog);
        }

        // Connects a new user to server.
        private static void ConnectNewUser(PacketHeader header, Connection connection, string newUsername)
        {
            Console.WriteLine("New user joining: " + newUsername);
            // Create new user connection info
            ConnectionInfo newUser = new ConnectionInfo(newUsername, connection);
            
            // Create list of usernames from those connected. Let them know a new user has been added to the list
            List<string> connectedUserNames = new List<string>();
            // Adds user Id to allow client class to assign user id for sending packets
            connectedUserNames.Add(newUser.userId.ToString());

            foreach (KeyValuePair<int,ConnectionInfo> connectedUser in connections)
            {
                int connectedUserId = connectedUser.Key;
                String username = connectedUser.Value.username;
                connectedUserNames.Add(username);
                Connection connectionExistingUser = connectedUser.Value.connection;
                connectionExistingUser.SendObject("newUserAdded", newUsername);
            }

            // Add new user to existing user list and return list of all other users
            connections.Add(newUser.userId, newUser);
            connection.SendObject("ConnectionInfo", connectedUserNames);
        }

        // Leaves all chat servers and the connected users list
        private static void closeConnection(Connection connection)
        {
            foreach (KeyValuePair<int, ConnectionInfo> connectedUser in connections)
            {
                if(connectedUser.Value.connection.ToString() == connection.ToString())
                {
                    for(int i = 0; i<connectedUser.Value.connectedServers.Count(); i++)
                    {
                        connectedUser.Value.leaveServer(chatServers[connectedUser.Value.connectedServers[i]]);
                    }
                    connections.Remove(connectedUser.Key);
                    return;
                }
            }

        }


    }
}
