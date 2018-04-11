using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fs.Processes
{
    internal class ProcessStreamReader
    {
        private const int DefaultBufferSize = 1024;

        private readonly Stream _stream;
        private readonly Decoder _decoder;
        private readonly Action<string> _callback;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly byte[] _byteBuffer;
        private readonly char[] _charBuffer;
        private readonly bool _waitForLineBreaks;
        private readonly StringBuilder _lineBuilder;
        private Lazy<Task> _readTask;
        private bool _previousIsCR;

        public ProcessStreamReader ( Stream stream, Action<string> callback, Encoding encoding, bool waitForLineBreaks = false )
        {
            _stream = stream;
            _callback = callback;
            _cancellationSource = new CancellationTokenSource();

            if (_waitForLineBreaks = waitForLineBreaks)
                _lineBuilder = new StringBuilder(DefaultBufferSize);

            _decoder = encoding.GetDecoder();
            _byteBuffer = new byte[DefaultBufferSize];
            _charBuffer = new char[encoding.GetMaxCharCount(DefaultBufferSize)];

            _readTask = new Lazy<Task>(ReadStreamAsync);
        }

        public Task BeginReadingAsync ()
        {
            return _readTask.Value;
        }

        public void EndReading ()
        {
            _cancellationSource.Cancel();
        }

        private Task ReadStreamAsync ()
        {
            return ReadStreamAsync(_cancellationSource.Token);
        }

        private async Task ReadStreamAsync ( CancellationToken cancellationToken )
        {
            while (true)
            {
                int decodedCount;

                try
                {
                    int bytesRead = await _stream.ReadAsync(_byteBuffer, 0, _byteBuffer.Length, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;

                    // decode the bytes into our character buffer..
                    decodedCount = _decoder.GetChars(_byteBuffer, 0, bytesRead, _charBuffer, 0, false);
                    if (decodedCount == 0)
                        continue;
                }
                catch (TaskCanceledException)
                {
                    // TaskCanceledException is a superclass of OperationCanceledException, we do not want
                    // to treat it as EOF, rather we want the current Task to be flagged as Cancelled.

                    throw;
                }
                catch (Exception ex) when ((ex is OperationCanceledException) || (ex is IOException))
                {
                    // OperationCanceledException and IOException are both treated as EOF..
                    break;
                }

                DispatchData(_charBuffer, decodedCount, false);
            }

            // flush any remaining data..
            DispatchData(_charBuffer, _decoder.GetChars(_byteBuffer, 0, 0, _charBuffer, 0, true), true);

            // callback with null to indicate end of output..
            _callback(null);
        }

        private void DispatchData ( char[] charBuffer, int charCount, bool flush )
        {
            if (_waitForLineBreaks)
            {
                DispatchLineData(charBuffer, charCount, flush);
                return;
            }

            _callback(new String(charBuffer, 0, charCount));
        }

        private void DispatchLineData ( char[] charBuffer, int charCount, bool flush )
        {
            int charIndex = 0;

            // if the last line ended with a CR and new data starts with a LF, skip it..
            if ((_previousIsCR) && (charCount > 0) && (charBuffer[0] == '\n'))
                charIndex++;

            while (charIndex < charCount)
            {
                // scan through the incoming data, until we find a line terminator or reach
                // the end of the new input..

                int charStart = charIndex;
                while ((charIndex < charCount) && (!IsLineTerminator(charBuffer[charIndex])))
                    charIndex++;

                if (charIndex == charCount)
                {
                    // reached the end of input without a line terminator, append everything to the
                    // string builder and continue waiting for more data..

                    _lineBuilder.Append(charBuffer, charStart, charIndex - charStart);
                    _previousIsCR = false;
                    break;
                }

                if (_lineBuilder.Length > 0)
                {
                    // append current data to line builder and dispatch complete line..
                    _callback(_lineBuilder.Append(charBuffer, charStart, charIndex - charStart).ToString());
                    _lineBuilder.Length = 0;
                }
                else
                    // line builder is empty, so all of the line is in charBuffer..
                    _callback(new String(charBuffer, charStart, charIndex - charStart));

                if (charBuffer[charIndex] == '\r')
                {
                    if (charIndex + 1 == charCount)
                    {
                        // input ends with a CR, record that and stop..
                        _previousIsCR = true;
                        break;
                    }

                    // skip the next character if it is a LF..
                    if (charBuffer[charIndex + 1] == '\n')
                        charIndex++;
                }

                // skip CR or LF..
                charIndex++;
            }

            if ((flush) && (_lineBuilder.Length > 0))
            {
                _callback(_lineBuilder.ToString());
                _lineBuilder.Length = 0;
            }
        }

        private static bool IsLineTerminator ( char chChar )
        {
            return (chChar == '\r') || (chChar == '\n');
        }
    }
}
