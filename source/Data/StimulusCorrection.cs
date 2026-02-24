namespace Data
{
    public class StimulusCorrection
    {
        // We only need one row now. 
        // Columns 0-3: Test Ratios (A5 White)
        // Columns 4-7: Ref Ratios (A5 White dimmed by 50% manually)
        public static double[,] stimulusCorrection = new double[,]
        {
            { 0.3827, 0.3682, 0.4995, 0.3780,  0.1913, 0.1841, 0.2497, 0.1890 }
        };
    }
}