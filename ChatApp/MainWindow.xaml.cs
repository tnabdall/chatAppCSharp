using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String serverIP;
        int serverPort;
        int userId;
        Dictionary<int,ChatWindow> chatWindows = new Dictionary<int, ChatWindow>(); // Key is chat id
        public MainWindow()
        {
            InitializeComponent();
            NetworkComms.AppendGlobalIncomingPacketHandler<List<String>>("ConnectionInfo", ReceiveConnectionInfoPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("newUserAdded", ReceiveNewUser);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("userConnectionRequest", ReceiveUserConnectionRequestPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("chatServerOpened", OpenNewChatServer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string[]>("newMessage", AddNewMessage);
            NetworkComms.AppendGlobalIncomingPacketHandler<List<String>>("chatLog", WriteChatLog);
            
        }

        private void WriteChatLog(PacketHeader header, Connection connection, List<String> logInfo)
        {
            int serverId = int.Parse(logInfo[0]);
            for (int i = 1; i< logInfo.Count(); i++)
            {
                chatWindows[serverId].writeMessage(logInfo[i]);
            }
        }

        private void AddNewMessage(PacketHeader header, Connection connection, String[] info)
        {
            chatWindows[int.Parse(info[0])].writeMessage(info[1]);
        }

        // Opens new chat server
        private void OpenNewChatServer(PacketHeader header, Connection connection, int chatServerId)
        {
            this.Dispatcher.BeginInvoke(new Action<int>((serverId) =>
            {
                chatWindows.Add(serverId, new ChatWindow());
                chatWindows[serverId].Dispatcher.BeginInvoke(new Action<int>((chatId) => this.Show()), new object[] { serverId });
                chatWindows[serverId].Show();
                chatWindows[serverId].setChatId(serverId);
                chatWindows[serverId].setMainWindow(this);
            }), new object[] { chatServerId });
        }

        // Receives initial connection request. Assigns user id and populates list.
        private void ReceiveConnectionInfoPacket(PacketHeader header, Connection connection, List<String> userList)
        {           
            connectedUsersListBox.Dispatcher.BeginInvoke(new Action<List<String>>((userListToAdd) =>
            {
                userId = int.Parse(userListToAdd.First());
                userListToAdd.RemoveAt(0);
                foreach (String user in userListToAdd)
                {
                    connectedUsersListBox.Items.Add(user);
                }
            }), new object[] { userList });

            connectButton.Dispatcher.BeginInvoke(new Action(()=>{
                connectButton.Content = "Connected!";
                connectButton.IsEnabled = false;
            }));

            usernameTextBox.Dispatcher.BeginInvoke(new Action(() => {
                usernameTextBox.IsReadOnly = true;
            }));

            serverIpTextBox.Dispatcher.BeginInvoke(new Action(() => {
                serverIpTextBox.IsReadOnly = true;
            }));

        }

        // Receives packet that lets them know a user has been added.
        private void ReceiveNewUser(PacketHeader header, Connection connection, String message)
        {
            connectedUsersListBox.Dispatcher.BeginInvoke(new Action<string>((userToAdd) => {connectedUsersListBox.Items.Add(userToAdd); }), new object[] { message });
        }

        private void ReceiveUserConnectionRequestPacket(PacketHeader header, Connection connection, String message)
        {
            MessageBoxResult result = MessageBox.Show(message.Split('|').Last(), "Connection Request", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int chatServerId = int.Parse(message.Split('|').First());
                NetworkComms.SendObject("connectMe", serverIP, serverPort, message.Split('|').First()+"|"+userId);
                this.Dispatcher.BeginInvoke(new Action<int>((serverId) =>
                {
                    chatWindows.Add(serverId, new ChatWindow());
                    chatWindows[serverId].Dispatcher.BeginInvoke(new Action<int>((chatId) => this.Show()), new object[] { serverId });
                    chatWindows[serverId].Show();
                    chatWindows[serverId].setChatId(serverId);
                    chatWindows[serverId].setMainWindow(this);
                }), new object[] { chatServerId });
            }
        }

        private bool isValidUser()
        {
            return usernameTextBox.Text != "";
        }

        private bool isValidIP()
        {
            String text = serverIpTextBox.Text;
            Regex rx = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}[:]\d{1,6}$",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = rx.Matches(text);
            return (matches.Count != 0);            
        }

        public void sendMessage(int chatWindow, String message)
        {
            String[] info = new string[3];
            info[0] = userId.ToString();
            info[1] = chatWindow.ToString();
            info[2] = message;
            NetworkComms.SendObject("newMessage", serverIP, serverPort, info);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValidUser())
            {
                if (isValidIP())
                {
                    try
                    {
                        string username = usernameTextBox.Text.Trim();
                        string serverInfo = serverIpTextBox.Text.Trim();
                        serverIP = serverInfo.Split(':').First();
                        serverPort = int.Parse(serverInfo.Split(':').Last());
                        NetworkComms.SendObject("ConnectionRequest", serverIP, serverPort, username);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to connect to specified server.");
                    }
                }
                else
                {
                    MessageBox.Show("Must enter IP address in format 192.168.1.100:5000");
                    serverIpTextBox.Clear();
                }
            }
            else
            {
                MessageBox.Show("Must enter a valid username");

            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectedUsersListBox.SelectedItems.Count > 0)
            {
                String[] requestedUsers = new string[connectedUsersListBox.SelectedItems.Count + 1];
                requestedUsers[0] = userId.ToString();
                for(int i = 0; i< connectedUsersListBox.SelectedItems.Count; i++)
                {
                    requestedUsers[i + 1] = connectedUsersListBox.SelectedItems[i].ToString();
                }
                NetworkComms.SendObject("OpenServer", serverIP, serverPort, requestedUsers);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
        }
    }
}
