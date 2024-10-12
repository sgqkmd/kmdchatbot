using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace SemanticKernelLibrary.Plugins
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
