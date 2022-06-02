using Projekat3.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Projekat3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public XmlNodeList xmlNodeList;
        public XmlDocument xmlDocument = new XmlDocument();


        public List<SubstationEntity> subs = new List<SubstationEntity>();
        public List<NodeEntity> nodes = new List<NodeEntity>();
        public List<SwitchEntity> switches = new List<SwitchEntity>();
        public List<MyLineEntity> lines = new List<MyLineEntity>();
        public List<MyLineEntity> mylines = new List<MyLineEntity>();
        public List<bool> openlines = new List<bool>();
        
        
        public ModelVisual3D model = new ModelVisual3D();
        
        public double MinX = 45.2325;
        public double MaxX = 45.277031;
        public double MinY = 19.793909;
        public double MaxY = 19.894459;

        public double BoardWidth;
        public double BoardHeight;
        public double CubeSize;
        public double VerticalSize;
        public List<PointID> newpoints = new List<PointID>();
        public Dictionary<long, Point> newpointsrecnik= new Dictionary<long, Point>();
        
        
        public List<Point> boardpoints = new List<Point>();
        public List<Point3D> boardpointsvertices = new List<Point3D>();
        
        
        public ToolTip tooltip = new ToolTip();
        private ArrayList models = new ArrayList();
        private ArrayList modelslines = new ArrayList();
        private GeometryModel3D hitgeo;
        private List<PowerEntity> elements = new List<PowerEntity>();
        private List<PowerEntity> allelements = new List<PowerEntity>();
        public Point clickedlocation = new Point();

        public MainWindow()
        {
            InitializeComponent();
            SceneSettings();
            LoadBoard();
            LoadSubstations();
            LoadNodes();
            LoadSwitches();
            LoadLines();
            
            ConvertPointsAndDraw();
            DrawLines();
            //DrawFristLine();
            //drawvertices(0);
        }

        public void SceneSettings()
        {
            xmlDocument.Load("Geographic.xml");
            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 1, 5);
            myPCamera.LookDirection = new Vector3D(0, -0.25, -1);
            viewport1.Camera = myPCamera;
            viewport1.RotateGesture = new MouseGesture(MouseAction.MiddleClick);
            viewport1.FixedRotationPoint = new Point3D(0, 0, 0);
            viewport1.FixedRotationPointEnabled = true;
            viewport1.PanGesture = new MouseGesture(MouseAction.LeftClick);
            viewport1.PanGesture2 = null;
            BoardWidth = MaxY - MinY;
            BoardHeight = MaxX - MinX;
            CubeSize = BoardWidth / 5;
            tooltip.IsOpen = false;
            tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
        }

        public void LoadSubstations()
        {

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            
            
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                sub.Name = xmlNode.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                double convertedx, convertedy;
                ToLatLon(sub.X, sub.Y, 34, out convertedx, out convertedy);
                
                if (convertedx >= 45.2325 && convertedx <= 45.277031)
                {
                    if (convertedy >= 19.793909 && convertedy <= 19.894459)
                    {
                        subs.Add(sub);

                        Point p = new Point();
                        p.X = convertedx;
                        p.Y = convertedy;
                        newpoints.Add(new PointID(p, sub.Id));
                        //newpointsrecnik.Add(sub.Id, p);

                       
                        double boardz = CalculateBoardZ(convertedx);
                        double boardx = CalculateBoardX(convertedy);
                        
                        sub.BoardZ = boardz;
                        sub.BoardX = boardx;
                        elements.Add(sub);
                        allelements.Add(sub);

                    }
                }
            }

        }

        public void ConvertPointsAndDraw()
        {
            
            for (int i = 0; i < newpoints.Count; i++)
            {
                double z;
                double x;
                z = CalculateBoardZ(newpoints[i].p.X);
                x = CalculateBoardX(newpoints[i].p.Y);
                Point p = new Point(z, x);
                boardpoints.Add(p);

                int noc = CalculateNumberOfConnections(newpoints[i].id);
                DrawCube(i, noc);
            }
           
        }
        
        

        public void LoadNodes()
        {
            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                NodeEntity node = new NodeEntity();
                node.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                node.Name = xmlNode.SelectSingleNode("Name").InnerText;
                node.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                node.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                double convertedx, convertedy;
                ToLatLon(node.X, node.Y, 34, out convertedx, out convertedy);
                if (convertedx >= 45.2325 && convertedx <= 45.277031)
                {
                    if (convertedy >= 19.793909 && convertedy <= 19.894459)
                    {
                        nodes.Add(node);

                        Point p = new Point();
                        p.X = convertedx;
                        p.Y = convertedy;
                        newpoints.Add(new PointID(p, node.Id));
                        //newpointsrecnik.Add(node.Id, p);

                        double boardz = CalculateBoardZ(convertedx);
                        double boardx = CalculateBoardX(convertedy);
                       
                        node.BoardZ = boardz;
                        node.BoardX = boardx;
                        elements.Add(node);
                        allelements.Add(node);
                    }
                }
            }
        }

        public void LoadSwitches()
        {
            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                SwitchEntity switchy = new SwitchEntity();
                switchy.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                switchy.Name = xmlNode.SelectSingleNode("Name").InnerText;
                switchy.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                switchy.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);
                switchy.Status = xmlNode.SelectSingleNode("Status").InnerText;

                double convertedx, convertedy;
                ToLatLon(switchy.X, switchy.Y, 34, out convertedx, out convertedy);
                
                
                if (convertedx >= 45.2325 && convertedx <= 45.277031)
                {
                    if (convertedy >= 19.793909 && convertedy <= 19.894459)
                    {
                        switches.Add(switchy);

                        Point p = new Point();
                        p.X = convertedx;
                        p.Y = convertedy;
                        newpoints.Add(new PointID(p, switchy.Id));
                        //newpointsrecnik.Add(switchy.Id, p);
                        double boardz = CalculateBoardZ(convertedx);
                        double boardx = CalculateBoardX(convertedy);
                        
                        switchy.BoardZ = boardz;
                        switchy.BoardX = boardx;
                        elements.Add(switchy);
                        allelements.Add(switchy);
                    }
                }
            }
        }



        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

       
        private void LoadBoard()
        {
            ImageBrush texture = new ImageBrush(new BitmapImage(new Uri(@"Images/map.jpg", UriKind.Relative)));
            Material material = new DiffuseMaterial(texture);




            Point3D t1 = new Point3D(-2, 0, -2);
            Point3D t2 = new Point3D(-2, 0, 2);
            Point3D t3 = new Point3D(2, 0, -2);
            Point3D t4 = new Point3D(2, 0, 2);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(t1);
            mesh.Positions.Add(t2);
            mesh.Positions.Add(t3);
            mesh.Positions.Add(t4);

            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);
            
            
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            
            GeometryModel3D z = new GeometryModel3D(mesh, material);
            ModelUIElement3D ModelUI = new ModelUIElement3D();
            ModelUI.Model = z;
            viewport1.Children.Add(ModelUI);

        }

        private double CalculateBoardZ(double x)
        {
            // 4=>2-(-2), -2 na kraju=>dodavanje min vrednosti da se odnosi na pocetak nova vrednost
            double Z = ((4 * (x - MinX)) / BoardHeight) - 2;
            return -1 * Z;
        }

        private double CalculateBoardX(double y)
        {
            //Y predstavlja Z osu, i ide od -2 do 2, ako se dobije pozitivna vrednost za novo Y(0,2) to znaci da se element nalazi na gornjoj polovini mape ako je negativan onda je na donjoj(-2,0)
            //Mapa je podeljena tako da gornja polovina ide u ekran a donja izlazi iz njega
            //Ako je nova vrednost pozivtina, element se nalazi na gornjoj polovini koja ide u ekran pa se mora dodati - jer z koordinata tada ide u ekran
            //Ako je nova vrednost negativna, element se nalazi na donjoj polovini koja ide iz ekrana pa se mora dodati - jer z koordinata tada ide u plus

            // 4=>2-(-2), -2 na kraju=>dodavanje min vrednosti da se odnosi na pocetak nova vrednost
            double X;
            X = ((4 * (y - MinY)) / BoardWidth) - 2;
            return X;
        }

        private void DrawCube(int e, int noc)
        {
            double elevation = 0;
            int exists = 0;
            double newz = boardpoints[e].X;
            double newx = boardpoints[e].Y;
        
            for (int i = 0; i < e; i++)
            {
                    double z11, z22, x11, x22;
                    z11 = boardpoints[i].X - CubeSize;
                    z22 = boardpoints[i].X + CubeSize;
                    x11 = boardpoints[i].Y - CubeSize;
                    x22 = boardpoints[i].Y + CubeSize;
                    
                    if (boardpoints[e].Y > x11 && boardpoints[e].Y < x22 && boardpoints[e].X > z11 && boardpoints[e].X < z22)
                    {
                        exists++;
                        elevation = 0.006;

                        newz = boardpoints[i].X;
                        newx = boardpoints[i].Y;
                        boardpoints[e] = new Point(newz, newx);
                    }
                

            }
            

            double z1, z2, x1, x2, y1, y2;
            
            z1 = newz - CubeSize/2;
            z2 = newz + CubeSize/2;
            x1 = newx - CubeSize/2;
            x2 = newx + CubeSize/2;
            
            y1 = exists * (CubeSize + elevation);
            y2 = y1 + CubeSize;

            
            Point3D t1 = new Point3D(x1, y1, z1);
            Point3D t2 = new Point3D(x2, y1, z1);
            Point3D t3 = new Point3D(x1, y1, z2);
            Point3D t4 = new Point3D(x2, y1, z2);

            Point3D t12 = new Point3D(x1, y2, z1);
            Point3D t22 = new Point3D(x2, y2, z1);
            Point3D t32 = new Point3D(x1, y2, z2);
            Point3D t42 = new Point3D(x2, y2, z2);



            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(t1);   //0
            mesh.Positions.Add(t2);   //1
            mesh.Positions.Add(t3);   //2
            mesh.Positions.Add(t4);   //3
            mesh.Positions.Add(t12);  //4
            mesh.Positions.Add(t22);  //5
            mesh.Positions.Add(t32);  //6
            mesh.Positions.Add(t42);  //7






            //Bottom Face
            mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1);


            //Fron face
            mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7);

            //Left face
            mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4);

            //Right Face
            mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7);

            //Back Face
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(5);

            //Upper Face
            mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(5);
            
            Material mat = new DiffuseMaterial();
            mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 0, 127)));
            
            
            GeometryModel3D cube = new GeometryModel3D(mesh, mat);
            models.Add(cube);
            ModelUIElement3D ModelUI = new ModelUIElement3D();
            ModelUI.Model = cube;
            viewport1.Children.Add(ModelUI);


        }


      
        public void LoadLines()
        {
            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode xmlNode in xmlNodeList)
            {

                bool firsendexists = false;
                bool secondendexists = false;
                MyLineEntity line = new MyLineEntity();
                line.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                line.Name = xmlNode.SelectSingleNode("Name").InnerText;
                line.IsUnderground = Boolean.Parse(xmlNode.SelectSingleNode("IsUnderground").InnerText);
                line.R = float.Parse(xmlNode.SelectSingleNode("R").InnerText);
                line.ConductorMaterial = xmlNode.SelectSingleNode("ConductorMaterial").InnerText;
                line.LineType = xmlNode.SelectSingleNode("LineType").InnerText;
                line.ThermalConstantHeat = long.Parse(xmlNode.SelectSingleNode("ThermalConstantHeat").InnerText);
                line.FirstEnd = long.Parse(xmlNode.SelectSingleNode("FirstEnd").InnerText);
                line.SecondEnd = long.Parse(xmlNode.SelectSingleNode("SecondEnd").InnerText);

                foreach (PowerEntity pe in elements)
                {
                    if (pe.Id == line.FirstEnd)
                    {
                        firsendexists = true;
                    }
                    if (pe.Id == line.SecondEnd)
                    {
                        secondendexists = true;
                    }
                }
                

                line.Vertices = new List<Point>();
                foreach (XmlNode pointNode in xmlNode.ChildNodes[9].ChildNodes) 
                {
                    Point p = new Point();
                    p.X = double.Parse(pointNode.SelectSingleNode("X").InnerText);
                    p.Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText);
                    double noviX, noviY;
                    ToLatLon(p.X, p.Y, 34, out noviX, out noviY);
                    if (noviX >= 45.2325 && noviX <= 45.277031)
                    {
                        if (noviY >= 19.793909 && noviY <= 19.894459)
                        {
                            line.Vertices.Add(new Point(noviX, noviY));
                        }
                    }

                }
                if (firsendexists && secondendexists)
                {
                    lines.Add(line);
                }



            }
        }
      
        public void LoadAllLines()
        {
            int alllines = 0;
            int ZeroToOne = 0;
            int OneToTwo = 0;
            int GreaterThanTwo = 0;
            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                
                MyLineEntity line = new MyLineEntity();
                line.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                line.Name = xmlNode.SelectSingleNode("Name").InnerText;
                line.IsUnderground = Boolean.Parse(xmlNode.SelectSingleNode("IsUnderground").InnerText);
                line.R = float.Parse(xmlNode.SelectSingleNode("R").InnerText);
                line.ConductorMaterial = xmlNode.SelectSingleNode("ConductorMaterial").InnerText;
                line.LineType = xmlNode.SelectSingleNode("LineType").InnerText;
                line.ThermalConstantHeat = long.Parse(xmlNode.SelectSingleNode("ThermalConstantHeat").InnerText);
                line.FirstEnd = long.Parse(xmlNode.SelectSingleNode("FirstEnd").InnerText);
                line.SecondEnd = long.Parse(xmlNode.SelectSingleNode("SecondEnd").InnerText);

                alllines++;
                if(line.R<=1)
                {
                    ZeroToOne++;
                }
                if(line.R >1 && line.R <=2)
                {
                    OneToTwo++;
                }
                if(line.R>2)
                {
                    GreaterThanTwo++;
                }



            }
            Console.WriteLine();
        }

        public int CalculateNumberOfConnections(long id)
        {
            int rez = 0;
            foreach (MyLineEntity le in lines)
            {
                if (le.FirstEnd == id || le.SecondEnd == id)
                {
                    rez++;
                }
            }
            return rez;
        }

        private void viewport1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point mouseposition = e.GetPosition(viewport1);
            clickedlocation = mouseposition;
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

            PointHitTestParameters pointparams =
                     new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams =
                     new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D     
            hitgeo = null;
            VisualTreeHelper.HitTest(viewport1, MyHitTestFilter, HTResult, pointparams);
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {

                DiffuseMaterial darkSide =
                     new DiffuseMaterial(new SolidColorBrush(
                     System.Windows.Media.Colors.Red));
                bool gasit = false;
                for (int i = 0; i < models.Count; i++)
                {
                    if ((GeometryModel3D)models[i] == rayResult.ModelHit)
                    {
                        string id = "";
                        string name = "";
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;
                        //hitgeo.Material = darkSide;
                        string tip = "";
                        try
                        {
                            SubstationEntity sse = (SubstationEntity)allelements[i];
                            tip = "SUBSTATION";
                            id = sse.Id.ToString();
                            name = sse.Name;
                            tooltip.Content = "ID: " + id + Environment.NewLine + "NAME: " + name + Environment.NewLine + "TIP: " + tip;
                            tooltip.HorizontalOffset = clickedlocation.X + 175;
                            tooltip.VerticalOffset = clickedlocation.Y + 205;
                            tooltip.IsOpen = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        try
                        {
                            NodeEntity ne = (NodeEntity)allelements[i];
                            tip = "NODE";
                            id = ne.Id.ToString();
                            name = ne.Name;
                            tooltip.Content = "ID: " + id + Environment.NewLine + "NAME: " + name + Environment.NewLine + "TIP: " + tip;
                            tooltip.HorizontalOffset = clickedlocation.X + 175;
                            tooltip.VerticalOffset = clickedlocation.Y + 205;
                            tooltip.IsOpen = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        try
                        {
                            SwitchEntity se = (SwitchEntity)allelements[i];
                            tip = "SWITCH";
                            id = se.Id.ToString();
                            name = se.Name;
                            tooltip.Content = "ID: " + id + Environment.NewLine + "NAME: " + name + Environment.NewLine + "TIP: " + tip + Environment.NewLine + "STATUS: " + se.Status;
                            tooltip.HorizontalOffset = clickedlocation.X + 175;
                            tooltip.VerticalOffset = clickedlocation.Y + 205;
                            tooltip.IsOpen = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        try
                        {
                            MyLineEntity mle = (MyLineEntity)allelements[i];
                            tip = "LINE";
                            id = mle.Id.ToString();
                            name = mle.Name;
                            tooltip.Content = "ID: " + id + Environment.NewLine + "NAME: " + name + Environment.NewLine + "TIP: " + tip + Environment.NewLine + "FIRST END: " + mle.FirstEnd.ToString() + Environment.NewLine + "SECOND END: " + mle.SecondEnd.ToString()+Environment.NewLine+"MATERIAL: "+mle.ConductorMaterial + Environment.NewLine + "R: " + mle.R;
                            tooltip.HorizontalOffset = clickedlocation.X + 175;
                            tooltip.VerticalOffset = clickedlocation.Y + 205;
                            tooltip.IsOpen = true;

                            long firstend = mle.FirstEnd;
                            long secondend = mle.SecondEnd;

                            for (int x = 0; x < elements.Count; x++)
                            {
                                if (elements[x].Id == firstend || elements[x].Id==secondend)
                                {
                                    Material mat = new DiffuseMaterial();
                                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0, 255, 0)));
                                    ((GeometryModel3D)models[x]).Material = mat;
                                }else
                                {
                                    Material mat = new DiffuseMaterial();
                                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 0, 127)));
                                    ((GeometryModel3D)models[x]).Material = mat;
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }





                    }
                    else
                    {
                        //((GeometryModel3D)models[i]).Material = blue;
                        //tooltip.IsOpen = false;
                    }
                }

                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }


        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {

            if (o.GetType() == typeof(ModelUIElement3D))
            {
                // Visual object and descendants are NOT part of hit test results enumeration.
                ModelUIElement3D model = (ModelUIElement3D)o;
                if (model.Visibility == Visibility.Hidden)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }
                else
                {
                    return HitTestFilterBehavior.Continue;
                }
               
            }
            else
            {
                // Visual object is part of hit test results enumeration.
                return HitTestFilterBehavior.Continue;

            }
        }

        private void viewport1_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip.IsOpen = false;
        }

        public void DrawPath(Point p1, Point p2,string material)
        {
            if (Math.Abs(p1.X - p2.X) >= Math.Abs(p1.Y - p2.Y))
            {
                Point levi = new Point();
                Point desni = new Point();
                if (p1.X <= p2.X)
                {
                    levi = p1;
                    desni = p2;
                }
                else
                {
                    levi = p2;
                    desni = p1;
                }
                Point levigornjidesni = new Point();
                levigornjidesni.X = levi.X + CubeSize;
                levigornjidesni.Y = levi.Y - CubeSize;
                Point levidonjidesni = new Point();
                levidonjidesni.X = levi.X + CubeSize;
                levidonjidesni.Y = levi.Y + CubeSize;

                Point desnigornjilevi = new Point();
                desnigornjilevi.X = desni.X - CubeSize;
                desnigornjilevi.Y = desni.Y - CubeSize;
                Point desnidonjilevi = new Point();
                desnidonjilevi.X = desni.X - CubeSize;
                desnidonjilevi.Y = desni.Y + CubeSize;

                Point3D P2levigornjidesni3D = new Point3D(levigornjidesni.X, 0.002, levigornjidesni.Y);
                Point3D P1levidonjidesni3D = new Point3D(levidonjidesni.X, 0.002, levidonjidesni.Y);
                Point3D P6desnigornjilevi3D = new Point3D(desnigornjilevi.X, 0.002, desnigornjilevi.Y);
                Point3D P5desnidonjilevi3D = new Point3D(desnidonjilevi.X, 0.002, desnidonjilevi.Y);
                Point3D P4=new Point3D(levigornjidesni.X,0.02,levigornjidesni.Y);
                Point3D P3 = new Point3D(levidonjidesni.X, 0.02, levidonjidesni.Y);
                Point3D P8 = new Point3D(desnigornjilevi.X, 0.02, desnigornjilevi.Y);
                Point3D P7 = new Point3D(desnidonjilevi.X, 0.02, desnidonjilevi.Y);




                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P2levigornjidesni3D);   //0
                mesh.Positions.Add(P1levidonjidesni3D);   //1
                mesh.Positions.Add(P6desnigornjilevi3D);   //2
                mesh.Positions.Add(P5desnidonjilevi3D);   //3

                mesh.Positions.Add(P4); //4
                mesh.Positions.Add(P3); //5
                mesh.Positions.Add(P8); //6
                mesh.Positions.Add(P7); //7





                //Bottom Face
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

                //Fron Face
                mesh.TriangleIndices.Add(5);mesh.TriangleIndices.Add(1);mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(3);mesh.TriangleIndices.Add(7);mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3);mesh.TriangleIndices.Add(7);mesh.TriangleIndices.Add(5);

                //Top Face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);

                //Back Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);



                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }
                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
            }
            else
            {
                Point gornji = new Point();
                Point donji = new Point();
                if (p1.Y <= p2.Y)
                {
                    gornji = p1;
                    donji = p2;
                }
                else
                {
                    gornji = p2;
                    donji = p1;
                }

                Point gornjidonjilevi = new Point();
                gornjidonjilevi.X = gornji.X - CubeSize;
                gornjidonjilevi.Y = gornji.Y + CubeSize;
                Point gornjidonjidesni = new Point();
                gornjidonjidesni.X = gornji.X + CubeSize;
                gornjidonjidesni.Y = gornji.Y + CubeSize;

                Point donjigornjilevi = new Point();
                donjigornjilevi.X = donji.X - CubeSize;
                donjigornjilevi.Y = donji.Y - CubeSize;
                Point donjigornjidesni = new Point();
                donjigornjidesni.X = donji.X + CubeSize;
                donjigornjidesni.Y = donji.Y - CubeSize;

                Point3D P5gornjidonjilevi3D = new Point3D(gornjidonjilevi.X, 0.002, gornjidonjilevi.Y);
                Point3D P6gornjidonjidesni3D = new Point3D(gornjidonjidesni.X, 0.002, gornjidonjidesni.Y);
                Point3D P1donjigornjilevi3D = new Point3D(donjigornjilevi.X, 0.002, donjigornjilevi.Y);
                Point3D P2donjigornjidesni3D = new Point3D(donjigornjidesni.X, 0.002, donjigornjidesni.Y);

                Point3D P3 = new Point3D(donjigornjilevi.X, 0.02, donjigornjilevi.Y);
                Point3D P4 = new Point3D(donjigornjidesni.X, 0.02, donjigornjidesni.Y);
                Point3D P7 = new Point3D(gornjidonjilevi.X, 0.02, gornjidonjilevi.Y);
                Point3D P8 = new Point3D(gornjidonjidesni.X, 0.02, gornjidonjidesni.Y);

                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P5gornjidonjilevi3D);   //0
                mesh.Positions.Add(P6gornjidonjidesni3D);   //1
                mesh.Positions.Add(P1donjigornjilevi3D);   //2
                mesh.Positions.Add(P2donjigornjidesni3D);   //3

                mesh.Positions.Add(P3); //4
                mesh.Positions.Add(P4); //5
                mesh.Positions.Add(P7); //6
                mesh.Positions.Add(P8); //7

                

                //Left Face
                mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4);


                //Top Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);

                //Bottom Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

                //Right Face
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(7);




                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if(material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }else if(material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }else if(material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }

               
                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
            }
        }

        public void DrawPathBetweenVertecies(Point p1,Point p2,string material)
        {
            if (Math.Abs(p1.X - p2.X) >= Math.Abs(p1.Y - p2.Y))
            {
                Point levi = new Point();
                Point desni = new Point();
                if (p1.X <= p2.X)
                {
                    levi = p1;
                    desni = p2;
                }
                else
                {
                    levi = p2;
                    desni = p1;
                }
                Point levigornjidesni = new Point();
                levigornjidesni.X = levi.X;
                levigornjidesni.Y = levi.Y - CubeSize;
                Point levidonjidesni = new Point();
                levidonjidesni.X = levi.X;
                levidonjidesni.Y = levi.Y + CubeSize;

                Point desnigornjilevi = new Point();
                desnigornjilevi.X = desni.X;
                desnigornjilevi.Y = desni.Y - CubeSize;
                Point desnidonjilevi = new Point();
                desnidonjilevi.X = desni.X;
                desnidonjilevi.Y = desni.Y + CubeSize;

                Point3D P2levigornjidesni3D = new Point3D(levigornjidesni.X, 0.002, levigornjidesni.Y);
                Point3D P1levidonjidesni3D = new Point3D(levidonjidesni.X, 0.002, levidonjidesni.Y);
                Point3D P6desnigornjilevi3D = new Point3D(desnigornjilevi.X, 0.002, desnigornjilevi.Y);
                Point3D P5desnidonjilevi3D = new Point3D(desnidonjilevi.X, 0.002, desnidonjilevi.Y);
                Point3D P4 = new Point3D(levigornjidesni.X, 0.02, levigornjidesni.Y);
                Point3D P3 = new Point3D(levidonjidesni.X, 0.02, levidonjidesni.Y);
                Point3D P8 = new Point3D(desnigornjilevi.X, 0.02, desnigornjilevi.Y);
                Point3D P7 = new Point3D(desnidonjilevi.X, 0.02, desnidonjilevi.Y);




                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P2levigornjidesni3D);   //0
                mesh.Positions.Add(P1levidonjidesni3D);   //1
                mesh.Positions.Add(P6desnigornjilevi3D);   //2
                mesh.Positions.Add(P5desnidonjilevi3D);   //3

                mesh.Positions.Add(P4); //4
                mesh.Positions.Add(P3); //5
                mesh.Positions.Add(P8); //6
                mesh.Positions.Add(P7); //7





                //Bottom Face
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

                //Fron Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);

                //Top Face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);

                //Back Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);



                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }
                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
            }
            else
            {
                Point gornji = new Point();
                Point donji = new Point();
                if (p1.Y <= p2.Y)
                {
                    gornji = p1;
                    donji = p2;
                }
                else
                {
                    gornji = p2;
                    donji = p1;
                }

                Point gornjidonjilevi = new Point();
                gornjidonjilevi.X = gornji.X - CubeSize;
                gornjidonjilevi.Y = gornji.Y;
                Point gornjidonjidesni = new Point();
                gornjidonjidesni.X = gornji.X + CubeSize;
                gornjidonjidesni.Y = gornji.Y;

                Point donjigornjilevi = new Point();
                donjigornjilevi.X = donji.X - CubeSize;
                donjigornjilevi.Y = donji.Y;
                Point donjigornjidesni = new Point();
                donjigornjidesni.X = donji.X + CubeSize;
                donjigornjidesni.Y = donji.Y;

                Point3D P5gornjidonjilevi3D = new Point3D(gornjidonjilevi.X, 0.002, gornjidonjilevi.Y);
                Point3D P6gornjidonjidesni3D = new Point3D(gornjidonjidesni.X, 0.002, gornjidonjidesni.Y);
                Point3D P1donjigornjilevi3D = new Point3D(donjigornjilevi.X, 0.002, donjigornjilevi.Y);
                Point3D P2donjigornjidesni3D = new Point3D(donjigornjidesni.X, 0.002, donjigornjidesni.Y);

                Point3D P3 = new Point3D(donjigornjilevi.X, 0.02, donjigornjilevi.Y);
                Point3D P4 = new Point3D(donjigornjidesni.X, 0.02, donjigornjidesni.Y);
                Point3D P7 = new Point3D(gornjidonjilevi.X, 0.02, gornjidonjilevi.Y);
                Point3D P8 = new Point3D(gornjidonjidesni.X, 0.02, gornjidonjidesni.Y);

                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P5gornjidonjilevi3D);   //0
                mesh.Positions.Add(P6gornjidonjidesni3D);   //1
                mesh.Positions.Add(P1donjigornjilevi3D);   //2
                mesh.Positions.Add(P2donjigornjidesni3D);   //3

                mesh.Positions.Add(P3); //4
                mesh.Positions.Add(P4); //5
                mesh.Positions.Add(P7); //6
                mesh.Positions.Add(P8); //7



                //Left Face
                mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4);


                //Top Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);

                //Bottom Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

                //Right Face
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(7);




                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }


                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
            }
        }

        public void DrawLine(Point p1,Point p2,string material,string pozicijaelementa,MyLineEntity mle)
        {
            if (Math.Abs(p1.X - p2.X) >= Math.Abs(p1.Y - p2.Y))
            {
                Point levi = new Point();
                Point desni = new Point();
                if (p1.X <= p2.X)
                {
                    levi = p1;
                    desni = p2;
                    if(pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "levi";
                    }else if(pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "desni";
                    }
                        

                }
                else
                {
                    levi = p2;
                    desni = p1;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "desni";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "levi";
                    }

                }
                Point levigornjidesni = new Point();
                Point levidonjidesni = new Point();
                Point desnigornjilevi = new Point();
                Point desnidonjilevi = new Point();
                if (pozicijaelementa.Equals("levi"))
                {
                    levigornjidesni.X = levi.X + CubeSize;
                    levigornjidesni.Y = levi.Y - CubeSize;
                    levidonjidesni.X = levi.X + CubeSize;
                    levidonjidesni.Y = levi.Y + CubeSize;
                    desnigornjilevi.X = desni.X;
                    desnigornjilevi.Y = desni.Y - CubeSize;
                    desnidonjilevi.X = desni.X;
                    desnidonjilevi.Y = desni.Y + CubeSize;
                }else if (pozicijaelementa.Equals("desni"))
                {
                    levigornjidesni.X = levi.X;
                    levigornjidesni.Y = levi.Y - CubeSize;
                    levidonjidesni.X = levi.X;
                    levidonjidesni.Y = levi.Y + CubeSize;  
                    desnigornjilevi.X = desni.X - CubeSize;
                    desnigornjilevi.Y = desni.Y - CubeSize;
                    desnidonjilevi.X = desni.X - CubeSize;
                    desnidonjilevi.Y = desni.Y + CubeSize;
                } else if(pozicijaelementa.Equals("both"))
                {
                    levigornjidesni.X = levi.X + CubeSize;
                    levigornjidesni.Y = levi.Y - CubeSize;
                    levidonjidesni.X = levi.X + CubeSize;
                    levidonjidesni.Y = levi.Y + CubeSize;
                    desnigornjilevi.X = desni.X - CubeSize;
                    desnigornjilevi.Y = desni.Y - CubeSize;
                    desnidonjilevi.X = desni.X - CubeSize;
                    desnidonjilevi.Y = desni.Y + CubeSize;
                }
                else
                {
                    levigornjidesni.X = levi.X;
                    levigornjidesni.Y = levi.Y - CubeSize;
                    levidonjidesni.X = levi.X;
                    levidonjidesni.Y = levi.Y + CubeSize;
                    desnigornjilevi.X = desni.X;
                    desnigornjilevi.Y = desni.Y - CubeSize;
                    desnidonjilevi.X = desni.X;
                    desnidonjilevi.Y = desni.Y + CubeSize;
                }
               

                Point3D P2levigornjidesni3D = new Point3D(levigornjidesni.X, CubeSize, levigornjidesni.Y);
                Point3D P1levidonjidesni3D = new Point3D(levidonjidesni.X, CubeSize, levidonjidesni.Y);
                Point3D P6desnigornjilevi3D = new Point3D(desnigornjilevi.X, CubeSize, desnigornjilevi.Y);
                Point3D P5desnidonjilevi3D = new Point3D(desnidonjilevi.X, CubeSize, desnidonjilevi.Y);
                Point3D P4 = new Point3D(levigornjidesni.X, CubeSize, levigornjidesni.Y);
                Point3D P3 = new Point3D(levidonjidesni.X, CubeSize, levidonjidesni.Y);
                Point3D P8 = new Point3D(desnigornjilevi.X, CubeSize, desnigornjilevi.Y);
                Point3D P7 = new Point3D(desnidonjilevi.X, CubeSize, desnidonjilevi.Y);




                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P2levigornjidesni3D);   //0
                mesh.Positions.Add(P1levidonjidesni3D);   //1
                mesh.Positions.Add(P6desnigornjilevi3D);   //2
                mesh.Positions.Add(P5desnidonjilevi3D);   //3

                mesh.Positions.Add(P4); //4
                mesh.Positions.Add(P3); //5
                mesh.Positions.Add(P8); //6
                mesh.Positions.Add(P7); //7





                //Bottom Face
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

                //Fron Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);

                //Top Face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);

                //Back Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6);



                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }
                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
                allelements.Add(mle);
                mylines.Add(mle);
            }
            else
            {
                Point gornji = new Point();
                Point donji = new Point();
                if (p1.Y <= p2.Y)
                {
                    gornji = p1;
                    donji = p2;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "gornji";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "donji";
                    }
                }
                else
                {
                    gornji = p2;
                    donji = p1;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "donji";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "gornji";
                    }
                }
                Point gornjidonjilevi = new Point();
                Point gornjidonjidesni = new Point();
                Point donjigornjilevi = new Point();
                Point donjigornjidesni = new Point();
                if (pozicijaelementa.Equals("gornji"))
                {
                    gornjidonjilevi.X = gornji.X - CubeSize;
                    gornjidonjilevi.Y = gornji.Y - CubeSize;

                    gornjidonjidesni.X = gornji.X + CubeSize;
                    gornjidonjidesni.Y = gornji.Y - CubeSize;


                    donjigornjilevi.X = donji.X - CubeSize;
                    donjigornjilevi.Y = donji.Y;

                    donjigornjidesni.X = donji.X + CubeSize;
                    donjigornjidesni.Y = donji.Y;
                }else if(pozicijaelementa.Equals("donji"))
                {
                    gornjidonjilevi.X = gornji.X - CubeSize;
                    gornjidonjilevi.Y = gornji.Y;

                    gornjidonjidesni.X = gornji.X + CubeSize;
                    gornjidonjidesni.Y = gornji.Y;


                    donjigornjilevi.X = donji.X - CubeSize;
                    donjigornjilevi.Y = donji.Y + CubeSize;

                    donjigornjidesni.X = donji.X + CubeSize;
                    donjigornjidesni.Y = donji.Y + CubeSize;
                }else if(pozicijaelementa.Equals("both"))
                {
                    gornjidonjilevi.X = gornji.X - CubeSize;
                    gornjidonjilevi.Y = gornji.Y - CubeSize;

                    gornjidonjidesni.X = gornji.X + CubeSize;
                    gornjidonjidesni.Y = gornji.Y - CubeSize;


                    donjigornjilevi.X = donji.X - CubeSize;
                    donjigornjilevi.Y = donji.Y + CubeSize;

                    donjigornjidesni.X = donji.X + CubeSize;
                    donjigornjidesni.Y = donji.Y + CubeSize;
                }
                else
                {
                    gornjidonjilevi.X = gornji.X - CubeSize;
                    gornjidonjilevi.Y = gornji.Y;

                    gornjidonjidesni.X = gornji.X + CubeSize;
                    gornjidonjidesni.Y = gornji.Y;


                    donjigornjilevi.X = donji.X - CubeSize;
                    donjigornjilevi.Y = donji.Y;

                    donjigornjidesni.X = donji.X + CubeSize;
                    donjigornjidesni.Y = donji.Y ;
                }

                
                

                Point3D P5gornjidonjilevi3D = new Point3D(gornjidonjilevi.X, CubeSize, gornjidonjilevi.Y);
                Point3D P6gornjidonjidesni3D = new Point3D(gornjidonjidesni.X, CubeSize, gornjidonjidesni.Y);
                Point3D P1donjigornjilevi3D = new Point3D(donjigornjilevi.X, CubeSize, donjigornjilevi.Y);
                Point3D P2donjigornjidesni3D = new Point3D(donjigornjidesni.X, CubeSize, donjigornjidesni.Y);

                Point3D P3 = new Point3D(donjigornjilevi.X, CubeSize, donjigornjilevi.Y);
                Point3D P4 = new Point3D(donjigornjidesni.X, CubeSize, donjigornjidesni.Y);
                Point3D P7 = new Point3D(gornjidonjilevi.X, CubeSize, gornjidonjilevi.Y);
                Point3D P8 = new Point3D(gornjidonjidesni.X, CubeSize, gornjidonjidesni.Y);

                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P5gornjidonjilevi3D);   //0
                mesh.Positions.Add(P6gornjidonjidesni3D);   //1
                mesh.Positions.Add(P1donjigornjilevi3D);   //2
                mesh.Positions.Add(P2donjigornjidesni3D);   //3

                mesh.Positions.Add(P3); //4
                mesh.Positions.Add(P4); //5
                mesh.Positions.Add(P7); //6
                mesh.Positions.Add(P8); //7



                //Left Face
                mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4);


                //Top Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);

                //Bottom Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

                //Right Face
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(7);




                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }


                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
                allelements.Add(mle);
                mylines.Add(mle);
            }
        }


        public void DrawLineReplica(Point p1, Point p2, string material, string pozicijaelementa, MyLineEntity mle)
        {
            


            if (Math.Abs(p1.Y - p2.Y) >= Math.Abs(p1.X - p2.X))
            {
                Point levi = new Point();
                Point desni = new Point();
                if (p1.Y <= p2.Y)
                {
                    levi = p1;
                    desni = p2;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "levi";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "desni";
                    }


                }
                else
                {
                    levi = p2;
                    desni = p1;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "desni";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "levi";
                    }

                }
                
                Point levigornjidesni = new Point();
                Point levidonjidesni = new Point();
                
                Point desnigornjilevi = new Point();
                Point desnidonjilevi = new Point();
                
                if (pozicijaelementa.Equals("levi"))
                {
                    levigornjidesni.X = levi.X - CubeSize/2;
                    levigornjidesni.Y = levi.Y + CubeSize/2;
                    
                    levidonjidesni.X = levi.X + CubeSize/2;
                    levidonjidesni.Y = levi.Y + CubeSize/2;
                    
                    
                    
                    desnigornjilevi.X = desni.X - CubeSize/2;
                    desnigornjilevi.Y = desni.Y ;
                    
                    desnidonjilevi.X = desni.X + CubeSize/2;
                    desnidonjilevi.Y = desni.Y ;
                    Console.WriteLine("OVDE USO");
                }
                else if (pozicijaelementa.Equals("desni"))
                {
                    levigornjidesni.X = levi.X - CubeSize/2;
                    levigornjidesni.Y = levi.Y;
                    
                    levidonjidesni.X = levi.X + CubeSize/2;
                    levidonjidesni.Y = levi.Y;
                    
                    
                    
                    desnigornjilevi.X = desni.X - CubeSize/2;
                    desnigornjilevi.Y = desni.Y - CubeSize/2;
                    
                    
                    desnidonjilevi.X = desni.X + CubeSize/2;
                    desnidonjilevi.Y = desni.Y - CubeSize/2;
                }
                else if (pozicijaelementa.Equals("both"))
                {
                    levigornjidesni.X = levi.X - CubeSize/2;
                    levigornjidesni.Y = levi.Y + CubeSize/2;
                    
                    levidonjidesni.X = levi.X + CubeSize/2;
                    levidonjidesni.Y = levi.Y + CubeSize/2;
                    
                    
                    
                    
                    desnigornjilevi.X = desni.X - CubeSize/2;
                    desnigornjilevi.Y = desni.Y - CubeSize/2;
                    
                    desnidonjilevi.X = desni.X + CubeSize/2;
                    desnidonjilevi.Y = desni.Y - CubeSize/2;
                }
                else
                {
                    
                    levigornjidesni.X = levi.X - CubeSize/2;
                    levigornjidesni.Y = levi.Y ;
                    
                    levidonjidesni.X = levi.X + CubeSize /2;
                    levidonjidesni.Y = levi.Y ;
                    
                    desnigornjilevi.X = desni.X - CubeSize /2;
                    desnigornjilevi.Y = desni.Y ;
                    
                    desnidonjilevi.X = desni.X +CubeSize/2;
                    desnidonjilevi.Y = desni.Y;
                }


                Point3D P2levigornjidesni3D = new Point3D(levigornjidesni.Y, 0, levigornjidesni.X);
                Point3D P1levidonjidesni3D = new Point3D(levidonjidesni.Y, 0, levidonjidesni.X);
                Point3D P6desnigornjilevi3D = new Point3D(desnigornjilevi.Y, 0, desnigornjilevi.X);
                Point3D P5desnidonjilevi3D = new Point3D(desnidonjilevi.Y, 0, desnidonjilevi.X);
                Point3D P4 = new Point3D(levigornjidesni.Y, CubeSize, levigornjidesni.X);
                Point3D P3 = new Point3D(levidonjidesni.Y, CubeSize, levidonjidesni.X);
                Point3D P8 = new Point3D(desnigornjilevi.Y, CubeSize, desnigornjilevi.X);
                Point3D P7 = new Point3D(desnidonjilevi.Y, CubeSize, desnidonjilevi.X);




                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P2levigornjidesni3D);   //0
                mesh.Positions.Add(P1levidonjidesni3D);   //1
                mesh.Positions.Add(P6desnigornjilevi3D);   //2
                mesh.Positions.Add(P5desnidonjilevi3D);   //3

                mesh.Positions.Add(P4); //4
                mesh.Positions.Add(P3); //5
                mesh.Positions.Add(P8); //6
                mesh.Positions.Add(P7); //7





                //Bottom Face
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

                //Fron Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);

                //Top Face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);

                //Back Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6);



                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }
                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
                allelements.Add(mle);
                mylines.Add(mle);
            }
            else
            {
                Point gornji = new Point();
                Point donji = new Point();
                if (p1.X <= p2.X)
                {
                    gornji = p1;
                    donji = p2;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "gornji";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "donji";
                    }
                }
                else
                {
                    gornji = p2;
                    donji = p1;
                    if (pozicijaelementa.Equals("first"))
                    {
                        pozicijaelementa = "donji";
                    }
                    else if (pozicijaelementa.Equals("second"))
                    {
                        pozicijaelementa = "gornji";
                    }
                }
                Point gornjidonjilevi = new Point();
                Point gornjidonjidesni = new Point();
                Point donjigornjilevi = new Point();
                Point donjigornjidesni = new Point();
                if (pozicijaelementa.Equals("gornji"))
                {
                    gornjidonjilevi.X = gornji.X + CubeSize/2;
                    gornjidonjilevi.Y = gornji.Y - CubeSize/2;

                    gornjidonjidesni.X = gornji.X + CubeSize/2;
                    gornjidonjidesni.Y = gornji.Y + CubeSize/2;


                    donjigornjilevi.X = donji.X;
                    donjigornjilevi.Y = donji.Y - CubeSize/2;

                    donjigornjidesni.X = donji.X ;
                    donjigornjidesni.Y = donji.Y + CubeSize/2;
                    Console.WriteLine("IPAK OVDE USO");
                }
                else if (pozicijaelementa.Equals("donji"))
                {
                    gornjidonjilevi.X = gornji.X ;
                    gornjidonjilevi.Y = gornji.Y - CubeSize/2;

                    gornjidonjidesni.X = gornji.X ;
                    gornjidonjidesni.Y = gornji.Y + CubeSize/2;


                    donjigornjilevi.X = donji.X - CubeSize/2;
                    donjigornjilevi.Y = donji.Y - CubeSize/2;

                    donjigornjidesni.X = donji.X - CubeSize/2;
                    donjigornjidesni.Y = donji.Y + CubeSize/2;
                }
                else if (pozicijaelementa.Equals("both"))
                {
                    gornjidonjilevi.X = gornji.X + CubeSize/2;
                    gornjidonjilevi.Y = gornji.Y - CubeSize/2;

                    gornjidonjidesni.X = gornji.X + CubeSize/2;
                    gornjidonjidesni.Y = gornji.Y + CubeSize/2;


                    donjigornjilevi.X = donji.X - CubeSize/2;
                    donjigornjilevi.Y = donji.Y - CubeSize/2;

                    donjigornjidesni.X = donji.X - CubeSize/2;
                    donjigornjidesni.Y = donji.Y + CubeSize/2;
                }
                else
                {
                    gornjidonjilevi.X = gornji.X ;
                    gornjidonjilevi.Y = gornji.Y - CubeSize/2;

                    gornjidonjidesni.X = gornji.X ;
                    gornjidonjidesni.Y = gornji.Y + CubeSize/2;


                    donjigornjilevi.X = donji.X ;
                    donjigornjilevi.Y = donji.Y - CubeSize/2;

                    donjigornjidesni.X = donji.X ;
                    donjigornjidesni.Y = donji.Y + CubeSize/2;
                }




                Point3D P5gornjidonjilevi3D = new Point3D(gornjidonjilevi.Y, 0, gornjidonjilevi.X);
                Point3D P6gornjidonjidesni3D = new Point3D(gornjidonjidesni.Y, 0, gornjidonjidesni.X);
                Point3D P1donjigornjilevi3D = new Point3D(donjigornjilevi.Y, 0, donjigornjilevi.X);
                Point3D P2donjigornjidesni3D = new Point3D(donjigornjidesni.Y, 0, donjigornjidesni.X);

                Point3D P3 = new Point3D(donjigornjilevi.Y, CubeSize, donjigornjilevi.X);
                Point3D P4 = new Point3D(donjigornjidesni.Y, CubeSize, donjigornjidesni.X);
                Point3D P7 = new Point3D(gornjidonjilevi.Y, CubeSize, gornjidonjilevi.X);
                Point3D P8 = new Point3D(gornjidonjidesni.Y, CubeSize, gornjidonjidesni.X);

                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(P5gornjidonjilevi3D);   //0
                mesh.Positions.Add(P6gornjidonjidesni3D);   //1
                mesh.Positions.Add(P1donjigornjilevi3D);   //2
                mesh.Positions.Add(P2donjigornjidesni3D);   //3

                mesh.Positions.Add(P3); //4
                mesh.Positions.Add(P4); //5
                mesh.Positions.Add(P7); //6
                mesh.Positions.Add(P8); //7



                //Left Face
                mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(4);


                //Top Face
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);

                //Bottom Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

                //Right Face
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(7);




                ModelVisual3D modela = new ModelVisual3D();
                DiffuseMaterial mat;
                if (material.Equals("Steel"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(153, 0, 153)));
                }
                else if (material.Equals("Copper"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 128, 0)));
                }
                else if (material.Equals("Acsr"))
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)));
                }
                else
                {
                    mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(102, 51, 0)));
                }


                GeometryModel3D path = new GeometryModel3D(mesh, mat);
                models.Add(path);
                modela.Content = path;

                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = modela.Content;

                viewport1.Children.Add(ModelUI);
                allelements.Add(mle);
                mylines.Add(mle);
            }
        }











        public void DrawLines()
        {
            for(int i=0;i<lines.Count;i++)
            {
                if(!lines[i].IsUnderground)
                {
                    if (lines[i].Vertices.Count > 0)
                    {
                        Point start = FindBoardPointsById(lines[i].FirstEnd);
                        Point end = FindBoardPointsById(lines[i].SecondEnd);
                        Point vert = new Point();
                        vert.X = CalculateBoardZ(lines[i].Vertices[0].X);
                        vert.Y = CalculateBoardX(lines[i].Vertices[0].Y);
                        DrawLineReplica(start, vert, lines[i].ConductorMaterial, "first", lines[i]);


                        for (int j = 0; j < lines[i].Vertices.Count - 1; j++)
                        {
                            Point vert1 = new Point();
                            vert1.X = CalculateBoardZ(lines[i].Vertices[j].X);
                            vert1.Y = CalculateBoardX(lines[i].Vertices[j].Y);
                            Point vert2 = new Point();
                            vert2.X = CalculateBoardZ(lines[i].Vertices[j + 1].X);
                            vert2.Y = CalculateBoardX(lines[i].Vertices[j + 1].Y);
                            DrawLineReplica(vert1, vert2, lines[i].ConductorMaterial, "none", lines[i]);
                        }

                        Point vertn = new Point();
                        vertn.X = CalculateBoardZ(lines[i].Vertices[lines[i].Vertices.Count - 1].X);
                        vertn.Y = CalculateBoardX(lines[i].Vertices[lines[i].Vertices.Count - 1].Y);
                        DrawLineReplica(vertn, end, lines[i].ConductorMaterial, "second", lines[i]);
                    }

                    else
                    {
                        Point start = FindBoardPointsById(lines[0].FirstEnd);
                        Point end = FindBoardPointsById(lines[0].SecondEnd);
                        DrawLineReplica(start, end, lines[0].ConductorMaterial, "both", lines[i]);
                    }   
                }
                
            }
        }

        public void drawvertices(int k)
        {
            for(int i=0;i<lines[k].Vertices.Count;i++)
            {
                Point3D p = new Point3D();
                p.Z = CalculateBoardZ(lines[k].Vertices[i].X);
                p.X = CalculateBoardX(lines[k].Vertices[i].Y);
                p.Y = 0;
                boardpointsvertices.Add(p);
            }

            for(int j=0;j<boardpointsvertices.Count;j++)
            {
                double z1, z2, x1, x2, y1, y2;

                z1 = boardpointsvertices[j].Z - CubeSize / 2;
                z2 = boardpointsvertices[j].Z + CubeSize / 2;
                x1 = boardpointsvertices[j].X - CubeSize / 2;
                x2 = boardpointsvertices[j].X + CubeSize / 2;

                y1 = 0;
                y2 = CubeSize;


                Point3D t1 = new Point3D(x1, y1, z1);
                Point3D t2 = new Point3D(x2, y1, z1);
                Point3D t3 = new Point3D(x1, y1, z2);
                Point3D t4 = new Point3D(x2, y1, z2);

                Point3D t12 = new Point3D(x1, y2, z1);
                Point3D t22 = new Point3D(x2, y2, z1);
                Point3D t32 = new Point3D(x1, y2, z2);
                Point3D t42 = new Point3D(x2, y2, z2);



                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(t1);   //0
                mesh.Positions.Add(t2);   //1
                mesh.Positions.Add(t3);   //2
                mesh.Positions.Add(t4);   //3
                mesh.Positions.Add(t12);  //4
                mesh.Positions.Add(t22);  //5
                mesh.Positions.Add(t32);  //6
                mesh.Positions.Add(t42);  //7






                //Bottom Face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1);


                //Fron face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7);

                //Left face
                mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4);

                //Right Face
                mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7);

                //Back Face
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(5);

                //Upper Face
                mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(5);

                Material mat = new DiffuseMaterial();
                mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 0, 127)));


                GeometryModel3D cube = new GeometryModel3D(mesh, mat);
                models.Add(cube);
                ModelUIElement3D ModelUI = new ModelUIElement3D();
                ModelUI.Model = cube;
                viewport1.Children.Add(ModelUI);
            }
        }










        public void DrawFristLine()
        {
            int k = 0;
            
                if (lines[k].Vertices.Count > 0)
                {
                    Point start = FindBoardPointsById(lines[k].FirstEnd);
                    Point end = FindBoardPointsById(lines[k].SecondEnd);
                    Point vert = new Point();
                    vert.X = CalculateBoardZ(lines[k].Vertices[0].X);
                    vert.Y = CalculateBoardX(lines[k].Vertices[0].Y);
                    DrawLineReplica(start, vert, lines[k].ConductorMaterial, "first", lines[k]);


                    for (int j = 0; j < lines[k].Vertices.Count - 1; j++)
                    {
                        Point vert1 = new Point();
                        vert1.X = CalculateBoardZ(lines[k].Vertices[j].X);
                        vert1.Y = CalculateBoardX(lines[k].Vertices[j].Y);
                        Point vert2 = new Point();
                        vert2.X = CalculateBoardZ(lines[k].Vertices[j + 1].X);
                        vert2.Y = CalculateBoardX(lines[k].Vertices[j + 1].Y);
                        DrawLineReplica(vert1, vert2, lines[k].ConductorMaterial, "none", lines[k]);
                    }

                    Point vertn = new Point();
                    vertn.X = CalculateBoardZ(lines[k].Vertices[lines[k].Vertices.Count - 1].X);
                    vertn.Y = CalculateBoardX(lines[k].Vertices[lines[k].Vertices.Count - 1].Y);
                    DrawLineReplica(vertn, end, lines[k].ConductorMaterial, "second", lines[k]);
                }

                else
                {
                    Point start = FindBoardPointsById(lines[k].FirstEnd);
                    Point end = FindBoardPointsById(lines[k].SecondEnd);
                    DrawLineReplica(start, end, lines[k].ConductorMaterial, "both", lines[k]);
                }
            
        }

        public void DrawPathsWithoutVertecies()
        {
            /*
            foreach (MyLineEntity le in lines)
            {
                Point start = FindBoardPointsById(le.FirstEnd);
                Point end = FindBoardPointsById(le.SecondEnd);
                DrawPath(start, end,le.ConductorMaterial);
            }
            */
            
            Point start = FindBoardPointsById(lines[0].FirstEnd);
            Point end = FindBoardPointsById(lines[0].SecondEnd);
            DrawPath(start, end, lines[0].ConductorMaterial);
        }
        
        
        public Point FindBoardPointsById(long id)
        {
            Point boardpoint = new Point();
            foreach (PowerEntity pe in elements)
            {
                if (pe.Id == id)
                {
                    boardpoint.X = pe.BoardZ;
                    boardpoint.Y = pe.BoardX;
                }
            }
            return boardpoint;
        }
        
        private void Od0Do3_Checked(object sender, RoutedEventArgs e)
        {
            for(int i=0;i<elements.Count;i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if(br_veza<3)
                {
                    ModelUIElement3D model = viewport1.Children[i+3] as ModelUIElement3D;
                    model.Visibility = Visibility.Hidden;
                  
                    
                    
                }
            }
           
           

        }



        private void Od0Do3_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if (br_veza < 3)
                {
                    ModelUIElement3D model = viewport1.Children[i + 3] as ModelUIElement3D;
                    model.Visibility = Visibility.Visible;
                }
            }
        }

        private void Od3Do5_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if (br_veza > 2 && br_veza < 6)
                {
                    ModelUIElement3D model = viewport1.Children[i + 3] as ModelUIElement3D;
                    model.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Od3Do5_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if (br_veza > 2 && br_veza < 6)
                {
                    ModelUIElement3D model = viewport1.Children[i + 3] as ModelUIElement3D;
                    model.Visibility = Visibility.Visible;
                }
            }
        }

        private void Preko5_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if (br_veza > 5)
                {
                    ModelUIElement3D model = viewport1.Children[i + 3] as ModelUIElement3D;
                    model.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Preko5_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                int br_veza = CalculateNumberOfConnections(elements[i].Id);
                if (br_veza > 5)
                {
                    ModelUIElement3D model = viewport1.Children[i + 3] as ModelUIElement3D;
                    model.Visibility = Visibility.Visible;
                }
            }
        }

        private void Od0Do1_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if(mylines[i].R<=1)
                {
                        ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                        linija.Visibility = Visibility.Hidden;
                }
                
            }
        }

        private void Od0Do1_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if (mylines[i].R <= 1)
                {
                    ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                    linija.Visibility = Visibility.Visible;
                    
                }

            }
        }

        private void Od1Do2_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if (mylines[i].R <= 2 && mylines[i].R>1)
                {
                    ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                    linija.Visibility = Visibility.Hidden;
                    
                }

            }

        }

        private void Od1Do2_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if (mylines[i].R <= 2 && mylines[i].R > 1)
                {
                    ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                    linija.Visibility = Visibility.Visible;
                   
                }

            }
        }

        private void Preko2_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if (mylines[i].R > 2)
                {
                    ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                    linija.Visibility = Visibility.Hidden;
                    
                }

            }
        }

        private void Preko2_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                if (mylines[i].R > 2)
                {
                    ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                    linija.Visibility = Visibility.Visible;
                    
                }

            }
        }

        private void Otvoren_Checked(object sender, RoutedEventArgs e)
        {
            for(int i=0;i< mylines.Count;i++)
            {
                foreach(SwitchEntity se in switches)
                {
                    if (((se.Id== mylines[i].FirstEnd) || (se.Id== mylines[i].SecondEnd)) && se.Status.Equals("Open"))
                    {
                        ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                        linija.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void Otvoren_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mylines.Count; i++)
            {
                foreach (SwitchEntity se in switches)
                {
                    if (((se.Id == mylines[i].FirstEnd) || (se.Id == mylines[i].SecondEnd)) && se.Status.Equals("Open"))
                    {
                        ModelUIElement3D linija = viewport1.Children[2048 + i] as ModelUIElement3D;
                        linija.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }

}
