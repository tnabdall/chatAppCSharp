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
            foreach(ConnectionInfo connectedUser in ConnectedUsers)
            {
                string[] info = new string[2];
                info[0]=chatServerId.ToString();
                info[1]=message;
                connectedUser.connection.SendObject("newMessage", info);
            }
        }

        public void connectUser(ConnectionInfo newUser)
        {
            ConnectedUsers.Add(newUser);
            if (ConnectedUsers.Count() > 1)
            {
                List<String> chatLogCopy = new List<string>(ChatLog);
                chatLogCopy.Insert(0, chatServerId.ToString());
                newUser.connection.SendObject("chatLog", chatLogCopy);
            }
        }

        public void disconnectUser(ConnectionInfo user)
        {
            ConnectedUsers.Remove(user);
        }

    }
}
