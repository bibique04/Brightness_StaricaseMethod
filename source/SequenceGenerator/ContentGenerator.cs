using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Data;

namespace SequenceGenerator
{
    class ContentGenerator
    {
        #region Input values
        private const int RedLedIndex = 0;
        private const int GreenLedIndex = 1;
        private const int BlueLedIndex = 2;
        private const int OrangeLedIndex = 3;


        private const int MaxPwmValue = 65535;
        private const int StimulusDuration = 3000; //msec
        private const int RampTime = 1000; //msec
        private const int WaitDuration = 1000; //ms
        private const double TF = 0.33333;// Hz
        private const bool Sin_SinOnFlag = false;

        private const int MeasurementDuration = 20000;
        private const int BeepIndex = 4;


        #endregion

        /// <summary>
        /// Apply a 4th order correction to the PWM value to remove the non linearity of the LEDs.
        /// output = pow(input, 4) * a + pow(input, 3) * b + pow(input, 2) * c + input * d
        /// </summary>
        /// <param name="input">The 4 LEDs PWM value</param>
        /// <param name="int2Volt">The correction matrix containing the coefficients</param>
        /// <returns></returns>
        private static Vector<double> MapToGammaCorrection(Vector<double> input, Matrix<double> int2Volt)
        {

            Vector<double> retVal = input.PointwisePower(4).PointwiseMultiply(int2Volt.Column(0))
                                    + input.PointwisePower(3).PointwiseMultiply(int2Volt.Column(1))
                                    + input.PointwisePower(2).PointwiseMultiply(int2Volt.Column(2))
                                    + input.PointwiseMultiply(int2Volt.Column(3));
            // correction of the non-linearity of the gammaCorrection
            for (int i = 0; i < retVal.Count; i++)
            {
                if (retVal[i] > 1)
                {
                    retVal[i] = 1;
                }
                else if (retVal[i] < 0)
                {
                    retVal[i] = 0;
                }
            }
            return retVal;
        }
        public static JsonContent GenerateBackground(double red, double green, double blue, double orange)
        {
            // Build Vectors and Matrix from the array values
            Matrix<double> int2Volt = Matrix<double>.Build.DenseOfArray(IntensityVoltage.Int2Volt);
            // Correct the LEDs data
            double[] colorArray = new double[4] { red, green, blue, orange };
            Vector<double> color = Vector.Build.DenseOfArray(colorArray);
            Vector<double> colorAValues = MapToGammaCorrection(color, int2Volt);

            JsonContent jsonContent = new JsonContent();
            // pattern data for background stimulation
            PatternData pRedBgnData = new PatternData()
            {
                Name = "Red Background"
            };
            PatternData pGreenBgnData = new PatternData()
            {
                Name = "Green Background"
            };
            PatternData pBlueBgnData = new PatternData()
            {
                Name = "Blue Background"
            };
            PatternData pOrangeBgnData = new PatternData()
            {
                Name = "Orange Background"
            };


            // generate sequence for background
            pRedBgnData.Data.Add((int)(color[0] * MaxPwmValue));
            pGreenBgnData.Data.Add((int)(color[1] * MaxPwmValue));
            pBlueBgnData.Data.Add((int)(color[2] * MaxPwmValue));
            pOrangeBgnData.Data.Add((int)(color[3] * MaxPwmValue));

            jsonContent.PatternDatas.Add(pRedBgnData);
            jsonContent.PatternDatas.Add(pGreenBgnData);
            jsonContent.PatternDatas.Add(pBlueBgnData);
            jsonContent.PatternDatas.Add(pOrangeBgnData);

            // Create all the patterns that will be used
            Pattern[] stimulations =
            {
                // red background
                new Pattern
                {
                    PatternDataIndex = 0,
                    Duration = 0,
                    Interval = 10
                },
                // green background
                new Pattern
                {
                    PatternDataIndex = 1,
                    Duration = 0,
                    Interval =10
                },
                // blue background
                new Pattern
                {
                    PatternDataIndex = 2,
                    Duration = 0,
                    Interval = 10
                },
                // orange background
                new Pattern
                {
                    PatternDataIndex = 3,
                    Duration = 0,
                    Interval = 10
                }
                
            };

            // Create all sequences and add into JSONContent
            // Create the four sequences for each LED
            // Red sequence
            Sequence seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[0]));
            seq.LedIndex = RedLedIndex;

            jsonContent.Sequences.Add(seq);

            // Green sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[1]));
            seq.LedIndex = GreenLedIndex;

            jsonContent.Sequences.Add(seq);

            // Blue sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[2]));
            seq.LedIndex = BlueLedIndex;

            jsonContent.Sequences.Add(seq);

            // Orange sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[3]));
            seq.LedIndex = OrangeLedIndex;

            jsonContent.Sequences.Add(seq);

            return jsonContent;
        }

        public static JsonContent GenerateSineStimulationWithoutHamming(double[] meanArray, double[] contrastArray, double[] phaseReceptorArray, double[] testStimulus, double[] refStimulus, double[] adaptationStimulus, int direction)
        {
            JsonContent jsonContent = new JsonContent();

            #region Matrix and Vector Setup
            // Constants for indices
            double testR = testStimulus[0]; double testG = testStimulus[1]; double testB = testStimulus[2]; double testY = testStimulus[3];
            double refR = refStimulus[0]; double refG = refStimulus[1]; double refB = refStimulus[2]; double refY = refStimulus[3];
            double adaptR = adaptationStimulus[0]; double adaptG = adaptationStimulus[1]; double adaptB = adaptationStimulus[2]; double adaptY = adaptationStimulus[3];

            // Build calibration matrix
            Matrix<double> int2Volt = Matrix<double>.Build.DenseOfArray(IntensityVoltage.Int2Volt);

            // Prepare arrays for the timeline (100 values per stimulus period)
            int points = (int)(100 / TF);
            double[] tRed = new double[points]; double[] tGreen = new double[points]; double[] tBlue = new double[points]; double[] tYellow = new double[points];
            double[] rRed = new double[points]; double[] rGreen = new double[points]; double[] rBlue = new double[points]; double[] rYellow = new double[points];
            double[] beepLine = new double[points];

            // Helper Wave vectors
            Vector<double> RWave = Vector<double>.Build.Dense(points);
            Vector<double> GWave = Vector<double>.Build.Dense(points);
            Vector<double> BWave = Vector<double>.Build.Dense(points);
            Vector<double> YWave = Vector<double>.Build.Dense(points);
            Vector<double> BeepWave = Vector<double>.Build.Dense(points);

            // Generate the baseline shapes (Trapezoid for LEDs, Pulse for Beep)
            GenerateWave(ref RWave, 1, 0, false, points);
            GenerateWave(ref GWave, 1, 0, false, points);
            GenerateWave(ref BWave, 1, 0, false, points);
            GenerateWave(ref YWave, 1, 0, false, points);
            GenerateWave(ref BeepWave, 1, 0, true, points);
            #endregion

            #region Fill Pattern Data
            // Scale the waves by the target intensities (Test vs Reference)
            for (int i = 0; i < points; i++)
            {
                tRed[i] = (testR - adaptR) * RWave[i] + adaptR;
                rRed[i] = (refR - adaptR) * RWave[i] + adaptR;

                tGreen[i] = (testG - adaptG) * GWave[i] + adaptG;
                rGreen[i] = (refG - adaptG) * GWave[i] + adaptG;

                tBlue[i] = (testB - adaptB) * BWave[i] + adaptB;
                rBlue[i] = (refB - adaptB) * BWave[i] + adaptB;

                tYellow[i] = (testY - adaptY) * YWave[i] + adaptY;
                rYellow[i] = (refY - adaptY) * YWave[i] + adaptY;

                beepLine[i] = BeepWave[i] + 1; // Standard beep level
            }

            // Convert everything to Gamma-Corrected PWM Values and add to PatternData objects
            PatternData pRedBgn = new PatternData { Name = "Red Bgn" };
            PatternData pRedTest = new PatternData { Name = "Red Test" };
            PatternData pRedRef = new PatternData { Name = "Red Ref" };

            PatternData pGreenBgn = new PatternData { Name = "Green Bgn" };
            PatternData pGreenTest = new PatternData { Name = "Green Test" };
            PatternData pGreenRef = new PatternData { Name = "Green Ref" };

            PatternData pBlueBgn = new PatternData { Name = "Blue Bgn" };
            PatternData pBlueTest = new PatternData { Name = "Blue Test" };
            PatternData pBlueRef = new PatternData { Name = "Blue Ref" };

            PatternData pOrangeBgn = new PatternData { Name = "Orange Bgn" };
            PatternData pOrangeTest = new PatternData { Name = "Orange Test" };
            PatternData pOrangeRef = new PatternData { Name = "Orange Ref" };

            PatternData pBeepBgn = new PatternData { Name = "Beep Bgn" };
            PatternData pBeepPulse = new PatternData { Name = "Beep Pulse" };

            // Apply the polynomial: y = ax4 + bx3 + cx2 + dx
            for (int i = 0; i < points; i++)
            {
                pRedTest.Data.Add((int)((Math.Pow(tRed[i], 4) * int2Volt[0, 0] + Math.Pow(tRed[i], 3) * int2Volt[0, 1] + Math.Pow(tRed[i], 2) * int2Volt[0, 2] + tRed[i] * int2Volt[0, 3]) * MaxPwmValue));
                pRedRef.Data.Add((int)((Math.Pow(rRed[i], 4) * int2Volt[0, 0] + Math.Pow(rRed[i], 3) * int2Volt[0, 1] + Math.Pow(rRed[i], 2) * int2Volt[0, 2] + rRed[i] * int2Volt[0, 3]) * MaxPwmValue));

                pGreenTest.Data.Add((int)((Math.Pow(tGreen[i], 4) * int2Volt[1, 0] + Math.Pow(tGreen[i], 3) * int2Volt[1, 1] + Math.Pow(tGreen[i], 2) * int2Volt[1, 2] + tGreen[i] * int2Volt[1, 3]) * MaxPwmValue));
                pGreenRef.Data.Add((int)((Math.Pow(rGreen[i], 4) * int2Volt[1, 0] + Math.Pow(rGreen[i], 3) * int2Volt[1, 1] + Math.Pow(rGreen[i], 2) * int2Volt[1, 2] + rGreen[i] * int2Volt[1, 3]) * MaxPwmValue));

                pBlueTest.Data.Add((int)((Math.Pow(tBlue[i], 4) * int2Volt[2, 0] + Math.Pow(tBlue[i], 3) * int2Volt[2, 1] + Math.Pow(tBlue[i], 2) * int2Volt[2, 2] + tBlue[i] * int2Volt[2, 3]) * MaxPwmValue));
                pBlueRef.Data.Add((int)((Math.Pow(rBlue[i], 4) * int2Volt[2, 0] + Math.Pow(rBlue[i], 3) * int2Volt[2, 1] + Math.Pow(rBlue[i], 2) * int2Volt[2, 2] + rBlue[i] * int2Volt[2, 3]) * MaxPwmValue));

                pOrangeTest.Data.Add((int)((Math.Pow(tYellow[i], 4) * int2Volt[3, 0] + Math.Pow(tYellow[i], 3) * int2Volt[3, 1] + Math.Pow(tYellow[i], 2) * int2Volt[3, 2] + tYellow[i] * int2Volt[3, 3]) * MaxPwmValue));
                pOrangeRef.Data.Add((int)((Math.Pow(rYellow[i], 4) * int2Volt[3, 0] + Math.Pow(rYellow[i], 3) * int2Volt[3, 1] + Math.Pow(rYellow[i], 2) * int2Volt[3, 2] + rYellow[i] * int2Volt[3, 3]) * MaxPwmValue));

                pBeepPulse.Data.Add((int)(beepLine[i] * MaxPwmValue / 2));
            }

            // Add Background Values (Single point)
            Vector<double> backCorrected = MapToGammaCorrection(Vector<double>.Build.DenseOfArray(adaptationStimulus), int2Volt);
            pRedBgn.Data.Add((int)(backCorrected[0] * MaxPwmValue));
            pGreenBgn.Data.Add((int)(backCorrected[1] * MaxPwmValue));
            pBlueBgn.Data.Add((int)(backCorrected[2] * MaxPwmValue));
            pOrangeBgn.Data.Add((int)(backCorrected[3] * MaxPwmValue));
            pBeepBgn.Data.Add(0);

            // Register Patterns into JSON (Strict Order for indexing)
            // Red: 0(B), 1(T), 2(B), 3(R), 4(B)
            jsonContent.PatternDatas.Add(pRedBgn); jsonContent.PatternDatas.Add(pRedTest); jsonContent.PatternDatas.Add(pRedBgn); jsonContent.PatternDatas.Add(pRedRef); jsonContent.PatternDatas.Add(pRedBgn);
            // Green: 5(B), 6(T), 7(B), 8(R), 9(B)
            jsonContent.PatternDatas.Add(pGreenBgn); jsonContent.PatternDatas.Add(pGreenTest); jsonContent.PatternDatas.Add(pGreenBgn); jsonContent.PatternDatas.Add(pGreenRef); jsonContent.PatternDatas.Add(pGreenBgn);
            // Blue: 10(B), 11(T), 12(B), 13(R), 14(B)
            jsonContent.PatternDatas.Add(pBlueBgn); jsonContent.PatternDatas.Add(pBlueTest); jsonContent.PatternDatas.Add(pBlueBgn); jsonContent.PatternDatas.Add(pBlueRef); jsonContent.PatternDatas.Add(pBlueBgn);
            // Orange: 15(B), 16(T), 17(B), 18(R), 19(B)
            jsonContent.PatternDatas.Add(pOrangeBgn); jsonContent.PatternDatas.Add(pOrangeTest); jsonContent.PatternDatas.Add(pOrangeBgn); jsonContent.PatternDatas.Add(pOrangeRef); jsonContent.PatternDatas.Add(pOrangeBgn);
            // Beep: 20(B), 21(B), 22(Pulse), 23(B), 24(B) -> Actually needs to match interval timing
            jsonContent.PatternDatas.Add(pBeepBgn); jsonContent.PatternDatas.Add(pBeepPulse); jsonContent.PatternDatas.Add(pBeepBgn); jsonContent.PatternDatas.Add(pBeepPulse); jsonContent.PatternDatas.Add(pBeepBgn);
            #endregion

            #region Sequence creation with Directional Swap
            // direction == 0 -> Test then Reference
            // direction == 1 -> Reference then Test
            int firstR = (direction == 0) ? 1 : 3; int secondR = (direction == 0) ? 3 : 1;
            int firstG = (direction == 0) ? 6 : 8; int secondG = (direction == 0) ? 8 : 6;
            int firstB = (direction == 0) ? 11 : 13; int secondB = (direction == 0) ? 13 : 11;
            int firstY = (direction == 0) ? 16 : 18; int secondY = (direction == 0) ? 18 : 16;

            // Build LED Sequences
            int[] ledIndices = { RedLedIndex, GreenLedIndex, BlueLedIndex, OrangeLedIndex };
            int[][] patternMaps = {
                new int[] {0, firstR, 2, secondR, 4},
                new int[] {5, firstG, 7, secondG, 9},
                new int[] {10, firstB, 12, secondB, 14},
                new int[] {15, firstY, 17, secondY, 19}
            };

            for (int l = 0; l < 4; l++)
            {
                Sequence s = new Sequence { LedIndex = ledIndices[l] };
                s.Patterns.Add(new Pattern { PatternDataIndex = patternMaps[l][0], Duration = WaitDuration, Interval = WaitDuration });
                s.Patterns.Add(new Pattern { PatternDataIndex = patternMaps[l][1], Duration = StimulusDuration, Interval = 10 });
                s.Patterns.Add(new Pattern { PatternDataIndex = patternMaps[l][2], Duration = WaitDuration, Interval = WaitDuration });
                s.Patterns.Add(new Pattern { PatternDataIndex = patternMaps[l][3], Duration = StimulusDuration, Interval = 10 });
                s.Patterns.Add(new Pattern { PatternDataIndex = patternMaps[l][4], Duration = 0, Interval = 10 });
                jsonContent.Sequences.Add(s);
            }

            // Beep Sequence (Always the same timing, indices 21 and 23)
            Sequence bSeq = new Sequence { LedIndex = BeepIndex };
            bSeq.Patterns.Add(new Pattern { PatternDataIndex = 20, Duration = WaitDuration, Interval = WaitDuration });
            bSeq.Patterns.Add(new Pattern { PatternDataIndex = 21, Duration = StimulusDuration, Interval = 10 });
            bSeq.Patterns.Add(new Pattern { PatternDataIndex = 22, Duration = WaitDuration, Interval = WaitDuration });
            bSeq.Patterns.Add(new Pattern { PatternDataIndex = 23, Duration = StimulusDuration, Interval = 10 });
            bSeq.Patterns.Add(new Pattern { PatternDataIndex = 24, Duration = 0, Interval = 10 });
            jsonContent.Sequences.Add(bSeq);

            #endregion

            return jsonContent;
        }
        public static JsonContent GenerateSineStimulationForMeasurement(double[] meanArray, double[] contrastArray, double[] phaseReceptorArray)
        {
            JsonContent jsonContent = new JsonContent();
            #region Matrix maths
            double meanL = meanArray[0];
            double meanM = meanArray[1];
            double meanS = meanArray[2];
            double meansipRGC = meanArray[3];
            double contrastL = contrastArray[0];
            double contrastM = contrastArray[1];
            double contrastS = contrastArray[2];
            double contrastipRGC = contrastArray[3];
            double[] meanrgby = new double[4];


            // Build Vectors and Matrix from the array values
            Vector<double> meanRGBY = Vector<double>.Build.DenseOfArray(meanrgby);
            Vector<double> meanMLSipRGC = Vector<double>.Build.DenseOfArray(meanArray);
            Vector<double> phaseReceptor = Vector<double>.Build.DenseOfArray(phaseReceptorArray);
            Vector<double> contrast = Vector<double>.Build.DenseOfArray(contrastArray);
            Matrix<double> int2Volt = Matrix<double>.Build.DenseOfArray(IntensityVoltage.Int2Volt);
            Matrix<double> p2CArray = Matrix<double>.Build.DenseOfArray(P2CArray.p2cArray);
            // Inverse of P2CArray
            Matrix<double> c2PArray = p2CArray.Inverse();
            meanRGBY = c2PArray * meanMLSipRGC;

            Vector<double> backgroundCorrectedValue = MapToGammaCorrection(meanRGBY, Matrix.Build.DenseOfArray(IntensityVoltage.Int2Volt));

            double[] red = new double[(int)(100 / TF)];
            double[] green = new double[(int)(100 / TF)];
            double[] blue = new double[(int)(100 / TF)];
            double[] yellow = new double[(int)(100 / TF)];
            double[] lValue = new double[(int)(100 / TF)];
            double[] mValue = new double[(int)(100 / TF)];
            double[] sValue = new double[(int)(100 / TF)];
            double[] ipRGCValue = new double[(int)(100 / TF)];
            double[] temp = new double[4];
            double[] rgby = new double[4];
            double[] highStimulation = new double[4];
            double[] lowStimulation = new double[4];



            Vector<double> LStimulus = Vector<double>.Build.Dense(lValue);
            Vector<double> MStimulus = Vector<double>.Build.Dense(mValue);
            Vector<double> SStimulus = Vector<double>.Build.Dense(sValue);
            Vector<double> ipRGCStimulus = Vector<double>.Build.Dense(ipRGCValue);

            // pattern measurements
            PatternData pRedHigh = new PatternData()
            {
                Name = "Red high stimulation"
            };
            PatternData pRedLow = new PatternData()
            {
                Name = "Red low stimulation"
            };
            PatternData pGreenHigh = new PatternData()
            {
                Name = "Green high stimulation"
            };
            PatternData pGreenLow = new PatternData()
            {
                Name = "Green low stimulation"
            };
            PatternData pBlueHigh = new PatternData()
            {
                Name = "Blue high stimulation"
            };
            PatternData pBlueLow = new PatternData()
            {
                Name = "Blue low stimulation"
            };
            PatternData pYellowHigh = new PatternData()
            {
                Name = "Yellow high stimulation"
            };
            PatternData pYellowLow = new PatternData()
            {
                Name = "Yellow low stimulation"
            };

            #endregion
            #region Sequence creation
            // generate the relative value with the offset for illuminance correction
            // vector is used as reference, it's the same result than &RedWave[0] in C language
            GenerateWave(ref LStimulus, 1, phaseReceptor[0], Sin_SinOnFlag, (int)(100 / TF));
            GenerateWave(ref MStimulus, 1, phaseReceptor[1], Sin_SinOnFlag, (int)(100 / TF));
            GenerateWave(ref SStimulus, 1, phaseReceptor[2], Sin_SinOnFlag, (int)(100 / TF));
            GenerateWave(ref ipRGCStimulus, 1, phaseReceptor[3], Sin_SinOnFlag, (int)(100 / TF));

            // generate the PWM value with hamming window
            /*
            GenerateRisingUpValue(ref pRedSineUpData.Data, LStimulus, RampTime, ConRed);
            GenerateRisingUpValue(ref pGreenSineUpData.Data, MStimulus, RampTime, ConGreen);
            GenerateRisingUpValue(ref pBlueSineUpData.Data, SStimulus, RampTime, ConBlue);
            GenerateRisingUpValue(ref pOrangeSineUpData.Data, ipRGCStimulus, RampTime, ConOrange);

            GenerateRisingDownValue(ref pRedSineDownData.Data, LStimulus, RampTime, ConRed);
            GenerateRisingDownValue(ref pGreenSineDownData.Data, MStimulus, RampTime, ConGreen);
            GenerateRisingDownValue(ref pBlueSineDownData.Data, SStimulus, RampTime, ConBlue);
            GenerateRisingDownValue(ref pOrangeSineDownData.Data, ipRGCStimulus, RampTime, ConOrange);
            */

            for (int i = 0; i < LStimulus.Count; i++)
            {
                LStimulus[i] = (contrastL * LStimulus[i] + 1) * meanL;
                //pRedSineData.Data.Add((int)((LStimulus[i] + 1) * receptor[0]));
            }
            for (int i = 0; i < MStimulus.Count; i++)
            {
                MStimulus[i] = (contrastM * MStimulus[i] + 1) * meanM;
                //pGreenSineData.Data.Add((int)((MStimulus[i] + 1) * receptor[1]));
            }
            for (int i = 0; i < SStimulus.Count; i++)
            {
                SStimulus[i] = (contrastS * SStimulus[i] + 1) * meanS;
                //pBlueSineData.Data.Add((int)((SStimulus[i] + 1) * receptor[2]));
            }
            for (int i = 0; i < ipRGCStimulus.Count; i++)
            {
                ipRGCStimulus[i] = (contrastipRGC * ipRGCStimulus[i] + 1) * meansipRGC;
                //pOrangeSineData.Data.Add((int)((ipRGCStimulus[i] + 1) * receptor[3]));
            }
            Vector<double> Red = Vector<double>.Build.Dense(red);
            Vector<double> Green = Vector<double>.Build.Dense(green);
            Vector<double> Blue = Vector<double>.Build.Dense(blue);
            Vector<double> Yellow = Vector<double>.Build.Dense(yellow);
            Vector<double> Temp = Vector<double>.Build.Dense(temp);
            Vector<double> RGBY = Vector<double>.Build.Dense(rgby);

            for (int i = 0; i < LStimulus.Count; i++)
            {
                Temp[0] = LStimulus[i];
                Temp[1] = MStimulus[i];
                Temp[2] = SStimulus[i];
                Temp[3] = ipRGCStimulus[i];

                RGBY = c2PArray * Temp;

                Red[i] = RGBY[0];
                Green[i] = RGBY[1];
                Blue[i] = RGBY[2];
                Yellow[i] = RGBY[3];
            }

            Vector<double> HIGHStimulation = Vector<double>.Build.Dense(highStimulation);
            Vector<double> LOWStimulation = Vector<double>.Build.Dense(lowStimulation);

            HIGHStimulation[0] = Red[50];
            HIGHStimulation[1] = Green[50];
            HIGHStimulation[2] = Blue[50];
            HIGHStimulation[3] = Yellow[50];
            LOWStimulation[0] = Red[150];
            LOWStimulation[1] = Green[150];
            LOWStimulation[2] = Blue[150];
            LOWStimulation[3] = Yellow[150];

            Vector<double> HIGHBGN = MapToGammaCorrection(HIGHStimulation, int2Volt);
            Vector<double> LOWBGN = MapToGammaCorrection(LOWStimulation, int2Volt);


            // generate sequence for background
            pRedHigh.Data.Add((int)(HIGHBGN[0] * MaxPwmValue));
            pRedLow.Data.Add((int)(LOWBGN[0] * MaxPwmValue));
            pGreenHigh.Data.Add((int)(HIGHBGN[1] * MaxPwmValue));
            pGreenLow.Data.Add((int)(LOWBGN[1] * MaxPwmValue));
            pBlueHigh.Data.Add((int)(HIGHBGN[2] * MaxPwmValue));
            pBlueLow.Data.Add((int)(LOWBGN[2] * MaxPwmValue));
            pYellowHigh.Data.Add((int)(HIGHBGN[3] * MaxPwmValue));
            pYellowLow.Data.Add((int)(LOWBGN[3] * MaxPwmValue));
            #endregion
            #region Json creation

            // add all patternData in the JsonContent
            jsonContent.PatternDatas.Add(pRedHigh);
            jsonContent.PatternDatas.Add(pRedLow);
            jsonContent.PatternDatas.Add(pGreenHigh);
            jsonContent.PatternDatas.Add(pGreenLow);
            jsonContent.PatternDatas.Add(pBlueHigh);
            jsonContent.PatternDatas.Add(pBlueLow);
            jsonContent.PatternDatas.Add(pYellowHigh);
            jsonContent.PatternDatas.Add(pYellowLow);

            // Create all the patterns that will be used
            Pattern[] stimulations =
            {
                
                // sine red
                new Pattern
                {
                    PatternDataIndex = 0,
                    Duration = MeasurementDuration,
                    Interval =MeasurementDuration
                },
                
                // red background
                new Pattern
                {
                    PatternDataIndex = 1,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                // green sine
                new Pattern
                {
                    PatternDataIndex = 2,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                //green background
                new Pattern
                {
                    PatternDataIndex = 3,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                // blue sine
                new Pattern
                {
                    PatternDataIndex = 4,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                // blue background
                new Pattern
                {
                    PatternDataIndex = 5,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                // orange sine
                new Pattern
                {
                    PatternDataIndex = 6,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                },
                
                // orange background
                new Pattern
                {
                    PatternDataIndex = 7,
                    Duration = MeasurementDuration,
                    Interval = MeasurementDuration
                }
            };

            // Create all sequences and add into JSONContent
            // Create the four sequences for each LED
            // Red sequence
            Sequence seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[0]));
            seq.Patterns.Add(new Pattern(stimulations[1]));
            seq.LedIndex = RedLedIndex;

            jsonContent.Sequences.Add(seq);

            // Green sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[2]));
            seq.Patterns.Add(new Pattern(stimulations[3]));
            seq.LedIndex = GreenLedIndex;

            jsonContent.Sequences.Add(seq);

            // Blue sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[4]));
            seq.Patterns.Add(new Pattern(stimulations[5]));
            seq.LedIndex = BlueLedIndex;

            jsonContent.Sequences.Add(seq);

            // orange sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[6]));
            seq.Patterns.Add(new Pattern(stimulations[7]));
            seq.LedIndex = OrangeLedIndex;

            jsonContent.Sequences.Add(seq);

            return jsonContent;
            #endregion
        }
        public static JsonContent GenerateSinusStimulation(double[]meanArray, double[] contrastArray,double[] phaseReceptorArray)
        {
            JsonContent jsonContent = new JsonContent();
            #region Matrix maths
            double ConRed = meanArray[0];
            double ConGreen = meanArray[1];
            double ConBlue = meanArray[2];
            double ConOrange = meanArray[3];

            Vector<double> backgroundCorrectedValue = MapToGammaCorrection(Vector.Build.DenseOfArray(meanArray),Matrix.Build.DenseOfArray(IntensityVoltage.Int2Volt));

            double[] red = new double[(int)(100 / TF)];
            double[] green = new double[(int)(100 / TF)];
            double[] blue = new double[(int)(100 / TF)];
            double[] orange = new double[(int)(100 / TF)];


            Vector<double> RedWave = Vector<double>.Build.Dense(red);
            Vector<double> GreenWave = Vector<double>.Build.Dense(green); 
            Vector<double> BlueWave = Vector<double>.Build.Dense(blue); 
            Vector<double> OrangeWave = Vector<double>.Build.Dense(orange);

            // pattern data for rising up sine stimulation
            PatternData pRedSineUpData = new PatternData()
            {
                Name = "Red sine rising up"
            };
            PatternData pGreenSineUpData = new PatternData()
            {
                Name = "Green sine rising up"
            };
            PatternData pBlueSineUpData = new PatternData()
            {
                Name = "Blue sine rising up"
            };
            PatternData pOrangeSineUpData = new PatternData()
            {
                Name = "Orange sine rising up"
            };

            // pattern data for sine stimulation
            PatternData pRedSineData = new PatternData()
            {
                Name = "Red sine"
            };
            PatternData pGreenSineData = new PatternData()
            {
                Name = "Green sine"
            };
            PatternData pBlueSineData = new PatternData()
            {
                Name = "Blue sine"
            };
            PatternData pOrangeSineData = new PatternData()
            {
                Name = "Orange sine"
            };

            // pattern data rising down stimulation
            PatternData pRedSineDownData = new PatternData()
            {
                Name = "Red sine rising down"
            };
            PatternData pGreenSineDownData = new PatternData()
            {
                Name = "Green sine rising down"
            };
            PatternData pBlueSineDownData = new PatternData()
            {
                Name = "Blue sine rising down"
            };
            PatternData pOrangeSineDownData = new PatternData()
            {
                Name = "Orange sine rising down"
            };
            // pattern data for background stimulation
            PatternData pRedBgnData = new PatternData()
            {
                Name = "Red Background"
            };
            PatternData pGreenBgnData = new PatternData()
            {
                Name = "Green Background"
            };
            PatternData pBlueBgnData = new PatternData()
            {
                Name = "Blue Background"
            };
            PatternData pOrangeBgnData = new PatternData()
            {
                Name = "Orange Background"
            };
            // Build Vectors and Matrix from the array values
            Vector<double> phaseReceptor = Vector<double>.Build.DenseOfArray(phaseReceptorArray);
            Vector<double> contrast = Vector<double>.Build.DenseOfArray(contrastArray);
            Matrix<double> int2Volt = Matrix<double>.Build.DenseOfArray(IntensityVoltage.Int2Volt);
            Matrix<double> p2CArray = Matrix<double>.Build.DenseOfArray(P2CArray.p2cArray);

            #endregion
            #region Sequence creation
            // generate the relative value with the offset for illuminance correction
            // vector is used as reference, it's the same result than &RedWave[0] in C language
            GenerateWave(ref RedWave, 1, phaseReceptor[0], Sin_SinOnFlag,(int) (TF*100));
            GenerateWave(ref GreenWave, 1, phaseReceptor[1], Sin_SinOnFlag, (int)(TF * 100));
            GenerateWave(ref BlueWave, 1, phaseReceptor[2], Sin_SinOnFlag, (int)(TF * 100));
            GenerateWave(ref OrangeWave, 1, phaseReceptor[3], Sin_SinOnFlag, (int)(TF * 100));

            // generate the PWM value with hamming window
            GenerateRisingUpValue(ref pRedSineUpData.Data, RedWave, RampTime, ConRed);
            GenerateRisingUpValue(ref pGreenSineUpData.Data, GreenWave, RampTime, ConGreen);
            GenerateRisingUpValue(ref pBlueSineUpData.Data, BlueWave, RampTime, ConBlue);
            GenerateRisingUpValue(ref pOrangeSineUpData.Data, OrangeWave, RampTime, ConOrange);

            GenerateRisingDownValue(ref pRedSineDownData.Data, RedWave, RampTime, ConRed);
            GenerateRisingDownValue(ref pGreenSineDownData.Data, GreenWave, RampTime, ConGreen);
            GenerateRisingDownValue(ref pBlueSineDownData.Data, BlueWave, RampTime, ConBlue);
            GenerateRisingDownValue(ref pOrangeSineDownData.Data, OrangeWave, RampTime, ConOrange);

            for(int i = 0; i < RedWave.Count; i++)
            {
                pRedSineData.Data.Add((int)((RedWave[i] + 1) * (MaxPwmValue / 2)));
            }
            for (int i = 0; i < GreenWave.Count; i++)
            {
                pGreenSineData.Data.Add((int)((GreenWave[i] + 1) * (MaxPwmValue / 2)));
            }
            for (int i = 0; i < BlueWave.Count; i++)
            {
                pBlueSineData.Data.Add((int)((BlueWave[i] + 1) * (MaxPwmValue / 2)));
            }
            for (int i = 0; i < OrangeWave.Count; i++)
            {
                pOrangeSineData.Data.Add((int)((OrangeWave[i] + 1) * (MaxPwmValue / 2)));
            }

            // generate sequence for background
            pRedBgnData.Data.Add((int) (backgroundCorrectedValue[0] * 65535));
            pGreenBgnData.Data.Add((int)(backgroundCorrectedValue[1] * 65535));
            pBlueBgnData.Data.Add((int)(backgroundCorrectedValue[2] * 65353));
            pOrangeBgnData.Data.Add((int)(backgroundCorrectedValue[3] * 65535));

            #endregion
            #region Json creation

            // add all patternData in the JsonContent
            jsonContent.PatternDatas.Add(pRedSineUpData);
            jsonContent.PatternDatas.Add(pRedSineData);
            jsonContent.PatternDatas.Add(pRedSineDownData);
            jsonContent.PatternDatas.Add(pRedBgnData);
            jsonContent.PatternDatas.Add(pGreenSineUpData);
            jsonContent.PatternDatas.Add(pGreenSineData);
            jsonContent.PatternDatas.Add(pGreenSineDownData);
            jsonContent.PatternDatas.Add(pGreenBgnData);
            jsonContent.PatternDatas.Add(pBlueSineUpData);
            jsonContent.PatternDatas.Add(pBlueSineData);
            jsonContent.PatternDatas.Add(pBlueSineDownData);
            jsonContent.PatternDatas.Add(pBlueBgnData);
            jsonContent.PatternDatas.Add(pOrangeSineUpData);
            jsonContent.PatternDatas.Add(pOrangeSineData);
            jsonContent.PatternDatas.Add(pOrangeSineDownData);
            jsonContent.PatternDatas.Add(pOrangeBgnData);

            // Create all the patterns that will be used
            Pattern[] stimulations =
            {
                // rising up red
                new Pattern 
                {
                    PatternDataIndex = 0,
                    Duration = RampTime,
                    Interval = 10
                },
                // sine red
                new Pattern
                {
                    PatternDataIndex = 1,
                    Duration = StimulusDuration,
                    Interval =10
                },
                // rising down red
                new Pattern
                {
                    PatternDataIndex = 2,
                    Duration = RampTime,
                    Interval = 10
                },
                // red background
                new Pattern
                {
                    PatternDataIndex = 3,
                    Duration = 0,
                    Interval = 10
                },
                // green rising up
                new Pattern
                {
                    PatternDataIndex = 4,
                    Duration = RampTime,
                    Interval = 10
                },
                // green sine
                new Pattern
                {
                    PatternDataIndex = 5,
                    Duration = StimulusDuration,
                    Interval = 10
                },
                // green rising down
                new Pattern
                {
                    PatternDataIndex = 6,
                    Duration = RampTime,
                    Interval = 10
                },
                //green background
                new Pattern
                {
                    PatternDataIndex = 7,
                    Duration = 0,
                    Interval = 10
                },
                // blue rising up
                new Pattern
                {
                    PatternDataIndex = 8,
                    Duration = RampTime,
                    Interval = 10
                },
                // blue sine
                new Pattern
                {
                    PatternDataIndex = 9,
                    Duration = StimulusDuration,
                    Interval = 10
                },
                // blue rising down
                new Pattern
                {
                    PatternDataIndex = 10,
                    Duration = RampTime,
                    Interval = 10
                },
                // blue background
                new Pattern
                {
                    PatternDataIndex = 11,
                    Duration = 0,
                    Interval = 10
                },
                // orange rising up
                new Pattern
                {
                    PatternDataIndex = 12,
                    Duration = RampTime,
                    Interval = 10
                },
                // orange sine
                new Pattern
                {
                    PatternDataIndex = 13,
                    Duration = StimulusDuration,
                    Interval = 10
                },
                // orange rising down
                new Pattern
                {
                    PatternDataIndex = 14,
                    Duration = RampTime,
                    Interval = 10
                },
                // orange background
                new Pattern
                {
                    PatternDataIndex = 15,
                    Duration = 0,
                    Interval = 10
                }
            };

            // Create all sequences and add into JSONContent
            // Create the four sequences for each LED
            // Red sequence
            Sequence seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[0]));
            seq.Patterns.Add(new Pattern(stimulations[1]));
            seq.Patterns.Add(new Pattern(stimulations[2]));
            seq.Patterns.Add(new Pattern(stimulations[3]));
            seq.LedIndex = RedLedIndex;

            jsonContent.Sequences.Add(seq);

            // Green sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[4]));
            seq.Patterns.Add(new Pattern(stimulations[5]));
            seq.Patterns.Add(new Pattern(stimulations[6]));
            seq.Patterns.Add(new Pattern(stimulations[7]));
            seq.LedIndex = GreenLedIndex;

            jsonContent.Sequences.Add(seq);

            // Blue sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[8]));
            seq.Patterns.Add(new Pattern(stimulations[9]));
            seq.Patterns.Add(new Pattern(stimulations[10]));
            seq.Patterns.Add(new Pattern(stimulations[11]));
            seq.LedIndex = BlueLedIndex;

            jsonContent.Sequences.Add(seq);

            // orange sequence
            seq = new Sequence();

            seq.Patterns.Add(new Pattern(stimulations[12]));
            seq.Patterns.Add(new Pattern(stimulations[13]));
            seq.Patterns.Add(new Pattern(stimulations[14]));
            seq.Patterns.Add(new Pattern(stimulations[15]));
            seq.LedIndex = OrangeLedIndex;

            jsonContent.Sequences.Add(seq);

            return jsonContent;
            #endregion
        }
        public static void GenerateRisingUpValue(ref List<int> PWMArray, Vector<double> wave, int ramptime, double bgn)
        {
            int steps = ramptime / 10;
            double ramp = 0;
            double amp = 0;
            for (int i= 0; i < steps; i++)
            {
                ramp = i * 10 / (double)ramptime;
                amp = ((wave[i % wave.Count] - bgn + 1) * ramp + bgn);
                PWMArray.Add((int)((amp) * (MaxPwmValue/ 2)));
            }
        }
        public static void GenerateRisingDownValue(ref List<int> PWMArray, Vector<double> wave, int ramptime, double bgn)
        {
            int steps = ramptime / 10;
            double ramp = 0;
            double amp = 0;
            for (int i = 0; i < steps; i++)
            {
                ramp = (steps - i) * 10 / (double)ramptime;
                amp = ((wave[i % wave.Count] - bgn + 1) * ramp + bgn);
                PWMArray.Add((int)(amp * MaxPwmValue/2));
            }
        }

        public static void GenerateWave(ref Vector<double> result, double average, double phase, bool SinOnFlag, int numberOfValue)
        {
            double e;
            for (int i = 0; i < numberOfValue; i++)
            {
                if (SinOnFlag == false)
                {
                    // LED Shape: Steady Trapezoid (Continuous White)
                    // Ramp up for the first 1/6th, Stay flat, Ramp down at the last 1/6th
                    if (i < numberOfValue / 6.0)
                    {
                        e = (double)i / (numberOfValue / 6.0);
                    }
                    else if (i > numberOfValue * 5.0 / 6.0)
                    {
                        e = 6.0 - ((double)i / (numberOfValue / 6.0));
                    }
                    else
                    {
                        e = 1.0; // This makes the stimulus steady/flat in the middle
                    }
                }
                else
                {
                    // Beep Shape: Pulse exactly in the center of the flash
                    if (i >= (int)(numberOfValue * 0.4) && i <= (int)(numberOfValue * 0.6))
                    {
                        e = 0.1;
                    }
                    else
                    {
                        e = 0;
                    }
                }
                result[i] = e;
            }
        }

        public static int getStimulusDuration()
        {
            return StimulusDuration;
        }
        public static int getRampTime()
        {
            return RampTime;
        }
        public static double getTemporalFrequency()
        {
            return TF;
        }
    }
}
