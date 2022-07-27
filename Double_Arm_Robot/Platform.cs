using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace Double_Arm_Robot
{
    partial class Platform
    {
        public Platform()
        {

        }
        ~Platform()
        {
            try
            {
                this._mySerialport.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private SerialPort _mySerialport;
        public void inital(string serial_Num)
        {
            try
            {
                _mySerialport = new SerialPort(serial_Num);
                //设置波特率、奇偶校验位、数据位、停止位
                _mySerialport.BaudRate = 9600;
                _mySerialport.Parity = 0;
                _mySerialport.DataBits = 8;
                _mySerialport.StopBits = StopBits.One;
                _mySerialport.DataReceived += new SerialDataReceivedEventHandler(SeriaPort_DataReceived);
                _mySerialport.Open();
                if (_mySerialport.IsOpen)
                {
                    //滑台没有在初始位置的话让滑台移到初始位置
                    back();
                    //byte[] hexStr_go = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x50, 0x00, 0xFF, 0xFF, 0xAF, 0x00 };
                    //_mySerialport.Write(hexStr_go, 0, 10);
                    //byte[] hexStr_go1 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x53, 0x00, 0x12, 0x00, 0x00, 0x00 };
                    //_mySerialport.Write(hexStr_go1, 0, 10);
                    //byte[] hexStr_go2 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x44, 0x00, 0x01, 0x00, 0x00, 0x00 };
                    //_mySerialport.Write(hexStr_go2, 0, 10);
                    //byte[] hexStr_go3 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    //_mySerialport.Write(hexStr_go3, 0, 10);
                }
                else
                {
                    MessageBox.Show("串口打开失败，请检查与滑台的连线以及输入正确的滑台串口号");
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

            
        }
        public void run()
        {
            try
            {
                byte[] hexStr_go = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x50, 0x00, 0xFF, 0xFF, 0x0A, 0x00 };
                _mySerialport.Write(hexStr_go, 0, 10);
                byte[] hexStr_go1 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x53, 0x00, 0x12, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go1, 0, 10);
                byte[] hexStr_go2 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x44, 0x00, 0x00, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go2, 0, 10);
                byte[] hexStr_go3 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go3, 0, 10);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        public void back()
        {
            try
            {
                byte[] hexStr_go = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x50, 0x00, 0xFF, 0xFF, 0xAF, 0x00 };
                _mySerialport.Write(hexStr_go, 0, 10);
                byte[] hexStr_go1 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x53, 0x00, 0xFF, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go1, 0, 10);
                byte[] hexStr_go2 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x44, 0x00, 0x01, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go2, 0, 10);
                byte[] hexStr_go3 = new byte[10] { 0x00, 0x00, 0x40, 0x01, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00 };
                _mySerialport.Write(hexStr_go3, 0, 10);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SeriaPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_mySerialport.IsOpen)
            {
                try
                {
                    int byteLength = _mySerialport.BytesToRead;
                    /*if (byteLength == 10) *///4)
                    //{
                        Byte[] receiveData = new Byte[byteLength];
                        _mySerialport.Read(receiveData, 0, receiveData.Length);  //读取数据
                        if (receiveData[0] == 164)
                        {
                                back();
                        }
                    //由于滑台结束每一条指令后都会返回一个字符串，但是返回来的字符串长度不是固定的有时候是3，有时候是4，5，9
                    //所以只判断前三位即可，但是由于每条指令返回的字符串前几位都是相同的，所以会多次调用back函数，这个问题暂时无法解决
                    //但是滑台运行过程中就算接收字符串也不会立刻执行命令，所以只会在最后的时候调用back函数
                    _mySerialport.DiscardInBuffer();                         //清空缓冲区

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                MessageBox.Show( "请先打开串口！\r\n");
            }
        }
    }
}