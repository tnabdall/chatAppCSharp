using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace chatServer
{
    class ChatServer
    {
        public int chatServerId;
        public static int CHATSERVERIDCREATOR = 0;
        public List<String> ChatLog = new List<string>();
        public List<ConnectionInfo> ConnectedUsers = new List<ConnectionInfo>();

        public ChatServer()
        {
            chatServerId = CHATSERVERIDCREATOR;
            CHATSERVERIDCREATOR += 1;
        }

        public void writeMessage(String message)
        {
            ChatLog.Add(message);
        }

        public void connectUser(ConnectionInfo newUser)
        {
            ConnectedUsers.Add(newUser);
        }

        public void disconnectUser(ConnectionInfo user)
        {
            ConnectedUsers.Remove(user);
        }

    }
}
