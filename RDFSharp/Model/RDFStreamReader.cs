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
using System.Collections.Generic;
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

        private int _bufferStart;      // Absolute position of the first character in the buffer
        private int _bufferLength;     // Number of valid characters in the buffer
        private bool _endOfStream;

        // Cache to handle backward seek without having to reseek on the stream
        private readonly List<char> _readHistory;
        private const int MAX_HISTORY_SIZE = 1024;
        #endregion

        #region Properties
        /// <summary>
        /// Indicates the current position into the buffer
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Flag indicating that we have reached the end of file
        /// </summary>
        public bool IsEndOfFile => _endOfStream && Position >= _bufferStart + _bufferLength;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a RDFStreamReader on the given StreamReader with the given buffer size
        /// </summary>
        internal RDFStreamReader(StreamReader reader, int bufferSize=16384)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferSize = bufferSize;
            _buffer = new char[bufferSize];
            _bufferStart = 0;
            _bufferLength = 0;
            _endOfStream = false;
            _readHistory = new List<char>(MAX_HISTORY_SIZE);
            Position = 0;

            // Fetch the first block
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
        public int ReadCodePoint()
        {
            if (IsEndOfFile)
                return -1;

            // Ensure that the current character is in the buffer
            // and, if possible, feed the buffer with new data
            EnsureBufferContainsPosition();

            // Update position after buffering of new data
            if (Position >= _bufferStart + _bufferLength)
                return -1; // EOF
            int bufferIndex = Position - _bufferStart;
            char highSurrogate = _buffer[bufferIndex];

            // Also update backward seek history
            AddToHistory(highSurrogate);
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
                        AddToHistory(lowSurrogate);
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
        public int PeekCodePoint()
        {
            // Grab the next character without impacting the position
            int currentPos = Position;
            int codePoint = ReadCodePoint();
            Position = currentPos;

            // Remove from seek history the character we have peeked
            if (codePoint != -1)
            {
                // Supplementary character (UTF16 surrogate pair): remove 2 chars from the seek history
                if ((codePoint & ~char.MaxValue) != 0)
                {
                    if (_readHistory.Count >= 2)
                        _readHistory.RemoveRange(_readHistory.Count - 2, 2);
                }

                //Normal character (UTF8): remove 1 char from the seek history
                else
                {
                    if (_readHistory.Count >= 1)
                        _readHistory.RemoveAt(_readHistory.Count - 1);
                }
            }

            return codePoint;
        }

        /// <summary>
        /// Goes back the given codepoint
        /// </summary>
        public void UnreadCodePoint(int codePoint)
        {
            if (codePoint == -1)
                return;

            // Supplementary character (UTF16 surrogate pair): go back 2 positions
            if ((codePoint & ~char.MaxValue) != 0)
            {
                Position = Math.Max(0, Position - 2);
                // Remove 2 chars from seek history
                if (_readHistory.Count >= 2)
                    _readHistory.RemoveRange(_readHistory.Count - 2, 2);
            }

            //Normal character (UTF8): go back 1 position
            else
            {
                Position = Math.Max(0, Position - 1);
                // Remove 1 char from seek history
                if (_readHistory.Count >= 1)
                    _readHistory.RemoveAt(_readHistory.Count - 1);
            }
        }

        /// <summary>
        /// Goes back the given string of characters
        /// </summary>
        public void UnreadString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            // Compute hoiw many characters must go back
            int charsToUnread = str.Length;

            // Update the position
            Position = Math.Max(0, Position - charsToUnread);

            // Remove from the seek history the correct number of characters
            int historyToRemove = Math.Min(charsToUnread, _readHistory.Count);
            if (historyToRemove > 0)
                _readHistory.RemoveRange(_readHistory.Count - historyToRemove, historyToRemove);
        }

        /// <summary>
        /// Ensures that the buffer contains the current position
        /// </summary>
        private void EnsureBufferContainsPosition()
        {
            // If the position exceeds the end of the buffer, load new buffer
            if (Position >= _bufferStart + _bufferLength && !_endOfStream)
            {
                FillBuffer();
                return;
            }

            // If the position is before the start of the buffer, exploit the cache if possible
            if (Position < _bufferStart)
            {
                // If we can seek beyond the buffer, let's do it
                if (_reader.BaseStream.CanSeek)
                    ReloadFromPosition(Position);
                // Otherwise throw an error because we cannot proceed
                else
                    throw new InvalidOperationException("Cannot seek backward beyond buffer on non-seekable stream");
            }
        }

        /// <summary>
        /// Fills the buffer from the current position
        /// </summary>
        private void FillBuffer()
        {
            if (_endOfStream)
                return;

            // If we are going to read outside the buffer
            if (Position >= _bufferStart + _bufferLength)
            {
                _bufferStart = Position;
                _bufferLength = _reader.Read(_buffer, 0, _bufferSize);

                if (_bufferLength == 0)
                    _endOfStream = true;
            }
        }

        /// <summary>
        /// Reloads the buffer from a specific location (for backward seeks)
        /// </summary>
        private void ReloadFromPosition(int position)
        {
            if (!_reader.BaseStream.CanSeek)
                throw new InvalidOperationException("Cannot seek backward on non-seekable stream");

            // We are going to reset the stream to begin from the given position
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            _reader.DiscardBufferedData();
            _bufferStart = 0;
            Position = 0;
            _endOfStream = false;
            _readHistory.Clear();

            // Read the first buffer
            _bufferLength = _reader.Read(_buffer, 0, _bufferSize);
            if (_bufferLength == 0)
                _endOfStream = true;

            // Advance to the desired position
            while (Position < position && !IsEndOfFile)
                ReadCodePoint();

            // Save exact position
            Position = position;
        }

        /// <summary>
        /// Adds the given character to the seek history (for backward seek)
        /// </summary>
        private void AddToHistory(char c)
        {
            _readHistory.Add(c);

            // Keep history under the limit size
            if (_readHistory.Count > MAX_HISTORY_SIZE)
                _readHistory.RemoveAt(0);
        }
        #endregion
    }
}