using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace Approximation
{
    public partial class MainWindow : Window
    {
        const double EPS = 1e-10;
        enum Method
        {
            CHAN,
            KORI,
            KURBKUR,
            NAARA,
            UNDEFINED
        }

        Method activeMethod = Method.CHAN;

        List<Button> ButtonList = new List<Button>(); 
        List<double> pointX = new List<double>();
        List<double> pointY = new List<double>();
        List<double> pointX2 = new List<double>();
        List<double> pointY2 = new List<double>();

        private void ShowPoint()
        {
            
            ChartApproximation.Reset();

            double[] pX = pointX.ToArray();
            double[] pY = pointY.ToArray();
            double[] pX2 = pointX2.ToArray();
            double[] pY2 = pointY2.ToArray();

            var ChartLabelWater = ChartApproximation.Plot.AddScatter(pX, pY);
            var ChartLabelOil = ChartApproximation.Plot.AddScatter(pX2, pY2);

            ChartLabelOil.Label = "ОФП нефти";
            ChartLabelWater.Label = "ОФП воды";

            ChartApproximation.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

            ChartApproximation.Refresh();
        }

        private void SaveImageChart()
        {
            if (pointX.Count + pointX2.Count > 0)
            {
                DateTime timeNow = DateTime.Now;

                var imagePlot = ChartApproximation.Plot;

                imagePlot.SaveFig($"SaveImage/{timeNow.Hour}-{timeNow.Minute} {timeNow.Day}.{timeNow.Month}.{timeNow.Year}.png");

            }
            else MessageBox.Show("График пуст", "Невозможно сохранить график");
        }

        private void ClearPointList()
        {
            pointX.Clear();
            pointY.Clear();

            pointX2.Clear();
            pointY2.Clear();
        }

        private void SetActiveStart()
        {
            ChartApproximation.Reset();

            CurrentWaterSaturation.Text = "0,1";
            CriticalWaterSaturation.Text = "1,0";

            Equation1.Background = Brushes.White;
            Equation1.BorderBrush = Brushes.White;
        }

        private void SetActiveButton(object sender, EventArgs e)
        {
            Button but = (Button)sender;

            foreach (Button item in ButtonList)
            {
                if (item == but)
                {
                    item.Background = Brushes.White;
                    item.BorderBrush = Brushes.White;
                }
                else
                {
                    item.Background = stack.Background;
                    item.BorderBrush = Brushes.DarkGray;
                }
            }

            ChartApproximation.Reset();

            CurrentWaterSaturation.Text = "0,1";
            CriticalWaterSaturation.Text = "1,0";

        }

        private void MethodChan(double currentWS, double criticalWS)
        {
            ClearPointList();
            for (double i = currentWS; i < criticalWS + EPS; i += 0.05)
            {
                if (0 <= i && i < 0.2)
                {
                    pointX.Add(i);
                    pointY.Add(0);
                }
                else if (0.2 <= i && i <= 1.0)
                {
                    pointX.Add(i);
                    pointY.Add(Math.Pow((i - 0.2) / 0.8, 3.5));
                }

                if (0 <= i && i < 0.85)
                {
                    pointX2.Add(i);
                    pointY2.Add(Math.Pow((0.85 - i) / 0.85, 2.8) * (1 + 2.4 * i));
                }           
                else if (0.85 <= i && i <= 1.0)
                {
                    pointX2.Add(i);
                    pointY2.Add(0);
                }
            }
        }

        private void MethodKori(double currentWS, double criticalWS)
        {         
            ClearPointList();

            for (double i = currentWS; i < criticalWS + EPS; i += 0.05) 
            {
                pointX.Add(i);
                pointY.Add(Math.Pow(i, 3) * (2 - i / (1 - currentWS)));

                pointX2.Add(i);
                pointY2.Add(Math.Pow((1 - i / (1 - currentWS)), 4));      
            }
        }

        private void MethodKurbanovaKuranova(double currentWS, double criticalWS)
        {
            ClearPointList();
            for (double i = currentWS; i < criticalWS + EPS; i += 0.05)
            {
                if (0.1 < i && i <= 1.0)
                {
                    pointX.Add(i);
                    pointY.Add(Math.Pow((i - 0.1) / 0.9, 3));                
                }
                else if (0 <= i && i <= 0.1)
                {
                    pointX.Add(i);
                    pointY.Add(0);
                }

                if (0 <= i && i < 0.9)
                {
                    pointX2.Add(i);
                    pointY2.Add(Math.Pow((0.9 - i) / 0.9, 3));
                }
                else if (0.9 <= i && i <= 1.0)
                {
                    pointX2.Add(i);
                    pointY2.Add(0);
                }
            }
        }

        private void MethodNaaraGendersona(double currentWS, double criticalWS)//
        {
            ClearPointList();

            for (double i = currentWS; i < criticalWS + EPS; i += 0.05)
            {
                pointX.Add(i);
                pointY.Add(Math.Pow(i, 3) * (2 - i / (1 - currentWS)));

                pointX2.Add(i);
                pointY2.Add(Math.Pow((1 - i / (1 - currentWS)), 4));
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            ButtonList.Add(Equation1);
            ButtonList.Add(Equation2);
            ButtonList.Add(Equation3);
            ButtonList.Add(Equation4);
            ButtonList.Add(Equation5);

            foreach (Button btn in ButtonList)
            {
                btn.Click += new RoutedEventHandler(SetActiveButton);
            }

            SetActiveStart();
        }
       
        private void CreateСhart_Click(object sender, RoutedEventArgs e)
        {
            CurrentWaterSaturation.Text = CurrentWaterSaturation.Text.Replace('.',',');
            CriticalWaterSaturation.Text = CriticalWaterSaturation.Text.Replace('.', ',');

            double currentWS;
            double criticalWS;

            bool isCorrectcurrentWS = double.TryParse(CurrentWaterSaturation.Text, out currentWS);
            bool isCorrectcriticalWS = double.TryParse(CriticalWaterSaturation.Text, out criticalWS);

            if (activeMethod == Method.UNDEFINED)
            {
                MessageBox.Show("Данный метод еще не реализован", "Ошибка");
                return;
            }
            if (!isCorrectcurrentWS || !isCorrectcriticalWS )
            {
                MessageBox.Show("Некорректный ввод", "Ошибка");
                return;
            }
            if (currentWS >= criticalWS)
            {
                MessageBox.Show("Связанная водонасыщенность должна быть меньше критической", "Ошибка");
                return;
            }
            if (currentWS >= 0 && criticalWS >= 0 && currentWS <= 1 && criticalWS <= 1)
            {
                switch (activeMethod)
                {
                    case Method.CHAN:
                        MethodChan(currentWS, criticalWS);
                        break;
                    case Method.KORI:
                        MethodKori(currentWS, criticalWS);
                        break;
                    case Method.KURBKUR:
                        MethodKurbanovaKuranova(currentWS, criticalWS);
                        break;
                    case Method.NAARA:
                        MethodNaaraGendersona(currentWS, criticalWS);
                        break;
                    default:
                        break;
                }
                if (pointX.Count + pointX2.Count > 0)
                {
                    ShowPoint();
                }
                
            }
            else
            {
                MessageBox.Show("Значения должны быть в диапазоне от [0, 1]", "Ошибка");
            }
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            SaveImageChart();
        }

        private void Equation1_Click(object sender, RoutedEventArgs e)
        {
            ClearPointList();
            activeMethod = Method.CHAN;
        }

        private void Equation2_Click(object sender, RoutedEventArgs e)
        {
            ClearPointList();
            activeMethod = Method.KORI;
        }
    
        private void Equation3_Click(object sender, RoutedEventArgs e)
        {
            ClearPointList();
            activeMethod = Method.KURBKUR;
        }

        private void Equation4_Click(object sender, RoutedEventArgs e)
        {
            ClearPointList();
            activeMethod = Method.NAARA;
        }

        private void Equation5_Click(object sender, RoutedEventArgs e)
        {
            ClearPointList();
            activeMethod = Method.UNDEFINED;
        }

    }
}