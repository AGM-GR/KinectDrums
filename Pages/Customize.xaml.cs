﻿namespace NPI.KinectDrums {

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using NPI.KinectDrums.DataModel;
    using NPI.KinectDrums.Gestures;
    using System.Collections;

    public partial class Customize : UserControl {

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
        // Variable para indicar si está en modo editar o jugar
        private bool editMode = true;

        // Variable para controlar si se está arrastrando un tambor
        private bool dragging = false;
        private JointType dragger;
        private Drum draggedDrum = null;
        private int dragDropCount = 5;

        //Papelera
        private Rect bin = new Rect();
        private BitmapImage imageBin = new BitmapImage();
        Color LightGrey = Color.FromArgb(200, 107, 107, 107);
        Color LightRed = Color.FromArgb(200, 255, 10, 10);
        private Brush binBrush = new SolidColorBrush();
        private double binReduction;

        //DrumKit variables
        private Drum bass;
        private Drum snare;
        private Drum middleTom;
        private Drum floorTom;
        private Drum crash;
        private Hihat hihat;

        //Vector de Drums 
        private ArrayList drums = new ArrayList();
        private ArrayList hihats = new ArrayList();

        // Brush para pintar los HitArea
        private readonly Brush hitBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        private readonly Brush footHitBrush = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));

        //Lista de GestureDetector, uno para cada body detectado
        private List<GestureDetector> gestureDetectorList = null;

        //Número máximo de personas detectadas
        private const int maxBodies = 1;
        /*****************************************************************************************************************/



        // Inicializa una instancia de la clase Play.
        public Customize() {

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
                          this.displayHeight - (Image.Height / Reduction) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth - (Image.Width / Reduction / 5)) / 2,
                          this.displayHeight - (Image.Height / Reduction / 10) - 10,
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
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) + (Image.Width / 8),
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)) - 15,
                          Image.Width / Reduction, Image.Height / Reduction / 2 + 10),
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
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) + 11,
                          this.displayHeight - ((bass.Height) + (Image.Height / Reduction)) - 10,
                          (Image.Width / Reduction) - 11, Image.Height / Reduction),
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
                          this.displayHeight - (Image.Height / Reduction) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) + (bass.Width / 2) + 11,
                          this.displayHeight - (Image.Height / Reduction) - 10,
                          (Image.Width / Reduction) - 20, Image.Height / Reduction / 2),
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
                          this.displayHeight - (Image.Height / Reduction) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / 1.7) + 2,
                          this.displayHeight - (Image.Height / Reduction) - 10,
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
            MediaPlayer playerAux = new MediaPlayer();
            playerAux.Open(new Uri("Sounds/ClosedHihat.wav", UriKind.Relative));
            MediaPlayer playerPedal = new MediaPlayer();
            playerPedal.Open(new Uri("Sounds/ClosingHihat.wav", UriKind.Relative));
            Reduction = 5;

            hihat = new Hihat(
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction),
                          this.displayHeight - (Image.Height / Reduction) - 10,
                          Image.Width / Reduction, Image.Height / Reduction),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction) + 8,
                          this.displayHeight - (Image.Height / Reduction) + 16 - 10,
                          Image.Width / Reduction - 16, Image.Height / Reduction / 6),
                new Rect((this.displayWidth / 2) - (bass.Width / 2) - (Image.Width / Reduction / 2) - (Image.Width / Reduction / 4 / 2),
                          this.displayHeight - (Image.Height / Reduction / 12) - 10,
                          Image.Width / Reduction / 4, Image.Height / Reduction / 12),
                player,
                playerAux,
                playerPedal,
                Image,
                Reduction
            );

            //Crea el botón de eliminar
            imageBin = new BitmapImage(new Uri("pack://application:,,,/Images/Bin.png", UriKind.Absolute));
            binBrush = new SolidColorBrush(LightGrey);
            binReduction = 6;
            bin = new Rect(10, 10, imageBin.Width / binReduction, imageBin.Height / binReduction);

            // Añade los botones en el lateral
            var sampleDataSource = SampleDataSource.GetGroup("DrumPieces");
            this.itemsControl.ItemsSource = sampleDataSource;

            //Inicializa el vector de GestureDetector
            this.gestureDetectorList = new List<GestureDetector>();
            player = new MediaPlayer();
            player.Open(new Uri("Sounds/Drumsticks.wav", UriKind.Relative));

            for (int i = 0; i < maxBodies; ++i) {

                GestureDetector detector = new GestureDetector(this.kinectSensor, player);
                this.gestureDetectorList.Add(detector);
            }
            /*****************************************************************************************************************/
        }

        // Obtiene el bitmap
        public ImageSource ImageSource {
            get{
                return this.imageSource;
            }
        }

        // Inicia el Programa, iniciando la llegada de frames
        private void Customize_Loaded(object sender, RoutedEventArgs e) {

            if (this.bodyFrameReader != null) {

                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        // Funcion para cerrar correctamente el programa
        public void Customize_Closing() {

            if (this.bodyFrameReader != null) {

                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null) {

                // The GestureDetector contains disposable members
                foreach (GestureDetector detector in this.gestureDetectorList) {

                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            this.drums.Clear();
            this.drums = null;
            this.hihats.Clear();
            this.hihats = null;
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

                    //Si está en modo editar dibuja la papelera
                    if (editMode) this.DrawBin(dc);
                    /*******************************************************/

                        // Draw a transparent background to set the render size
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    //Mantiene la cuenta de cuerpos detectados para solo mostrar el máximo establecido
                    int bodiesCount = 0;

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
                            Dictionary<JointType, HandState> handsState = new Dictionary<JointType, HandState>();
                            handsState[JointType.HandTipLeft] = body.HandLeftState;
                            handsState[JointType.HandTipRight] = body.HandRightState;

                            //Si está en modo editar, deja mover los Drum
                            if (editMode) {

                                // Si no se esta arrastrando ninguno, comprueba si se ha seleccionado alguno.
                                // Si seleccionamos alguno pasamos al modo arrastando hasta que lo soltemos.
                                if (!dragging) {

                                    draggedDrum = this.OnDrumSelect(body.HandLeftState, jointPoints[JointType.HandTipLeft]);
                                    if (draggedDrum != null) {
                                        dragging = true;
                                        dragger = JointType.HandTipLeft;
                                    }
                                    else {

                                        draggedDrum = this.OnDrumSelect(body.HandRightState, jointPoints[JointType.HandTipRight]);
                                        if (draggedDrum != null) {
                                            dragging = true;
                                            dragger = JointType.HandTipRight;
                                        }
                                    }

                                } else if (!this.OnDrumDrop(draggedDrum, handsState[dragger], jointPoints[dragger])) {

                                    this.OnDrumDrag(draggedDrum, jointPoints[dragger]);

                                } else {

                                    dragging = false;
                                    draggedDrum = null;
                                }
                            }
                            //Si no está en modo editar, podemos tocar los Drum
                            else { 
                                //Comprueba si ha tocado un Drum
                                this.OnDrumHit(jointPoints[JointType.HandTipLeft], jointPoints[JointType.HandTipRight], jointPoints[JointType.FootLeft], jointPoints[JointType.FootRight]);
                            }

                            //Obtiene el ID del body actual
                            ulong trackingId = body.TrackingId;
                            // Si el TrackingId de un Body cambia, actualiza su correspondiente GestureDetector con el nuevo valor
                            if (trackingId != this.gestureDetectorList[bodiesCount].TrackingId)
                            {

                                this.gestureDetectorList[bodiesCount].TrackingId = trackingId;

                                // Si el Body actual esta siendo detectado, inicia su GestureDetector
                                // Si no está siendo detectado, pausa el GestureDetector para ahorrar recursos
                                this.gestureDetectorList[bodiesCount].IsPaused = trackingId == 0;
                            }

                            //Incrementa los cuerpos detectados;
                            bodiesCount++;
                            //Si ya se ha pintado el máximo de bodies, termina de pintarlos
                            if (bodiesCount == maxBodies)
                                break;
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

        //Dibuja la Papelera
        private void DrawBin(DrawingContext drawingContext) {

            drawingContext.DrawRectangle(this.binBrush, null, bin);
            drawingContext.DrawImage(imageBin, bin);
        }

        //Elimina un Drum de la batería
        private void OnDeleteDrum(Drum deleteDrum) {

            if (drums.Contains(deleteDrum))
                drums.Remove(deleteDrum);
            else if (hihats.Contains(deleteDrum))
                hihats.Remove(deleteDrum);
        }

        //Dibuja la batería
        private void DrawDrums(DrawingContext drawingContext) {

            foreach (Drum dr in drums) { 

                dr.Draw(drawingContext);
                dr.DrawHit(drawingContext, this.hitBrush);
            }

            foreach (Hihat ht in hihats) {

                ht.Draw(drawingContext);
                ht.DrawHit(drawingContext, this.hitBrush, this.footHitBrush);
            }
        }

        // Maneja cuando golpeas un tambor
        private void OnDrumHit(Point LeftHand, Point RightHand, Point LeftFoot, Point RightFoot) {

            foreach (Drum dr in drums) {

                if (dr.HandHit == 0)
                    dr.HitDrum(LeftFoot, RightFoot);
                else if (dr.HandHit == 1)
                    dr.HitDrum(LeftHand, RightHand);
            }

            foreach (Hihat ht in hihats) {

                ht.HitDrum(LeftHand, RightHand, LeftFoot, RightFoot);
            }

        }

        //Comprueba si se ha seleccionado algun Drum
        private Drum OnDrumSelect(HandState handState, Point handPosition) {

            if (handState == HandState.Closed) {

                foreach (Drum dr in drums)

                    if (dr.Position.Contains(handPosition)) {

                        dr.MoveTo(handPosition);
                        return dr;
                    }


                foreach (Hihat ht in hihats)

                    if (ht.Position.Contains(handPosition)) {

                        ht.MoveTo(handPosition);
                        return ht;
                    }
            }

            return null;
        }

        // Manjea cuando arrastras un Drum con la mano cerrada
        private void OnDrumDrag(Drum dr, Point handPosition) {

            if (dr.HandHit == 2)
                ((Hihat)dr).MoveTo(handPosition);
            else
                dr.MoveTo(handPosition);

            if (bin.Contains(handPosition))
                binBrush = new SolidColorBrush(LightRed);
            else
                binBrush = new SolidColorBrush(LightGrey);
        }

        // Manjea cuando sueltas un Drum que se estaba arrastrando
        private bool OnDrumDrop(Drum dr,  HandState handState, Point handPosition) {

            if (handState == HandState.Open) {
                dragDropCount--;

                if (dragDropCount <= 0) {

                    if (dr.HandHit == 2)
                        ((Hihat)dr).MoveTo(handPosition);
                    else
                        dr.MoveTo(handPosition);

                    //Si está sobre la papelera lo elimina
                    if (bin.Contains(handPosition)) {
                        this.OnDeleteDrum(dr);
                        binBrush = new SolidColorBrush(LightGrey);
                    }

                    return true;
                }
            }

            else dragDropCount = 5;

            return false;
        }

        // Maneja que hacer al pulsar un botón: ButtonClick
        private void ButtonClick(object sender, RoutedEventArgs e) {

            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null) {

                switch (sampleDataItem.UniqueId) {

                    case "bass":
                        drums.Add(new Drum(bass));
                    break;

                    case "snare":
                        drums.Add(new Drum(snare));
                    break;

                    case "middleTom":
                        drums.Add(new Drum(middleTom));
                    break;

                    case "floorTom":
                        drums.Add(new Drum(floorTom));
                    break;

                    case "crash":
                        drums.Add(new Drum(crash));
                    break;

                    case "hihat":
                        hihats.Add(new Hihat(hihat));
                    break;
                }
            }
        }


        // Maneja que hacer al pulsar un botón: PlayButtonClick
        private void PlayButtonClick(object sender, RoutedEventArgs e) {

            if (editMode) {
                PlayButtonText.Text = "Customize";
                navigationRegion.Visibility = Visibility.Hidden;
                editMode = false;
            }
            else {
                PlayButtonText.Text = "Play";
                navigationRegion.Visibility = Visibility.Visible;
                editMode = true;
            }
        }

        /*****************************************************************************************************************/
    }
}
