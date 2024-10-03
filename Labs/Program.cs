using System.IO.Ports;
using System.Text;
using Labs;

var port1 = new SerialPort("/dev/ttys002", 9600, Parity.None, 8, StopBits.One);
var port2 = new SerialPort("/dev/ttys003", 9600, Parity.None, 8, StopBits.One);

port1.Open();
port2.Open();

port1.DataReceived += (sender, e) => HandleDataReceived(port1, port1.PortName);
port2.DataReceived += (sender, e) => HandleDataReceived(port2, port2.PortName);

while (true)
{
    await Task.Delay(3000);
    
    Console.WriteLine("1 - Transmit from First to Second");
    Console.WriteLine("2 - Transmit from Second to First");
    Console.WriteLine("3 - Change COM port speed");
    Console.WriteLine("4 - Exit");
    Console.Write("Input: ");
    
    var choice = int.Parse(Console.ReadLine()!);

    await (choice switch
    {
        1 => TransmitData(port1, port2),
        2 => TransmitData(port2, port1),
        3 => ChangeSpeed(port1, port2),
        4 => ExitProgram(port1, port2),
        _ => Task.Run(() => Console.WriteLine("Invalid input"))
    });

    if (choice == 4) return;
}

async Task TransmitData(SerialPort senderPort, SerialPort receiverPort)
{
    var dataLength = 28 + 1;

    Console.WriteLine("Enter data to transmit: ");
    var input = Console.ReadLine()!;
    
    if (input.Length > dataLength - 1)
    {
        Console.WriteLine($"Data length must be at most {dataLength - 1} characters.");
        return;
    }
    
    var packet = new Packet
    {
        SourceAddress = senderPort.PortName[^1],
        DestinationAddress = 0,
        FCS = 0,
        Data = Encoding.UTF8.GetBytes(input.PadRight(dataLength - 1, '\0'))
    };
    
    var packetBytes = packet.ToBytes();
    var stuffedBytes = Packet.BitStuff(packetBytes);
    
    await Task.Run(() => senderPort.Write(stuffedBytes, 0, stuffedBytes.Length));
}

Task ChangeSpeed(SerialPort port1, SerialPort port2)
{
    Console.WriteLine("Enter new speed: ");
    var newSpeed = int.Parse(Console.ReadLine()!);
    port1.BaudRate = port2.BaudRate = newSpeed;
    Console.WriteLine($"New speed - {port1.BaudRate}");
    return Task.CompletedTask;
}

Task ExitProgram(SerialPort port1, SerialPort port2)
{
    port1.Close();
    port2.Close();
    return Task.CompletedTask;
}

void HandleDataReceived(SerialPort port, string portName)
{
    var data = port.ReadExisting();
    var bytes = Encoding.UTF8.GetBytes(data);


    var originalBytes = bytes.ToArray(); 
    
    var unstuffedBytes = Packet.BitUnstuff(bytes);
    var packet = Packet.FromBytes(unstuffedBytes);

    Console.WriteLine($"Data received from {portName}:");
    
    Console.WriteLine("Original Frame (before de-stuffing):");
    PrintFrame(originalBytes, "Original");
    
    Console.WriteLine("Decoded Packet:");
    PrintFrame(unstuffedBytes, "Decoded");

    var flagBinary = string.Join(", ", packet.Flag.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    Console.WriteLine($"  Flag: [{flagBinary}]");
    
    var sourceAddressHex = $"0x{packet.SourceAddress:X}";
    var destinationAddressHex = $"0x{packet.DestinationAddress:X}";
    
    Console.WriteLine($"  Source Address: {sourceAddressHex}");
    Console.WriteLine($"  Destination Address: {destinationAddressHex}");
    
    var dataAddresses = string.Join(", ", packet.Data.Select(b => $"0x{b:X2}")); 
    Console.WriteLine($"  Data: [{dataAddresses}]");
}

void PrintFrame(byte[] frame, string label)
{
    Console.WriteLine($"{label} Frame:");

    for (int i = 0; i < frame.Length; i++)
    {
        string binaryString = Convert.ToString(frame[i], 2).PadLeft(8, '0');
        
        if (frame[i] == 0x7D) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Byte {i}: {binaryString} (Modified)");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"  Byte {i}: {binaryString}");
        }
    }

    Console.WriteLine();
}