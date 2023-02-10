namespace RB3EHelper
{
    class RB3E_EventHeader
    {
        public static int Size = 8;
        public uint ProtocolMagic;
        public byte ProtocolVersion;
        public byte PacketType;
        public byte PacketSize;
        public byte Platform;

        public RB3E_EventHeader(byte[] data, int start)
        {
            if (data.Length - start < Size)
                throw new Exception("Invalid byte array size given for RB3E_EventHeader constructor.");

            ProtocolMagic = Program.SwapBytes(BitConverter.ToUInt32(data, start));
            ProtocolVersion = data[start + 4];
            PacketType = data[start + 5];
            PacketSize = data[start + 6];
            Platform = data[start + 7];
        }
    }

    class RB3E_EventScore
    {
        public static int Size = 0x14;
        public uint TotalScore;
        public uint[] MemberScores;
        public byte Stars;

        public RB3E_EventScore(byte[] data, int start)
        {
            if (data.Length - start < Size)
                throw new Exception("Invalid byte array size given for RB3E_EventScore constructor.");

            TotalScore = Program.SwapBytes(BitConverter.ToUInt32(data, start));
            MemberScores = new uint[4];
            MemberScores[0] = Program.SwapBytes(BitConverter.ToUInt32(data, start + 0x4));
            MemberScores[1] = Program.SwapBytes(BitConverter.ToUInt32(data, start + 0x8));
            MemberScores[2] = Program.SwapBytes(BitConverter.ToUInt32(data, start + 0xc));
            MemberScores[3] = Program.SwapBytes(BitConverter.ToUInt32(data, start + 0x10));
            Stars = data[start + 0x14];
        }
    }

    class RB3E_EventStagekit
    {
        public static int Size = 2;
        public byte LeftChannel;
        public byte RightChannel;

        public RB3E_EventStagekit(byte[] data, int start)
        {
            if (data.Length - start < Size)
                throw new Exception("Invalid byte array size given for RB3E_EventStagekit constructor.");

            LeftChannel = data[start];
            RightChannel = data[start + 1];
        }
    }

    class RB3E_EventBandInfo
    {
        public static int Size = 0xC;
        public byte[] MemberExists;
        public byte[] Difficulty;
        public byte[] TrackType;

        public RB3E_EventBandInfo(byte[] data, int start)
        {
            if (data.Length - start < Size)
                throw new Exception("Invalid byte array size given for RB3E_EventScore constructor.");

            MemberExists = new byte[4];
            Difficulty = new byte[4];
            TrackType = new byte[4];
            Array.Copy(data, start + 0, MemberExists, 0, 4);
            Array.Copy(data, start + 4, Difficulty, 0, 4);
            Array.Copy(data, start + 8, TrackType, 0, 4);
        }
    }
}
