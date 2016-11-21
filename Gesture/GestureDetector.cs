namespace NPI.KinectDrums.Gestures {

    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System.Windows.Media;

    public class GestureDetector : IDisposable {

        //Gesture frame source which should be tied to a body tracking ID
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        //Gesture frame reader which will handle gesture events coming from the sensor
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /********************************************************************************************/
        //Dirección de la base de datos con el gesto entrenado
        private readonly string gestureDatabase = @"Gesture/Database/Baquetas.gbd";

        //Nombre de el gesto guardado en la base de datos
        private readonly string gestureName = "Baquetas";

        //Reproductor con el sonido a reproducir al hacer el gesto
        private MediaPlayer sound = new MediaPlayer();
        private bool sonando = false;
        /********************************************************************************************/

        //Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        public GestureDetector(KinectSensor kinectSensor, MediaPlayer sound) {
            
            /************************************************/
            //Guarda el sonido
            this.sound = sound;
            /************************************************/

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null) {

                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the 'Baquetas' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase)) {

                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                foreach (Gesture gesture in database.AvailableGestures) {

                    if (gesture.Name.Equals(this.gestureName)) {

                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }
        }

        // Gets or sets the body tracking ID associated with the current detector
        // The tracking ID can change whenever a body comes in/out of scope
        public ulong TrackingId {

            get {

                return this.vgbFrameSource.TrackingId;
            }

            set {

                if (this.vgbFrameSource.TrackingId != value) {

                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        // Gets or sets a value indicating whether or not the detector is currently paused
        // If the body tracking ID associated with the detector is not valid, then the detector should be paused
        public bool IsPaused {

            get {

                return this.vgbFrameReader.IsPaused;
            }

            set {

                if (this.vgbFrameReader.IsPaused != value) {

                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        // Disposes all unmanaged resources for the class
        public void Dispose() {

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        protected virtual void Dispose(bool disposing) {

            if (disposing) {

                if (this.vgbFrameReader != null) {

                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null) {

                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        // Handles gesture detection results arriving from the sensor for the associated body tracking Id
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e) {

            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame()) {

                if (frame != null) {

                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null) {

                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures) {

                            if (gesture.Name.Equals(this.gestureName) && gesture.GestureType == GestureType.Discrete) {

                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null) {

                                    /**********************************************************************************************/
                                    //Cuando el gesto es reconocido reproduce el sonido
                                    this.ReproducirSonido(result.Detected, result.Confidence);
                                    /**********************************************************************************************/
                                }
                            }
                        }
                    }
                }
            }
        }

        /********************************************************************************************/
        //Función que reproduce el sonido al detectar el gesto con un grado de confianza determinado
        private void ReproducirSonido(bool detected, double confidence) {

            if (detected && confidence > 0.98 && !sonando) {

                this.sonando = true;
                this.sound.Stop();
                this.sound.Play();
            }

            else if (!detected && sonando) {

                this.sonando = false;
            }
        }
        /********************************************************************************************/
    }
}
