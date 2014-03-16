using System;

namespace CommonLib
{
    /// <summary>
    /// Generic byte extension functions
    /// </summary>
    public static class ByteExtension
    {

        public static byte[] SubArray(this byte[] mydata, int index, int count)
        {
            if (count >= mydata.Length) return mydata;
            byte[] message = new byte[count];
            Buffer.BlockCopy(mydata, index, message, 0, count);
            return message;
        }

        public static byte[] Concat(this byte[] data, byte[] val)
        {
            byte[] newdata = new byte[data.Length + val.Length];
            Buffer.BlockCopy(data, 0, newdata, 0, data.Length);
            Buffer.BlockCopy(val, 0, newdata, data.Length, val.Length);
            return newdata;
        }
    }
}