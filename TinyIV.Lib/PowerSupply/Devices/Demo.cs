using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace TinyIV.Lib.PowerSupply
{
    public class DemoSupply : IPowerSupply
    {
        private List<IVPair> curve;

        public DemoSupply()
        {
            curve = new List<IVPair> {
                new IVPair{ Voltage=0.39f, Current=0.01f },
                new IVPair{ Voltage=2.59f, Current=0.02f },
                new IVPair{ Voltage=9.10f, Current=0.02f },
                new IVPair{ Voltage=14.35f, Current=0.03f },
                new IVPair{ Voltage=20.93f, Current=0.04f },
                new IVPair{ Voltage=22.74f, Current=0.05f },
                new IVPair{ Voltage=23.93f, Current=0.06f },
                new IVPair{ Voltage=25.72f, Current=0.08f },
                new IVPair{ Voltage=26.33f, Current=0.09f },
                new IVPair{ Voltage=26.91f, Current=0.11f },
                new IVPair{ Voltage=27.53f, Current=0.12f },
                new IVPair{ Voltage=28.11f, Current=0.15f },
                new IVPair{ Voltage=28.72f, Current=0.19f },
                new IVPair{ Voltage=29.33f, Current=0.24f },
                new IVPair{ Voltage=29.92f, Current=0.30f },
                new IVPair{ Voltage=30.53f, Current=0.39f },
                new IVPair{ Voltage=31.73f, Current=0.64f },
                new IVPair{ Voltage=32.34f, Current=0.80f },
                new IVPair{ Voltage=32.93f, Current=1.03f },
                new IVPair{ Voltage=34.15f, Current=1.62f },
                new IVPair{ Voltage=34.73f, Current=2.00f },
                new IVPair{ Voltage=35.32f, Current=2.45f },
                new IVPair{ Voltage=36.52f, Current=3.52f },
                new IVPair{ Voltage=37.08f, Current=4.13f },
                new IVPair{ Voltage=38.14f, Current=5.40f },
                new IVPair{ Voltage=39.05f, Current=6.63f },
                new IVPair{ Voltage=39.76f, Current=7.67f },
                new IVPair{ Voltage=40.37f, Current=8.63f }
            };
        }

        public string AdapterName { get => "Demo PSU"; }
        public void Open(string port) => IsOpen = true;
        public void Close() => IsOpen = false;
        public bool IsOpen { get; private set; }
        public string DeviceName { get => "Demo Device"; }
        public bool OutputEnabled { get; set; }
        public float VoltageLimit { get; set; }
        public float CurrentLimit { get; set; }
        public float VoltageMax { get => 60f; }
        public float CurrentMax { get => 15f; }
        private float voltage;
        public float Voltage
        {
            get => voltage; set
            {
                voltage = value * 1.05f;
            }
        }
        public float Current { get; set; }
        public void GetOutput(out IVPair output, out OutputConstant status)
        {
            Thread.Sleep(100);

            output = new IVPair() { Voltage = Voltage, Current = Current };

            output = curve.Where(p => p.Voltage <= Voltage)
                          .OrderBy(p => p.Voltage)
                          .LastOrDefault();



            status = OutputConstant.Voltage;
        }
    }
}