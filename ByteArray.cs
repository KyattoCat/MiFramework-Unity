using System;
using System.Text;

namespace MiFramework.Stream
{
    public class ByteArray
    {
        private const UInt32 NEG_FLAG = 0x80;
        private const UInt16 INT16_FLAG = 0x4000;
        private const UInt32 INT32_FLAG = 0x60000000;

        private const int ADAPT_INT8_MAXVALUE = 64;
        private const int ADAPT_INT16_MAXVALUE = 8192;
        private const int ADAPT_INT32_MAXVALUE = 536870912;

        private byte[] data;            // 
        private uint offset;            // 
        public uint Offset { get => offset; set => offset = value; }
        private uint maxCapacity = 16;  // data数组的大小
        private uint count;             // 当前ByteArray中有效位数
        public uint Count => count;

        public ByteArray()
        {
            data = new byte[maxCapacity];
        }

        public ByteArray(uint maxCapacity)
        {
            this.maxCapacity = maxCapacity;
            data = new byte[maxCapacity];
        }

        public void Clear()
        {
            offset = 0;
            count = 0;
        }

        public byte[] GetData()
        {
            byte[] result = new byte[count];
            Array.Copy(data, result, count);
            return result;
        }

        public void WriteTo(ByteArray destination)
        {
            destination.maxCapacity = maxCapacity;
            destination.count = count;
            destination.offset = 0;
            destination.data = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                destination.data[i] = data[i];
            }
        }

        private void ExpandCapacity()
        {
            ulong newCapacity = maxCapacity * 2;

            if (newCapacity > uint.MaxValue)
            {
                throw new OverflowException();
            }

            byte[] bytes = new byte[newCapacity];

            Array.Copy(data, bytes, maxCapacity);

            maxCapacity = (uint)newCapacity;

            data = bytes;
        }

        public void WriteByte(byte value)
        {
            if (offset + 1 > maxCapacity)
            {
                ExpandCapacity();
            }
            data[offset++] = value;
            count++;
        }

        public byte ReadByte()
        {
            if (offset + 1 > count)
            {
                throw new IndexOutOfRangeException();
            }
            return data[offset++];
        }

        public void WriteBytes(byte[] bytes, int index, int length)
        {
            for (int i = index; i < index + length; i++)
            {
                WriteByte(bytes[i]);
            }
        }

        private byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = ReadByte();
            }
            return bytes;
        }

        public void WriteUInt16(ushort value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public ushort ReadUInt16()
        {
            ushort result = 0;
            result |= (ushort)(ReadByte() << 8);
            result |= (ushort)(ReadByte() << 0);
            return result;
        }

        public void WriteUInt32(uint data)
        {
            WriteByte((byte)(data >> 24));
            WriteByte((byte)(data >> 16));
            WriteByte((byte)(data >> 8));
            WriteByte((byte)data);
        }

        public uint ReadUInt32()
        {
            uint result = 0;
            result |= (uint)(ReadByte() << 24);
            result |= (uint)(ReadByte() << 16);
            result |= (uint)(ReadByte() << 8);
            result |= (uint)(ReadByte() << 0);
            return result;
        }

        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public void WriteFloat(float data)
        {
            WriteBytes(BitConverter.GetBytes(data), 0, 4);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4));
        }

        public void WriteString(string data)
        {
            char[] charArray = data.ToCharArray();

            WriteIntAdaptive(charArray.Length);

            for (int i = 0; i < charArray.Length; i++)
            {
                WriteUInt16(charArray[i]);
            }
        }

        public string ReadString()
        {
            int length = ReadIntAdaptive();

            StringBuilder stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                char temp = (char)ReadUInt16();
                stringBuilder.Append(temp);
            }

            return stringBuilder.ToString();
        }

        public void WriteIntAdaptive(int data)
        {
            bool isNegative = data < 0;

            if (isNegative)
            {
                data = -data;
            }

            if (data < ADAPT_INT8_MAXVALUE)
            {
                byte temp = (byte)data;

                if (isNegative)
                    temp |= (byte)NEG_FLAG;

                WriteByte(temp);
            }
            else if (data < ADAPT_INT16_MAXVALUE)
            {
                ushort temp = (ushort)data;

                if (isNegative)
                    temp |= (ushort)(NEG_FLAG << 8);

                temp |= INT16_FLAG;
                WriteUInt16(temp);
            }
            else if (data < ADAPT_INT32_MAXVALUE)
            {
                uint temp = (uint)data;

                if (isNegative)
                    temp |= (uint)(NEG_FLAG << 24);

                temp |= INT32_FLAG;
                WriteUInt32(temp);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"data >= {ADAPT_INT32_MAXVALUE}(ADAPT_INT32_MAXVALUE)");
            }
        }

        public int ReadIntAdaptive()
        {
            byte temp1 = ReadByte();
            bool isPositive = (temp1 & NEG_FLAG) == 0;
            bool byteFlag = ((temp1 >> 6) & 1) == 0;
            int flag = (temp1 >> 5) & 0b11;

            int result = 0;
            if (byteFlag)
            {
                result = temp1 & 0x3F;
            }
            else if (flag == 0b10)
            {
                byte temp2 = ReadByte();

                result |= (temp1 & 0x1F) << 8;
                result |= temp2;
            }
            else if (flag == 0b11)
            {
                byte temp2 = ReadByte();
                byte temp3 = ReadByte();
                byte temp4 = ReadByte();

                result |= (temp1 & 0x1F) << 24;
                result |= temp2 << 16;
                result |= temp3 << 8;
                result |= temp4;
            }

            return isPositive ? result : -result;
        }
    }
}
