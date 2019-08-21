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
            
        }

        // Opens new chat server
        private void OpenNewChatServer(PacketHeader header, Connection connection, int chatServerId)
        {
            chatWindows.Add(chatServerId, new ChatWindow());
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
                ChatWindow chatWindow = new ChatWindow();
                chatWindow.Show();
                NetworkComms.SendObject("connectMe", serverIP, serverPort, message.Split('|').First()+"|"+userId);
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
