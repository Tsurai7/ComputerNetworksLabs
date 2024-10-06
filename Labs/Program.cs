using System.IO.Ports;
using System.Text;
using Labs;

var port1 = new SerialPort("/dev/ttys001", 9600, Parity.None, 8, StopBits.One);
var port2 = new SerialPort("/dev/ttys002", 9600, Parity.None, 8, StopBits.One);

port1.Open();
port2.Open();

port1.DataReceived += (sender, e) => HandleDataReceived(port1, port1.PortName);
port2.DataReceived += (sender, e) => HandleDataReceived(port2, port2.PortName);

while (true)
{
    await Task.Delay(300);
    
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
    const int maxDataLength = 28;
    Console.WriteLine("Enter data to transmit: ");
    var input = Console.ReadLine()!;

    if (input.Length > maxDataLength)
    {
        Console.WriteLine($"Data length must be at most {maxDataLength} characters.");
        return;
    }
    
    var packet = new Frame
    {
        SourceAddress = senderPort.PortName[^1],
        DestinationAddress = 0,
        Data = Encoding.UTF8.GetBytes(input.PadRight(maxDataLength, '\0')) 
    };
    
    packet.FCS = Frame.CalculateHammingCode(packet.Data);

    var packetBytes = packet.ToBytes();
    var stuffedBytes = Frame.BitStuff(packetBytes);
    
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
    
    var unstuffedBytes = Frame.BitUnstuff(originalBytes);
    
    try
    {
        var packet = Frame.FromBytes(unstuffedBytes);

        Console.WriteLine($"Data received from {portName}:");
        
        var flagBinary = string.Join(", ", packet.Flag.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        Console.WriteLine($"  Flag: [{flagBinary}]");
        
        var sourceAddressHex = $"0x{packet.SourceAddress:X}";
        var destinationAddressHex = $"0x{packet.DestinationAddress:X}";
        
        var computedFCS = Frame.CalculateHammingCode(packet.Data);
        var fcsBinary = Convert.ToString(computedFCS, 2).PadLeft(8, '0');

        Console.WriteLine($"  Source Address: {sourceAddressHex}");
        Console.WriteLine($"  Destination Address: {destinationAddressHex}");
        
        var dataAddresses = string.Join(", ", packet.Data.Select(b => $"0x{b:X2}")); 
        Console.WriteLine($"  Data: [{dataAddresses}]");
        
        Console.WriteLine($"  Calculated  FCS: \u001b[31m{fcsBinary}\u001b[0m");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing received data: {ex.Message}");
    }
}
