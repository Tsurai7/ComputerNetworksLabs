using System.IO.Ports;

namespace Labs;

public static class Utils
{
    public static (SerialPort, SerialPort) ChooseSenderAndReceiver()
    {
        var ports = GetSerialPorts();
        Console.WriteLine("Choose sender port:");

        for (var i = 0; i < ports.Count; i++)
        {
            Console.WriteLine($"{i + 1} - {ports[i]}");
        }

        var senderIndex = int.Parse(Console.ReadLine()!) - 1;
        if (senderIndex < 0 || senderIndex >= ports.Count)
        {
            Console.WriteLine("Invalid input");
            throw new ApplicationException();
        }
        var senderPort = new SerialPort(ports[senderIndex], 9600, Parity.None, 8, StopBits.One);

        Console.WriteLine("Choose receiver port:");
        for (var i = 0; i < ports.Count; i++)
        {
            if (i != senderIndex)
            {
                Console.WriteLine($"{i + 1} - {ports[i]}");
            }
        }

        var receiverIndex = int.Parse(Console.ReadLine()!) - 1;
        if (receiverIndex < 0 || receiverIndex >= ports.Count || receiverIndex == senderIndex)
        {
            Console.WriteLine("Invalid input");
            throw new ApplicationException();
        }
        var receiverPort = new SerialPort(ports[receiverIndex], 9600, Parity.None, 8, StopBits.One);
        
        return (senderPort, receiverPort);
    }
    
    // All virtual serial ports on macOS are stored here: /dev/ttys...
    private static List<string> GetSerialPorts()
        => Directory.GetFiles("/dev/", "tty*")
            .Where(x => x.StartsWith("/dev/ttys00")).ToList();
}