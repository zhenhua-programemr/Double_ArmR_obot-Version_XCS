using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using Lmi3d.GoSdk;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using Lmi3d.GoSdk.Messages;

using AnyCAD.Forms;
using AnyCAD.Foundation;


using Kitware.VTK;
namespace Double_Arm_Robot
{
    public struct Point
    {
        public double x;
        public double y;
        public double z;
    }
    public class DataContext
    {

        public double xResolution;
        public double yResolution;
        public double zResolution;
        public double xOffset;
        public double yOffset;
        public double zOffset;
        public uint serialNumber;
    }
    class Gocater
    {
        public const string SENSOR_IP = "192.168.1.10";
        public GoSystem system = null;
        static Float32Buffer mPositions;
        static Float32Buffer mColor;
        public List<Point> listPointcloud;
        public vtkPoints points = vtkPoints.New();
        public void initial()
        {
            try
            {
                KApiLib.Construct();
                GoSdkLib.Construct();
                system = new GoSystem();
                GoSensor sensor;
                KIpAddress ipAddress = KIpAddress.Parse(SENSOR_IP);
                sensor = system.FindSensorByIpAddress(ipAddress);
                sensor.Connect();
                GoSetup setup = sensor.Setup;
                setup.ScanMode = GoMode.Surface;
                system.EnableData(true);
                standardPoint();
                Console.WriteLine("线激光初始化完成");
            }
            catch(KException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
        }

        public void SavePCD(string path, List<Point> listPointCloud)
        {
            try
            {
                StreamWriter pcdFile = new StreamWriter(path);
                pcdFile.WriteLine("# .PCD v0.7 - Point Cloud Data file format");
                pcdFile.WriteLine("VERSION 0.7");
                pcdFile.WriteLine("FIELDS x y z");
                pcdFile.WriteLine("SIZE 4 4 4");
                pcdFile.WriteLine("TYPE F F F");
                pcdFile.WriteLine("COUNT 1 1 1");
                pcdFile.WriteLine("WIDTH " + listPointCloud.Count.ToString());
                pcdFile.WriteLine("HEIGHT 1");
                pcdFile.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
                pcdFile.WriteLine("POINTS " + listPointCloud.Count.ToString());
                pcdFile.WriteLine("DATA ascii");
                foreach (Point p in listPointCloud)
                {
                    pcdFile.Write(p.x + " " + p.y + " " + p.z + "\r\n");
                }
                pcdFile.Flush();
                pcdFile.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void startScan()
        {
            try
            {
                listPointcloud = new List<Point>();
                GoDataSet dataSet = new GoDataSet();
                system.Start();
                Console.WriteLine("Waiting for Whole Part Data...");
                dataSet = system.ReceiveData(100000000);
                DataContext context = new DataContext();
                

                //下边的position和color是anycad中的参数
                mPositions = new Float32Buffer(0);
                mColor = new Float32Buffer(0);
                for (UInt32 i = 0; i < dataSet.Count; i++)
                {
                    GoDataMsg dataObj = (GoDataMsg)dataSet.Get(i);
                    switch (dataObj.MessageType)
                    {
                        case GoDataMessageType.Stamp:
                            {
                                GoStampMsg stampMsg = (GoStampMsg)dataObj;
                                for (UInt32 j = 0; j < stampMsg.Count; j++)
                                {
                                    GoStamp stamp = stampMsg.Get(j);
                                    Console.WriteLine("Frame Index = {0}", stamp.FrameIndex);
                                    Console.WriteLine("Time Stamp = {0}", stamp.Timestamp);
                                    Console.WriteLine("Encoder Value = {0}", stamp.Encoder);
                                }
                            }
                            break;
                        case GoDataMessageType.UniformSurface:
                            {
                                GoSurfaceMsg surfaceMsg = (GoSurfaceMsg)dataObj;
                                Point pointcloud = new Point();

                                context.xResolution = (double)surfaceMsg.XResolution / 1000000;
                                context.yResolution = (double)surfaceMsg.YResolution / 1000000;
                                context.zResolution = (double)surfaceMsg.ZResolution / 1000000;
                                context.xOffset = (double)surfaceMsg.XOffset / 1000;
                                context.yOffset = (double)surfaceMsg.YOffset / 1000;
                                context.zOffset = (double)surfaceMsg.ZOffset / 1000;
                                long width = surfaceMsg.Width;
                                long length = surfaceMsg.Length;
                                long bufferSize = width * length;
                                IntPtr bufferPointer = surfaceMsg.Data;

                                Console.WriteLine("Whole Part Height Map received:");
                                Console.WriteLine(" Buffer width: {0}", width);
                                Console.WriteLine(" Buffer length: {0}", length);

                                short[] ranges = new short[bufferSize];
                                Marshal.Copy(bufferPointer, ranges, 0, ranges.Length);

                                long colIdx, rowIdx;
                                    for (rowIdx = 0; rowIdx < length; rowIdx++)
                                    {

                                        for (colIdx = 0; colIdx < width; colIdx++)
                                        {
                                            long index = rowIdx * width + colIdx;
                                            pointcloud.z = context.zOffset + context.zResolution * ranges[index];
                                            if (pointcloud.z != -124.5184)
                                            {
                                                pointcloud.x = context.xOffset + context.xResolution * colIdx;
                                                pointcloud.y = context.yOffset + context.yResolution * rowIdx;
                                                listPointcloud.Add(pointcloud);

                                                //anyCAD显示点云
                                                mPositions.Append(Convert.ToSingle(pointcloud.x));
                                                mPositions.Append(Convert.ToSingle(pointcloud.y));
                                                mPositions.Append(Convert.ToSingle(pointcloud.z));
                                                mColor.Append(0);
                                                mColor.Append(0);
                                                mColor.Append(1);
                                                points.InsertNextPoint(pointcloud.x, pointcloud.y, pointcloud.z);
                                            }
                                        }
                                    }
                            }
                            break;
                        case GoDataMessageType.SurfacePointCloud:
                            {
                                GoSurfacePointCloudMsg surfaceMsg = (GoSurfacePointCloudMsg)dataObj;
                                long width = surfaceMsg.Width;
                                long length = surfaceMsg.Length;
                                long bufferSize = width * length;
                                IntPtr bufferPointer = surfaceMsg.Data;

                                Console.WriteLine("Whole Part Height Map received:");
                                Console.WriteLine(" Buffer width: {0}", width);
                                Console.WriteLine(" Buffer length: {0}", length);

                                short[] ranges = new short[bufferSize];
                                Marshal.Copy(bufferPointer, ranges, 0, ranges.Length);

                            }
                            break;
                        case GoDataMessageType.SurfaceIntensity:
                            {
                                GoSurfaceIntensityMsg surfaceMsg = (GoSurfaceIntensityMsg)dataObj;
                                long width = surfaceMsg.Width;
                                long length = surfaceMsg.Length;
                                long bufferSize = width * length;
                                IntPtr bufferPointeri = surfaceMsg.Data;

                                Console.WriteLine("Whole Part Intensity Image received:");
                                Console.WriteLine(" Buffer width: {0}", width);
                                Console.WriteLine(" Buffer length: {0}", length);
                                byte[] ranges = new byte[bufferSize];
                                Marshal.Copy(bufferPointeri, ranges, 0, ranges.Length);
                            }
                            break;
                    }
                }
                system.Stop();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void Run(RenderControl render)
        {
            try
            {
                //if (!ReadData())
                //    return;
                //mPositions.Append(0, 0, 0);
                //mColor.Append(0, 0, 1);
                //mPositions.Append(0, 1, 0);
                //mColor.Append(0, 0, 1);
                //mPositions.Append(0, 0, 1);
                //mColor.Append(0, 0, 1);

                //PointCloud node = PointCloud.Create(mPositions, mColor, 1);
                //render.ShowSceneNode(node);
                //render.ZoomAll();
                //render.SetBackgroundColor(0, 0, 0, 0);
                render.ClearAll();
                var material = BasicMaterial.Create("point-material");
                material.GetTemplate().SetVertexColors(true);

                var geometry = GeometryBuilder.CreatePoints(new Float32Array(mPositions), new Float32Array(mColor));

                var node = new PrimitiveSceneNode(geometry, material);

                //node.SetPickable(false);
                render.ShowSceneNode(node);
                render.ZoomAll();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void standardPoint()
        {
            listPointcloud = new List<Point>();
            Point pointcloud = new Point();
            double colIdx, rowIdx;
            for (rowIdx = 0; rowIdx <= 35; rowIdx += 0.2)
            {

                for (colIdx = 0; colIdx <= 30; colIdx += 0.2)
                {
                    pointcloud.z = 0;
                    pointcloud.x = rowIdx;
                    pointcloud.y = colIdx;
                    listPointcloud.Add(pointcloud);
                }
            }
            StreamWriter pcdFile = new StreamWriter("standardCloud.pcd");
            pcdFile.WriteLine("# .PCD v0.7 - Point Cloud Data file format");
            pcdFile.WriteLine("VERSION 0.7");
            pcdFile.WriteLine("FIELDS x y z");
            pcdFile.WriteLine("SIZE 4 4 4");
            pcdFile.WriteLine("TYPE F F F");
            pcdFile.WriteLine("COUNT 1 1 1");
            pcdFile.WriteLine("WIDTH " + listPointcloud.Count.ToString());
            pcdFile.WriteLine("HEIGHT 1");
            pcdFile.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
            pcdFile.WriteLine("POINTS " + listPointcloud.Count.ToString());
            pcdFile.WriteLine("DATA ascii");
            foreach (Point p in listPointcloud)
            {
                pcdFile.Write(p.x + " " + p.y + " " + p.z + "\r\n");
            }
            pcdFile.Flush();
            pcdFile.Close();
        }
    }
}
