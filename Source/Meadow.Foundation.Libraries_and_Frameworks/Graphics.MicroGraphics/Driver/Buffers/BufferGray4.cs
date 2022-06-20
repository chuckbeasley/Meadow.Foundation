﻿using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 4bpp pixel buffer
    /// </summary>
    public class BufferGray4 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorType ColorMode => ColorType.Format4bppGray;

        public BufferGray4(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferGray4(int width, int height) : base(width, height) { }

        public override void Fill(Color color)
        {
            // split the color in to two byte values
            Buffer[0] = (byte)(color.Color4bppGray | color.Color4bppGray << 4);

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        public override void Fill(int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x + width > Width ||
                y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            //TODO optimize
            var bColor = color.Color4bppGray;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, bColor);
                }
            }
        }
		
        public override Color GetPixel(int x, int y)
        {   //comes back as a 4bit value
            var gray = GetPixel4bpp(x, y);

            return new Color(gray << 4, gray << 4, gray << 4);
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color4bppGray);
        }
				
        public void SetPixel(int x, int y, byte gray)
        {
            int index = y * Width / 2 + x / 2;

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                Buffer[index] = (byte)((Buffer[index] & 0x0f) | (gray << 4));
            }
            else
            {   //odd pixel
                Buffer[index] = (byte)((Buffer[index] & 0xf0) | (gray));
            }
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a buffer to specific location to the current buffer
        /// </summary>
        /// <param name="x">x origin</param>
        /// <param name="y">y origin</param>
        /// <param name="buffer">buffer to write</param>
        public override void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            if (buffer.ColorMode == ColorMode &&
                x % 2 == 0 && 
                buffer.Width % 2 == 0)
            {
                //we have a happy path
                int sourceIndex, destinationIndex;
                int length = buffer.Width / 2;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) >> 2; //divide by 2

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length);
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }

        public byte GetPixel4bpp(int x, int y)
        {
            int index = y * Width / 2 + x / 2;
            byte color;

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                color = (byte)((Buffer[index] & 0x0f) >> 4);
            }
            else
            {   //odd pixel
                color = (byte)((Buffer[index] & 0xf0));
            }
            return color; 
        }
    }
}