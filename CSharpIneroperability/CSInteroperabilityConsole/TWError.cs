using System.Runtime.InteropServices;

namespace CSInteroperabilityConsole
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TWError
    {
        public char* Message;
        public TWErrorCategory Category;
    }
}