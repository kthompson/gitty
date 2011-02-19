using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gitty
{

    /// <summary>
    /// Config class to represent 
    /// </summary>
    public class Config
    {
        private readonly List<ConfigEntry> _entries;
        private readonly Config _parentConfig;

        private static readonly Dictionary<Type, Func<string, object>> Readers = new Dictionary<Type, Func<string, object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config(Config parentConfig = null)
        {
            this._parentConfig = parentConfig;
            this._entries = new List<ConfigEntry>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        private Config(IEnumerable<ConfigEntry> entries, Config parentConfig = null)
        {
            this._parentConfig = parentConfig;
            this._entries = new List<ConfigEntry>(entries);
        }

        /// <summary>
        /// Reads the specified section/subsection/name from the config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Read<T>(string section, string subsection, string name, T defaultValue)
        {
            var entry = GetEntries(this, section, subsection, name).FirstOrDefault();
            if (entry == null)
                return defaultValue;

            return ReadInternal(entry, defaultValue);
        }

        /// <summary>
        /// Reads the specified section/subsection/name from the config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<T> ReadList<T>(string section, string subsection, string name)
        {
            return GetEntries(this, section, subsection, name).Select(e => ReadInternal(e, default(T)));
        }


        /// <summary>
        /// Reads the specified section/subsection/name from the config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Read<T>(string section, string name, T defaultValue)
        {
            return this.Read(section, null, name, defaultValue);
        }

        /// <summary>
        /// Reads the specified section/name from the config.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string Read(string section, string subsection, string name)
        {
            return this.Read<string>(section, subsection, name, null);
        }

        /// <summary>
        /// Writes to the config at the specified section/subsection/name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Write<T>(string section, string subsection, string name, T value)
        {
            var entry = GetEntries(this, section, subsection, name).FirstOrDefault() ?? AddEntry(section, subsection, name);

            WriteInternal(entry, value);
        }

        /// <summary>
        /// Writes the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        public void WriteList<T>(string section, string subsection, string name, IEnumerable<T> values)
        {
            var entries = GetEntries(this, section, subsection, name).ToList();
            foreach (var entry in entries)
            {
                this._entries.Remove(entry);
            }

            foreach (var value in values)
            {
                WriteInternal(AddEntry(section, subsection, name), value);
            }
        }

        private void WriteInternal<T>(ConfigEntry entry, T value)
        {
            if (typeof(T) == typeof(bool))
            {
                WriteBoolInternal(entry, value);
                return;
            }

            if (typeof(T) == typeof(string))
            {
                entry.Value = value.Cast<string>();
                return;
            }

            throw new NotSupportedException(typeof(T).FullName);
        }

        /// <summary>
        /// Deletes the specified section from the Config.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        public void DeleteSection(string section, string subsection = null)
        {
            var sub = subsection == null;
            Func<ConfigEntry, bool> predicate = e => AreEqual(e.Section, section) && 
                                                    (sub || AreEqual(e.Subsection, subsection));

            var entries = this._entries.Where(predicate).ToArray();
            
            foreach (var entry in entries)
            {
                _entries.Remove(entry);
            }
        }

        /// <summary>
        /// Creates a new Config from a string.
        /// </summary>
        /// <param name="content">The reader.</param>
        /// <param name="parent">the parent config</param>
        /// <returns></returns>
        public static Config FromString(string content, Config parent = null)
        {
            using(var reader = new StringReader(content))
            {
                return FromString(reader, parent);
            }
        }

        /// <summary>
        /// Creates a new Config from a TextReader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static Config FromString(TextReader reader, Config parent = null)
        {
            var entries = new List<ConfigEntry>();

            string section = string.Empty;
            string subsection = string.Empty;

            while (true)
            {
                var c = reader.Peek();

                switch (c)
                {
                    case -1:
                        return new Config(entries, parent);
                    case '\n':
                    case '\r':
                        reader.Read();
                        break;
                    case '[':
                        //start of section
                        reader.Read();
                        section = ReadSectionName(reader);
                        var p = reader.Peek();
                        if (p == '"')
                        {
                            subsection = ReadValue(reader, ']') ?? string.Empty;
                            p = reader.Peek();
                        }
                        else
                        { 
                            subsection = string.Empty; 
                        }

                        if (p == ']')
                        {
                            reader.Read();
                            continue;
                        }

                        throw new InvalidConfigException("Unexpected section name");
                   
                    default:
                        var prefix = reader.ReadWhile(i => i == ' ' || i == '\t');
                        var name = ReadKeyName(reader);
                        var e = reader.Peek();

                        string value;
                        bool bare;
                        if(e == '=')
                        {
                            reader.Read();
                            bare = false;
                            value = ReadValue(reader, '\n');
                        }
                        else
                        {
                            value = string.Empty;
                            bare = true;
                        }

                        string suffix;
                        if(e == ';' || e == '#')
                        {
                            suffix = reader.ReadUntil(i => i == '\n');
                        }
                        else
                        {
                            suffix = string.Empty;
                        }
                        
                        entries.Add(new ConfigEntry(section, subsection, prefix, name, value, suffix, bare));
                        break;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var section in _entries.GroupBy(e => e.Section))
            {
                var sectionKey = section.Key;
                foreach (var subsection in section.GroupBy(e => e.Subsection))
                {
                    string subsectionKey = string.IsNullOrEmpty(subsection.Key) 
                                         ? string.Empty 
                                         : string.Format(" \"{0}\"", subsection.Key);

                    sb.AppendFormat("[{0}{1}]\n", sectionKey, subsectionKey);
                    
                    foreach (var entry in subsection)
                    {
                        sb.Append(entry.Prefix);
                        sb.Append(entry.Name);
                        if (!entry.IsBare)
                        {
                            sb.Append(" =");
                            if (!string.IsNullOrEmpty(entry.Value))
                                sb.AppendFormat(" {0}", entry.Value);
                        }
                        sb.Append(entry.Suffix);
                        sb.Append("\n");
                    }

                    sb.Append("\n");
                }
            }

            return sb.Length > 0 ?  sb.ToString(0, sb.Length - 1) : string.Empty;
        }

        private ConfigEntry AddEntry(string section, string subsection, string name)
        {
            var entry = new ConfigEntry(section, subsection ?? string.Empty, name);
            this._entries.Add(entry);
            return entry;
        }

        private static bool AreEqual(string section1, string section2)
        {
            return string.Compare(section1, section2, true) == 0;
        }

        private T ReadInternal<T>(ConfigEntry entry, T defaultValue)
        {
            if (typeof(T) == typeof(bool))
                return ReadBoolInternal(entry, defaultValue);

            if (typeof(T) == typeof(string))
                return ReadStringInternal(entry, defaultValue);

            if (typeof(T) == typeof(long))
                return ReadLongInternal(entry, defaultValue);

            if (typeof(T) == typeof(int))
                return ReadIntInternal(entry, defaultValue);

            var convertor = GetReader<T>();
            return convertor(entry.Value);
        }

        private static T ReadLongInternal<T>(ConfigEntry entry, T defaultValue)
        {
            if (entry.Value == null)
                return defaultValue;

            long result;
            if (long.TryParse(entry.Value, out result))
            {
                return result.Cast<T>();
            }

            var regex = new Regex(@"^(\d+)\s*([kmgKMG])$");
            var match = regex.Match(entry.Value);
            if (match.Success)
            {
                var value = match.Groups[1].Value;
                var suffix = match.Groups[2].Value;
                var multiplier = SuffixToMultiplier(suffix);

                if (long.TryParse(value, out result))
                {
                    return (result * multiplier).Cast<T>();
                }
            }

            return defaultValue;
        }

        private static T ReadIntInternal<T>(ConfigEntry entry, T defaultValue)
        {
            if (entry.Value == null)
                return defaultValue;

            int result;
            if (int.TryParse(entry.Value, out result))
            {
                return result.Cast<T>();
            }

            return defaultValue;
        }

        private static long SuffixToMultiplier(string suffix)
        {
            switch (suffix.ToLower())
            {
                case "k":
                    return 1024;
                case "m":
                    return 1024 * 1024;
                case "g":
                    return 1024 * 1024 * 1024;
                default:
                    throw new ArgumentOutOfRangeException("suffix");
            }
        }

        private static T ReadBoolInternal<T>(ConfigEntry entry, T defaultValue)
        {
            switch (entry.Value.Try(s => s.ToLower()))
            {
                case null:
                    return defaultValue;

                case "":
                case "yes":
                case "true":
                case "on":
                case "1":
                    return true.Cast<T>();

                case "no":
                case "false":
                case "0":
                case "off":
                    return false.Cast<T>();

                default:
                    return defaultValue;
            }
        }

        private static T ReadStringInternal<T>(ConfigEntry entry, T defaultValue)
        {
            //var value = entry.Value ?? defaultValue.Cast<string>();
            //if(entry.IsBare)
            //    return value.Cast<T>();
            
            //return (value ?? string.Empty).Cast<T>();
            return entry.Value.Cast<T>();
        }

        private void WriteBoolInternal<T>(ConfigEntry entry, T value)
        {
            entry.Value = value.Cast<bool>().ToString().ToLower();
        }

        private Func<string, T> GetReader<T>()
        {
            var type = typeof(T);
            if(Readers.ContainsKey(type))
            {
                var f = Readers[type];
                Func<string, T> convertor = value => (T)f(value);
                return convertor;
            }

            throw new NotSupportedException(string.Format("Type {0} not supported.", type.Name));
        }

        private static IEnumerable<ConfigEntry> GetEntries(Config config, string section, string subsection, string name)
        {
            if (subsection == null)
                subsection = string.Empty;

            Func<ConfigEntry, bool> predicate = entry => AreEqual(entry.Section, section) &&
                                                         AreEqual(entry.Subsection, subsection) &&
                                                         AreEqual(entry.Name, name);

            return config._entries.Where(predicate);
        }

        private static string ReadKeyName(TextReader reader)
        {
            var name = new StringBuilder();
            while (true)
            {
                int c = reader.Peek();
                if (c < 0)
                    throw new InvalidConfigException("Unexpected end of config file");

                if ('=' == c)
                    break;

                if (' ' == c || '\t' == c)
                {
                    while (true)
                    {
                        c = reader.ReadAndPeek();
                        if (c < 0)
                            throw new InvalidConfigException("Unexpected end of config file");

                        if (c == ';' || c == '#' || c == '\n' || c == '=')
                            break;

                        if (' ' == c || '\t' == c)
                            continue;

                        throw new InvalidConfigException("Bad entry delimiter");
                    }
                    break;
                }

                if (char.IsLetterOrDigit((char)c) || c == '-')
                {
                    name.Append((char)reader.Read());
                }
                else if ('\n' == c)
                {
                    break;
                }
                else
                    throw new InvalidConfigException("Bad entry name: " + name);
            }

            return name.ToString();
        }

        private static string ReadValue(TextReader reader, int eol)
        {
            var value = new StringBuilder();
            var space = false;
            var quote = false;

            while(true)
            {
                int c = reader.Peek();
                if (c < 0)
                {
                    if (value.Length == 0)
                        throw new InvalidConfigException("Unexpected end of config file");
                    break;
                }

                if ('\n' == c)
                {
                    if (quote)
                        throw new InvalidConfigException("Newline in quotes not allowed");
                    break;
                }

                if (eol == c)
                    break;

                if (!quote)
                {
                    if (char.IsWhiteSpace((char)c))
                    {
                        reader.Read();
                        space = true;
                        continue;
                    }

                    if (';' == c || '#' == c)
                    {
                        break;
                    }
                }

                if (space)
                {
                    if (value.Length > 0)
                        value.Append(' ');
                    space = false;
                }

                if ('\\' == c)
                {
                    reader.Read();
                    c = reader.Read();
                    switch (c)
                    {
                        case -1:
                            throw new InvalidConfigException("End of file in escape");

                        case '\n':
                            continue;

                        case 't':
                            value.Append('\t');
                            continue;

                        case 'b':
                            value.Append('\b');
                            continue;

                        case 'n':
                            value.Append('\n');
                            continue;

                        case '\\':
                            value.Append('\\');
                            continue;

                        case '"':
                            value.Append('"');
                            continue;

                        default:
                            throw new InvalidConfigException("Bad escape: " + ((char)c));
                    }
                }

                if ('"' == c)
                {
                    quote = !quote;
                    reader.Read();
                    continue;
                }

                value.Append((char)reader.Read());
            }

            return value.Length > 0 ? value.ToString() : null;
        
        }

        private static string ReadSectionName(TextReader reader)
        {
            try
            {
                return reader.ReadWhile(c => char.IsLetterOrDigit(c) || c == '.' || c == '-');
            }
            finally
            {
                reader.SkipWhile(c => c == ' ' || c == '\t');
            }
        }
    }

    internal class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message)
            : base(message)
        {
        }
    }

    internal class ConfigEntry
    {
        public string Section { get; private set; }
        public string Subsection { get; private set; }
        public string Prefix { get; private set; }
        public string Name { get; private set; }
        public string Value { get; set; }
        public string Suffix { get; private set; }
        public bool IsBare { get; private set; }

        public ConfigEntry(string section, string subsection, string prefix, string name, string value, string suffix, bool bare)
        {
            Contract.Requires(subsection != null);

            this.Section = section;
            this.Subsection = subsection;
            this.Prefix = prefix;
            this.Name = name;
            this.Value = value;
            this.Suffix = suffix;
            this.IsBare = bare;
        }

        public ConfigEntry(string section, string subsection, string name)
        {
            Contract.Requires(subsection != null);

            this.Section = section;
            this.Subsection = subsection;
            this.Prefix = "\t";
            this.Name = name;
        }

        public override string ToString()
        {
            return Section + 
                   (string.IsNullOrEmpty(Subsection) ? "" : "." + Subsection) +
                   "." + Name + 
                   (this.IsBare ? "" : "=" + Value);
        }
    }
}