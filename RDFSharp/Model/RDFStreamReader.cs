/*
   Copyright 2012-2025 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.IO;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFStreamReader wraps a StreamReader with a sliding buffer for efficient reading of large streams
    /// </summary>
    internal class RDFStreamReader : IDisposable
    {
        #region Fields
        private readonly StreamReader _reader;
        private readonly char[] _buffer;
        private readonly int _bufferSize;

        private int _bufferStart;      // Absolute file position of the first character in the buffer
        private int _bufferLength;     // Number of valid characters in the buffer
        private bool _endOfStream;
        #endregion

        #region Properties
        internal int Position { get; set; }

        private bool IsEndOfFile
            => _endOfStream && Position >= _bufferStart + _bufferLength;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a RDFStreamReader on the given StreamReader with the given buffer size
        /// </summary>
        public RDFStreamReader(StreamReader reader, int bufferSize=8192)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferSize = bufferSize;
            _buffer = new char[bufferSize];
            _bufferStart = 0;
            _bufferLength = 0;
            _endOfStream = false;
            Position = 0;

            FillBuffer();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the RDFStreamReader
        /// </summary>
        public void Dispose()
            => _reader?.Dispose();
        #endregion

        #region Methods
        /// <summary>
        /// Reads the next Unicode codepoint
        /// </summary>
        public int Read()
        {
            if (IsEndOfFile)
                return -1;

            // Ensure that the current character is in the buffer
            // and, if possibile, feed the buffer with new data
            EnsureBufferContainsPosition();

            // Update position after buffering of new data
            if (Position >= _bufferStart + _bufferLength)
                return -1; // EOF
            int bufferIndex = Position - _bufferStart;
            char highSurrogate = _buffer[bufferIndex];
            Position++;

            // Handle eventual presence of surrogate pairs
            if (char.IsHighSurrogate(highSurrogate))
            {
                EnsureBufferContainsPosition();
                if (Position < _bufferStart + _bufferLength)
                {
                    bufferIndex = Position - _bufferStart;
                    char lowSurrogate = _buffer[bufferIndex];
                    if (char.IsLowSurrogate(lowSurrogate))
                    {
                        Position++;
                        return char.ConvertToUtf32(highSurrogate, lowSurrogate);
                    }
                }
            }

            return highSurrogate;
        }

        /// <summary>
        /// Peeks at the next code point without advancing position
        /// </summary>
        public int Peek()
        {
            int currentPos = Position;
            int codePoint = Read();
            Position = currentPos; // Restore the position
            return codePoint;
        }

        /// <summary>
        /// Goes back one codepoint (or 2, depending if it represents a surrogate pair)
        /// </summary>
        public void Unread(int codePoint)
        {
            if (codePoint == -1)
                return;

            // Surrogate character (represented in UTF-16 as pair of 2 chars): move back 2 positions
            if ((codePoint & ~char.MaxValue) != 0)
                Position = Math.Max(0, Position - 2);

            // Normal character: move back 1 position
            else
                Position = Math.Max(0, Position - 1);
        }

        /// <summary>
        /// Goes back the given string of characters
        /// </summary>
        public void Unread(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            // Go back character by character (starting from the last)
            for (int i = str.Length - 1; i >= 0; i--)
                Unread(str[i]);
        }

        /// <summary>
        /// Ensures that the buffer contains the current position
        /// </summary>
        private void EnsureBufferContainsPosition()
        {
            // If position is beyond the end of the current buffer, load a new buffer
            if (Position >= _bufferStart + _bufferLength && !_endOfStream)
                FillBuffer();

            // If position is before the start of the buffer, we need to handle backward seek
            else if (Position < _bufferStart)
                ReloadFromPosition(Position);
        }

        /// <summary>
        /// Fills the buffer from the current position
        /// </summary>
        private void FillBuffer()
        {
            if (_endOfStream)
                return;

            // If we are at the end of the current buffer, shift the file position of the buffer
            if (Position >= _bufferStart + _bufferLength)
                _bufferStart = Position;

            // Read the next block of data from the stream
            _bufferLength = _reader.Read(_buffer, 0, _bufferSize);
            if (_bufferLength == 0)
                _endOfStream = true;
        }

        /// <summary>
        /// Reloads the buffer from a specific location (for backward seeks)
        /// </summary>
        private void ReloadFromPosition(int position)
        {
            // Reset the stream to begin from the given position
            if (_reader.BaseStream.CanSeek)
            {
                _reader.BaseStream.Seek(0, SeekOrigin.Begin);
                _reader.DiscardBufferedData();
                _bufferStart = 0;
                Position = 0;
                _endOfStream = false;

                // Advance to the desired position
                while (Position < position && !IsEndOfFile)
                    Read();
                Position = position;
            }
            else
            {
                throw new InvalidOperationException("Cannot seek backward on non-seekable stream");
            }
        }
        #endregion
    }
}