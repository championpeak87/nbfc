using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using NbfcClient.NbfcService;
using NbfcClient.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using SettingsService = StagWare.Settings.SettingsService<NbfcClient.AppSettings>;

namespace NbfcClient.ViewModels
{
    public class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region Private Fields

        private readonly IFanControlClient client;

        private string version;
        private string selectedConfig;
        private string serviceStateString;
        private bool isServiceReadOnly;
        private bool isServiceDisabled;
        private bool isServiceEnabled;
        private int serviceState;
        private int temperature;
        private string temperatureSourceName;
        private ObservableCollection<FanControllerViewModel> fanControllers;

        private RelayCommand selectConfigCommand;
        RelayCommand settingsCommand;

        #endregion

        #region Constructors

        public MainViewModel(IFanControlClient client)
        {
            this.FanControllers = new ObservableCollection<FanControllerViewModel>();
            this.client = client;
            this.client.FanControlStatusChanged += Client_FanControlStatusChanged;
            Messenger.Default.Register<ReloadFanControlInfoMessage>(this, Refresh);
            Refresh(true);
        }

        #endregion

        #region Properties

        public string Version
        {
            get
            {
                if (this.version == null)
                {
                    this.version = GetInformationalVersionString();
                }

                return version;
            }
        }

        public int ServiceState
        {
            get
            {
                if (this.IsServiceDisabled)
                    return 0;
                else if (this.IsServiceReadOnly)
                    return 1;
                else if (this.IsServiceEnabled)
                    return 2;
                else
                    return -1;
            }
            set
            {
                this.serviceState = value;
                if (value == 0)
                {
                    IsServiceDisabled = true;
                    IsServiceEnabled = false;
                    IsServiceReadOnly = false;

                    ServiceStateString = "DISABLED";
                }
                else if (value == 1)
                {
                    IsServiceDisabled = false;
                    IsServiceEnabled = false;
                    IsServiceReadOnly = true;

                    ServiceStateString = "READ-ONLY";
                }
                else if (value == 2)
                {
                    IsServiceDisabled = false;
                    IsServiceEnabled = true;
                    IsServiceReadOnly = false;

                    ServiceStateString = "ENABLED";
                }
                else
                {
                    IsServiceDisabled = false;
                    IsServiceEnabled = false;
                    IsServiceReadOnly = false;

                    ServiceStateString = "UNKNOWN";
                }

                OnPropertyChanged("IsServiceEnabled");
                OnPropertyChanged("IsServiceReadOnly");
                OnPropertyChanged("IsServiceDisabled");
                OnPropertyChanged("ServiceState");
                OnPropertyChanged("ServiceStateString");
            }
        }

        public string ServiceStateString
        {
            get
            {
                if (this.serviceStateString == null)
                {
                    if (this.IsServiceDisabled)
                        this.serviceStateString = "DISABLED";
                    else if (this.IsServiceReadOnly)
                        this.serviceStateString = "READ-ONLY";
                    else if (this.IsServiceEnabled)
                        this.serviceStateString = "ENABLED";
                    else
                        this.serviceStateString = "UNKNOWN";
                }

                return this.serviceStateString;
            }
            set
            {
                this.serviceStateString = value; OnPropertyChanged("IsServiceEnabled");
                OnPropertyChanged("IsServiceReadOnly");
                OnPropertyChanged("IsServiceDisabled");
                OnPropertyChanged("ServiceState");
                OnPropertyChanged("ServiceStateString");
            }
        }

        public string SelectedConfig
        {
            get { return this.selectedConfig; }
            private set
            {
                this.Set(ref this.selectedConfig, value); OnPropertyChanged("IsServiceEnabled");
                OnPropertyChanged("IsServiceReadOnly");
                OnPropertyChanged("IsServiceDisabled");
                OnPropertyChanged("ServiceState");
                OnPropertyChanged("ServiceStateString");
            }
        }

        public bool IsServiceDisabled
        {
            get { return this.isServiceDisabled; }
            set
            {
                if (Set(ref this.isServiceDisabled, value) && value)
                {
                    client.Stop();
                    this.serviceState = 0;
                    IsServiceReadOnly = false;
                    IsServiceEnabled = false;
                    Refresh(true);

                    OnPropertyChanged("IsServiceEnabled");
                    OnPropertyChanged("IsServiceReadOnly");
                    OnPropertyChanged("IsServiceDisabled");
                    OnPropertyChanged("ServiceState");
                    OnPropertyChanged("ServiceStateString");
                }
            }
        }

        public bool IsServiceReadOnly
        {
            get { return this.isServiceReadOnly; }
            set
            {
                if (Set(ref this.isServiceReadOnly, value) && value)
                {
                    client.Start(true);
                    this.serviceState = 1;
                    IsServiceDisabled = false;
                    IsServiceEnabled = false;
                    Refresh(true);

                    OnPropertyChanged("IsServiceEnabled");
                    OnPropertyChanged("IsServiceReadOnly");
                    OnPropertyChanged("IsServiceDisabled");
                    OnPropertyChanged("ServiceState");
                    OnPropertyChanged("ServiceStateString");
                }
            }
        }

        public bool IsServiceEnabled
        {
            get { return this.isServiceEnabled; }
            set
            {
                if (Set(ref this.isServiceEnabled, value) && value)
                {
                    client.Start(false);
                    this.serviceState = 2;
                    IsServiceDisabled = false;
                    IsServiceReadOnly = false;
                    Refresh(true);

                    OnPropertyChanged("IsServiceEnabled");
                    OnPropertyChanged("IsServiceReadOnly");
                    OnPropertyChanged("IsServiceDisabled");
                    OnPropertyChanged("ServiceState");
                    OnPropertyChanged("ServiceStateString");
                }
            }
        }

        public int Temperature
        {
            get { return this.temperature; }
            private set { this.Set(ref this.temperature, value); OnPropertyChanged(nameof(Temperature)); }
        }

        public string TemperatureSourceName
        {
            get { return this.temperatureSourceName; }
            private set { this.Set(ref this.temperatureSourceName, value); OnPropertyChanged(nameof(TemperatureSourceName)); }
        }

        public ObservableCollection<FanControllerViewModel> FanControllers
        {
            get { return this.fanControllers; }
            private set { this.Set(ref this.fanControllers, value); OnPropertyChanged(nameof(FanControllers)); }
        }



        #endregion

        #region Commands       

        public RelayCommand SelectConfigCommand
        {
            get
            {
                if (this.selectConfigCommand == null)
                {
                    this.selectConfigCommand = new RelayCommand(SendOpenSelectConfigDialogMessage);
                }

                return this.selectConfigCommand;
            }
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                if (this.settingsCommand == null)
                {
                    this.settingsCommand = new RelayCommand(SendOpenSettingsDialogMessage);
                }

                return this.settingsCommand;
            }
        }

        #endregion

        #region Private Methods

        private void Refresh(ReloadFanControlInfoMessage msg)
        {
            Refresh(msg.IgnoreCache);
        }

        private void Refresh(bool ignoreCache)
        {
            FanControlInfo info = ignoreCache
                ? client.GetFanControlInfo()
                : client.FanControlInfo;

            UpdateProperties(info);
        }

        private void UpdateProperties(FanControlInfo info)
        {
            IsServiceDisabled = !info.Enabled;
            IsServiceReadOnly = (info.Enabled && info.ReadOnly);
            IsServiceEnabled = info.Enabled;
            Temperature = info.Temperature;
            SelectedConfig = info.SelectedConfig;
            TemperatureSourceName = info.TemperatureSourceDisplayName;

            if (info.FanStatus == null)
            {
                this.fanControllers.Clear();
            }
            else if (this.fanControllers.Count != info.FanStatus.Length)
            {
                this.fanControllers.Clear();

                for (int i = 0; i < info.FanStatus.Length; i++)
                {
                    fanControllers.Add(new FanControllerViewModel(client, i));
                }
            }
        }

        private static void SendOpenSelectConfigDialogMessage()
        {
            Messenger.Default.Send(new OpenSelectConfigDialogMessage());
        }

        private static void SendOpenSettingsDialogMessage()
        {
            Messenger.Default.Send(new OpenSettingsDialogMessage());
        }

        private static string GetInformationalVersionString()
        {
            var attribute = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault();

            if (attribute == null)
            {
                return string.Empty;
            }
            else
            {
                return attribute.InformationalVersion;
            }
        }

        #endregion        

        #region EventHandlers

        private void Client_FanControlStatusChanged(object sender, FanControlStatusChangedEventArgs e)
        {
            Refresh(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }
}
