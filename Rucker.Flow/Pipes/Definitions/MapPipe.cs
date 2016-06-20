﻿using System.Linq;
using System.Collections.Generic;
using Rucker.Data;

namespace Rucker.Flow
{
    public class MapPipe<C, P>: LambdaMidPipe<C, P>
    {
        public MapPipe(params IMap<C, P>[] mappers) : base(pages => Map(mappers, pages))
        { }

        public static IEnumerable<P> Map(IEnumerable<IMap<C, P>> mappers, IEnumerable<C> pages)
        {
            mappers = mappers.ToArray();

            foreach (var page in pages)
            {
                foreach (var map in mappers)
                {
                    yield return map.Map(page);
                }
            }
        }
    }
}