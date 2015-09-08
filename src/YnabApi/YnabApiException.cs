using System;
using System.Runtime.Serialization;

namespace YnabApi
{
    public class YnabApiException : Exception
    {
        public YnabApiException()
        {
        }

        public YnabApiException(string message)
            : base(message)
        {
        }

        public YnabApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}