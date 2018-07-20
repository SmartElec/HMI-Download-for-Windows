using System;
//using System.Collections;//包含队列Queue
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using WINUI.ControlEx.InvokeEx;
using System.Drawing.Imaging;
using HMI_DownLoad.Properties;

//字符前面的@，用于不使用转义字符
namespace UART_Demo
{
    public partial class Form1 : Form
    {
        byte[] HEADER={0xaa,0xbb,0xcc,0xdd};
        Queue<byte[]> recQueue = new Queue<byte[]>();//接收数据过程中，接收数据线程与数据处理线程直接传递的队列，先进先出
        Object locker_receive = new Object();

        //串口关闭标志
        bool serialportIsClosing=false;
        bool IsFileReceive=false;
        bool IsFileSend = false;
        
        SerialPort mySerialPort = new SerialPort();
        string[] portsName;
        int[] BaudRateArr = new int[] { 115200, 57600, 38400, 9600 };
        int[] PacketSizeArr = new int[] { 256, 512, 1024, 4096 };
        UInt16 PacketSize =0;

        byte[] RestartOK = new byte[] { 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x88, 0xFF, 0xFF, 0xFF };
        bool WaitRestart = false;
        string Device_Type = null;
        public Form1()
        {
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
            //允许拖拽
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            
            InitializeComponent();
            baudRate_combox.DataSource = BaudRateArr;
            packet_combox.DataSource = PacketSizeArr;
            err_label.Text = "";
            //this.Width = 400;
            RecFilebtn.Enabled = false;
            Ex_btn.Enabled = false;
            groupBox1.Enabled = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            open_btn.Text = "打开设备";
            open_btn.Tag = "open";
            //Ex_btn.Text = ">";//触发内容改变事件
            //this.AllowDrop = false;//禁止拖拽
            
            aboutToolStripMenuItem.Enabled = false;

            baudRate_combox.SelectedIndex = 3;
            packet_combox.SelectedIndex = 3;
            baudRate_combox.Enabled = false;
            packet_combox.Enabled = false;
            PacketSize = UInt16.Parse(packet_combox.Text);
            Search_Port();
        }
        private void Search_Port()
        {
            comport.Items.Clear();
            portsName = SerialPort.GetPortNames();
            if (portsName.Length > 0)
            {
                Array.Sort(portsName, new CustomComparer());
                //comport.SelectedIndex = 0;
                for (int i = 0; i < portsName.Length; i++)
                {
                    comport.Items.Add(portsName[i]);//下拉控件里添加可用串口
                    if (Settings.Default._COMPORT == portsName[i])
                    {
                        comport.SelectedIndex = i;
                    }
                }
                if (comport.SelectedIndex == -1)
                {
                    comport.SelectedIndex = 0;
                }

            }
            else
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }
        }
        private void open_btn_Click(object sender, EventArgs e)
        {
            OpenOrClose();
        }
        private void OpenOrClose()
        {
            open_btn.Enabled = false;
            if (open_btn.Tag.ToString() == "close")
            {
                if (mySerialPort.IsOpen)
                {
                    Thread thClose = new Thread(CloseSerialPort);
                    thClose.Start();
                    //open_btn.Text = "打开设备";
                    //open_btn.Tag = "open";
                }
                else
                {
                    MessageBox.Show("串口已关闭");
                    open_btn.Text = "打开设备";
                    open_btn.Tag = "open";
                    open_btn.Enabled = true;
                }
                ParameterGroup.Enabled = true;
            }
            else if (open_btn.Tag.ToString() == "open")
            {
                if (!mySerialPort.IsOpen)
                {
                    OpenSerialPort();
                }
                else
                {
                    MessageBox.Show("串口已打开");
                    open_btn.Text = "关闭设备";
                    open_btn.Tag = "colse";
                }
                open_btn.Enabled = true;
            }
            else
            {
                MessageBox.Show("错误");
                open_btn.Enabled = true;
            }
        }
        private void OpenSerialPort()
        {
            if (comport.SelectedItem == null)
            {
                open_btn.Text = "打开设备";
                open_btn.Tag = "open";
                MessageBox.Show("没有找到串口");
                return;
            }
            mySerialPort.PortName = comport.SelectedItem.ToString();
            mySerialPort.BaudRate = int.Parse(baudRate_combox.Text);
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            
            try
            {
                mySerialPort.Open();
                open_btn.Text = "关闭设备";
                open_btn.Tag = "close";
                byte[] NoACK=new byte[]{0xff,0xff,0xff};
                mySerialPort.Write(NoACK,0,3);
                serialportIsClosing = false;
                ParameterGroup.Enabled = false;
               
                /*保存新的设置信息*/
                bool update = false;
                if (Settings.Default._COMPORT != comport.SelectedItem.ToString())
                {
                    update = true;
                    Settings.Default._COMPORT = comport.SelectedItem.ToString();
                }
                if (Settings.Default._BAUDINDEX != baudRate_combox.SelectedIndex)
                {
                    update = true;
                    Settings.Default._BAUDINDEX = baudRate_combox.SelectedIndex;
                }
                if (Settings.Default._PACKETSIZEINDEX != packet_combox.SelectedIndex)
                {
                    update = true;
                    Settings.Default._PACKETSIZEINDEX = packet_combox.SelectedIndex;
                }
                if(update==true)
                {
                    Settings.Default.Save(); 
                }               
            }
            catch
            {
                open_btn.Text = "打开设备";
                open_btn.Tag = "open";
                MessageBox.Show("串口被占用！");
                return;
            }
        }
        private void CloseSerialPort()
        {
            serialportIsClosing = true;
            //serialportIsClosing为true后，mySerialPort_DataReceived就不会在接收数据
            //等待个200毫秒，以确保不再接收，在关闭串口
            //否则，如果频繁点击打开/关闭 串口还在接收数据就关闭串口会出现界面卡死
            Thread.Sleep(200);
            if (mySerialPort.IsOpen)
            {
                mySerialPort.Close();
            }
            else
            {
                MessageBox.Show("串口已关闭");
            }
            this.Invoke(new EventHandler(delegate
            {
                open_btn.Text = "打开设备";
                open_btn.Tag = "open";
                open_btn.Enabled = true;
            }));

        }

        private void mySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //如果串口正在关闭，返回
            if (serialportIsClosing == true)
            {
                return;
            }
            lock (locker_receive)
            {
                int data_num=0;
                byte[] recBuffer = new byte[2048];
                do
                {
                    int count = mySerialPort.BytesToRead;
                
                    if(count<=0)
                    {
                        break;
                    }
                    //Application.DoEvents();
                    Thread.Sleep(100);
                    try
                    {
                        mySerialPort.Read(recBuffer, data_num, count);
                        data_num += count;
                        if(data_num>2048)
                        {
                            data_num = 2048;
                            break;
                        }
                    }
                    catch(Exception er)
                    {
                        MessageBox.Show(er.Message);
                    }
                    
                }while (mySerialPort.BytesToRead>0);

                if (data_num > 0)
                {
                    byte[] buffer = new byte[data_num];
                    Array.Copy(recBuffer, buffer, data_num);
                    if(IsFileReceive==true)
                    {
                        recQueue.Enqueue(buffer);//数据入列Enqueue（全局）
                        return;
                    }
                        this.Invoke(new EventHandler(delegate
                       {
                           //dataprocess(buffer);
                           if (WaitRestart == true)
                           {
                               for (int i = 0; i < RestartOK.Length; i++)
                               {
                                   if (buffer[i] != RestartOK[i])
                                   {
                                       WaitRestart = false;
                                       return;
                                   }
                               }
                               Debug.WriteLine("设备重启成功");
                               rec_box.Text += "设备重启成功[" + DateTime.Now.ToLocalTime().ToString()  + "]\r\n";
                           }
                           else
                           {
                               dataprocess(buffer);
                           }
                           WaitRestart = false;
                       })); 
                }
               // string abc = ByteArrayToHexString(buf);
                //  Debug.WriteLine(abc);
            }

        }
        private void FileOpen()
        {
            OpenFileDialog open_fd = new OpenFileDialog();
            open_fd.Multiselect = false;
            open_fd.Title = "请选择文件";
            open_fd.Filter = "所有文件(*.*)|*.*|HMI编译文件(*.tft)|*.tft";
            open_fd.FilterIndex = 2;
            if (open_fd.ShowDialog() != DialogResult.OK )
            {
                return;
            }
         
            FILEPATH.Text = open_fd.FileName.ToString();
            FILEPATH.Focus();
            FILEPATH.Select(FILEPATH.Text.Length, 0);
            FileStream file = new FileStream(FILEPATH.Text.ToString(), FileMode.Open, FileAccess.Read);
            int FILE_SIZE = Convert.ToInt32(file.Length);
            FileSize_lab.Text = "size:"+Convert.ToString(FILE_SIZE)+" Byte";
            if (FILE_SIZE > 16 * 1024 * 1024)               //the maximum size of file is 64M
            {
                MessageBox.Show("FLASH空间不足！！", "错误");
                FILE_SIZE = 0;
                FILEPATH.Text = ""; 
            }
            file.Close();
        }

        #region 字符串转换函数
        //翻转byte数组
        public static void ReverseBytes(byte[] bytes)
        {
            byte tmp;
            int len = bytes.Length;

            for (int i = 0; i < len / 2; i++)
            {
                tmp = bytes[len - 1 - i];
                bytes[len - 1 - i] = bytes[i];
                bytes[i] = tmp;
            }
        }
        //规定转换起始位置和长度
        public static void ReverseBytes(byte[] bytes, int start, int len)
        {
            int end = start + len - 1;
            byte tmp;
            int i = 0;
            for (int index = start; index < start + len / 2; index++, i++)
            {
                tmp = bytes[end - i];
                bytes[end - i] = bytes[index];
                bytes[index] = tmp;
            }
        }

        // 翻转字节顺序 (16-bit)
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }


        // 翻转字节顺序 (32-bit)
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        // 翻转字节顺序 (64-bit)
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }

        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = s.Substring(0, s.Length - 1) + "0" + s.Substring(s.Length - 1);
            }
            byte[] buffer = new byte[s.Length / 2];

            try
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch
            {
                string errorString = "E4";
                byte[] errorData = new byte[errorString.Length / 2];
                errorData[0] = (byte)Convert.ToByte(errorString, 16);
                return errorData;
            }
        }

        public string StringToHexString(string s)
        {
            s = s.Replace(" ", "");
            string buffer = "";
            char[] myChar;
            myChar = s.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                buffer = buffer + Convert.ToString(myChar[i], 16);
                buffer = buffer.ToUpper();
            }
            return buffer;
        }
        #endregion
        private void btSend_Event(string strSend)
        {
            if (mySerialPort.IsOpen)
            {
                byte[] sendHexData = HexStringToByteArray(strSend);
                mySerialPort.Write(sendHexData, 0, sendHexData.Length);
            }
            else
            {
               // MessageBox.Show("串口没有打开，请检查！");
            }
        }
        private bool Wait_Ack(byte ACK)
        {
            byte firstByte = 0;
            mySerialPort.ReadTimeout = 1000;
            try
            {
                firstByte = Convert.ToByte(mySerialPort.ReadByte());//超时时间为2s
                //mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据
                if (firstByte == ACK)//ACK=0x06
                {
                    return true;
                }
                int bytesLen = mySerialPort.BytesToRead;
                byte[] bytesData = new byte[bytesLen];
                mySerialPort.Read(bytesData, 0, bytesLen);
                return false;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);//"操作已超时"
                Debug.Print(e.Message);
            }
            return false;
        }
        private bool Wait_Ack(string ACK)
        {
            byte firstByte = 0;
            byte[] ByteACK = System.Text.Encoding.Default.GetBytes(ACK);
            mySerialPort.ReadTimeout = 1000;
            try
            {
                firstByte = Convert.ToByte(mySerialPort.ReadByte());//超时时间为2s
                //mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据
                Delay(100);
                int bytesLen = mySerialPort.BytesToRead;
                byte[] bytesData = new byte[bytesLen + 1];
                bytesData[0] = firstByte;
                mySerialPort.Read(bytesData, 1, bytesLen);

                if (System.Text.Encoding.Default.GetString(bytesData).Contains(ACK))
                {
                    string[] sArray = System.Text.Encoding.Default.GetString(bytesData).Split(',');
                    this.Invoke(new EventHandler(delegate
                    {
                        if (sArray.Length>2)
                        {
                            Device_Type = "设备型号:" + sArray[2];                    
                            this.Text = Device_Type;
                        }
                            
                    }));
                    return true;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);//"操作已超时"
                Debug.Print(e.Message);
            }
            return false;
        }
        private bool Wait_Ack(byte[] ACK)
        {
            byte firstByte = 0;
            mySerialPort.ReadTimeout = 1000;
            try
            {
                firstByte = Convert.ToByte(mySerialPort.ReadByte());//超时时间为2s
                //mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据
                Delay(100);
                int bytesLen = mySerialPort.BytesToRead;
                byte[] bytesData = new byte[bytesLen+1];
                bytesData[0] = firstByte;
                mySerialPort.Read(bytesData, 1, bytesLen);
                if (ACK.Length <= (bytesLen+1))
                {
                    for (int i = 0; i < (ACK.Length); i++)
                    {
                       if (ACK[i] != bytesData[i])
                            return false;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);//"操作已超时"
                Debug.Print(e.Message);
            }
            return false;
        }
        private void send_file()
        {
            mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
            FileStream filefd = null;
            UInt32 FILE_SIZE = 0;
            UInt32 packets = 0;
            string packet1 = null;
            try
            {
                mySerialPort.BaudRate = 9600;
                Delay(200);
                mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据
                mySerialPort.DiscardOutBuffer();
                Delay(200);
                WaitRestart = false;
                string filepath = TextBoxInvoke.GetTextBoxText(FILEPATH);
                filefd = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                FILE_SIZE = Convert.ToUInt32(filefd.Length);
                packets = FILE_SIZE / PacketSize + (UInt32)((FILE_SIZE % PacketSize)>0?1:0);

                LabelInvoke.SetLabelText(err_label, "");
                LabelInvoke.SetLabelText(FileSize_lab, "size:" + Convert.ToString(FILE_SIZE) + " Byte" + " packets:" + packets);
                ProgressBarInvoke.SetProgressBarValue(progressBar1, 0);
                ProgressBarInvoke.SetProgressBarMaxValue(progressBar1, (int)packets);
                LabelInvoke.SetLabelText(label_packet, 0 + "/" + packets);

                if (FILE_SIZE > 16 * 1000 * 1000)               //the maximum size of file is 16M
                {
                    MessageBox.Show("FLASH空间不足！！", "错误");
                    FILE_SIZE = 0;
                    return;
                }
                Device_Type = null;
                LabelInvoke.SetLabelText(err_label, "正在建立连接..." + mySerialPort.BaudRate);
                //byte[] Connect = new byte[10] {0x63,0x6f,0x6e,0x6e,0x65,0x63,0x74,0xff,0xff,0xff};
                byte[] Connect = new byte[10] { (byte)'c', (byte)'o', (byte)'n', (byte)'n', (byte)'e', (byte)'c', (byte)'t', 0xff, 0xff, 0xff };              
                byte[] ExitSleep = new byte[10] { (byte)'s', (byte)'l', (byte)'e', (byte)'e', (byte)'p', (byte)'=', (byte)'0', 0xff, 0xff, 0xff };

                mySerialPort.Write(Connect, 7, 3);
                Wait_Ack(0);
                mySerialPort.Write(Connect, 0, Connect.Length);
                //packet1 = "正在建立连接..." + mySerialPort.BaudRate+"\r\n";
                //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "正在建立连接..." + mySerialPort.BaudRate + "\r\n");
                if(Wait_Ack("comok") == false )
                {
                    mySerialPort.BaudRate = 115200;
                    LabelInvoke.SetLabelText(err_label, "正在建立连接..." + mySerialPort.BaudRate);
                    //packet1 = packet1 + "正在建立连接..." + mySerialPort.BaudRate + "\r\n";
                    //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                    TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "正在建立连接..." + mySerialPort.BaudRate + "\r\n");
                    Delay(200);
                    mySerialPort.Write(Connect, 7, 3);
                    Wait_Ack(0);
                    mySerialPort.Write(Connect, 0, Connect.Length);
                    if (Wait_Ack("comok") == false)
                    {
                        MessageBox.Show("请确认设备正在运行并检查端口号", "联机失败");
                        LabelInvoke.SetLabelText(err_label, "联机失败,请确认设备正在运行");
                        //packet1 = packet1 + "联机失败,请确认设备正在运行\r\n";
                        //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                        TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "联机失败,请确认设备正在运行并检查端口号\r\n");
                        return;
                    }
                }
                LabelInvoke.SetLabelText(err_label, "联机成功 " + Device_Type);
                //packet1 = TextBoxInvoke.GetRichTextBoxText(rec_box);
                //packet1 = packet1 + "联机成功 " + mySerialPort.BaudRate + "\r\n";
                //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) +"联机成功 " + Device_Type + "\r\n");

                byte[] sendData = new byte[4096];
                bool response = false;
                int read_len = 0;
                UInt32 TimeOfSecond = 0;
                packet1 = "whmi-wri " + FILE_SIZE + "," + 115200 +",0";
                byte[] header = System.Text.Encoding.Default.GetBytes(packet1);
                header.CopyTo(sendData, 0);

                Debug.WriteLine(packet1);

                read_len = System.Text.Encoding.Default.GetBytes(packet1).Length;
                sendData[read_len] = 0xff;
                sendData[read_len+1] = 0xff;
                sendData[read_len+2] = 0xff;

                /*退出休眠模式*/
                mySerialPort.Write(ExitSleep, 0, ExitSleep.Length);
                Delay(100);
                mySerialPort.Write(ExitSleep, 0, ExitSleep.Length);
                Delay(100);
                /*---------切换到下载模式 强制下载波特率115200----------*/
                mySerialPort.Write(sendData, 0, read_len + 3);
                Delay(100);
                mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据

                mySerialPort.BaudRate = 115200;
                response = Wait_Ack(0x05);
                if (response != true)
                {
                    MessageBox.Show("请断电再上电重试", "响应失败");
                    LabelInvoke.SetLabelText(err_label, "响应失败,请断电再上电重试");
                    //packet1 = packet1 + "响应失败,请断电再上电重试\r\n";
                    //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                    TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "响应失败,请断电再上电重试\r\n") ;
                    return;   
                }
                TimeOfSecond = FILE_SIZE / 12288 + packets * 70 / 1000;
                LabelInvoke.SetLabelText(err_label, "正在升级...预计耗时:" + TimeOfSecond + "s");
                //packet1 = packet1 + "正在升级...预计耗时:" + TimeOfSecond + "s\r\n";
                //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "正在升级...预计耗时:" + TimeOfSecond + "s\r\n");
                DateTime startTime = DateTime.Now;
                DateTime stopTime = DateTime.Now;

                for (int i = 0; i < packets; i++)
                {
                    //ProgressBarInvoke.SetProgressBarValue(progressBar1, i);
                    //LabelInvoke.SetLabelText(label_packet,i+"/"+packets);
                    read_len = filefd.Read(sendData, 0, PacketSize);

                     mySerialPort.Write(sendData, 0, read_len);//
                    // Debug.WriteLine("packet:"+ i);
                    response = Wait_Ack(0x05);
                    if (response != true)
                    {
                        Delay(100);
                        mySerialPort.DiscardInBuffer();//丢弃来自串行驱动程序的接受缓冲区的数据
                        mySerialPort.DiscardOutBuffer();
                        Delay(100);
                        LabelInvoke.SetLabelText(err_label, "升级出错");
                        return;
                    }
                    LabelInvoke.SetLabelText(label_packet, (i + 1) + "/" + packets);
                    ProgressBarInvoke.SetProgressBarValue(progressBar1, i + 1);
                }
                //发送完成
                LabelInvoke.SetLabelText(err_label, "升级完成");

                WaitRestart = true;
                stopTime = DateTime.Now;
                TimeSpan elapsedTime = stopTime - startTime;
                string strTime = string.Format("实际耗时:{0}s", Convert.ToInt32(elapsedTime.TotalSeconds));
                LabelInvoke.SetLabelText(err_label, "升级完成 " + strTime);
                //packet1 = packet1 + "升级完成 " + strTime + "\r\n";
                //TextBoxInvoke.SetRichTextBoxText(rec_box, packet1);
                TextBoxInvoke.SetRichTextBoxText(rec_box, TextBoxInvoke.GetRichTextBoxText(rec_box) + "升级完成 " + strTime + "\r\n");
                Console.WriteLine(strTime);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                LabelInvoke.SetLabelText(err_label, "端口异常,终止发送");
            }
            finally
            {
                IsFileSend = false;
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
                filefd.Close();
                this.Invoke(new EventHandler(delegate
                {
                    sendfile_btn.Enabled = true;
                }));
            }

        }
        private void sendfile_btn_Click(object sender, EventArgs e)
        {
            if (FILEPATH.Text == "请选择文件")
            {
                MessageBox.Show("请选择文件", "提示");
                return;
            }
            if (!File.Exists(FILEPATH.Text))
            {
                MessageBox.Show("文件不存在");
                return;
            }
            if (mySerialPort.IsOpen == false)
            {
                MessageBox.Show("请打开设备后重试", "提示");
                return;
            }

            if (IsFileReceive == true)//确认退出接收状态
            {
                DialogResult dr = MessageBox.Show("确认中断吗？", "正在接收文件", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    //用户选择确认的操作
                    IsFileReceive = false;
                }
                else if (dr == DialogResult.Cancel)
                {
                    //用户选择取消的操作
                    MessageBox.Show("已取消本操作");
                    return;
                }
            }
            mySerialPort.BaudRate = int.Parse(baudRate_combox.Text);
            IsFileSend = true;
            sendfile_btn.Enabled = false;
            IsFileReceive = false;
            Thread thread = new Thread(send_file);
            thread.IsBackground = true;
            thread.Start();
            
        }
        public void dataprocess(byte[] InputBuf)
        {
             string abc = ByteArrayToHexString(InputBuf);
             string bbc = "[" + BitConverter.ToString(InputBuf) + "]" + abc + "\r\n";
             //rec_box.Text += ("[" + BitConverter.ToString(InputBuf) + "]" + abc + "\r\n");
             if (rec_box.Text.Length > 1000)
                 rec_box.Text = "";
             rec_box.AppendText(bbc);
             rec_box.ScrollToCaret();
        }
        private void openfile_btn_Click(object sender, EventArgs e)
        {
            FileOpen();
        }
        private void send_box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btSend_Event(send_box.Text);
                //send_box.Text = "";
            }
        }
        private void Delay(int Millisecond) //延迟系统时间，但系统又能同时能执行其它任务；
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(Millisecond) > DateTime.Now)
            {
                Application.DoEvents();//转让控制权            
            }
            return;
        }
        private void FILEPATH__Enter(object sender, EventArgs e)
        {
            FILEPATH.ForeColor = SystemColors.WindowText;
            if (FILEPATH.Text == "请选择文件")
            {
                FILEPATH.Text = "";
                FileSize_lab.Text = "";
                return;
            }
            
        }
        private void FILEPATH__Leave(object sender, EventArgs e)
        {
            FILEPATH.Text = FILEPATH.Text.Trim();
            if (FILEPATH.Text == "")
            {
                FILEPATH.ForeColor = SystemColors.InactiveCaption;
                FILEPATH.Text = "请选择文件";
                FileSize_lab.Text = "";
                return;
            }
            
        }
        #region CRC16校验
        private static readonly byte[] aucCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };
        private static readonly byte[] aucCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
            0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
            0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
            0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
            0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
            0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
            0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
            0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
            0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
            0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
            0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
            0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
            0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
            0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
            0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
            0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
            0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
            0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };
        /// <summary>
        /// CRC效验
        /// </summary>
        /// <param name="pucFrame">效验数据</param>
        /// <param name="usLen">数据长度</param>
        /// <returns>效验结果</returns>
        public static int Crc16(byte[] pucFrame,int offset, int usLen)
        {
            int i = offset;
            byte ucCRCHi = 0xFF;
            byte ucCRCLo = 0xFF;
            UInt16 iIndex = 0x0000;

            while (usLen-- > 0)
            {
                iIndex = (UInt16)(ucCRCLo ^ pucFrame[i++]);
                ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                ucCRCHi = (byte)aucCRCLo[iIndex];
            }
            return (ucCRCHi << 8 | ucCRCLo);
        }

        #endregion

        private void RecFilebtn_Click(object sender, EventArgs e)
        {

            if (IsFileSend==true)
            {
                MessageBox.Show("正在传输文件，请稍后");
                return;
            }
            if (IsFileReceive)
            {
                RecFilebtn.Text = "接收文件";
                IsFileReceive = false;
                return;
            }
            else
            {
                if (mySerialPort.IsOpen == false)
                {
                    MessageBox.Show("请打开设备后重试", "提示");
                    return;
                }
                RecFilebtn.Text = "终止接收";
                IsFileReceive = true;
            }

            Thread _FileRec = new Thread(new ThreadStart(FileRec)); //查询串口接收数据线程声明
            _FileRec.IsBackground = true;
            _FileRec.SetApartmentState(ApartmentState.STA); // 设置为单线程单元(STA)状态 
            _FileRec.Start();//启动线程

        }
        void FileRec()
        {
            FileStream fs = null;
            bool successful_flag = false;
            int receive_state = 0;
            int timeout_count = 0;
            int buf_length = 0;
            UInt32 file_size = 0;
            string filename=null;
            UInt32 packets_now = 0, packets_all = 0;
            UInt16 packet_size = 0;//必须为2字节
            UInt16 temp_crc, calc_crc;
            Int16 PacketSizeIndex=-1;
            byte[] ACK=new byte[3]{0xaa,0xbb,0xdd};
            
            try// lock (locker_receive)
            {
                mySerialPort.Write(ACK, 0, 3);
                LabelInvoke.SetLabelText(err_label, "正在接收文件");
                while ((IsFileReceive)&&(!successful_flag))
                {
                    if (recQueue.Count > 0)
                    {
                        timeout_count = 0;
                        byte[] recBuffer = (byte[])recQueue.Dequeue();//出列Dequeue（全局）
                        buf_length = recBuffer.Length;
                        //\xaa\xbb\xcc\xdd\x00\x00\x00\xff\x11\x22dell.jpg
                        //Debug.WriteLine("buf_length:" + buf_length);
                        switch(receive_state)
                        {
                            case 0:
                                if (buf_length > 10)
                                {
                                    UInt32 HEADER = BitConverter.ToUInt32(recBuffer, 0);
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        HEADER = ReverseBytes(HEADER);
                                    }
                                    if (HEADER == 0xaabbccdd)
                                    {

                                        temp_crc = BitConverter.ToUInt16(recBuffer, 10);
                                        if (BitConverter.IsLittleEndian)
                                        {
                                            temp_crc = ReverseBytes(temp_crc);
                                        }
                                        calc_crc = (UInt16)Crc16(recBuffer, 4, 6);
                                        //Debug.WriteLine("crc="+calc_crc);
                                        if (temp_crc == calc_crc) 
                                        {
                                            //file_size = (recBuffer[4] << 24) | (recBuffer[5]<<16) |( recBuffer[6] << 8) | recBuffer[7];
                                            file_size = BitConverter.ToUInt32(recBuffer, 4);
                                            packet_size = BitConverter.ToUInt16(recBuffer, 8);
                                            if (BitConverter.IsLittleEndian) // 若为 小端模式
                                            {
                                                file_size = ReverseBytes(file_size);
                                                packet_size = ReverseBytes(packet_size);
                                            }
                                            packets_all = (UInt32)(file_size / packet_size) + (UInt32)((file_size % packet_size) > 0 ? 1 : 0);
                                            if (packet_size == 128)
                                            { 
                                                PacketSizeIndex=0;
                                            }
                                            else if (packet_size == 256)
                                            {
                                                PacketSizeIndex=1;
                                            }
                                            else if (packet_size == 512)
                                            {
                                                PacketSizeIndex=2;
                                            }
                                            else if (packet_size == 1024)
                                            {
                                                PacketSizeIndex=3;
                                            }
                                            else
                                            {
                                                PacketSizeIndex = -1;
                                            }
                                            if ((PacketSizeIndex > -1) && (PacketSizeIndex <4))
                                            {
                                                this.Invoke(new EventHandler(delegate
                                                {
                                                    packet_combox.SelectedIndex = PacketSizeIndex;
                                                }));
                                            }

                                            filename = System.Text.Encoding.Default.GetString(recBuffer, 12, buf_length-12);
                                            filename = filename.Trim();                                            
                                            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                                            {
                                                filename = "unknown";
                                            }
                                            ProgressBarInvoke.SetProgressBarValue(progressBar1, 0);
                                            ProgressBarInvoke.SetProgressBarMaxValue(progressBar1, (int)packets_all);
                                            LabelInvoke.SetLabelText(label_packet, 0 + "/" + packets_all);
                                            LabelInvoke.SetLabelText(err_label, "文件接收中");
                                            LabelInvoke.SetLabelText(FileSize_lab, "size:" + Convert.ToString(file_size) + " Byte" + " ["+ filename+ "]");


                                            receive_state = 1;
                                            Debug.WriteLine(filename + ":" + filename.Length + ":" + file_size);
                                            ACK[0] = 0x06;
                                            if (mySerialPort.IsOpen == true)
                                                mySerialPort.Write(ACK, 0, 1);
                                            fs = new FileStream(GetNewPathForDupes(filename), FileMode.Create);//初始化文件流
                                            
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Error Header" + recBuffer[0] + " " + recBuffer[1]);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine(ByteArrayToHexString(recBuffer));
                                }

                                break;

                            case 1:
                                if (buf_length > 4)
                                {
                                    temp_crc = (UInt16)(recBuffer[buf_length - 2] << 8 | recBuffer[buf_length - 1]);
                                    calc_crc = (UInt16)Crc16(recBuffer, 2, buf_length-4);
                                    //  Debug.WriteLine("crc1=" + calc_crc);
                                    if (temp_crc == calc_crc) 
                                    {
                                         Debug.WriteLine("packets=" + packets_now);
                                         UInt16 num = BitConverter.ToUInt16(recBuffer, 0);
                                         if (BitConverter.IsLittleEndian) // 若为 小端模式
                                         {
                                             num = ReverseBytes(num);
                                         }
                                        if (packets_now == num)
                                        {
                                            packets_now++;
                                            ACK[0] = 0x06;
                                            LabelInvoke.SetLabelText(label_packet, packets_now + "/" + packets_all);
                                            ProgressBarInvoke.SetProgressBarValue(progressBar1, (UInt16)packets_now);
                                            if (mySerialPort.IsOpen == true)
                                                mySerialPort.Write(ACK,0,1);
                                            fs.Write(recBuffer, 2, buf_length-4);//将字节数组写入文件流
                                            
                                            if(packets_now==packets_all)
                                            {
                                                successful_flag = true;
                                                fs.Close();//关闭流
                                                MessageBox.Show(filename,"保存成功！");
                                            }
                                        }
                                        else
                                        {
                                            ACK[0] = 0x08;
                                            ACK[1] = (byte)(packets_now>>8);
                                            ACK[2] = (byte)packets_now;
                                            if (mySerialPort.IsOpen == true)
                                                mySerialPort.Write(ACK, 0, 3);
                                        }
                                        
                                    }
                                    else
                                    {
                                        Debug.WriteLine("crc error:" + temp_crc + "  p:" + packets_now);
                                        ACK[0] = 0x07;
                                        if (mySerialPort.IsOpen == true)
                                            mySerialPort.Write(ACK, 0, 1);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine(ByteArrayToHexString(recBuffer));
                                    //IsFileReceive = false;
                                }

                                break;

                            default:
                                break;
                        }



                    }
                    else
                    {
                        Thread.Sleep(100);//如果不延时，一直查询，将占用CPU过高
                        timeout_count++;
                        if(timeout_count>100)//10s
                        {
                            IsFileReceive = false;
                        }
                    } 
                }
                if (successful_flag == false)
                {
                    if (receive_state == 1)
                    {
                        fs.Close();//关闭流
                        if (filename != null)
                        {
                            File.Delete(filename);
                        }
                    }
                    ACK[0] = 0xff;
                    ACK[1] = 0xff;
                    if (mySerialPort.IsOpen == true)
                        mySerialPort.Write(ACK, 0, 2);
                    LabelInvoke.SetLabelText(err_label, "文件接收失败");
                }
                else
                {
                    LabelInvoke.SetLabelText(err_label, "文件接收成功");
                }
                       
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                LabelInvoke.SetLabelText(err_label, "文件接收失败");
            }
            finally
            {
                IsFileReceive = false;
                this.Invoke(new EventHandler(delegate
                {
                    RecFilebtn.Enabled = true;
                    RecFilebtn.Text = "接收文件";
                }));
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsFileReceive = false;
            //Environment.Exit(0);
        }

        private void packet_combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PacketSize = UInt16.Parse(packet_combox.Text);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            send_box.Focus();
        }
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;
        }
        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //获取第一个文件名
            string fileName = (e.Data.GetData(DataFormats.FileDrop, false) as String[])[0];
           
            FILEPATH.Text = fileName;
            //if(true==Load_Picture(fileName))
            //{
            //    Ex_btn.Text = "<";
            //}
        }
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = @"图像|*.bmp;;*.jpg;*.gif;*.png|图标文件|*.ico|所有|*.*";
            //openFileDialog.FilterIndex = 4;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Load_Picture(openFileDialog.FileName);
            }
        }
        void Write_ArrayToFile(byte[] bdata,string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);//初始化文件流
            fs.Write(bdata, 0, bdata.Length);
            fs.Close();
            MessageBox.Show("文件保存成功" + bdata.Length, "pic.bin");
        }
        //保存文件如有重名加（） **(1)
        private string GetNewPathForDupes(string path)
        {
            string newFullPath = path.Trim();
            if (System.IO.File.Exists(path))
            {
                string directory = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);
                int counter = 1;
                do
                {
                    string newFilename = string.Format("{0}({1}){2}", filename, counter, extension);
                    newFullPath = Path.Combine(directory, newFilename);
                    counter++;
                } while (System.IO.File.Exists(newFullPath));
            }
            return newFullPath;
        }
        bool Load_Picture(string filepath)
        {

            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            try
            {
                Image img = Image.FromFile(filepath);
                Image bmp = new Bitmap(img);
                labelTips.Text = "双击或拖拽加载图片 " + bmp.Width + "x" + bmp.Height;
                pictureBox1.Image = bmp;
                img.Dispose();
               // bmp.Dispose();//image占用的不能释放
            }
            catch (Exception)
            {
                if(Ex_btn.Text=="<")
                    MessageBox.Show("文件格式不正确");
                return false;
            }
            return true;
        }
        private void Ex_btn_Click(object sender, EventArgs e)
        {
            if (Ex_btn.Text == ">")
            {
                Ex_btn.Text = "<";
            }
            else
            {
                Ex_btn.Text = ">";
            }
        }

        private void Ex_btn_TextChanged(object sender, EventArgs e)
        {
            if (Ex_btn.Text == ">")
            {
                this.Width = this.Width / 2;
                groupBox1.Visible = false;
            }
            else
            {
                this.Width = this.Width * 2;
                groupBox1.Visible = true;
            }
        }
        private void savepic_btn_Click(object sender, EventArgs e)
        {
            bool isSave = true;
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Title = "图片保存";
            saveImageDialog.Filter = @"bmp图像|*.bmp|jpeg图像|*.jpg|gif图像|*.gif|png图像|*.png";
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveImageDialog.FileName.ToString();
                fileName = GetNewPathForDupes(fileName);
                if (fileName != "" && fileName != null)
                {
                    string fileExtName = fileName.Substring(fileName.LastIndexOf(".") + 1).ToString();
                    //string fileExtName = System.IO.Path.GetExtension(fileName).ToLower();
                    ImageFormat imgformat = null;

                    if (fileExtName != "")
                    {
                        switch (fileExtName)
                        {
                            case "jpg":
                                imgformat = ImageFormat.Jpeg;
                                break;
                            case "bmp":
                                imgformat = ImageFormat.Bmp;
                                break;
                            case "gif":
                                imgformat = ImageFormat.Gif;
                                break;
                            case "png":
                                imgformat = ImageFormat.Png;
                                break;
                            default:
                                MessageBox.Show("只能存取为: jpg,bmp,gif,png 格式");
                                isSave = false;
                                break;
                        }
                        //默认保存为BMP格式   
                        if (imgformat == null)
                        {
                            imgformat = ImageFormat.Bmp;
                        }

                        if (isSave)
                        {
                            try
                            {    
                                Bitmap bit = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                                Graphics g = Graphics.FromImage(bit);//从指定的 Image 创建新的 Graphics(绘图)。
                                g.DrawImage(pictureBox1.Image, new Rectangle(0, 0, bit.Width, bit.Height), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height), GraphicsUnit.Pixel);
 
                                pictureBox1.Image.Save(fileName, imgformat);
                                g.Dispose();
                                bit.Dispose();
                                MessageBox.Show("图片已经成功保存!"); 
                            }
                            catch
                            {
                                MessageBox.Show("保存失败,你还没有截取过图片或已经清空图片!");
                            }
                        }   
                    
                    }
                }
            }
        
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image!=null)
            {
                Graphics img = Graphics.FromImage(pictureBox1.Image);
                Pen pen = new Pen(Color.Crimson);
                Brush brush = new SolidBrush(Color.Cyan);
                Font drawFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Millimeter);
                img.DrawString("Hello World", drawFont, brush, 0, 0);
                img.Dispose();
                pictureBox1.Refresh();
            }

        }

        private void btn_download_Click(object sender, EventArgs e)
        {
            //Application.StartupPath
            MessageBox.Show("文件保存格式:\r\n宽2字节+高2字节+单像素所占bit数(24)+像素数据在头位置的偏移+[...]+图像数据\r\n", "提示,请选择保存位置");
            if (pictureBox1.Image == null)
                return;
          // Bitmap bitmap = new Bitmap(FILEPATH.Text);
            Bitmap bitmap = pictureBox1.Image.Clone() as Bitmap;
            if (bitmap != null)
            {
                //位图矩形
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                Debug.WriteLine(bitmap.Width + "x" + bitmap.Height);
                //以可读写的方式将图像数据锁定
                BitmapData bmpdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);//PixelFormat.Format24bppRgb//bitmap.PixelFormat
                //得到图形在内存中的首地址
                IntPtr ptr = bmpdata.Scan0;

                //构造一个位图数组进行数据存储
#if false
                int LineByteCnt = (((bitmap.Width * 3 * 8 + 31) >> 5) << 2);
                int bytes = LineByteCnt * bitmap.Height;
                int skip = 4 - ((bitmap.Width * 3 * 8) >> 3)&3;
                //Debug.WriteLine("Linebyte=" + LineByteCnt + " bytes=" + bytes + " skip=" + skip);
#else
                int LineByteCnt = (((bitmap.Width * 3  + 3) >> 2) << 2);
                int bytes = LineByteCnt * bitmap.Height;
                int skip = 4 - (bitmap.Width * 3 ) & 3;//   (&3相当于%4)
                //Debug.WriteLine("Linebyte=" + LineByteCnt + " bytes=" + bytes + " skip=" + skip);
                //Debug.WriteLine("skip1=" + (bmpdata.Stride - bitmap.Width * 3));
#endif          
                byte[] rgbvalues = new byte[bytes];
                byte[] RGB_Array = new byte[bitmap.Width * bitmap.Height *3+ 6];
                int arcnt = 6;
                RGB_Array[0] = (byte)(bitmap.Width>>8);
                RGB_Array[1] = (byte)bitmap.Width;
                RGB_Array[2] = (byte)(bitmap.Height>>8);
                RGB_Array[3] = (byte)bitmap.Height;
                RGB_Array[4] = 24;//一个像素点需要的bit数
                RGB_Array[5] = 6;//数据在头的偏移位置
                //将被锁定的位图数据复制到该数组内
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, bytes);

                //对每一个像素的颜色进行灰度化
                double colortemp = 0;
                int WidthCnt = 0;
                for (int i = 0; i < rgbvalues.Length; i += 3)
                {

                    RGB_Array[arcnt] = rgbvalues[i];
                    RGB_Array[arcnt] = rgbvalues[i+1];
                    RGB_Array[arcnt] = rgbvalues[i+2];
                    arcnt += 3;
                    int MAX_BACK_COLOR = 0xff/2; //颜色值 
#if true
                    //转换为灰度图像
                    colortemp = rgbvalues[i + 2] * 0.299 + rgbvalues[i + 1] * 0.587 + rgbvalues[i] * 0.114;
                    if (colortemp > MAX_BACK_COLOR) colortemp = 0xff;
                    else colortemp = 0;
                    rgbvalues[i] = (byte)colortemp;
                    rgbvalues[i + 1] = (byte)colortemp;
                    rgbvalues[i + 2] = (byte)colortemp;
#else
                    int iType = 2;
                    switch (iType)
                    {
                        case 0://平均值法  
                            colortemp = ((rgbvalues[i] + rgbvalues[i+1] + rgbvalues[i+2]) / 3);
                            break;
                        case 1://最大值法  
                            colortemp = rgbvalues[i] > rgbvalues[i + 1] ? rgbvalues[i] : rgbvalues[i + 1];
                            colortemp = colortemp > rgbvalues[i + 2] ? colortemp : rgbvalues[i + 2];
                            break;
                        case 2://加权平均值法  
                            colortemp = ((int)(0.7 * rgbvalues[i]) + (int)(0.2 * rgbvalues[i + 1]) + (int)(0.1 * rgbvalues[i + 2]));
                            break;
                    }
                    rgbvalues[i] = (byte)colortemp;
                    rgbvalues[i + 1] = (byte)colortemp;
                    rgbvalues[i + 2] = (byte)colortemp;
#endif
                    WidthCnt++;
                    if (WidthCnt == bitmap.Width)
                    {
                        WidthCnt = 0;
                        i=i+skip;
                    }
                }
                //把处理后的图像数组复制回图像
                System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr, bytes);
                //解锁位图像素
                bitmap.UnlockBits(bmpdata);

                SaveFileDialog saveImageDialog = new SaveFileDialog();
                saveImageDialog.Title = "保存";
                saveImageDialog.Filter = @"bin文件|*.bin";
                if (saveImageDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveImageDialog.FileName.ToString();
                    fileName = GetNewPathForDupes(fileName);
                    FILEPATH.Text = fileName;
                    Write_ArrayToFile(RGB_Array, saveImageDialog.FileName);
                }
                 
                //bitmap.Save(Path.GetFileNameWithoutExtension(FILEPATH.Text)+".bmp", ImageFormat.Bmp);//保存灰度图像
                if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
                pictureBox1.Image = bitmap.Clone() as Image;
                pictureBox1.Refresh();

                bitmap.Dispose();

            }
        }
      /*格式:宽2字节+高2字节+单像素所占bit数+像素数据在头位置的偏移+...+图像数据*/
        private void btn_save1BitBin_Click(object sender, EventArgs e)
        {
            //Application.StartupPath
            MessageBox.Show("文件保存格式:\r\n宽2字节+高2字节+单像素所占bit数(1)+像素数据在头位置的偏移+[...]+图像数据\r\n", "提示,请选择保存位置");

            if (pictureBox1.Image == null)
                return;
            Bitmap bitmap = pictureBox1.Image.Clone() as Bitmap;
            if(bitmap!=null)
            {
                //位图矩形
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                //以可读写的方式将图像数据锁定
                BitmapData bmpdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);//PixelFormat.Format24bppRgb//bitmap.PixelFormat
                //得到图形在内存中的首地址
                IntPtr ptr = bmpdata.Scan0;
                int LineByteCnt = bmpdata.Stride;//每行字节宽度
                int scanBytes = LineByteCnt * bitmap.Height;//所有字节数
                int skip = bmpdata.Stride - bitmap.Width * 3;//每行多余的字节

                //scanBytes-skip*bitmap.Height等同于bitmap.Height * bitmap.Width
                int bppixel_num = ((bitmap.Height * bitmap.Width) / 8) + (((bitmap.Height * bitmap.Width)&0x07)>0?1:0);
                byte[] grayValues = new byte[bppixel_num+6];////////////////
                byte[] rgbvalues = new byte[scanBytes];

                Debug.WriteLine(bitmap.Width + "x" + bitmap.Height + " skip:" + skip);
                Debug.WriteLine("Linebyte=" + LineByteCnt + " bytes=" + scanBytes + " bppixel_num=" + bppixel_num);
                //将被锁定的位图数据复制到该数组内
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, scanBytes);
                grayValues[0] = (byte)(bitmap.Width >> 8);
                grayValues[1] = (byte)bitmap.Width;
                grayValues[2] = (byte)(bitmap.Height >> 8);
                grayValues[3] = (byte)bitmap.Height;
                grayValues[4] = 1;//一个像素点需要的bit数
                grayValues[5] = 6;//数据在头的偏移位置
                //对每一个像素的颜色进行二值化
                double colortemp = 0;
                int WidthCnt = 0;
                int bits = 0,j=6;
                for (int i = 0; i < rgbvalues.Length; i += 3)
                {
                    colortemp = rgbvalues[i + 2] * 0.299 + rgbvalues[i + 1] * 0.587 + rgbvalues[i] * 0.114;
                    grayValues[j] <<= 1;
                    if (colortemp > 127) 
                    {
                       grayValues[j]|=1; 
                    }
                    else 
                    {
                        //grayValues[j] |= 0;
                    }
                    bits++;
                    
                    if (bits == 8)
                    {
                        bits = 0;
                        if (j + 1 < bppixel_num)
                            grayValues[++j] = 0;
                    }

                    WidthCnt++;
                    if (WidthCnt == bitmap.Width)
                    {
                        WidthCnt = 0;
                        i = i + skip;
                    }
                }
                if ((bits&7)!=0)
                {
                    for(int i=0;i<8-bits;i++)
                    {
                        grayValues[j] <<= 1;
                    }
                }
                SaveFileDialog saveImageDialog = new SaveFileDialog();
                saveImageDialog.Title = "保存";
                saveImageDialog.Filter = @"bin文件|*.bin";
                if (saveImageDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveImageDialog.FileName.ToString();
                    fileName = GetNewPathForDupes(fileName);
                    FILEPATH.Text = fileName;
                    Write_ArrayToFile(grayValues, saveImageDialog.FileName);
                }
                //把处理后的图像数组复制回图像
               // System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr, scanBytes);
                //解锁位图像素
                bitmap.UnlockBits(bmpdata);
                bitmap.Dispose();
            }
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutBox AboutDialog = new frmAboutBox();
            AboutDialog.ShowDialog(this);
        }

        private void 打开图像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1_DoubleClick(null,null);
        }

        private void rec_box_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            rec_box.Text = "";
        }

        private void 重新搜索串口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Search_Port();
        }

    }

}
