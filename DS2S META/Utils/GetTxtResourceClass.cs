using System.IO;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    // TODEPRECATE, see DS2Resource
    class GetTxtResourceClass
    {
        public static readonly string? ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string GetTxtResource(string filePath)
        {
            //Get local directory + file path, read file, return string contents of file
            return File.ReadAllText($@"{ExeDir}/{filePath}");
        }

        public static bool IsValidTxtResource(string txtLine)
        {
            //see if txt resource line is valid and should be accepted 
            //(bare bones, only checks for a couple obvious things)
            if (txtLine.Contains("//"))
                txtLine = txtLine[..txtLine.IndexOf("//")]; // keep everything until '//' comment

            return !string.IsNullOrWhiteSpace(txtLine); //empty line check
        }
    }
}