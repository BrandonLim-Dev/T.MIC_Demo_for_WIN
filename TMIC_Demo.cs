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
        decimal portNumber;
        string protocol;

        // for MIC Device
        public MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        public MMDevice micDevice;

        // for Voice Processing
        private WasapiCapture capture;
        private WaveFileWriter writer;
        WaveFormat waveFormat;
        public byte[] micByte = new byte[0];

        // for Network
        IPEndPoint endPoint;
        // 1. UDP
        private static UdpClient udpCli;
        private Thread thrUDPSender;
        // 2. TCP/IP
        private static TcpClient tcpCli;
        #endregion

        public TMIC_Demo()
        {
            InitializeComponent();
            initCaptureVoice();
        }

        #region Property
        private void TMIC_Demo_Load(object sender, EventArgs e)
        {
            // Combobox Init
            cbProtocol.SelectedIndex = 0;
            tbIPAddress.Invoke(new MethodInvoker(delegate { tbIPAddress.Text = "192.168.50.100"; }));
            tbPortNumber.Invoke(new MethodInvoker(delegate { tbPortNumber.Text = "50011"; }));

            // 사용 가능한 마이크 Loading 및 기본값 설정
            SelectMICList();
            if (cbMICList.Items.Count > 0)
                cbMICList.SelectedIndex = 0;

            // Socket Thread
            tcpCli = new TcpClient();
            Thread tcpReceiveThread = new Thread(new ThreadStart(tcpReceive));
            tcpReceiveThread.IsBackground = true;
            tcpReceiveThread.Start();

            //udpCli = new UdpClient();
            //Thread udpSenderThread = new Thread(new ThreadStart(udpSender));
            //udpSenderThread.Start();
        }
        #endregion

        #region ButtonEvent
        private void btnConnection_Click(object sender, EventArgs e)
        {
            if (IsConnected == false)
            {
                if (tbIPAddress.Text == "" || tbPortNumber.Text == "" || cbMICList.Text == "")
                {
                    Console.WriteLine("IP 또는 PORT, 마이크 정보가 입력되지 않았습니다.");
                    return;
                }

                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결시도"; }));
                ipAddress = tbIPAddress.Text;
                portNumber = Convert.ToInt32(tbPortNumber.Text);
                protocol = cbProtocol.Text;

                if (ipAddress.Split('.').Length != 4 || portNumber < 0)
                {
                    Console.WriteLine("IP 또는 PORT 정보가 잘못 되었습니다. [{0}-{1}]", ipAddress, portNumber);
                    return;
                }

                if (cbProtocol.Text == "UDP")
                {
                    Console.WriteLine("UDP 프로토콜은 업데이트 예정입니다.");
                    return;
                }

                Console.WriteLine("[{0}:{1}-{2}] 연결 요청", ipAddress, portNumber, protocol);

                if (cbProtocol.Text == "TCP/IP")
                {
                    Thread tcpConnThread = new Thread(new ThreadStart(tcpConnect));
                    tcpConnThread.IsBackground = true;
                    tcpConnThread.Start();
                } 
                else
                {

                }

                // [SessionControl]
                // {
                //     "MsgType" : "ConnectModel",
                //     "MsgData" :
                //     {
                //         "ModelName" : "WHISPER-Large"
                //     }
                // }


                string sndMsg = reqSessionControl("WHISPER-Large");
                Console.WriteLine("sndMsg : {0}", sndMsg);

                             

                //capture.StartRecording();
                //
                //writer = new WaveFileWriter("recorded.wav", capture.WaveFormat);
                //
                //try
                //{
                //    while(true)
                //    {
                //        if(micByte.Length > 2048)
                //        {
                //            Console.WriteLine("{0}", micByte.ToString());
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //
                //}
            }
            else // IsConnected = true
            {
                btnConnection.Invoke(new MethodInvoker(delegate { btnConnection.Text = "연결요청"; }));
            }
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            if (IsConnected == false)
            {
                Console.WriteLine("마이크가 연결되지 않았습니다.");
            }
            else // IsConnected = true
            {

            }
        }
        #endregion

        #region Network
        private void tcpConnect()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(portNumber.ToString()));
            try
            {
                tcpCli.Connect(endPoint);
                Console.WriteLine("[tcpConnect() :: 연결 요청 완료");
            }
            catch (SocketException se)
            {
                Console.WriteLine("[tcpConnect() :: {0}", se.Message);
            }
        }

        private void tcpReceive()
        {
            while (true)
            {
                if(IsConnected == true)
                {
                    try
                    {
                        NetworkStream stream = tcpCli.GetStream();
                        byte[] buffer = new byte[1024];
                        int bytes = stream.Read(buffer, 0, buffer.Length);
                        if (bytes <= 0)
                            continue;
                        string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                        Console.WriteLine("[TCP Recv] {0}", message);

                        //1. [SessionControl]
                        //{
                        //    "result" : "0",
                        //    "readon" : "SUCCESS"
                        //}
                        //string result = resSessionControl(message);
                        //if (result.equals("0"))
                        //{
                        //    tcpSender();
                        //}

                        //2. [StreamTranscribe]
                        //{
                        //    "start" : "0",
                        //    "end" : "1040",
                        //    "txt" : "안녕하세요",
                        //    "vol" : "1049"
                        //}
                        //jsonStreamTranscribe(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[tcpReceive():: {0}", ex.ToString());
                    }
                }
            }
        }

        private void udpSender()
        {
            try
            {
                while (true)
                {
                    if (micByte.Length > 172)
                    {
                        byte[] datagram = new byte[172];
                        Array.Copy(micByte, 0, datagram, 0, 172);
                        micByte = splitByteArry(micByte, 172, micByte.Length - 172);
                        //Console.WriteLine("micBdatagramyte.Length : " + datagram.Length);
                        if (datagram.Length >= 172)
                        {
                            udpCli.Send(datagram, datagram.Length, "211.201.11.12", 50011);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UDP_Sender():: " + ex.ToString());
            }
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public byte[] CombineByteArray(byte[] first, byte[] second)
        {
            try
            {
                byte[] result = new byte[first.Length + second.Length];
                Array.Copy(first, 0, result, 0, first.Length);
                Array.Copy(second, 0, result, 0, second.Length);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CombineByteArray() ::" + ex.ToString());
                return null;
            }
        }

        private void Capture_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            try
            {
                byte[] bytes = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, bytes, 0, e.BytesRecorded);

                micByte = CombineByteArray(micByte, bytes);
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Capture_DataAvailable():: " + ex.ToString());
            }
        }

        private void initCaptureVoice()
        {
            try
            {
                micDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                capture = new WasapiCapture(micDevice);
                capture.DataAvailable += Capture_DataAvailable;
                waveFormat = new WaveFormat(16000, 16, 1); // rate, bits, channels
                capture.WaveFormat = waveFormat;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[initCaptureVoice():: {0}", ex.ToString());
            }
        }

        public byte[] splitByteArry(byte[] array, int startIndex, int length)
        {
            try
            {
                byte[] result = new byte[length];
                Array.Copy(array, startIndex, result, 0, length);
                return result;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region JSON
        private string reqSessionControl(string model)
        {
            //JObject msgType = new JObject(
            //    new JProperty("MsgType", "ConnectModel")
            //    );
            JObject msgData = new JObject(
                new JProperty("ModelName", model)
                );

            JObject jsonMsg = new JObject(
                new JProperty("msgType", "ConnectModel"),
                new JProperty("msgData", msgData)
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
            if (cbMICList.Text == "TCP/IP")
            {
                if(tcpCli.Connected == true)
                {
                    Console.WriteLine("[TCP/IP] 연결");
                    IsConnected = true;
                }
                else
                {
                    Console.WriteLine("[TCP/IP] 연결해제");
                    IsConnected = false;
                }
            }
        }
        #endregion
    }
}
