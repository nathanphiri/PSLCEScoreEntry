using DevExpress.Utils.Extensions;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Data.Extensions;
using static DevExpress.XtraEditors.Mask.MaskSettings;
using DevExpress.XtraSpellChecker.Parser;
using DevExpress.XtraSplashScreen;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace PSLCEScoreEntry
{

    public partial class ucFirstScoreEntry : DevExpress.XtraEditors.XtraUserControl
    {
        FirstEntryCandidates firstEntryCandidates;

        int CandidateListLength;
        int SubjectId;
        int DistrictID;
        List<CapturedScore> scoreItems = new List<CapturedScore>();
        int currentIndexvalue;
         public ucFirstScoreEntry(FirstEntryCandidates CandidatesObject, int subjectID, int districtID)
        {
            if (CandidatesObject != null)                
            {
                firstEntryCandidates = CandidatesObject;
                firstEntryCandidates.Candidate.OrderBy(c=>c.SeqNo).ToList();

                if ((firstEntryCandidates.Candidate?.Any()) == true)
                {
                    // XtraMessageBox.Show($"Retrieved {firstEntryCandidates.Candidate.Count()}  for  {firstEntryCandidates.CenterName} in {firstEntryCandidates.District} district", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                    InitializeComponent();

                    this.tctCentername.EditValue = firstEntryCandidates.CenterName;
                    this.txtCenterNo.EditValue = firstEntryCandidates.CenterNo;
                    this.txtDistrictname.EditValue = firstEntryCandidates.District;
                    this.txtTotalCandidates.EditValue = firstEntryCandidates.TotalCandidates;
                    this.txtCandudateUncaptured.EditValue = firstEntryCandidates.CandidatesForFirstEntry;
                    this.txtCandidatesCaptured.EditValue = Int32.Parse(txtTotalCandidates.EditValue.ToString()) - Int32.Parse(txtCandudateUncaptured.EditValue.ToString());

                    var SubjectInfomation = Initialization.LookUpStructure.SubjectList.Where(s => s.ID == subjectID).FirstOrDefault();
                    if (SubjectInfomation != null)
                    {
                        this.txtSubjectName.Text = SubjectInfomation.Name;
                        this.txtSubjectCode.Text = SubjectInfomation.Code;
                    }
                    CandidateListLength = firstEntryCandidates.Candidate.Count();
                    SubjectId = subjectID;
                    DistrictID = districtID;
                    txtCaptured.EditValue = 0;
                    txtREmaining.EditValue = firstEntryCandidates.Candidate.Count();
                }
                else
                {
                    XtraMessageBox.Show($"There are no candidates available for intial entry.", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                }
            }

            else
            {
                XtraMessageBox.Show($"Failed to retrieve candidate information for first entry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }


        }


        private void txtSeqNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            else if (char.IsControl(e.KeyChar) && (e.KeyChar == (char)Keys.Enter))
            {
                int StartSeqNo = Int32.Parse(txtSeqNo.EditValue.ToString()); 
                if (firstEntryCandidates.Candidate.Where(c => c.SeqNo == StartSeqNo).Any())
                {
                    this.txtSeqNo.Enabled = false;
                    txtScore.Enabled = true;
                    currentIndexvalue = firstEntryCandidates.Candidate.IndexOf(c => c.SeqNo == StartSeqNo);                   
                    loadCandidate(currentIndexvalue);
                }
                else
                {
                    XtraMessageBox.Show($"The sequence number does not match any candidate for first entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                }
                   

               
            }


        }

        private void loadCandidate(int position)
        {
           
                this.txtScore.EditValue = "";
                this.txtScore.Focus();
                var CurrentCandidate = firstEntryCandidates.Candidate[position];
                this.txtSeqNo.EditValue = CurrentCandidate.SeqNo;
                this.txtExamNo.EditValue = CurrentCandidate.ExamNo;
           

        }

        private void txtScore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;

            }
            else if (char.IsControl(e.KeyChar) && (e.KeyChar == (char)Keys.Enter))
            {
                if (txtScore.Text == "." || txtScore.Text == ".." || (int.TryParse(txtScore.Text, out _) && int.Parse(txtScore.Text) >=0 && int.Parse(txtScore.Text) <= 100))
                {

                    
                        int scoreValue;
                        int scoreEntryResultID;
                        if (txtScore.EditValue.ToString() == "." && !int.TryParse(txtScore.EditValue.ToString(), out _))
                        {
                            scoreValue = 0;
                            scoreEntryResultID = 0;

                        }
                        else if (txtScore.EditValue.ToString() == ".." && !int.TryParse(txtScore.EditValue.ToString(), out _))
                        {
                            scoreValue = 0;
                            scoreEntryResultID = -1;

                        }
                        else
                        {
                            char c = e.KeyChar;
                            scoreValue = Int32.Parse(txtScore.Text.ToString());
                            scoreEntryResultID = 1;

                        }

                        scoreItems.Add(new CapturedScore()
                        {
                            CANDIDATEID = firstEntryCandidates.Candidate[currentIndexvalue].SysID,
                            EXAMNO = (string)txtExamNo.EditValue,
                            SUBJECTID = SubjectId,
                            ENTRYLEVELID = 1,
                            SCOREVALUE = scoreValue,
                            USERID = LoggedUser.UserID,
                            ENTRYRESULTID = scoreEntryResultID
                        });
                        using (DataTable scoresTable = new DataTable())
                        {
                            scoresTable.Columns.Add("ExaminationNumber", typeof(string));
                            scoresTable.Columns.Add("InitialScore", typeof(string));
                            scoresTable.Columns.Add("ScoreStatus(1=Captured, 0=Absent, -1=Missing)", typeof(string));

                            foreach (var ScoreItem in scoreItems)
                            {
                                scoresTable.Rows.Add(ScoreItem.EXAMNO, ScoreItem.SCOREVALUE, ScoreItem.ENTRYRESULTID);

                            }
                            gridControl1.DataSource = scoresTable;
                            btnClear.Enabled = true;
                            btnSave.Enabled = true;
                        }
                        txtCaptured.EditValue = scoreItems.Count();
                        txtREmaining.EditValue = firstEntryCandidates.Candidate.Count() - scoreItems.Count();
                        int nextCandidateIndexValue = ++currentIndexvalue;

                        if (nextCandidateIndexValue < firstEntryCandidates.Candidate.Count())
                        {
                            loadCandidate(nextCandidateIndexValue);
                        }
                        else
                        {
                            XtraMessageBox.Show($"Score capturing for {txtSubjectName.EditValue} for {tctCentername.EditValue} done. Please procees to upload the scores. ", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                            txtScore.Enabled = false;
                        }
                    
                }
                else
                {
                    XtraMessageBox.Show($"Invalid score entered. Enter an integer between 0 and 100 or \".\" for absent or \"..\" for a missing script. ", "Input validation error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                    txtScore.Text= string.Empty;
                }

            }    
        }

        private void txtSeqNo_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void splitContainerControl2_Paint(object sender, PaintEventArgs e)
        {


        }

        private void ucFirstScoreEntry_Load(object sender, EventArgs e)
        {

        }

        private void txtScore_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void ucFirstScoreEntry_Load_1(object sender, EventArgs e)
        {
            gridControl1.Visible= false;
            btnClear.Enabled= false;
            btnSave.Enabled = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("This will clear the reults. Do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ClearScores();
            }


            
        }

        private void ClearScores()
        {
            gridControl1.DataSource = null;
            this.txtSeqNo.EditValue = String.Empty;
            this.txtExamNo.EditValue = String.Empty;
            this.txtScore.EditValue = String.Empty;
            this.txtCaptured.EditValue = 0;
            this.txtREmaining.EditValue = firstEntryCandidates.Candidate.Count();
            this.btnSave.Enabled = false;
            this.btnClear.Enabled = false;
            this.scoreItems.Clear();

            this.txtSeqNo.Enabled = true;
            txtScore.Enabled = false;
            this.txtSeqNo.Focus();


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (scoreItems.Count() > 0)
            {
                if (XtraMessageBox.Show("This will upload the reults. Do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {

                    List<ScoreList> ListUploadScore= new List<ScoreList>();
                    foreach(var scoreItem in scoreItems) 
                    {
                        ListUploadScore.Add(new ScoreList
                        {
                         CANDIDATEID=scoreItem.CANDIDATEID,
                        SUBJECTID=scoreItem.SUBJECTID,
                        ENTRYLEVELID=scoreItem.ENTRYLEVELID,
                        SCOREVALUE=scoreItem.SCOREVALUE,
                        USERID=scoreItem.USERID,
                        ENTRYRESULTID=scoreItem.ENTRYRESULTID
                        });
                    }


                    ScoreUpload scoreUpload = new ScoreUpload()
                    {
                        centerID = firstEntryCandidates.CenterNo,
                        districtID = this.DistrictID,
                        scoreList= ListUploadScore
                    };
                    _=upLoadScores(scoreUpload);
                }
            }
        }

        private async Task  upLoadScores(ScoreUpload scoreUploadObject)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                 splashScreenManager1.ShowWaitForm();
                var response = await client.PostAsJsonAsync("/PSLCE/UploadScores", scoreUploadObject);
                splashScreenManager1.CloseWaitForm();

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        XtraMessageBox.Show(responseStrong.ToString(), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                        ClearScores();
                        break;
                    default:
                        var responseStrong2 = await response.Content.ReadAsStringAsync();
                        XtraMessageBox.Show(responseStrong2.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;
                }
            }
        }
    }
    public class ScoreCandidate
    {
        public int sysID { get; set; }
        public int SeqNo { get; set; }
        public string ExamNo { get; set; }
        public string Name { get; set; }
    }
    public class CapturedScore 
    {
        public int CANDIDATEID { get; set; }
        public string EXAMNO { get; set; }
        public int SUBJECTID { get; set; }
        public int ENTRYLEVELID { get; set; }
        public int SCOREVALUE { get; set; }


        public int USERID { get; set; }
        public int ENTRYRESULTID { get; set; }
    }
   
    public class ScoreUpload
    {
        public int centerID { get; set; }
        public int districtID { get; set; }
        public List<ScoreList> scoreList { get; set; }
    }

    public class ScoreList
    {
        public int CANDIDATEID { get; set; }
        public int SUBJECTID { get; set; }
        public int ENTRYLEVELID { get; set; }
        public int SCOREVALUE { get; set; }
        public int USERID { get; set; }
        public int ENTRYRESULTID { get; set; }
    }
}
