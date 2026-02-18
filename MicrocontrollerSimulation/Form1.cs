using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using SequenceGenerator;

namespace MicrocontrollerSimulation
{
    public partial class Form1 : Form
    {
        // Define the fixed path for the live feed
        private string livePath = @"C:\Users\tsujilab\source\results\live_stimulus.json";

        public Form1()
        {
            InitializeComponent();
            SetupChartProperties();
            SetupFileWatcher();
        }

        private void SetupChartProperties()
        {
            chart1.Series.Clear();
            var area = chart1.ChartAreas[0];

            // Fix Y-Axis between 0 and 1 (PWM 0% to 100%)
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1.1; // Slightly higher to see the 1.0 plateau clearly
            area.AxisY.Title = "PWM Intensity (Normalized)";

            // Set X-Axis for the 7000ms timeline
            area.AxisX.Minimum = 0;
            area.AxisX.Title = "Time (ms)";
        }

        private void SetupFileWatcher()
        {
            string folder = Path.GetDirectoryName(livePath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = folder;
            watcher.Filter = Path.GetFileName(livePath);
            watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Trigger update when the experiment program saves a new trial
            watcher.Changed += (s, e) => this.Invoke((MethodInvoker)delegate { LoadAndDraw(livePath); });
            watcher.EnableRaisingEvents = true;
        }

        private void btnLoadJson_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadAndDraw(openFileDialog.FileName);
            }
        }

        private void LoadAndDraw(string path)
        {
            try
            {
                // Small delay to ensure the file is fully written by the other project
                System.Threading.Thread.Sleep(50);

                string json = File.ReadAllText(path);
                JsonContent content = JsonConvert.DeserializeObject<JsonContent>(json);

                VirtualLEDProcessor processor = new VirtualLEDProcessor();
                chart1.Series.Clear();

                string[] ledNames = { "Red", "Green", "Blue", "Yellow" };
                System.Drawing.Color[] ledColors = { System.Drawing.Color.Red, System.Drawing.Color.Green, System.Drawing.Color.Blue, System.Drawing.Color.Yellow };

                for (int i = 0; i < 4; i++)
                {
                    var series = new Series(ledNames[i])
                    {
                        ChartType = SeriesChartType.StepLine, // StepLine looks like a real digital signal
                        BorderWidth = 3,
                        Color = ledColors[i]
                    };

                    var timeline = processor.ReconstructTimeline(content, i);

                    for (int t = 0; t < timeline.Count; t++)
                    {
                        series.Points.AddXY(t * 10, timeline[t]);
                    }
                    chart1.Series.Add(series);
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}