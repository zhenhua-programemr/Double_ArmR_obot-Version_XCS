using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using AnyCAD.Forms;
using AnyCAD.Exchange;
using AnyCAD.Foundation;

using Kitware.VTK;
namespace Double_Arm_Robot
{
    public partial class MainForm : Form
    {
        private Platform myPlatform;
        private Gocater myGocater;
        private ABBRobot myABBRobot1;
        bool isInitial_Ok = false;
        string btnText_init = "初始化";
        string btnText_inited = "已初始化";
        static int currentCount = 0;
        static int maxCount = 30;
        Bitmap redLight = new Bitmap("./circle_red.png");
        Bitmap greenLight = new Bitmap("./circle_green.png");
        //相对路径引用时..//..//表示在可执行文件回跳两级然后进入后边的路径下，./表示当前exe所在路径
        RenderControl mRenderControl;
        vtkPoints points = new vtkPoints();
        public static MainForm myForm;
        //RenderWindowControl renderWindowControl1;
        
        public MainForm()
        {
            try
            {
                InitializeComponent();
                myForm = this;
                appendLog("欢迎使用该软件，请将机器人控制柜切换到自动模式，然后进行初始化", 4);
                mRenderControl = new RenderControl(this.panel_DisplayPointCloud);
                this.combo_SerialNum.Text = "COM5";
                this.textBox_PolishCount.Text = "1";
                this.textBox_PolishSpeed.Text = "7.9mm/s";
                this.comboBo_PolishPressure.Text = "10N";
                this.combo_SDJSpeed.Text = @"12000r/min";
                //renderWindowControl1 = new RenderWindowControl();
                //renderWindowControl1.Dock = DockStyle.Fill;
                //tabPage7.Controls.Add(renderWindowControl1);
            }
            catch(Exception ex)
            {
                appendLog(ex.Message,1);
            }
        }
        private void btn_Initial_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (!isInitial_Ok && btn_Initial.Text == btnText_init)
                {
                    //SimplePointsReader();
                    myABBRobot1 = new ABBRobot();
                    //myPlatform = new Platform();
                    //myPlatform.inital(combo_SerialNum.Text);
                    //label_PlatForm.Image = greenLight;
                    myABBRobot1.init();
                    myABBRobot1.return_StartPoint();
                    //label_RobotLeft.Image = greenLight;
                    //myGocater = new Gocater();
                    ////myGocater.standardPoint();
                    //myGocater.initial();
                    label_Laser.Image = greenLight;
                    appendLog("滑台和线激光已成功连接", 3);
                    appendLog("机器人初始化完成，点击加载文件便可添加磨抛路径，如果文件已存在可以直接点击开始研抛",4);
                    btn_Initial.Text = btnText_inited;
                    isInitial_Ok = true;
                    label_Initialed.Image = greenLight;
                    //btn_Initial.BackgroundImage = new Bitmap("./GreenBackground.png");
                }
                else if (isInitial_Ok && btn_Initial.Text == btnText_inited)
                {
                    btn_Initial.Text = btnText_init;
                    btn_Initial.BackgroundImage = new Bitmap("./WhiteBackground.png");
                    isInitial_Ok = false;
                    label_Initialed.Image = redLight;
                }
            }
            catch (Exception ex)
            {
                appendLog(ex.Message, 1);
                //MessageBox.Show(ex.Message);
            }
        }
        public void appendLog(string log, int type)
        {
            try
            {
                if (currentCount > maxCount)
                {
                    richTextBox1.Clear();
                    currentCount = 0;
                }
                string Time = Convert.ToString(DateTime.Now);
                switch (type)
                {
                    case 0:
                        richTextBox1.SelectionColor = Color.Gray;
                        break;
                    case 1:
                        richTextBox1.SelectionColor = Color.Red;
                        break;
                    case 2:
                        richTextBox1.SelectionColor = Color.Orange;
                        break;
                    case 3:
                        richTextBox1.SelectionColor = Color.Green;
                        break;
                    default:
                        richTextBox1.SelectionColor = Color.Black;
                        break;

                }
                this.richTextBox1.Invoke(new EventHandler(delegate
                {
                    this.richTextBox1.AppendText("<< " + Time + " " + log + "\n");
                }));
                currentCount += 1;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private void btn_LoadModFIle_Click(object sender, EventArgs e)
        {
            try
            {
                myABBRobot1.trans_PCModFile_ToController();
                label_LoadPath.Image = greenLight;
                appendLog("已成功加载路径，可点击开始磨抛按钮开始工作",3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                appendLog(ex.Message,1);
            }
        }

        private void btn_StartPolish_Click(object sender, EventArgs e)
        {
            try
            {
                label_LoadPath.Image = greenLight;
                myABBRobot1.abb_MotorOn();
                myABBRobot1.pp_ToMain();
                myABBRobot1.startRun_RapidProgram();
                label_Polishing.Image = greenLight;
                appendLog("机器人开始研抛工作，请勿重复点击此按钮", 3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_StopWork_Click(object sender, EventArgs e)
        {
            try
            {
                myABBRobot1.stopRun_RapidProgram();
                myABBRobot1.abb_MotorOff();
                label_Polishing.Image = redLight;
                appendLog("研抛工作终止，点击继续研抛可以继续工作", 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btn_ScanCloud_Click(object sender, EventArgs e)
        {
            try
            {
                //SimplePointsReader();
                appendLog("开始扫描点云,请勿进行其他操作", 3);
                Task task1 = new Task(() =>
                {
                    myPlatform.run();
                    myGocater.startScan();
                    myGocater.SavePCD("saved.pcd",myGocater.listPointcloud);
                    myGocater.Run(mRenderControl);
                    Action action = () =>
                    {

                            appendLog("点云扫描完成", 3);
                    };
                });
                task1.Start();
                label_ScanSucc.Image = greenLight;
            }
            catch(Exception ex)
            {
                if(myGocater.system != null)
                {
                    myGocater.system.Stop();
                }
                appendLog(ex.Message,1);
            }
        }
        //private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{

        //}
        private void MainForm_Load(object sender, EventArgs e)
        {
            //SimplePointsReader();
            //VTKShowCloud1();
            mRenderControl.SetBackgroundColor(255, 255, 255, 0);
        }

        private void bun_ReturnStartPoint_Click(object sender, EventArgs e)
        {
            try
            {
                myABBRobot1.stopRun_RapidProgram();
                myABBRobot1.abb_MotorOn();
                myABBRobot1.return_StartPoint();
                myABBRobot1.startRun_RapidProgram();
                appendLog("机器人已返回到初始位置", 3);
            }
            catch(Exception ex)
            {
                appendLog(ex.Message, 1);
            }
        }

        private void btn_ContinueWork_Click(object sender, EventArgs e)
        {
            try
            {
                myABBRobot1.abb_MotorOn();
                myABBRobot1.startRun_RapidProgram();
                label_Polishing.Image = greenLight;
                appendLog("机器人继续工作", 3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void btn_BiaoDing_Click(object sender, EventArgs e)
        {

        }

        private void btn_RotateScan_Click(object sender, EventArgs e)
        {
            try
            {
                appendLog("开始旋转扫描点云,请勿进行其他操作", 1);
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择点云文件的保存路径";
                string foldPath = "";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foldPath = dialog.SelectedPath + @"\";
                }
                else
                {
                    foldPath = "./RotateScanPointCloud/";
                }
                myABBRobot1.abb_MotorOn();
                myABBRobot1.RAX_6To180();
                myABBRobot1.startRun_RapidProgram();
                Task task1 = new Task(() =>
                {
                    for (int count = 1; count <= 12; count++)
                    {
                        myPlatform.run();
                        //appendLog("第" + count.ToString() + "次点云扫描-启动滑台", 3);
                        myGocater.startScan();
                        //appendLog("第" + count.ToString() + "次点云扫描-启动激光", 3);
                        myGocater.SavePCD(foldPath + ((count-1)*30-180).ToString()+"°.pcd", myGocater.listPointcloud);
                        myGocater.Run(mRenderControl);
                        //appendLog("第" + count.ToString() + "次点云扫描-显示点云", 3);
                        Action action = () =>
                        {
                            appendLog("第" + count.ToString() + "次点云扫描完成，点云文件已保存", 3);
                        };
                        Invoke(action);

                        Task.Delay(32000).Wait();
                        myABBRobot1.RAX_6Add30Per();
                        myABBRobot1.startRun_RapidProgram();
                        Task.Delay(2000).Wait();
                    }

                    Action action1 = () =>
                    {
                        appendLog("点云扫描完成", 3);
                    };
                    Invoke(action1);
                });
                task1.Start();
                appendLog("旋转扫描点云任务已启动", 3);
            }
            catch(Exception ex)
            {
                appendLog("点云扫描出现异常，异常为：" + ex.Message, 1);
            }
        }

        private void btn_SelectFileDialog_Click(object sender, EventArgs e)
        {
            
        }

        private void combo_SerialNum_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {





















        }
        //private void VTKShowCloud1()
        //{
        //    try
        //    {
        //        //Path to vtk data must be set as an environment variable
        //        // VTK_DATA_ROOT = "C:\VTK\vtkdata-5.8.0"
        //        //vtkTesting test = vtkTesting.New();
        //        //string root = test.GetDataRoot();
        //        //string filePath = System.IO.Path.Combine(root, "saved.txt");

        //        //vtkSimplePointsReader reader = vtkSimplePointsReader.New();
        //        //reader.SetFileName("./saved.txt");
        //        //reader.Update();

        //        for (double i = 0; i < 100; i++)
        //        {
        //            this.points.InsertNextPoint(i, i + 1, i - 1);
        //        }
        //        vtkPolyData polydata = vtkPolyData.New();
        //        polydata.SetPoints(this.points);

        //        vtkVertexGlyphFilter glyphFilter = vtkVertexGlyphFilter.New();
        //        glyphFilter.SetInputConnection(polydata.GetProducerPort());

        //        // Visualize,可视化部分的代码都是大同小异的
        //        vtkPolyDataMapper mapper1 = vtkPolyDataMapper.New();
        //        mapper1.SetInputConnection(glyphFilter.GetOutputPort());

        //        vtkActor actor = vtkActor.New();
        //        actor.SetMapper(mapper1);
        //        actor.GetProperty().SetPointSize(4);
        //        actor.GetProperty().SetColor(1, 0.5, 0);
        //        // get a reference to the renderwindow of our renderWindowControl1
        //        vtkRenderWindow renderWindow1 = this.renderWindowControl1.RenderWindow;

        //        // renderer
        //        vtkRenderer renderer = renderWindow1.GetRenderers().GetFirstRenderer();
        //        // set background color
        //        vtkRenderer renderer1 = vtkRenderer.New();

        //        renderWindow1.AddRenderer(renderer1);
        //        renderer1.AddActor(actor);
        //        // add our actor to the renderer
        //        renderer1.SetViewport(0.0, 0.0, 0.5,0.5);
        //        renderer1.SetBackground(0, 0, 0);
        //        //vtkCamera camera = renderer1.GetActiveCamera();
        //        //camera.Zoom(1.0);
        //        //renderer1.ResetCamera();
        //        //renderWindow1.Render();
        //    }
        //    catch (IOException ex)
        //    {
        //        MessageBox.Show(ex.Message, "IOException", MessageBoxButtons.OK);
        //    }

        //}
    }
}
