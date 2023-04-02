namespace Emulator
{
    public enum Debug
    {
        Instruction,
        Timer,
        Register,
        Stack,
        Input,
        Display,
        Memory,
        All
    }

    public enum EmulStatus
    {
        Exit,
        Idle,
        Running,
        Error,
    }
}