using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.MotionDomain;
namespace Double_Arm_Robot
{
    partial class ABBRobot
    {
        private NetworkScanner scanner = null;
        private Controller controller = null;
        private Controller controller1 = null;
        private ABB.Robotics.Controllers.RapidDomain.Task[] tasks = null;
        public void init()//ABB机器人的初始化函数，用于通过网线登陆到机器人上
        {
            try
            {
                if (scanner == null)
                {
                    scanner = new NetworkScanner();
                }
                scanner.Scan();
                ControllerInfoCollection controls = scanner.Controllers;
                //搜索可以使用的控制器存储到controls中去
                ControllerInfo info = controls[0];
                ControllerInfo info1 = controls[1];
                if (info.Availability == Availability.Available && info1.Availability == Availability.Available)
                {
                    if (controller != null)
                    {
                        controller.Logoff();
                        controller.Dispose();
                        controller = null;
                    }
                    if (controller1 != null)
                    {
                        controller1.Logoff();
                        controller1.Dispose();
                        controller1 = null;
                    }
                    controller = ControllerFactory.CreateFrom(info);
                    controller.Logon(UserInfo.DefaultUser);
                    controller1 = ControllerFactory.CreateFrom(info1);
                    controller1.Logon(UserInfo.DefaultUser);
                    //使用默认用户登录到机器人控制器上
                    tasks = controller.Rapid.GetTasks();
                    RobTarget aRobTarget = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(CoordinateSystemType.World);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("初始化ABB机器人时{0},请重新检查ABB机器人的接线和控制器状态",ex);
            }
        }
        public void abb_MotorOn()
        {
            try
            {
                if(controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    if (controller.State == ControllerState.MotorsOn&& controller1.State == ControllerState.MotorsOn)
                    {
                        return;
                    }
                    else
                    {
                        controller.State = ControllerState.MotorsOn;
                        controller1.State = ControllerState.MotorsOn;
                    }
                }
                else
                {
                    MessageBox.Show("请将将机器人切换到自动模式");
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void abb_MotorOff()
        {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto&&controller1.OperatingMode == ControllerOperatingMode.Auto)
                {
                    controller.State = ControllerState.MotorsOff ;
                    controller1.State = ControllerState.MotorsOff;
                }
                else
                {
                    MessageBox.Show("请将将机器人切换到自动模式");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void trans_PCModFile_ToController()
        {
            
            string localDir = controller.FileSystem.LocalDirectory;
            string strFileFullname = "";
            string strFilename = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RAPID文件(*.mod;*.sys)|*.mod;*sys|所有文件|*.*";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                strFileFullname = ofd.FileName;

                strFilename = ofd.SafeFileName;

            }
            try
            {
                string remoteDir = controller.FileSystem.RemoteDirectory + "/OPTION/" + strFilename;

                if(controller.FileSystem.FileExists("/OPTION/"+strFilename))
                 //判断控制器的文件夹下是否存在所上传的mod文件
                {
                    controller.FileSystem.PutFile(strFileFullname, "/OPTION/" + strFilename,true);
                }
                else
                {
                    controller.FileSystem.PutFile(strFileFullname, "/OPTION/" + strFilename);
                }
                //以上程序可以将PC上的Mod文件传输到机器人的控制器上
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    bool flag = tasks[0].LoadModuleFromFile(remoteDir, RapidLoadMode.Replace);
                    if(flag)
                    {
                        MessageBox.Show("加载模块成功");
                    }
                }

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void pp_ToMain()
        {
            try
            {
                if(controller.OperatingMode == ControllerOperatingMode.Auto&& controller1.OperatingMode == ControllerOperatingMode.Auto)
                {
                    
                    using (Mastership m = Mastership.Request(controller.Rapid))
                    {
                        tasks[0].ResetProgramPointer();
                        MessageBox.Show("机器人左程序指针已复位");
                    }
                    using (Mastership m = Mastership.Request(controller1.Rapid))
                    {
                        tasks[1].ResetProgramPointer();
                        MessageBox.Show("机器人右程序指针已复位");
                    }
                }
            }
            catch(System.InvalidOperationException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void startRun_RapidProgram()
        {
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    StartResult result = controller.Rapid.Start();
                }
                using (Mastership m = Mastership.Request(controller1.Rapid))
                {
                    StartResult result1 = controller1.Rapid.Start();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void stopRun_RapidProgram()
        {
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Stop(StopMode.Immediate);
                }
                using (Mastership m = Mastership.Request(controller1.Rapid))
                {
                    controller1.Rapid.Stop(StopMode.Immediate);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void return_StartPoint()
        {
            Module LeftRobot_module = tasks[0].GetModule("MAINLINER1");
            Module RightRobot_module = tasks[1].GetModule("MAINLINER2");
            string LeftRobot_startPointModule = "LeftRobot_Start_Polishing";
            string RightRobot_startPointModule = "RightRobot_Return_InitPoint";
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    tasks[0].SetProgramPointer("MAINLINER1", LeftRobot_startPointModule);
                } 
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    tasks[1].SetProgramPointer("MAINLINER2", RightRobot_startPointModule);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void RAX_6To180()
        {
            Module module = tasks[0].GetModule("module1");
            string startPointModule = "RAX_6To180";
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    tasks[0].SetProgramPointer("module1", startPointModule);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void RAX_6Add30Per()
        {
            Module module = tasks[0].GetModule("module1");
            string startPointModule = "RAX_6Add30Per";
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    tasks[0].SetProgramPointer("module1", startPointModule);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
