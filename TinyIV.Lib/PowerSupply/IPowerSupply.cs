namespace TinyIV.Lib.PowerSupply
{
    public interface IPowerSupply
    {
        string AdapterName {get;}
        void Open(string port);
        void Close();
        bool IsOpen { get; }
        string DeviceName { get; }
        bool OutputEnabled { get; set; }
        float VoltageLimit { get; set; }
        float CurrentLimit { get; set; }
        float VoltageMax {get;}
        float CurrentMax {get;}
        float Voltage { get; set; }
        float Current { get; set; }
        void GetOutput(out IVPair output, out OutputConstant status);
    }
}