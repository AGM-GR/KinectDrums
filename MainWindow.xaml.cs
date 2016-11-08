namespace NPI.KinectDrums {

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using NPI.KinectDrums.DataModel;
    using System.ComponentModel;

    // Interaction logic for MainWindow
    public partial class MainWindow {

        public MainWindow() {

            this.InitializeComponent();

            KinectRegion.SetKinectRegion(this, kinectRegion);

            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Use the default sensor
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

            //// Add in display content
            var sampleDataSource = SampleDataSource.GetGroup("Group-1");
            this.itemsControl.ItemsSource = sampleDataSource;
        }


        // Handle the ButtonClick
        private void ButtonClick(object sender, RoutedEventArgs e) {
                       
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null) {

                navigationRegion.Content = Activator.CreateInstance(sampleDataItem.NavigationPage);
            }
        }

        // Handle the back button click.
        private void GoBack(object sender, RoutedEventArgs e) {

            if (navigationRegion.Content == this.kinectRegionGrid)
                this.Close();
            else if (navigationRegion.Content.GetType() == typeof(Play)) {
                Play playContent = navigationRegion.Content as Play;
                playContent.Play_Closing();
                navigationRegion.Content = this.kinectRegionGrid;
            } else
                navigationRegion.Content = this.kinectRegionGrid;
        }
    }
}
