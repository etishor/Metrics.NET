using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Reporters
{
    public class JsonObject
    {
        public JsonObject(IEnumerable<JsonProperty> properties)
        {
            this.Properties = properties;
        }

        public IEnumerable<JsonProperty> Properties { get; private set; }

        public string AsJson(bool indented = true, int indent = 0)
        {
            indent = indented ? indent : 0;
            var properties = this.Properties.Select(p => p.AsJson(indented, indent + 2));

            var jsonProperties = string.Join(indented ? "," + Environment.NewLine : ",", properties);

            return string.Format(indented ? "{{\r\n{0}\r\n{1}}}" : "{{{0}}}{1}", jsonProperties, new string(' ', indent));
        }
    }

    public class JsonProperty
    {
        public JsonProperty(string name, IEnumerable<JsonProperty> properties)
            : this(name, new ObjectJsonValue(properties))
        { }

        public JsonProperty(string name, string value)
            : this(name, new StringJsonValue(value))
        { }

        public JsonProperty(string name, long value)
            : this(name, new LongJsonValue(value))
        { }

        public JsonProperty(string name, double value)
            : this(name, new DoubleJsonValue(value))
        { }

        public JsonProperty(string name, bool value)
            : this(name, new BoolJsonValue(value))
        { }

        public JsonProperty(string name, JsonValue value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }
        public JsonValue Value { get; private set; }

        public string AsJson(bool indented, int indent)
        {
            indent = indented ? indent : 0;
            return string.Format("{0}\"{1}\":{2}", new string(' ', indent), JsonValue.Escape(this.Name), this.Value.AsJson(indented, indent + 2));
        }
    }

    public abstract class JsonValue
    {
        public abstract string AsJson(bool indented = true, int indent = 0);

        public static string Escape(string value)
        {
            return value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\r", @"\r").Replace("\n", @"\n");
        }
    }

    public class StringJsonValue : JsonValue
    {
        public StringJsonValue(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public override string AsJson(bool indented = true, int indent = 0)
        {
            return "\"" + Escape(this.Value) + "\"";
        }
    }

    public class LongJsonValue : JsonValue
    {
        public LongJsonValue(long value)
        {
            this.Value = value;
        }

        public long Value { get; private set; }

        public override string AsJson(bool indented = true, int indent = 0)
        {
            return this.Value.ToString("D");
        }
    }

    public class DoubleJsonValue : JsonValue
    {
        public DoubleJsonValue(double value)
        {
            this.Value = value;
        }

        public double Value { get; private set; }

        public override string AsJson(bool indented = true, int indent = 0)
        {
            return this.Value.ToString("F");
        }
    }

    public class BoolJsonValue : JsonValue
    {
        public BoolJsonValue(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; private set; }

        public override string AsJson(bool indented = true, int indent = 0)
        {
            return this.Value ? "true" : "false";
        }
    }

    public class ObjectJsonValue : JsonValue
    {
        public ObjectJsonValue(IEnumerable<JsonProperty> properties)
            : this(new JsonObject(properties))
        { }

        public ObjectJsonValue(JsonObject value)
        {
            this.Value = value;
        }

        public JsonObject Value { get; set; }

        public override string AsJson(bool indented = true, int indent = 0)
        {
            return this.Value.AsJson(indented, indent);
        }
    }
}
