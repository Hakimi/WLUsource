//This file contains Credit check and loans creation code and explanation 

//SetCrLimit method calculates and sets credit limit of customer, when "set" button is clicked by user
//It calls SetCreditLimitPopup form to calculate and confirm credit limit, source code in SetCreditLimitPopup.cs file
//Also calls CheckBadList and CheckPreQualifyLead methods of service
//Then sets/saves credit limit and creates a new loan object, using setCreditLimit method, defines in LoansController.cs file
private void SetCrLimit()
        {
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            try
            {
                /*010511*/
                if (Common.CheckSetCreditLimitQuickApplication("") == false)
                {
                    MessageBox.Show("This action cannot be performed until the customer’s SSN and/or Home Address information is updated.  Please update info and try again.");
                    return;
                }
                /*010511*/
                //if (ddlCreditLimit.SelectedIndex == 0)
                if (string.IsNullOrEmpty(lblCreditLimit.Text)) //041211
                {
                    MessageBox.Show("Please select Credit Limit", "YesPay Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);//140410_mitesh
                    Cursor = Cursors.Default;
                    Application.DoEvents();
                    return;
                }
                else
                {
                    bool retVal;
                    string crLimit = "";
                    retVal = Common.CheckUserRight(Common.UserTypeId, "Transactions");
                    if (retVal == false)
                    {
                        //Page.ClientScript.RegisterStartupScript(typeof(Page), "alert", "<script language='javascript'> alert('You donot have proper rights to open this screen.'); </script>");
                        //ddlCreditLimit.SelectedValue = OldCreditLimit;
                        lblCreditLimit.Text = OldCreditLimit.ToString("c2"); //041211
                        MessageBox.Show("You donot have proper rights to open this screen.", "YesPay Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);//140410_mitesh
                        Cursor = Cursors.Default;
                        Application.DoEvents();
                        return;
                    }
                    SetCreditLimitPopup objSetCreditLimitPopup = new SetCreditLimitPopup();
                    crLimit = lblCreditLimit.Text.Substring(1, lblCreditLimit.Text.IndexOf(".") - 1); //041811 
                    crLimit = crLimit.Replace(",", "");
                    objSetCreditLimitPopup.CreditLimit = Convert.ToInt32(crLimit);//(int)ddlCreditLimit.SelectedValue; //041211
                    objSetCreditLimitPopup.CustomerLevelID = Convert.ToInt32(ddlCustomerLevel.SelectedValue); //051411
                    objSetCreditLimitPopup.ShowDialog();
                    objSetCreditLimitPopup.Dispose();
                    /*031711*/
                    Cursor = Cursors.WaitCursor;
                    Application.DoEvents();
                    /*031711*/
                    if (objSetCreditLimitPopup.CloseWtihSubmit == true)
                    {
                        bool retValCheckList;
                        string BadList = "";
                        retValCheckList = Common.CheckBadList(Common.CustomerID, out BadList);
                        if (retValCheckList == true)
                        {
                            ShowBadList objShowBadList = new ShowBadList();
                            objShowBadList.BadList = BadList;
                            objShowBadList.ShowDialog();
                            objShowBadList.Dispose();
                            if (objShowBadList.CloseWithSubmit == false)
                            {
                                //ddlCreditLimit.SelectedValue = OldCreditLimit;
                                lblCreditLimit.Text = OldCreditLimit.ToString("c2");//041211
                                Cursor = Cursors.Default;
                                Application.DoEvents();
                                return;
                            }
                        }
                        /*031711*/
                        Cursor = Cursors.WaitCursor;
                        Application.DoEvents();
                        /*031711*/
                        string PreQualifyError = "";
                        lblCreditLimit.Text = objSetCreditLimitPopup.CreditLimit.ToString("c2"); //041811 
                        crLimit = lblCreditLimit.Text.Substring(1, lblCreditLimit.Text.IndexOf(".") - 1); //041811 
                        retVal = Common.CheckPreQualifyLead(Common.CustomerID, Common.CustomerStoreId, Convert.ToDecimal(crLimit), out PreQualifyError); //Convert.ToInt16(ddlCreditLimit.SelectedValue); //041211
                        if (retVal == false)
                        {
                            //ddlCreditLimit.SelectedValue = OldCreditLimit;
                            lblCreditLimit.Text = OldCreditLimit.ToString("c2");//041211
                            MessageBox.Show(PreQualifyError, "YesPay Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Cursor = Cursors.Default;
                            Application.DoEvents();
                            return;
                        }

                        string SetCreditLimitError = "";
                        //retVal = Common.setCreditLimit(Convert.ToInt16(ddlCreditLimit.SelectedValue), OldCreditLimit, out SetCreditLimitError);
                        retVal = Common.setCreditLimit(Common.ApplicationID, Common.CustomerID, Convert.ToDecimal(lblCreditLimit.Text.Replace("$", "")), OldCreditLimit, Convert.ToInt16(ddlCustomerLevel.SelectedValue), Common.UserID, objSetCreditLimitPopup.NewCashAdvAmount, objSetCreditLimitPopup.ScheduledAllotment, Common.CustomerStoreId, Common.IPAddress(), out  SetCreditLimitError); //Convert.ToInt16(ddlCreditLimit.SelectedValue); //041211

                        if (retVal == false)
                        {
                            //ddlCreditLimit.SelectedValue = OldCreditLimit;
                            lblCreditLimit.Text = OldCreditLimit.ToString("c2");//041211
                            //RadAjaxManagerCP.Alert("Selected credit limit exceeded allowable limit for this customer.");
                            MessageBox.Show(SetCreditLimitError, "YesPay Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Cursor = Cursors.Default;
                            Application.DoEvents();
                            return;
                        }
                        else
                        {
                            btnAttachment.Enabled = true;
                            FirstTime = true;//140410_mitesh
                            FillAllData();
                            
                            FirstTime = false;//140410_mitesh
                        }
                        HidePaymentDueButton(); //06292011
                        string strCreditLimit = string.Empty;
                        switch (Common.CustomerStoreId)
                        {
                            case 1:
                            case 7:
                            case 8:
                            case 10:
                            case 12:
                            case 13:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 21: //05282011
                            //080912 7.31
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                                //080912 7.31
                                strCreditLimit = System.Configuration.ConfigurationManager.AppSettings["SetCreditLimitCash1"];
                                MessageBox.Show("Credit Limit Set Successfuly ", "YesPay Admin", MessageBoxButtons.OK);//140410_mitesh
                                //ResponseHelper.Redirect(strCreditLimit, "_blank", "menubar=0,width=300,height=160,scrollbars=no", false);
                                //System.Diagnostics.Process.Start(strCreditLimit);
                                break;
                            case 20:
                                strCreditLimit = System.Configuration.ConfigurationManager.AppSettings["SetCreditLimitFastCashNation"];
                                MessageBox.Show("Credit Limit Set Successfuly ", "YesPay Admin", MessageBoxButtons.OK);//140410_mitesh
                                //ResponseHelper.Redirect(strCreditLimit, "_blank", "menubar=0,width=300,height=160,scrollbars=no", false);
                                //System.Diagnostics.Process.Start(strCreditLimit);
                                break;
                            case 2:
                                strCreditLimit = System.Configuration.ConfigurationManager.AppSettings["SetCreditLimitFAC"];
                                MessageBox.Show("Credit Limit Set Successfuly ", "YesPay Admin", MessageBoxButtons.OK);//140410_mitesh
                                //ResponseHelper.Redirect(strCreditLimit, "_blank", "menubar=0,width=300,height=160,scrollbars=no", false);
                                //System.Diagnostics.Process.Start(strCreditLimit);
                                break;
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                strCreditLimit = System.Configuration.ConfigurationManager.AppSettings["SetCreditLimitRetailAmerica"];
                                MessageBox.Show("Credit Limit Set Successfuly ", "YesPay Admin", MessageBoxButtons.OK);//140410_mitesh
                                //System.Diagnostics.Process.Start(strCreditLimit);
                                //ResponseHelper.Redirect(strCreditLimit, "_blank", "menubar=0,width=300,height=160,scrollbars=no", false);
                                break;
                            case 9:
                            case 11:
                            case 14:
                            case 19:
                                strCreditLimit = System.Configuration.ConfigurationManager.AppSettings["SetCreditLimitCashMart"];
                                MessageBox.Show("Credit Limit Set Successfuly ", "YesPay Admin", MessageBoxButtons.OK);//140410_mitesh
                                //ResponseHelper.Redirect(strCreditLimit, "_blank", "menubar=0,width=300,height=160,scrollbars=no", false);
                                //System.Diagnostics.Process.Start(strCreditLimit);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in btnCreditLimit_Click() Function CustomerID:"+ Common.CustomerID  ); //031511
                MessageBox.Show(ex.ToString(), "YesPay Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);//140410_mitesh
            }
            finally
            {
                Cursor = Cursors.Default;
                Application.DoEvents();
            }
        }


//CheckBadList method of service called from SetCrLimit
public bool CheckBadList(int iCustomerId)
        {
            bool bChecklist = false;
            //string Bad_Str = string.Empty; 
            try
            {
                using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString, System.Data.IsolationLevel.ReadUncommitted))
                {
                    GetPersonalInfoData(iCustomerId);
                    if (_CustomerPersonalInfo != null)
                    {

                        var iSSN = (from BSSN in db.BadSSNs where BSSN.BadSSN1 == _CustomerPersonalInfo.SSN select BSSN).FirstOrDefault();
                        if (iSSN != null)
                        {

                            _Bad_Str += "<b>Bad SSN:</b> " + iSSN.BadSSN1 + " </br>";
                            bChecklist = true;
                        }
                        GetBankInfoDataAll(iCustomerId);

                        foreach (BankAccountInfo BA in _BankAccInfoList)
                        {
                            var bankABA = (from BABA in db.BadBankABANos where BABA.BadBankABA == BA.ABARoutingNo select BABA).FirstOrDefault();
                            if (bankABA != null)
                            {
                                _Bad_Str += "<b>Bad Bank ABA:</b>" + bankABA.BadBankABA + " </br>";
                                bChecklist = true;
                            }
                        }

                        foreach (BankAccountInfo BA in _BankAccInfoList)
                        {
                            var BankAcc = (from BAc in db.BadAccounts where BAc.BadBankAccount == BA.BankAccountNo select BAc).FirstOrDefault();
                            if (BankAcc != null)
                            {
                                _Bad_Str += "<b>Bad Bank Account:</b> " + BankAcc.BadBankAccount + " </br>";
                                bChecklist = true;
                            }
                        }
                        var email = (from BEmail in db.BadEmailAddresses select BEmail).ToList();
                        GetCustomerMasterData(iCustomerId);
                        if (email != null)
                        {
                            string BadEmail = string.Empty;
                            foreach (BadEmailAddress em in email)
                            {
                                if (_CustomerMasterInfo.EmailId.Contains(em.EmailAddress))
                                {
                                    BadEmail += " " + em.EmailAddress + ",";
                                    bChecklist = true;
                                }
                                if (_CustomerMasterInfo.UserName.Contains(em.EmailAddress))
                                {
                                    BadEmail += " " + em.EmailAddress + ",";
                                    bChecklist = true;
                                }

                            }
                            if (!string.IsNullOrEmpty(BadEmail))
                            {
                                _Bad_Str += "<b>Bad Email </b>" + _CustomerMasterInfo.UserName + ", " + _CustomerMasterInfo.EmailId + " (" + BadEmail + ") </br>";
                            }
                        }
                        GetEmployerMasterData(iCustomerId);
                        //_EmployerMaster 
                        //var EmpTA = (from EmpMaster in db.BadTempAgencies where EmpMaster.TempAgencie.Contains(_EmployerMaster.EmployerName) select EmpMaster).ToList();
                        if (!string.IsNullOrEmpty(_EmployerMaster.EmployerName))
                        {
                            var EmpTA = db.GetBadTempAgencies(_EmployerMaster.EmployerName).ToList();
                            string BadEmp = string.Empty;
                            if (EmpTA != null)
                            {
                                string BTA = string.Empty;
                                foreach (BadTempAgency BT in EmpTA)
                                {
                                    BTA += "" + BT.TempAgencie + ",";
                                    bChecklist = true;
                                }
                                if (!string.IsNullOrEmpty(BTA))
                                {
                                    BTA = BTA.Substring(0, BTA.Length - 1);
                                    BadEmp += " <b>Bad Temp Agencie:</b> " + BTA;
                                }
                            }
                            //var EmpRest = (from EmpMaster in db.BadRestaurants where EmpMaster.Restaurant.Contains(_EmployerMaster.EmployerName) select EmpMaster).ToList();
                            var EmpRest = db.GetBadRestaurants(_EmployerMaster.EmployerName).ToList();
                            if (EmpRest != null)
                            {
                                string BR = string.Empty;
                                foreach (BadRestaurant Rest in EmpRest)
                                {
                                    BR += " " + Rest.Restaurant + ",";
                                    bChecklist = true;
                                }

                                if (!string.IsNullOrEmpty(BR))
                                {
                                    BR = BR.Substring(0, BR.Length - 1);
                                    BadEmp += " <b>Bad Restaurant:</b> " + BR;
                                }
                            }

                            //var EmpTD = (from EmpMaster in db.BadTruckDrivers  where EmpMaster.TruckDriver.Contains(_EmployerMaster.EmployerName) select EmpMaster).ToList();
                            var EmpTD = db.GetBadTruckDrivers(_EmployerMaster.EmployerName).ToList();
                            if (EmpTD != null)
                            {
                                string BTD = string.Empty;
                                foreach (BadTruckDriver TD in EmpTD)
                                {
                                    BTD += "" + TD.TruckDriver + ",";
                                    bChecklist = true;
                                }
                                if (!string.IsNullOrEmpty(BTD))
                                {
                                    BTD = BTD.Substring(0, BTD.Length - 1);
                                    BadEmp += " <b>Bad Truck Driver:</b> " + BTD;
                                }
                            }
                            //var BadEmpWord = (from EmpMaster in db.BadWords  select EmpMaster).ToList();
                            var BadEmpWord = db.GetBadWords(_EmployerMaster.EmployerName).ToList();
                            if (BadEmpWord != null)
                            {
                                string BEW = string.Empty;
                                foreach (BadWord BW in BadEmpWord)
                                {
                                    if (_EmployerMaster.EmployerName.Contains(BW.Word))
                                    {
                                        BEW += "" + BW.Word + ",";
                                        bChecklist = true;
                                    }
                                }
                                if (!string.IsNullOrEmpty(BEW))
                                {
                                    BEW = BEW.Substring(0, BEW.Length - 1);
                                    BadEmp += " <b>Bad Word:</b>  " + BEW;
                                }
                            }
                            if (!string.IsNullOrEmpty(BadEmp))
                            {
                                _Bad_Str += "<b> Employer: </b> " + _EmployerMaster.EmployerName + " (" + BadEmp + ") </br>";
                            }
                        }
                        GetReferencesMasterData(iCustomerId);
                        string sBadRef = string.Empty;
                        foreach (ReferencesMaster RM in _ReferencesMaster)
                        {
                            if (!string.IsNullOrEmpty(RM.FullName) || !string.IsNullOrEmpty(RM.LastName))
                            {
                                var BadRef = (from Ref in db.BadReferences where Ref.FirstName.StartsWith(RM.FullName) && Ref.LastName.StartsWith(RM.LastName) select Ref).ToList();
                                sBadRef = "";
                                foreach (BadReference BR in BadRef)
                                {
                                    sBadRef += "(" + BR.FirstName + " , " + BR.LastName + " , " + BR.City + " , " + BR.State + " , " + BR.Phone + ")";
                                    bChecklist = true;

                                }
                                if (!string.IsNullOrEmpty(sBadRef))
                                {
                                    _Bad_Str += "<b>Bad References:</b> " + RM.FullName + ", " + RM.LastName + ", " + RM.City + ", " + RM.State + ", " + RM.PhoneNumber + ", " + sBadRef + "</br>";
                                }
                            }
                        }
                        // For Bad Phone
                        var BadPhoneH = (from P in db.BadPhoneNos where P.BadPhoneNumber == _CustomerPersonalInfo.HomePhone select P).FirstOrDefault();
                        string BHomePhone = string.Empty;
                        if (BadPhoneH != null)
                        {
                            BHomePhone = "<b>Bad Home Phone:</b>" + _CustomerPersonalInfo.HomePhone + "(" + BadPhoneH.BadPhoneNumber + ")";
                        }
                        if (!string.IsNullOrEmpty(BHomePhone))
                        {
                            _Bad_Str += BHomePhone + " </br> ";
                            bChecklist = true;
                        }
                        var BadPhoneW = (from P in db.BadPhoneNos where P.BadPhoneNumber == _CustomerPersonalInfo.WorkPhone select P).FirstOrDefault();
                        string BWorkPhone = string.Empty;
                        if (BadPhoneW != null)
                        {
                            BWorkPhone = "<b>Bad Work Phone:</b>" + _CustomerPersonalInfo.WorkPhone + "(" + BadPhoneW.BadPhoneNumber + ")";
                        }
                        if (!string.IsNullOrEmpty(BWorkPhone))
                        {
                            _Bad_Str += BWorkPhone + " </br> ";
                            bChecklist = true;
                        }
                        var BadPhoneC = (from P in db.BadPhoneNos where P.BadPhoneNumber == _CustomerPersonalInfo.CellNumber select P).FirstOrDefault();
                        string BCellPhone = string.Empty;
                        if (BadPhoneC != null)
                        {
                            BCellPhone = "<b>Bad Cell Phone:</b>" + _CustomerPersonalInfo.CellNumber + "(" + BadPhoneC.BadPhoneNumber + ")";
                        }
                        if (!string.IsNullOrEmpty(BCellPhone))
                        {
                            _Bad_Str += BCellPhone + " </br> ";
                            bChecklist = true;
                        }
                        var BadPhoneSP = (from P in db.BadPhoneNos where P.BadPhoneNumber == _CustomerPersonalInfo.SpouseWorkPhone select P).FirstOrDefault();
                        string BPhoneSP = string.Empty;
                        if (BadPhoneSP != null)
                        {
                            BPhoneSP = "<b>Bad Spouse Work Phone:</b>" + _CustomerPersonalInfo.SpouseWorkPhone + "(" + BadPhoneSP.BadPhoneNumber + ")";
                        }
                        if (!string.IsNullOrEmpty(BPhoneSP))
                        {
                            _Bad_Str += BPhoneSP + " </br> ";
                            bChecklist = true;
                        }

                        string BPhoneRef = string.Empty;
                        foreach (ReferencesMaster RM in _ReferencesMaster)
                        {
                            var BadPhoneRef = (from P in db.BadPhoneNos where P.BadPhoneNumber == RM.PhoneNumber select P).FirstOrDefault();

                            if (BadPhoneRef != null)
                            {
                                BPhoneRef += "<b>Bad Ref. Phone:</b>" + RM.PhoneNumber + "(" + BadPhoneRef.BadPhoneNumber + ")</br>";
                            }
                        }
                        if (!string.IsNullOrEmpty(BPhoneRef))
                        {
                            _Bad_Str += BPhoneRef + " </br> ";
                            bChecklist = true;
                        }

                        string BPhoneBank = string.Empty;
                        foreach (BankAccountInfo BankA in _BankAccInfoList)
                        {
                            var BadPhoneBank = (from P in db.BadPhoneNos where P.BadPhoneNumber == BankA.BankphoneNo select P).FirstOrDefault();

                            if (BadPhoneBank != null)
                            {
                                BPhoneRef = "<b>Bad Bank Phone:</b>:" + BankA.BankphoneNo + "(" + BadPhoneBank.BadPhoneNumber + ")</br>";
                            }
                        }
                        if (!string.IsNullOrEmpty(BPhoneBank))
                        {
                            _Bad_Str += BPhoneBank + " </br> ";
                            bChecklist = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Error_Logger oLogger = new Error_Logger();
                oLogger.logError(ex.ToString(), "Error in CheckBadList function");
            }
            return bChecklist;
        }

//MNI calculation based on last pay check amount and pay frequency
public decimal Calc_MNI(DateTime LastP, DateTime NextP, decimal? LastPay)
        {
            int Days = 0;
            //YesPay.Admin.Enums.CustomerPaydays Paydays = Utilities.CalculatePaydays.CalcPayFrequency(LastP, NextP); //011813
            YesPay.Admin.Enums.CustomerPaydays Paydays = CalcPayFrequency(LastP, NextP); //011813
            switch (Paydays)
            {
                case YesPay.Admin.Enums.CustomerPaydays.None:
                    Days = 14;
                    break;
                case YesPay.Admin.Enums.CustomerPaydays.BIWEEKLY:
                    Days = 14;
                    break;
                case YesPay.Admin.Enums.CustomerPaydays.WEEKLY:
                    Days = 7;
                    break;
                case YesPay.Admin.Enums.CustomerPaydays.TWICE_A_MONTH:
                    Days = 15;
                    break;
                case YesPay.Admin.Enums.CustomerPaydays.MONTHLY:
                    Days = 30;
                    break;
                case YesPay.Admin.Enums.CustomerPaydays.Other:
                    Days = 14;
                    break;
                default:
                    Days = 14;
                    break;
            }
            decimal MNI = 0;
            if (LastPay != null)    //080409
            {
                MNI = (decimal)((30 * LastPay) / Days);
            }
            //MNI = Math.Round(MNI, (int)2);//022412
            MNI = Round(MNI, null);//022412
            return MNI;
        }

//CheckPreQualifyLead performs pre-qualification based on admin settings
public bool CheckPreQualifyLead(int CustomerId, int StoreId, decimal CreditLimit, bool FromLead)
        {
            bool retVal = true;
            CalculatePaydays _CalculatePaydays = new CalculatePaydays(); //011813
            try
            {
                using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString, System.Data.IsolationLevel.ReadUncommitted))
                {
                    var PreQ = (from PQ in db.PreQualifies where PQ.Store_ID == StoreId select PQ).FirstOrDefault();
                    var CEMP = (from Emp in db.EmployerMasters where Emp.CustomerID == CustomerId select Emp).FirstOrDefault();
                    var PInfo = (from PI in db.CustomerPersonalInfos where PI.CustomerID == CustomerId select PI).FirstOrDefault();
                    var POtherInfo = (from PI in db.CustomerOtherInfos where PI.CustomerID == CustomerId select PI).FirstOrDefault();
                    var CM = (from C in db.CustomerMasters where C.CustomerID == CustomerId select C).FirstOrDefault();


                    //082611
                    //decimal MNI = Calculation_Engine.Calc_MNI(Convert.ToDateTime(CEMP.LastPayDate), Convert.ToDateTime(CEMP.NextPayDate), (decimal)CEMP.AmountofLastPayCheck);
                    decimal MNI = 0;
                    if (CEMP.MonthlyNetIncome == 0)
                    {
                        //MNI = Calculation_Engine.Calc_MNI(Convert.ToDateTime(CEMP.LastPayDate), Convert.ToDateTime(CEMP.NextPayDate), (decimal)CEMP.AmountofLastPayCheck);//011713
                        MNI = _clsCalculationEngine.Calc_MNI(Convert.ToDateTime(CEMP.LastPayDate), Convert.ToDateTime(CEMP.NextPayDate), (decimal)CEMP.AmountofLastPayCheck);//011713
                    }
                    else
                    {
                        MNI = CEMP.MonthlyNetIncome;
                    }
                    //082611
                    int Days = 0;
                     
                    //YesPay.Admin.Enums.CustomerPaydays Paydays = Utilities.CalculatePaydays.CalcPayFrequency(Convert.ToDateTime(CEMP.LastPayDate), Convert.ToDateTime(CEMP.NextPayDate)); //011813 
                    YesPay.Admin.Enums.CustomerPaydays Paydays = _CalculatePaydays.CalcPayFrequency(Convert.ToDateTime(CEMP.LastPayDate), Convert.ToDateTime(CEMP.NextPayDate)); //011813 
                    switch (Paydays)
                    {
                        case YesPay.Admin.Enums.CustomerPaydays.None:
                            CEMP.PayFrequency = 1;
                            Days = 14;
                            break;
                        case YesPay.Admin.Enums.CustomerPaydays.BIWEEKLY:
                            CEMP.PayFrequency = 1;
                            Days = 14;
                            break;
                        case YesPay.Admin.Enums.CustomerPaydays.WEEKLY:
                            CEMP.PayFrequency = 2;
                            Days = 7;
                            break;
                        case YesPay.Admin.Enums.CustomerPaydays.TWICE_A_MONTH:
                            CEMP.PayFrequency = 3;
                            Days = 15;
                            break;
                        case YesPay.Admin.Enums.CustomerPaydays.MONTHLY:
                            CEMP.PayFrequency = 4;
                            Days = 30;
                            break;
                        case YesPay.Admin.Enums.CustomerPaydays.Other:
                            CEMP.PayFrequency = 1;
                            Days = 14;
                            break;
                        default:
                            CEMP.PayFrequency = 1;
                            Days = 14;
                            break;
                    }

                    TimeSpan ts = DateTime.Now.Date.Subtract(Convert.ToDateTime(CEMP.HireDate).Date);
                    //TimeSpan tsHSince = DateTime.Now.Date.Subtract(Convert.ToDateTime(PInfo.CurrentAddress).Date);
                    if (PreQ != null)
                    {
                        if (CEMP.PayFrequency != null)
                        {
                            //Biweekly 1
                            //Weekly 2
                            //Twoce a Month 3
                            //Monthly 4
                            //Other 5

                            if (((PreQ.PFBInternet == null) || (PreQ.PFBInternet == false)) &&
                                ((PreQ.PayFreMaxInternetWeekly == null) || (PreQ.PayFreMaxInternetWeekly == false)) &&
                                ((PreQ.PFTInternet == null) || (PreQ.PFTInternet == false)) &&
                                ((PreQ.PayFreMaxInternetMonthly == null) || (PreQ.PayFreMaxInternetMonthly == false)))
                            {
                                retVal = true;
                            }
                            else
                            {
                                if ((PreQ.PFBInternet == true) &&
                                    (PreQ.PayFreMaxInternetWeekly == true) &&
                                    (PreQ.PFTInternet == true) &&
                                    (PreQ.PayFreMaxInternetMonthly == true))
                                {
                                    retVal = true;
                                }
                                else
                                {
                                    retVal = false;
                                    if (CEMP.PayFrequency == 1)
                                    {
                                        if (PreQ.PFBInternet != null)
                                        {
                                            if (PreQ.PFBInternet == true)
                                            {
                                                retVal = true;
                                            }
                                        }
                                    }
                                    if (CEMP.PayFrequency == 2)
                                    {
                                        if (PreQ.PayFreMaxInternetWeekly != null)
                                        {
                                            if (PreQ.PayFreMaxInternetWeekly == true)
                                            {
                                                retVal = true;
                                            }
                                        }
                                    }
                                    if (CEMP.PayFrequency == 3)
                                    {
                                        if (PreQ.PFTInternet != null)
                                        {
                                            if (PreQ.PFTInternet == true)
                                            {
                                                retVal = true;
                                            }
                                        }
                                    }
                                    if (CEMP.PayFrequency == 4)
                                    {
                                        if (PreQ.PayFreMaxInternetMonthly != null)
                                        {
                                            if (PreQ.PayFreMaxInternetMonthly == true)
                                            {
                                                retVal = true;
                                            }
                                        }
                                    }

                                    if (retVal == false)
                                    {
                                        ErrorMessage += "Pre-Qualify: Pay frequency of Customer is not allowed. So the Customer is disqualified. \\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            retVal = false;
                            ErrorMessage += "Pre-Qualify: Pay frequency of Customer is not set. So the Customer is disqualified. \\n";
                        }

                        if (PreQ.MonthlyNetIncomeInternet != null)
                        {
                            if (MNI < PreQ.MonthlyNetIncomeInternet)
                            {
                                ErrorMessage += "Pre-Qualify: Monthly Net Income is " + MNI.ToString("F2") + "(((30 * LastPay(" + CEMP.AmountofLastPayCheck.Value.ToString("F2") + ")) / Days(" + Days + ") )  less then required: MNI(" + PreQ.MonthlyNetIncomeInternet.Value.ToString("F2") + ").  So the Customer is disqualified \\n ";
                                retVal = false;
                            }
                        }

                        if (PreQ.HireDayInternet != null)
                        {
                            //022411 if (PreQ.HireDayInternet > ts.Days)
                            if (PreQ.HireDayInternet > ts.TotalDays)
                            {
                                ErrorMessage += "Pre-Qualify: Days since Customer has been Hire Date( " + ts.TotalDays + " ) is less then required Days: Day(" + PreQ.HireDayInternet + ") So the Customer is disqualify. \\n ";
                                retVal = false;
                            }
                        }

                        if (StoreId != 2) // 032312 7.20 no need to check for 200
                        {
                            if (PreQ.YearInternet != null)
                            {
                                if (PInfo.DOB != null)
                                {
                                    if (PreQ.YearInternet > (DateTime.Now.Date.Year - PInfo.DOB.Value.Year))
                                    {
                                        //ErrorMessage += "DOB Years (" + PreQ.YearInternet + ") is less then Pre-Qualify DOB Years(" + PreQ.YearInternet + "). So that Customer is disqualify for DOB Years  \\n ";
                                        ErrorMessage += "Pre-Qualify:  Age of Customer Years (" + (DateTime.Now.Date.Year - PInfo.DOB.Value.Year) + ") is less than required Age: Years(" + PreQ.YearInternet + "). So the Customer is disqualified.";
                                        retVal = false;
                                    }
                                }
                                else
                                {
                                    ErrorMessage += "Pre-Qualify:  Age of Customer Years (0) is less than required Age: Years(" + PreQ.YearInternet + "). So the Customer is disqualified.";
                                    retVal = false;
                                }
                            }
                        }
                        if (FromLead == false)
                        {
                            if ((PreQ.MaxCreditMNIInternet != null) && (CEMP.AmountofLastPayCheck != null) && (CEMP.LastPayDate != null) && (CEMP.NextPayDate != null))
                            {
                                //decimal CustomerMNI = Calculation_Engine.Calc_MNI(CEMP.LastPayDate.Value, CEMP.NextPayDate.Value, CEMP.AmountofLastPayCheck);//101713
                                decimal CustomerMNI = _clsCalculationEngine.Calc_MNI(CEMP.LastPayDate.Value, CEMP.NextPayDate.Value, CEMP.AmountofLastPayCheck);//101713
                                decimal? CalcLimit = ((PreQ.MaxCreditMNIInternet * CustomerMNI) / 100);
                                if (CalcLimit < CreditLimit)
                                {
                                    // ErrorMessage += "Max Credit % of MNI (" + CreditLimit + ") is less then Pre-Qualify Max Credit % of MNI ((( Max Credit of MNI(" + PreQ.MaxCreditMNIInternet + ") *  Amount of Last Pay Check (" + CEMP.AmountofLastPayCheck + ")) / 100) < CreditLimit )" + ((PreQ.MaxCreditMNIInternet * CEMP.AmountofLastPayCheck) / 100) + ".  SO that Customer is disqualify for Max Credit % of MNI \\n ";
                                    //ErrorMessage += "Max Credit % of MNI (" + CreditLimit + ") is less then Pre-Qualify Max Credit % of MNI ((( Max Credit of MNI(" + PreQ.MaxCreditMNIInternet.Value.ToString("F2") + ") *  Amount of Last Pay Check (" + CEMP.AmountofLastPayCheck.Value.ToString("F2") + ")) / 100) < CreditLimit )" + ((PreQ.MaxCreditMNIInternet * CEMP.AmountofLastPayCheck) / 100).Value.ToString("F2") + ".  So the Customer does not qualify for requested credit limit of " + CreditLimit.ToString("F2") + " \\n";
                                    ErrorMessage += "Requested Credit Limit: (" + CreditLimit + ") is more than " + CalcLimit.Value.ToString("F2") + ". Pre-Qualify: [Max Credit % of MNI ((( Max Credit of MNI(" + PreQ.MaxCreditMNIInternet.Value.ToString("F2") + ") *  Amount of Last Pay Check (" + CEMP.AmountofLastPayCheck.Value.ToString("F2") + ")) / 100) < CreditLimit )]" + ".  So the Customer does not qualify for requested credit limit of " + CreditLimit.ToString("F2") + " \\n";
                                    retVal = false;
                                }
                            }
                        }

                        if (PreQ.DirectDepositRequiredInternet != null)
                        {
                            if (PreQ.DirectDepositRequiredInternet == true)
                            {
                                if (CEMP.DirectDeposit == false)
                                {
                                    //ErrorMessage += "Pre-Qualify Direct Required is Required and This Customer geting Pay Check. So that Customer is disqualify for Direct Required \\n ";
                                    ErrorMessage += "Pre-Qualify: Customer does not have Direct Deposit. So the Customer is disqualified. \\n";

                                    retVal = false;
                                }
                            }
                        }
                        if (PreQ.CantReachAtWorkInternet != null)
                        {
                            if (PreQ.CantReachAtWorkInternet == true)
                            {
                                if (PInfo.WorkPhoneDNC == true)
                                {
                                    //ErrorMessage += "Pre-Qualify Cant Reach at work is Required and This Customer have DNC So that Customer is disqualify for Cant Reach at work  \\n ";
                                    ErrorMessage += "Pre-Qualify: Customer cannot be reached at work, as customer opted for DNC. So the Customer is disqualified. \\n";
                                    retVal = false;
                                }
                            }
                        }

                        if (PreQ.Valid2RefReqInternet != null)
                        {
                            if (PreQ.Valid2RefReqInternet > 0)
                            {
                                var RefCount = (from Ref in db.ReferencesMasters where Ref.CustomerID == CustomerId select Ref).Count();
                                if (PreQ.Valid2RefReqInternet >= RefCount)
                                {
                                    //ErrorMessage += "Pre-Qualify Valid References is Required " + PreQ.Valid2RefReqInternet + " References and This Customer have " + RefCount + " Valid References So that Customer is disqualify for Valid References  \\n ";
                                    ErrorMessage += "Pre-Qualify: " + PreQ.Valid2RefReqInternet + " Valid References are required, but Customer has only " + RefCount + " Valid References. So the Customer is disqualified. \\n";
                                    retVal = false;
                                }
                            }
                        }

                        if (PreQ.OpenValidBackAcctReqInternet != null)
                        {
                            if (PreQ.OpenValidBackAcctReqInternet == true)
                            {
                                var BankAccount = (from B in db.BankAccountInfos
                                                   where B.CustomerID == CustomerId
                                                       && B.IsDelete == false // 05042011
                                                   where (B.BankAccountNo != "") && (B.BankAccountStatus == true)
                                                   select B).Count();
                                if (BankAccount == 0)
                                {
                                    //ErrorMessage += "Pre-Qualify Bank Account Status is Required Open Account and This Customer have Close Account. So that Bank Account Status is disqualify  \\n ";
                                    ErrorMessage += "Pre-Qualify: Customer does not have an Open Bank Account. So the Customer is disqualified. \\n";
                                    retVal = false;
                                }
                            }
                        }
                        if (PreQ.ValidCreditCardReqInternet != null)
                        {
                            if (PreQ.ValidCreditCardReqInternet == true)
                            {
                                var BankCard = (from B in db.BankAccountInfos
                                                where B.CustomerID == CustomerId && B.IsDelete == false // 05042011
                                                && B.BankCardNumber != ""
                                                select B).Count();

                                if (BankCard == 0)
                                {
                                    //ErrorMessage += "Pre-Qualify Credit Card is Required Valid Credit Card and This Customer have not Valid Credit Card. So that Customer is disqualify for Valid Credit Card  \\n ";
                                    ErrorMessage += "Pre-Qualify: Customer does not have not Valid Credit Card. So the Customer is disqualified. \\n";
                                    retVal = false;
                                }
                            }
                        }
                        if (StoreId != 2) // 032312 7.20 no need to check for 200
                        {
                            if (PreQ.BankAccountInternet != null)
                            {/*050310_Mitesh*/
                                //if (PreQ.BankAccountInternet == true)
                                //{
                                //    var BankType = (from B in db.BankAccountInfos where B.CustomerID == CustomerId where B.AccountType == 1 select B).Count();
                                //    if (BankType == 0)
                                //    {

                                //        //ErrorMessage += "Pre-Qualify Bank Account Type is Required Savings Account Type and This Customer have Checkings Account Type . So that Customer is disqualify for Bank Account Type  \\n ";
                                //        ErrorMessage += "Pre-Qualify:  Customer does not have Checking’s Account. So the Customer is disqualified. \\n";
                                //        retVal = false;
                                //    }
                                //}
                                if (PreQ.BankAccountInternet == false)
                                {
                                    var BankType = (from B in db.BankAccountInfos
                                                    where B.CustomerID == CustomerId
                                                        && B.IsDelete == false // 05042011
                                                    //where B.AccountType == 1 //083011
                                                    where B.AccountType == 2  //083011
                                                    select B).Count();
                                    //if (BankType != 0) //083011
                                    if (BankType == 0) //083011
                                    {

                                        //ErrorMessage += "Pre-Qualify Bank Account Type is Required Savings Account Type and This Customer have Checkings Account Type . So that Customer is disqualify for Bank Account Type  \\n ";
                                        ErrorMessage += "Pre-Qualify:  Customer does not have Checking’s Account. So the Customer is disqualified. \\n";//031711
                                        retVal = false;
                                    }
                                }
                                /*050310_Mitesh*/
                            }
                        }
                        if (PreQ.ValiadHomePhoneReqInternet != null)
                        {
                            if (PreQ.ValiadHomePhoneReqInternet == true)
                            {
                                if (PInfo.HomePhone.Length == 0)
                                {
                                    // ErrorMessage += "Pre-Qualify Home Phone  is Required Home Phone and This Customer have not Home Phone. So that Customer is disqualify for Home Phone  \\n ";
                                    ErrorMessage += "Pre-Qualify:  Customer does not have a home phone number. So the Customer is disqualified. \\n";
                                    retVal = false;
                                }
                            }
                        }
                        // Tarun 010910
                        if (CEMP != null)
                        {
                            if (PreQ.MortgageRentAmount != 0)
                            {
                                if (CEMP.MonthlyMortgageRentAmount >= PreQ.MortgageRentAmount)
                                {
                                    //ErrorMessage += "Pre-Qualify: Customer does not have a Mortgage Rent Amount. So the Customer is disqualified. \\n ";
                                    ErrorMessage += "Pre-Qualify:  Mortgage Rent Amount of Customer is disqualified.";
                                    retVal = false;
                                }
                            }
                        }
                        if (POtherInfo != null)
                        {
                            //CurrentBankBalance
                            if (PreQ.CurrentBankBalance != 0)
                            {
                                if (POtherInfo.CurrentBankBalance < PreQ.CurrentBankBalance)
                                {
                                    //ErrorMessage += "Pre-Qualify: Customer does not have a Current Bank Balance. So the Customer is disqualified. \\n ";
                                    ErrorMessage += "Pre-Qualify:  Bank Balance of Customer is disqualified.";
                                    retVal = false;
                                }
                            }
                            //NoOfNSF
                            if (PreQ.NoOfNSF <= POtherInfo.NSFLastBankStatement)
                            {
                                //ErrorMessage += "Pre-Qualify: Customer does not have a NSF Last Bank Statement. So the Customer is disqualified. \\n ";
                                ErrorMessage += "Pre-Qualify:  Mortgage NSF Last Bank Statement of Customer is disqualified.";
                                retVal = false;
                            }
                            //NumberOfOutstandingLoans
                            if (PreQ.NumberOfOutstandingLoans <= POtherInfo.NoOfOutstandingLoans)
                            {
                                //ErrorMessage += "Pre-Qualify: Customer does not have a Number Of Out standing Loans. So the Customer is disqualified. \\n ";
                                ErrorMessage += "Pre-Qualify:  Number Of Out standing Loans of Customer is disqualified.";
                                retVal = false;
                            }
                            //TotalPaydayLoansOutstanding
                            if (PreQ.TotalPaydayLoansOutstanding != 0)
                            {
                                if (POtherInfo.PaydayLoansOutstanding >= PreQ.TotalPaydayLoansOutstanding)
                                {
                                    //ErrorMessage += "Pre-Qualify: Customer does not have a Payday Loans Out standing. So the Customer is disqualified. \\n ";
                                    ErrorMessage += "Pre-Qualify:  Payday Loans Out standing of Customer is disqualified.";
                                    retVal = false;
                                }
                            }
                        }
                        //EmailAddressMonths  

                        if (CM != null)
                        {
                            if (StoreId != 2) // 032312 7.20 no need to check for 200
                            {
                                int EmailAddressMonths = (DateTime.Now.Month - Convert.ToDateTime(CM.CurrentEmailAddressSince).Month) + (12 * (DateTime.Now.Year - Convert.ToDateTime(CM.CurrentEmailAddressSince).Year));

                                if (PreQ.DateEmailAddress >= EmailAddressMonths)
                                {
                                    ErrorMessage += "Pre-Qualify: Email Address Months of Customer is disqualified.";
                                    retVal = false;
                                }
                            }
                            //AllowMilitaryMember
                            /*050310_Mitesh*/
                            //if (PreQ.AllowMilitaryMember == true && CM.ArmedForcesDisclosure != 0)
                            if (PreQ.AllowMilitaryMember == false && CM.ArmedForcesDisclosure != 0)
                            /*050310_Mitesh*/
                            {
                                ErrorMessage += "Pre-Qualify: Allow Military Member of Customer is disqualified.";
                                retVal = false;
                            }
                            //if (PreQ.AllowMilitaryMember == false && CM.ArmedForcesDisclosure != 0)
                            //{
                            //    ErrorMessage += "Pre-Qualify: Allow Military Member of Customer is disqualified.";
                            //    retVal = false;
                            //}
                        }


                        //

                        if (PreQ.DateHomeSinceInternet != null)
                        {
                            if (PreQ.DateHomeSinceInternet >= monthDifference(DateTime.Now, Convert.ToDateTime(PInfo.CurrentAddress)))
                            {
                                //ErrorMessage += "Pre-Qualify Address Since is Required Address Since and This Customer have not Address Since. So that Customer is disqualify for Address Since  \\n ";                                    
                                ErrorMessage += "Pre-Qualify:  How long Customer is at this Address Since, has not been entered.  So the Customer is disqualified. \\n";
                                retVal = false;
                            }
                        }
                        if (StoreId != 2) // 032312 7.20 no need to check for 200
                        {
                            int CountPreQ = (from P in db.PreQualifyStateInternets where P.State == PInfo.State && P.StoreId.Value == StoreId select P).Count();
                            if (CountPreQ == 0)
                            {
                                //ErrorMessage += PInfo.State + " State is not in Eligible States list. So that Customer is disqualify for " +  PInfo.State + " State \\n ";
                                ErrorMessage += PInfo.State + " State is not in the list of Eligible States. So the Customer is disqualified. \\n";
                                retVal = false;
                            }
                        }
                        //if (PInfo.State == "NV" || PInfo.State == "WA" || PInfo.State == "AZ")
                        //{
                        //    if (PreQ.StateNVInternet != null)
                        //    {
                        //        if (PreQ.StateNVInternet == false && PInfo.State == "NV")
                        //        {
                        //            ErrorMessage += "State NV is disqualify \\n ";
                        //            retVal = false;
                        //        }
                        //    }
                        //    if (PreQ.StateAZInternet != null)
                        //    {
                        //        if (PreQ.StateAZInternet == false && PInfo.State == "AZ")
                        //        {
                        //            ErrorMessage += "State AZ is disqualify \\n";
                        //            retVal = false;
                        //        }
                        //    }

                        //    if (PreQ.StateWAInternet != null)
                        //    {
                        //        if (PreQ.StateWAInternet == false && PInfo.State == "WA")
                        //        {
                        //            ErrorMessage += "State WA is disqualify \\n ";
                        //            retVal = false;
                        //        }
                        //    }
                        //}
                        //else
                        //{

                        //    ErrorMessage += "State " + PInfo.State + " is disqualify \\n ";
                        //    retVal = false;
                        //}
                    }

                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Error_Logger oLogger = new Error_Logger();
                if (ErrorMessage == "")
                {
                    ErrorMessage = "Error Code: " + Convert.ToInt16(Error_Logger.ErrorCode.CheckPreQualifyLeadFn) + " Error in Processing Pre Qualify Rules";
                    oLogger.logError(ex.ToString(), ErrorMessage);
                }
                else
                {
                    ErrorMessage = "Error Code: " + Convert.ToInt16(Error_Logger.ErrorCode.CheckPreQualifyLeadFn) + " " + ErrorMessage;
                    oLogger.logError(ex.ToString(), ErrorMessage);
                }
            }
            finally
            {

            }
            return retVal;
        }

