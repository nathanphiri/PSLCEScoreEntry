using DevExpress.Utils.DirectXPaint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLCEScoreEntry
{
   public  static class LoggedUser
    {
       
        public static  bool  isUserValid { get; set; }
        public static int UserID { get; set; }
        public static int UserNO { get; set; }
        public static string userName { get; set; }
        public static int userCategoryID { get; set; }
        public static int userStatus { get; set; }
    }
}
