using System;
using System.Linq;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using TinyIV.Lib;

namespace TinyIV.App.Models
{
    public class Measurement
    {
        public string Name { get; set; }
        public List<IVPair> Curve { get; set; }

        public string[] GetLines()
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format("Nr. {0}", Name));
            lines.Add(string.Format("U\tI"));

            foreach (IVPair pair in Curve)
                lines.Add(string.Format("{0}\t{1}", pair.Voltage, -pair.Current));

            return lines.ToArray();
        }

        public LineSeries GetSeries()
        {
            var ls = new LineSeries
            {
                MarkerType = MarkerType.Star,
                MarkerStroke = OxyColors.Black,

                RenderInLegend = true,
                Title = string.Format("Nr. {0}", Name)
            };

            ls.Points.AddRange(Curve.Select(p => new DataPoint(p.Voltage, -p.Current)));

            return ls;
        }
    }
}