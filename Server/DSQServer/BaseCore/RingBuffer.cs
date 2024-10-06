using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCore
{
    /// <summary>
    /// 环形缓冲区
    /// </summary>
    public class RingBuffer
    {
        /// <summary>
        /// 向环形缓冲区写入一段字节数据
        /// </summary>
        /// <param name="bytes">将要被写入的字节数组</param>
        /// <param name="offset">将要写入的的首个字节在输入字节数组中的位置偏移</param>
        /// <param name="count">要写入的字节长度</param>
        /// <returns>写入成功，返回ture 写入失败，返回false（只有环形缓冲器可用容量不足时，才会写入失败）</returns>  
        public bool Write(byte[] bytes, int offset, int count)
        {
            // 环形缓冲区中当前可用的空间大小
            int remainCount = rawBuffer.Length - dataCount;

            if (remainCount >= count)
            {
                if (endIndex + count <= rawBuffer.Length)
                {
                    // 说明没有到rawBuffer数组的结尾

                    Array.Copy(bytes, offset, rawBuffer, endIndex, count);
                    endIndex += count;
                    if (endIndex == rawBuffer.Length)
                    {
                        endIndex = 0;
                    }

                    dataCount += count;
                }
                else
                {
                    // 说明数据结束索引超出rawBuffer数组的结尾

                    // rawBuffer数组的结尾可以追加的长度
                    int appendCount = rawBuffer.Length - endIndex;

                    // 超出了rawBuffer数组的结尾，需要放到数组头部的长度
                    int overflowCount = endIndex + count - rawBuffer.Length;

                    Array.Copy(bytes, offset, rawBuffer, endIndex, appendCount);

                    Array.Copy(bytes, offset + appendCount, rawBuffer, 0, overflowCount);

                    endIndex = overflowCount;
                    dataCount += count;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 从环形缓冲区中读取一段字节数据
        /// </summary>
        /// <param name="count">请求读取的长度</param>
        /// <param name="outBytes">用来接收读取的数据的字节数组，由调用者提供</param>
        /// <param name="clear">如果读取成功，是否要把环形缓冲区中读取的那一段清除掉，默认是true，是要清除掉</param>
        /// <returns>读取成功，返回true 读取失败，返回false</returns>  
        public bool Read(int count, byte[] outBytes, bool clear = true)
        {
            if (count > dataCount)
            {
                return false;
            }

            if (startIndex + count <= rawBuffer.Length)
            {
                // 说明读取没有超过rawBuffer的索引

                Array.Copy(rawBuffer, startIndex, outBytes, 0, count);

                if (clear == true)
                {
                    startIndex += count;
                    if (startIndex == rawBuffer.Length)
                    {
                        startIndex = 0;
                    }

                    dataCount -= count;
                }
            }
            else
            {
                // 说明读取超过了rawBuffer的索引，需要在读完rawBuffer的尾部后，进一步回到rawBuffer头部继续读

                // rawBuffer的尾部剩余可以读取的长度
                int appendCount = rawBuffer.Length - startIndex;

                // rawBuffer的头部还需要追加读取的长度
                int overflowCount = startIndex + count - rawBuffer.Length;

                Array.Copy(rawBuffer, startIndex, outBytes, 0, appendCount);

                Array.Copy(rawBuffer, 0, outBytes, appendCount, overflowCount);

                if (clear == true)
                {
                    startIndex = overflowCount;
                    dataCount -= count;
                }
            }

            return true;
        }

        /// <summary>
        /// 环形缓冲区的构造函数
        /// </summary>
        /// <param name="bufferSize">缓冲区的容量</param>
        public RingBuffer(int bufferSize)
        {
            rawBuffer = new byte[bufferSize];
            startIndex = 0;
            endIndex = 0;
            dataCount = 0;
        }

        public int DataCount
        {
            get { return dataCount; }
        }

        /// <summary>
        /// 有效数据的起始位置
        /// </summary>
        private int startIndex;

        /// <summary>
        /// 有效数据的结束位置,不包含这个位置本身
        /// </summary>
        private int endIndex;

        /// <summary>
        /// 有效数据的长度
        /// </summary>
        private int dataCount;

        /// <summary>
        /// 内部用来存储数据的原始byte数组
        /// </summary>
        private byte[] rawBuffer;
    }
}