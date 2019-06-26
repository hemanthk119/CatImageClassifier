using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CatImageRecognizer.ViewModels
{
    public class GraphData : INotifyPropertyChanged
    {
        public SeriesCollection SeriesCollection { get; set; }

        private double _axisMax = 10;
        private double _axisMin = 1;

        public int MaxItemsToShow { get; set; } = 100;
        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                RaiseProperty("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                RaiseProperty("AxisMin");
            }
        }

        private void SetAxisLimits()
        {
            AxisMax = SeriesCollection[0].Values.Count + 10;
            AxisMin = SeriesCollection[0].Values.Count > MaxItemsToShow ? SeriesCollection[0].Values.Count - MaxItemsToShow : 1;
        }

        public void AddDataPoint(double dataPoint)
        {
            SeriesCollection[0].Values.Add(dataPoint);
            SeriesCollection[1].Values.Add(new OhlcPoint(0, 0, 0, 0));
            SeriesCollection[2].Values.Add(new OhlcPoint(0, 0, 0, 0));
            SetAxisLimits();

            if(SeriesCollection[0].Values.Count > MaxItemsToShow)
            {
                SeriesCollection[0].Values.RemoveAt(0);
                SeriesCollection[1].Values.RemoveAt(0);
                SeriesCollection[2].Values.RemoveAt(0);
            }
        }

        public void AddPageChanged()
        {
            SeriesCollection[1].Values[SeriesCollection[1].Values.Count - 1] = new OhlcPoint(0, 100, 0, 0);
        }

        public void AddIterationChanged()
        {
            SeriesCollection[2].Values[SeriesCollection[2].Values.Count - 1] = new OhlcPoint(0, 100, 0, 0);
        }

        public void ClearDataCollection()
        {
            foreach(var dataSeries in SeriesCollection)
            {
                dataSeries.Values.Clear();
            }
        }

        public GraphData()
        {
            SeriesCollection = new SeriesCollection()
            {
                new LineSeries
                {
                    Values = new ChartValues<double>(),
                    ScalesYAt = 0,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 6,
                    StrokeThickness = 1
                },
                new OhlcSeries()
                {
                    Values = new ChartValues<OhlcPoint>
                    {
                        new OhlcPoint(0, 0, 0, 0)
                    },
                    ScalesYAt = 1,
                    StrokeThickness = 1,
                    DecreaseBrush = new SolidColorBrush(Colors.Green)
                },
                new OhlcSeries()
                {
                    Values = new ChartValues<OhlcPoint>
                    {
                        new OhlcPoint(0, 0, 0, 0)
                    },
                    ScalesYAt = 2,
                    StrokeThickness = 1,
                    DecreaseBrush = new SolidColorBrush(Colors.Blue)
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
