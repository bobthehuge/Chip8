namespace Emulator
{
    public class Helpers
    {
        public static byte GetNibble(ushort instruction, int n)
        {
            if (n == 1)
                return (byte)((instruction & 0xF000) >> 12);

            if (n == 2)
                return (byte)((instruction & 0x0F00) >> 8);

            if (n == 3)
                return (byte)((instruction & 0x00F0) >> 4);

            if (n == 4)
                return (byte)(instruction & 0x000F);

            return 0;
        }

        public static byte GetN(ushort instruction)
        {
            return (byte)(instruction & 0x000F);
        }

        public static byte GetNN(ushort instruction)
        {
            return (byte)(instruction & 0x00FF);
        }

        public static ushort GetNNN(ushort instruction)
        {
            return (ushort)(instruction & 0x0FFF);
        }

        public static byte GetX(ushort instruction)
        {
            return GetNibble(instruction, 2);
        }

        public static byte GetY(ushort instruction)
        {
            return GetNibble(instruction, 3);
        }
    }
}