namespace Labs
{
    public class Frame
    {
        public byte[] Flag = new byte[] { 28, 28, 28, 28, 28, 28, 28, 28 };
        public int DestinationAddress { get; init; }
        public int SourceAddress { get; init; }
        public byte[] Data = new byte[28];
        public byte FCS { get; set; }
        
        public byte[] ToBytes()
        {
            FCS = CalculateHammingCode(Data);
            var stuffedData = BitStuff(Data);
            var packetLength = Flag.Length + 4 + 4 + stuffedData.Length + 1; 
            var bytes = new byte[packetLength];

            Array.Copy(Flag, 0, bytes, 0, Flag.Length);
            BitConverter.GetBytes(DestinationAddress).CopyTo(bytes, Flag.Length);
            BitConverter.GetBytes(SourceAddress).CopyTo(bytes, Flag.Length + 4);
            stuffedData.CopyTo(bytes, Flag.Length + 8);
            bytes[bytes.Length - 1] = FCS; // Place the FCS at the end

            return bytes;
        }
        
        public static Frame FromBytes(byte[] bytes)
        {
            var flag = new byte[8];
            Array.Copy(bytes, 0, flag, 0, flag.Length);
            var destinationAddress = BitConverter.ToInt32(bytes, flag.Length);
            var sourceAddress = BitConverter.ToInt32(bytes, flag.Length + 4);
            var dataLength = bytes.Length - (flag.Length + 2 * sizeof(int) + 1);
            var data = new byte[dataLength];
            Array.Copy(bytes, flag.Length + 2 * sizeof(int), data, 0, dataLength);
            var receivedFCS = bytes[bytes.Length - 1];
            
            var unstuffedData = BitUnstuff(data);
            var valid = ValidateHammingCode(unstuffedData, receivedFCS);

            return new Frame
            {
                Flag = flag,
                DestinationAddress = destinationAddress,
                SourceAddress = sourceAddress,
                Data = unstuffedData,
                FCS = receivedFCS
            };
        }
        
        public static byte CalculateHammingCode(byte[] data)
        {
            int dataBits = data.Length * 8;
            int parityBits = 0;

            while (Math.Pow(2, parityBits) < dataBits + parityBits + 1)
                parityBits++;

            int totalBits = dataBits + parityBits;
            List<bool> hammingCode = new List<bool>(totalBits);
            
            for (int i = 0; i < totalBits; i++)
            {
                if (IsPowerOfTwo(i + 1)) 
                {
                    hammingCode.Add(false); 
                }
                else
                {
                    int dataBitIndex = i - CountPowersOfTwo(i + 1);
                    hammingCode.Add(GetBit(data, dataBitIndex));
                }
            }

            // Calculate the parity bits
            for (int i = 0; i < parityBits; i++)
            {
                int position = (int)Math.Pow(2, i);
                bool parity = false;

                for (int j = 1; j <= totalBits; j++)
                {
                    if ((j & position) == position) // Check if j has the parity position bit set
                    {
                        parity ^= hammingCode[j - 1]; // XOR the bits controlled by this parity bit
                    }
                }

                hammingCode[position - 1] = parity; // Set the calculated parity bit
            }

            // Convert the List<bool> back to a single byte for FCS
            byte fcs = 0;
            for (int i = 0; i < hammingCode.Count; i++)
            {
                if (hammingCode[i])
                {
                    fcs |= (byte)(1 << i);
                }
            }

            return fcs;
        }

        // Check if a number is a power of two
        private static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        // Count how many powers of two are less than or equal to a number
        private static int CountPowersOfTwo(int number)
        {
            int count = 0;
            while (number > 0)
            {
                if (IsPowerOfTwo(number))
                {
                    count++;
                }
                number--;
            }
            return count;
        }

        // Get a specific bit from a byte array
        private static bool GetBit(byte[] data, int index)
        {
            if (index < 0 || index >= data.Length * 8)
                return false;

            int byteIndex = index / 8;
            int bitIndex = index % 8;
            return (data[byteIndex] & (1 << bitIndex)) != 0;
        }

        // Validate Hamming code and correct errors
        public static bool ValidateHammingCode(byte[] data, byte receivedFCS)
        {
            int errorPosition = 0;
            int parityBits = 8; // We only need to check 8 bits in FCS

            for (int i = 0; i < parityBits; i++)
            {
                byte expectedParity = CalculateParityBit(data, (int)Math.Pow(2, i) - 1);
                byte actualParity = (byte)((receivedFCS >> i) & 1);
                if (expectedParity != actualParity)
                {
                    errorPosition += (int)Math.Pow(2, i);
                }
            }

            if (errorPosition > 0)
            {
                if (errorPosition <= data.Length * 8)
                {
                    // Correct single error
                    int bytePos = (errorPosition - 1) / 8;
                    int bitPos = (errorPosition - 1) % 8;
                    data[bytePos] ^= (byte)(1 << bitPos);
                    return true; // Error corrected
                }
                else
                {
                    return false; // Double error, cannot correct
                }
            }

            return true; // No errors
        }

        // Calculate a single parity bit
        public static byte CalculateParityBit(byte[] data, int position)
        {
            int parity = 0;
            for (int i = 0; i < data.Length * 8; i++)
            {
                if (((i + 1) & (position + 1)) != 0)
                {
                    if (((data[i / 8] >> (i % 8)) & 1) == 1)
                    {
                        parity ^= 1;
                    }
                }
            }
            return (byte)parity;
        }


        public static byte[] CorruptData(byte[] data)
        {
            Random rand = new Random();
            byte[] corruptedData = new byte[data.Length];
            Array.Copy(data, corruptedData, data.Length);

            // 60% chance of one error
            if (rand.NextDouble() < 0.60)
            {
                int bytePos = rand.Next(data.Length);
                int bitPos = rand.Next(8);
                corruptedData[bytePos] ^= (byte)(1 << bitPos);
            }

            // 25% chance of two errors
            if (rand.NextDouble() < 0.25)
            {
                int bytePos1 = rand.Next(data.Length);
                int bitPos1 = rand.Next(8);
                corruptedData[bytePos1] ^= (byte)(1 << bitPos1);

                int bytePos2 = rand.Next(data.Length);
                int bitPos2 = rand.Next(8);
                corruptedData[bytePos2] ^= (byte)(1 << bitPos2);
            }

            return corruptedData;
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
