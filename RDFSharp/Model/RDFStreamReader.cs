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

        private int _bufferStart;      // Absolute file position of the first character in the buffer
        private int _bufferLength;     // Number of valid characters in the buffer
        private bool _endOfStream;

        // Cache to handle backward seek without having to reseek on the stream
        private readonly List<char> _readHistory;
        private const int MAX_HISTORY_SIZE = 1024;
        #endregion

        #region Properties
        public int Position { get; set; }

        public bool IsEndOfFile
            => _endOfStream && Position >= _bufferStart + _bufferLength;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a RDFStreamReader on the given StreamReader with the given buffer size
        /// </summary>
        internal RDFStreamReader(StreamReader reader, int bufferSize = 8192)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferSize = bufferSize;
            _buffer = new char[bufferSize];
            _bufferStart = 0;
            _bufferLength = 0;
            _endOfStream = false;
            _readHistory = new List<char>();
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
            // and, if possibile, feed the buffer with new data
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
            int currentPos = Position;
            int codePoint = ReadCodePoint();
            Position = currentPos;

            // Rimuovi dalla history quello che abbiamo appena aggiunto per il peek
            if (codePoint != -1)
            {
                if (IsSupplementaryCodePoint(codePoint))
                {
                    // Rimuovi 2 caratteri per surrogate pair
                    if (_readHistory.Count >= 2)
                    {
                        _readHistory.RemoveRange(_readHistory.Count - 2, 2);
                    }
                }
                else
                {
                    // Rimuovi 1 carattere
                    if (_readHistory.Count >= 1)
                    {
                        _readHistory.RemoveAt(_readHistory.Count - 1);
                    }
                }
            }

            return codePoint;
        }

        /// <summary>
        /// Goes back one codepoint (or 2, depending if it represents a surrogate pair)
        /// </summary>
        public void UnreadCodePoint(int codePoint)
        {
            if (codePoint == -1)
                return;

            if (IsSupplementaryCodePoint(codePoint))
            {
                // Carattere supplementare (surrogate pair) - torna indietro di 2 posizioni
                Position = Math.Max(0, Position - 2);
                // Rimuovi dalla history
                if (_readHistory.Count >= 2)
                    _readHistory.RemoveRange(_readHistory.Count - 2, 2);
            }
            else
            {
                // Carattere normale - torna indietro di 1 posizione
                Position = Math.Max(0, Position - 1);
                // Rimuovi dalla history
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

            // Calcola quanti caratteri tornare indietro
            int charsToUnread = str.Length;

            // Aggiorna la posizione (assicurati di non andare prima dell'inizio)
            Position = Math.Max(0, Position - charsToUnread);

            // Rimuovi dalla history il numero corretto di caratteri
            int historyToRemove = Math.Min(charsToUnread, _readHistory.Count);
            if (historyToRemove > 0)
            {
                _readHistory.RemoveRange(_readHistory.Count - historyToRemove, historyToRemove);
            }
        }

        /// <summary>
        /// Ensures that the buffer contains the current position
        /// </summary>
        private void EnsureBufferContainsPosition()
        {
            // Se la posizione è oltre la fine del buffer corrente, carica nuovo buffer
            if (Position >= _bufferStart + _bufferLength && !_endOfStream)
            {
                FillBuffer();
            }
            // Se la posizione è prima dell'inizio del buffer, usa la cache history se possibile
            else if (Position < _bufferStart)
            {
                // Per backward seek limitato, possiamo usare la history
                // Per seek più grandi, dovremmo ricaricare (implementazione semplificata)
                if (_reader.BaseStream.CanSeek)
                {
                    ReloadFromPosition(Position);
                }
                else
                {
                    // Se non possiamo fare seek e la posizione è troppo indietro, errore
                    if (Position < _bufferStart)
                        throw new InvalidOperationException("Cannot seek backward beyond buffer on non-seekable stream");
                }
            }
        }

        /// <summary>
        /// Fills the buffer from the current position
        /// </summary>
        private void FillBuffer()
        {
            if (_endOfStream)
                return;

            // Se stiamo per leggere oltre il buffer corrente
            if (Position >= _bufferStart + _bufferLength)
            {
                _bufferStart = Position;
                _bufferLength = _reader.Read(_buffer, 0, _bufferSize);

                if (_bufferLength == 0)
                {
                    _endOfStream = true;
                }
            }
        }

        /// <summary>
        /// Reloads the buffer from a specific location (for backward seeks)
        /// </summary>
        private void ReloadFromPosition(int position)
        {
            if (!_reader.BaseStream.CanSeek)
                throw new InvalidOperationException("Cannot seek backward on non-seekable stream");

            // Reset the stream to begin from the given position
            // Reset dello stream
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            _reader.DiscardBufferedData();

            // Reset delle variabili
            _bufferStart = 0;
            Position = 0;
            _endOfStream = false;
            _readHistory.Clear();

            // Leggi il primo buffer
            _bufferLength = _reader.Read(_buffer, 0, _bufferSize);
            if (_bufferLength == 0)
                _endOfStream = true;

            // Avanza fino alla posizione desiderata
            while (Position < position && !IsEndOfFile)
            {
                ReadCodePoint();
            }

            // Imposta la posizione esatta
            Position = position;
        }

        /// <summary>
        /// Aggiunge un carattere alla history per il backward seek
        /// </summary>
        private void AddToHistory(char c)
        {
            _readHistory.Add(c);

            // Mantieni la history sotto il limite
            if (_readHistory.Count > MAX_HISTORY_SIZE)
            {
                _readHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Determina se il code point richiede surrogate pair
        /// </summary>
        private bool IsSupplementaryCodePoint(int codePoint)
            => (codePoint & ~char.MaxValue) != 0;
        #endregion
    }
}