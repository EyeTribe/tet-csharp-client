namespace TETCSharpClient
{
    /// <summary>
    /// Currently used setting for the Tracker API
    /// </summary>
    internal class GazeApiSettings
    {      
        private const string DEFAULT_TET_HOST = "localhost";
        private const int DEFAULT_TET_PORT = 6555;

        private static string host = string.Empty;
        private static int port = 0;

        private GazeApiSettings()
        {
        }

        #region Get/Set

        public static string Host
        {
            get
            {
                if (host == string.Empty)
                    return DEFAULT_TET_HOST;
                else
                    return host;
            }

            set { host = value; }
        }

        public static int Port
        {
            get
            {
                if (port == 0)
                    return DEFAULT_TET_PORT;
                else
                    return port;
            }
            set
            {
                port = value;
            }
        }

        #endregion
    }
}
