using System;

namespace MachtAuthenticate.Localization.Engine
{
    /// <summary>
    ///     Event arguments for a missing key event.
    /// </summary>
    public class MissingKeyEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of <see cref="MissingKeyEventArgs" />.
        /// </summary>
        /// <param name="key">The missing key.</param>
        public MissingKeyEventArgs(string key)
        {
            Key = key;
            Reload = false;
        }

        /// <summary>
        ///     The key that is missing or has no data.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        ///     A flag indicating that a reload should be performed.
        /// </summary>
        public bool Reload { get; set; }
    }
}