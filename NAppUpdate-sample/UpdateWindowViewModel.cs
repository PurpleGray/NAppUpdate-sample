using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate_sample.Annotations;
using NAppUpdate_sample.Utils;

namespace NAppUpdate_sample
{
    public class UpdateWindowViewModel : INotifyPropertyChanged
    {
        private readonly UpdateManager updateManager;
        private IList<UpdateTaskInfo> updates;
        private int _downloadProgress;

        public UpdateTaskHelper Helper { get; set; }

        public ImageSource Icon { get; set; }

        public bool DownloadActive { get; set; }

        public Action Close { get; set; }

        private ICommand installNow;

        public ICommand InstallNow
        {
            get
            {
                if (installNow == null)
                {
                    installNow = new RelayCommand(p => true, p =>
                    {
                        DownloadActive = true;
                        // dummy time delay for demonstration purposes
                        var t = new System.Timers.Timer(2000) { AutoReset = false };
                        t.Start();
                        while (t.Enabled) { DoEvents(); }

                        updateManager.BeginPrepareUpdates(new AsyncCallback(asyncResult =>
                            {
                                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                                // ApplyUpdates is a synchronous method by design. Make sure to save all user work before calling
                                // it as it might restart your application
                                // get out of the way so the console window isn't obstructed
                                try
                                {
                                    updateManager.ApplyUpdates(true);

                                    if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        Close();
                                    }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(new Action(Close));
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("An error occurred while trying to install software updates");
                                }
                                finally
                                {
                                    updateManager.CleanUp();
                                }
                            }), null);
                    });
                }

                return installNow;
            }
        }

        private ICommand installWhenExit;

        public ICommand InstallWhenExit
        {
            get
            {
                if (installWhenExit == null)
                {
                    installWhenExit = new RelayCommand(p => true, p => Close?.Invoke());
                }

                return installWhenExit;
            }
        }

        static void DoEvents()
        {
            var frame = new DispatcherFrame(true);
            Application.Current.Dispatcher.BeginInvoke
            (
                DispatcherPriority.Background,
                (System.Threading.SendOrPostCallback)delegate (object arg)
                {
                    var f = arg as DispatcherFrame;
                    if (f != null) f.Continue = false;
                },
                frame
            );
            Dispatcher.PushFrame(frame);
        }

        public UpdateWindowViewModel()
        {
            updateManager = UpdateManager.Instance;
            Helper = new UpdateTaskHelper();

            var iconStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NAppUpdate.Framework.updateicon.ico");
            if (iconStream != null)
                this.Icon = new IconBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}