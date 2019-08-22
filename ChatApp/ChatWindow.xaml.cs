using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public int chatServerId = -1;
        private MainWindow main = null;

        public ChatWindow()
        {
            InitializeComponent();
        }

        public void setChatId(int ChatId)
        {
            chatServerId = ChatId;
        }

        public void setMainWindow(MainWindow mainWindow)
        {
            main = mainWindow;
        }

        public void writeMessage(String newMessage)
        {
            chatTextBox.Dispatcher.BeginInvoke(new Action<String>((message) =>
            {
                chatTextBox.Text += message + '\n';
                var oldFocusedElement = FocusManager.GetFocusedElement(this);

                this.chatTextBox.Focus();
                this.chatTextBox.CaretIndex = this.chatTextBox.Text.Length;
                this.chatTextBox.ScrollToEnd();
                FocusManager.SetFocusedElement(this, oldFocusedElement);

            }), new object[] { newMessage });
        }

        // When you press enter, send message to everyone on server
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && messageTextBox.Text!="")
            {
                main.sendMessage(chatServerId,messageTextBox.Text);
                messageTextBox.Clear();
            }
        }
    }
}
