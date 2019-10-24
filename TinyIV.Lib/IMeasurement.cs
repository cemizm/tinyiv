using System.Collections.Generic;
using TinyIV.Lib.PowerSupply;

namespace TinyIV.Lib
{
    public interface IMeasurement
    {
        IPowerSupply PowerSupply { get; set; }

        bool IsActive { get; }

        IVPair LastMeasure { get; }
        
        List<IVPair> Measure();
    }
}