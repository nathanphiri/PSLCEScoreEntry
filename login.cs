using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSLCEScoreEntry
{
    public partial class login : DevExpress.XtraEditors.XtraForm
    {
        public login()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtUsername.EditValue.ToString()) && !String.IsNullOrWhiteSpace(txtPassword.EditValue.ToString()))
            {
                string UNo = txtUsername.EditValue.ToString().Trim();
                string PWD = txtPassword.EditValue.ToString().Trim();
                _ = AuthenticateUser(UNo, PWD);
            }
        }

        async Task AuthenticateUser (string uNo, string PWD)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Properties.Settings.Default.serviceURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                splashScreenManager1.ShowWaitForm();
                var response = await client.PostAsJsonAsync("/PSLCE/login", new  { userNo = uNo, Password = PWD });
                splashScreenManager1.CloseWaitForm();


                switch (response.StatusCode)
                {

                    case System.Net.HttpStatusCode.OK:
                        var responseStrong = await response.Content.ReadAsStringAsync();
                        var authenticationObject = System.Text.Json.JsonSerializer.Deserialize<AuthenticationResult>(responseStrong, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (authenticationObject != null)
                        {
                            LoggedUser.isUserValid = true;
                            LoggedUser.UserID = authenticationObject.UserDetails.userID;
                            LoggedUser.UserNO = authenticationObject.UserDetails.userNO;
                            LoggedUser.userName = authenticationObject.UserDetails.userName;
                            LoggedUser.userCategoryID = authenticationObject.UserDetails.userCategoryID;
                            LoggedUser.userStatus = authenticationObject.UserDetails.userStatus;
                            _=LoadCenterandSubjectLookps();
                            frmMain mainForm = new frmMain();
                            this.Hide();
                            mainForm.ShowDialog();
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

                            if (LookupObject!=null)
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
        }

    }
    public class UserProfile
    {

        public int userID { get; set; }
        public int userNO { get; set; }
        public string userName { get; set; }
        public int userCategoryID { get; set; }
        public int userStatus { get; set; }

    }
   
    public class AuthenticationResult

    {
        public bool isUserValid { get; set; }
        public string Message { get; set; }
        public UserProfile UserDetails { get; set; }


    }
}