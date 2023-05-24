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
using static PSLCEScoreEntry.SecondEntryCandidates;

namespace PSLCEScoreEntry
{

    public partial  class UcSecondScoreEntry : DevExpress.XtraEditors.XtraUserControl
    {
        SecondEntryCandidates secondEntryCandidates= null;
        int CandidateListLength;
        int SubjectId;
        int DistrictID;
        List<CapturedScore> scoreItems = new List<CapturedScore>();
        int currentIndexvalue=0;
        bool scoresMatching = false;
       

        public UcSecondScoreEntry(SecondEntryCandidates CandidatesObject, int subjectID, int districtID)
        {
            if (CandidatesObject != null)
            {
                secondEntryCandidates = CandidatesObject;
                secondEntryCandidates.FirstEntryResults.OrderBy(c => c.SeqNo).ToList();
                if ((secondEntryCandidates.FirstEntryResults?.Any()) == true)
                {
                    InitializeComponent();

                    this.tctCentername.EditValue = secondEntryCandidates.CenterName;
                    this.txtCenterNo.EditValue = secondEntryCandidates.CenterNo;
                    this.txtDistrictname.EditValue = secondEntryCandidates.District;
                    this.txtTotalCandidates.EditValue = secondEntryCandidates.TotalCandidates;
                    this.txtCandudateUncaptured.EditValue = secondEntryCandidates.CandidatesForSecondEntry;
                    this.txtCandidatesCaptured.EditValue = Int32.Parse(txtTotalCandidates.EditValue.ToString()) - Int32.Parse(txtCandudateUncaptured.EditValue.ToString());

                    var SubjectInfomation = Initialization.LookUpStructure.SubjectList.Where(s => s.ID == subjectID).FirstOrDefault();
                    if (SubjectInfomation != null)
                    {
                        this.txtSubjectName.Text = SubjectInfomation.Name;
                        this.txtSubjectCode.Text = SubjectInfomation.Code;
                    }
                    CandidateListLength = secondEntryCandidates.FirstEntryResults.Count();
                    SubjectId = subjectID;
                    DistrictID = districtID;
                    txtCaptured.EditValue = 0;
                    txtREmaining.EditValue = CandidateListLength;
                }
                else
                {
                    XtraMessageBox.Show($"There are no candidates available  to confirm for the selected subject and center.", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                }
            }
            else
            {
                XtraMessageBox.Show($"Failed to retrieve candidate information for confirmation", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
                
           
        }




        private void txtScore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;

            }
            else if (char.IsControl(e.KeyChar) && (e.KeyChar == (char)Keys.Enter))
            {


                if (txtScore.Text == "." || txtScore.Text == ".." || (int.TryParse(txtScore.Text, out _) && int.Parse(txtScore.Text) >= 0 && int.Parse(txtScore.Text) <= 100))
                {
                    int scoreValue=0;
                    int scoreEntryResultID=0;
                    if (txtScore.EditValue.ToString() == "." && !int.TryParse(txtScore.EditValue.ToString(), out _))
                    {
                        if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID==0)
                        {
                           
                            scoreValue = 0;
                            scoreEntryResultID = 0;
                            scoreItems.Add(new CapturedScore()
                            {
                                CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                EXAMNO = (string)txtExamNo.EditValue,
                                SUBJECTID = SubjectId,
                                ENTRYLEVELID = 2,
                                SCOREVALUE = scoreValue,
                                USERID = LoggedUser.UserID,
                                ENTRYRESULTID = scoreEntryResultID
                            });
                        }
                        else if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == 1)
                        {
                           
                            if (XtraMessageBox.Show("The candidate's  score was given a value in the first entry. However you captured the candidate as absent. Are you sure the candidate was absent?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });

                            }
                            else
                            {
                                loadCandidate();
                            }

                        }
                        else if((secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == -1))
                        {
                           
                            if (XtraMessageBox.Show("Candidate's script  was recorded missing in the first entry. However you captured the score as absent. Are you sure the candidate was absent?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });

                            }
                            else
                            {                                
                                loadCandidate();
                            }

                        }
                        

                    }
                    else if (txtScore.EditValue.ToString() == ".." && !int.TryParse(txtScore.EditValue.ToString(), out _))
                    {
                        if ((secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == -1))
                        {
                            scoreValue = 0;
                            scoreEntryResultID = -1;
                            scoreItems.Add(new CapturedScore()
                            {
                                CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                EXAMNO = (string)txtExamNo.EditValue,
                                SUBJECTID = SubjectId,
                                ENTRYLEVELID = 2,
                                SCOREVALUE = scoreValue,
                                USERID = LoggedUser.UserID,
                                ENTRYRESULTID = scoreEntryResultID
                            });

                        }
                        else if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == 0)
                        {
                           
                            if (XtraMessageBox.Show("Candidate's script was recorded as absent  in the first entry. However you have recorded the script as missing. Are you sure the candidate was missed the paper?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });
                            }
                            else
                            {                               
                                loadCandidate();
                            }

                        }
                        else if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == 1)
                        {
                            if (XtraMessageBox.Show("Candidate's score  was captured in the first entry. However you have recorded candidate missing. Are you sure the candidate was missed the paper?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });
                            }
                            else
                            {
                               
                                loadCandidate();
                            }
                        }


                    }
                    else if (int.TryParse(txtScore.EditValue.ToString(), out _))
                    {
                        
                        if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == 1)
                        {
                            if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryScore.ToString() == txtScore.EditValue.ToString())
                            {
                                scoreValue = Int32.Parse(txtScore.Text.ToString());
                                scoreEntryResultID = 1;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });
                            }
                            else
                            {

                                if (XtraMessageBox.Show($"Candidate's intiial score does not match with the entered score. Click \"Yes\" for supervisor override or \"No\" to change.  ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                {   
                                    SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                    supervisorOverideForm.ShowDialog();
                                    scoreValue = ScoreConfirmation.confirmedEntryScore;
                                    scoreEntryResultID =ScoreConfirmation.confirmedEntryResultID;
                                    scoreItems.Add(new CapturedScore()
                                    {
                                        CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                        EXAMNO = (string)txtExamNo.EditValue,
                                        SUBJECTID = SubjectId,
                                        ENTRYLEVELID = 2,
                                        SCOREVALUE = scoreValue,
                                        USERID = LoggedUser.UserID,
                                        ENTRYRESULTID = scoreEntryResultID
                                    });

                                        

                                }
                                else
                                {                                   
                                    loadCandidate();
                                }
                            }

                            

                        }
                        else if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == -1)
                        {
                            if (XtraMessageBox.Show($"Candidate was initially recorded as absent. However you have entered a grade in confirmation. Click \"Yes\" for supervisor override or \"No\" to change.  ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });

                            }
                            else
                            {
                                loadCandidate();
                            }

                        }
                        else if (secondEntryCandidates.FirstEntryResults[currentIndexvalue].FirstEntryResultID == 0)
                        {
                            if (XtraMessageBox.Show("Candidate was initially recorded as absent. However you have entered a grade in confirmation. Click \"Yes\" for supervisor override or \"No\" to change.  ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {

                                SupervisorOveride supervisorOverideForm = new SupervisorOveride(txtScore.EditValue.ToString(), $"Intial and confirmed score mismatched. Please confirm the actual score");
                                supervisorOverideForm.ShowDialog();
                                scoreValue = ScoreConfirmation.confirmedEntryScore;
                                scoreEntryResultID = ScoreConfirmation.confirmedEntryResultID;
                                scoreItems.Add(new CapturedScore()
                                {
                                    CANDIDATEID = secondEntryCandidates.FirstEntryResults[currentIndexvalue].SysID,
                                    EXAMNO = (string)txtExamNo.EditValue,
                                    SUBJECTID = SubjectId,
                                    ENTRYLEVELID = 2,
                                    SCOREVALUE = scoreValue,
                                    USERID = LoggedUser.UserID,
                                    ENTRYRESULTID = scoreEntryResultID
                                });
                            }
                            else
                            {

                                loadCandidate();
                            }
                        }
                           

                    }
                    MessageBox.Show($"ScoreItems length {scoreItems.Count()}");
                    //if(scoreItems.Any())
                   // {
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

                    //}
                    
                    txtCaptured.EditValue = scoreItems.Count();
                    txtREmaining.EditValue = secondEntryCandidates.FirstEntryResults.Count() - scoreItems.Count();
                    currentIndexvalue = currentIndexvalue+1;
                       
                   
                    if (currentIndexvalue < secondEntryCandidates.FirstEntryResults.Count())
                    {
                        loadCandidate();
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
                    txtScore.Text = string.Empty;
                }
            }
        }

        private void loadCandidate()
        {
            this.txtScore.EditValue = "";
            this.txtScore.Focus();
            var CurrentCandidate = secondEntryCandidates.FirstEntryResults[currentIndexvalue];
            this.txtSeqNo.EditValue = CurrentCandidate.SeqNo;
            this.txtExamNo.EditValue = CurrentCandidate.ExamNo;
        }

        private void txtScore_EditValueChanged(object sender, EventArgs e)
        {
           
        }

        private void ucSecondScoreEntry_Load_1(object sender, EventArgs e)
        {
           
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
                if (secondEntryCandidates.FirstEntryResults.Where(c => c.SeqNo == StartSeqNo).Any())
                {
                    this.txtSeqNo.Enabled = false;
                    txtScore.Enabled = true;
                    currentIndexvalue = secondEntryCandidates.FirstEntryResults.IndexOf(c => c.SeqNo == StartSeqNo);
                    //MessageBox.Show("SeqNo key press event fired");
                    loadCandidate();
                }
                else
                {
                    XtraMessageBox.Show($"The sequence number does not match any candidate for first entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                }




            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
           
                    if (scoreItems.Count() > 0)
                    {
                        if (XtraMessageBox.Show("This will upload the reults. Do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {

                            List<ScoreList> ListUploadScore = new List<ScoreList>();
                            foreach (var scoreItem in scoreItems)
                            {
                                ListUploadScore.Add(new ScoreList
                                {
                                    CANDIDATEID = scoreItem.CANDIDATEID,
                                    SUBJECTID = scoreItem.SUBJECTID,
                                    ENTRYLEVELID = scoreItem.ENTRYLEVELID,
                                    SCOREVALUE = scoreItem.SCOREVALUE,
                                    USERID = scoreItem.USERID,
                                    ENTRYRESULTID = scoreItem.ENTRYRESULTID
                                });
                            }


                            ScoreUpload scoreUpload = new ScoreUpload()
                            {
                                centerID = secondEntryCandidates.CenterNo,
                                districtID = this.DistrictID,
                                scoreList = ListUploadScore
                            };
                            _ = upLoadScores(scoreUpload);
                        }
                    }
              
        }

        private async Task upLoadScores(ScoreUpload scoreUploadObject)
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

        private void ClearScores()
        {
            gridControl1.DataSource = null;
            this.txtSeqNo.EditValue = String.Empty;
            this.txtExamNo.EditValue = String.Empty;
            this.txtScore.EditValue = String.Empty;
            this.txtCaptured.EditValue = 0;
            this.txtREmaining.EditValue = secondEntryCandidates.FirstEntryResults.Count();
            this.btnSave.Enabled = false;
            this.btnClear.Enabled = false;
            this.scoreItems.Clear();

            this.txtSeqNo.Enabled = true;
            txtScore.Enabled = false;
            this.txtSeqNo.Focus();
        }
    }

}
