using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace UITest___Launcher.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AppFormViewModel : INotifyPropertyChanged
    {
        #region Properties

        [JsonProperty]
        public string Token { get; set; }

        [JsonProperty]
        public string AppID { get; set; }

        [JsonProperty]
        public string DevicesID { get; set; }

        [JsonProperty]
        public string AppPath { get; set; }

        [JsonProperty]
        public string TestPath { get; set; }

        [JsonProperty]
        public string TestSeries { get; set; }

        [JsonProperty]
        public string Locale { get; set; }

        [JsonProperty]
        public string Category { get; set; }

        public string ResultValue { get; set; }

        public ICommand Import { get; }
        public ICommand Export { get; }
        public ICommand Execute { get; }
        #endregion

        #region Property changed

        /// <summary>
        /// Property change event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method used for update properties in view
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructor

        public AppFormViewModel()
        {
            Import = new Command(ImportExecute);
            Export = new Command(ExportExecute);
            Execute = new Command(ExecuteTestExecute);

            NotifyPropertyChanged(nameof(Token));
        }

        #endregion

        #region Methods

        private void ExecuteTestExecute()
        {
            Task task = new Task(async () => { await ExecutePowerShell(); });
            task.Start();
        }

        private Task ExecutePowerShell()
        {
            UpdateResultValue("==== Start ====");

            try
            {
                PowerShell ps = PowerShell.Create();

                PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
                output.DataAdded += new EventHandler<DataAddedEventArgs>(Output_DataAdded);

                var loginCommand = ps.AddCommand("appcenter")
                   .AddArgument("login")
                   .AddParameter("--token", Token)
                   .BeginInvoke<PSObject, PSObject>(null, output);

                loginCommand.AsyncWaitHandle.WaitOne();

                PSDataCollection<PSObject> secondOutput = new PSDataCollection<PSObject>();
                secondOutput.DataAdded += new EventHandler<DataAddedEventArgs>(Output_DataAdded);

                var testCommand = ps.AddCommand("appcenter")
                    .AddArgument("test")
                    .AddArgument("run")
                    .AddArgument("uitest")
                    .AddParameter("--app", SurroundByQuote(AppID))
                    .AddParameter("--devices", SurroundByQuote(DevicesID))
                    .AddParameter("--app-path", AppPath)
                    .AddParameter("--build-dir", TestPath)
                    .AddParameter("--test-series", TestSeries)
                    .AddParameter("--locale", SurroundByQuote(Locale))
                    .AddParameter("--include-category", Category)
                    .AddArgument("--async")
                    .BeginInvoke<PSObject, PSObject>(null, secondOutput);

                testCommand.AsyncWaitHandle.WaitOne();
            }
            catch (Exception e)
            {
                UpdateResultValue(e.Message);
            }

            UpdateResultValue("==== End ====");
            return Task.CompletedTask;
        }

        private void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<PSObject> myp = (PSDataCollection<PSObject>)sender;

            Collection<PSObject> results = myp.ReadAll();
            foreach (PSObject result in results)
            {
                UpdateResultValue(result.ToString());
            }
        }

        private string SurroundByQuote(string value)
        {
            return "\"" + value + "\"";
        }

        private void UpdateResultValue(string value)
        {
            StringBuilder builder = new StringBuilder(ResultValue);
            builder.AppendLine(value);
            ResultValue = builder.ToString();
            NotifyPropertyChanged(nameof(ResultValue));
        }

        private void ExportExecute()
        {
            string serializedConfig = JsonConvert.SerializeObject(this);

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save value";
            saveDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveDialog.FileName != "")
            {
                File.WriteAllText(saveDialog.FileName, serializedConfig);
            }
        }

        private void ImportExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open config";
            openFileDialog.Multiselect = false;
            openFileDialog.ShowDialog();

            if (!string.IsNullOrEmpty(openFileDialog.FileName))
            {
                var item = JsonConvert.DeserializeObject<AppFormViewModel>(File.ReadAllText(openFileDialog.FileName));

                this.AppID = item.AppID;
                this.DevicesID = item.DevicesID;
                this.AppPath = item.AppPath;
                this.Category = item.Category;
                this.TestPath = item.TestPath;
                this.TestSeries = item.TestSeries;
                this.Locale = item.Locale;
                this.Token = item.Token;

                NotifyPropertyChanged(nameof(AppID));
                NotifyPropertyChanged(nameof(DevicesID));
                NotifyPropertyChanged(nameof(AppPath));
                NotifyPropertyChanged(nameof(Category));
                NotifyPropertyChanged(nameof(TestPath));
                NotifyPropertyChanged(nameof(TestSeries));
                NotifyPropertyChanged(nameof(Locale));
                NotifyPropertyChanged(nameof(Token));
            }
        }

        #endregion
    }
}
