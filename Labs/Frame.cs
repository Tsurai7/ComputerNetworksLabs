namespace Labs
{
    public class Frame
    {
        public byte[] Flag = [ 28, 28, 28, 28, 28, 28, 28, 28 ];
        public int DestinationAddress { get; init; }
        public int SourceAddress { get; init; }
        public byte[] Data = new byte[28];
        public byte FCS { get; set; }
        
        public byte[] ToBytes()
        {
            FCS = CalculateHammingCode();
            var packetLength = Flag.Length + 4 + 4 + Data.Length + 1; 
            var bytes = new byte[packetLength];

            Array.Copy(Flag, 0, bytes, 0, Flag.Length);
            BitConverter.GetBytes(DestinationAddress).CopyTo(bytes, Flag.Length);
            BitConverter.GetBytes(SourceAddress).CopyTo(bytes, Flag.Length + 4);
            Data.CopyTo(bytes, Flag.Length + 8);
            bytes[^1] = FCS;

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

            return new Frame
            {
                Flag = flag,
                DestinationAddress = destinationAddress,
                SourceAddress = sourceAddress,
                Data = data,
                FCS = receivedFCS
            };
        }
        
        public byte CalculateHammingCode()
        {
            var dataBits = Data.Length * 8;
            var parityBits = 0;

            while (Math.Pow(2, parityBits) < dataBits + parityBits + 1)
                parityBits++;

            var totalBits = dataBits + parityBits;
            var hammingCode = new List<bool>(totalBits);
            
            for (int i = 0; i < totalBits; i++)
            {
                if (IsPowerOfTwo(i + 1)) 
                {
                    hammingCode.Add(false); 
                }
                else
                {
                    var dataBitIndex = i - CountPowersOfTwo(i + 1);
                    hammingCode.Add(GetBit(Data, dataBitIndex));
                }
            }

            // Calculate the parity bits
            for (var i = 0; i < parityBits; i++)
            {
                var position = (int)Math.Pow(2, i);
                var parity = false;

                for (var j = 1; j <= totalBits; j++)
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
            for (var i = 0; i < hammingCode.Count; i++)
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
            var count = 0;
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

            var byteIndex = index / 8;
            var bitIndex = index % 8;
            return (data[byteIndex] & (1 << bitIndex)) != 0;
        }
        
        public static bool ValidateHammingCode(byte[] data, byte receivedFCS)
        {
            var errorPosition = 0;
            var parityBits = 8; // We only need to check 8 bits in FCS

            for (int i = 0; i < parityBits; i++)
            {
                var expectedParity = CalculateParityBit(data, (int)Math.Pow(2, i) - 1);
                var actualParity = (byte)((receivedFCS >> i) & 1);
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
                return false; // Double error, cannot correct
            }

            return true; // No errors
        }
        
        private static byte CalculateParityBit(byte[] data, int position)
        {
            var parity = 0;
            for (var i = 0; i < data.Length * 8; i++)
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
            var rand = new Random();
            var corruptedData = new byte[data.Length];
            Array.Copy(data, corruptedData, data.Length);

            // 60% chance of one error
            if (rand.NextDouble() < 0.60)
            {
                int bytePos = rand.Next(data.Length);
                int bitPos = rand.Next(8);
                corruptedData[bytePos] ^= (byte)(1 << bitPos);
            }

            // 25% chance of two errors
            if (rand.NextDouble() > 0)
            {
                var bytePos1 = rand.Next(data.Length);
                var bitPos1 = rand.Next(8);
                corruptedData[bytePos1] ^= (byte)(1 << bitPos1);

                var bytePos2 = rand.Next(data.Length);
                var bitPos2 = rand.Next(8);
                corruptedData[bytePos2] ^= (byte)(1 << bitPos2);
            }

            return corruptedData;
        }
    }
}
