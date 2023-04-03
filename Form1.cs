using Microsoft.Office.Interop.Outlook;
using Microsoft.WindowsAPICodePack.Dialogs;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace EmailSenderUI
{
    public partial class Form1 : Form
    {
        private List<string> attachments;
        public Form1()
        {
            InitializeComponent();
            this.attachments = new List<string>();
            this.GetSenderAccounts();
        }

        private void btnAddAttachment_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.IsFolderPicker = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.attachments.Add(dialog.FileName);
                this.lstAttachments.Items.Add(Path.GetFileName(dialog.FileName));
                this.btnDeleteAttachment.Visible = true;
            }
        }

        private void btnDeleteAttachment_Click(object sender, EventArgs e)
        {
            var selIndex = this.lstAttachments.SelectedIndex;
            if (selIndex != -1)
            {
                this.lstAttachments.Items.RemoveAt(selIndex);
                this.attachments.RemoveAt(selIndex);
                if (this.lstAttachments.Items.Count < 1)
                    this.btnDeleteAttachment.Visible = false;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var close = MessageBox.Show("Are you sure you want to exit?", "Email Sender", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (close == DialogResult.Yes)
                this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            var clear = MessageBox.Show("Are you sure you want to clear?", "Email Sender", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (clear == DialogResult.Yes)
                this.ClearDetails();
        }

        private void ClearDetails()
        {
            this.txtTo.Text = "";
            this.txtCC.Text = "";
            this.txtBCC.Text = "";
            this.lstAttachments.Items.Clear();
            this.txtSubject.Text = "";
            this.txtMessage.Text = "";
            this.txtNoOfCopies.Text = "1";
            this.btnDeleteAttachment.Visible = false;
            this.attachments = new List<string>();
            this.txtTo.Focus();
        }

        private void txtNoOfCopies_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(this.txtNoOfCopies.Text, "[^0-9]"))
            {
                this.txtNoOfCopies.Text = txtNoOfCopies.Text.Remove(txtNoOfCopies.Text.Length - 1);
                this.txtNoOfCopies.SelectionStart = this.txtNoOfCopies.Text.Length;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtTo.Text))
                MessageBox.Show("Unable to send email, no recipient (To) defined!", "Email Sender", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                //if (string.IsNullOrWhiteSpace(this.txtSubject.Text))
                //{
                //    var proceed = MessageBox.Show("Do you want to send this message without Subject?", "Email Sender", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                //    if (proceed == DialogResult.No)
                //        this.txtSubject.Focus();
                //}
                //else if (string.IsNullOrWhiteSpace(this.txtMessage.Text))
                //{
                //    var proceed = MessageBox.Show("Do you want to send this message without Body?", "Email Sender", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                //    if (proceed == DialogResult.No)
                //        this.txtMessage.Focus();
                //}
                //else
                    ProcessSendEmail();
            }
        }

        private void ProcessSendEmail()
        {
            try
            {
                var noOfCopies = string.IsNullOrWhiteSpace(this.txtNoOfCopies.Text) ? 1 : Int32.Parse(this.txtNoOfCopies.Text);

                var cnt = 1;
                while (cnt <= noOfCopies)
                {
                    OutlookApp oApp = new OutlookApp();
                    var session = oApp.Application.Session;
                    Accounts accounts = session.Accounts;

                    MailItem mailItem = (MailItem)oApp.CreateItem(OlItemType.olMailItem);
                    mailItem.SendUsingAccount = accounts[this.txtFrom.Text];
                    mailItem.To = this.txtTo.Text.Replace(",", ";");
                    mailItem.CC = this.txtCC.Text.Replace(",", ";");
                    mailItem.BCC = this.txtBCC.Text.Replace(",", ";");
                    mailItem.Subject = this.txtSubject.Text;
                    mailItem.Body = this.txtMessage.Text;

                    //Add Attachments
                    foreach (var attachment in this.attachments)
                    {
                        mailItem.Attachments.Add(attachment);
                    }

                    mailItem.Display(false);
                    mailItem.Send();
                    cnt += 1;
                }

                MessageBox.Show("Email successfully sent!", "Email Sender", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.ClearDetails();
            }
            catch (System.Exception)
            {
                MessageBox.Show("Unable to send email, an error encountered!", "Email Sender", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void GetSenderAccounts()
        {
            OutlookApp oApp = new OutlookApp();
            var session = oApp.Application.Session;
            Accounts accounts = session.Accounts;
            foreach (Account account in accounts)
            {
                this.txtFrom.Items.Add(account.DisplayName);
            }

            if (this.txtFrom.Items.Count > 0)
                this.txtFrom.Text = this.txtFrom.Items[0].ToString();
        }
    }
}