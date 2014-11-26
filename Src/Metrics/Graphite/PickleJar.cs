
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Metrics.Graphite
{
    public sealed class PickleJar
    {
        private const char
            MARK = '(',
            STOP = '.',
            LONG = 'L',
            STRING = 'S',
            APPEND = 'a',
            LIST = 'l',
            TUPLE = 't',
            QUOTE = '\'',
            LF = '\n';

        private class Pickle
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Timestamp { get; set; }
        }

        private readonly List<Pickle> jar = new List<Pickle>();

        public void Append(string name, string value, string timestamp)
        {
            this.jar.Add(new Pickle { Name = name, Value = value, Timestamp = timestamp });
        }

        public int Size { get { return this.jar.Count; } }

        public void WritePickleData(Stream stream)
        {
            if (this.Size == 0)
            {
                return;
            }

            var pickles = ReadPickles();
            var payload = Encoding.UTF8.GetBytes(pickles);
            var header = BitConverter.GetBytes(payload.Length);

            stream.Write(header, 0, header.Length);
            stream.Write(payload, 0, payload.Length);

            stream.Flush();
        }

        private string ReadPickles()
        {
            // this is copied from
            // https://github.com/dropwizard/metrics/blob/master/metrics-graphite/src/main/java/com/codahale/metrics/graphite/PickledGraphite.java#L300

            StringBuilder buffer = new StringBuilder();

            buffer.Append(MARK);
            buffer.Append(LIST);

            foreach (var pickle in this.jar)
            {
                // start the outer tuple
                buffer.Append(MARK);

                // the metric name is a string.
                buffer.Append(STRING);
                // the single quotes are to match python's repr("abcd")
                buffer.Append(QUOTE);
                buffer.Append(pickle.Name);
                buffer.Append(QUOTE);
                buffer.Append(LF);

                // start the inner tuple
                buffer.Append(MARK);

                // timestamp is a long
                buffer.Append(LONG);
                buffer.Append(pickle.Timestamp);
                // the trailing L is to match python's repr(long(1234))
                buffer.Append(LONG);
                buffer.Append(LF);

                // and the value is a string.
                buffer.Append(STRING);
                buffer.Append(QUOTE);
                buffer.Append(pickle.Value);
                buffer.Append(QUOTE);
                buffer.Append(LF);

                buffer.Append(TUPLE); // inner close
                buffer.Append(TUPLE); // outer close

                buffer.Append(APPEND);
            }

            buffer.Append(STOP);

            return buffer.ToString();
        }
    }
}
