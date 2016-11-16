namespace NPI.KinectDrums {

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using NPI.KinectDrums.DataModel;
    using System.ComponentModel;

    //Venta principal de la aplicación
    public partial class MainWindow {

        public MainWindow() {

            this.InitializeComponent();

            KinectRegion.SetKinectRegion(this, kinectRegion);

            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Usa el sensor por defecto
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

            // Añade los botones a la venta principal
            var sampleDataSource = SampleDataSource.GetGroup("Group-1");
            this.itemsControl.ItemsSource = sampleDataSource;
        }


        // Maneja que hacer al pulsar un botón: ButtonClick
        private void ButtonClick(object sender, RoutedEventArgs e) {
                       
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null) {

                navigationRegion.Content = Activator.CreateInstance(sampleDataItem.NavigationPage);
            }
            else if (sampleDataItem != null) {

                GoBack(this,null);
            }
        }

        // Maneja el funcionamiento del botón GoBack.
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

        // Maneja el cierre del programa, para una correcta finalización.
        private void MainWindow_Closing(object sender, CancelEventArgs e) {

            if (this.kinectRegion.KinectSensor != null) {

                this.kinectRegion.KinectSensor.Close();
                this.kinectRegion.KinectSensor = null;
            }
        }
    }
}
