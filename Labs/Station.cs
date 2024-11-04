using System.IO.Ports;

namespace Labs;

public class Station
{
    private SerialPort _port;
    public int Address { get; }
    public bool IsTokenHolder { get; set; }
    public event Action<Frame> OnReceivePacket;

    public Station(string portName, int address, bool isTokenHolder)
    {
        _port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
        _port.DataReceived += OnDataReceived;
        Address = address;
        IsTokenHolder = isTokenHolder;
    }

    public void Start()
    {
        _port.Open();
    }

    public void Stop()
    {
        if (_port.IsOpen)
        {
            _port.Close();
        }
    }

    public void Send(Frame packet)
    {
        if (_port.IsOpen)
        {
            byte[] data = packet.Serialize();
            _port.Write(data, 0, data.Length);
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var buffer = new byte[_port.BytesToRead];
        _port.Read(buffer, 0, buffer.Length);

        var packet = Frame.Deserialize(buffer);
        
        OnReceivePacket?.Invoke(packet);
    }
}
