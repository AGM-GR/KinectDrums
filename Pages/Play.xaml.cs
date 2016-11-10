namespace NPI.KinectDrums {

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;

    public partial class Play : UserControl {

        // Radius of drawn hand circles
        private const double HandSize = 10;

        // Thickness of drawn joint lines
        private const double JointThickness = 3;

        // Constant for clamping Z values of camera space points from being negative
        private const float InferredZPositionClamp = 0.1f;

        // Brush used for drawing hands that are currently tracked as closed
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        // Brush used for drawing hands that are currently tracked as opened
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        // Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        // Brush used for drawing hands
        private readonly Brush handBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

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


        /**********************************************************************************************************************************/
        // Drum elements players
        private string exePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
        private string exeDir;
        MediaPlayer playerBass = new MediaPlayer();
        
        MediaPlayer playerSnare = new MediaPlayer();
        
        MediaPlayer playerHihat = new MediaPlayer();
        

        /**********************************************************************************************************************************/
        //DrumsKit variables
        private BitmapImage bass = new BitmapImage(new Uri("pack://application:,,,/Images/bass.png", UriKind.Absolute));
        double bassReduction = 2.2;
        Rect bassHitRect;
        bool bassHit = false;

        BitmapImage snare = new BitmapImage(new Uri("pack://application:,,,/Images/snare.png", UriKind.Absolute));
        double snareReduction = 5.6;
        Rect snareHitRect;
        bool snareHit = false;

        Rect hihatHitRect;
        bool hihatHit = false;
        /**********************************************************************************************************************************/



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

            //////////////////////////////////////////
            bassHitRect = new Rect((this.displayWidth - (bass.Width / bassReduction / 5)) / 2 - 15,
                                    this.displayHeight - (bass.Height / bassReduction / 5),
                                    bass.Width / bassReduction / 3, bass.Height / bassReduction / 10);
            snareHitRect = new Rect((this.displayWidth / 2) + (bass.Width / bassReduction / 3),
                                    this.displayHeight - ((bass.Height / bassReduction) + (snare.Height / snareReduction / 2)),
                                    snare.Width / snareReduction, snare.Height / snareReduction / 2);
            hihatHitRect = new Rect((this.displayWidth / 4) + (bass.Width / bassReduction / 10),
                                    this.displayHeight - ((bass.Height / bassReduction * 2) + (snare.Height / snareReduction / 5)),
                                    snare.Width / snareReduction, snare.Height / snareReduction / 2);
            //////////////////////////////////////////
            exeDir = System.IO.Path.GetDirectoryName(exePath);
            playerBass.Open(new Uri(exeDir + "\\Sounds\\Bass.wav"));
            playerSnare.Open(new Uri(exeDir + "\\Sounds\\Snare.wav"));
            playerHihat.Open(new Uri(exeDir + "\\Sounds\\Hihat.wav"));
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
            }

            else

                using (DrawingContext dc = this.drawingGroup.Open()) {

                    // Dibuja la batería
                    this.DrawDrums(dc);
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
        // <param name="sender">object sending the event</param>
        // <param name="e">event arguments</param>
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

                    // Dibuja la batería
                    this.DrawDrums(dc);

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

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandTipLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandTipRight], dc);
                            this.DrawHand(body.HandLeftState, jointPoints[JointType.FootLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.FootRight], dc);

                            //Comprueba si ha tocado un tambor
                            OnDrumHit(jointPoints[JointType.HandTipLeft], jointPoints[JointType.HandTipRight], jointPoints[JointType.FootLeft], jointPoints[JointType.FootRight]);

                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        // Draws a body
        // <param name="joints">joints to draw</param>
        // <param name="jointPoints">translated positions of joints to draw</param>
        // <param name="drawingContext">drawing context to draw to</param>
        // <param name="drawingPen">specifies color to draw a specific body</param>
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
        // <param name="joints">joints to draw</param>
        // <param name="jointPoints">translated positions of joints to draw</param>
        // <param name="jointType0">first joint of bone to draw</param>
        // <param name="jointType1">second joint of bone to draw</param>
        // <param name="drawingContext">drawing context to draw to</param>
        // /// <param name="drawingPen">specifies color to draw a specific bone</param>
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

        // Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        // <param name="handState">state of the hand</param>
        // <param name="handPosition">position of the hand</param>
        // <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext) {

            switch (handState) {

                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;

                default:
                    drawingContext.DrawEllipse(this.handBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        private void DrawDrums(DrawingContext drawingContext) {

            //Bombo
            drawingContext.DrawImage(bass, new Rect((this.displayWidth-(bass.Width/bassReduction))/2, 
                                                        this.displayHeight-(bass.Height/bassReduction), 
                                                        bass.Width/bassReduction, bass.Height/bassReduction));
            drawingContext.DrawRectangle(this.handOpenBrush,null,bassHitRect);

            //Snare
            drawingContext.DrawImage(snare, new Rect((this.displayWidth/2)+(bass.Width / bassReduction / 3), 
                                                        this.displayHeight - ((bass.Height / bassReduction)+(snare.Height/snareReduction/2)),
                                                        snare.Width / snareReduction, snare.Height / snareReduction));
            drawingContext.DrawRectangle(this.handOpenBrush, null, snareHitRect);

            //HitHat
            drawingContext.DrawRectangle(this.handOpenBrush, null, hihatHitRect);
        }

        // Maneja cuando golpeas un tambor
        private void OnDrumHit(Point LeftHand, Point RightHand, Point LeftFoot, Point RightFoot) {

            if ((bassHitRect.Contains(LeftFoot) || bassHitRect.Contains(RightFoot)) && !bassHit) {

                bassHit = true;
                playerBass.Stop();
                playerBass.Play();

            }
            else if(!(bassHitRect.Contains(LeftFoot) || bassHitRect.Contains(RightFoot))) {
                bassHit = false;
            }

            if ((snareHitRect.Contains(LeftHand) || snareHitRect.Contains(RightHand)) && !snareHit) {

                snareHit = true;
                playerSnare.Stop();
                playerSnare.Play();

            }
            else if(!(snareHitRect.Contains(LeftHand) || snareHitRect.Contains(RightHand))) {
                snareHit = false;
            }

            if((hihatHitRect.Contains(LeftHand) || hihatHitRect.Contains(RightHand)) && !hihatHit) {

                hihatHit = true;
                playerHihat.Stop();
                playerHihat.Play();

            }
            else if (!(hihatHitRect.Contains(LeftHand) || hihatHitRect.Contains(RightHand))) {
                hihatHit = false;
            }
        }
    }
}
