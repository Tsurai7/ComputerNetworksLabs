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
    
    private const double CollisionProbability = 0.3;
    private const double ChannelBusyProbability = 0.5;
    private const int MaxRetries = 16;
    private const int BaseCollisionWindow = 500;

    public MonoChannel(
        SerialPort senderPort,
        SerialPort receiverPort)
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

    public void TransmitDataWithCsmaCd(string message)
    {
        var frames = Frame.CreateFrames(message, 1, 2);

        foreach (var frame in frames)
        {
            var dataToSend = frame.Serialize();
            
            Console.WriteLine($"[Sender {_senderPort.PortName}] Sending frame with length: {dataToSend.Length}");

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
            
            if (_random.NextDouble() < CollisionProbability)
            {
                Console.WriteLine($"[Sender {_senderPort.PortName}] Collision detected on packet.");
                HandleCollision();
                return false;
            }

            _channelBusy = true;
            Console.WriteLine($"[Sender {_senderPort.PortName}] Sending data...");

            var transmissionSuccessful = true;
            
            for (var i = 0; i < data.Length; i++)
            {
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

                if (bytesRead > 0 && buffer.Length >= 18)
                {
                    var receivedFrame = Frame.Deserialize(buffer);
                    
                    var receivedData = Encoding.UTF8.GetString(receivedFrame.Data.ToArray());

                    Console.WriteLine($"[Receiver {_receiverPort.PortName}] Received Frame Structure:");
                    Console.WriteLine($"  - Flags: {BitConverter.ToString(receivedFrame.Flag.ToArray())}");
                    Console.WriteLine($"  - Destination Address: {receivedFrame.DestinationAddress}");
                    Console.WriteLine($"  - Source Address: {receivedFrame.SourceAddress}");
                    Console.WriteLine($"  - Data: {receivedData}");
                    Console.WriteLine($"  - FCS: {10101110}");
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
