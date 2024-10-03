namespace Labs
{
    public class Packet
    {
        public byte[] Flag = [28, 0, 0, 0, 0, 0, 0, 0];
        public int DestinationAddress { get; init; }
        public int SourceAddress { get; init; }
        public byte[] Data = new byte[29];
        public byte FCS { get; init; }

        public byte[] ToBytes()
        {
            var packetLength = Flag.Length + 4 + 4 + Data.Length + 1;
            var bytes = new byte[packetLength];

            Array.Copy(Flag, 0, bytes, 0, Flag.Length); 
            BitConverter.GetBytes(DestinationAddress).CopyTo(bytes, Flag.Length); 
            BitConverter.GetBytes(SourceAddress).CopyTo(bytes, Flag.Length + 4);
            Data.CopyTo(bytes, Flag.Length + 8); 
            bytes[^1] = FCS;

            return bytes;
        }

        public static Packet FromBytes(byte[] bytes)
        {
            var flag = new byte[8];
            Array.Copy(bytes, 0, flag, 0, 8);
            var destinationAddress = BitConverter.ToInt32(bytes, 8);
            var sourceAddress = BitConverter.ToInt32(bytes, 12);
            var dataLength = bytes.Length - 17;
            var data = new byte[dataLength];
            Array.Copy(bytes, 16, data, 0, dataLength);
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
}
