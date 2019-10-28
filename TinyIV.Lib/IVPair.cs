using System;
using System.Linq;
using System.Collections.Generic;

namespace TinyIV.Lib
{
    public struct IVPair
    {
        public float Voltage;
        public float Current;

        public float Watt { get { return Voltage * Current; } }

        public float GetGradient(IVPair point)
        {
            return (Current - point.Current) / (Voltage - point.Voltage);
        }
    }
}