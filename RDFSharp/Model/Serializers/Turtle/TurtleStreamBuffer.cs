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
    /// Buffer scorrevole per la lettura efficiente di stream di grandi dimensioni durante il parsing Turtle
    /// </summary>
    internal class TurtleStreamBuffer : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly char[] _buffer;
        private readonly int _bufferSize;

        private int _bufferStart;      // Posizione assoluta nel file del primo carattere nel buffer
        private int _bufferLength;     // Numero di caratteri validi nel buffer
        private bool _endOfStream;

        /// <summary>
        /// Posizione corrente nel file
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Indica se abbiamo raggiunto la fine del file
        /// </summary>
        public bool IsEndOfFile => _endOfStream && Position >= _bufferStart + _bufferLength;

        public TurtleStreamBuffer(StreamReader reader, int bufferSize = 8192)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferSize = bufferSize;
            _buffer = new char[bufferSize];
            _bufferStart = 0;
            _bufferLength = 0;
            Position = 0;
            _endOfStream = false;

            // Carica il primo blocco
            FillBuffer();
        }

        /// <summary>
        /// Legge il prossimo code point Unicode
        /// </summary>
        public int ReadCodePoint()
        {
            if (IsEndOfFile)
                return -1;

            // Assicurati che il carattere corrente sia nel buffer
            EnsureBufferContainsPosition();

            if (Position >= _bufferStart + _bufferLength)
                return -1; // EOF

            int bufferIndex = Position - _bufferStart;
            char highSurrogate = _buffer[bufferIndex];
            Position++;

            // Gestione surrogate pairs per caratteri Unicode supplementari
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
        /// Sbircia il prossimo code point senza avanzare la posizione
        /// </summary>
        public int PeekCodePoint()
        {
            int currentPos = Position;
            int codePoint = ReadCodePoint();
            Position = currentPos; // Ripristina posizione
            return codePoint;
        }

        /// <summary>
        /// Torna indietro di un code point
        /// </summary>
        public void UnreadCodePoint(int codePoint)
        {
            if (codePoint == -1)
                return;

            if (IsSupplementaryCodePoint(codePoint))
            {
                // Carattere supplementare (surrogate pair) - torna indietro di 2 posizioni
                Position = Math.Max(0, Position - 2);
            }
            else
            {
                // Carattere normale - torna indietro di 1 posizione
                Position = Math.Max(0, Position - 1);
            }
        }

        /// <summary>
        /// Torna indietro per una stringa di caratteri
        /// </summary>
        public void UnreadString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            // Torna indietro carattere per carattere (dal fondo)
            for (int i = str.Length - 1; i >= 0; i--)
            {
                UnreadCodePoint(str[i]);
            }
        }

        /// <summary>
        /// Assicura che il buffer contenga la posizione corrente
        /// </summary>
        private void EnsureBufferContainsPosition()
        {
            // Se la posizione è oltre la fine del buffer corrente, carica nuovo buffer
            if (Position >= _bufferStart + _bufferLength && !_endOfStream)
            {
                FillBuffer();
            }
            // Se la posizione è prima dell'inizio del buffer, dobbiamo gestire backward seek
            else if (Position < _bufferStart)
            {
                // Per semplicità, ricarica dall'inizio (può essere ottimizzato se necessario)
                ReloadFromPosition(Position);
            }
        }

        /// <summary>
        /// Riempie il buffer dalla posizione corrente
        /// </summary>
        private void FillBuffer()
        {
            if (_endOfStream)
                return;

            // Se siamo alla fine del buffer corrente, shift del buffer
            if (Position >= _bufferStart + _bufferLength)
            {
                _bufferStart = Position;
            }

            // Leggi il prossimo blocco
            _bufferLength = _reader.Read(_buffer, 0, _bufferSize);

            if (_bufferLength == 0)
            {
                _endOfStream = true;
            }
        }

        /// <summary>
        /// Ricarica il buffer da una posizione specifica (per backward seeks)
        /// </summary>
        private void ReloadFromPosition(int position)
        {
            // Questo è un caso limite - per semplicità resettiamo lo stream
            // In un'implementazione più sofisticata si potrebbe mantenere un buffer circolare
            if (_reader.BaseStream.CanSeek)
            {
                _reader.BaseStream.Seek(0, SeekOrigin.Begin);
                _reader.DiscardBufferedData();
                _bufferStart = 0;
                Position = 0;
                _endOfStream = false;

                // Avanza fino alla posizione desiderata
                while (Position < position && !IsEndOfFile)
                {
                    ReadCodePoint();
                }
                Position = position;
            }
            else
            {
                throw new InvalidOperationException("Cannot seek backward on non-seekable stream");
            }
        }

        /// <summary>
        /// Determina se il code point richiede surrogate pair
        /// </summary>
        private static bool IsSupplementaryCodePoint(int codePoint)
            => (codePoint & ~char.MaxValue) != 0;

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}