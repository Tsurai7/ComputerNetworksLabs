using System.Text;

namespace Labs;

public class Frame
{
    public IReadOnlyList<byte> Flag { get; } = new List<byte> { 28, 28, 28, 28, 28, 28, 28, 28 };
    public int DestinationAddress { get; set; }
    public int SourceAddress { get; set; }
    public string Data { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int TTL { get; set; } = 5; 
    public bool IsToken { get; set; }
    
    public byte Fcs =>
        (byte)(SourceAddress ^ DestinationAddress ^ Priority ^ Data.Length);

    public byte[] Serialize()
    {
        var packetData = $"{SourceAddress}|{DestinationAddress}|{Priority}|{Data}|{(IsToken ? 1 : 0)}";
        return Encoding.UTF8.GetBytes(packetData);
    }

    public static Frame Deserialize(byte[] data)
    {
        var packetData = Encoding.UTF8.GetString(data).Split('|');
        return new Frame
        {
            SourceAddress = int.Parse(packetData[0]),
            DestinationAddress = int.Parse(packetData[1]),
            Priority = int.Parse(packetData[2]),
            Data = packetData[3],
            IsToken = packetData[4] == "1"
        };
    }
    
    public void DisplayFrameStructure()
    {
        Console.WriteLine("=== Frame Structure ===");
        Console.WriteLine($"Flag: [{string.Join(", ", Flag)}]");
        Console.WriteLine($"Destination Address: {DestinationAddress}");
        Console.WriteLine($"Source Address: {SourceAddress}");
        Console.WriteLine($"Data: {Data}");
        Console.WriteLine($"Priority: {Priority}");
        Console.WriteLine($"Is Token: {IsToken}");
        Console.WriteLine($"TTL: {TTL}");
        Console.WriteLine($"Checksum: {Fcs}");
        Console.WriteLine("=======================");
    }
}
