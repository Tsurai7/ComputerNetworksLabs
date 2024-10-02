namespace Labs;

public class Packet
{
    public byte Flag { get; init; }
    public int DestinationAddress { get; init; }
    public int SourceAddress { get; init; }
    public byte[] Data { get; init; }
    public byte FCS { get; init; }

    public byte[] ToBytes()
    {
        var packetLength = 1 + 4 + 4 + Data.Length + 1;
        var bytes = new byte[packetLength];
        bytes[0] = Flag;
        BitConverter.GetBytes(DestinationAddress).CopyTo(bytes, 1);
        BitConverter.GetBytes(SourceAddress).CopyTo(bytes, 5);
        Data.CopyTo(bytes, 9);
        bytes[^1] = FCS;
        return bytes;
    }

    public static Packet FromBytes(byte[] bytes)
    {
        var flag = bytes[0];
        var destinationAddress = BitConverter.ToInt32(bytes, 1);
        var sourceAddress = BitConverter.ToInt32(bytes, 5);
        var data = new byte[bytes.Length - 10];
        Array.Copy(bytes, 9, data, 0, data.Length);
        var fcs = bytes[^1];
        return new Packet
        { 
            Flag = flag,
            DestinationAddress = destinationAddress,
            SourceAddress = sourceAddress,
            Data = data,
            FCS = fcs 
        };
    }
    
    public static byte[] BitStuff(byte[] data)
    {
        var stuffedList = new List<byte>();
        foreach (var b in data)
        {
            if (b == 0x7E) 
            {
                stuffedList.Add(0x7D);
                stuffedList.Add(0x5E);
            }
            else if (b == 0x7D) 
            {
                stuffedList.Add(0x7D);
                stuffedList.Add(0x5D);
            }
            else
            {
                stuffedList.Add(b);
            }
        }
        return stuffedList.ToArray();
    }

    public static byte[] BitUnstuff(byte[] data)
    {
        var unstuffedList = new List<byte>();
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == 0x7D)
            {
                if (i + 1 < data.Length)
                {
                    if (data[i + 1] == 0x5E)
                    {
                        unstuffedList.Add(0x7E);
                        i++;
                    }
                    else if (data[i + 1] == 0x5D)
                    {
                        unstuffedList.Add(0x7D);
                        i++;
                    }
                    else
                    {
                        unstuffedList.Add(data[i]);
                    }
                }
            }
            else
            {
                unstuffedList.Add(data[i]);
            }
        }
        return unstuffedList.ToArray();
    }

}