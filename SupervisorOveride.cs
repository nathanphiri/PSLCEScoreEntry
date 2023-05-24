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
using System.CodeDom;

namespace PSLCEScoreEntry
{
    public partial class SupervisorOveride : DevExpress.XtraEditors.XtraForm
    {
        string enteredScore;
        string  confirmedScore;
      
        public SupervisorOveride(string enteredScore, string message)
        {
            InitializeComponent();
            this.enteredScore = enteredScore;
            this.txtCaturedScore.Text = enteredScore;
            lblMessage.Text = message;
        }

        private void SupervisorOveride_Load(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtActualScore.Text.ToString()) && !String.IsNullOrWhiteSpace(txtPassword.Text.ToString()) && !String.IsNullOrWhiteSpace(txtPassword.Text.ToString()))
            {

                this.confirmedScore =txtActualScore.EditValue.ToString();
                string UNo = txtUsername.Text.ToString().Trim();
                string PWD = txtPassword.Text.ToString().Trim();
               // MessageBox.Show($"Username={UNo} and Password={PWD}");

               _ = AuthenticateUser(UNo, PWD);
            }
            else
            {
                XtraMessageBox.Show($"Please ensure all required fileds are filled", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
        }

        private void txtActualScore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void txtActualScore_EditValueChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtActualScore.Text.ToString()) && !String.IsNullOrEmpty(txtUsername.Text.ToString()) && !String.IsNullOrEmpty(txtPassword.Text.ToString()))
            {
                btnSave.Enabled = true;

            }
            else
            { btnSave.Enabled = false; }
        }

        private void txtUsername_EditValueChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtActualScore.Text.ToString()) && !String.IsNullOrEmpty(txtUsername.Text.ToString()) && !String.IsNullOrEmpty(txtPassword.Text.ToString()))
            {
                btnSave.Enabled = true;

            }
            else
            { btnSave.Enabled = false; }
        }
    

        private void txtPassword_EditValueChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtActualScore.Text.ToString()) && !String.IsNullOrEmpty(txtUsername.Text.ToString()) && !String.IsNullOrEmpty(txtPassword.Text.ToString()))
            {
                btnSave.Enabled = true;

            }
            else
            { btnSave.Enabled = false; }
        }
        private void processScore()
        {

            if (txtActualScore.Text == "." || txtActualScore.Text == ".." || (int.TryParse(txtActualScore.Text, out _) && int.Parse(txtActualScore.Text) >= 0 && int.Parse(txtActualScore.Text) <= 100))
            {
                if(!int.TryParse(txtActualScore.Text, out _))
                {
                    switch (txtActualScore.Text)
                    {
                        case ".":
                            ScoreConfirmation.confirmedEntryScore = 0;
                            ScoreConfirmation.confirmedEntryResultID = 0;
                            break;
                        case "..":
                            ScoreConfirmation.confirmedEntryScore = 0;
                            ScoreConfirmation.confirmedEntryResultID = -1;
                            break;                 
                    }
                    XtraMessageBox.Show($"Score mismatch successfully resolved. Take note that this has been logged.", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                    this.Hide();
                }
                else
                {
                    double ScorevalueDouble = double.Parse(txtActualScore.Text);
                    int ScoreValueInt = (int)ScorevalueDouble;
                    if (ScoreValueInt == ScorevalueDouble)
                    {
                        ScoreConfirmation.confirmedEntryScore = int.Parse(txtActualScore.Text);
                        ScoreConfirmation.confirmedEntryResultID = 1;
                        XtraMessageBox.Show($"Score mismatch successfully resolved. Take note that this has been logged.", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information, DefaultBoolean.True);
                        this.Hide();
                    }
                    else
                    {
                        XtraMessageBox.Show($"Score value should be an integer between 0 and 100.", "Input  error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                    }
                }
               
            }
            else
            {
                XtraMessageBox.Show($"Invalid score entered. Enter an integer between 0 and 100 or \".\" for absent or \"..\" for a missing script. ", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }


        }
        async Task AuthenticateUser(string UNo, string PWD)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                splashScreenManager1.ShowWaitForm();
                var response = await client.PostAsJsonAsync("/PSLCE/login", new { userNo = UNo, Password = PWD });
                splashScreenManager1.CloseWaitForm();
                switch (response.StatusCode)               
                {

                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        var authenticationObject = JsonSerializer.Deserialize<AuthenticationResult>(responseStrong, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (authenticationObject != null)
                        {
                            if (authenticationObject.isUserValid)
                            {
                                if (authenticationObject.UserDetails.userCategoryID == 2)
                                {

                                   
                                    ScoreConfirmation.confirmedEntryUserNo = authenticationObject.UserDetails.userNO;
                                    processScore();
                                  
                                }
                                else
                                {
                                    XtraMessageBox.Show($"User does not have rights to confirm scores", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                                }
                            }
                            else
                            {
                                XtraMessageBox.Show($"User account not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                            }

                        }
                        else
                        {
                            XtraMessageBox.Show($"Invalid server response", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        }
                        break;

                    case System.Net.HttpStatusCode.Unauthorized:
                        XtraMessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;

                    case System.Net.HttpStatusCode.NotFound:
                        XtraMessageBox.Show("Connection to the  server failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;

                    default:
                        XtraMessageBox.Show($"Error: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
                        break;



                }

            }
        }
    }
    
}