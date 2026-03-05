using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{
    public class Devices
    {
        public XboxConsole consoles { get; set; }
    }

    public class XboxConsole
    {
        public XboxConsoleStatus status { get; set; }
        public List<XboxConsoleResult> result { get; set; }
    }

    public class XboxConsoleStatus 
    {
        public string errorCode { get; set; }
        public string? errorMessage { get; set; }
    }

    public class XboxConsoleResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public string powerState { get; set; }
        public string consoleType { get; set; }
    }

}
