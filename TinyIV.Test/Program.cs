using System;
using System.IO;
using System.Collections.Generic;
using TinyIV.Lib.PowerSupply;
using TinyIV.Lib;

namespace TinyIV.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var culture =  System.Globalization.CultureInfo.GetCultureInfo("de-DE"); 

            IMeasurement measurement = new IVGradientMeasurement();
            measurement.PowerSupply = new Voltcraft();
            measurement.PowerSupply.Open("/dev/cu.SLAB_USBtoUART");
            measurement.PowerSupply.VoltageLimit = 60;
            measurement.PowerSupply.CurrentLimit = 9;
            DateTime start = DateTime.Now;

            IEnumerable<IVPair> points = measurement.Measure();
            Console.WriteLine("{0}", (DateTime.Now - start).TotalSeconds);
            
            List<string> lines = new List<string>();
            foreach(IVPair point in points)
                lines.Add(string.Format(culture, "{0:0.00}\t{1:0.00}", point.Voltage, -point.Current));

            File.WriteAllLines("pvserve.txt", lines.ToArray());
        }
    }
}
