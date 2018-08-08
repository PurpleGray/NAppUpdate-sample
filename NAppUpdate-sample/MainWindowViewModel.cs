using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Sources;
using NAppUpdate_sample.Annotations;
using NAppUpdate_sample.Utils;

namespace NAppUpdate_sample
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool applyUpdates;

        public Action CloseAction { get; set; }

        public string AppVersion
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"App Version: {fvi.FileVersion}";
            }
        }

        private ICommand checkUpdates;

        public ICommand CheckUpdates
        {
            get
            {
                if (checkUpdates == null)
                {
                    checkUpdates = new RelayCommand(p => true, p =>
                    {
                        UpdateManager updManager = UpdateManager.Instance;

                        updManager.BeginCheckForUpdates(asyncResult =>
                        {
                            Action showUpdateAction = ShowUpdateWindow;

                            if (asyncResult.IsCompleted)
                            {
                                // still need to check for caught exceptions if any and rethrow
                                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                                // No updates were found, or an error has occured. We might want to check that...
                                if (updManager.UpdatesAvailable == 0)
                                {
                                    MessageBox.Show("All is up to date!");
                                    return;
                                }
                            }

                            applyUpdates = true;

                            Application.Current.Dispatcher.Invoke(showUpdateAction);
                        }, null);
                    });
                }

                return checkUpdates;
            }
        }

        private void ShowUpdateWindow()
        {
            var updateWindow = new UpdateWindow();

            updateWindow.Closed += (sender, e) =>
            {
                if (UpdateManager.Instance.State == UpdateManager.UpdateProcessState.AppliedSuccessfully)
                {
                    applyUpdates = false;

                    // Update the app version
                    OnPropertyChanged("AppVersion");
                }
            };

            updateWindow.Show();
        }


        private NAppUpdate.Framework.Sources.IUpdateSource PrepareUpdateSource()
        {
            var source = new FtpSource("193.232.26.47", @"/DATA/Projects/___ПО/test_app/upd.xml", "sergei", "O2P8pL1Y");
            return source;
        }

        public MainWindowViewModel()
        {
            var updManager = UpdateManager.Instance;
            updManager.UpdateSource = PrepareUpdateSource();
            updManager.ReinstateIfRestarted();

            CloseAction = () =>
            {
                if (applyUpdates)
                {
                    try
                    {
                        UpdateManager.Instance.PrepareUpdates();
                    }
                    catch
                    {
                        UpdateManager.Instance.CleanUp();
                        return;
                    }
                    UpdateManager.Instance.ApplyUpdates(false);
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}