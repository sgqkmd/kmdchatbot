﻿using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIConsole
{
    public class TimeInformationPlugin
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
}
