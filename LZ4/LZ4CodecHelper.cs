using System;
// ReSharper disable All

namespace Hi3Helper.UABT.LZ4
{
    public static partial class LZ4CodecHelper
    {
        private static readonly int[] DECODER_TABLE_32 = new int[8] { 0, 3, 2, 3, 0, 0, 0, 0 };

        private static readonly int[] DECODER_TABLE_64 = new int[8] { 0, 0, 0, -1, 0, 1, 2, 3 };

        internal static void CheckArguments(byte[] input, int inputOffset, ref int inputLength, byte[] output, int outputOffset, ref int outputLength)
        {
            if (inputLength < 0)
            {
                inputLength = input.Length - inputOffset;
            }
            if (inputLength == 0)
            {
                outputLength = 0;
                return;
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (inputOffset < 0 || inputOffset + inputLength > input.Length)
            {
                throw new ArgumentException("inputOffset and inputLength are invalid for given input");
            }
            if (outputLength < 0)
            {
                outputLength = output.Length - outputOffset;
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (outputOffset >= 0 && outputOffset + outputLength <= output.Length)
            {
                return;
            }
            throw new ArgumentException("outputOffset and outputLength are invalid for given output");
        }

        internal static ushort Peek2(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static void Copy4(byte[] buf, int src, int dst)
        {
            buf[dst + 3] = buf[src + 3];
            buf[dst + 2] = buf[src + 2];
            buf[dst + 1] = buf[src + 1];
            buf[dst] = buf[src];
        }

        private static void Copy8(byte[] buf, int src, int dst)
        {
            buf[dst + 7] = buf[src + 7];
            buf[dst + 6] = buf[src + 6];
            buf[dst + 5] = buf[src + 5];
            buf[dst + 4] = buf[src + 4];
            buf[dst + 3] = buf[src + 3];
            buf[dst + 2] = buf[src + 2];
            buf[dst + 1] = buf[src + 1];
            buf[dst] = buf[src];
        }

        private static void BlockCopy(byte[] src, int src_0, byte[] dst, int dst_0, int len)
        {
            if (len >= 16)
            {
                Buffer.BlockCopy(src, src_0, dst, dst_0, len);
                return;
            }
            while (len >= 8)
            {
                dst[dst_0] = src[src_0];
                dst[dst_0 + 1] = src[src_0 + 1];
                dst[dst_0 + 2] = src[src_0 + 2];
                dst[dst_0 + 3] = src[src_0 + 3];
                dst[dst_0 + 4] = src[src_0 + 4];
                dst[dst_0 + 5] = src[src_0 + 5];
                dst[dst_0 + 6] = src[src_0 + 6];
                dst[dst_0 + 7] = src[src_0 + 7];
                len -= 8;
                src_0 += 8;
                dst_0 += 8;
            }
            while (len >= 4)
            {
                dst[dst_0] = src[src_0];
                dst[dst_0 + 1] = src[src_0 + 1];
                dst[dst_0 + 2] = src[src_0 + 2];
                dst[dst_0 + 3] = src[src_0 + 3];
                len -= 4;
                src_0 += 4;
                dst_0 += 4;
            }
            while (len-- > 0)
            {
                dst[dst_0++] = src[src_0++];
            }
        }

        private static int WildCopy(byte[] src, int src_0, byte[] dst, int dst_0, int dst_end)
        {
            int num = dst_end - dst_0;
            if (num >= 16)
            {
                Buffer.BlockCopy(src, src_0, dst, dst_0, num);
            }
            else
            {
                while (num >= 4)
                {
                    dst[dst_0] = src[src_0];
                    dst[dst_0 + 1] = src[src_0 + 1];
                    dst[dst_0 + 2] = src[src_0 + 2];
                    dst[dst_0 + 3] = src[src_0 + 3];
                    num -= 4;
                    src_0 += 4;
                    dst_0 += 4;
                }
                while (num-- > 0)
                {
                    dst[dst_0++] = src[src_0++];
                }
            }
            return num;
        }

        private static int SecureCopy(byte[] buffer, int src, int dst, int dst_end)
        {
            int num = dst - src;
            int num2 = dst_end - dst;
            int num3 = num2;
            if (num >= 16)
            {
                if (num >= num2)
                {
                    Buffer.BlockCopy(buffer, src, buffer, dst, num2);
                    return num2;
                }
                do
                {
                    Buffer.BlockCopy(buffer, src, buffer, dst, num);
                    src += num;
                    dst += num;
                    num3 -= num;
                }
                while (num3 >= num);
            }
            while (num3 >= 4)
            {
                buffer[dst] = buffer[src];
                buffer[dst + 1] = buffer[src + 1];
                buffer[dst + 2] = buffer[src + 2];
                buffer[dst + 3] = buffer[src + 3];
                dst += 4;
                src += 4;
                num3 -= 4;
            }
            while (num3-- > 0)
            {
                buffer[dst++] = buffer[src++];
            }
            return num2;
        }

        public static int Decode32(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
        {
            CheckArguments(input, inputOffset, ref inputLength, output, outputOffset, ref outputLength);
            if (outputLength == 0)
            {
                return 0;
            }
            if (knownOutputLength)
            {
                if (LZ4_uncompress_safe32(input, output, inputOffset, outputOffset, outputLength) != inputLength)
                {
                    throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
                }
                return outputLength;
            }
            int num = LZ4_uncompress_unknownOutputSize_safe32(input, output, inputOffset, outputOffset, inputLength, outputLength);
            if (num < 0)
            {
                throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
            }
            return num;
        }

        public static int Decode64(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
        {
            CheckArguments(input, inputOffset, ref inputLength, output, outputOffset, ref outputLength);
            if (outputLength == 0)
            {
                return 0;
            }
            if (knownOutputLength)
            {
                if (LZ4_uncompress_safe64(input, output, inputOffset, outputOffset, outputLength) != inputLength)
                {
                    throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
                }
                return outputLength;
            }
            int num = LZ4_uncompress_unknownOutputSize_safe64(input, output, inputOffset, outputOffset, inputLength, outputLength);
            if (num < 0)
            {
                throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
            }
            return num;
        }

        public static byte[] Decode64(byte[] input, int inputOffset, int inputLength, int outputLength)
        {
            if (inputLength < 0)
            {
                inputLength = input.Length - inputOffset;
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (inputOffset < 0 || inputOffset + inputLength > input.Length)
            {
                throw new ArgumentException("inputOffset and inputLength are invalid for given input");
            }
            byte[] array = new byte[outputLength];
            if (Decode64(input, inputOffset, inputLength, array, 0, outputLength, true) != outputLength)
            {
                throw new ArgumentException("outputLength is not valid");
            }
            return array;
        }

        private static int LZ4_uncompress_safe32(byte[] src, byte[] dst, int src_0, int dst_0, int dst_len)
        {
            int[] dECODER_TABLE_ = DECODER_TABLE_32;
            int num = src_0;
            int num2 = dst_0;
            int num3 = num2 + dst_len;
            int num4 = num3 - 5;
            int num5 = num3 - 8;
            int num6 = num3 - 8;
            while (true)
            {
                byte b = src[num++];
                int num7;
                if ((num7 = b >> 4) == 15)
                {
                    int num8;
                    while ((num8 = src[num++]) == 255)
                    {
                        num7 += 255;
                    }
                    num7 += num8;
                }
                int num9 = num2 + num7;
                if (num9 > num5)
                {
                    if (num9 != num3)
                    {
                        break;
                    }
                    BlockCopy(src, num, dst, num2, num7);
                    num += num7;
                    return num - src_0;
                }
                if (num2 < num9)
                {
                    int num10 = WildCopy(src, num, dst, num2, num9);
                    num += num10;
                    num2 += num10;
                }
                num -= num2 - num9;
                num2 = num9;
                int num11 = num9 - Peek2(src, num);
                num += 2;
                if (num11 < dst_0)
                {
                    break;
                }
                if ((num7 = b & 0xF) == 15)
                {
                    while (src[num] == byte.MaxValue)
                    {
                        num++;
                        num7 += 255;
                    }
                    num7 += src[num++];
                }
                if (num2 - num11 < 4)
                {
                    dst[num2] = dst[num11];
                    dst[num2 + 1] = dst[num11 + 1];
                    dst[num2 + 2] = dst[num11 + 2];
                    dst[num2 + 3] = dst[num11 + 3];
                    num2 += 4;
                    num11 += 4;
                    num11 -= dECODER_TABLE_[num2 - num11];
                    Copy4(dst, num11, num2);
                }
                else
                {
                    Copy4(dst, num11, num2);
                    num2 += 4;
                    num11 += 4;
                }
                num9 = num2 + num7;
                if (num9 > num6)
                {
                    if (num9 > num4)
                    {
                        break;
                    }
                    if (num2 < num5)
                    {
                        int num10 = SecureCopy(dst, num11, num2, num5);
                        num11 += num10;
                        num2 += num10;
                    }
                    while (num2 < num9)
                    {
                        dst[num2++] = dst[num11++];
                    }
                    num2 = num9;
                }
                else
                {
                    if (num2 < num9)
                    {
                        SecureCopy(dst, num11, num2, num9);
                    }
                    num2 = num9;
                }
            }
            return -(num - src_0);
        }

        private static int LZ4_uncompress_unknownOutputSize_safe32(byte[] src, byte[] dst, int src_0, int dst_0, int src_len, int dst_maxlen)
        {
            int[] dECODER_TABLE_ = DECODER_TABLE_32;
            int num = src_0;
            int num2 = num + src_len;
            int num3 = dst_0;
            int num4 = num3 + dst_maxlen;
            int num5 = num2 - 8;
            int num6 = num2 - 6;
            int num7 = num4 - 8;
            int num8 = num4 - 8;
            int num9 = num4 - 5;
            int num10 = num4 - 12;
            if (num != num2)
            {
                while (true)
                {
                    byte b = src[num++];
                    int num11;
                    if ((num11 = b >> 4) == 15)
                    {
                        int num12 = 255;
                        while (num < num2 && num12 == 255)
                        {
                            num11 += (num12 = src[num++]);
                        }
                    }
                    int num13 = num3 + num11;
                    if (num13 > num10 || num + num11 > num5)
                    {
                        if (num13 > num4 || num + num11 != num2)
                        {
                            break;
                        }
                        BlockCopy(src, num, dst, num3, num11);
                        num3 += num11;
                        return num3 - dst_0;
                    }
                    if (num3 < num13)
                    {
                        int num14 = WildCopy(src, num, dst, num3, num13);
                        num += num14;
                        num3 += num14;
                    }
                    num -= num3 - num13;
                    num3 = num13;
                    int num15 = num13 - Peek2(src, num);
                    num += 2;
                    if (num15 < dst_0)
                    {
                        break;
                    }
                    if ((num11 = b & 0xF) == 15)
                    {
                        while (num < num6)
                        {
                            int num16 = src[num++];
                            num11 += num16;
                            if (num16 != 255)
                            {
                                break;
                            }
                        }
                    }
                    if (num3 - num15 < 4)
                    {
                        dst[num3] = dst[num15];
                        dst[num3 + 1] = dst[num15 + 1];
                        dst[num3 + 2] = dst[num15 + 2];
                        dst[num3 + 3] = dst[num15 + 3];
                        num3 += 4;
                        num15 += 4;
                        num15 -= dECODER_TABLE_[num3 - num15];
                        Copy4(dst, num15, num3);
                    }
                    else
                    {
                        Copy4(dst, num15, num3);
                        num3 += 4;
                        num15 += 4;
                    }
                    num13 = num3 + num11;
                    if (num13 > num8)
                    {
                        if (num13 > num9)
                        {
                            break;
                        }
                        if (num3 < num7)
                        {
                            int num14 = SecureCopy(dst, num15, num3, num7);
                            num15 += num14;
                            num3 += num14;
                        }
                        while (num3 < num13)
                        {
                            dst[num3++] = dst[num15++];
                        }
                        num3 = num13;
                    }
                    else
                    {
                        if (num3 < num13)
                        {
                            SecureCopy(dst, num15, num3, num13);
                        }
                        num3 = num13;
                    }
                }
            }
            return -(num - src_0);
        }

        private static int LZ4_uncompress_safe64(byte[] src, byte[] dst, int src_0, int dst_0, int dst_len)
        {
            int[] dECODER_TABLE_ = DECODER_TABLE_32;
            int[] dECODER_TABLE_2 = DECODER_TABLE_64;
            int num = src_0;
            int num2 = dst_0;
            int num3 = num2 + dst_len;
            int num4 = num3 - 5;
            int num5 = num3 - 8;
            int num6 = num3 - 8 - 4;
            while (true)
            {
                uint num7 = src[num++];
                int num8;
                if ((num8 = (byte)(num7 >> 4)) == 15)
                {
                    int num9;
                    while ((num9 = src[num++]) == 255)
                    {
                        num8 += 255;
                    }
                    num8 += num9;
                }
                int num10 = num2 + num8;
                if (num10 > num5)
                {
                    if (num10 != num3)
                    {
                        break;
                    }
                    BlockCopy(src, num, dst, num2, num8);
                    num += num8;
                    return num - src_0;
                }
                if (num2 < num10)
                {
                    int num11 = WildCopy(src, num, dst, num2, num10);
                    num += num11;
                    num2 += num11;
                }
                num -= num2 - num10;
                num2 = num10;
                int num12 = num10 - Peek2(src, num);
                num += 2;
                if (num12 < dst_0)
                {
                    break;
                }
                if ((num8 = (byte)(num7 & 0xF)) == 15)
                {
                    while (src[num] == byte.MaxValue)
                    {
                        num++;
                        num8 += 255;
                    }
                    num8 += src[num++];
                }
                if (num2 - num12 < 8)
                {
                    int num13 = dECODER_TABLE_2[num2 - num12];
                    dst[num2] = dst[num12];
                    dst[num2 + 1] = dst[num12 + 1];
                    dst[num2 + 2] = dst[num12 + 2];
                    dst[num2 + 3] = dst[num12 + 3];
                    num2 += 4;
                    num12 += 4;
                    num12 -= dECODER_TABLE_[num2 - num12];
                    Copy4(dst, num12, num2);
                    num2 += 4;
                    num12 -= num13;
                }
                else
                {
                    Copy8(dst, num12, num2);
                    num2 += 8;
                    num12 += 8;
                }
                num10 = num2 + num8 - 4;
                if (num10 > num6)
                {
                    if (num10 > num4)
                    {
                        break;
                    }
                    if (num2 < num5)
                    {
                        int num11 = SecureCopy(dst, num12, num2, num5);
                        num12 += num11;
                        num2 += num11;
                    }
                    while (num2 < num10)
                    {
                        dst[num2++] = dst[num12++];
                    }
                    num2 = num10;
                }
                else
                {
                    if (num2 < num10)
                    {
                        SecureCopy(dst, num12, num2, num10);
                    }
                    num2 = num10;
                }
            }
            return -(num - src_0);
        }

        private static int LZ4_uncompress_unknownOutputSize_safe64(byte[] src, byte[] dst, int src_0, int dst_0, int src_len, int dst_maxlen)
        {
            int[] dECODER_TABLE_ = DECODER_TABLE_32;
            int[] dECODER_TABLE_2 = DECODER_TABLE_64;
            int num = src_0;
            int num2 = num + src_len;
            int num3 = dst_0;
            int num4 = num3 + dst_maxlen;
            int num5 = num2 - 8;
            int num6 = num2 - 6;
            int num7 = num4 - 8;
            int num8 = num4 - 12;
            int num9 = num4 - 5;
            int num10 = num4 - 12;
            if (num != num2)
            {
                while (true)
                {
                    byte b = src[num++];
                    int num11;
                    if ((num11 = b >> 4) == 15)
                    {
                        int num12 = 255;
                        while (num < num2 && num12 == 255)
                        {
                            num11 += (num12 = src[num++]);
                        }
                    }
                    int num13 = num3 + num11;
                    if (num13 > num10 || num + num11 > num5)
                    {
                        if (num13 > num4 || num + num11 != num2)
                        {
                            break;
                        }
                        BlockCopy(src, num, dst, num3, num11);
                        num3 += num11;
                        return num3 - dst_0;
                    }
                    if (num3 < num13)
                    {
                        int num14 = WildCopy(src, num, dst, num3, num13);
                        num += num14;
                        num3 += num14;
                    }
                    num -= num3 - num13;
                    num3 = num13;
                    int num15 = num13 - Peek2(src, num);
                    num += 2;
                    if (num15 < dst_0)
                    {
                        break;
                    }
                    if ((num11 = b & 0xF) == 15)
                    {
                        while (num < num6)
                        {
                            int num16 = src[num++];
                            num11 += num16;
                            if (num16 != 255)
                            {
                                break;
                            }
                        }
                    }
                    if (num3 - num15 < 8)
                    {
                        int num17 = dECODER_TABLE_2[num3 - num15];
                        dst[num3] = dst[num15];
                        dst[num3 + 1] = dst[num15 + 1];
                        dst[num3 + 2] = dst[num15 + 2];
                        dst[num3 + 3] = dst[num15 + 3];
                        num3 += 4;
                        num15 += 4;
                        num15 -= dECODER_TABLE_[num3 - num15];
                        Copy4(dst, num15, num3);
                        num3 += 4;
                        num15 -= num17;
                    }
                    else
                    {
                        Copy8(dst, num15, num3);
                        num3 += 8;
                        num15 += 8;
                    }
                    num13 = num3 + num11 - 4;
                    if (num13 > num8)
                    {
                        if (num13 > num9)
                        {
                            break;
                        }
                        if (num3 < num7)
                        {
                            int num14 = SecureCopy(dst, num15, num3, num7);
                            num15 += num14;
                            num3 += num14;
                        }
                        while (num3 < num13)
                        {
                            dst[num3++] = dst[num15++];
                        }
                        num3 = num13;
                    }
                    else
                    {
                        if (num3 < num13)
                        {
                            SecureCopy(dst, num15, num3, num13);
                        }
                        num3 = num13;
                    }
                }
            }
            return -(num - src_0);
        }
    }
}
