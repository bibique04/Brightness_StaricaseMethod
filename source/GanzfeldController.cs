#define TestY
#define Fovea//Peri//Fovea	// fovea/peripheral

using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ComPort;
using SequenceGenerator;
using Data;
using FileIO;

namespace GanzfeldController
{
    public class GanzeldController : System.Windows.Forms.Form
    {
        //------------------------------------------------
        // TESTVCG.h
        //------------------------------------------------
        //static int ColorNum = 256;
        static double PI = (4.0 * Math.Atan(1.0));

        static int N = 140;
        //static double TF = 5.0;// Hz
        static int InitialCntrstLvl = 22;
        static int ECC = 0;
        static int FieldSize = 5;   //deg fovea 5deg, 10degECC 10deg, 20degECC 15deg

        static int Dt = 14; // frames
        //static int RampTime = 0; // RampTime/150 sec
        static int NumberOfReverse = 11;
        static double Phi = (0.0);// physical phase shift [deg]

#if TestY  // based on ST_Ganzfeld_RContrastfile_07Sep2019.xls
        static double ConRed = 0.095577; //r 
        static double ConGreen = 0.069734; //g
        static double ConBlue = 0.084462; //b
        static double ConOrange = 0.189725; //y
        static double meanL = 60.01593;
        static double meanM = 20.41335;
        static double meanS = 1.65755;
        static double meanipRGC = 17.17489;
        static double phaseL = 0;
        static double phaseM = 0;
        static double phaseS = 0;
        static double phaseipRGC = 0;
        #endif

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_RBUTTONDOWN = 0x0204;
        const string SerialPortNumber = "TEXT(\"COM3\")";
        const int Brate = 115200;
        const int MAXFILE = 256;
        //-----------------------------------------------------------
        // END TESTVCG.h
        //-----------------------------------------------------------

        //--------------------------------------------------
        // GLOBAL ATTRIBUTES C file
        //--------------------------------------------------
        int w, h;
        RGB2[] Color2RGB = new RGB2[1200];
        int[,] FixP = new int[50, 50];
        int[] j = new int[10];
        int RightLeft, Direction, VectorDirection = 1;
        double[,] C2PArray = new double[4, 4];
        uint OrderCntr;
        uint[] ContrastLevel = new uint[12];
        uint[] EndFlg = new uint[12] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1 }; //(0,3)(1,4)(2,5)
        RECORD[] Record = new RECORD[12];
        Receptors Contrast = new Receptors();
        Receptors MaskContrast = new Receptors();
        short[] buff = new short[256];
        double[] PhaseAngle = new double[12] { 0, 30, 60, 90, 120, 150, 180, 210, 225, 240, 45, 135 };
        double[] SFs = new double[12] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 };
        double SF = 1;
        RGB2 AmpLEDs = new RGB2();
        int OutOfGamut = 0;
        char[] filename = new char[MAXFILE + 4];
        int[] order = new int[24];

        //static unsigned int MouseFlg = 0;
        static uint FirstFlg = 0;
        static uint StimulusPresentationCounter = 0;
        double angleX = 108.82; //isoluminant axis in L,M plane

        Receptors PhaseReceptors = new Receptors();
        RGB2 PhaseLEDs = new RGB2();

        //----------------------------added 28Dec2007
        static char[] str = new char[3000];    /* 送信データを格納する */
        float[] s_Buf = new float[100];


        /*
        HANDLE hComm;       // シリアルポートのハンドル 
        DCB dcb;            // 通信パラメータ 
        DWORD writesize;    // ポートへ書き込んだバイト数 
        DWORD dwErrors;
        COMSTAT ComStat;
        DWORD dwCount;
        DWORD dwRead;
        */
        //--------------------------------------------------------
        // END GLOBAL ATTRIBUTES C FILE
        //--------------------------------------------------------

        //--------------------------------------------------------
        // GLOBAL ATTRIBUTES C#
        //--------------------------------------------------------

        VirtualComPort _vcp = new VirtualComPort();
        JsonContent content = new JsonContent();
        Logger result;
        Logger log;
        int stimulationNumber = 1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;

        //--------------------------------------------------------
        // END GLOBAL ATTRIBUTES C#
        //--------------------------------------------------------

        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public GanzeldController()
        {
            Initialize();
            InitializeComponent();
            CreateRSTFile();
            CreateLogFile();
            WarmUp();
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {                
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of stimulation : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Mouse Event : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(146, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(148, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "None";
            // 
            // GanzeldController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "GanzeldController";
            this.Text = "GanzfeldController";
            this.Load += new System.EventHandler(this.GanzeldController_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
        
        protected override void WndProc(ref Message m)
        {
            this.label3.Text = stimulationNumber.ToString();
            switch (m.Msg)
            {
               
                case WM_LBUTTONDOWN:
                    //log.Write("");
                    //log.Write("Stimluation #"+ stimulationNumber + " => Left button clicked");
                    this.label4.Text = "Left button clicked";
                    stimulationNumber++;
                    mouse(1);
                    break;
                case WM_RBUTTONDOWN:
                    //log.Write("");
                    //log.Write("Stimluation #" + stimulationNumber + " => Right button clicked");
                    this.label4.Text = "Right button clicked";
                    stimulationNumber++;
                    mouse(0);
                    break;
            }
            base.WndProc(ref m);
        }
        
        private void GanzeldController_Load(object sender, EventArgs e)
        {

        }
        private void CreateLogFile()
        {
            log.Write("File create on : " + DateTime.Now.ToString());
            log.Write("----------------BEGIN STIMULATION ATTRIBUTES---------------");
            log.Write("");
            log.Write("mean value Red : " + ConRed.ToString());
            log.Write("mean value Green : " + ConGreen.ToString());
            log.Write("mean value Blue : " + ConBlue.ToString());
            log.Write("mean value Orange : " + ConOrange.ToString());
            log.Write("duration of stimulus : " + ContentGenerator.getStimulusDuration().ToString() + "ms");
            log.Write("duration of the ramp(Hamming window) : " + ContentGenerator.getRampTime().ToString() + "ms");
            log.Write("temporal frequency : " + ContentGenerator.getTemporalFrequency().ToString() + "Hz");
            log.Write("");
            log.Write("----------------END STIMULATION ATTRIBUTES---------------");
            log.Write("");
            log.Write("----------------BEGIN GAMMA CORRECTION---------------");
            log.Write("");
            log.Write("R : " + IntensityVoltage.Int2Volt[0, 0].ToString() + "  |  " + IntensityVoltage.Int2Volt[0, 1].ToString() + "  |  " + IntensityVoltage.Int2Volt[0, 2].ToString() + "  |  " + IntensityVoltage.Int2Volt[0, 3].ToString());
            log.Write("G : " + IntensityVoltage.Int2Volt[1, 0].ToString() + "  |  " + IntensityVoltage.Int2Volt[1, 1].ToString() + "  |  " + IntensityVoltage.Int2Volt[1, 2].ToString() + "  |  " + IntensityVoltage.Int2Volt[1, 3].ToString());
            log.Write("B : " + IntensityVoltage.Int2Volt[2, 0].ToString() + "  |  " + IntensityVoltage.Int2Volt[2, 1].ToString() + "  |  " + IntensityVoltage.Int2Volt[2, 2].ToString() + "  |  " + IntensityVoltage.Int2Volt[2, 3].ToString());
            log.Write("Y : " + IntensityVoltage.Int2Volt[3, 0].ToString() + "  |  " + IntensityVoltage.Int2Volt[3, 1].ToString() + "  |  " + IntensityVoltage.Int2Volt[3, 2].ToString() + "  |  " + IntensityVoltage.Int2Volt[3, 3].ToString());
            log.Write("");
            log.Write("----------------END GAMMA CORRECTION---------------");
            log.Write("");
            log.Write("----------------BEGIN P2C ARRAY---------------");
            log.Write("");
            log.Write("            Y                 B               G               R");
            log.Write("L     : " + P2CArray.p2cArray[0, 0].ToString() + "  |  " + P2CArray.p2cArray[0, 1].ToString() + "  |  " + P2CArray.p2cArray[0, 2].ToString() + "  |  " + P2CArray.p2cArray[0, 3].ToString());
            log.Write("M     : " + P2CArray.p2cArray[1, 0].ToString() + "  |  " + P2CArray.p2cArray[1, 1].ToString() + "  |  " + P2CArray.p2cArray[1, 2].ToString() + "  |  " + P2CArray.p2cArray[1, 3].ToString());
            log.Write("S     : " + P2CArray.p2cArray[2, 0].ToString() + "  |  " + P2CArray.p2cArray[2, 1].ToString() + "  |  " + P2CArray.p2cArray[2, 2].ToString() + "  |  " + P2CArray.p2cArray[2, 3].ToString());
            log.Write("ipRgc : " + P2CArray.p2cArray[3, 0].ToString() + "  |  " + P2CArray.p2cArray[3, 1].ToString() + "  |  " + P2CArray.p2cArray[3, 2].ToString() + "  |  " + P2CArray.p2cArray[3, 3].ToString());
            log.Write("");
            log.Write("----------------END P2C ARRAY---------------");
            log.Write("");
        }
        private void CreateRSTFile()
        {
            MessageBox.Show("This is a program modified based on YN_LMS_ipRGC_10Hz_1s_18Mar2015 that can vary a phase shift among receptors.");

            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = @"C:\Users\Tsuji-lab\Documents\GanzfeldSoftware\Result2",
                Title = "Browse rst file",
                CheckPathExists = true,
                CheckFileExists = false,
                CreatePrompt = true,
                OverwritePrompt = false,
                DefaultExt = "rst",
                Filter = "(*.rst)|*rst",

            };
            dialog.ShowDialog();

            result = new Logger(dialog.FileName);
            log = new Logger(dialog.InitialDirectory + "\\" + Path.GetFileNameWithoutExtension(dialog.FileName) + ".log");

            result.Write("File create on : " + DateTime.Now.ToString());

        }
        private void WarmUp()
        {
            // call the method to generate PWM value and send it to the microcontroller
            content = ContentGenerator.GenerateBackground(ConRed, ConGreen,ConBlue,ConOrange);
                // Stop everything the cube was doing
                _vcp.SendJsonContent("{}");
                _vcp.SendJsonContent("{\"StopAllSequences\" : 0}");

                // Send the new configuration
                _vcp.SendJsonContent(JsonConvert.SerializeObject(content));

                // Start only the used channels
                foreach (Sequence seq in content.Sequences)
                {
                    _vcp.SendJsonContent("{\"StartSequence\" : " + seq.LedIndex + "}");
                }
            
        }
        //---------------------------------------------------------------------------
        void Phos2Cone(ref RGB2 PhosLumi,ref Receptors Receptor)
        {
            double nR, nO, nG, nB;

            nR = PhosLumi.Red;
            nG = PhosLumi.Green;
            nB = PhosLumi.Blue;
            nO = PhosLumi.Orange;

            Receptor.L = (P2CArray.p2cArray[0, 0] * nO) + (P2CArray.p2cArray[0, 1] * nB) + (P2CArray.p2cArray[0, 2] * nG) + (P2CArray.p2cArray[0, 3] * nR);
            Receptor.M = (P2CArray.p2cArray[1, 0] * nO) + (P2CArray.p2cArray[1, 1] * nB) + (P2CArray.p2cArray[1, 2] * nG) + (P2CArray.p2cArray[1, 3] * nR);
            Receptor.S = (P2CArray.p2cArray[2, 0] * nO) + (P2CArray.p2cArray[2, 1] * nB) + (P2CArray.p2cArray[2, 2] * nG) + (P2CArray.p2cArray[2, 3] * nR);
            Receptor.ipRGC = (P2CArray.p2cArray[3, 0] * nO) + (P2CArray.p2cArray[3, 1] * nB) + (P2CArray.p2cArray[3, 2] * nG) + (P2CArray.p2cArray[3, 3] * nR);

            if (Receptor.L < 0) {
                Receptor.L = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Receptor.M < 0) {
                Receptor.M = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Receptor.S < 0) {
                Receptor.S = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Receptor.ipRGC < 0) {
                Receptor.ipRGC = 0;
                EndFlg[VectorDirection] = 1;
            }
        }
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        void CalcDuties(ref RGB2 AmpLEDs)
        {
            RGB2 BgnDuty = new RGB2();
            RGB2 Duty = new RGB2();
            RGB2 AmpLEDs1 = new RGB2();
            //Red
            BgnDuty.Red = ConRed;
            //------------------------------------------------------------
            if (AmpLEDs.Red >= 0) {
                AmpLEDs1.Red = AmpLEDs.Red; //plus x plus
            }
            else
            {
                AmpLEDs1.Red = AmpLEDs.Red; //minus x plus
            }
            Duty.Red = BgnDuty.Red + AmpLEDs1.Red;
            Duty.Red = Duty.Red * (Duty.Red * Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 0] + Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 1] + Duty.Red * IntensityVoltage.Int2Volt[0, 2] + IntensityVoltage.Int2Volt[0, 3]);
            if (Duty.Red >= 1) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Red <= 0) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }

            if (AmpLEDs.Red >= 0) {
                AmpLEDs1.Red = AmpLEDs.Red; //plus x plus
            }
            else
            {
                AmpLEDs1.Red = AmpLEDs.Red; //minus x plus
            }
            Duty.Red = BgnDuty.Red - AmpLEDs1.Red;
            Duty.Red = Duty.Red * (Duty.Red * Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 0] + Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 1] + Duty.Red * IntensityVoltage.Int2Volt[0, 2] + IntensityVoltage.Int2Volt[0, 3]);
            if (Duty.Red >= 1) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Red <= 0) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }

            //Green
            BgnDuty.Green = ConGreen;
            //------------------------------------------------------------
            if (AmpLEDs.Green >= 0) {
                AmpLEDs1.Green = AmpLEDs.Green; //plus x plus
            }
            else
            {
                AmpLEDs1.Green = AmpLEDs.Green; //minus x plus
            }
            Duty.Green = BgnDuty.Green + AmpLEDs1.Green;
            Duty.Green = Duty.Green * (Duty.Green * Duty.Green * Duty.Green * IntensityVoltage.Int2Volt[1, 0] + Duty.Green * Duty.Green * IntensityVoltage.Int2Volt[1, 1] + Duty.Green * IntensityVoltage.Int2Volt[1, 2] + IntensityVoltage.Int2Volt[1, 3]);
            if (Duty.Green >= 1) {
                AmpLEDs.Green = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Green <= 0) {
                AmpLEDs.Green = 0;
                EndFlg[VectorDirection] = 1;
            }
            //
            if (AmpLEDs.Green >= 0) {
                AmpLEDs1.Green = AmpLEDs.Green; //plus x minus
            }
            else
            {
                AmpLEDs1.Green = AmpLEDs.Green; //minus x minus
            }
            Duty.Green = BgnDuty.Green - AmpLEDs1.Green;
            Duty.Green = Duty.Green * (Duty.Green * Duty.Green * Duty.Green * IntensityVoltage.Int2Volt[1, 0] + Duty.Green * Duty.Green * IntensityVoltage.Int2Volt[1, 1] + Duty.Green * IntensityVoltage.Int2Volt[1, 2] + IntensityVoltage.Int2Volt[1, 3]);
            if (Duty.Green >= 1) {
                AmpLEDs.Green = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Green <= 0) {
                AmpLEDs.Green = 0;
                EndFlg[VectorDirection] = 1;
            }


            //Blue
            BgnDuty.Blue = ConBlue;
            //------------------------------------------------------------
            if (AmpLEDs.Blue >= 0) {
                AmpLEDs1.Blue = AmpLEDs.Blue; //plus x plus
            }
            else
            {
                AmpLEDs1.Blue = AmpLEDs.Blue; //minus x plus
            }
            Duty.Blue = BgnDuty.Blue + AmpLEDs1.Blue;
            Duty.Blue = Duty.Blue * (Duty.Blue * Duty.Blue * Duty.Blue * IntensityVoltage.Int2Volt[2, 0] + Duty.Blue * Duty.Blue * IntensityVoltage.Int2Volt[2, 1] + Duty.Blue * IntensityVoltage.Int2Volt[2, 2] + IntensityVoltage.Int2Volt[2, 3]);
            if (Duty.Blue >= 1) {
                AmpLEDs.Blue = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Blue <= 0) {
                AmpLEDs.Blue = 0;
                EndFlg[VectorDirection] = 1;
            }
            //
            if (AmpLEDs.Blue >= 0) {
                AmpLEDs1.Blue = AmpLEDs.Blue; //plus x minus
            }
            else
            {
                AmpLEDs1.Blue = AmpLEDs.Blue; //minus x minus
            }
            Duty.Blue = BgnDuty.Blue - AmpLEDs1.Blue;
            Duty.Blue = Duty.Blue * (Duty.Blue * Duty.Blue * Duty.Blue * IntensityVoltage.Int2Volt[2, 0] + Duty.Blue * Duty.Blue * IntensityVoltage.Int2Volt[2, 1] + Duty.Blue * IntensityVoltage.Int2Volt[2, 2] + IntensityVoltage.Int2Volt[2, 3]);
            if (Duty.Blue >= 1) {
                AmpLEDs.Blue = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Blue <= 0) {
                AmpLEDs.Blue = 0;
                EndFlg[VectorDirection] = 1;
            }

            //Orange
            BgnDuty.Orange = ConOrange;
            //------------------------------------------------------------
            if (AmpLEDs.Orange >= 0) {
                AmpLEDs1.Orange = AmpLEDs.Orange; //plus x plus
            }
            else
            {
                AmpLEDs1.Orange = AmpLEDs.Orange; //minus x plus
            }
            Duty.Orange = BgnDuty.Orange + AmpLEDs1.Orange;
            Duty.Orange = Duty.Orange * (Duty.Orange * Duty.Orange * Duty.Orange * IntensityVoltage.Int2Volt[3, 0] + Duty.Orange * Duty.Orange * IntensityVoltage.Int2Volt[3, 1] + Duty.Orange * IntensityVoltage.Int2Volt[3, 2] + IntensityVoltage.Int2Volt[3, 3]);
            if (Duty.Orange >= 1) {
                AmpLEDs.Orange = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Orange <= 0) {
                AmpLEDs.Orange = 0;
                EndFlg[VectorDirection] = 1;
            }
            //
            if (AmpLEDs.Orange >= 0) {
                AmpLEDs1.Orange = AmpLEDs.Orange; //plus x minus
            }
            else
            {
                AmpLEDs1.Orange = AmpLEDs.Orange; //minus x minus
            }
            Duty.Orange = BgnDuty.Orange - AmpLEDs1.Orange;
            Duty.Orange = Duty.Orange * (Duty.Orange * Duty.Orange * Duty.Orange * IntensityVoltage.Int2Volt[3, 0] + Duty.Orange * Duty.Orange * IntensityVoltage.Int2Volt[3, 1] + Duty.Orange * IntensityVoltage.Int2Volt[3, 2] + IntensityVoltage.Int2Volt[3, 3]);
            if (Duty.Orange >= 1) {
                AmpLEDs.Orange = 0;
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Orange <= 0) {
                AmpLEDs.Orange = 0;
                EndFlg[VectorDirection] = 1;
            }

        }

        void Cone2PhosAmpPhase( Receptors AmpReceptors,ref RGB2 AmpLEDs, Receptors PhaseReceptor,ref RGB2 PhaseLEDs)
        {

            RGB2 AmpLEDsCos = new RGB2();
            RGB2 AmpLEDsSin = new RGB2();
            // Cos component
            AmpLEDsCos.Red =
                AmpReceptors.L * C2PArray[0, 0] * Math.Cos(PhaseReceptor.L)
                + AmpReceptors.M * C2PArray[0, 1] * Math.Cos(PhaseReceptor.M)
                + AmpReceptors.S * C2PArray[0, 2] * Math.Cos(PhaseReceptor.S)
                + AmpReceptors.ipRGC * C2PArray[0, 3] * Math.Cos(PhaseReceptor.ipRGC);

            AmpLEDsCos.Green =
                    AmpReceptors.L * C2PArray[1, 0] * Math.Cos(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[1, 1] * Math.Cos(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[1, 2] * Math.Cos(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[1, 3] * Math.Cos(PhaseReceptor.ipRGC);
            AmpLEDsCos.Blue =
                    AmpReceptors.L * C2PArray[2, 0] * Math.Cos(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[2, 1] * Math.Cos(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[2, 2] * Math.Cos(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[2, 3] * Math.Cos(PhaseReceptor.ipRGC);

            AmpLEDsCos.Orange =
                    AmpReceptors.L * C2PArray[3, 0] * Math.Cos(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[3, 1] * Math.Cos(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[3, 2] * Math.Cos(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[3, 3] * Math.Cos(PhaseReceptor.ipRGC);

            // Sin component
            AmpLEDsSin.Red =
                    AmpReceptors.L * C2PArray[0, 0] * Math.Sin(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[0, 1] * Math.Sin(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[0, 2] * Math.Sin(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[0, 3] * Math.Sin(PhaseReceptor.ipRGC);

            AmpLEDsSin.Green =
                    AmpReceptors.L * C2PArray[1, 0] * Math.Sin(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[1, 1] * Math.Sin(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[1, 2] * Math.Sin(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[1, 3] * Math.Sin(PhaseReceptor.ipRGC);
            AmpLEDsSin.Blue =
                    AmpReceptors.L * C2PArray[2, 0] * Math.Sin(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[2, 1] * Math.Sin(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[2, 2] * Math.Sin(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[2, 3] * Math.Sin(PhaseReceptor.ipRGC);

            AmpLEDsSin.Orange =
                    AmpReceptors.L * C2PArray[3, 0] * Math.Sin(PhaseReceptor.L)
                    + AmpReceptors.M * C2PArray[3, 1] * Math.Sin(PhaseReceptor.M)
                    + AmpReceptors.S * C2PArray[3, 2] * Math.Sin(PhaseReceptor.S)
                    + AmpReceptors.ipRGC * C2PArray[3, 3] * Math.Sin(PhaseReceptor.ipRGC);

            AmpLEDs.Red = Math.Sqrt(Math.Pow(AmpLEDsCos.Red, 2.0) + Math.Pow(AmpLEDsSin.Red, 2.0));
            PhaseLEDs.Red = Math.Atan2(AmpLEDsSin.Red, AmpLEDsCos.Red);
            AmpLEDs.Green = Math.Sqrt(Math.Pow(AmpLEDsCos.Green, 2.0) + Math.Pow(AmpLEDsSin.Green, 2.0));
            PhaseLEDs.Green = Math.Atan2(AmpLEDsSin.Green, AmpLEDsCos.Green);
            AmpLEDs.Blue = Math.Sqrt(Math.Pow(AmpLEDsCos.Blue, 2.0) + Math.Pow(AmpLEDsSin.Blue, 2.0));
            PhaseLEDs.Blue = Math.Atan2(AmpLEDsSin.Blue, AmpLEDsCos.Blue);
            AmpLEDs.Orange = Math.Sqrt(Math.Pow(AmpLEDsCos.Orange, 2.0) + Math.Pow(AmpLEDsSin.Orange, 2.0));
            PhaseLEDs.Orange = Math.Atan2(AmpLEDsSin.Orange, AmpLEDsCos.Orange);


            if (AmpLEDs.Red >= 1) {
                AmpLEDs.Red = 1;
                EndFlg[VectorDirection] = 1;
            }
            if (AmpLEDs.Orange >= 1) {
                AmpLEDs.Orange = 1;
                EndFlg[VectorDirection] = 1;
            }
            if (AmpLEDs.Green >= 1) {
                AmpLEDs.Green = 1;
                EndFlg[VectorDirection] = 1;
            }
            if (AmpLEDs.Blue >= 1) {
                AmpLEDs.Blue = 1;
                EndFlg[VectorDirection] = 1;
            }
        }
        //---------------------------------------------------------------------------
        void CalcNextCntrst(uint ContrastLevel)
        {
            double angle, amp, OmegaT;
            //angleX   = 108.82; // isoluminant axis in L,M plane
            //VectorDirection=11;
            //ContrastLevel=32;
            angle = PhaseAngle[VectorDirection];
            SF = SFs[VectorDirection];
            amp = 0.0005 * Math.Pow(1.258925, (double)ContrastLevel);
            OmegaT = 2.0 * Math.PI * angle / 360.0;
            //amp = 0.2;

            Contrast.L = amp* Math.Cos(OmegaT);
            Contrast.M = amp* Math.Sin(OmegaT);
            Contrast.S = 0.0;
            Contrast.ipRGC = 0.0;
            //Contrast.L = amp * Math.Cos(angleX / 180 * PI) * Math.Cos(OmegaT);//amp*cos(angleX/180*PI)*cos(OmegaT);
            //Contrast.M = amp * Math.Sin(angleX / 180 * PI) * Math.Cos(OmegaT);//amp*sin(angleX/180*PI)*cos(OmegaT);
            //Contrast.S = 0.0;//*cos(OmegaT);//amp*sin(OmegaT);
            //Contrast.ipRGC = amp * Math.Sin(OmegaT);
            //

        }

        //---------------------------------------------------------------------------

        void dswap(ref double a,ref double b)
        {
            double t;
            t = a;
            a = b;
            b = t;
        }
        int inv_mat1(ref double[,] a, double det, double eps)
        {
            double w, piv;
            int[] index = new int[N];
            int i, j, k, m, n, ipv, idx;
            m = 4; n = 4;
            det = 1.0;
            for (k = 0; k < m; k++)
            {
                piv = eps;
                ipv = -1;
                for (i = k; i < m; i++)
                {
                    if (Math.Abs(piv) < Math.Abs(a[i, k]))
                    {
                        piv = a[i, k]; ipv = i;
                    }
                }
                index[k] = ipv;
                if (ipv < 0) return 1;
                if (ipv != k)
                {
                    for (j = 0; j < n; j++) dswap(ref a[ipv, j], ref a[k, j]);
                    det = -(det);
                }
                det *= piv;
                a[k, k] = 1.0;
                for (j = 0; j < n; j++) a[k, j] /= piv;
                for (i = 0; i < m; i++)
                {
                    if (i != k)
                    {
                        w = a[i, k]; a[i, k] = 0.0;
                        for (j = 0; j < n; j++)
                            a[i, j] -= a[k, j] * w;
                    }
                }
            }
            for (j = 0; j < m - 1; j++)
            {
                k = m - j - 2; idx = index[k];
                if (idx != k)
                {
                    for (i = 0; i < m; i++)
                        dswap(ref a[i, idx],ref a[i, k]);
                }
            }
            return 0;
        }
        //---------------------------------------------------------------------------
        public void Initialize()
        {   for(int z = 0; z < 12; z++)
            {
                Record[z] = new RECORD();
            }
            int indx, i;
            for (int z = 0; z < 24; z++) { order[z] = z; }
            indx = RandomStimulation.RandomOrder[OrderCntr];
            if (indx >= 24)
            {
                Console.Write("Error occurred in making a random sequence.");
                System.Environment.Exit(0);
            }
            Direction = order[indx] & 0x01;
            for (i = 0; i < 12; i++)
            {
                ContrastLevel[i] = (uint)InitialCntrstLvl;
            }
            ContrastLevel[0] = 22; //30;  //0deg
            ContrastLevel[1] = 22; //32;  //30deg
            ContrastLevel[2] = 22; //34;  //45deg
            ContrastLevel[3] = 22; //32;  //60deg
            ContrastLevel[4] = 22; //30;  //90deg
            ContrastLevel[5] = 22; //30;  //120deg

            ContrastLevel[6] = 20; //30;  //135deg
            ContrastLevel[7] = 20; //30;  //150deg
            ContrastLevel[8] = 20; //30;  //180deg
            ContrastLevel[9] = 20; //32;  //210deg
            ContrastLevel[10] = 20; //34;  //225deg
            ContrastLevel[11] = 20; //32; //240deg

            PhaseReceptors.L = 0.0 / 180 * PI;       // Initialise Phases for receptors
            PhaseReceptors.M = 0.0 / 180 * PI;
            PhaseReceptors.S = 0.0 / 180 * PI;
            PhaseReceptors.ipRGC = 0.0 / 180 * PI;

            //fprintf(fp, "%s %s %4.2lf[deg] %4.2lf[deg] %4.2lf %4.2lf %4.2lf %4.2lf\n", _argv[0], DateTimeToStr(Now()), angleX, angleX, PhaseReceptors.L, PhaseReceptors.M, PhaseReceptors.S, PhaseReceptors.ipRGC);


            //
            // Calc inverse function of  P2CArray
            double det;
            int j;
            det = P2CArray.p2cArray[2, 3];
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    C2PArray[i, j] = P2CArray.p2cArray[i, j];
                }
            }
            if (0 != inv_mat1(ref C2PArray, det, 0.000001))
            {
                Console.Write("Error occurred in Initialise() or inv_mat1().");
                System.Environment.Exit(0);
            }

            indx = RandomStimulation.RandomOrder[OrderCntr];
            if (indx >= 24)
            {
                Console.Write("Error occurred in making a random sequence.");
                System.Environment.Exit(0);
            }
            Direction = order[indx] & 0x01;
            VectorDirection = (order[indx] >> 1);
            CalcNextCntrst(ContrastLevel[VectorDirection]);
            for (i = 0; i < 12; i++)
            {
                Record[i].first = Record[i].second = Record[i].third = 1;
            }
        }
        //---------------------------------------------------------------------------

        void SerialSend()
        {
            ////////////////////////////////読み出し系
            System.Media.SystemSounds.Beep.Play();
            int i;
            //char[] tmp_buff = new char[1000];
            //char[] rcveBuff = new char[10];
            //HDC hdc;
            //TCHAR buf[128];

            // FollowingTransCal[][] should be less than 1.0 to round off 5 decimals.
            for (i = 0; i < 32; i++)
            {
                s_Buf[i] = 0.0f;
            }
            // LED0
            s_Buf[0] = (float)meanL; // mean for LED0
            s_Buf[1] = (float)Contrast.L; // contrast for LED0
            s_Buf[2] = (float)IntensityVoltage.Int2Volt[0, 0]; // Gamma x^4
            s_Buf[3] = (float)IntensityVoltage.Int2Volt[0, 1]; // Gamma x^3
            s_Buf[4] = (float)IntensityVoltage.Int2Volt[0, 2]; // Gamma x^2
            s_Buf[5] = (float)IntensityVoltage.Int2Volt[0, 3]; // Gamma x^1
            s_Buf[6] = (float)phaseL; //Phase in radian
                                                  // LED1
            s_Buf[7] = (float)meanM; // mean for LED2
            s_Buf[8] = (float)Contrast.M; // contrast for LED2
            s_Buf[9] = (float)IntensityVoltage.Int2Volt[1, 0]; // Gamma x^4
            s_Buf[10] = (float)IntensityVoltage.Int2Volt[1, 1]; // Gamma x^3
            s_Buf[11] = (float)IntensityVoltage.Int2Volt[1, 2]; // Gamma x^2
            s_Buf[12] = (float)IntensityVoltage.Int2Volt[1, 3]; // Gamma x^1
            s_Buf[13] = (float)phaseM; //Phase in radian
                                                      // LED2
            s_Buf[14] = (float)meanS; // mean for LED3
            s_Buf[15] = (float)Contrast.S; // contrast for LED3
            s_Buf[16] = (float)IntensityVoltage.Int2Volt[2, 0]; // Gamma x^4
            s_Buf[17] = (float)IntensityVoltage.Int2Volt[2, 1]; // Gamma x^3
            s_Buf[18] = (float)IntensityVoltage.Int2Volt[2, 2]; // Gamma x^2
            s_Buf[19] = (float)IntensityVoltage.Int2Volt[2, 3]; // Gamma x^1
            s_Buf[20] = (float)phaseS; //Phase in radian
                                                    // LED3
            s_Buf[21] = (float)meanipRGC; // mean for LED1
            s_Buf[22] = (float)Contrast.ipRGC; // contrast for LED1
            s_Buf[23] = (float)IntensityVoltage.Int2Volt[3, 0];  //Gamma x^4
            s_Buf[24] = (float)IntensityVoltage.Int2Volt[3, 1];  //Gamma x^3
            s_Buf[25] = (float)IntensityVoltage.Int2Volt[3, 2]; // Gamma x^2
            s_Buf[26] = (float)IntensityVoltage.Int2Volt[3, 3]; // Gamma x^1
            s_Buf[27] = (float)phaseipRGC; //Phase in radian

            // for all LEDs
            /*
            s_Buf[28] = 0.00500f; // TF (x100 Hz)
            s_Buf[29] = 0.04000f; // Duration of Test stimulus in  x100 sec
            s_Buf[30] = 0.01000f; // Duration of background stimulus in  x100 sec
            s_Buf[31] = 0.00052f; //wait 52
            */
            // create array to create sequence
            double[] mean = new double[4] { s_Buf[0], s_Buf[7], s_Buf[14], s_Buf[21] };
            double[] contrast = new double[4] {s_Buf[1], s_Buf[8], s_Buf[15], s_Buf[22]};
            double[] phase = new double[4] { s_Buf[6], s_Buf[13], s_Buf[20], s_Buf[27] };
            
            // call the method to generate PWM value and send it to the microcontroller
            // the temporal frequency is written in Hz and the duration in millisecond
            //content = ContentGenerator.GenerateSinusStimulation(mean, contrast, phase);
            content = ContentGenerator.GenerateSineStimulationWithoutHamming(mean, contrast, phase);
            //content = ContentGenerator.GenerateSineStimulationForMeasurement(mean, contrast, phase);

            // Stop everything the cube was doing
            //_vcp.SendJsonContent("{}");
                //_vcp.SendJsonContent("{\"StopAllSequences\" : 0}");

                // Send the new configuration
                _vcp.SendJsonContent(JsonConvert.SerializeObject(content));

                // Start only the used channels
                foreach (Sequence seq in content.Sequences)
                {
                    _vcp.SendJsonContent("{\"StartSequence\" : " + seq.LedIndex + "}");
                }
                //log.Write("Contrast Value => R : " + Convert.ToString(contrast[0]) + " | G : " + Convert.ToString(contrast[1]) + " | B : " + Convert.ToString(contrast[2]) + " | Y : " + Convert.ToString(contrast[3]));
                //log.Write("Phase Value => R : " + Convert.ToString(phase[0]) + " | G : " + Convert.ToString(phase[1]) + " | B : " + Convert.ToString(phase[2]) + " | Y : " + Convert.ToString(phase[3]));



            /*
            long sum = 0;
            String convert = "";
            for (i = 0; i < 32; i++)
            {
                convert += string.Format("{0:000000}", (s_Buf[i] * 100000)) + ",";
                Console.WriteLine(convert);
                //sprintf(tmp_buff, "%6.0f,", s_Buf[i] * 100000);
                if (s_Buf[i] > 0)
                {
                    sum += (int)(s_Buf[i] * 100000 + 0.5);
                }
                else
                {
                    sum += (int)(s_Buf[i] * 100000 - 0.5);
                }
                //strcat(str, tmp_buff);
            }
            CheckSum = (int)(sum & 0xff);
            */
            //Form1.Label2.Caption = IntToStr(CheckSum);
            /*
            hdc = GetDC(hwnd);
            wsprintf(buf, TEXT("CheckSum:%d"), CheckSum);
            TextOut(hdc, 20, 45, buf, lstrlen(buf));
            ReleaseDC(hwnd, hdc);
            */
            //sprintf(tmp_buff,"%1d,",Direction);
            /*
            sprintf(tmp_buff, "%3d,", CheckSum);  //check sum
            strcat(str, tmp_buff);
            strcat(str, "e");
            */
            /*
            convert += string.Format("{0:000}", CheckSum) + ",";
            convert += 'e';
            _vcp.SendJsonContent(convert);
            */
            //int response = _vcp.read(rcveBuff, 0, 1);
            //Console.WriteLine(response);
            //	fprintf(fp, "%s\n", str);
            //	Sleep(5000);
            //WriteFile(hComm, str, strlen(str), &writesize, NULL);
            /* シリアルポートに書き込み */
        }
        //---------------------------------------------------------------------------
        /*
        int SerialReceive()
        {
            static char rdBuf[256];
            int i;
            HDC hdc;
            TCHAR buf[256];

            for (i = 0; i < 256; i++)
            {
                rdBuf[i] = 0;
            }

            hdc = GetDC(hwnd);
            wsprintf(buf, TEXT("rdBuf:%s"), (char *)rdBuf);
            TextOut(hdc, 20, 70, buf, lstrlen(buf));
            ReleaseDC(hwnd,hdc);
            return 1;


            i = 0;
            do
            {
                Sleep(20);
                ClearCommError(hComm, &dwErrors, &ComStat);
                dwCount = ComStat.cbInQue;
                i++;
            } while (dwCount == 0);

            ReadFile(hComm, rdBuf, dwCount, &dwRead, NULL);
            int NumberofData;
            //Form1.Label1.Caption = (char *)rdBuf;
            hdc = GetDC(hwnd);
            wsprintf(buf, TEXT("rdBuf:%s"), (char*)rdBuf);
            TextOut(hdc, 20, 70, buf, lstrlen(buf));
            ReleaseDC(hwnd, hdc);
            //    Sleep(100);
            //Application.ProcessMessages();
            NumberofData = atoi(rdBuf);

            return (NumberofData);
        }*/
        /*
        void SerialOpen()
        {
            hComm = CreateFile(
                SerialPortNumber,             // シリアルポートの指定 
                GENERIC_READ | GENERIC_WRITE, // アクセスモード 
                0,                            // 共有モード 
                NULL,                         // セキュリティ属性 
                OPEN_EXISTING,                // 作成フラグ 
                FILE_ATTRIBUTE_NORMAL,        // 属性 
                NULL                          // テンプレートのハンドル 
                );

            GetCommState(hComm, &dcb); // DCB を取得 
            dcb.BaudRate = Brate;
            dcb.ByteSize = 8;
            dcb.Parity = NOPARITY;
            dcb.StopBits = ONESTOPBIT;
            SetCommState(hComm, &dcb); // DCB を設定 
        }

        void SerialClose()
        {
            CloseHandle(hComm);
            } */
        //---------------------------------------------------------------------------
        void MakeLUTs()
        {
            //short i,j;
            RGB2 PhosLumi1 = new RGB2();
            Receptors ReceptorOut = new Receptors();
            Receptors StmlCones = new Receptors();

            /* obtain mean(BG) luminance */
            PhosLumi1.Red = ConRed;
            PhosLumi1.Green = ConGreen;
            PhosLumi1.Blue = ConBlue;
            PhosLumi1.Orange = ConOrange;

            /* derive mean luminance of each cone */
            Phos2Cone(ref PhosLumi1,ref ReceptorOut);


            StmlCones.L = ReceptorOut.L * Contrast.L;
            StmlCones.M = ReceptorOut.M * Contrast.M;
            StmlCones.S = ReceptorOut.S * Contrast.S;
            StmlCones.ipRGC = ReceptorOut.ipRGC * Contrast.ipRGC;
            //Cone2PhosAmp(StmlCones, ref AmpLEDs);  //add 24Nov2011
            Cone2PhosAmpPhase(StmlCones, ref AmpLEDs, PhaseReceptors, ref PhaseLEDs);  //add 25Apr2015 ST
            CalcDuties(ref AmpLEDs);

            //    i=0;

            // Output process
            //if (FirstFlg == 1){

            int NumberofData;
            int counters = 0;
            //static int tmp[100];
            SerialSend();
            //Sleep(20);
            //NumberofData = SerialReceive();
            // MessageBox(0,"finished","Thanks",MB_OK);
            //      exit(0);

            /*
            while (NumberofData != 1){
                //Sleep(10);
                SerialSend();
                Sleep(20);
                NumberofData = SerialReceive();
                tmp[counters] = NumberofData;
                counters++;
                if (counters>50){
                    MessageBox(0, TEXT("Thank you!!"), TEXT("Thanks"), MB_OK);
                    exit(0);
                }
            }*/
            //SerialClose();
            StimulusPresentationCounter++;
            //  MessageBox(0,"Thank you!","Thanks",MB_OK);
            //        exit(0);
            //}
        }
        //---------------------------------------------------------------------------
        /*
        void ExperimentStart(void)
        {
            System::Windows::Forms::OpenFileDialog ^ OpenDialog1 = gcnew System::Windows::Forms::OpenFileDialog();

            MessageBox(0, TEXT("This is a program modified based on YN_LMS_ipRGC_10Hz_1s_18Mar2015 that can vary a phase shift among receptors."), TEXT("Notice!"), MB_OK);
            OpenDialog1.Filter = "*.rst|*.rst";
            OpenDialog1.CheckFileExists = false;
            if (OpenDialog1.ShowDialog() == System::Windows::Forms::DialogResult::OK)
            {
                try
                {
                    //convert System::String^ to char*
                    char* pStr = (char*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(OpenDialog1.FileName).ToPointer();
                    //fnsplit(OpenDialog1.FileName,0,0,filename,0);
                    //strcat(filename, ".rst");
                    if (NULL == (fp = fopen(pStr, "a")))
                    {
                        MessageBox(0, TEXT("Error occurred at fopen."), TEXT("Error"), MB_OK);
                        exit(0);
                    }
                    System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr(pStr));

                }
                catch (...) {
                    MessageBox(0, TEXT("Error occurred at OpenDialog."), TEXT("Error"), MB_OK);
                    exit(0);
                }
                }
            else {
                    MessageBox(0, TEXT("Cancel the execution."), TEXT("Error"), MB_OK);
                    exit(0);
                }
            }
            */
        /*void ExperimentStart(void)
        {
            System::Windows::Forms::OpenFileDialog^ OpenDialog1 = gcnew System::Windows::Forms::OpenFileDialog();

            MessageBox(0, TEXT("This is a program modified based on YN_LMS_ipRGC_10Hz_1s_18Mar2015 that can vary a phase shift among receptors."), TEXT("Notice!"), MB_OK);
            OpenDialog1.Filter = "*.rst|*.rst";
            OpenDialog1.CheckFileExists = false;
            if (OpenDialog1.ShowDialog() == System::Windows::Forms::DialogResult::OK){
                try{
                    //convert System::String^ to char*
                    char* pStr = (char*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(OpenDialog1.FileName).ToPointer();
                    //fnsplit(OpenDialog1.FileName,0,0,filename,0);
                    //strcat(filename, ".rst");
                    if (NULL == (fp = fopen(pStr, "a"))){
                        MessageBox(0, TEXT("Error occurred at fopen."), TEXT("Error"), MB_OK);
                        exit(0);
                    }
                    System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr(pStr));

                }
                catch (...) {
                    MessageBox(0, TEXT("Error occurred at OpenDialog."), TEXT("Error"), MB_OK);
                    exit(0);
                }
            }
            else {
                MessageBox(0, TEXT("Cancel the execution."), TEXT("Error"), MB_OK);
                exit(0);
            }
        }*/

        /*void TestVSG(void)
        {
            MakeLUTs();
        }*/
        //---------------------------------------------------------------------------

        //void __fastcall TForm1::FormCreate(TObject *Sender)
        //{
        //	char filename_rcrd[MAXFILE + 4];
        //
        //	MessageBox(0, "This is a program modified based on YN_LMS_ipRGC_10Hz_1s_18Mar2015 that can vary a phase shift among receptors.", "Notice!", MB_OK);
        //	if (OpenDialog1.Execute()){
        //		try{
        //			fnsplit(OpenDialog1.FileName.c_str(), 0, 0, filename, 0);
        //			strcpy(filename_rcrd, filename);
        //			strcat(filename_rcrd, ".rst");
        //			if (NULL == (fp = fopen(filename_rcrd, "a"))){
        //				MessageBox(0, "Error occurred at fopen.", "Error", MB_OK);
        //				exit(0);
        //			}
        //			Initialize();
        //			TestVSG();
        //		}
        //		catch (...){
        //			MessageBox(0, "Error occurred at OpenDialog.", "Error", MB_OK);
        //			exit(0);
        //		}
        //	}
        //	else{
        //		MessageBox(0, "Cancel the execution.", "Error", MB_OK);
        //		exit(0);
        //	}
        //}

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        void UpDown71()
        {
            //	Record[VectorDirection].third=Record[VectorDirection].second;
            Record[VectorDirection].second = Record[VectorDirection].first;
            if (RightLeft == 0)
            {  // For Change Detection
                Record[VectorDirection].first = 1;
            }
            else
            {
                Record[VectorDirection].first = 0;
            }
            if ((Record[VectorDirection].first == 1) && (Record[VectorDirection].second == 1))
            {
                if (ContrastLevel[VectorDirection] > 0)
                {
                    Record[VectorDirection].first = Record[VectorDirection].second = 0;
                    ContrastLevel[VectorDirection]--;
                }

            }
            else
            {
                if (Record[VectorDirection].first == 0)
                {
                    //			if(ContrastLevel[VectorDirection]<25){
                    ContrastLevel[VectorDirection]++;
                    //			}
                }
            }
        }
        void mouse(int rl)
        {
            int indx;
            int TerminationFlg = 1, i;
            //HDC hdc;
            //TCHAR buf[128];
            //MSG msg;
            //PAINTSTRUCT ps;

            //if (MouseFlg == 1) return;
            //else MouseFlg = 1;
            Console.WriteLine("Mouse");

            if (FirstFlg == 1)
            {
                RightLeft = rl;
                //fprintf(fp, "%4d %4d %4d %4d\n", OrderCntr, VectorDirection, ContrastLevel[VectorDirection], RightLeft);
                ChkConv((uint)VectorDirection, ContrastLevel[VectorDirection]);
                //fflush(fp);
                //Application.ProcessMessages();
                result.Write(OrderCntr.ToString() + " " + ContrastLevel[VectorDirection].ToString() + " " + VectorDirection.ToString() + " " + Direction.ToString() + " " + RightLeft.ToString());
                UpDown71();
                do
                {
                    Console.WriteLine("OrderCntr: " + OrderCntr);
                    OrderCntr++;
                    if (OrderCntr >= 1440)
                    {
                        for (i = 0; i < 12; i++)
                        {
                            TerminationFlg *= (int)EndFlg[i];
                        }
                        if (TerminationFlg == 1)
                        {
                            //MessageBeep(MB_ICONQUESTION);
                            //MessageBox(0, TEXT("Program normally terminated."), TEXT("OK"), MB_OK);
                            Console.WriteLine("Program normally terminated.");
                        }
                        else
                        {
                            //MessageBeep(MB_ICONQUESTION);
                            //MessageBox(0, TEXT("more than 1440"), TEXT("Error"), MB_OK);
                            Console.WriteLine("more than 1440");
                        }
                        System.Environment.Exit(0);
                    }
                    indx = RandomStimulation.RandomOrder[OrderCntr];
                    if (indx >= 24)
                    {
                        //MessageBox(0, TEXT("Error occurred in making a random sequence."), TEXT("Error"), MB_OK);
                        //exit(0);
                        System.Environment.Exit(0);
                    }
                    //Direction = order[indx] & 0x01;  //0:1st 1:2nd
                    Console.WriteLine("order: " + order[indx]);
                    VectorDirection = (order[indx] >> 1);
                    Console.WriteLine("VectorDirection: " + VectorDirection);
                    CalcNextCntrst(ContrastLevel[VectorDirection]);
                } while (EndFlg[VectorDirection] != 0);

            }
            //
            //Canvas.TextRect(TheRect, 20, 20, StimulusPresentationCounter);
            //InvalidateRect(hwnd, NULL, TRUE);
            //hdc = BeginPaint(hwnd, &ps);
            //wsprintf(buf, TEXT("[%d]"), StimulusPresentationCounter);
            //TextOut(hdc, 20, 20, buf, lstrlen(buf));
            //EndPaint(hwnd, &ps);

            //
            //Application.ProcessMessages();
            //
            FirstFlg = 1;
            MakeLUTs();
            //Application.ProcessMessages();

            //
           // MessageBeep(MB_ICONEXCLAMATION);
            // キューを削除
            //while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) ;
            //MouseFlg = 0;
        }
        //---------------------------------------------------------------------------

        //---------------------------------------------------------------------------
        void ChkConv(uint VectorDirection, uint ContrastLevel)
        {
            int[,] RsltUpDown = new int[12,200];
            int[,] RsltUpDownDiff = new int[12, 200];
            int[,] RsltUpDownDiff2 = new int[12,200];
            uint[] NumberOfData = new uint[12];
            uint[] NumberOfData2 = new uint[12];
            uint[] NumberOfDiff = new uint[12];
            uint indx1, indx2, indx3;
            int DiffSign;

            indx1 = VectorDirection;
	        indx2 = NumberOfData[indx1];
	        RsltUpDown[indx1,indx2] = (int)ContrastLevel;
	        //
	        if (indx2>1){
		        RsltUpDownDiff[indx1,indx2] = RsltUpDown[indx1,indx2] - RsltUpDown[indx1,indx2 - 1];

		        if (RsltUpDownDiff[indx1,indx2] != 0){
			        indx3 = NumberOfData2[indx1];
			        RsltUpDownDiff2[indx1,indx3] = RsltUpDownDiff[indx1,indx2];
    			    NumberOfData2[indx1]++;

	    		    if (indx3>0){
		    		    DiffSign = RsltUpDownDiff2[indx1,indx3] * RsltUpDownDiff2[indx1,indx3 - 1];

			    	    if (DiffSign == -1){// Reversed Point.
				    	    NumberOfDiff[indx1]++;
					        if (NumberOfDiff[indx1]>NumberOfReverse){
						        EndFlg[indx1] = 1;
                            }
                        }
				    }
			    }
		    }
	    NumberOfData[indx1]++;
	    }
    }
}


