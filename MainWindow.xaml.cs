namespace NPI.KinectDrums {

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using NPI.KinectDrums.DataModel;

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


        private void ButtonClick(object sender, RoutedEventArgs e) {
                       
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null) {

                backButton.Visibility = System.Windows.Visibility.Visible;
                navigationRegion.Content = Activator.CreateInstance(sampleDataItem.NavigationPage);
            }
            else {

                var selectionDisplay = new SelectionDisplay(button.Content as string);
                this.kinectRegionGrid.Children.Add(selectionDisplay);

                // Selection dialog covers the entire interact-able area, so the current press interaction
                // should be completed. Otherwise hover capture will allow the button to be clicked again within
                // the same interaction (even whilst no longer visible).
                selectionDisplay.Focus();

                // Since the selection dialog covers the entire interact-able area, we should also complete
                // the current interaction of all other pointers.  This prevents other users interacting with elements
                // that are no longer visible.
                this.kinectRegion.InputPointerManager.CompleteGestures();

                e.Handled = true;
            }
        }

        // Handle the back button click.
        // <param name="sender">Event sender</param>
        // <param name="e">Event arguments</param>
        private void GoBack(object sender, RoutedEventArgs e) {

            backButton.Visibility = System.Windows.Visibility.Hidden;
            navigationRegion.Content = this.kinectRegionGrid;
        }
    }
}
