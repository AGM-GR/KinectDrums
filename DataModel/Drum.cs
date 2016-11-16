using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.ControlsBasics.DataModel {

    public class Drum {

        //Posición
        private Rect position;

        //Area de golpe
        private Rect hitArea;
        
        //Controla si se está golpeando
        private bool hitted = false;

        //Sonido
        private string soundSource = string.Empty;
        MediaPlayer sound = new MediaPlayer();

        //Imagen
        private string imageSource = string.Empty;
        private double imageReduction;
        private BitmapImage image = null;

        //True si se golpea con la mano, false si es con los pies
        private bool handHit = true;

        //Constructor
        public Drum (Rect position, Rect hitArea, string soundSource, string imageSource, double imageReduction, bool handHit) {

            this.position = position;
            this.hitArea = hitArea;
            this.soundSource = soundSource;
            this.imageSource = imageSource;
            this.imageReduction = imageReduction;
            this.handHit = handHit;
        }

        //Inicializa el drum
        public void initialize() {
            
            //Crea la imagen
            image = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));

            //Crea el sonido
            sound.Open(new Uri(soundSource, UriKind.RelativeOrAbsolute));
        }

        //Dibuja la imagen en la posicion.
        public void Draw (DrawingContext drawingContext) {

            drawingContext.DrawImage(image, position);
        }

        //Dibuja el hitArea de un color determinado.
        public void DrawHit (DrawingContext drawingContext, Brush color) {

            drawingContext.DrawRectangle(color, null, hitArea);
        }

        //Reproduce el sonido al golpear el tambor.
        public void HitDrum (Point LeftHitter, Point RightHitter) {

            if ((hitArea.Contains(LeftHitter) || hitArea.Contains(RightHitter)) && !hitted) {

                hitted = true;
                sound.Stop();
                sound.Play();

            }
            else if (!(hitArea.Contains(LeftHitter) || hitArea.Contains(RightHitter))) {

                hitted = false;
            }
        }

        //Modificadores y Consultores//
        public MediaPlayer Sound {
            get { return sound; }
        }

        public Rect Position {
            get { return position;  }
        }

        public BitmapImage Image {
            get { return image; }
        }

        public Rect HitArea {
            get { return hitArea; }
        }

        public bool Hited {
            get { return hitted; }
        }
    }
}
