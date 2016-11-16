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

        //True si se golpea con la mano, false si es con los pies
        private bool handHit = true;

        //Constructor
        public Drum (Rect position, Rect hitArea, MediaPlayer sound, BitmapImage image, bool handHit) {

            this.position = position;
            this.hitArea = hitArea;
            this.sound = sound;
            this.image = image;
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
