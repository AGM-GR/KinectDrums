namespace NPI.KinectDrums.DataModel {

    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class Drum {

        //Posición
        private Rect position = new Rect();

        //Area de golpe
        private Rect hitArea = new Rect();
        
        //Controla si se está golpeando
        private bool hit = false;

        //Sonido
        MediaPlayer sound = new MediaPlayer();

        //Imagen
        private BitmapImage image = null;
        private double imageReduction = 0;

        //1 si se golpea con la mano, 0 si es con los pies, 2 si puede ser con ambos
        private int handHit = 1;

        //Constructor
        public Drum (Rect position, Rect hitArea, MediaPlayer sound, BitmapImage image, double imageReduction, int handHit) {

            this.position = position;
            this.hitArea = hitArea;
            this.sound = sound;
            this.image = image;
            this.imageReduction = imageReduction;
            this.handHit = handHit;
        }

        //Constructor de copia
        public Drum(Drum copy) {

            this.position = copy.position;
            this.hitArea = copy.hitArea;
            this.sound = copy.sound;
            this.image = copy.image;
            this.imageReduction = copy.imageReduction;
            this.handHit = copy.handHit;
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

            if ((hitArea.Contains(LeftHitter) || hitArea.Contains(RightHitter)) && !hit) {

                hit = true;
                sound.Stop();
                sound.Play();

            }
            else if (!(hitArea.Contains(LeftHitter) || hitArea.Contains(RightHitter))) {

                hit = false;
            }
        }

        public void MoveTo(Point point) {

            double distanciaX = hitArea.Location.X - position.Location.X;
            double distanciaY = hitArea.Location.Y - position.Location.Y;

            position.Location = new Point(point.X - (Width/2), point.Y - (Height/2));
            hitArea.Location = new Point(position.Location.X + distanciaX, position.Location.Y + distanciaY);

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

        public bool Hit {
            get { return hit; }
        }

        public int HandHit {
            get { return handHit; }
        }
    }

    public class Hihat : Drum {

        //Imagen del plato del hihat
        private Rect hitCrash = new Rect();

        //Sonidos del hihat abierto y cerrado
        MediaPlayer soundOpen = new MediaPlayer();
        MediaPlayer soundClosed = new MediaPlayer();

        //Controla si se está golpeando
        private bool crashHit = false;

        //Constructor, establece la clase padre como el pedal y añade el plato y los sonidos del plato
        public Hihat(Rect position, Rect hitCrash, Rect hitPedal, MediaPlayer soundOpen, MediaPlayer soundClosed, MediaPlayer soundPedal, BitmapImage image, double imageReduction) 
            : base(position,hitPedal,soundPedal,image,imageReduction,0) {

            this.hitCrash = hitCrash;
            this.soundClosed = soundClosed;
            this.soundOpen = soundOpen;
        }

        //Constructor de copia
        public Hihat(Hihat copy) : base(copy) {

            this.hitCrash = copy.hitCrash;
            this.soundClosed = copy.soundClosed;
            this.soundOpen = copy.soundOpen;
        }

        //Dibuja el hitArea de un color determinado.
        new public void DrawHit(DrawingContext drawingContext, Brush color) {

            base.DrawHit(drawingContext, color);
            drawingContext.DrawRectangle(color, null, hitCrash);
        }

        //Reproduce el sonido al golpear el tambor.
        public void HitDrum(Point LeftHand, Point RightHand, Point LeftFoot, Point RightFoot) {

            base.HitDrum (LeftFoot, RightFoot);

            if ((hitCrash.Contains(LeftHand) || hitCrash.Contains(RightHand)) && !crashHit) {

                crashHit = true;
                if (base.Hit) {

                    soundClosed.Stop();
                    soundClosed.Play();
                }
                else {

                    soundOpen.Stop();
                    soundOpen.Play();
                }

            }
            else if (!(hitCrash.Contains(LeftHand) || hitCrash.Contains(RightHand))) {

                crashHit = false;
            }
        }

        //Modificadores y Consultores//
        public MediaPlayer SoundPedal {
            get { return base.Sound; }
        }

        public MediaPlayer SoundClosed {
            get { return soundClosed; }
        }

        public MediaPlayer SoundOpen {
            get { return soundOpen; }
        }

        new public int HandHit {
            get { return 2; }
        }
    }
}
