using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using TinyIV.Lib.PowerSupply;

namespace TinyIV.Lib
{
    public class IVGradientMeasurement : IMeasurement
    {
        private IPowerSupply psu;
        public IPowerSupply PowerSupply
        {
            get => psu;
            set
            {
                if (IsActive)
                    throw new InvalidOperationException("Das Messgerät kann während einer Messung nicht geändert werden!");

                psu = value;
            }
        }

        public bool IsActive { get; private set; }

        private OutputConstant status;
        private IVPair lastMeasure;
        public IVPair LastMeasure
        {
            get
            {
                if (!IsActive)
                    psu.GetOutput(out lastMeasure, out status);
                return lastMeasure;
            }
        }

        public List<IVPair> Measure()
        {
            if (IsActive)
                throw new InvalidOperationException("Messung ist bereits aktiv!");

            if (PowerSupply == null)
                throw new InvalidOperationException("Kein Messgerät gewählt!");

            if (PowerSupply.VoltageLimit == 0 || PowerSupply.CurrentLimit == 0)
                throw new InvalidOperationException("Ungültige Limits für Messung!");

            IsActive = true;

            List<IVPair> curve = null;
            try
            {
                curve = MeasureCurve();
            }
            catch (Exception e)
            {
                IsActive = false;
                throw e;
            }

            IsActive = false;

            return curve;
        }

        private List<IVPair> MeasureCurve()
        {
            List<IVPair> curve = new List<IVPair>();
            List<IVPair> measurements = new List<IVPair>();

            float voltageStep = (float)Math.Round(psu.VoltageLimit / 100, 1);
            if (voltageStep < 0.1f)
                voltageStep = 0.1f;

            //float voltageStep = 0.5f;

            float voltageLog = (float)Math.Round(psu.VoltageLimit / 10, 1);
            float currentLog = (float)Math.Round(psu.CurrentLimit / 10, 1);

            SetOutput(true);

            IVPair p;
            float gradient;
            float? last = null;

            CoolDown();

            p = GetSmoothIV(measurements);

            curve.Add(p);

            psu.Current = psu.CurrentLimit;

            do
            {
                p = GetSmoothIV(measurements);

                gradient = curve.Last().GetGradient(p);

                if ((gradient > 0 && (!last.HasValue || Math.Abs(1 - gradient / last.Value) > .2f)) ||
                   (p.Voltage - curve.Last().Voltage > voltageLog) ||
                   (p.Current - curve.Last().Current > currentLog))
                {
                    curve.Add(p);
                    last = gradient;
                }

                if (psu.Voltage < psu.VoltageLimit)
                    psu.Voltage += voltageStep;
            } while(HasChanges(measurements));

            SetOutput(true);

            return curve;
        }

        private bool HasChanges(List<IVPair> measurements)
        {
            if(measurements.Count < 20)
                return true;

            IVPair last = measurements.FirstOrDefault();
            IVPair avg;
            var tmp = measurements.Take(20);

            avg.Current = tmp.Sum(a => a.Current) / tmp.Count();
            avg.Voltage = tmp.Sum(a => a.Voltage) / tmp.Count();

            if (Math.Abs(last.Current - avg.Current) > 0.05)
                return true;

            if (Math.Abs(last.Voltage - avg.Voltage) > 0.05)
                return true;

            return false;
        }

        private IVPair GetSmoothIV(List<IVPair> measurements)
        {
            IVPair p;
            OutputConstant status;
            psu.GetOutput(out p, out status);

            lastMeasure = p;

            measurements.Insert(0, p);
            if(measurements.Count < 10)
                return measurements.Last();

            var tmp = measurements.Take(10);

            p.Current = tmp.Sum(a => a.Current) / tmp.Count();
            p.Voltage = tmp.Sum(a => a.Voltage) / tmp.Count();

            return p;
        }

        private void CoolDown()
        {
            IVPair p;
            OutputConstant status;
            do
            {
                psu.GetOutput(out p, out status);
                lastMeasure = p;
            } while (p.Voltage > (psu.Voltage * 1.05));
        }

        private void SetOutput(bool enabled)
        {
            psu.Current = 0;
            psu.Voltage = 1;
            psu.OutputEnabled = enabled;
        }
    }
}