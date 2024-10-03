namespace Labs
{
    public class Packet
    {
        public byte[] Flag = new byte[] { 28, 28, 28, 28, 28, 28, 28, 28 };
        public int DestinationAddress { get; init; }
        public int SourceAddress { get; init; }
        public byte[] Data = new byte[29]; // Убедитесь, что размер соответствует вашим требованиям
        public byte FCS { get; init; }

        public byte[] ToBytes()
        {
            var stuffedData = BitStuff(Data);
            var packetLength = Flag.Length + 4 + 4 + stuffedData.Length + 1;
            var bytes = new byte[packetLength];

            Array.Copy(Flag, 0, bytes, 0, Flag.Length); 
            BitConverter.GetBytes(DestinationAddress).CopyTo(bytes, Flag.Length); 
            BitConverter.GetBytes(SourceAddress).CopyTo(bytes, Flag.Length + 4);
            stuffedData.CopyTo(bytes, Flag.Length + 8); 
            bytes[^1] = FCS;

            return bytes;
        }

        public static Packet FromBytes(byte[] bytes)
        {
            var flag = new byte[8];
            Array.Copy(bytes, 0, flag, 0, flag.Length);
            var destinationAddress = BitConverter.ToInt32(bytes, flag.Length);
            var sourceAddress = BitConverter.ToInt32(bytes, flag.Length + 4);
            var dataLength = bytes.Length - (flag.Length + 2 * sizeof(int) + 1);
            var data = new byte[dataLength];
            Array.Copy(bytes, flag.Length + 2 * sizeof(int), data, 0, dataLength);
            var fcs = bytes[^1];

            return new Packet
            {
                Flag = flag,
                DestinationAddress = destinationAddress,
                SourceAddress = sourceAddress,
                Data = BitUnstuff(data), // Применяем бит-унстаффинг к данным
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
