﻿using System.Collections.Generic;
using Data.Core;

namespace Data.Flow
{
    public class WritePipe<T>: LambdaLastPipe<T>
    {
        public WritePipe(IWrite<T> write) : base(pages => Write(write, pages))
        { }

        public static void Write(IWrite<T> writer, IEnumerable<T> pages)
        {
            foreach (var page in pages)
            {
                writer.Write(page);
            }
        }
    }
}