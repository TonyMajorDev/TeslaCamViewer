using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Collections.Generic;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace TeslaCamViewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TeslaCamDirectoryCollection> ListItems { get; set; }
        public TeslaCamFileSet CurrentPlaybackFile { get; set; }

        private GridLength _TopVideoRowHeight;
        public GridLength TopVideoRowHeight
        {
            get
            {
                return this._TopVideoRowHeight;
            }
            set
            {
                if (value != this._TopVideoRowHeight)
                {
                    this._TopVideoRowHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private GridLength _BottomVideoRowHeight;
        public GridLength BottomVideoRowHeight
        {
            get
            {
                return this._BottomVideoRowHeight;
            }
            set
            {
                if (value != this._BottomVideoRowHeight)
                {
                    this._BottomVideoRowHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private GridLength _LeftVideoColumnWidth;
        public GridLength LeftVideoColumnWidth
        {
            get
            {
                return this._LeftVideoColumnWidth;
            }
            set
            {
                if (value != this._LeftVideoColumnWidth)
                {
                    this._LeftVideoColumnWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private GridLength _RightVideoColumnWidth;
        public GridLength RightVideoColumnWidth
        {
            get
            {
                return this._RightVideoColumnWidth;
            }
            set
            {
                if (value != this._RightVideoColumnWidth)
                {
                    this._RightVideoColumnWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double DisplayPlaybackSpeed
        {
            get
            {
                return Properties.Settings.Default.PlaybackSpeed;
            }
            set
            {
                if (value != Properties.Settings.Default.PlaybackSpeed)
                {
                    Properties.Settings.Default.PlaybackSpeed = value;
                    Properties.Settings.Default.Save();

                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CalculatedPlaybackSpeed");
                }
            }
        }
        public double CalculatedPlaybackSpeed
        {
            get
            {
                if (DisplayPlaybackSpeed < 0)
                {
                    double calculatedMin = 0.25;
                    double calculatedMax = 1.00;
                    double displayMin = -50;
                    double displayMax = 0;

                    double calc = (calculatedMax - calculatedMin) / (displayMax - displayMin) * (DisplayPlaybackSpeed - displayMax) + calculatedMax;
                    return calc;
                }
                else
                    return this.DisplayPlaybackSpeed + 1.0;
            }
            set
            {
            }
        }

        private string _LeftStatusText;
        public string LeftStatusText
        {
            get
            {
                return this._LeftStatusText;
            }

            set
            {
                if (value != this._LeftStatusText)
                {
                    this._LeftStatusText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string _RightStatusText;
        public string RightStatusText
        {
            get
            {
                return this._RightStatusText;
            }

            set
            {
                if (value != this._RightStatusText)
                {
                    this._RightStatusText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool EnableAutoSearch
        {
            get
            {
                return Properties.Settings.Default.EnableAutoSearch;
            }
            set
            {
                Properties.Settings.Default.EnableAutoSearch = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool EnableAutoPlaylist
        {
            get
            {
                return Properties.Settings.Default.EnableAutoPlaylist;
            }
            set
            {
                Properties.Settings.Default.EnableAutoPlaylist = value;
                Properties.Settings.Default.Save();
            }
        }
        public VideoViewModel VideoModel { get; set; }
        public MainWindowViewModel()
        {
            this.ListItems = new ObservableCollection<TeslaCamDirectoryCollection>();
            this.VideoModel = new VideoViewModel();

            ResetVideoDisplay();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand Delete3 { get { return new DelegateCommand(DeleteVideos); } }
        public ICommand DeleteSet { get { return new DelegateCommand(DeleteVideos); } }
        public ICommand Archive3 { get { return new DelegateCommand(ArchiveVideos); } }

        private void DeleteVideos(object whichVideos)
        {
            // Skip to the next video, then 
            var deleteSet = new List<string>();

            foreach (var aCamera in this.CurrentPlaybackFile.Cameras) deleteSet.Add(aCamera.FilePath);

            // skip to next video here
            

            //foreach (var aFile in deleteSet) File.Delete(aFile);
            
            //File.Delete(aCamera.FilePath);

            System.Diagnostics.Debug.WriteLine(whichVideos);

            //MessageBox.Show("Feature is not yet implemented. ");
        }

        private void ArchiveVideos(object whichVideos)
        {
            //MessageBox.Show("Feature is not yet implemented. ");

            System.Diagnostics.Debug.WriteLine(whichVideos);
        }


        public ICommand SelectFullVideo { get { return new DelegateCommand(DisplayFullVideo); } }
        private TeslaCamFile.CameraType CurrentFullVideo;

        private void DisplayFullVideo(object Camera)
        {
            if (Camera is TeslaCamFile.CameraType)
            {
                if (CurrentFullVideo == TeslaCamFile.CameraType.UNKNOWN)
                {
                    var cam = (TeslaCamFile.CameraType)Camera;
                    if (cam == TeslaCamFile.CameraType.FRONT)
                    {
                        this.BottomVideoRowHeight = new GridLength(0);
                        this.TopVideoRowHeight = new GridLength(1, GridUnitType.Star);
                    }
                    if (cam == TeslaCamFile.CameraType.LEFT_REPEATER)
                    {
                        this.TopVideoRowHeight = new GridLength(0);
                        this.BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
                        this.LeftVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                        this.RightVideoColumnWidth = new GridLength(0);
                    }
                    if (cam == TeslaCamFile.CameraType.RIGHT_REPEATER)
                    {
                        this.TopVideoRowHeight = new GridLength(0);
                        this.BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
                        this.RightVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                        this.LeftVideoColumnWidth = new GridLength(0);
                    }
                    CurrentFullVideo = cam;
                }
                else
                {
                    ResetVideoDisplay();
                    CurrentFullVideo = TeslaCamFile.CameraType.UNKNOWN;
                }
            }
        }
        private void ResetVideoDisplay()
        {
            this.TopVideoRowHeight = new GridLength(1, GridUnitType.Star);
            this.BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
            this.LeftVideoColumnWidth = new GridLength(1, GridUnitType.Star);
            this.RightVideoColumnWidth = new GridLength(1, GridUnitType.Star);
        }
        public void LoadFileSet(TeslaCamFileSet set)
        {
            this.VideoModel.LoadFileSet(set);
            this.CurrentPlaybackFile = set;
        }
    }
}
