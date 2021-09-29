using System.Runtime.CompilerServices;

namespace ZargoEngine
{
    public static unsafe class Bytes
    {
        public static byte[] GetBytes<T>(this ref T value) where T : unmanaged
        {
            byte[] bytes = new byte[Unsafe.SizeOf<T>()];
            fixed (void* pointer = &bytes[0])
                Unsafe.CopyBlock(pointer, Unsafe.AsPointer(ref value), (uint)Unsafe.SizeOf<T>());
            return bytes;
        }

        public static T BytesTo<T>(this byte[] bytes) where T : unmanaged
        {
            T result = new ();
            fixed(void* pointer = &bytes[0])
            Unsafe.CopyBlock(&result, pointer, (uint)Unsafe.SizeOf<T>());
            return result;
        }
    }
}
