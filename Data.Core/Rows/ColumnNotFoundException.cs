﻿using System;

namespace Data.Core
{
    public class ColumnNotFoundException: Exception
    {
        public ColumnNotFoundException(string column): base($"The given column ({column}) was not present in the row")
        { }
    }
}