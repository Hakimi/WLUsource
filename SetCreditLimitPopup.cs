using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinLookup.Utilities;

namespace WinLookup
{
    public partial class SetCreditLimitPopup : Form
    {
        public SetCreditLimitPopup()
        {
            InitializeComponent();

            this.txtNewCashAdvAmount.MaskedEditBoxElement.KeyPress += new KeyPressEventHandler(txtNewCashAdvAmount_KeyPress );
            this.txtScheduledAllotment.MaskedEditBoxElement.KeyPress += new KeyPressEventHandler(txtScheduledAllotment_KeyPress);
            
            ColorSetUp.setColorToControls(this);//140410_mitesh
        }

        public decimal NewCashAdvAmount;
        public decimal ScheduledAllotment;
        
        public int CreditLimit;
        int ActualCrLimit; //070912
        public int CustomerLevelID;
        public bool CloseWtihSubmit = false;
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DoUpdate();//010710
        }
        private void DoUpdate()
        {
            try
            {

                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                if (string.IsNullOrEmpty(txtNewCreditLimit.Text) || Convert.ToDecimal(txtNewCreditLimit.Text) == 0)
                {
                    txtNewCreditLimit.Focus();
                    MessageBox.Show("Please enter New Credit Limit.");
                    Cursor = Cursors.Default;
                    Application.DoEvents();
                    return;
                } 
                //071212
                if (Convert.ToDecimal(ActualCrLimit) < Convert.ToDecimal(txtNewCreditLimit.Value)) 
                {
                    //if (string.IsNullOrEmpty(txtNewCashAdvAmount.Text)) //100412
                    if (string.IsNullOrEmpty(txtNewCashAdvAmount.Text) || Convert.ToDecimal(txtNewCashAdvAmount.Text) == 0) //100412
                    {
                        txtNewCashAdvAmount.Focus();
                        MessageBox.Show("Please enter Expected Cash Advance.");
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }
                    //if (string.IsNullOrEmpty(txtScheduledAllotment.Text))//100412
                    if (string.IsNullOrEmpty(txtScheduledAllotment.Text) || Convert.ToDecimal(txtScheduledAllotment.Text) == 0)//100412
                    {
                        txtScheduledAllotment.Focus();
                        MessageBox.Show("Please enter Expected Allotment amount.");
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }

                    if (Common.CheckNewCashAdvance(CreditLimit, Convert.ToDecimal(txtNewCashAdvAmount.Text)) == false)
                    {
                        MessageBox.Show("Expected Cash Advance Amount between $50 and (Credit limit - Customer Balance).");
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }  

                    decimal Balance = Common.GetCustomerBalance();
                    decimal minAll = 0;
                    minAll = Common.GetScheduledAllotmentAmt(Convert.ToDecimal(txtNewCashAdvAmount.Text), Balance); 
                    //minAll = (((Convert.ToDecimal(txtNewCashAdvAmount.Text) + Balance) * (9.5M)) / 100);
                    if (Convert.ToDecimal(txtScheduledAllotment.Text) < minAll)
                    {
                        MessageBox.Show("Expected Allotment amount cannot be less than " + minAll.ToString("c2"));
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }
                
                    if (Convert.ToDecimal(txtScheduledAllotment.Text) > Convert.ToDecimal(txtNewCashAdvAmount.Text))
                    {
                        MessageBox.Show("Expected Allotment amount cannot be more than Expected Cash Advance");
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }
                }
                //071212
                NewCashAdvAmount = Convert.ToDecimal(txtNewCashAdvAmount.Text);
                ScheduledAllotment = Convert.ToDecimal(txtScheduledAllotment.Text);
                CloseWtihSubmit = true;
                Cursor = Cursors.Default;
                Application.DoEvents();
                this.Close();

            }
            catch (Exception ex)
            {
                Application.DoEvents();
                this.Close();

                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in btnSubmit_Click() Function CustomerID:"+ Common.CustomerID  );
                MessageBox.Show(ex.Message);
            }
        }
        private void txtNewCashAdvAmount_Leave(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                decimal Balance = Common.GetCustomerBalance();
                decimal ScheduledAllotment = Common.GetScheduledAllotmentAmt(Convert.ToDecimal(txtNewCashAdvAmount.Text), Balance); //140410_mitesh
                //decimal ScheduledAllotment = (((Convert.ToDecimal(txtNewCashAdvAmount.Text) + Balance) * 9.5M) / 100);
                txtScheduledAllotment.Value = ScheduledAllotment.ToString("f2");
                Cursor = Cursors.Default;
                Application.DoEvents();
                //return;

            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Application.DoEvents();
                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in txtNewCashAdvAmount_Leave() Function CustomerId:"+Common.CustomerID  ); //030411
                MessageBox.Show(ex.Message);
            }
         
        }

        private void SetCreditLimitPopup_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void SetCreditLimitPopup_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F9:
                    DoUpdate();
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void txtNewCashAdvAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar("."))
            {
                e.Handled = true;
                txtNewCashAdvAmount.SelectionStart = (txtNewCashAdvAmount.Text.Length - 2);
            }
        }

        private void txtScheduledAllotment_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar("."))
            {
                e.Handled = true;
                txtScheduledAllotment.SelectionStart = (txtScheduledAllotment.Text.Length - 2);
            }
        }

        //private void txtScheduledAllotment_Leave(object sender, EventArgs e)
        //{
            //Cursor = Cursors.WaitCursor;
            //Application.DoEvents();
            //if (string.IsNullOrEmpty(txtNewCashAdvAmount.Text))
            //{
            //    txtNewCashAdvAmount.Focus();
            //    MessageBox.Show("Please enter Expected Cash Advance.");
            //    Cursor = Cursors.Default;
            //    Application.DoEvents();
            //    return;
            //}
            //if (string.IsNullOrEmpty(txtScheduledAllotment.Text))
            //{
            //    txtScheduledAllotment.Focus();
            //    MessageBox.Show("Please enter Expected Allotment amount.");
            //    Cursor = Cursors.Default;
            //    Application.DoEvents();
            //    return;
            //}
            //decimal Balance = Common.GetCustomerBalance();
            //decimal minAll = 0;
            //minAll = (((Convert.ToDecimal(txtNewCashAdvAmount.Text) + Balance) * 9.5M) / 100);
            //if (Convert.ToDecimal(txtScheduledAllotment.Text) < Convert.ToDecimal(minAll.ToString("f2")))
            //{
            //    MessageBox.Show("Expected Allotment amount cannot be less than " + minAll.ToString("c2"));
            //    Cursor = Cursors.Default;
            //    Application.DoEvents();
            //    return;
            //}
            //if (Convert.ToDecimal(txtScheduledAllotment.Text) > Convert.ToDecimal(txtNewCashAdvAmount.Text))
            //{
            //    MessageBox.Show("Expected Allotment amount cannot be more than Expected Cash Advance");
            //    Cursor = Cursors.Default;
            //    Application.DoEvents();
            //    return;
            //}
            //Cursor = Cursors.Default;
            //Application.DoEvents();
       // }


        //041811
        public void getQualifiedCreditLimit()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                decimal crLimit = Common.calculateCreditLimit(CustomerLevelID);
                lblQualified.Text = crLimit.ToString("c2");

                Cursor = Cursors.Default;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Application.DoEvents();

                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in getQualifiedCreditLimit() Function CustomerID:" + Common.CustomerID);
                MessageBox.Show(ex.Message);
            }
        }
        public void getAutoCreditCashAdvance()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();


                if (string.IsNullOrEmpty(txtNewCreditLimit.Text) || Convert.ToDecimal(txtNewCreditLimit.Text) == 0)
                {
                    txtNewCashAdvAmount.Enabled = false;
                    txtScheduledAllotment.Enabled = false;
                }
                else
                {
                    txtNewCashAdvAmount.Enabled = true;
                    txtScheduledAllotment.Enabled = true;
                }

                Common.getAutoCreditCashAdvance();

                txtNewCashAdvAmount.Text =  Common.NewCashAdvAmount.ToString("f") ;
                txtScheduledAllotment.Text =  Common.ScheduledAllotment.ToString("f");

                Cursor = Cursors.Default;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Application.DoEvents();

                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in getAutoCreditCashAdvance() Function CustomerID:" + Common.CustomerID);
                MessageBox.Show(ex.Message);
            }
        }

        private void txtNewCreditLimit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar("."))
            {
                e.Handled = true;
                txtNewCreditLimit.SelectionStart = (txtNewCreditLimit.Text.Length - 2);
            }
        }

        private void txtNewCreditLimit_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtNewCreditLimit.Text) || Convert.ToDecimal(txtNewCreditLimit.Text) != 0)
                {
                    //if (Convert.ToDecimal(txtNewCreditLimit.Text) != RoundLastDigitofCreditLimit(Math.Floor(Convert.ToDecimal(txtNewCreditLimit.Text))))//113012
                    if (Convert.ToDecimal(txtNewCreditLimit.Text) != Utilities.WinLookupUtils.RoundLastDigitofCreditLimit(Math.Floor(Convert.ToDecimal(txtNewCreditLimit.Text))))//113012
                    {
                        MessageBox.Show("Please enter New CreditLimit $50 Increments Only");
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        txtNewCreditLimit.Focus();
                        return;
                    }
                    else if (Convert.ToDecimal(ActualCrLimit) > Convert.ToDecimal(txtNewCreditLimit.Value)) //071212
                    {
                        txtNewCashAdvAmount.Enabled = false;
                        txtScheduledAllotment.Enabled = false;
                        txtNewCashAdvAmount.Text = "0.00";
                        txtScheduledAllotment.Text = "0.00";
                        string crLimit = txtNewCreditLimit.Text.Substring(0, txtNewCreditLimit.Text.IndexOf("."));
                        CreditLimit = Convert.ToInt32(crLimit);
                    } //071212
                    else
                    {
                        txtNewCashAdvAmount.Enabled = true;
                        txtScheduledAllotment.Enabled = true;
                        string crLimit = txtNewCreditLimit.Text.Substring(0, txtNewCreditLimit.Text.IndexOf("."));
                        CreditLimit = Convert.ToInt32(crLimit);
                        //txtNewCashAdvAmount.Focus();
                    }
                }
                else
                {
                    txtNewCashAdvAmount.Enabled = false;
                    txtScheduledAllotment.Enabled = false;
                    txtNewCreditLimit.Focus();
                    MessageBox.Show("Enter New Credit Limit");
                    return;
                }

            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Application.DoEvents();

                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in txtNewCreditLimit_Leave() Function CustomerID:" + Common.CustomerID);
                MessageBox.Show(ex.Message);
            }
        }

        private void SetCreditLimitPopup_Load(object sender, EventArgs e)
        {
            lblCurrent.Text = CreditLimit.ToString("c2");

            ActualCrLimit = CreditLimit;//071212

            getQualifiedCreditLimit();
            getAutoCreditCashAdvance();
            //if (Common.IsPaydayLoanSetting(CustomerLevelID))  //011912
            if (Common.IsPaydayLoanSetting(CustomerLevelID) == (byte)Enums.ProductType.Payday_Loan)  //011912                
            {
                lblExpectedAllotment.Text = "Expected Fixed Payment";
            }
            else
            {
                lblExpectedAllotment.Text = "Expected Allotment";
            }
        }
        private decimal RoundLastDigitofCreditLimit(decimal CreditLimit)
        {
            decimal retVal = 0;
            string limit = Convert.ToString(CreditLimit);
            int len = limit.Length;
            if (len > 2)
            {
                //Convert in string array
                string[] strArray = new string[len];
                for (int i = 0; i < len; i++)
                {
                    strArray[i] = limit[i].ToString();
                }
                //get last 2 digit
                int arrLen = strArray.Length;
                string last2digit = strArray[arrLen - 2].ToString() + strArray[arrLen - 1].ToString();
                if (Convert.ToInt32(last2digit) >= 50)
                {
                    strArray[arrLen - 2] = "5";
                    strArray[arrLen - 1] = "0";
                }
                else
                {
                    strArray[arrLen - 2] = "0";
                    strArray[arrLen - 1] = "0";
                }
                //convert into array to string
                limit = "";
                for (int j = 0; j < arrLen; j++)
                {
                    limit = limit + strArray[j].ToString();
                }
                retVal = Convert.ToDecimal(limit);
            }
            else if (len == 2)
            {
                if (CreditLimit >= 50)
                {
                    retVal = 50;
                }
                else
                {
                    retVal = 0;
                }

            }
            return retVal;
        }
        //041811
        
    }
}
