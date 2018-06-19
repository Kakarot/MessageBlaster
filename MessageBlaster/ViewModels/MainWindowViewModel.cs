using ReactiveUI;
using System;
using System.Reactive;
using MessageBlaster.Protocols;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using System.IO;
using Avalonia.Controls;

namespace MessageBlaster.ViewModels
{
    public class MainWindowViewModel : ViewModelBase , INotifyPropertyChanged
    {
        private string destination;
        private int port;
        private string tcpBinaryFilePath;


        public static IConfiguration Configuration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => "Message Blaster";

        public string Destination
        {
            get { return this.destination; }
            set
            {
                this.destination = value;
                NotifyPropertyChanged();
            }
        }

        
        public string TcpBinaryFilePath
        {
            get { return this.tcpBinaryFilePath; }
            set
            {
                this.tcpBinaryFilePath = value;
                NotifyPropertyChanged();
            }
        }

        public int Port
        {
            get { return this.port; }
            set
            {
                this.port = value;
                NotifyPropertyChanged();
            }
        }
       

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ReactiveCommand<Unit, Unit> BrowseFileButtonCommand { get; }
        public ReactiveCommand<Unit, Unit> SendTcpButtonCommand { get; }
                     

        public MainWindowViewModel()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            BrowseFileButtonCommand = ReactiveCommand.Create(LaunchOpenFileDialog);
            SendTcpButtonCommand = ReactiveCommand.Create(SendTcpMessage);

            //Populate UI defaults from appsettings.json file
            PopulateDefaultDataFromAppSettings();
        }

        private void SendTcpMessage()
        {
            var binarySender = new BinarySender(this.destination,this.port);
            Thread sendMessageThread = new Thread(()=>binarySender.SendMessage(GetDataFromFile(TcpBinaryFilePath)));
            sendMessageThread.Start();
        }

        private void PopulateDefaultDataFromAppSettings()
        {
            this.destination = Configuration["Destination"];
            this.port = Convert.ToInt32(Configuration["Port"]);
            this.tcpBinaryFilePath = Configuration["FileForBinaryTcpMessage"];
        }

    
        private async void LaunchOpenFileDialog()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "xml" } });
                       
            var result = await dlg.ShowAsync();
            if (result != null && result.Length > 0)
            {
                TcpBinaryFilePath = result[0];
            }
            
        }

        private string GetDataFromFile(string FileName)
        {
            string Message = string.Empty;
            if (File.Exists(FileName))
            {
                Message = File.ReadAllText(FileName);
            }
            else
            {
                Console.WriteLine("File {0} does not exist.", FileName);
            }
            return Message;
        }

    }
}
