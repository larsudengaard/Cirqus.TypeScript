﻿using System;
using System.Runtime.Serialization;

namespace Cirqus.TypeScript
{
    [Serializable]
    public class PrettyException : ApplicationException
    {
        public PrettyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public PrettyException(string message, params object[] objs)
            : base(string.Format(message, objs))
        {
        }
    }
}