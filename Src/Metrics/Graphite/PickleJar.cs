
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Metrics.Graphite
{
    public sealed class PickleJar
    {
        private const char
            Mark = '(',
            Stop = '.',
            Long = 'L',
            String = 'S',
            AppendChar = 'a',
            List = 'l',
            Tuple = 't',
            Quote = '\'',
            Lf = '\n';

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

            var buffer = new StringBuilder();

            buffer.Append(Mark);
            buffer.Append(List);

            foreach (var pickle in this.jar)
            {
                // start the outer tuple
                buffer.Append(Mark);

                // the metric name is a string.
                buffer.Append(String);
                // the single quotes are to match python's repr("abcd")
                buffer.Append(Quote);
                buffer.Append(pickle.Name);
                buffer.Append(Quote);
                buffer.Append(Lf);

                // start the inner tuple
                buffer.Append(Mark);

                // timestamp is a long
                buffer.Append(Long);
                buffer.Append(pickle.Timestamp);
                // the trailing L is to match python's repr(long(1234))
                buffer.Append(Long);
                buffer.Append(Lf);

                // and the value is a string.
                buffer.Append(String);
                buffer.Append(Quote);
                buffer.Append(pickle.Value);
                buffer.Append(Quote);
                buffer.Append(Lf);

                buffer.Append(Tuple); // inner close
                buffer.Append(Tuple); // outer close

                buffer.Append(AppendChar);
            }

            buffer.Append(Stop);

            return buffer.ToString();
        }
    }
}
