using System.IO.Ports;

namespace Labs;

public static class ActionHandlers
{
    public static void TransmitDataHandler(TokenRingController tokenRingController)
    {
        Console.Write("Enter source address: ");
        int sourceAddress = int.Parse(Console.ReadLine()!);

        Console.Write("Enter destination address: ");
        int destinationAddress = int.Parse(Console.ReadLine()!);

        Console.Write("Enter message: ");
        string message = Console.ReadLine()!;

        Console.Write("Enter priority (1-2): ");
        int priority = int.Parse(Console.ReadLine()!);
        
        var dataPacket = new Frame
        {
            SourceAddress = sourceAddress,
            DestinationAddress = destinationAddress,
            Data = message,
            Priority = priority,
            IsToken = false
        };

        tokenRingController.QueueMessage(dataPacket);
        Console.WriteLine("Data packet sent.");
    }

    public static void ChangeBaudRateHandler(SerialPort port1, SerialPort port2)
    {
        Console.WriteLine("Enter new baud rate:");
        var newBaudRate = int.Parse(Console.ReadLine()!);
        port1.BaudRate = newBaudRate;
        port2.BaudRate = newBaudRate;
        Console.WriteLine($"New baud rate set to {newBaudRate}");
    }

    public static void ExitProgramHandler(SerialPort port1, SerialPort port2)
    {
        port1.Close();
        port2.Close();
        Console.WriteLine("Ports closed. Exiting program.");
    }
}