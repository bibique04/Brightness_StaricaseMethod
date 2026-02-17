#define TestY

using Newtonsoft.Json;
using System;
using System.IO;
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
 
        // PHYSICAL CONSTANTS
        static double PI = (4.0 * Math.Atan(1.0));
        static int N = 140; // only used in invmat1
        static int InitialCntrstLvl = 33; // increased to 35 from 23 for better visibility
        static int NumberOfReverse = 2; // the number of reversals in the staircase method

#if TestY  // based on ST_Ganzfeld_RContrastfile_07Sep2019.xls  COMPILED ONLY IN DEBUG
        // BASELINE INTENSITIES FOR THE 4 LEDS
        static double ConRed = 0.0382766; //r 
        static double ConGreen = 0.0368295; //g
        static double ConBlue = 0.0499570; //b
        static double ConOrange = 0.0378037; //y
        static double meanL = 20.634;
        static double meanM = 7.590;
        static double meanS = 13.222;
        static double meanipRGC = 17.444;
        static double phaseL = 0;
        static double phaseM = 0;
        static double phaseS = 0;
        static double phaseipRGC = 0;
#endif

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_RBUTTONDOWN = 0x0204;

        //-----------------------------------------------------------
        // END TESTVCG.h
        //-----------------------------------------------------------

        //--------------------------------------------------
        // GLOBAL ATTRIBUTES C file
        //--------------------------------------------------

        // FOR THE ChkConv method
        int[,] RsltUpDown = new int[12, 200]; // contains the ContrastLevels after every response
        int[,] RsltUpDownDiff = new int[12, 200]; // the differences between the current and the previous brightness levels
        int[,] RsltUpDownDiff2 = new int[12, 200]; // retains the contrast differences between the trials
        uint[] NumberOfData = new uint[12]; // the number of trials completed for each experiment condition
        uint[] NumberOfData2 = new uint[12]; // number of changes
        uint[] NumberOfDiff = new uint[12]; // the number of times the staircase flipped
        double currentStaircaseAmp = 1.0;

        int RightLeft, Direction = 1;
        int VectorDirection = 0;
        double[,] C2PArray = new double[4, 4];
        uint OrderCntr;
        uint[] ContrastLevel = new uint[12];
        uint[] EndFlg = new uint[12] { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; //(0,3)(1,4)(2,5)
        RECORD[] Record = new RECORD[12]; // three objects: first, second, third
        Receptors Contrast = new Receptors(); // L, M, S, ipRGCs
        double[] PhaseAngle = new double[12] { 0, 30, 60, 90, 120, 150, 180, 210, 225, 240, 45, 135 };
        double[] SFs = new double[12] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        RGB2 AmpLEDs = new RGB2(); // Red, Green, Blue, Orange
        int[] order = new int[24];

        static uint FirstFlg = 0;

        Receptors PhaseReceptors = new Receptors();
        RGB2 PhaseLEDs = new RGB2();

        //----------------------------added 28Dec2007
        float[] s_Buf = new float[100];

        /*
        HANDLE hComm;       // シリアルポートのハンドル  Serial port handle
        DCB dcb;            // 通信パラメータ  Communication parameters
        DWORD writesize;    // ポートへ書き込んだバイト数  Number of bytes written to the port
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

        VirtualComPort _vcp = new VirtualComPort(); // acts as a serial port - allows applications to communicate without real hardware
        JsonContent content = new JsonContent();
        Logger result;
        Logger log;
        int stimulationNumber = 1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;

        //--------------------------------------------------------
        // END GLOBAL ATTRIBUTES C#
        //--------------------------------------------------------

        private System.ComponentModel.IContainer components = null; // Required designer variables

        // Clean up all resources in use.
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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(146, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "0";
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(148, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "None";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "Number of trials done :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(150, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "0/12";
            // 
            // GanzeldController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
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
        
        protected override void WndProc(ref Message m)  // windows procedure, takes action based on the button pressed by the user

        /* Acts as a listener. It catches the Left Click or Right Click and passes that choice to the mouse method. */

        {
            this.label3.Text = stimulationNumber.ToString();
            switch (m.Msg)
            {
               
                case WM_LBUTTONDOWN:
                    this.label4.Text = "Left button clicked";
                    stimulationNumber++;
                    mouse(1);
                    break;

                case WM_RBUTTONDOWN:
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
                InitialDirectory = @"D:\master_japonia\tsujilab\Results",
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
        
        /* Sends a command to the device to turn on the baseline "Mean" lights */

        {
            // call the method to generate PWM value and send it to the microcontroller
            content = ContentGenerator.GenerateBackground(ConRed, ConGreen, ConBlue, ConOrange);
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
        
        void Phos2Cone(ref RGB2 PhosLumi, ref Receptors Receptor)
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

        /* Checks the Peak and Trough of the calculated wave.
         * 
         * If the wave is too tall (over 100%) or too deep (below 0%), it "clips" the value and warns the system that the hardware is at its limit. */

        {

            // Calibration Coefficients from IntensityVoltage.Int2Volt
            // Polynomial: y = ax^4 + bx^3 + cx^2 + dx

            double[] results = new double[4];
            double[] rawIntensities = {
                ConRed + AmpLEDs.Red,
                ConGreen + AmpLEDs.Green,
                ConBlue + AmpLEDs.Blue,
                ConOrange + AmpLEDs.Orange
            };

            for (int i = 0; i < 4; i++)
            {
                double x = rawIntensities[i];

                // 1. Physical Safety Clamp
                // If the intensity + staircase exceeds 1.0, we cap it at 0.99 
                // to prevent the hardware from "tripping" or turning off.
                if (x > 1.0) x = 0.99;
                if (x < 0.0) x = 0.001; // Keep a floor so it's never truly 'off'

                // 2. Apply 4th Order Polynomial (Calibration translation)
                // y = x * (x^3*a + x^2*b + x*c + d)
                results[i] = x * (Math.Pow(x, 3) * IntensityVoltage.Int2Volt[i, 0] +
                                  Math.Pow(x, 2) * IntensityVoltage.Int2Volt[i, 1] +
                                  x * IntensityVoltage.Int2Volt[i, 2] +
                                  IntensityVoltage.Int2Volt[i, 3]);
            }

            // Update the AmpLEDs with the final calibrated duties
            AmpLEDs.Red = results[0];
            AmpLEDs.Green = results[1];
            AmpLEDs.Blue = results[2];
            AmpLEDs.Orange = results[3];

            /*
            RGB2 BgnDuty = new RGB2();
            RGB2 Duty = new RGB2();
            RGB2 AmpLEDs1 = new RGB2();

            double[] results = new double[4];
            double[] rawIntensities = {
                ConRed + AmpLEDs.Red,
                ConGreen + AmpLEDs.Green,
                ConBlue + AmpLEDs.Blue,
                ConOrange + AmpLEDs.Orange
            };

            //Red
            BgnDuty.Red = ConRed;
            AmpLEDs1.Red = AmpLEDs.Red;

            // check if the wave is too "tall" for the ceiling (100% brightness)
            Duty.Red = BgnDuty.Red + AmpLEDs1.Red;
            Duty.Red = Duty.Red * (Duty.Red * Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 0] + Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 1] + Duty.Red * IntensityVoltage.Int2Volt[0, 2] + IntensityVoltage.Int2Volt[0, 3]);
            
            if (Duty.Red >= 1) {
                // AmpLEDs.Red = 0;
                Duty.Red = 0.99;
                // EndFlg[VectorDirection] = 1;
            }

            if (Duty.Red <= 0) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }

            AmpLEDs1.Red = AmpLEDs.Red;

            // check if the wave is too "deep" for the floor (0% brightness)
            Duty.Red = BgnDuty.Red - AmpLEDs1.Red;
            Duty.Red = Duty.Red * (Duty.Red * Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 0] + Duty.Red * Duty.Red * IntensityVoltage.Int2Volt[0, 1] + Duty.Red * IntensityVoltage.Int2Volt[0, 2] + IntensityVoltage.Int2Volt[0, 3]);
           
            if (Duty.Red >= 1) {
                Duty.Red = 
                EndFlg[VectorDirection] = 1;
            }
            if (Duty.Red <= 0) {
                AmpLEDs.Red = 0;
                EndFlg[VectorDirection] = 1;
            }

            //Green
            BgnDuty.Green = ConGreen;
            AmpLEDs1.Green = AmpLEDs.Green;
            
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

            AmpLEDs1.Green = AmpLEDs.Green;

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
            AmpLEDs1.Blue = AmpLEDs.Blue;

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

            AmpLEDs1.Blue = AmpLEDs.Blue;

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
            AmpLEDs1.Orange = AmpLEDs.Orange;
            
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

            AmpLEDs1.Orange = AmpLEDs.Orange;

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
            */

        }

        void Cone2PhosAmpPhase(Receptors AmpReceptors, ref RGB2 AmpLEDs, Receptors PhaseReceptor, ref RGB2 PhaseLEDs)

        /* Takes those cone targets and uses the 4x4 matrix math to figure out the Voltage and Phase for each of the 4 LEDs.
         * 
         * It calculates Phasors (using Atan2 and Sqrt) to ensure the four colors flicker in perfect harmony to create the desired visual effect. */

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

        /* Turns the level into a physical Amplitude using a logarithmic scale.
         * 
         * Sine/Cosine math to split that amplitude into specific targets for the L-cones and M-cones in the eye. */

        {
            currentStaircaseAmp = 0.0005 * Math.Pow(1.258925, (double)ContrastLevel); // 0.0005 * Math.Pow(1.258925, (double)ContrastLevel);
        }

        //---------------------------------------------------------------------------

        void dswap(ref double a,ref double b)
        {
            double t;
            t = a;
            a = b;
            b = t;
        }

        int inv_mat1(ref double[,] a, double det, double eps) // inverse matrix
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

        /*Sets up the 12 staircases (one for each phase angle).

        Creates a randomized order array so the participant can't predict which of the 12 colors appears next.

        Calculates the C2PArray, which is the "translator" matrix that tells the software how to mix the 4 physical LEDs to target specific human eye receptors.*/

        {   for(int z = 0; z < 12; z++)
            {
                Record[z] = new RECORD();
            }

            int indx, i;

            for (int z = 0; z < 24; z++) // 12 experiment conditions x 2 distinct temporal intervals
            { 
                order[z] = z; 
            }

            indx = RandomStimulation.RandomOrder[OrderCntr];
            
            if (indx >= 24)
            {
                Console.Write("Error occurred in making a random sequence.");
                System.Environment.Exit(0);
            }

            Direction = order[indx] & 0x01; // checks whether the number is odd or even to decide which flash comes first

            for (i = 0; i < 12; i++)
            {
                ContrastLevel[i] = (uint)InitialCntrstLvl;
            }

            /*
            ContrastLevel[0] = 35; //30;  //0deg
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
            */

            PhaseReceptors.L = 0.0 / 180 * PI;       // Initialise Phases for receptors
            PhaseReceptors.M = 0.0 / 180 * PI;
            PhaseReceptors.S = 0.0 / 180 * PI;
            PhaseReceptors.ipRGC = 0.0 / 180 * PI;

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

        /* Packages the Mean, Contrast, and Phase for all 4 LEDs into a JSON string.
         * 
         * Sends this string to the _vcp (Virtual Com Port), which physically tells the 4-LED device to flicker. */

        {
            System.Media.SystemSounds.Beep.Play();

            double[] adapt = { ConRed, ConGreen, ConBlue, ConOrange };

            // 1. Package the Test Stimulus into an RGB2 object
            // This uses your Direct LED scaling (Ratio * Staircase Amplitude)
            RGB2 currentAmps = new RGB2
            {
                Red = StimulusCorrection.stimulusCorrection[VectorDirection, 0] * currentStaircaseAmp,
                Green = StimulusCorrection.stimulusCorrection[VectorDirection, 1] * currentStaircaseAmp,
                Blue = StimulusCorrection.stimulusCorrection[VectorDirection, 2] * currentStaircaseAmp,
                Orange = StimulusCorrection.stimulusCorrection[VectorDirection, 3] * currentStaircaseAmp
            };

            // 2. CALL THE FINAL CALCDUTIES
            // This applies the 4th-order polynomial and clips values at 0.99
            // so you never lose a color channel (no yellow shift).
            CalcDuties(ref currentAmps);

            // 3. Extract the calibrated/safe values
            double[] testStml = { currentAmps.Red, currentAmps.Green, currentAmps.Blue, currentAmps.Orange };

            // To this (Dimmed by half):
            double dimFactor = 0.4; // 0.5 = 50% brightness. Change to 0.25 for 25%, etc.

            // 4. Reference Stimulus (Fixed White Flash)
            double[] refStml = {
                StimulusCorrection.stimulusCorrection[VectorDirection, 4] * dimFactor,
                StimulusCorrection.stimulusCorrection[VectorDirection, 5] * dimFactor,
                StimulusCorrection.stimulusCorrection[VectorDirection, 6] * dimFactor,
                StimulusCorrection.stimulusCorrection[VectorDirection, 7] * dimFactor
            };

            // 5. Generate the JSON content for the hardware
            content = ContentGenerator.GenerateSineStimulationWithoutHamming(
                new double[4], new double[4], new double[4], // Dummy values for unused cone params
                testStml, refStml, adapt, Direction
            );

            // 6. Physically send to the cube
            _vcp.SendJsonContent(JsonConvert.SerializeObject(content));
            foreach (Sequence seq in content.Sequences)
            {
                _vcp.SendJsonContent("{\"StartSequence\" : " + seq.LedIndex + "}");
            }
        }

        void MakeLUTs()
        {
            SerialSend();
        }
        
        void UpDown71(bool isCorrect)  // 2-down, 1-up staircase logic
        {
            Record[VectorDirection].second = Record[VectorDirection].first;  // shifts the previous answer into a "memory" slot so the computer knows what happened last trial
            Record[VectorDirection].first = isCorrect ? (uint)1 : (uint)0;

            if ((Record[VectorDirection].first == 1) && (Record[VectorDirection].second == 1))  // if the user recorded two successful detection in a row...
            {
                if (ContrastLevel[VectorDirection] > 0)  // if the contrast level can be lowered...
                {
                    Record[VectorDirection].first = Record[VectorDirection].second = 0;  // the recorded values reset
                    ContrastLevel[VectorDirection]--;  // the light gets dimmer
                }

            }
            else if (Record[VectorDirection].first == 0)
            {
                ContrastLevel[VectorDirection]++;  // the light gets brighter
            }
        }

        void mouse(int rl) // method header, rl is the variable that stores the mouse click
        {
            int indx;
            int TerminationFlg = 1, i;
            
            Console.WriteLine("Mouse");

            if (FirstFlg == 1)  // checks if the experiment has already started (the first click is just for test)
            {
                // rl: 1 = Left(1st brighter), 0 = Right(2nd brighter)
                // Direction: 0 = Test first, 1 = Reference first
                bool isCorrect = false;

                if ((rl == 0 && Direction == 1) || (rl == 1 && Direction == 0)) isCorrect = true;

                // DEBUG: Output the status of the click
                string testPos = (Direction == 0) ? "1st" : "2nd";
                string userChoice = (rl == 1) ? "1st" : "2nd";
                Console.WriteLine($">>> TRIAL: {OrderCntr} | CONDITION: {VectorDirection} | LEVEL: {ContrastLevel[VectorDirection]}");
                Console.WriteLine($"    Test was: {testPos} | You chose: {userChoice} | Result: {(isCorrect ? "CORRECT" : "WRONG")}");

                ChkConv((uint)VectorDirection, ContrastLevel[VectorDirection]);
                CheckTermination();
                result.Write(OrderCntr.ToString() + " " + ContrastLevel[VectorDirection].ToString() + " " + VectorDirection.ToString() + " " + Direction.ToString() + " " + RightLeft.ToString());  // logs the current trial number, brightness level and user response to the data file
                UpDown71(isCorrect); // decides whether to make the next pulse brighter or dimmer
                
                // Search for the next contrast that hasn't finished its 11 reversals yet
                do
                {
                    Console.WriteLine("OrderCntr: " + OrderCntr);
                    OrderCntr++;
                    if (OrderCntr >= 1440)
                    {
                        CheckTermination(); 
                        return;
                    }

                    indx = RandomStimulation.RandomOrder[OrderCntr % 180];
                    VectorDirection = (indx >> 1);
                    Direction = indx & 0x01; // Correctly get new interval order

                } while (EndFlg[VectorDirection] != 0);

                CalcNextCntrst(ContrastLevel[VectorDirection]);
            }

            FirstFlg = 1;
            stimulationNumber++;
            this.label3.Text = stimulationNumber.ToString();
            MakeLUTs(); // sends the new brightness commands to the hardware
        }
        //---------------------------------------------------------------------------

        //---------------------------------------------------------------------------
        void ChkConv(uint VectorDirection, uint ContrastLevel)  // the exit checker - monitors the "reversals"
        {
       
            uint indx1, indx2, indx3; // the specific color/condition being tested right now, how many trials you have completed for this color, how many times the brightness has actually changed
            int DiffSign;

            indx1 = VectorDirection;  // sets which color/axis we are currently checking, it is 0 in the beginning
	        indx2 = NumberOfData[indx1]; // that's 0 everytime? (yeah, if it is not global); but it should tell us for condition idx1, what trial is it on?
	        RsltUpDown[indx1,indx2] = (int)ContrastLevel;  // saves the current ContrastLevel for the indx1 condition for the indx2 trial
	       
	        if (indx2>1){  // the program needs 3 data to recognize a reversal
		        RsltUpDownDiff[indx1,indx2] = RsltUpDown[indx1,indx2] - RsltUpDown[indx1,indx2 - 1];  // Current - Previous: calculates if the brightness went up or down

		        if (RsltUpDownDiff[indx1,indx2] != 0){  // if there is a difference
			        indx3 = NumberOfData2[indx1];  // copy the number of changes in contrast
			        RsltUpDownDiff2[indx1,indx3] = RsltUpDownDiff[indx1,indx2]; // copy the difference in contrasts at the indx3 change
    			    NumberOfData2[indx1]++; // increase the number of changes

	    		    if (indx3>0){  // if there is more than 1 change
		    		    DiffSign = RsltUpDownDiff2[indx1, indx3] * RsltUpDownDiff2[indx1, indx3 - 1]; // calculate if the direction has changed (should be -1 if changed)

			    	    if (DiffSign == -1){   // Reverse Point
				    	    NumberOfDiff[indx1] ++;  // increment the reversal counter for this specific value

                            // DEBUG: Print the reversal to the Output Window
                            Console.WriteLine($"*** REVERSAL detected for Condition {indx1}! Count: {NumberOfDiff[indx1]} / {NumberOfReverse}");

                            if (NumberOfDiff[indx1] > NumberOfReverse){  // NumberOfReverse is 11
						        EndFlg[indx1] = 1; // marks this color as "Finished"
                            }
                        }
				    }
			    }
		    }
	        NumberOfData[indx1]++;  // move on to the next experiment condition
	    }

        void CheckTermination()
        {
            int totalFinished = 0;
            int totalActiveConditions = 0;

            for (int i = 0; i < 12; i++)
            {
                // Only count conditions that are actually enabled in your SFs array
                if (SFs[i] > 0)
                {
                    totalActiveConditions++;
                    if (EndFlg[i] == 1)
                    {
                        totalFinished++;
                    }
                }
            }

            // Update the GUI progress label (e.g., "5 / 12")
            if (this.label6 != null)
            {
                this.label6.Text = $"{totalFinished} / {totalActiveConditions}";
            }

            // If all active conditions are done, wrap up the experiment
            if (totalFinished >= totalActiveConditions && totalActiveConditions > 0)
            {
                log.Write("");
                log.Write("----------------BEGIN FINAL SUMMARY----------------");
                log.Write($"Experiment Finished at: {DateTime.Now.ToString()}");
                log.Write($"Total Trials Conducted: {OrderCntr}");
                log.Write("");

                for (int i = 0; i < 12; i++)
                {
                    if (SFs[i] > 0)
                    {
                        log.Write($"Condition [{i}] Final Contrast Level: {ContrastLevel[i]} (Reversals: {NumberOfDiff[i]})");
                    }
                }

                log.Write("----------------END FINAL SUMMARY------------------");
                log.Write("");

                MessageBox.Show("Experiment Complete! All conditions reached 11 reversals.\nData saved successfully.", "Done");
                System.Environment.Exit(0);
            }
        }

    }

}


