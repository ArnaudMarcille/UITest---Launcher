using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
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
            try
            {
                PowerShell ps = PowerShell.Create();
                var loginCommand = ps.AddCommand("appcenter")
                   .AddArgument("login")
                   .AddParameter("--token", Token)
                   .Invoke();

                foreach (PSObject result in loginCommand)
                {
                    UpdateResultValue(result.ToString());
                }

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
                    .Invoke();

                foreach (PSObject result in testCommand)
                {
                    UpdateResultValue(result.ToString());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
