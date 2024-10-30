using System.IO.Ports;

namespace Labs;

public static class ActionHandlers
{
    public static void TransmitDataWithCsmaCdHandler(MonoChannel channel)
    {
        Console.WriteLine("Choose direction of transmission (1: Port1 to Port2, 2: Port2 to Port1):");
        var directionChoice = Console.ReadLine();

        switch (directionChoice)
        {
            case "1":
                Console.WriteLine("Enter data to send from Port1 to Port2:");
                break;
            case "2":
                Console.WriteLine("Enter data to send from Port2 to Port1:");
                break;
            default:
                Console.WriteLine("Invalid choice, please try again.");
                return;
        }

        var data = Console.ReadLine();
        var bytesToSend = System.Text.Encoding.UTF8.GetBytes(data);
        
        channel.TransmitDataWithCsmaCd(bytesToSend);
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