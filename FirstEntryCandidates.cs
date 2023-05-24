using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLCEScoreEntry
{
    //// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FirstEntryCandidates
    {

        public string District { get; set; }
       public string CenterName { get; set; }
        public int CenterNo { get; set; }
        public int TotalCandidates { get; set; }
        public int CandidatesForFirstEntry { get; set; }
        public List<Candidate> Candidate { get; set; }

    }

       
        public class Candidate
        {
            public int SysID { get; set; }
            public int SeqNo { get; set; }
            public string ExamNo { get; set; }
            public string Name { get; set; }
        }
    
}
