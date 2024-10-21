using System.Text;

namespace Labs;

public class Frame
{
    public IReadOnlyList<byte> Flag { get; } = new List<byte> { 28, 28, 28, 28, 28, 28, 28, 28 };
    public int DestinationAddress { get; set; }
    public int SourceAddress { get; set; }
    public List<byte> Data { get; } = new();
    public byte Fcs { get; set; }

    public static List<Frame> CreateFrames(string message, int destinationAddress, int sourceAddress)
    {
        var frames = new List<Frame>();
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var maxDataLength = 28;
        var totalFrames = (int)Math.Ceiling((double)messageBytes.Length / maxDataLength);

        for (var i = 0; i < totalFrames; i++)
        {
            var frame = new Frame
            {
                DestinationAddress = destinationAddress,
                SourceAddress = sourceAddress,
                Fcs = 0
            };
            
            var dataLength = Math.Min(maxDataLength, messageBytes.Length - (i * maxDataLength));
            for (int j = 0; j < dataLength; j++)
            {
                frame.Data.Add(messageBytes[i * maxDataLength + j]);
            }

            frames.Add(frame);
        }

        return frames;
    }
    
    public byte[] Serialize()
    {
        var byteArray = new List<byte>();
        
        foreach (var flag in Flag)
        {
            byteArray.Add(flag);
        }
        
        byteArray.AddRange(BitConverter.GetBytes(DestinationAddress));
        byteArray.AddRange(BitConverter.GetBytes(SourceAddress));
        
        byteArray.AddRange(Data);
        
        byteArray.Add(Fcs);

        return byteArray.ToArray();
    }
    
    public static Frame Deserialize(byte[] byteArray)
    {
        var frame = new Frame();
        
        var flagLength = frame.Flag.Count;

        frame.DestinationAddress = BitConverter.ToInt32(byteArray, flagLength);
        frame.SourceAddress = BitConverter.ToInt32(byteArray, flagLength + sizeof(int));
        
        var dataStartIndex = flagLength + sizeof(int) * 2;
        var dataLength = byteArray.Length - dataStartIndex - 1;
        for (int i = 0; i < dataLength; i++)
        {
            frame.Data.Add(byteArray[dataStartIndex + i]);
        }
        
        frame.Fcs = byteArray[^1];

        return frame;
    }
}

