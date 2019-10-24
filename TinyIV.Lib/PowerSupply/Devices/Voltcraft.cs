using System;
using System.IO.Ports;

namespace TinyIV.Lib.PowerSupply
{
    public class Voltcraft : IPowerSupply
    {
        private SerialPort serialPort;

        private bool outputEnabled;
        private float voltage;
        private float current;
        private float voltageLimit;
        private float currentLimit;

        private float voltageMax;
        private float currentMax;

        public string AdapterName { get { return "Voltcraft DPPS"; } }

        public void Open(string portName)
        {
            if (serialPort != null)
                Close();
            
            try
            {
                Initialize(portName);
            }
            catch (Exception e)
            {
                Close();
                throw e;
            }
        }

        public void Close()
        {
            if(serialPort == null)
                return;

            serialPort.Close();
            serialPort = null;
        }

        private void Initialize(string portName) 
        {
            if(string.IsNullOrEmpty(portName))
                throw new InvalidOperationException("Kein Anschluss gewählt!");

            serialPort = new SerialPort(portName);
            serialPort.ReadTimeout = 2000;
            serialPort.Open();

            string tmp;
            int itmp;

            
            if(!SendCommand("GMOD", out tmp))
                throw new InvalidOperationException("Unbekanntes Messgerät!");
            DeviceName = tmp;

            if(!SendCommand("GOVP", out tmp))
                throw new InvalidOperationException("Unbekanntes Messgerät!");
            if(!int.TryParse(tmp, out itmp))
                throw new InvalidOperationException("Unbekanntes Messgerät!");
            VoltageMax = itmp / 10f;

            if(!SendCommand("GOCP", out tmp))
                throw new InvalidOperationException("Unbekanntes Messgerät!");
            if(!int.TryParse(tmp, out itmp))
                throw new InvalidOperationException("Unbekanntes Messgerät!");
            CurrentMax = itmp / 10f;
        }

        private bool SendCommand(string command, bool value)
        {
            WriteLine(command + (value ? "0" : "1"));
            try {
                return ReadLine() == "OK";
            }
            catch {
                return false;
            }
        }

        private bool SendCommand(string command, float value, int scale=10)
        {
            string val = string.Format("{0:000}", ((int)(value * scale)));
            WriteLine(command + val);
            try {
                return ReadLine() == "OK";
            }
            catch {
                return false;
            }
        }

        private bool SendCommand(string command, out string value)
        {
            WriteLine(command);
            try {
                value = ReadLine();
                return ReadLine() == "OK";
            }
            catch {
                value = null;
                return false;
            }
        }

        private void WriteLine(string text)
        {
            serialPort.Write(text);
            serialPort.Write("\r");
        }

        private string ReadLine()
        {
            return serialPort.ReadTo("\r");
        }

        public bool IsOpen { get { return serialPort != null ? serialPort.IsOpen : false; } }

        public string DeviceName { get; private set;}

        public bool OutputEnabled { 
            get { return outputEnabled; } 
            set {
                if(!SendCommand("SOUT", value))
                    throw new InvalidOperationException("Messgerät konnte nicht aktiviert werden!");

                outputEnabled = value;
            } 
        }
        public float VoltageLimit { 
            get { return voltageLimit; } 
            set {
                if(value < 0)
                    value = 0;

                if(value > VoltageMax)
                    value = VoltageMax;

                voltageLimit = value;

                if(Voltage > voltageLimit)
                    Voltage = voltageLimit;
            } 
        }
        public float CurrentLimit { 
            get { return currentLimit; } 
            set {
                if(value < 0)
                    value = 0;

                if(value > CurrentMax)
                    value = CurrentMax;

                currentLimit = value;

                if(Current > currentLimit)
                    Current = currentLimit;
            } 
        }
        public float VoltageMax { 
            get => voltageMax; 
            private set {
                voltageMax = value;

                if(VoltageLimit > VoltageMax)
                    VoltageLimit = VoltageMax;
            }
        }
        public float CurrentMax { 
            get => currentMax; 
            private set {
                currentMax = value;

                if(CurrentLimit > CurrentMax)
                    CurrentLimit = CurrentMax;
            }
        }
        public float Voltage { 
            get { return voltage; }
            set {
                if(value > VoltageLimit)
                    value = VoltageLimit;
                
                if(!SendCommand("VOLT", value))
                    throw new InvalidOperationException("Spannung konnte nicht gesetzt werden!");
                
                voltage = value;
            }
        }
        public float Current { 
            get { return current; }
            set {
                if(value > CurrentLimit)
                    value = CurrentLimit;
                
                if(!SendCommand("CURR", value))
                    throw new InvalidOperationException("Strom konnte nicht gesetzt werden!");
                
                current = value;
            }
        }
        public void GetOutput(out IVPair output, out OutputConstant status)
        {
            string tmp;
            int iTmp;
            
            output = new IVPair();

            if(!SendCommand("GETD", out tmp))
                throw new InvalidOperationException("Strom/Spannung konnte nicht gelesen werden!");

            if(!int.TryParse(tmp.Substring(0, 4), out iTmp))
                throw new InvalidOperationException("Spannung konnte nicht gelesen werden!");
            output.Voltage = iTmp / 100f;

            if(!int.TryParse(tmp.Substring(4, 4), out iTmp))
                throw new InvalidOperationException("Strom konnte nicht gelesen werden!");
            output.Current = iTmp / 100f;

            if(!int.TryParse(tmp.Substring(8, 1), out iTmp))
                throw new InvalidOperationException("Status konnte nicht gelesen werden!");

            status = (OutputConstant)iTmp;
        }
    }
}