using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLCEScoreEntry
{
    public partial class SecondEntryCandidates
    {
        public string District { get; set; }
        public string CenterName { get; set; }
        public int CenterNo { get; set; }
        public int TotalCandidates { get; set; }
        public int CandidatesForSecondEntry { get; set; }
        public List<FirstEntryResult> FirstEntryResults { get; set; }
        public SecondEntryCandidates() { }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class FirstEntryResult
        {
            public int SysID { get; set; }
            public int SeqNo { get; set; }
            public string ExamNo { get; set; }
            public string Name { get; set; }
            public int FirstEntryScore { get; set; }
            public int FirstEntryResultID { get; set; }
            public int UserNo { get; set; }
        }

       

    }
}
