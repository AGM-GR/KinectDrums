namespace NPI.KinectDrums {

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using NPI.KinectDrums.DataModel;

    public partial class Play : UserControl {

        // Radius of drawn hand circles
        private const double HandSize = 10;

        // Thickness of drawn joint lines
        private const double JointThickness = 3;

        // Constant for clamping Z values of camera space points from being negative
        private const float InferredZPositionClamp = 0.1f;

        // Brush used for drawing hands
        private readonly Brush handBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        // Brush used for drawing joints that are currently tracked
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        // Brush used for drawing joints that are currently inferred     
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        // Pen used for drawing bones that are currently inferred      
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        // Drawing group for body rendering output
        private DrawingGroup drawingGroup;

        // Drawing image that we will display
        private DrawingImage imageSource;

        // Active Kinect sensor
        private KinectSensor kinectSensor = null;

        // Coordinate mapper to map one type of point to another
        private CoordinateMapper coordinateMapper = null;

        // Reader for body frames
        private BodyFrameReader bodyFrameReader = null;

        // Array for the bodies
        private Body[] bodies = null;

        // definition of bones
        private List<Tuple<JointType, JointType>> bones;

        // Width of display (depth space)
        private int displayWidth;

        // Height of display (depth space)
        private int displayHeight;

        // List of colors for each body tracked
        private List<Pen> bodyColors;

        /*****************************************************************************************************************/
        //DrumKit variables
        private Drum bass;
        private Drum snare;
        private Drum middleTom;
        private Drum floorTom;
        private Drum crash;
        private Hihat hihat;
        /*****************************************************************************************************************/



        // Inicializa una instancia de la clase Play.
        public Play() {

            // Obtiene el sensor por defecto
            this.kinectSensor = KinectSensor.GetDefault();

            // Obtiene el mapa de coordenadas
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // Obtiene el tamaño de la imagen del sensor
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // Guarda el tamaño de la imagen del sensor
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // Abre el lector de BodyFrames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // Los huesos se definen como una linea entre dos Joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Brazo Derecho
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Brazo Izquierdo
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Pierna Derecha
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Pierna Izquierda
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // Lista de colores para cada Body dibujado
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            // Abre el sensor
            this.kinectSensor.Open();

            // Crea el drawingGroup sobre el que dibujaremos
            this.drawingGroup = new DrawingGroup();

            // Crea la imagen sobre la que vamos a trabajar
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Usa esta ventana como la vista del modelo
            this.DataContext = this;

            // Inicializa los componentes de la vista
            this.InitializeComponent();


            /*****************************************************************************************************************/
            // Inicializa la batería

            MediaPlayer player = new MediaPlayer();
            BitmapImage Image = new BitmapImage();
            double Reduction = 0;

            //Bass
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/Bass.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/Bass.wav", UriKind.Relative));
            Reduction = 2.2;

            bass = new Drum(
                new Rect((this.displayWidth - (Image.Width / Reduction)) / 2,
                          this.displayHeight - (Image.Height / Reduction),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth - (Image.Width / Reduction / 5)) / 2,
                          this.displayHeight - (Image.Height / Reduction / 10),
                          Image.Width / Reduction / 5, Image.Height / Reduction / 10),
                player,
                Image,
                Reduction,
                0 
            );

            //Snare
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/Snare.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/Snare.wav", UriKind.Relative));
            Reduction = 2;

            snare = new Drum(
                new Rect((this.displayWidth / 2) + (Image.Width / 8),
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) + (Image.Width / 8),
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)),
                          Image.Width / Reduction, Image.Height / Reduction / 2),
                player,
                Image,
                Reduction,
                1
            );


            //Middle Tom
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/MiddleTom.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/MidTom.wav", UriKind.Relative));
            Reduction = 8.5;

            middleTom = new Drum(
                new Rect((this.displayWidth / 2) - (bass.Width / 2),
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) + 11,
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)),
                          (Image.Width / Reduction) - 11, Image.Height / Reduction / 4),
                player,
                Image,
                Reduction,
                1
            );

            //Floor Tom
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/FloorTom.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/FloorTom.wav", UriKind.Relative));
            Reduction = 12;

            floorTom = new Drum(
                new Rect((this.displayWidth / 2) + (bass.Width / 2),
                          this.displayHeight - (Image.Height / Reduction),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) + (bass.Width / 2) + 11,
                          this.displayHeight - (Image.Height / Reduction),
                          (Image.Width / Reduction) - 20, Image.Height / Reduction / 6),
                player,
                Image,
                Reduction,
                1
            );

            //Crash
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/Crash.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/CrashCymbal.wav", UriKind.Relative));
            Reduction = 2;

            crash = new Drum(
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / 1.7),
                          this.displayHeight - (Image.Height / Reduction),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / 1.7) + 2,
                          this.displayHeight - (Image.Height / Reduction),
                          Image.Width / Reduction - 13, Image.Height / Reduction / 7.5),
                player,
                Image,
                Reduction,
                1
            );

            //Hihat
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/Hihat.png", UriKind.Absolute));
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/Hihat.wav", UriKind.Relative));
            Reduction = 5;

            hihat = new Hihat(
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction),
                          this.displayHeight - (Image.Height / Reduction),
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction) + 8,
                          this.displayHeight - (Image.Height / Reduction) + 16,
                          Image.Width / Reduction - 16, Image.Height / Reduction /6),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction / 2) - (Image.Width / Reduction / 4 / 2),
                          this.displayHeight - (Image.Height / Reduction / 12),
                          Image.Width / Reduction / 4, Image.Height / Reduction / 12),
                player,
                player,
                player,
                Image,
                Reduction
            );
            /*****************************************************************************************************************/
        }

        // Obtiene el bitmap
        public ImageSource ImageSource {
            get{
                return this.imageSource;
            }
        }

        // Inicia el Programa, iniciando la llegada de frames
        private void Play_Loaded(object sender, RoutedEventArgs e) {

            if (this.bodyFrameReader != null) {

                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;

                /*******************************************************/
                using (DrawingContext dc = this.drawingGroup.Open()) {

                    // Dibuja la batería
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    this.DrawDrums(dc);
                }
                /*******************************************************/
            }
        }

        // Funcion para cerrar correctamente el programa
        public void Play_Closing() {

            if (this.bodyFrameReader != null) {

                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
        }

        // Handles the body frame data arriving from the sensor
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {

            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame()) {

                if (bodyFrame != null) {

                    if (this.bodies == null) {

                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived) {

                using (DrawingContext dc = this.drawingGroup.Open()) {

                    /*******************************************************/
                    // Dibuja la batería
                    this.DrawDrums(dc);
                    /*******************************************************/

                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    foreach (Body body in this.bodies) {

                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked) {

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys) {

                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0) {

                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawCircle(body.HandLeftState, jointPoints[JointType.HandTipLeft], dc);
                            this.DrawCircle(body.HandRightState, jointPoints[JointType.HandTipRight], dc);
                            this.DrawCircle(body.HandLeftState, jointPoints[JointType.FootLeft], dc);
                            this.DrawCircle(body.HandRightState, jointPoints[JointType.FootRight], dc);

                            /*************************************************************************************************************/

                            //Comprueba si ha tocado un tambor
                            OnDrumHit(jointPoints[JointType.HandTipLeft], jointPoints[JointType.HandTipRight], jointPoints[JointType.FootLeft], jointPoints[JointType.FootRight]);

                            /*************************************************************************************************************/

                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        // Draws a body
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen) {

            // Draw the bones
            foreach (var bone in this.bones) {

                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys) {

                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked) {

                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred) {

                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null) {

                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        // Draws one bone of a body (joint to joint)
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen) {

            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked) {

                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked)) {

                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /*****************************************************************************************************************/
        // Dibuja un circulo en la mano
        private void DrawCircle(HandState handState, Point handPosition, DrawingContext drawingContext) {

            drawingContext.DrawEllipse(this.handBrush, null, handPosition, HandSize, HandSize);
        }

        //Dibuja la batería
        private void DrawDrums(DrawingContext drawingContext) {

            //Floor Tom
            floorTom.Draw(drawingContext);
            floorTom.DrawHit(drawingContext, this.handBrush);

            //Crash
            crash.Draw(drawingContext);
            crash.DrawHit(drawingContext, this.handBrush);

            //HiHat
            hihat.Draw(drawingContext);
            hihat.DrawHit(drawingContext, this.handBrush);

            //Bass
            bass.Draw(drawingContext);
            bass.DrawHit(drawingContext, this.handBrush);

            //Snare
            snare.Draw(drawingContext);
            snare.DrawHit(drawingContext, this.handBrush);

            //Middle Tom
            middleTom.Draw(drawingContext);
            middleTom.DrawHit(drawingContext, this.handBrush);
        }

        // Maneja cuando golpeas un tambor
        private void OnDrumHit(Point LeftHand, Point RightHand, Point LeftFoot, Point RightFoot) {

            // Bass
            bass.HitDrum(LeftFoot, RightFoot);

            // Snare
            snare.HitDrum(LeftHand, RightHand);

            //Middle Tom
            middleTom.HitDrum(LeftHand, RightHand);

            //Floor Tom
            floorTom.HitDrum(LeftHand, RightHand);

            //Crash
            crash.HitDrum(LeftHand, RightHand);

            // Hihat Hit
            hihat.HitDrum(LeftHand, RightHand, LeftFoot, RightFoot);
        }

        /*****************************************************************************************************************/
    }
}
