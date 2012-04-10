using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Gamefloor.Support
{
    class GamefloorException : Exception
    {
        public GamefloorException()
            : base()
        {
        }

        public GamefloorException(String message)
            : base(message)
        {
        }

        public GamefloorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GamefloorException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
