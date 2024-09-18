using System.IO.Ports;

var port1 = new SerialPort("/dev/ttys002", 9600, Parity.None, 8, StopBits.One);
var port2 = new SerialPort("/dev/ttys008", 9600, Parity.None, 8, StopBits.One);

port1.Open();
port2.Open();

port1.DataReceived += (sender, e) 
    => HandleDataReceived(port1, port1.PortName);
port2.DataReceived += (sender, e) 
    => HandleDataReceived(port2, port2.PortName);

while (true)
{
    await Task.Delay(300);
    
    Console.WriteLine("1 - Transmit from First to Second");
    Console.WriteLine("2 - Transmit from Fourth to Third");
    Console.WriteLine("3 - Change COM port speed");
    Console.WriteLine("4 - Exit");

    Console.WriteLine("Input: ");
    var choice = int.Parse(Console.ReadLine()!);

    await (choice switch
    {
        1 => TransmitData(port1),
        2 => TransmitData(port2),
        3 => ChangeSpeed(port1, port2),
        4 => ExitProgram(port1, port2),
        _ => Task.Run(() => Console.WriteLine("Invalid input"))
    });

    if (choice == 4) return;
}

Task TransmitData(SerialPort port)
{
    Console.WriteLine("Enter data to transmit: ");
    var input = Console.ReadLine()!;
    return Task.Run(() => port.Write(input));
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
    => Console.WriteLine($"Data received from {portName}: {port.ReadExisting()}");