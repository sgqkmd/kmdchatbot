using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIConsole
{
    internal class StudicaUiPlugin
    {
        [KernelFunction]
        [Description("Show the absence page of a student in the Studica UI.")]
        public void ShowStudentAbsence(string studentId)
        {
            var target = $"https://adhoc-preview.a.studica.dk/students/student/{studentId}/absence?absenceTab=courses";
            
            //url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {target}") { CreateNoWindow = true });
            
            //Console.WriteLine("Open: " + target);
            //Process.Start(target);
        }
    }
}
