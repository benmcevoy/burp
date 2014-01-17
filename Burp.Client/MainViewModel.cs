using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Burp.Services;
using Burp.Model;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Burp.Client
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MessageService _service;
        private readonly Dispatcher _dispatcher;

        public MainViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            
            _service = new MessageService(ConfigurationManager.AppSettings["burp.client.name"],
                ConfigurationManager.AppSettings["burp.client.avatar"],
                1100,
                IPAddressHelper.LocalIPAddress,
                IPAddress.Parse(ConfigurationManager.AppSettings["burp.client.subnetmask"]));

            _service.MessageReceived += new EventHandler<MessageEventArgs>(MessageReceived);
            _service.ConnectedClientsChanged += new EventHandler(ConnectedClientsChanged);

            Source = _service.Source;

            ConnectedClients = new ObservableCollection<Model.Client>();
            Messages = new ObservableCollection<Model.Message>();
            SendCommand = new RelayCommand(c => Send());
        }

        private void ConnectedClientsChanged(object sender, EventArgs e)
        {
            _dispatcher.Invoke((Action)delegate
            {
                ConnectedClients = new ObservableCollection<Model.Client>(_service.ConnectedClients.Values);
                OnPropertyChanged("ConnectedClients");
            });
        }

        public void Send()
        {
            if (!string.IsNullOrEmpty(_message))
            {
                _service.Send(_message);
                this.Message = "";
            }
        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            _dispatcher.Invoke((Action)delegate
            {
                Messages.Add(e.Message);
                OnPropertyChanged("Messages");
            });
        }

        public Model.Client Source { get; set; }

        private string _message;
        public string Message { get { return _message; } set { _message = value; OnPropertyChanged("Message"); } }

        public ObservableCollection<Model.Client> ConnectedClients { get; set; }

        public ObservableCollection<Message> Messages { get; set; }
        
        public RelayCommand SendCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose()
        {
            _service.Dispose();
        }
    }
}