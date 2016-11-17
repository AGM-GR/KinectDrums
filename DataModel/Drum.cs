namespace NPI.KinectDrums.DataModel {
    using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

    public class Drum {

        //Posición
        private Rect position;

        //Area de golpe
        private Rect hitArea;
        
        //Controla si se está golpeando
        private bool hitted = false;

        //Sonido
        MediaPlayer sound = new MediaPlayer();

        //Imagen
        private BitmapImage image = null;
        private double imageReduction;

        //True si se golpea con la mano, false si es con los pies
        private bool handHit = true;

        //Constructor
        public Drum (Rect position, Rect hitArea, MediaPlayer sound, BitmapImage image, double imageReduction, bool handHit) {

            this.position = position;
            this.hitArea = hitArea;
            this.sound = sound;
            this.image = image;
            this.imageReduction = imageReduction;
            this.handHit = handHit;
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
            set { this.position = value; }
        }

        public BitmapImage Image {
            get { return image; }
        }

        public double Width {
            get { return (image.Width / imageReduction); }
        }

        public double Height {
            get { return (image.Height / imageReduction); }
        }

        public Rect HitArea {
            get { return hitArea; }
        }

        public bool Hited {
            get { return hitted; }
        }
    }
}
