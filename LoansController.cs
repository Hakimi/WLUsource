using System;
using System.Linq;
using YesPay.Admin.Entite;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;
using YesPay.Admin;
using System.Data;

namespace YesPay.Admin
{
    public partial class AdminController : IAdminController
    {
        public void SetCustomerInfo(int iCustomerID)
        {
             using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString))
             //012211 using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString, System.Data.IsolationLevel.ReadUncommitted))
            {
                try
                {
                    _CustomerPersonalInfo = (from Cust in db.CustomerPersonalInfos where Cust.CustomerID == iCustomerID select Cust).FirstOrDefault();    
                }
                catch (Exception ex)
                {
                    Error_Logger oLogger = new Error_Logger();
                    oLogger.logError(ex.ToString(), "Error in SetCustomerInfo");
                }
                finally
                {
                    db.Dispose();
                }
            }
        }
        public void SetEMPInfo(int iCustomerID)
        {
             using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString))
             //012211 using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString, System.Data.IsolationLevel.ReadUncommitted))
            {
                try
                {
                    _EmployerMaster  = (from emp in db.EmployerMasters where emp.CustomerID == iCustomerID select emp).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Error_Logger oLogger = new Error_Logger();
                    oLogger.logError(ex.ToString(), "Error in SetEMPInfo");
                }
                finally
                {
                    db.Dispose();
                }
            }
        }

        public bool setCreditLimit(int iApplicationId, int iCustomerID, decimal iCreditLimit, int iCustomerLevelId, Int16 StoreId, Int16 UserID, decimal NewCashAdvAmount, decimal ScheduledAllotment,Allotment All)
        {
            bool retVal = false;

            try
            {
                Admin.SQLHelper.SQLHelper TSOptions = new Admin.SQLHelper.SQLHelper();

                using (var TS = new TransactionScope(TransactionScopeOption.RequiresNew, TSOptions.GetTSOptions()))
                {
                     using (YesPayDBLINQDataContext db = new YesPayDBLINQDataContext(AppController.YesPayConnectionString))
                     {
                        AdminFeesCharge AdminFees = new AdminFeesCharge();
                        EmployerMaster emp = new EmployerMaster();
                        Loan objLoanStatus = new Loan();
                        AdminFees = db.AdminFeesCharges.FirstOrDefault(a => a.CustomerLevel_Id == iCustomerLevelId && a.StoreId == StoreId);
                        emp = db.EmployerMasters.FirstOrDefault(em => em.CustomerID == iCustomerID);
                        //012313 Spec 7.44
                        CustomerMaster CM = new CustomerMaster();
                        CM = (from c in db.CustomerMasters where c.CustomerID == iCustomerID select c).FirstOrDefault();
                        //012313 Spec 7.44
                        #region Validation
                        
                        if ((emp.PayFrequency == null) || (emp.PayFrequency == 0) || (emp.PayFrequency == 5))
                        {
                            ErrorMessage = "Employer PayFrequency did not set properly.";
                            retVal = false;
                            TS.Dispose();//011912 
                            return false;
                        }
                        decimal MNI =0;
                        if (emp.MonthlyNetIncome == 0)
                        {
                            //MNI = _Calculation_Engine.Calc_MNI(Convert.ToDateTime(emp.LastPayDate), Convert.ToDateTime(emp.NextPayDate), (decimal)emp.AmountofLastPayCheck); //011713
                            MNI = _clsCalculationEngine.Calc_MNI(Convert.ToDateTime(emp.LastPayDate), Convert.ToDateTime(emp.NextPayDate), (decimal)emp.AmountofLastPayCheck); //011713
                            emp.MonthlyNetIncome = MNI;
                        }
                        else
                        {
                            MNI = emp.MonthlyNetIncome;
                        }
                        //082611
                        #endregion
                        if (AdminFees != null)
                        {
                            //082311
                            if (AdminFees.MaximumCreditLimit > 0)
                            {
                                if (iCreditLimit > AdminFees.MaximumCreditLimit)
                                {
                                    ErrorMessage = "Selected credit limit exceeded allowable limit for this customer. Because Requested Credit Limit value (" + iCreditLimit + ") is Greater than limit " + AdminFees.MaximumCreditLimit.ToString("F2") + " Admin's Maximum Credit Limit=" + AdminFees.MaximumCreditLimit.ToString("c2") ;
                                    retVal = false;
                                    TS.Dispose();//011912
                                    return false;
                                }
                            }
                            //082311
                            if (iCreditLimit > ((MNI * AdminFees.CreditLimit) / 100))
                            {
                                ErrorMessage = "Selected credit limit exceeded allowable limit for this customer. Because Requested Credit Limit value (" + iCreditLimit + ") is Greater than limit " + (((MNI * AdminFees.CreditLimit) / 100)).Value.ToString("F2") + "  (MNI(" + MNI + ") * Credit Limit(" + AdminFees.CreditLimit.Value.ToString("F2") + ") / 100 = % of MNI ) ";
                                retVal = false;
                                TS.Dispose();//011912
                                return false;
                            }
                            else
                            {
                                
                                objLoanStatus = db.Loans.SingleOrDefault(e => e.ApplicationId == iApplicationId && e.CustomerID == iCustomerID);
                                //05142011
                                //if (AdminFees.UsePaydayLoanSettings == true) //011912
                                if (AdminFees.UsePaydayLoanSettings == (byte)Enums.ProductType.Payday_Loan) //011912
                                {
                                    All.FixedPaymentAmount = ScheduledAllotment;
                                    All.FixedPayments = true;
                                    All.AllotmentOn = false;
                                }
                                //05142011
                                if (objLoanStatus != null)
                                {
                                   
                                    if ((objLoanStatus.ApplicationStatusID != 31) && (objLoanStatus.ApplicationStatusID != 32) && (objLoanStatus.ApplicationStatusID != 33))
                                    {
                                        objLoanStatus.ApplicationStatusID = 10;
                                    }
                                    decimal Balance,  CreditBalance = 0;
                                    decimal LateFee = 0;
                                    Balance = _clsCalculationEngine.GetBalanceWithDB(objLoanStatus.ApplicationId, Convert.ToInt32(objLoanStatus.User_ID), Convert.ToInt32(objLoanStatus.StoreId), DateTime.Now.Date, db, out LateFee).Value;  //070209// 011912 //011713
                                    CreditBalance = Convert.ToDecimal(iCreditLimit - Balance);
                                    
                                    if (CreditBalance <= 0)
                                    {
                                        if (Balance < iCreditLimit) //100412
                                        {
                                            ErrorMessage = "Credit Limit Cannot be Set as it Makes Credit Balance Negetive. \\n Credit Balance is " + CreditBalance.ToString("F2") + "( Credit Limit (" + iCreditLimit.ToString("F2") + ") - Balance (" + Balance.ToString("F2") + ") ) ";
                                            retVal = false;
                                            TS.Dispose();//011912
                                            return false;
                                        }
                                    }


                                    //012313 Spec 7.44 objLoanStatus.Credit_Limit = iCreditLimit;
                                    CM.Credit_Limit = iCreditLimit;//012313 Spec 7.44
                                    objLoanStatus.StoreId = StoreId;
                                    //objLoanStatus.User_ID = UserID;
                                    objLoanStatus.NewCashAdvAmount = NewCashAdvAmount;
                                    objLoanStatus.ScheduledAllotment = ScheduledAllotment;

                                    //For Email Service
                                    #region Email Service
                                    

                                    var S = db.StatusEmails.FirstOrDefault(e => e.CustomerId == iCustomerID && 
                                                                            e.ApplicationId == objLoanStatus.ApplicationId && 
                                                                            e.AllotmentsSet != null);
                                    if (S == null)
                                    {
                                        StatusEmail SE = new StatusEmail();
                                        SE.ApplicationId = iApplicationId;
                                        SE.CustomerId = iCustomerID;
                                        SE.AllotmentsSet = DateTime.Now;
                                        SE.StatusID = 1;
                                        db.StatusEmails.InsertOnSubmit(SE);
                                    }
                                    else
                                    {
                                        S.StatusID = 1;
                                        S.AllotmentsSet = DateTime.Now;
                                    }

                                    var SMList = (from s in db.SendEmailTimes 
                                                    where s.ApplicationId == objLoanStatus.ApplicationId && 
                                                            s.CustomerId == objLoanStatus.CustomerID &&
                                                            (s.SendMailFor == 17 || s.SendMailFor == 18 || s.SendMailFor == 2)
                                                  select s).ToList();

                                    foreach (SendEmailTime SM in SMList)
                                    {
                                        SM.SendTime = 0;
                                        SM.SendTimeSMS = 0;
                                    }
                                    #endregion
                                    //011912 Code Modify
                                    db.SubmitChanges();
                                }
                                else
                                {
                                    var NewApp = db.NewApplications.SingleOrDefault(e => e.NewApplicationId == iApplicationId && e.CustomerID == iCustomerID);
                                    //042213
                                    if (NewApp != null)
                                    {
                                        CustomerLoansDetail _CustomerLoansDetail = (from c in db.CustomerLoansDetails where c.CustomerID == iCustomerID select c).FirstOrDefault();
                                        if (_CustomerLoansDetail == null)
                                        {
                                            General_Config _General_Config = (from g in db.General_Configs
                                                                              where g.Store_Id == NewApp.StoreId
                                                                              select g).FirstOrDefault();

                                            string BankID = string.Empty;
                                            if (_General_Config.Active1 == true)
                                            {
                                                BankID = _General_Config.BankId1;
                                            }
                                            if (_General_Config.Active2 == true)
                                            {
                                                BankID = _General_Config.BankId2;
                                            }
                                            if (_General_Config.Active3 == true)
                                            {
                                                BankID = _General_Config.BankId3;
                                            }

                                            string strStoreId = NewApp.StoreId.ToString();

                                            if (strStoreId.Trim().Length > 2)
                                            {
                                                strStoreId = strStoreId.Substring(strStoreId.Length - 2);
                                            }

                                            Random _random = new Random();
                                            CustomerLoansDetail _CustomerLoansDetail1 = new CustomerLoansDetail();
                                            _CustomerLoansDetail1.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail1.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail1.LoanID = _random.Next(1, 9999999);
                                            bool isDuplicate = true;
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail1.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail1.LoanID = _random.Next(1, 9999999); ;
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail1.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail1.LoanID.ToString().PadLeft(7, '0');
                                            isDuplicate = true;
                                            CustomerLoansDetail _CustomerLoansDetail2 = new CustomerLoansDetail();
                                            _CustomerLoansDetail2.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail2.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail2.LoanID = _random.Next(1, 9999999);
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail2.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail2.LoanID = _random.Next(1, 9999999);
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail2.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail2.LoanID.ToString().PadLeft(7, '0');
                                            isDuplicate = true;

                                            CustomerLoansDetail _CustomerLoansDetail3 = new CustomerLoansDetail();
                                            _CustomerLoansDetail3.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail3.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail3.LoanID = _random.Next(1, 9999999);
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail3.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail3.LoanID = _random.Next(1, 9999999);
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail3.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail3.LoanID.ToString().PadLeft(7, '0');
                                            isDuplicate = true;

                                            CustomerLoansDetail _CustomerLoansDetail4 = new CustomerLoansDetail();
                                            _CustomerLoansDetail4.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail4.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail4.LoanID = _random.Next(1, 9999999);
                                            _CustomerLoansDetail4.PRNumber = _CustomerLoansDetail4.LoanID.ToString();
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail4.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail4.LoanID = _random.Next(1, 9999999);
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail4.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail4.LoanID.ToString().PadLeft(7, '0');
                                            isDuplicate = true;
                                            CustomerLoansDetail _CustomerLoansDetail5 = new CustomerLoansDetail();
                                            _CustomerLoansDetail5.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail5.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail5.LoanID = _random.Next(1, 9999999);
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail5.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail5.LoanID = _random.Next(1, 9999999);
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail5.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail5.LoanID.ToString().PadLeft(7, '0');
                                            isDuplicate = true;

                                            CustomerLoansDetail _CustomerLoansDetail6 = new CustomerLoansDetail();
                                            _CustomerLoansDetail6.CustomerID = NewApp.CustomerID;
                                            _CustomerLoansDetail6.CustomerLoansDetailsID = 0;
                                            _CustomerLoansDetail6.LoanID = _random.Next(1, 9999999);
                                            while (isDuplicate)
                                            {
                                                int c1 = (from c in db.CustomerLoansDetails
                                                          where c.LoanID == _CustomerLoansDetail6.LoanID
                                                          select c.LoanID).Count();
                                                if (c1 > 0)
                                                {
                                                    _CustomerLoansDetail6.LoanID = _random.Next(1, 9999999);
                                                }
                                                else
                                                {
                                                    isDuplicate = false;
                                                }
                                            }
                                            _CustomerLoansDetail6.PRNumber = BankID + strStoreId.PadLeft(2, '0') + _CustomerLoansDetail6.LoanID.ToString().PadLeft(7, '0');

                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail1);
                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail2);
                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail3);
                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail4);
                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail5);
                                            db.CustomerLoansDetails.InsertOnSubmit(_CustomerLoansDetail6);

                                            db.SubmitChanges();
                                        }
                                    }
                                    //042213
                                    var CashAdv = db.CashAdvance_Requests.FirstOrDefault(e => e.CustomerId == iCustomerID);
                                    var L = db.BankAccountInfos.FirstOrDefault(e => e.CustomerID == iCustomerID && e.IsDelete == false ); //05042011
                                    if (L != null)
                                    {
                                        if (L.Return_Code != "14")
                                        {
                                            if (CashAdv != null)
                                            {
                                                CashAdv.App_Stage = false;
                                            }
                                        }
                                    }
                                    if (NewApp != null)
                                    {
                                        objLoanStatus = new Loan();
                                        //012313 Spec 7.44 CustomerMaster CM = (from C in db.CustomerMasters where C.CustomerID == NewApp.CustomerID select C).FirstOrDefault();//082411
                                        objLoanStatus.ApplicationId = NewApp.NewApplicationId;
                                        //020913 Spec 7.44
                                        if (NewApp.StoreId == 2 || NewApp.StoreId  == 20)
                                        {
                                            objLoanStatus.LoanID = 0;
                                        }
                                        else
                                        {
                                            GetFreeLoanIdResult _FreeLoan = db.GetFreeLoanId(NewApp.CustomerID).FirstOrDefault();
                                            if (_FreeLoan != null)
                                            {
                                                objLoanStatus.LoanID = _FreeLoan.LoanId;
                                            }
                                        }
                                        //020913 Spec 7.44
                                        objLoanStatus.ApplyDate = CM.CreatedDate;//082411
                                        objLoanStatus.CustomerID = (int)NewApp.CustomerID;
                                        objLoanStatus.ACHStatusID = NewApp.ACHStatusID;  //03222012 7.20
                                        objLoanStatus.DueDate = NewApp.DueDate;
                                        objLoanStatus.LoanAmt = 0;
                                        objLoanStatus.User_ID = UserID;
                                        objLoanStatus.StoreId = StoreId;
                                        //06232011
                                        //CustomerMaster CM = (from C in db.CustomerMasters where C.CustomerID == objLoanStatus.CustomerID select C).FirstOrDefault(); //082411
                                        if (StoreId == 2)
                                        {
                                            CM.eStatement_subscription = true;
                                            CM.mStatement_subscription = false;
                                        }
                                        else
                                        {
                                            CM.eStatement_subscription = true;
                                            CM.mStatement_subscription = true;
                                        }
                                        //06232011
                                        if (NewApp.ApplicationStatusID == 31)
                                        {
                                            CustomerPersonalInfo CP = (from C in db.CustomerPersonalInfos where C.CustomerID == objLoanStatus.CustomerID select C).FirstOrDefault();
                                            ////06232011 CustomerMaster CM = (from C in db.CustomerMasters where C.CustomerID == objLoanStatus.CustomerID select C).FirstOrDefault(); 
                                            objLoanStatus.ApplicationStatusID = 31;
                                            if (CP != null)
                                            {
                                                string SSN = CP.SSN.Trim();
                                                //06202011
                                                if (SSN.Length < 4)
                                                {
                                                    CM.Password = Utils.StringEncrypt(CP.LastName + SSN.PadLeft(SSN.Length, Convert.ToChar("*")).Trim(Convert.ToChar("*")));
                                                    CM.HintQuestion = 1;
                                                    CM.Answer = Utils.StringEncrypt(CP.LastName + SSN.PadLeft(SSN.Length, Convert.ToChar("*")).Trim(Convert.ToChar("*"))); ;
                                                }
                                                else
                                                {
                                                    CM.Password = Utils.StringEncrypt(CP.LastName + SSN.Substring(SSN.Length - 4).PadLeft(SSN.Length, Convert.ToChar("*")).Trim(Convert.ToChar("*")));
                                                    CM.HintQuestion = 1;
                                                    CM.Answer = Utils.StringEncrypt(CP.LastName + SSN.Substring(SSN.Length - 4).PadLeft(SSN.Length, Convert.ToChar("*")).Trim(Convert.ToChar("*"))); ;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            objLoanStatus.ApplicationStatusID = 10;
                                        }
                                        objLoanStatus.CreditCardStatusID = NewApp.CreditCardStatusID  ;
                                        objLoanStatus.CollectionStatusID = 8;
                                        if (NewApp.CustomerLevel_Id != null)
                                        {
                                            objLoanStatus.CustomerLevel_Id = NewApp.CustomerLevel_Id.Value ;
                                        }
                                        else
                                        {
                                            objLoanStatus.CustomerLevel_Id = (byte)iCustomerLevelId;
                                        }

                                        #region For Email Count
                                        
                                        //For Email Count
                                        var SMCollectionStatusList = (from s in db.SendEmailTimes 
                                                                      where s.ApplicationId == objLoanStatus.ApplicationId && 
                                                                      s.CustomerId == objLoanStatus.CustomerID &&
                                                                      (s.SendMailFor == 4 || s.SendMailFor == 17 ||
                                                                      s.SendMailFor == 18 || s.SendMailFor == 2 ||
                                                                      s.SendMailFor == 11 || s.SendMailFor == 9 ||
                                                                      s.SendMailFor == 8 
                                                                      )
                                                                      
                                                                      select s).ToList();
                                        foreach (SendEmailTime SMCollectionStatus in SMCollectionStatusList)
                                        {
                                            SMCollectionStatus.SendTime = 0;
                                            SMCollectionStatus.SendTimeSMS = 0;
                                        }
                                       

                                        //For Email Service

                                        var StatusEmailsList = (from e in db.StatusEmails
                                                                where e.CustomerId == iCustomerID &&
                                                                                e.ApplicationId == objLoanStatus.ApplicationId &&
                                                                                (e.AllotmentsSet != null ||
                                                                                    e.ApplicationStatus != null ||
                                                                                    e.CollectionStatus != null)
                                                                select e
                                                                     ).ToList();

                                        var SA = StatusEmailsList.FirstOrDefault(e => e.CustomerId == iCustomerID && 
                                                                                e.ApplicationId == objLoanStatus.ApplicationId && 
                                                                                e.AllotmentsSet != null);
                                        if (SA == null)
                                        {
                                            StatusEmail SE = new StatusEmail();
                                            SE.ApplicationId = iApplicationId;
                                            SE.CustomerId = iCustomerID;
                                            SE.AllotmentsSet = DateTime.Now;
                                            SE.StatusID = 1;
                                            db.StatusEmails.InsertOnSubmit(SE);
                                        }
                                        else
                                        {
                                            SA.StatusID = 1;
                                            SA.AllotmentsSet = DateTime.Now;
                                        }


                                        var S = StatusEmailsList.FirstOrDefault(e => e.CustomerId == iCustomerID && 
                                                                                e.ApplicationId == objLoanStatus.ApplicationId && 
                                                                                e.ApplicationStatus != null);
                                        if (S == null)
                                        {
                                            StatusEmail SE = new StatusEmail();
                                            SE.ApplicationId = iApplicationId;
                                            SE.CustomerId = iCustomerID;
                                            SE.ApplicationStatus = DateTime.Now;
                                            SE.StatusID = 10;
                                            db.StatusEmails.InsertOnSubmit(SE);
                                        }
                                        else
                                        {
                                            S.StatusID = 10;
                                            S.ApplicationStatus = DateTime.Now;
                                        }

                                        var SC = StatusEmailsList.FirstOrDefault(e => e.CustomerId == iCustomerID && 
                                                                                e.ApplicationId == objLoanStatus.ApplicationId && 
                                                                                e.CollectionStatus != null);
                                        if (SC == null)
                                        {
                                            StatusEmail SE = new StatusEmail();
                                            SE.ApplicationId = iApplicationId;
                                            SE.CustomerId = iCustomerID;
                                            SE.CollectionStatus = DateTime.Now;
                                            SE.StatusID = 8;
                                            db.StatusEmails.InsertOnSubmit(SE);
                                        }
                                        else
                                        {
                                            SC.StatusID = 8;
                                            SC.CollectionStatus = DateTime.Now;
                                        }
                                        #endregion
                                        //011912 Code Modify
                                        objLoanStatus.NewCashAdvAmount = NewCashAdvAmount;
                                        objLoanStatus.ScheduledAllotment = ScheduledAllotment;
                                        //objLoanStatus.Credit_Limit = iCreditLimit; //012313 Spec 7.44
                                        CM.Credit_Limit = iCreditLimit; //012313 Spec 7.44
                                        db.NewApplications.DeleteOnSubmit(NewApp);
                                        db.SubmitChanges();
                                        db.Loans.InsertOnSubmit(objLoanStatus); 
                                        db.SubmitChanges();
                                    }
                                }

                            }
                            //UpdateAllotment(All, StoreId, UserID,db );//011912
                            //UpdateAllotment(All, StoreId, UserID, db, objLoanStatus, AdminFees, emp);//011912//101912
                            UpdateAllotment(All, StoreId, UserID, db, objLoanStatus, AdminFees, emp,false );//011912//101912
                        }
                        else
                        {
                            ErrorMessage = "Selected credit limit exceeded allowable limit for this customer. Because Minimum Requirements  is not set.";
                            TS.Dispose();//011912
                            return false;
                        }

                    } //datacontext
                    TS.Complete();
                    TS.Dispose();
                } //transactionscope
                ErrorMessage = "Credit Limit Successfully Set";
                retVal = true;
            } //try
            catch (Exception ex)
            {
                retVal = false;
                Error_Logger oLogger = new Error_Logger();
                if (ErrorMessage == "")
                {
                    ErrorMessage = "Error Code: " + Convert.ToInt16(Error_Logger.ErrorCode.setCreditLimitFn) + " Error in Setting Credit Limit";
                    oLogger.logError(ex.ToString(), ErrorMessage);
                }
                else
                {
                    ErrorMessage = "Error Code: " + Convert.ToInt16(Error_Logger.ErrorCode.setCreditLimitFn) + " " + ErrorMessage;
                    oLogger.logError(ex.ToString(), ErrorMessage);
                }
            }
            finally
            {
            }

            return retVal;

        }
    }
}
