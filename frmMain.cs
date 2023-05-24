using DevExpress.Utils;
using DevExpress.XtraEditors;
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
using DevExpress.XtraSplashScreen;

namespace PSLCEScoreEntry
{
     partial class frmMain : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        
        public frmMain()
        {
            if (!LoggedUser.isUserValid)
            {
                this.Hide();
                new login().ShowDialog();
            }
            else
            {
                InitializeComponent();
               

            }

        }
       

       

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void accordionControlElement4_Click(object sender, EventArgs e)
        {
          

           if (Initialization.LookUpStructure.SubjectList != null && Initialization.LookUpStructure.CenterList!=null) 
              {
                frmScoreCapture sc = new frmScoreCapture(1);
                sc.ShowDialog();
               
               // XtraMessageBox.Show($"{Initialization.LookUpStructure.SubjectList.Count()} subjects and {Initialization.LookUpStructure.CenterList.Count()} centers ", "Loopkup length", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
            else
            {
                XtraMessageBox.Show($"Failed to load lookup values ", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }

                
           
        }

        private void accordionControlElement5_Click(object sender, EventArgs e)
        {
            if (Initialization.LookUpStructure.SubjectList != null && Initialization.LookUpStructure.CenterList != null)
            {
                frmScoreCapture sc = new frmScoreCapture(2);
                sc.ShowDialog();

                // XtraMessageBox.Show($"{Initialization.LookUpStructure.SubjectList.Count()} subjects and {Initialization.LookUpStructure.CenterList.Count()} centers ", "Loopkup length", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
            else
            {
                XtraMessageBox.Show($"Failed to load lookup values ", "Initialization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, DefaultBoolean.True);
            }
        }
    }
}