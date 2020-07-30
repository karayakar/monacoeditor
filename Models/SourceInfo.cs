using System.Collections.Generic;

namespace monacoEditorCSharp.Models
{
    public class SourceInfo
    {
        public SourceInfo()
        {
            Usings = new List<string>();
            References = new List<string>();
        }

        public string SourceCode { get; set; }
        public string Nuget { get; set; }

        public List<string> Usings { get; set; }

        public List<string> References { get; set; }

        public int LineNumberOffsetFromTemplate { get; set; }

        internal int CalculateVisibleLineNumber(int compilerLineError) => compilerLineError - LineNumberOffsetFromTemplate;
    }
}
