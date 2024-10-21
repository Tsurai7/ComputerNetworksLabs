using System.IO.Ports;
using System.Text;

namespace Labs;

public class MonoChannel
{
    private readonly SerialPort _senderPort;
    private readonly SerialPort _receiverPort;

    private static readonly object ChannelLock = new();
    private static bool _channelBusy;
    private static Random _random = new();
    private const double CollisionProbability = 0.1;
    private const double ChannelBusyProbability = 0.2; 
    private const int MaxRetries = 16;
    private const int BaseCollisionWindow = 1000;

    public MonoChannel(SerialPort senderPort, SerialPort receiverPort)
    {
        _senderPort = senderPort;
        _receiverPort = receiverPort;

        if (!senderPort.IsOpen)
            senderPort.Open();

        if (!receiverPort.IsOpen)
            receiverPort.Open();

        _receiverPort.DataReceived += 
            (sender, e) => HandleDataReceived();
    }

    public void TransmitDataWithCsmaCd(byte[] data)
    {
        var frames = new List<Frame>();
        frames.AddRange(Frame.CreateFrames(Encoding.UTF8.GetString(data), 1, 2));

        foreach (var frame in frames)
        {
            var dataToSend = frame.Serialize();

            var retryCount = 0;
            while (retryCount < MaxRetries)
            {
                if (_random.NextDouble() < ChannelBusyProbability)
                {
                    Console.WriteLine($"[Sender {_senderPort.PortName}] Channel is busy. Waiting...");
                    Thread.Sleep(_random.Next(500, 1000));
                    continue;
                }

                Console.WriteLine($"[Sender {_senderPort.PortName}] Channel is free. Starting transmission...");
                if (AttemptTransmission(dataToSend))
                {
                    Console.WriteLine($"[Sender {_senderPort.PortName}] Frame transmitted successfully.");
                    break;
                }

                retryCount++;
                Console.WriteLine($"[Sender {_senderPort.PortName}] Collision detected. Retrying... Attempt {retryCount}/{MaxRetries}");
                var backoffTime = CalculateBackoffTime(retryCount);
                Thread.Sleep(backoffTime);
            }

            if (retryCount == MaxRetries)
            {
                Console.WriteLine($"[Sender {_senderPort.PortName}] Max retry limit reached. Transmission of frame failed.");
            }
        }
    }

    private bool AttemptTransmission(byte[] data)
    {
        lock (ChannelLock)
        {
            if (_channelBusy)
            {
                Console.WriteLine($"[Sender {_senderPort.PortName}] Channel is busy, can't send now.");
                return false;
            }

            _channelBusy = true;
            Console.WriteLine($"[Sender {_senderPort.PortName}] Sending data...");

            var transmissionSuccessful = true;
            
            for (var i = 0; i < data.Length; i++)
            {

                if (_random.NextDouble() < CollisionProbability)
                {
                    Console.WriteLine($"[Sender {_senderPort.PortName}] Collision detected on byte {i + 1}.");
                    HandleCollision();
                    transmissionSuccessful = false;
                    break;
                }
                _senderPort.Write(data, i, 1);
                Thread.Sleep(5);
            }

            _channelBusy = false;
            return transmissionSuccessful;
        }
    }

    private void HandleDataReceived()
    {
        lock (ChannelLock)
        {
            try
            {
                var buffer = new byte[_receiverPort.BytesToRead];
                var bytesRead = _receiverPort.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    var receivedFrame = Frame.Deserialize(buffer);
                    Console.WriteLine($"[Receiver {_receiverPort.PortName}] Frame received: DestinationAddress={receivedFrame.DestinationAddress}, SourceAddress={receivedFrame.SourceAddress}, Data={BitConverter.ToString(receivedFrame.Data.ToArray())}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data on {_receiverPort.PortName}: {ex.Message}");
            }
        }
    }

    private void HandleCollision()
    {
        Console.WriteLine($"[Sender {_senderPort.PortName}] Collision detected. Waiting for collision window...");
        Thread.Sleep(BaseCollisionWindow);
    }

    private static int CalculateBackoffTime(int attempt)
    {
        var maxBackoff = (int)Math.Pow(2, attempt) * BaseCollisionWindow;
        return _random.Next(1, maxBackoff + 1);
    }
}