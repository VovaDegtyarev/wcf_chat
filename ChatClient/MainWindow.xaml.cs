using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
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

using ChatClient.ServiceChatHost;
using Microsoft.Win32;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IServiceChatCallback
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //путь к файлу с диалогами
        public string FilePath { get; set; }
        //подключен ли пользователь к серверу
        bool isConnected = false;
        //id пользователя
        int id;
        //id необходимого пользователя
        int needId;
        //флаг личного сообщения
        bool privateMsg;
        string nameCurr;
        //int privateId;
        ServiceChatClient client;
        ServerUser[] listUser = null;
        //ServerUser[] listUserForPersonalMsg = null;
        string temp;
        //список для сохранения диалогов
        List<string> currentDialog = new List<string>();
        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// подключение пользователя к сервису
        /// </summary>
        void ConnectUser()
        {
            if (!isConnected && !string.IsNullOrWhiteSpace(tbUserName.Text))
            {
                client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
                id = client.Connect(out listUser, tbUserName.Text);
                nameCurr = tbUserName.Text;
                ShowListUsers(listUser);
                tbUserName.IsEnabled = false;
                bConnDiscon.Content = "Disconnect";
                isConnected = true;
            }
            else
            {
                MessageBox.Show("Введите имя пользователя");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// отключение пользователя от сервиса
        /// </summary>
        void DisconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(id);
                client = null;
                tbUserName.IsEnabled = true;
                bConnDiscon.Content = "Connect";
                isConnected = false;
                ShowListUsers(listUser);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// обработка нажатия кнопки подключения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            { 
                DisconnectUser();
            }
            else
            {
                ConnectUser();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// отправка сообщения всем пользователям
        /// </summary>
        /// <param name="msg"></param>
        public void MessageCallBack(string msg)
        {
            lbChat.Items.Add(msg); // отображение самого сообщения
        }

        //--------------------------------------------------------------------------------------------------------------------------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// обработка события при закрытии окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            DisconnectUser();
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// обработка события по нажитию enter, отправка сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (client != null && privateMsg != true)
                {
                    client.SendMessage(tbMessage.Text, id);
                    tbMessage.Text = string.Empty;
                } else
                if (privateMsg)
                {
                    string tempMsg = "[Private:] " + ": " + tbMessage.Text;
                    client.PrivateSendMessage(tempMsg, id, needId);
                    lbChat.Items.Add(tempMsg);                          //!!!!!!!
                    tbMessage.Text = string.Empty;
                    privateMsg = false;
                    TypeMsg.Content = "[General]";
                }
            }
            if (e.Key == Key.Escape) // отмена личного сообщения
            {
                privateMsg = false;
                tbMessage.Text = "";
                TypeMsg.Content = "[General]";
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// отображение пользователей в онлайне
        /// </summary>
        /// <param name="UserList"></param>
        public void ShowListUsers(ServerUser[] UserList)
        {
            ListOnlineUsers.Items.Clear();
            if (UserList != null)
                foreach (ServerUser ul in UserList)
                {
                    ListOnlineUsers.Items.Add(ul.Name);
                }
            listUser = UserList;
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// вызов личного сообщения и двойной клик по имени, чтобы в текст бокс отправилось имя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListOnlineUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //tbMessage.Text = ListOnlineUsers.SelectedValue.ToString() + ", ";
            //string tempName = ListOnlineUsers.SelectedValue.ToString(); //сохраняем имя пользователя, которому будем отправлять сообщение
            //вызов окна осуществляется только не при пустом списке онлайна
            /*
            if (listUser != null && nameCurr != tempName)
            {
                window1 = new Window1(this, client, listUser);
                window1.Show();
            }
            tempName = "";
            */
            
            temp = ListOnlineUsers.SelectedItem.ToString();
            TypeMsg.Content = "[Private]: " + temp;
            //tbMessage.Text = "[Private:] " + temp + ", ";
            foreach (var li in listUser)
            {
                if (li != null)
                {
                    if (li.Name == temp)
                    {
                        needId = li.id;
                    }
                }
            }
            privateMsg = true;
        }

        public void PrivateMessageCallBack(string msg)
        {
            lbChat.Items.Add(msg);
        }

        /// <summary>
        /// Сериализация данных listBox в файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<string>));
        private void SaveDialog_Click(object sender, RoutedEventArgs e)
        {
            string fileName = tbUserName.Text + ".json";
            foreach (string item in lbChat.Items)
            {
                currentDialog.Add(item);
            }           
            using (FileStream file = new FileStream(fileName, FileMode.Create))
            {
                jsonFormatter.WriteObject(file, currentDialog);
            }
        }

        /// <summary>
        /// Просмотр сохраненных диалогов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
            using (FileStream file = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                List<string> loadDialogs = jsonFormatter.ReadObject(file) as List<string>;
                if (loadDialogs != null)
                {
                    Window1 window1 = new Window1(loadDialogs);
                    window1.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Файл пуст");
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------



    }
}
