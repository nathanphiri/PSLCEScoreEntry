using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLCEScoreEntry
{
    public   class Lookups
    {
        public List<SubjectList> SubjectList { get; set; }
        public List<CenterList> CenterList { get; set; }

    }

    public class CenterList
    {
        public int centerNo { get; set; }
        public string centerName { get; set; }
        public int districtID { get; set; }
        public string districtName { get; set; }
    }

    

    public class SubjectList
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

}
