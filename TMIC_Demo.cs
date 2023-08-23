using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System.IO;

namespace T.MIC_Demo_for_WIN
{
    public partial class TMIC_Demo : Form
    {
        /// <summary>
        /// 전역변수 영역
        /// </summary>
        #region Global
        // Status
        bool IsConnected = false;
        bool IsMute = true;

        string ipAddress;
        int portNumber;
        string protocol;

        // Transfer Sequence
        int seq = -1;
        Int16 dataSeq;

        // for MIC Device
        public MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        public MMDevice micDevice;

        Dictionary<string, int> micDict = new Dictionary<string, int>();

        // for Voice Processing
        private WasapiCapture capture;
        private WaveFileWriter writer;
        WaveFormat waveFormat;
        public byte[] micByte = new byte[0];

        private WaveInEvent waveIn;
        string outputFolder;
        string outputFilePath;

        // for Network
        IPEndPoint endPoint;
        System.Threading.Timer asyncTimer;
        public const byte SESSION_CONTROL = 0x01;
        public const byte SESSION_TRANSCRIBE = 0x02;
        // 1. UDP
        private static UdpClient udpCli;
        private Thread thrUDPSender;
        // 2. TCP/IP
        private static TcpClient tcpCli;
        public const int TCP_PACKET_SIZE = 2048;
        Thread tcpConnThread;
        Thread tcpReceiveThread;
        #endregion

        public TMIC_Demo()
        {
            InitializeComponent();
        }

        #region Property
        private void TMIC_Demo_Load(object sender, EventArgs e)
        {
            // Combobox Init
            cbProtocol.SelectedIndex = 0;
            tbIPAddress.Invoke(new MethodInvoker(delegate { tbIPAddress.Text = "211.201.11.12"; }));
            mtbPortNumber.Invoke(new MethodInvoker(delegate { mtbPortNumber.Text = "50030"; }));

            btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightPink; }));
            btnMute.Invoke(new MethodInvoker(delegate { btnMute.BackColor = Color.LightPink; }));

            // 사용 가능한 마이크 Loading 및 기본값 설정
            SelectMICList();
            if (cbMICList.Items.Count > 0)
                cbMICList.SelectedIndex = 0;

            outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(outputFolder);
            outputFilePath = Path.Combine(outputFolder, "recorded.wav");

            lbLog.HorizontalScrollbar = true;
            lbSttText.HorizontalScrollbar = true;
            
            // Socket Thread
            //tcpCli = new TcpClient();
            //Thread tcpReceiveThread = new Thread(new ThreadStart(tcpReceive));
            //tcpReceiveThread.IsBackground = true;
            //tcpReceiveThread.Start();

            //udpCli = new UdpClient();
            //Thread udpSenderThread = new Thread(new ThreadStart(udpSender));
            //udpSenderThread.Start();
        }

        private void tbIPAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 입력된 키가 숫자나 점(.)이 아니면 입력을 막습니다.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        #endregion

        #region ButtonEvent
        private void btnConnection_Click(object sender, EventArgs e)
        {
            if (IsConnected == false)
            {
                if (tbIPAddress.Text == "" || mtbPortNumber.Text == "" || cbMICList.Text == "")
                {
                    Console.WriteLine("IP 또는 PORT, 마이크 정보가 입력되지 않았습니다.");
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("IP 또는 PORT, 마이크 정보가 입력되지 않았습니다."); }));
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                    return;
                }

                ipAddress = tbIPAddress.Text;
                portNumber = int.Parse(mtbPortNumber.Text);
                protocol = cbProtocol.Text;

                if (ipAddress.Split('.').Length != 4 || portNumber < 0)
                {
                    Console.WriteLine("IP 또는 PORT 정보가 잘못 되었습니다. [{0}-{1}]", ipAddress, portNumber);
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("IP 또는 PORT 정보가 잘못 되었습니다. [" + ipAddress + "]-[" + portNumber + "]"); }));
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                    return;
                }

                if (cbProtocol.Text == "UDP")
                {
                    Console.WriteLine("UDP 프로토콜은 업데이트 예정입니다.");
                    MessageBox.Show("UDP 프로토콜은 업데이트 예정입니다.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
               
                    return;
                }

                Console.WriteLine("[{0}:{1}-{2}] 연결 요청", ipAddress, portNumber, protocol);
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("[" + ipAddress + ":" + portNumber + "-" + protocol + "] 연결 요청"); }));
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));

                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결시도"; }));

                if (cbProtocol.Text == "TCP/IP")
                {
                    tcpCli = new TcpClient();

                    tcpConnThread = new Thread(new ThreadStart(tcpConnect));
                    tcpConnThread.IsBackground = true;
                    tcpConnThread.Start();
                } 
                else // UDP
                {

                }
                Delay(2000);

                if (IsConnected == true)
                {
                    tcpReceiveThread = new Thread(new ThreadStart(tcpReceive));
                    tcpReceiveThread.IsBackground = true;
                    tcpReceiveThread.Start();

                    // make TCP/IP Send Message
                    // [SessionControl]
                    // {
                    //     "MsgType" : "ConnectModel",
                    //     "MsgData" :
                    //     {
                    //         "ModelName" : "WHISPER"
                    //     }
                    // }
                    // TCP/IP Body
                    string sndMsg = reqSessionControl("WHISPER");
                    if (sndMsg.Length <= 0)
                    {
                        Console.WriteLine("reqSessionControl():: 메시지 생성 실패");
                        lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("reqSessionControl():: 메시지 생성 실패"); }));
                        lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                        return;
                    }
                    Console.WriteLine("sndMsg({0}) : {1}", sndMsg.Length, sndMsg);
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("sndMsg(" + sndMsg.Length + ") : " + sndMsg); }));
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                    // TCP/IP Header
                    byte[] version = { 0x00 };
                    byte[] msgcode = { 0x01 }; // 0x01 : SessionControl, 0x02 : SessionTranscribe
                    seq = seq + 1;
                    Byte[] header = makeTcpHeader(version, msgcode, seq, sndMsg.Length);

                    // TCP/IP Message Send
                    try
                    {
                        if(tcpCli.Connected)
                        {
                            NetworkStream tcpStream = tcpCli.GetStream();
                            // send Header (byte)
                            tcpStream.Write(header, 0, header.Length);
                            // send Body (String - json)
                            byte[] bodyBuf = Encoding.UTF8.GetBytes(sndMsg);
                            tcpStream.Write(bodyBuf, 0, bodyBuf.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    // ------------------------------------------------------------------  
                }
            }
            else // IsConnected = true
            {
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("TCP 연결 해제"); }));
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));

                IsConnected = false;
                seq = -1;
                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결요청"; }));
                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightPink; }));
                IsMute = true;
                btnMute.Invoke(new MethodInvoker(delegate { btnMute.Text = "마이크 꺼짐"; }));
                btnMute.Invoke(new MethodInvoker(delegate { btnMute.BackColor = Color.LightPink; }));
                tbIPAddress.Invoke(new MethodInvoker(delegate { tbIPAddress.ReadOnly = false; }));
                mtbPortNumber.Invoke(new MethodInvoker(delegate { mtbPortNumber.ReadOnly = false; }));
                cbMICList.Invoke(new MethodInvoker(delegate { cbMICList.Enabled = true; }));
                cbProtocol.Invoke(new MethodInvoker(delegate { cbProtocol.Enabled = true; }));
                lbLog.Items.Clear();
                lbSttText.Items.Clear();

                //asyncTimer.Change(0, System.Threading.Timeout.Infinite);
                if (writer != null)
                {
                    writer?.Dispose();
                    writer = null;
                }
                
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                }

                bool flag = false;
                cbProtocol.Invoke(new MethodInvoker(delegate { flag = cbProtocol.Text == "TCP/IP" ? true : false; }));
                if (flag) // TCP/IP
                {
                    if(tcpConnThread != null)
                    {
                        tcpConnThread.Interrupt();
                        //tcpConnThread = null;
                    }
                    if(tcpReceiveThread != null)
                    {
                        tcpReceiveThread.Interrupt();
                        //tcpReceiveThread = null;
                    }
                    if(tcpCli != null)
                    {
                        tcpCli.Close();
                        //tcpCli = null;
                    }
                }
                else // UDP
                {

                }
            }
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            if (IsConnected == false)
            {
                Console.WriteLine("네트워크가 연결되지 않았습니다.");
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("네트워크가 연결되지 않았습니다."); }));
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
            }
            else // IsConnected = true
            {
                if (IsMute == true) // 마이크 꺼짐
                {
                    IsMute = false;
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.Text = "마이크 켜짐"; }));
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.BackColor = Color.LightGreen; }));
                    Console.WriteLine("UNMUTE");
                }
                else // IsMute == false 마이크 켜짐
                {
                    IsMute = true;
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.Text = "마이크 꺼짐"; }));
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.BackColor = Color.LightPink; }));
                    Console.WriteLine("MUTE");
                }
            }
        }
        #endregion

        #region Network
        private byte[] makeTcpHeader(byte[] ver, byte[] msgCode, int seq, int length)
        {
            Int16 Seq = Convert.ToInt16(seq);
            byte[] sequence = BitConverter.GetBytes(Seq);
            byte[] payloadlength = BitConverter.GetBytes(length);

            Byte[] result = ver.Concat(msgCode).Concat(sequence).Concat(payloadlength).ToArray();

            return result;
        }

        private void tcpConnect()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNumber);
            try
            {
                if (tcpCli == null)
                    tcpCli = new TcpClient();
                tcpCli.Connect(endPoint);
                Console.WriteLine("[tcpConnect() :: 연결 요청 완료");
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("[tcpConnect() :: 연결 요청 완료"); }));
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
            }
            catch (SocketException se)
            {
                Console.WriteLine("[tcpConnect() :: {0}", se.Message);
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("[tcpConnect() :: " + se.Message); }));
                lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결 요청"; }));
            }
        }

        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }

        private void tcpReceive()
        {
            while (tcpCli.Connected)
            {
                try
                {
                    NetworkStream stream = tcpCli.GetStream();

                    byte[] rBuffer = new byte[2048];
                    int bytes = stream.Read(rBuffer, 0, rBuffer.Length);
                    if (bytes <= 0)
                        continue;
                    string message = Encoding.UTF8.GetString(rBuffer, 0, bytes);

                    Console.WriteLine("[TCP Recv]\n {0}", message);

                    // 1. GET Header
                    byte[] buffer = new byte[8];
                    Array.Copy(rBuffer, buffer, buffer.Length);
                    Console.WriteLine(BitConverter.ToString(buffer));

                    // 1-1. version (1byte)
                    byte[] tmpVersion = new byte[1];
                    Buffer.BlockCopy(buffer, 0, tmpVersion, 0, 1);
                    string Version = BitConverter.ToString(tmpVersion);
                    Console.WriteLine("Version : {0}", Version);
                    // 1-2. MsgCode (1byte)
                    byte[] tmpMsgCode = new byte[1];
                    Buffer.BlockCopy(buffer, 1, tmpMsgCode, 0, 1);
                    string MsgCode = BitConverter.ToString(tmpMsgCode);
                    Console.WriteLine("MsgCode {0}", MsgCode);
                    // 1-3. Sequence (2byte)
                    byte[] tmpSeq = new byte[2];
                    Buffer.BlockCopy(buffer, 2, tmpSeq, 0, 2);
                    short Sequence = BitConverter.ToInt16(tmpSeq, 0);
                    Console.WriteLine("Sequence : {0}", Sequence);
                    // 1-4. PayloadLength (4byte)
                    byte[] tmpLength = new byte[4];
                    Buffer.BlockCopy(buffer, 4, tmpLength, 0, 4);
                    int PayloadLength = BitConverter.ToInt32(tmpLength, 0);
                    Console.WriteLine("PayloadLength : {0}", PayloadLength);

                    // 2. GET Body
                    buffer = new byte[PayloadLength];
                    //bytes = stream.Read(buffer, 0, buffer.Length);
                    //if (bytes <= 0)
                    //    continue;
                    Array.Copy(rBuffer, 8, buffer, 0, buffer.Length);
                    message = Encoding.UTF8.GetString(buffer, 0, PayloadLength);
                    Console.WriteLine("[TCP Recv - Body]\n {0}", message);
                    lbLog.Invoke(new MethodInvoker(delegate {
                        //lbLog.IntegralHeight = true;
                        lbLog.Items.Add("rcvMsg(" + PayloadLength + ") : " + message);
                        //int hzSize = (int)g.MeasureString(lbLog.Items[lbLog.Items.Count - 1].ToString(), lbLog.Font).Width;
                        //Console.WriteLine("text size : " + hzSize);
                        //lbLog.HorizontalExtent = hzSize + 50;
                        lbLog.SelectedIndex = lbLog.Items.Count - 1;
                    }));

                    if (MsgCode.Equals("01"))
                    {
                        Console.WriteLine("SessionControl");

                        //1. [SessionControl]
                        //{
                        //    "result" : "0",
                        //    "readon" : "SUCCESS"
                        //}
                        string result = resSessionControl(message);
                        if (result.Equals("0"))
                        {
                            lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("STT Model 연결 완료!!"); }));
                            lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));
                            btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결됨"; }));
                            btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightGreen; }));
                            tbIPAddress.Invoke(new MethodInvoker(delegate { tbIPAddress.ReadOnly = true; }));
                            mtbPortNumber.Invoke(new MethodInvoker(delegate { mtbPortNumber.ReadOnly = true; }));
                            cbMICList.Invoke(new MethodInvoker(delegate { cbMICList.Enabled = false; }));
                            cbProtocol.Invoke(new MethodInvoker(delegate { cbProtocol.Enabled = false; }));

                            // Voice from MIC Device Capture Start
                            initWaveIn();
                            waveIn.StartRecording();
                            writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
                        }
                        else // Error Code
                        {
                            if (result.Equals("1000"))
                            {
                                Console.WriteLine("[TCP Recv Error] {0})", result);
                            }
                        }
                    }
                    else if (MsgCode.Equals("02"))
                    {
                        Console.WriteLine("SessionTranscribe");

                        //2. [StreamTranscribe]
                        //{
                        //    "start" : "0",
                        //    "end" : "1040",
                        //    "txt" : "안녕하세요",
                        //    "vol" : "1049"
                        //}
                        // <!-- 20230823 v1.1 STT 변환 결과 표시 수정 --!>
                        (string start, string end, string result) = jsonStreamTranscribe(message);

                        string startTime = ConvertMilisecondsToTime(start);
                        string endTime = ConvertMilisecondsToTime(end);
                        string sttText = "[" + startTime + "-" + endTime + "]  " + result;

                        lbSttText.Invoke(new MethodInvoker(delegate { lbSttText.Items.Add(sttText); }));
                        lbSttText.Invoke(new MethodInvoker(delegate { lbSttText.SelectedIndex = lbSttText.Items.Count - 1; }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[tcpReceive():: {0}", ex.ToString());
                    // 종료시 내용 추가                    
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add("TCP 연결 해제"); }));
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.SelectedIndex = lbLog.Items.Count - 1; }));

                    IsConnected = false;
                    seq = -1;
                    btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결요청"; }));
                    btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightPink; }));
                    IsMute = true;
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.Text = "마이크 꺼짐"; }));
                    btnMute.Invoke(new MethodInvoker(delegate { btnMute.BackColor = Color.LightPink; }));
                    tbIPAddress.Invoke(new MethodInvoker(delegate { tbIPAddress.ReadOnly = false; }));
                    mtbPortNumber.Invoke(new MethodInvoker(delegate { mtbPortNumber.ReadOnly = false; }));
                    cbMICList.Invoke(new MethodInvoker(delegate { cbMICList.Enabled = true; }));
                    cbProtocol.Invoke(new MethodInvoker(delegate { cbProtocol.Enabled = true; }));
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Clear(); }));
                    lbSttText.Invoke(new MethodInvoker(delegate { lbSttText.Items.Clear(); }));

                    //asyncTimer.Change(0, System.Threading.Timeout.Infinite);
                    if (writer != null)
                    {
                        writer?.Dispose();
                        writer = null;
                    }

                    if (waveIn != null)
                    {
                        waveIn.StopRecording();
                        waveIn.Dispose();
                    }

                    bool flag = false;
                    cbProtocol.Invoke(new MethodInvoker(delegate { flag = cbProtocol.Text == "TCP/IP" ? true : false; }));
                    if (flag) // TCP/IP
                    {
                        if(tcpConnThread != null)
                        {
                            tcpConnThread.Interrupt();
                            tcpConnThread = null;
                        }
                        if(tcpReceiveThread != null)
                        {
                            tcpReceiveThread.Interrupt();
                            tcpReceiveThread = null;
                        }
                        if(tcpCli != null)
                        {
                            tcpCli.Close();
                            //tcpCli = null;
                        }
                    }
                    else // UDP
                    {

                    }
                }
            }
        }

        private void udpSender()
        {
            //try
            //{
            //    while (true)
            //    {
            //        if (micByte.Length > 172)
            //        {
            //            byte[] datagram = new byte[172];
            //            Array.Copy(micByte, 0, datagram, 0, 172);
            //            micByte = splitByteArray(micByte, 172, micByte.Length - 172);
            //            //Console.WriteLine("micBdatagramyte.Length : " + datagram.Length);
            //            if (datagram.Length >= 172)
            //            {
            //                udpCli.Send(datagram, datagram.Length, "211.201.11.12", 50011);
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("UDP_Sender():: " + ex.ToString());
            //}
        }
        #endregion

        #region Inner Function
        private void SelectMICList()
        {
            try
            {
                MMDeviceCollection devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                foreach (var device in devices)
                {
                    cbMICList.Items.Add(device.FriendlyName);
                }

                for (int i = -1; i < NAudio.Wave.WaveIn.DeviceCount; i++)
                {
                    var device = NAudio.Wave.WaveIn.GetCapabilities(i);
                    Console.WriteLine($"{i}: {device.ProductName}");
                    micDict.Add(device.ProductName, i);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void DataAvailable(object sender, NAudio.Wave.WaveInEventArgs a)
        {
            //Console.WriteLine("{0}-{1}", a.Buffer.Length, a.BytesRecorded);
            if(writer != null)
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);

                if(tcpCli.Connected)
                {
                    NetworkStream tcpStream = tcpCli.GetStream();

                    // TCP/IP Header
                    byte[] version = { 0x00 };
                    byte[] msgcode = { 0x02 }; // 0x01 : SessionControl, 0x02 : SessionTranscribe
                    seq = seq + 1;
                    Byte[] header = makeTcpHeader(version, msgcode, seq, a.BytesRecorded);
                    //Console.WriteLine("seq : {0}, micByte.Length : {1}", seq, micByte.Length);

                    tcpStream.Write(header, 0, header.Length);
                    // TCP/IP Body
                    if (IsMute == false)
                    {
                        tcpStream.Write(a.Buffer, 0, a.BytesRecorded);
                    }
                    else
                    {
                        byte[] muteByte = new byte[a.BytesRecorded];
                        Array.Clear(muteByte, 0x00, muteByte.Length);
                        tcpStream.Write(muteByte, 0, muteByte.Length);
                    }
                }
            }
        }

        private void initWaveIn()
        {
            cbMICList.Invoke(new MethodInvoker(delegate { Console.WriteLine("사용할 마이크 : {0}({1})", cbMICList.Text, micDict[cbMICList.Text]); }));
            try
            {
                int deviceNumber = -1;
                cbMICList.Invoke(new MethodInvoker(delegate { deviceNumber = micDict[cbMICList.Text]; }));

                waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceNumber,
                    WaveFormat = new WaveFormat(rate: 16000, bits: 16, channels: 1)                    
                };
                waveIn.DataAvailable += DataAvailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[initWaveIn():: {0}", ex.ToString());
            }
        }

        private void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);
            while (dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return;
        }

        private string ConvertMilisecondsToTime(string milisec)
        {
            long tick = long.Parse(milisec);
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(tick);
            string Time = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            return Time;
        }
        #endregion

        #region JSON
        private string reqSessionControl(string model)
        {
            JObject msgData = new JObject(
                new JProperty("ModelName", model)
                );

            JObject jsonMsg = new JObject(
                new JProperty("MsgType", "ConnectModel"),
                new JProperty("MsgData", msgData)
                );

            return jsonMsg.ToString();
        }

        private string resSessionControl(string jsonString)
        {
            string result;
            string reason;

            JObject jObject = JObject.Parse(jsonString);
            result = jObject["result"].ToString();
            reason = jObject["reason"].ToString();
            Console.WriteLine("result : {0}, reason : {1}", result, reason);

            return result;
        }

        // <!-- 20230823 v1.1 STT 변환 결과 표시 수정 --!>
        private static (string, string, string) jsonStreamTranscribe(string jsonString)
        {
            string start;
            string end;
            string txt;
            string vol;

            JObject jObject = JObject.Parse(jsonString);
            start = jObject["start"].ToString();
            end = jObject["end"].ToString();
            txt = jObject["txt"].ToString();
            vol = jObject["vol"].ToString();

            Console.WriteLine("start : {0}, end : {1}1, txt : {2}, vol : {3}", start, end, txt, vol);

            // <!-- 20230823 v1.1 STT 변환 결과 표시 수정 --!>
            return (start, end, txt);
        }
        #endregion

        #region Timer
        /// <summary>
        /// 프로그램 상태관리 체크 타이머
        ///  - TCP 연동 상태 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void statusMngTimer_Tick(object sender, EventArgs e)
        {
            if (cbProtocol.Text == "TCP/IP")
            {
                if (tcpCli != null)
                {
                    if (tcpCli.Connected == true)
                    {
                        //Console.WriteLine("[TCP/IP] 연결");
                        IsConnected = true;
                        //btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결됨"; }));
                        //btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightGreen; }));
                    }
                    else
                    {
                        //Console.WriteLine("[TCP/IP] 연결해제");
                        IsConnected = false;
                        //btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결요청"; }));
                        //btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.BackColor = Color.LightPink; }));
                    }
                }
            }
            else
            {

            }
        }
        #endregion

    }
}
