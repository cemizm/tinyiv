using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TinyIV.Lib.PowerSupply;
using TinyIV.Lib;
using ReactiveUI;
using System.Reactive;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.IO;
using TinyIV.App.Models;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;

namespace TinyIV.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            measurement = new IVGradientMeasurement();

            Measurements = new ObservableCollection<Measurement>();
            SelectedMeasurements = new ObservableCollection<Measurement>();
            MeasurementView = new PlotModel();
            MeasurementView.Axes.Add(new LinearAxis() { Position = AxisPosition.Top, Title = "Spannung", Unit = "V", Minimum = 0, AxisTitleDistance = 5, MaximumPadding = 0.1 });
            MeasurementView.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "Strom", Unit = "A", Maximum = 0.2f, AxisTitleDistance = 25, MinimumPadding = 0.1 });

            SelectedMeasurements.CollectionChanged += (o, e) => UpdateMeasurements();

            StartMeasurement = ReactiveCommand.Create(RunMeasurement);
            Connect = ReactiveCommand.Create(ConnectToDevice);

            PowerSupplies = new List<IPowerSupply>() {
                new Voltcraft(),
                new DemoSupply()
            };
            PowerSupply = PowerSupplies[0];

            UpdatePorts();
            UpdateValues();
            IsActive = false;
        }

        #region PowerSupply
        private string port;
        public List<string> Ports
        {
            get => ports;
            set => this.RaiseAndSetIfChanged(ref ports, value);
        }
        private List<string> ports;
        public string Port
        {
            get => port;
            set => this.RaiseAndSetIfChanged(ref port, value);
        }

        private async Task UpdatePorts()
        {
            while (true)
            {
                Ports = SerialPort.GetPortNames().ToList();

                if (string.IsNullOrEmpty(Port))
                    Port = Ports.Where(p => p.Contains("SLAB")).FirstOrDefault();
                if (string.IsNullOrEmpty(Port))
                    Port = Ports.FirstOrDefault();

                await Task.Delay(500);
            }
        }

        private List<IPowerSupply> powerSupplies;
        public List<IPowerSupply> PowerSupplies
        {
            get => powerSupplies;
            set => this.RaiseAndSetIfChanged(ref powerSupplies, value);
        }

        private IPowerSupply powerSupply;
        public IPowerSupply PowerSupply
        {
            get => powerSupply;
            set
            {
                this.RaiseAndSetIfChanged(ref powerSupply, value);
                measurement.PowerSupply = powerSupply;
            }
        }
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            private set
            {
                this.RaiseAndSetIfChanged(ref isOpen, value);
                ConnectText = value ? "Trennen" : "Verbinden";
            }
        }
        private string connectText;
        public string ConnectText
        {
            get => connectText;
            private set => this.RaiseAndSetIfChanged(ref connectText, value);
        }

        public ReactiveCommand<Unit, Unit> Connect { get; }
        private void ConnectToDevice()
        {
            try
            {
                if (powerSupply == null)
                    throw new InvalidOperationException("Kein Messgerät gewählt!");

                if (powerSupply.IsOpen)
                {
                    AddLog("Verbindung zum Messgerät wird geschlossen!");
                    powerSupply.Close();
                }
                else
                {
                    AddLog("Verbindung zum Messgerät wird hergestellt!");

                    powerSupply.Open(Port);
                    powerSupply.VoltageLimit = 50;
                    powerSupply.CurrentLimit = 9;

                    AddLog(string.Format("Verbunden mit {0}.", powerSupply.DeviceName));
                }

                UpdateValues();
            }
            catch (Exception e)
            {
                AddLog(e.Message);
            }
        }

        #endregion

        #region Limits

        private float voltageLimit;
        public float VoltageLimit
        {
            get => voltageLimit;
            set
            {
                if (powerSupply == null)
                    throw new InvalidOperationException("Kein Messgerät gewählt!");

                powerSupply.VoltageLimit = value;
                this.RaiseAndSetIfChanged(ref voltageLimit, powerSupply.VoltageLimit);
            }
        }

        private float currentLimit;
        public float CurrentLimit
        {
            get => currentLimit;
            set
            {
                if (powerSupply == null)
                    throw new InvalidOperationException("Kein Messgerät gewählt!");

                powerSupply.CurrentLimit = value;
                this.RaiseAndSetIfChanged(ref currentLimit, powerSupply.CurrentLimit);
            }
        }

        #endregion

        #region Values

        private string voltageSet;
        public string VoltageSet
        {
            get => voltageSet;
            set => this.RaiseAndSetIfChanged(ref voltageSet, value);
        }

        private string currentSet;
        public string CurrentSet
        {
            get => currentSet;
            set => this.RaiseAndSetIfChanged(ref currentSet, value);
        }

        private string voltage;
        public string Voltage
        {
            get => voltage;
            set => this.RaiseAndSetIfChanged(ref voltage, value);
        }

        private string current;
        public string Current
        {
            get => current;
            set => this.RaiseAndSetIfChanged(ref current, value);
        }

        private async Task UpdateValues()
        {
            IsOpen = powerSupply != null ? powerSupply.IsOpen : false;

            while (IsOpen)
            {
                VoltageLimit = powerSupply.VoltageLimit;
                CurrentLimit = powerSupply.CurrentLimit;

                IVPair pair = measurement.LastMeasure;

                VoltageSet = string.Format("{0:00.00} V", powerSupply.Voltage);
                CurrentSet = string.Format("{0:00.00} A", powerSupply.Current);

                Voltage = string.Format("{0:00.00} V", pair.Voltage);
                Current = string.Format("{0:00.00} A", pair.Current);

                await Task.Delay(100);
            }

            VoltageSet = "--,-- V";
            CurrentSet = "--,-- A";
            Voltage = "--,-- V";
            Current = "--,-- A";
        }

        #endregion

        #region Measurement
        private IMeasurement measurement;

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            private set
            {
                this.RaiseAndSetIfChanged(ref isActive, value);
                MeasureText = value ? "Stop" : "Start";
            }
        }
        private string measureText;
        public string MeasureText
        {
            get => measureText;
            private set => this.RaiseAndSetIfChanged(ref measureText, value);
        }

        private int interval;
        public int Interval
        {
            get => interval;
            set => this.RaiseAndSetIfChanged(ref interval, value);
        }

        private int next = 1;

        public int Next
        {
            get => next;
            set => this.RaiseAndSetIfChanged(ref next, value);
        }

        private string filename;
        public string Filename
        {
            get => filename;
            set
            {
                if(value == null)
                    value = "";
                    
                this.RaiseAndSetIfChanged(ref filename, value);
                SelectedMeasurements.Clear();
                Measurements.Clear();
            }
        }

        public ObservableCollection<Measurement> Measurements { get; }

        public ObservableCollection<Measurement> SelectedMeasurements { get; private set; }

        private PlotModel measurementView;
        public PlotModel MeasurementView
        {
            get => measurementView;
            set => this.RaiseAndSetIfChanged(ref measurementView, value);
        }

        public ReactiveCommand<Unit, Unit> StartMeasurement { get; }
        private void RunMeasurement()
        {
            MeasureAsync();
        }
        private async Task MeasureAsync()
        {
            if (string.IsNullOrEmpty(Filename))
            {
                AddLog("Messung kann nicht ausgeführt werden. Bitte wählen Sie eine Zieldatei!");
                return;
            }

            IsActive = !IsActive;
            if (!IsActive)
                return;

            while (IsActive)
            {
                Exception ex = null;
                Measurement m = null;
                AddLog("Messung wird durchgeführt...");
                await Task.Run(() =>
                {
                    try
                    {
                        m = new Measurement();
                        m.Name = Next.ToString();
                        m.Curve = measurement.Measure();
                        File.AppendAllLines(Filename, m.GetLines());
                        File.AppendAllLines(Filename, new string[] { "", "" });
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                });

                if (ex != null)
                    AddLog(ex.Message);

                if (m != null)
                {
                    Measurements.Add(m);
                    SelectedMeasurements.Add(m);
                }

                Next++;
                AddLog("Messung abgeschlossen.");

                if (Interval > 0)
                    await Task.Delay(Interval * 60 * 1000);
                else
                    IsActive = false;
            }
        }

        private void UpdateMeasurements()
        {
            MeasurementView.Series.Clear();

            foreach (var m in SelectedMeasurements)
                MeasurementView.Series.Add(m.GetSeries());

            MeasurementView.InvalidatePlot(true);
        }

        #endregion

        #region Logs

        private string logText;

        public string LogText
        {
            get => logText;
            set => this.RaiseAndSetIfChanged(ref logText, value);
        }

        public void AddLog(string log)
        {
            LogText = string.Format("{0:g}: {1}{2}", DateTime.Now, log, Environment.NewLine) + LogText;
        }

        #endregion
    }
}
