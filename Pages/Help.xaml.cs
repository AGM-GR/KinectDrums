namespace NPI.KinectDrums
{

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using NPI.KinectDrums.DataModel;

    public partial class Help : UserControl
    {

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

        public Help()
        {

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
        }

        // Inicia el Programa, iniciando la llegada de frames
        private void Help_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.bodyFrameReader != null)
            {

                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {

           /* bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {

                if (bodyFrame != null)
                {

                    if (this.bodies == null)
                    {

                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {

                using (DrawingContext dc = this.drawingGroup.Open())
                {

                    /*******************************************************/
                    // Dibuja la batería
                   // this.DrawDrums(dc);
                    /*******************************************************/

                    // Draw a transparent background to set the render size
                   /* dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {

                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {

                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {

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

                            //Comprueba si ha tocado un tambor
                            OnDrumHit(jointPoints[JointType.HandTipLeft], jointPoints[JointType.HandTipRight], jointPoints[JointType.FootLeft], jointPoints[JointType.FootRight]);

                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }*/
        }
    }
}