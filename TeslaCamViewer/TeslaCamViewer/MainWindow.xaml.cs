﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Deployment.Application;

namespace TeslaCamViewer
{
    public class VideoViewModel
    {
        public MediaElement left;
        public MediaElement right;
        public MediaElement front;
        public TabControl tabs;

        public void LoadFileSet(TeslaCamFileSet set)
        {
            left.Stop();
            right.Stop();
            front.Stop();

            bool playLeft = false;
            bool playRight = false;
            bool playFront = false;

            foreach (var cam in set.Cameras)
            {
                if (cam.CameraLocation == TeslaCamFile.CameraType.FRONT)
                {
                    this.front.Source = new Uri(cam.FilePath);
                    playFront = true;
                }
                if (cam.CameraLocation == TeslaCamFile.CameraType.LEFT_REPEATER)
                {
                    this.left.Source = new Uri(cam.FilePath);
                    playLeft = true;
                }
                if (cam.CameraLocation == TeslaCamFile.CameraType.RIGHT_REPEATER)
                {
                    this.right.Source = new Uri(cam.FilePath);
                    playRight = true;
                }
            }

            if (playLeft) left.Play();
            if (playRight) right.Play();
            if (playFront) front.Play();
            
            //playPause_Button.Content = "4";  // 4 is the pause symbol in webdings font


            this.tabs.SelectedIndex = 1;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel model;
        private TimeSpan TotalTime;
        private bool paused;

        public MainWindow()
        {
            this.model = new MainWindowViewModel();

            this.DataContext = model;

            this.model.LeftStatusText = "Ready";
            InitializeComponent();

            this.Title += " " + DeployedVersion();

            model.VideoModel.left = this.left;
            model.VideoModel.right = this.right;
            model.VideoModel.front = this.front;
            model.VideoModel.tabs = this.tabs;

                        playbackSpeed_Slider_DragDelta(null, null);
        }


        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if ((sender as MediaElement).NaturalDuration.HasTimeSpan)
                if ((sender as MediaElement).NaturalDuration.TimeSpan.TotalSeconds > 0)
                    TotalTime = (sender as MediaElement).NaturalDuration.TimeSpan;

            var timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromMilliseconds(100);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (left.NaturalDuration.HasTimeSpan)
                if (left.NaturalDuration.TimeSpan.TotalSeconds > 0)
                    if (TotalTime.TotalSeconds > 0)
                    {
                        model.RightStatusText = left.Position.ToString(@"mm\:ss") + " / " + TotalTime.ToString(@"mm\:ss");
                        timeSlider.Value = left.Position.TotalSeconds / TotalTime.TotalSeconds;
                    }
        }
        private void SetPosition()
        {
            if (TotalTime.TotalSeconds > 0)
            {
                left.Position = TimeSpan.FromSeconds(timeSlider.Value * TotalTime.TotalSeconds);
                right.Position = TimeSpan.FromSeconds(timeSlider.Value * TotalTime.TotalSeconds);
                front.Position = TimeSpan.FromSeconds(timeSlider.Value * TotalTime.TotalSeconds);
            }
        }

        private void timeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            left.Pause();
            right.Pause();
            front.Pause();

            playPause_Button.Content = "4";  // 4 is the pause symbol in webdings font
        }

        private void timeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            left.Play();
            right.Play();
            front.Play();

            playPause_Button.Content = ";";  // ; is the pause symbol in webdings font

            System.Diagnostics.Debug.WriteLine("timeSlider_DragCompleted.  playing");
        }

        private void timeSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            this.SetPosition();
        }
        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    return;
                }
                if (treeview.SelectedItem is TeslaCamFileSet)
                {
                    var set = treeview.SelectedItem as TeslaCamFileSet;
                    model.LoadFileSet(set);
                }
            }

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var f = files[0];
                FileAttributes attr = File.GetAttributes(f);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    this.model.ListItems.Clear();
                    var c = new TeslaCamDirectoryCollection();
                    c.BuildFromBaseDirectory(f);
                    this.model.ListItems.Add(c);
                    this.model.LeftStatusText = "Location: " + f;
                    this.browseFrame.Navigate(new TeslaCamViewer.Views.RootCollectionView(this.model));
                }
            }
        }
        private async Task TeslaCamSearchAsync()
        {
            try
            {
                // Update Status
                this.model.LeftStatusText = "Searching for TeslaCam ...";

                // Placeholder variables used during and after worker task
                DirectoryInfo teslaCamDir = null;
                TeslaCamDirectoryCollection recentClips = null;
                TeslaCamDirectoryCollection savedClips = null;

                // Run the following in a worker thread and wait for it to finish
                await Task.Run(() =>
                {
                    // Get all drives
                    var drives = System.IO.DriveInfo.GetDrives();
                    //drives = drives.Where(e => e.DriveType == DriveType.Removable ||
                    //    e.DriveType == DriveType.Network ||
                    //    e.DriveType == DriveType.Fixed).ToArray();

                    drives = drives.Where(e => e.DriveType == DriveType.Removable && e.IsReady ||
                        e.DriveType == DriveType.Network && e.IsReady ||
                        e.DriveType == DriveType.Fixed && e.IsReady).ToArray();

                    // Find the first drive containing a TeslaCam folder and select that folder
                    teslaCamDir = (from drive in drives
                              let dirs = drive.RootDirectory.GetDirectories()
                              from dir in dirs
                              where dir.Name == "TeslaCam"
                              select dir).FirstOrDefault();

                    // If root is found load Recent and Saved
                    if (teslaCamDir != null)
                    {
                        // Get child dirs
                        var recentClipsDir = teslaCamDir.GetDirectories().FirstOrDefault(e => e.Name == "RecentClips");
                        var savedClipsDir = teslaCamDir.GetDirectories().FirstOrDefault(e => e.Name == "SavedClips");

                        // Load if found
                        if (recentClipsDir != null)
                        {
                            recentClips = new TeslaCamDirectoryCollection();
                            recentClips.BuildFromBaseDirectory(recentClipsDir.FullName);
                            recentClips.SetDisplayName("Recent Clips");
                        }
                        if (savedClipsDir != null)
                        {
                            savedClips = new TeslaCamDirectoryCollection();
                            savedClips.BuildFromBaseDirectory(savedClipsDir.FullName);
                            savedClips.SetDisplayName("Saved Clips");
                        }
                    }
                });

                // Do finial UI updating back on main thread
                if (teslaCamDir != null)
                {
                    // Update status to show drive was found
                    this.model.LeftStatusText = "Location: " + teslaCamDir.FullName;

                    // Add clips to UI tree
                    if (recentClips != null) { this.model.ListItems.Add(recentClips); }
                    if (savedClips != null) { this.model.ListItems.Add(savedClips); }

                    // Navigate
                    this.browseFrame.Navigate(new TeslaCamViewer.Views.RootCollectionView(this.model));
                }
                else
                {
                    // Update status to show that drive could not be found
                    this.model.LeftStatusText = "Ready";
                    await this.ShowMessageAsync("TeslaCam Drive Not Found", "A TeslaCam drive could not automatically be found. Drag a folder or file to start playing.");
                }
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("Could not load TeslaCam Drive", "An error ocurred: " + ex.Message).Wait();
            }
        }

        private async void teslaCamSearch_Menu_Click(object sender, RoutedEventArgs e)
        {
            this.model.ListItems.Clear();
            await TeslaCamSearchAsync();
        }

        private void playPause_Button_Click(object sender, RoutedEventArgs e)
        {
            if (paused)
            {
                left.Play();
                right.Play();
                front.Play();
                (sender as Button).Content = ";";  // ; is the pause symbol in webdings font
            }
            else
            {
                left.Pause();
                right.Pause();
                front.Pause();
                (sender as Button).Content = "4";  // 4 is the play symbol in webdings font
            }
            paused = !paused;
        }

        private void exit_Menu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MetroWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                playPause_Button_Click(sender, null);
            if (e.Key == Key.F)
            {
                fullscreen_Menu.IsChecked = !fullscreen_Menu.IsChecked;
                SetFullscreen(fullscreen_Menu.IsChecked);
            }
            if (e.Key == Key.Escape)
            {
                if (fullscreen_Menu.IsChecked)
                {
                    fullscreen_Menu.IsChecked = !fullscreen_Menu.IsChecked;
                    SetFullscreen(fullscreen_Menu.IsChecked);
                }
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (model.EnableAutoSearch)
            {
                await this.TeslaCamSearchAsync();
            }
        }

        private string DeployedVersion()
        {
            string version = "v0";

            if (ApplicationDeployment.IsNetworkDeployed)
                version = "v" + ApplicationDeployment.CurrentDeployment.CurrentVersion.Major + "." + ApplicationDeployment.CurrentDeployment.CurrentVersion.Minor + "." + ApplicationDeployment.CurrentDeployment.CurrentVersion.Build + "." + ApplicationDeployment.CurrentDeployment.CurrentVersion.Revision;

            return version;
        }
        

        private void about_Menu_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMessageAsync("TeslaCam Viewer " + DeployedVersion(), "TeslaCam Viewer " + DeployedVersion() + " Copyright 2019 TonyMajorDev\n\nBased on TeslaCamViewer by Mattw01\nSee LICENCES.txt for more information.");
        }

        private void viewOnGitHub_Menu_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/TonyMajorDev/TeslaCamViewer");
        }

        private void SetFullscreen(bool Enable)
        {
            if (Enable)
            {
                this.SetCurrentValue(IgnoreTaskbarOnMaximizeProperty, true);
                this.SetCurrentValue(WindowStateProperty, WindowState.Maximized);
                this.SetCurrentValue(UseNoneWindowStyleProperty, true);
            }
            else
            {
                this.SetCurrentValue(WindowStateProperty, WindowState.Normal);
                this.SetCurrentValue(UseNoneWindowStyleProperty, false);
                this.SetCurrentValue(ShowTitleBarProperty, true);
                this.SetCurrentValue(IgnoreTaskbarOnMaximizeProperty, false);
            }
        }

        private void fullscreen_Menu_Click(object sender, RoutedEventArgs e)
        {
            SetFullscreen(fullscreen_Menu.IsChecked);
        }
        private void ShowWelcomeMessage()
        {
            this.ShowMessageAsync("Welcome to TeslaCam Viewer!", "Getting Started:\n\nBrowse TeslaCam media in the left pane. " +
                "TeslaCam drive will automatically be detected on startup, or drag a folder containing TeslaCam data anywhere onto the window. " +
                "Double click event in TeslaCam Files pane to start playing.");
        }

        private void left_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (model.EnableAutoPlaylist)
                {
                    TeslaCamFileSet target = this.model.CurrentPlaybackFile;
                    TeslaCamEventCollection f = model.ListItems.SelectMany(d => d.Events).Where(d => d.Recordings.Contains(target)).First();
                    if (f != null)
                    {
                        int currentFileIndex = f.Recordings.IndexOf(target);
                        if (f.Recordings.Count - 1 > currentFileIndex)
                        {
                            TeslaCamFileSet nextSet = f.Recordings[currentFileIndex + 1];

                            model.LoadFileSet(nextSet);
                            var tvi = FindTviFromObjectRecursive(treeview, nextSet);

                            if (tvi != null)
                            {
                                tvi.IsSelected = true;
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public static TreeViewItem FindTviFromObjectRecursive(ItemsControl ic, object o)
        {
            if (ic == null)
                return null;

            TreeViewItem tvi = ic.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
            if (tvi != null) return tvi;
            foreach (object i in ic.Items)
            {
                TreeViewItem tvi2 = ic.ItemContainerGenerator.ContainerFromItem(i) as TreeViewItem;
                tvi = FindTviFromObjectRecursive(tvi2, o);
                if (tvi != null) return tvi;
            }
            return null;
        }

        private void playbackSpeed_Slider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            this.left.SpeedRatio = model.CalculatedPlaybackSpeed;
            this.right.SpeedRatio = model.CalculatedPlaybackSpeed;
            this.front.SpeedRatio = model.CalculatedPlaybackSpeed;
        }



        private async void NotImplemented_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("Not Implemented", "Feature is not yet implemented. ");
        }
    }
}
