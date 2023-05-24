using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils.Extensions;
using static PSLCEScoreEntry.SecondEntryCandidates;

namespace PSLCEScoreEntry
{
    public partial class frmScoreCapture : DevExpress.XtraEditors.XtraForm
    {
        int levelID;
        
        public frmScoreCapture(int levelID)
        {
            this.levelID = levelID;
          if(LoggedUser.isUserValid) 
            {
                if (Initialization.LookUpStructure.SubjectList != null && Initialization.LookUpStructure.SubjectList != null)
                {
                    InitializeComponent();
                   
                    splitContainerControl1.Panel1.BorderStyle = (DevExpress.XtraEditors.Controls.BorderStyles)BorderStyle.None;
                    if(levelID==1)
                    {
                        lblHeader.Text = "PSLCE Score Entry-Initial Entry";
                        
                    }
                    else if(levelID==2)
                    {
                        lblHeader.Text = "PSLCE Score Entry-Confirmation";
                    }
                    lkpSubject.Properties.DataSource = Initialization.LookUpStructure.SubjectList;
                    lkpSubject.Properties.DisplayMember = "Name";
                    lkpSubject.Properties.ValueMember = "ID";


                    List<DistrictLookupItem> districtLoopUpItemList = new List<DistrictLookupItem>();
                    var ListGroupedbyDistrict= Initialization.LookUpStructure.CenterList.GroupBy(d => d.districtID);
                    
                    foreach(var districtGroup  in ListGroupedbyDistrict)
                    {
                      
                        var centerItem=districtGroup.FirstOrDefault();
                        districtLoopUpItemList.Add(new DistrictLookupItem() { ID = centerItem.districtID, Name = centerItem.districtName });

                    }

                    lkpDistrict.Properties.DataSource = districtLoopUpItemList;
                    lkpDistrict.Properties.DisplayMember = "Name";
                    lkpDistrict.Properties.ValueMember = "ID";
                    

                }
                else
                {
                    XtraMessageBox.Show($"Failed to load lookup values. The system will log you out. ", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                    login login = new login();
                    this.Hide();
                    login.ShowDialog();
                }
            }
          else
            {
                login login = new login();
                this.Hide();
                login.ShowDialog();
            }
          
           
        }
        private void loadDistcistLookup()
        {
            var distinctList = Initialization.LookUpStructure.CenterList.ToList().Select(d=>new { d.districtID,d.districtName }).GroupBy(d => d.districtID).ToList();
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {

        }

        private void frmScoreCapture_Load(object sender, EventArgs e)
        {
            
        }

        private void lkpCenter_EditValueChanged(object sender, EventArgs e)
        {
            if (lkpCenter.EditValue != null)
            {
                btnSearch.Enabled = true;

            }
            else
            {
                btnSearch.Enabled = false;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (lkpSubject.EditValue != null && lkpDistrict.EditValue != null && lkpCenter.EditValue != null)
            {
                if (this.levelID == 1)
                {
                    _ = CaptureScores();

                }
                else if(this.levelID == 2)
                {
                    _ = ConfirmScores();
                }
                    
            }
            else
            {
                XtraMessageBox.Show($"Please make sure subject, district and center fields are selected", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
        }

        async private Task ConfirmScores()
        {
            splitContainerControl2.Panel2.Controls.Clear();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                splashScreenManager1.ShowWaitForm();
                var resquestObject = new { districtId = lkpDistrict.EditValue, centerId = lkpCenter.EditValue, subjectId = lkpSubject.EditValue, entryLevelId = 1 };
                var response = await client.PostAsJsonAsync("/PSLCE/getCandidatesSecondEntry", resquestObject);
                splashScreenManager1.CloseWaitForm();

                switch (response.StatusCode)
                {

                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        SecondEntryCandidates SecondEntryCandidateObject = JsonSerializer.Deserialize<SecondEntryCandidates>(responseStrong, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                        if (SecondEntryCandidateObject != null)
                        {
                            if ((SecondEntryCandidateObject.FirstEntryResults?.Any()) == true)
                            {
                                SecondEntryCandidateObject.FirstEntryResults.OrderBy(c => c.SeqNo);
                                // XtraMessageBox.Show($"Retrieved {FirstEntryCandidateObject.Candidate.Count()}  for  {FirstEntryCandidateObject.CenterName} in {FirstEntryCandidateObject.District} district", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                                UcSecondScoreEntry uc = new UcSecondScoreEntry(SecondEntryCandidateObject, (int)lkpSubject.EditValue, (int)lkpDistrict.EditValue);
                                uc.Dock = DockStyle.Fill;

                                splitContainerControl2.Panel2.AddControl(uc);

                            }
                            else
                            {
                                XtraMessageBox.Show($"There are no candidates at {lkpCenter.Text}  for  {lkpSubject.Text} for intial entry.", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                            }
                        }

                        else
                        {
                            XtraMessageBox.Show($"No scores to confirm for the subject at the selected center", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DefaultBoolean.True);
                        }
                        break;

                    case System.Net.HttpStatusCode.NotFound:
                        XtraMessageBox.Show($"No scores to confirm for the subject at the selected center. ", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DefaultBoolean.True);
                        break;
                    default:
                        XtraMessageBox.Show($" Error: {response.StatusCode}", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;



                }


            }
        }

        async private Task CaptureScores()
        {
            splitContainerControl2.Panel2.Controls.Clear();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                splashScreenManager1.ShowWaitForm();
                 var response = await client.PostAsJsonAsync("/PSLCE/getCandidatesFirstEntry", new { districtId = lkpDistrict.EditValue, centerId = lkpCenter.EditValue, subjectId = lkpSubject.EditValue, entryLevelId = 1 });
                splashScreenManager1.CloseWaitForm();

                switch (response.StatusCode)
                {

                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        FirstEntryCandidates FirstEntryCandidateObject = JsonSerializer.Deserialize<FirstEntryCandidates>(responseStrong, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                        if (FirstEntryCandidateObject != null)
                        {
                            if ((FirstEntryCandidateObject.Candidate?.Any())==true)
                            {
                                FirstEntryCandidateObject.Candidate.OrderBy(c => c.SeqNo);
                                 // XtraMessageBox.Show($"Retrieved {FirstEntryCandidateObject.Candidate.Count()}  for  {FirstEntryCandidateObject.CenterName} in {FirstEntryCandidateObject.District} district", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                                 ucFirstScoreEntry uc = new ucFirstScoreEntry(FirstEntryCandidateObject, (int)lkpSubject.EditValue, (int)lkpDistrict.EditValue);
                                uc.Dock = DockStyle.Fill;

                                splitContainerControl2.Panel2.AddControl(uc);

                            }
                            else
                            {
                                XtraMessageBox.Show($"There are no candidates at {lkpCenter.Text}  for  {lkpSubject.Text} for intial entry.", "No candidates", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                            }
                        }

                        else
                        {
                            XtraMessageBox.Show($"Failed to retrieve candidate information for first entry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        }
                        break;

                    default:
                        XtraMessageBox.Show($"Error: {response.StatusCode}", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;



                }


            }


        }
        async Task LoadCenterandSubjectLookps()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                splashScreenManager1.ShowWaitForm();
                var response = await client.GetAsync("/PSLCE/getLookups");
                splashScreenManager1.CloseWaitForm();

                switch (response.StatusCode)
                {

                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        var LookupObject = JsonSerializer.Deserialize<Lookups>(responseStrong, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                        if (LookupObject != null)
                        {
                            Initialization.LookUpStructure = LookupObject;
                        }

                        else
                        {
                            XtraMessageBox.Show($"Lookup list is null ", "Loopkup Eror", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        }
                        break;

                    default:
                        XtraMessageBox.Show($"Error: {response.StatusCode}", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;



                }


            }


        }
        private void lkpDistrict_EditValueChanged(object sender, EventArgs e)
        {
            if (lkpDistrict.EditValue != null)
            {

                int districtID = Int32.Parse(lkpDistrict.EditValue.ToString());
                lkpCenter.Properties.DataSource = Initialization.LookUpStructure.CenterList.Where(d=>d.districtID== districtID).Select(c => new { c.centerNo, c.centerName }).OrderBy(o => o.centerName).ToList(); ;
                lkpCenter.Properties.DisplayMember = "centerName";
                lkpCenter.Properties.ValueMember = "centerNo";

                lkpCenter.Enabled= true;
            }
            else
            {
                lkpCenter.Enabled = false;
                lkpCenter.Clear();
            }
        }
    }
    public class DistrictLookupItem
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }
}