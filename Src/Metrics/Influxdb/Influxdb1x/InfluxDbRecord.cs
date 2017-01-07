using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Metrics.NET.InfluxDB
{
    /// <summary>
    /// Influx db record. See: https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html
    /// </summary>
    public class InfluxDbRecord
    {
        private static readonly CultureInfo _cultureEnUs = CultureInfo.CreateSpecificCulture("en-US");

        private static readonly Func<object, string> _integerFormatter = i => string.Format(_cultureEnUs, "{0:G}i", i);

        private static readonly Func<object, string> _decimalFormatter = i => string.Format(_cultureEnUs, "{0:G}", i);

        private static readonly IDictionary<Type, Func<object, string>> _typeFormatters = new Dictionary<Type, Func<object, string>>
        {
            {typeof(short), _integerFormatter},
            {typeof(ushort), _integerFormatter},
            {typeof(int), _integerFormatter},
            {typeof(uint), _integerFormatter},
            {typeof(long), _integerFormatter},
            {typeof(ulong), _integerFormatter},
            {typeof(float), _decimalFormatter},
            {typeof(double), _decimalFormatter},
            {typeof(decimal), _decimalFormatter},
            {typeof(bool), i => i.ToString ().ToLower ()}
        };

        /// <summary>
        /// The line protocol for a measurement(s)
        /// </summary>
        /// <value>The line protocol.</value>
        public string LineProtocol { get; private set; }

        private readonly ConfigOptions _config;

        internal InfluxDbRecord(ConfigOptions config)
        {
            _config = config;
        }

        internal InfluxDbRecord(string name, object data, MetricTags tags, ConfigOptions config, Tuple<string, string>[] moreTags = null)
            : this(config)
        {
            var record = BuildRecordPreamble(name, tags, moreTags);

            record.Append("value=").Append(StringifyValue(data));

            LineProtocol = record.ToString();
        }

        internal InfluxDbRecord(string name, IEnumerable<string> columns, IEnumerable<object> data, MetricTags tags, ConfigOptions config, Tuple<string, string>[] moreTags = null)
            : this(config)
        {
            var record = BuildRecordPreamble(name, tags, moreTags);

            var fieldKeypairs = new List<string>();

            foreach (var pair in columns.Zip(data, (col, dat) => new { col, dat }))
            {
                fieldKeypairs.Add(string.Format("{0}={1}", Escape(pair.col), StringifyValue(pair.dat)));
            }

            record.Append(string.Join(",", fieldKeypairs));

            LineProtocol = record.ToString();
        }

        internal StringBuilder BuildRecordPreamble(string name, MetricTags tags, Tuple<string, string>[] moreTags = null)
        {
            var record = new StringBuilder();
            record.Append(Escape(_config.MetricNameConverter(name)));

            var allTags = GetAllTags(tags);

            if (moreTags != null && moreTags.Length > 0)
            {
                allTags = allTags.Union(moreTags);
            }

            if (allTags.Any())
            {
                record.Append(",");
                record.Append(string.Join(",", allTags.Select(t => string.Format("{0}={1}", Escape(t.Item1), Escape(t.Item2)))));
            }

            record.Append(" ");

            return record;
        }

        internal IEnumerable<Tuple<string, string>> GetAllTags(MetricTags tags)
        {
            var allTags = new List<Tuple<string, string>> 
            {
                new Tuple<string, string>("host", Environment.MachineName.ToLower ()),
                new Tuple<string, string>("user", Environment.UserName.ToLower ()),
                new Tuple<string, string>("pid", Process.GetCurrentProcess ().Id.ToString ()),
            };

            if (tags.Tags != null && tags.Tags.Length > 0)
            {
                allTags.AddRange(tags.Tags.Select(t => new Tuple<string, string>(Escape(t), "true")));
            }

            return allTags;
        }

        private static string Escape(string v)
        {
            // spaces, commas, and equals signs are escaped
            return v.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=");
        }

        /// <summary>
        /// Return the value as an InfluxDB-parseable string. 
        /// See: https://docs.influxdata.com/influxdb/v0.9/write_protocols/line/
        /// </summary>
        /// <param name="val">InfluxDB value</param>
        public static string StringifyValue(object val)
        {
            if (val == null)
            {
                return null;
            }

            var t = val.GetType();

            if (_typeFormatters.ContainsKey(t))
            {
                return _typeFormatters[t](val);
            }
            else if (t == typeof(string)
              && (string.Equals(val.ToString(), "true", StringComparison.InvariantCultureIgnoreCase)
                  || string.Equals(val.ToString(), "false", StringComparison.InvariantCultureIgnoreCase)))
            {
                return val.ToString().ToLower();
            }
            else if (t == typeof(string))
            {
                // surrounded by quoutes and escaped double quotes
                return string.Format("\"{0}\"", val.ToString().Replace("\"", "\\\""));
            }

            return val.ToString();
        }
    }
}
