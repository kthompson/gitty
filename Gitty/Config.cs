using System;
using System.Collections.Generic;
using System.Text;

namespace Gitty
{

    /// <summary>
    /// Config class to represent 
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the specified section/subsection/key from the config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Read<T>(string section, string subsection, string key, T defaultValue = default(T))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the specified section/key from the config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Read<T>(string section, string key, T defaultValue = default(T))
        {
            return this.Read(section, null, key, defaultValue);
        }

        /// <summary>
        /// Writes to the config at the specified section/subsection/key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Write<T>(string section, string subsection, string key, T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified section from the Config.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="subsection">The subsection.</param>
        public void DeleteSection(string section, string subsection = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new Config from a string.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static Config FromString(string content)
        {
            throw new NotImplementedException();
        }
    }
}