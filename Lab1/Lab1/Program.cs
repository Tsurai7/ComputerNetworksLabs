using System.IO.Ports; 
 
Console.WriteLine("\n\nВведите скорость порта: "); 
var speed = Int32.Parse(Console.ReadLine()); 
 
var port1 = new SerialPort("/dev/ttys008", speed, Parity.None, 8, StopBits.One); 
var port2 = new SerialPort("/dev/ttys010", speed, Parity.None, 8, StopBits.One); 
 
port1.Open(); 
port2.Open(); 
 
port1.DataReceived += (sender, e) 
    => HandleDataReceived(port1, "port1"); 
 
port2.DataReceived += (sender, e) 
    => HandleDataReceived(port2, "port2"); 
 
while (true) 
{ 
    int newSpeed = 9600; 
    string input; 
    await Task.Delay(300); 
 
    Console.WriteLine("\n\nРабота с COM портами"); 
    Console.WriteLine("1 - передать с Первого на Второй"); 
    Console.WriteLine("2 - передать со Второго на Первый"); 
    Console.WriteLine("3 - поменять скорость ком портов"); 
    Console.WriteLine("4 - выход\n"); 
 
    Console.WriteLine("Ввод: "); 
 
    var choise = Int32.Parse(Console.ReadLine()); 
 
    if (choise == 3) 
    { 
        Console.WriteLine("\nВведите новую скорость: "); 
        newSpeed = Int32.Parse(Console.ReadLine()); 
    } 
 
    switch (choise) 
    { 
        case 1: 
            Console.WriteLine("\nВведите данные для перадачи: "); 
            input = Console.ReadLine(); 
            await Task.Run(() => port1.Write(input)); 
            break; 
        case 2: 
            Console.WriteLine("\nВведите данные для перадачи: "); 
            input = Console.ReadLine(); 
            await Task.Run(() => port2.Write(input)); 
            break; 
        case 3: 
            port1.BaudRate = newSpeed; 
            Console.WriteLine($"Новая скорость - {port1.BaudRate}"); 
            break; 
        case 4: 
            port1.Close(); 
            port2.Close(); 
            return; 
        default: 
            Console.WriteLine("Неправильный ввод"); 
            break; 
    } 
} 
 
void HandleDataReceived(SerialPort port, string portName) 
{ 
    var data = port.ReadExisting(); 
    Console.WriteLine($"\nОтправлено {portName}: {data}"); 
}