using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace chatServer
{
    class ConnectionInfo
    {
        public String username;
        public Connection connection;
        public int userId;
        public static int USER_ID_CREATOR = 0;
        public List<int> connectedServers = new List<int>();

        public ConnectionInfo(String username, Connection connection)
        {
            this.username = username;
            this.connection = connection;
            userId = USER_ID_CREATOR;
            USER_ID_CREATOR += 1;
        }

        public void joinServer(ChatServer cs)
        {
            cs.connectUser(this);
            connectedServers.Add(cs.chatServerId);
        }

        public void leaveServer(ChatServer cs)
        {
            cs.disconnectUser(this);
            connectedServers.Remove(cs.chatServerId);
        }

        
    }
}
