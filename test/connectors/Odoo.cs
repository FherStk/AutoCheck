/*
    Copyright © 2021 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Concurrent;
using NUnit.Framework;

namespace AutoCheck.Test.Connectors
{    
    [Parallelizable(ParallelScope.All)]   //TODO: conflict between instances, each test must use its own connector instance!
    public class Odoo : Test
    {   
        private AutoCheck.Core.Connectors.Odoo Conn = null; //Just to share connector data                  
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Odoo> Connectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Odoo>();        
        private AutoCheck.Core.Connectors.Odoo Connector {
            get{
                return Connectors[TestContext.CurrentContext.Test.ID];
            }
        }

        [OneTimeSetUp]
        public override void OneTimeSetUp() 
        {            
            //The same database (but different connector instance, to allow parallel queries) will be shared along the different tests, because all the opperations 
            //are read-only; this will boost the test performance because loading the Odoo database is a long time opperation.
            Conn = new AutoCheck.Core.Connectors.Odoo(1, "localhost", $"autocheck_{TestContext.CurrentContext.Test.ID}", "postgres", "postgres");                        
            base.OneTimeSetUp();                                            //needs "Conn" on "CleanUp"
            Conn.CreateDataBase(base.GetSampleFile("dump.sql"));       //must be done after OneTimeSetup()
        }      

        protected override void CleanUp(){
            if(Conn.ExistsDataBase()) Conn.DropDataBase();
            Connectors.Clear();
        }
        
        [SetUp]
        public void Setup() 
        {            
            //Create a new and unique database connection for the current context (same DB for all tests)            
            var added = false;
            do added = Connectors.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.Odoo(1, Conn.Host, Conn.Database, Conn.User, Conn.User));             
            while(!added);            
        }

        [TearDown]
        public void TearDown(){
            Connector.Dispose();
        }

        [Test]
        [TestCase(0, null, null, null)]
        [TestCase(0, _FAKE, null, null)]
        [TestCase(0, _FAKE, _FAKE, null)]
        public void Constructor_Throws_ArgumentNullException_CompanyID(int companyID, string host, string database, string username)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(companyID, host, database, username));
        }

        [Test]
        [TestCase(null, _FAKE, _FAKE, _FAKE)]
        public void Constructor_Throws_ArgumentNullException_CompanyName(string companyName, string host, string database, string username)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(companyName, host, database, username));
        }

        [Test]
        [TestCase(0, _FAKE, _FAKE, _FAKE)]
        public void Constructor_Throws_ArgumentOutOfRangeException(int companyID, string host, string database, string username)
        {      
             Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.Connectors.Odoo(companyID, host, database, username));
        }

        [Test]        
        [TestCase(1, _FAKE, _FAKE, _FAKE)]
        public void Constructor_Local_DoesNotThrow_CompanyID(int companyID, string host, string database, string username)
        {      
             Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Odoo(companyID, host, database, username));
        }

        [Test]        
        [TestCase(_FAKE, _FAKE, _FAKE, _FAKE)]
        public void Constructor_Local_DoesNotThrow_CompanyName(string companyName, string host, string database, string username)
        {      
             Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Odoo(companyName, host, database, username));
        }

        [Test]
        [TestCase("")]
        public void GetCompanyID_Throws_ArgumentNullException(string companyName)
        {  
            Assert.Throws<ArgumentNullException>(() => Connector.GetCompanyID(companyName));
        }


        [Test]
        [TestCase("Play Puig", true, ExpectedResult=1)]
        [TestCase("Play Puig", false, ExpectedResult=1)]
        [TestCase("Pl ay  Puig", true, ExpectedResult=0)]
        [TestCase("Pl ay  Puig", false, ExpectedResult=1)]
        [TestCase("Play Puig Enterprises", true, ExpectedResult=0)]
        [TestCase("Play Puig Enterprises", false, ExpectedResult=0)]
        [TestCase("PlayPuig", true, ExpectedResult=0)]
        [TestCase("PlayPuig", false, ExpectedResult=0)]
        [TestCase("Puig", true, ExpectedResult=0)]
        [TestCase("Puig", false, ExpectedResult=0)]
        [TestCase("Play", true, ExpectedResult=0)]
        [TestCase("Play", false, ExpectedResult=0)]
        public int GetCompanyID_DoesNotThrow(string companyName, bool strict)
        {                    
            return Connector.GetCompanyID(companyName, strict);                    
        }

        [Test]
        [TestCase("")]
        public void GetCompanyData_Throws_ArgumentNullException(string companyName)
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetCompanyData(companyName));            
        }

        [Test]
        [TestCase(0)]
        public void GetCompanyData_Throws_ArgumentOutOfRangeException(int companyID)
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetCompanyData(companyID));            
        }

        [Test]
        [TestCase(_FAKE, ExpectedResult=0)]
        [TestCase("Play Puig", ExpectedResult=1)]
        [TestCase(_FAKE, ExpectedResult=0)]
        public int GetCompanyData_DoesNotThrows_CompanyName(string companyName)
        {                    
            return Connector.GetCompanyData(companyName).Rows.Count;           
        }

        [Test]
        [TestCase(1, ExpectedResult=1)]
        public int GetCompanyData_DoesNotThrows_CompanyID(int companyID)
        {                    
            return Connector.GetCompanyData(companyID).Rows.Count;
        }

        [Test]
        [TestCase("")]
        public void GetProviderID_Throws_ArgumentNullException(string companyName)
        { 
            Assert.Throws<ArgumentNullException>(() => Connector.GetProviderID(companyName));
        } 

        [Test]
        [TestCase("ASUSTeK", true, ExpectedResult=8)]
        [TestCase("ASUSTeK", false, ExpectedResult=8)]
        [TestCase("ASUS TeK", true, ExpectedResult=0)]
        [TestCase("ASUS TeK", false, ExpectedResult=8)]
        [TestCase("ASUS TeK Enterprises", true, ExpectedResult=0)]
        [TestCase("ASUS TeK Enterprises", false, ExpectedResult=0)]
        [TestCase("ASUS", true, ExpectedResult=0)]
        [TestCase("ASUS", false, ExpectedResult=0)]
        [TestCase("TeK", true, ExpectedResult=0)]
        [TestCase("TeK", false, ExpectedResult=0)]
        public int GetProviderID_DoesNotThrows(string companyName, bool strict)
        {                    
            return Connector.GetProviderID(companyName, strict);                              
        }

        [Test]
        public void GetProviderData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetProviderData(string.Empty));            
        }

        [Test]
        public void GetProviderData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetProviderData(0));                    
        }

        [Test]
        [TestCase(_FAKE, ExpectedResult=0)]
        [TestCase("ASUSTeK", ExpectedResult=1)]
        public int GetProviderData_DoesNotThrow_ProviderName(string providerName)
        {                    
            return Connector.GetProviderData(providerName).Rows.Count;
        }

        [Test]
        [TestCase(8, ExpectedResult=1)]
        public int GetProviderData_DoesNotThrow(int providerID)
        {                    
            return Connector.GetProviderData(providerID).Rows.Count;
        }

        [Test]
        public void GetProductTemplateID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetProductTemplateID(string.Empty));

        }

        [Test]
        [TestCase("iPod", true, ExpectedResult=20)]
        [TestCase("iPod", false, ExpectedResult=20)]
        [TestCase("i Pod", true, ExpectedResult=0)]
        [TestCase("i Pod", false, ExpectedResult=20)]
        [TestCase("Apple iPod", true, ExpectedResult=0)]
        [TestCase("Apple iPod", false, ExpectedResult=0)]
        [TestCase("HDD", true, ExpectedResult=0)]
        [TestCase("HDD", false, ExpectedResult=0)]
        [TestCase("SH-1", true, ExpectedResult=0)]
        [TestCase("SH-1", false, ExpectedResult=0)]
        [TestCase("HDD SH-1", true, ExpectedResult=25)]
        [TestCase("HDD SH-1", false, ExpectedResult=25)]
        [TestCase("HDDSH-1", true, ExpectedResult=0)]
        [TestCase("HDDSH-1", false, ExpectedResult=0)]
        public int GetProductTemplateID_DoesNotThrow(string productName, bool strict)
        {                    
            return Connector.GetProductTemplateID(productName, strict);            
        }

        [Test]
        public void GetProductTemplateData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetProductTemplateData(string.Empty));
        }

        [Test]
        public void GetProductTemplateData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetProductTemplateData(0));
        }

        [Test]
        [TestCase(_FAKE, ExpectedResult=0)]
        [TestCase("iPod", ExpectedResult=2)]
        public int GetProductTemplateData_DoesNotThrow_ProductName(string productName)
        {                    
            return Connector.GetProductTemplateData(productName).Rows.Count;
        }

        [Test]
        [TestCase(20, ExpectedResult=2)]
        public int GetProductTemplateData_DoesNotThrow_ProductName(int productID)
        {                    
            return Connector.GetProductTemplateData(productID).Rows.Count;
        }

        [Test]
        [TestCase(ExpectedResult=10)]
        public int GetLastPurchaseID()
        {                    
            return Connector.GetLastPurchaseID();
        }

        [Test]
        public void GetPurchaseID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetPurchaseID(string.Empty));
        }

        [Test]
        [TestCase("PO00006", ExpectedResult=6)]
        [TestCase("PO00009", ExpectedResult=9)]
        [TestCase("PO00999", ExpectedResult=0)]
        public int GetPurchaseID_DoesNotThrow(string purchaseCode)
        {                    
            return Connector.GetPurchaseID(purchaseCode);
        }


        [Test]
        public void GetPurchaseCode_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetPurchaseCode(0));
        }

        [Test]
        [TestCase(6, ExpectedResult="PO00006")]
        [TestCase(999, ExpectedResult=null)]
        public string GetPurchaseCode_DoesNotThrow(int purchaseID)
        {                    
            return Connector.GetPurchaseCode(purchaseID);
        }

        [Test]
        public void GetPurchaseData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetPurchaseData(0));
        }

        [Test]
        public void GetPurchaseData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetPurchaseData(null));
        }

        [Test]
        [TestCase(1, ExpectedResult=3)]
        [TestCase(6, ExpectedResult=4)]
        [TestCase(999, ExpectedResult=0)]
        public int GetPurchaseData_DoesNotThrow_PurchaseID(int purchaseID)
        {                    
            return Connector.GetPurchaseData(purchaseID).Rows.Count;
        }

        [Test]
        [TestCase("PO00001", ExpectedResult=3)]
        [TestCase("PO00006", ExpectedResult=4)]
        [TestCase("PO00999", ExpectedResult=0)]
        public int GetPurchaseData_DoesNotThrow_PurchaseID(string purchaseCode)
        {                    
            return Connector.GetPurchaseData(purchaseCode).Rows.Count;
        }

        [Test]
        public void GetStockMovementData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetStockMovementData(null, false));
        }

        [Test]
        [TestCase("PO00001", false, ExpectedResult=3)]
        [TestCase("PO00001", true, ExpectedResult=3)]
        [TestCase("PO00002", false, ExpectedResult=2)]
        [TestCase("PO00002", true, ExpectedResult=2)]
        [TestCase("PO00008", false, ExpectedResult=1)]
        [TestCase("PO00008", true, ExpectedResult=0)]
        [TestCase("PO00999", false, ExpectedResult=0)]
        [TestCase("PO00999", true, ExpectedResult=0)]
        [TestCase("SO020", false, ExpectedResult=1)]
        [TestCase("SO020", true, ExpectedResult=0)]
        [TestCase("SO021", false, ExpectedResult=1)]
        [TestCase("SO021", true, ExpectedResult=1)]
        [TestCase("SO999", false, ExpectedResult=0)]
        [TestCase("SO999", true, ExpectedResult=0)]
        public int GetStockMovementData_DoesNotThrow(string purchaseCode, bool isReturn)
        {                    
            return Connector.GetStockMovementData(purchaseCode, isReturn).Rows.Count;
        }   

        [Test]
        [TestCase(ExpectedResult=3)]
        public int GetScrappedStockData()
        {                    
            return Connector.GetScrappedStockData().Rows.Count;
        }

        [Test]
        public void GetInvoiceID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetInvoiceID(string.Empty));
        }

        [Test]
        [TestCase("PO00008", ExpectedResult=8)]
        [TestCase("PO00009", ExpectedResult=0)]
        public int GetInvoiceID_DoesNotThrow(string orderCode)
        {                    
            return Connector.GetInvoiceID(orderCode);
        }

        [Test]
        public void GetInvoiceCode_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetInvoiceCode(string.Empty));
        } 

        [Test]
        [TestCase("PO00008", ExpectedResult="FACTURA /2020/0003")]
        [TestCase("PO00009", ExpectedResult=null)]
        public string GetInvoiceCode_DoesNotThrow(string orderCode)
        {                    
            return Connector.GetInvoiceCode(orderCode);
        } 

        [Test]
        public void GetInvoiceData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetInvoiceData(0));  
        } 

        [Test]
        public void GetInvoiceData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetInvoiceData(string.Empty));
        } 

        [Test]
        [TestCase(8, ExpectedResult=1)]
        [TestCase(999, ExpectedResult=0)]
        public int GetInvoiceData_DoesNotThrow_InvoiceID(int invoiceID)
        {                    
            return Connector.GetInvoiceData(invoiceID).Rows.Count;
        } 

        [Test]
        [TestCase("PO00008", ExpectedResult=1)]
        [TestCase("PO00009", ExpectedResult=0)]
        public int GetInvoiceData_DoesNotThrow_InvoiceID(string orderCode)
        {                    
            return Connector.GetInvoiceData(orderCode).Rows.Count;
        } 

        [Test]
        [TestCase(ExpectedResult=3)]
        public int GetLastPosSaleID()
        {                    
            return Connector.GetLastPosSaleID();
        } 

        [Test]
        public void GetPosSaleID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetPosSaleID(string.Empty));
        }

        [Test]
        [TestCase("Main/0001", ExpectedResult=1)]
        [TestCase("Main/0002", ExpectedResult=2)]
        [TestCase("Main/0999", ExpectedResult=0)]
        public int GetPosSaleID_DoesNotThrow(string posSaleCode)
        {                    
            return Connector.GetPosSaleID(posSaleCode);
        }

        [Test]
        public void GetPosSaleCode_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetPosSaleCode(0));
        }

        [Test]
        [TestCase(1, ExpectedResult="Main/0001")]
        [TestCase(2, ExpectedResult="Main/0002")]
        [TestCase(999, ExpectedResult=null)]
        public string GetPosSaleCode_DoesNotThrow(int posSaleID)
        {                    
            return Connector.GetPosSaleCode(posSaleID);
        }

        [Test]
        public void GetPosSaleData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetPosSaleData(0));
        }

        [Test]
        public void GetPosSaleData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetPosSaleData(string.Empty));
        }

        [Test]
        [TestCase(1, ExpectedResult=1)]
        [TestCase(2, ExpectedResult=2)]
        [TestCase(999, ExpectedResult=0)]
        public int GetPosSaleData_DoesNotThrow_PosSaleID(int posSaleID)
        {                    
            return Connector.GetPosSaleData(posSaleID).Rows.Count;
        }

        [Test]
        [TestCase("Main/0001", ExpectedResult=1)]
        [TestCase("Main/0002", ExpectedResult=2)]
        [TestCase("Main/0999", ExpectedResult=0)]
        public int GetPosSaleData_DoesNotThrow_PosSaleID(string posSaleCode)
        {                    
            return Connector.GetPosSaleData(posSaleCode).Rows.Count;
        }

        [Test]
        [TestCase(ExpectedResult=23)]
        public int GetLastSaleID()
        {                    
            return Connector.GetLastSaleID();
        } 

        [Test]
        public void GetSaleID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetSaleID(string.Empty));
        }

        [Test]
        [TestCase("SO001", ExpectedResult=1)]
        [TestCase("SO002", ExpectedResult=2)]
        [TestCase("SO999", ExpectedResult=0)]
        public int GetSaleID_DoesNotThrow(string saleCode)
        {                    
            return Connector.GetSaleID(saleCode);
        }

        [Test]
        public void GetSaleCode_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetSaleCode(0));
        }

        [Test]
        [TestCase(1, ExpectedResult="SO001")]
        [TestCase(2, ExpectedResult="SO002")]
        [TestCase(999, ExpectedResult=null)]
        public string GetSaleCode_DoesNotThrow(int saleID)
        {                    
            return Connector.GetSaleCode(saleID);
        }

        [Test]
        public void GetSaleData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetSaleData(0));
        }

        [Test]
        public void GetSaleData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetSaleData(string.Empty));
        }

        [Test]
        [TestCase(1, ExpectedResult=3)]
        [TestCase(2, ExpectedResult=2)]
        [TestCase(999, ExpectedResult=0)]
        public int GetSaleData(int saleID)
        {                    
            return Connector.GetSaleData(saleID).Rows.Count;
        }

        [Test]
        [TestCase("SO001", ExpectedResult=3)]
        [TestCase("SO002", ExpectedResult=2)]
        [TestCase("SO999", ExpectedResult=0)]
        public int GetSaleData(string saleCode)
        {                    
            return Connector.GetSaleData(saleCode).Rows.Count;
        }

        [Test]
        public void GetUserID_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetUserID(string.Empty));
        }

        [Test]
        [TestCase("demo", true, ExpectedResult=5)]
        [TestCase("demo", false, ExpectedResult=5)]
        [TestCase("admin", true, ExpectedResult=0)]
        [TestCase("admin", false, ExpectedResult=0)]
        [TestCase("admin@elpuig.xeill.net", true, ExpectedResult=1)]
        [TestCase("admin@elpuig.xeill.net", false, ExpectedResult=1)]
        [TestCase("admin @ elpuig.xeill.net", true, ExpectedResult=0)]
        [TestCase("admin @ elpuig.xeill.net", false, ExpectedResult=1)]
        public int GetUserID_DoesNotThrow(string userName, bool strict)
        {                    
            return Connector.GetUserID(userName, strict);
        }

        [Test]
        public void GetUserName_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetUserName(0));
        }

        [Test]
        [TestCase(1, ExpectedResult="admin@elpuig.xeill.net")]
        [TestCase(5, ExpectedResult="demo")]
        [TestCase(999, ExpectedResult=null)]
        public string GetUserName_DoesNotThrow(int userID)
        {                    
            return Connector.GetUserName(userID);
        }

        [Test]
        public void GetUserData_Throws_ArgumentNullException()
        {                    
            Assert.Throws<ArgumentNullException>(() => Connector.GetUserData(string.Empty));
        }

         [Test]
        public void GetUserData_Throws_ArgumentOutOfRangeException()
        {                    
            Assert.Throws<ArgumentOutOfRangeException>(() => Connector.GetUserData(0));
        }

        [Test]
        [TestCase(1, ExpectedResult=20)]
        [TestCase(5, ExpectedResult=12)]
        [TestCase(999, ExpectedResult=0)]
        public int GetUserData(int userID)
        {                    
            return Connector.GetUserData(userID).Rows.Count;
        }

        [Test]
        [TestCase("admin@elpuig.xeill.net", ExpectedResult=20)]
        [TestCase("demo", ExpectedResult=12)]
        [TestCase(_FAKE, ExpectedResult=0)]
        public int GetUserData(string userName)
        {                    
            return Connector.GetUserData(userName).Rows.Count;
        }
    }
}