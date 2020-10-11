/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

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
        /// <summary>
        /// The connector instance is created here because a new one-time use BBDD will be created on every startup, and dropped when done.
        /// </summary>
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Odoo> Pool = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Odoo>();
        private AutoCheck.Core.Connectors.Odoo Conn = null;           

        [OneTimeSetUp]
        public override void OneTimeSetUp() 
        {            
            //The same database (but different connector instance, to allow parallel queries) will be shared along the different tests, because all the opperations 
            //are read-only; this will boost the test performance because loading the Odoo database is a long time opperation.
            this.Conn = new AutoCheck.Core.Connectors.Odoo(1, "localhost", string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), "postgres", "postgres");
            base.OneTimeSetUp();    //needs "Conn" on "CleanUp"
           
            this.Conn.CreateDataBase(base.GetSampleFile("dump.sql"));
        }

        [OneTimeTearDown]
        public new void OneTimeTearDown(){     
            this.Pool.Clear(); 
        }

        protected override void CleanUp(){
            if(this.Conn.ExistsDataBase()) 
                this.Conn.DropDataBase();
        }
        
        [SetUp]
        public void Setup() 
        {            
            //Create a new and unique database connection for the current context (same DB for all tests)
            var conn = new AutoCheck.Core.Connectors.Odoo(1, this.Conn.Host, this.Conn.Database, this.Conn.User, this.Conn.User);
            
            //Storing the connector instance for the current context
            var added = false;
            do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);            
        }

        [TearDown]
        public void TearDown(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            conn.Dispose();
        }

        [Test]
        public void Constructor()
        {                                            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(0, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(0, _FAKE, null, null));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(0, _FAKE, _FAKE, null));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Odoo(null,  _FAKE, _FAKE, _FAKE));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.Connectors.Odoo(0, _FAKE, _FAKE, _FAKE));            
            
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Odoo(1, _FAKE, _FAKE, _FAKE, _FAKE));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Odoo(_FAKE, _FAKE, _FAKE, _FAKE, _FAKE));
        }

        [Test]
        public void GetCompanyID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetCompanyID(string.Empty));

            Assert.AreEqual(1, conn.GetCompanyID("Play Puig", true));            
            Assert.AreEqual(1, conn.GetCompanyID("Play Puig", false));                      

            Assert.AreEqual(0, conn.GetCompanyID("Pl ay  Puig", true));            
            Assert.AreEqual(1, conn.GetCompanyID("Pl ay  Puig", false));                      

            Assert.AreEqual(0, conn.GetCompanyID("Play Puig Enterprises", true));
            Assert.AreEqual(0, conn.GetCompanyID("Play Puig Enterprises", false));

            Assert.AreEqual(0, conn.GetCompanyID("PlayPuig", true));
            Assert.AreEqual(0, conn.GetCompanyID("PlayPuig", false));

            Assert.AreEqual(0, conn.GetCompanyID("Puig", true));
            Assert.AreEqual(0, conn.GetCompanyID("Puig", false));   

            Assert.AreEqual(0, conn.GetCompanyID("Play", true));
            Assert.AreEqual(0, conn.GetCompanyID("Play", false));                        
        }

        [Test]
        public void GetCompanyData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetCompanyData(string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetCompanyData(0));

            Assert.AreEqual(0, conn.GetCompanyData(_FAKE).Rows.Count);
            Assert.AreEqual(1, conn.GetCompanyData("Play Puig").Rows.Count);
            Assert.AreEqual(1, conn.GetCompanyData(1).Rows.Count);                      
        }

        [Test]
        public void GetProviderID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetProviderID(string.Empty));

            Assert.AreEqual(8, conn.GetProviderID("ASUSTeK", true));            
            Assert.AreEqual(8, conn.GetProviderID("ASUSTeK", false));                      

            Assert.AreEqual(0, conn.GetProviderID("ASUS TeK", true));            
            Assert.AreEqual(8, conn.GetProviderID("ASUS TeK", false));                      

            Assert.AreEqual(0, conn.GetProviderID("ASUS TeK Enterprises", true));
            Assert.AreEqual(0, conn.GetProviderID("ASUS TeK Enterprises", false));           

            Assert.AreEqual(0, conn.GetProviderID("ASUS", true));
            Assert.AreEqual(0, conn.GetProviderID("ASUS", false));   

            Assert.AreEqual(0, conn.GetProviderID("TeK", true));
            Assert.AreEqual(0, conn.GetProviderID("TeK", false));                        
        }

        [Test]
        public void GetProviderData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetProviderData(string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetProviderData(0));

            Assert.AreEqual(0, conn.GetProviderData(_FAKE).Rows.Count);
            Assert.AreEqual(1, conn.GetProviderData("ASUSTeK").Rows.Count);
            Assert.AreEqual(1, conn.GetProviderData(8).Rows.Count);                      
        }

        [Test]
        public void GetProductTemplateID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetProductTemplateID(string.Empty));

            Assert.AreEqual(20, conn.GetProductTemplateID("iPod", true));            
            Assert.AreEqual(20, conn.GetProductTemplateID("iPod", false));                      

            Assert.AreEqual(0, conn.GetProductTemplateID("i Pod", true));            
            Assert.AreEqual(20, conn.GetProductTemplateID("i Pod", false));                      

            Assert.AreEqual(0, conn.GetProductTemplateID("Apple iPod", true));
            Assert.AreEqual(0, conn.GetProductTemplateID("Apple iPod", false));           

            Assert.AreEqual(0, conn.GetProductTemplateID("HDD", true));
            Assert.AreEqual(0, conn.GetProductTemplateID("HDD", false));   

            Assert.AreEqual(0, conn.GetProductTemplateID("SH-1", true));
            Assert.AreEqual(0, conn.GetProductTemplateID("SH-1", false));   

            Assert.AreEqual(25, conn.GetProductTemplateID("HDD SH-1", true));
            Assert.AreEqual(25, conn.GetProductTemplateID("HDD SH-1", false));                      

            Assert.AreEqual(0, conn.GetProductTemplateID("HDDSH-1", true));
            Assert.AreEqual(0, conn.GetProductTemplateID("HDDSH-1", false));
        }

        [Test]
        public void GetProductTemplateData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetProductTemplateData(string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetProductTemplateData(0));

            Assert.AreEqual(0, conn.GetProductTemplateData(_FAKE).Rows.Count);
            Assert.AreEqual(2, conn.GetProductTemplateData("iPod").Rows.Count);
            Assert.AreEqual(2, conn.GetProductTemplateData(20).Rows.Count);                      
        }

        [Test]
        public void GetLastPurchaseID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.AreEqual(10, conn.GetLastPurchaseID());            
        }

        [Test]
        public void GetPurchaseID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetPurchaseID(string.Empty));

            Assert.AreEqual(6, conn.GetPurchaseID("PO00006"));
            Assert.AreEqual(9, conn.GetPurchaseID("PO00009"));
            Assert.AreEqual(0, conn.GetPurchaseID("PO00999"));
        }

        [Test]
        public void GetPurchaseCode()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetPurchaseCode(0));

            Assert.AreEqual("PO00006", conn.GetPurchaseCode(6));
            Assert.AreEqual(null, conn.GetPurchaseCode(999));            
        }

        [Test]
        public void GetPurchaseData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetPurchaseData(0));
            Assert.Throws<ArgumentNullException>(() => conn.GetPurchaseData(null));

            Assert.AreEqual(3, conn.GetPurchaseData(1).Rows.Count);
            Assert.AreEqual(3, conn.GetPurchaseData("PO00001").Rows.Count);

            Assert.AreEqual(4, conn.GetPurchaseData(6).Rows.Count);
            Assert.AreEqual(4, conn.GetPurchaseData("PO00006").Rows.Count);

            Assert.AreEqual(0, conn.GetPurchaseData(999).Rows.Count);
            Assert.AreEqual(0, conn.GetPurchaseData("PO00999").Rows.Count);
        }

        [Test]
        public void GetStockMovementData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetStockMovementData(null, false));

            Assert.AreEqual(3, conn.GetStockMovementData("PO00001", false).Rows.Count);
            Assert.AreEqual(3, conn.GetStockMovementData("PO00001", true).Rows.Count);

            Assert.AreEqual(2, conn.GetStockMovementData("PO00002", false).Rows.Count);
            Assert.AreEqual(2, conn.GetStockMovementData("PO00002", true).Rows.Count);

            Assert.AreEqual(1, conn.GetStockMovementData("PO00008", false).Rows.Count);
            Assert.AreEqual(0, conn.GetStockMovementData("PO00008", true).Rows.Count); 

            Assert.AreEqual(0, conn.GetStockMovementData("PO00999", false).Rows.Count);
            Assert.AreEqual(0, conn.GetStockMovementData("PO00999", true).Rows.Count);               

            Assert.AreEqual(1, conn.GetStockMovementData("SO020", false).Rows.Count);
            Assert.AreEqual(0, conn.GetStockMovementData("SO020", true).Rows.Count);

            Assert.AreEqual(1, conn.GetStockMovementData("SO021", false).Rows.Count);
            Assert.AreEqual(1, conn.GetStockMovementData("SO021", true).Rows.Count);

            Assert.AreEqual(0, conn.GetStockMovementData("SO999", false).Rows.Count);
            Assert.AreEqual(0, conn.GetStockMovementData("SO999", true).Rows.Count);
        }   

        [Test]
        public void GetScrappedStockData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];        
            Assert.AreEqual(3, conn.GetScrappedStockData().Rows.Count);            
        }

        [Test]
        public void GetInvoiceID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetInvoiceID(string.Empty));

            Assert.AreEqual(8, conn.GetInvoiceID("PO00008"));
            Assert.AreEqual(0, conn.GetInvoiceID("PO00009"));            
        }

        [Test]
        public void GetInvoiceCode()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetInvoiceCode(string.Empty));

            Assert.AreEqual("FACTURA /2020/0003", conn.GetInvoiceCode("PO00008"));
            Assert.AreEqual(null, conn.GetInvoiceCode("PO00009"));            
        } 

        [Test]
        public void GetInvoiceData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetInvoiceData(0));
            Assert.Throws<ArgumentNullException>(() => conn.GetInvoiceData(string.Empty));

            Assert.AreEqual(1, conn.GetInvoiceData(8).Rows.Count);
            Assert.AreEqual(0, conn.GetInvoiceData(999).Rows.Count);
            Assert.AreEqual(1, conn.GetInvoiceData("PO00008").Rows.Count);            
            Assert.AreEqual(0, conn.GetInvoiceData("PO00009").Rows.Count);    
        } 

        [Test]
        public void GetLastPosSaleID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];            
            Assert.AreEqual(3, conn.GetLastPosSaleID());
        } 

        [Test]
        public void GetPosSaleID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetPosSaleID(string.Empty));

            Assert.AreEqual(1, conn.GetPosSaleID("Main/0001"));
            Assert.AreEqual(2, conn.GetPosSaleID("Main/0002"));
            Assert.AreEqual(0, conn.GetPosSaleID("Main/0999"));
        }

        [Test]
        public void GetPosSaleCode()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetPosSaleCode(0));

            Assert.AreEqual("Main/0001", conn.GetPosSaleCode(1));
            Assert.AreEqual("Main/0002", conn.GetPosSaleCode(2));
            Assert.AreEqual(null, conn.GetPosSaleCode(999));
        }

        [Test]
        public void GetPosSaleData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetPosSaleData(0));
            Assert.Throws<ArgumentNullException>(() => conn.GetPosSaleData(string.Empty));

            Assert.AreEqual(1, conn.GetPosSaleData(1).Rows.Count);
            Assert.AreEqual(1, conn.GetPosSaleData("Main/0001").Rows.Count);

            Assert.AreEqual(2, conn.GetPosSaleData(2).Rows.Count);
            Assert.AreEqual(2, conn.GetPosSaleData("Main/0002").Rows.Count);

            Assert.AreEqual(0, conn.GetPosSaleData(999).Rows.Count);
            Assert.AreEqual(0, conn.GetPosSaleData("Main/0999").Rows.Count);
        }

        [Test]
        public void GetLastSaleID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];            
            Assert.AreEqual(23, conn.GetLastSaleID());
        } 

        [Test]
        public void GetSaleID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetSaleID(string.Empty));

            Assert.AreEqual(1, conn.GetSaleID("SO001"));
            Assert.AreEqual(2, conn.GetSaleID("SO002"));
            Assert.AreEqual(0, conn.GetSaleID("SO999"));
        }

        [Test]
        public void GetSaleCode()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetSaleCode(0));

            Assert.AreEqual("SO001", conn.GetSaleCode(1));
            Assert.AreEqual("SO002", conn.GetSaleCode(2));
            Assert.AreEqual(null, conn.GetSaleCode(999));
        }

        [Test]
        public void GetSaleData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetSaleData(0));
            Assert.Throws<ArgumentNullException>(() => conn.GetSaleData(string.Empty));

            Assert.AreEqual(3, conn.GetSaleData(1).Rows.Count);
            Assert.AreEqual(3, conn.GetSaleData("SO001").Rows.Count);

            Assert.AreEqual(2, conn.GetSaleData(2).Rows.Count);
            Assert.AreEqual(2, conn.GetSaleData("SO002").Rows.Count);

            Assert.AreEqual(0, conn.GetSaleData(999).Rows.Count);
            Assert.AreEqual(0, conn.GetSaleData("SO999").Rows.Count);
        }

        [Test]
        public void GetUserID()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetUserID(string.Empty));

            Assert.AreEqual(5, conn.GetUserID("demo", true));            
            Assert.AreEqual(5, conn.GetUserID("demo", false));                      

            Assert.AreEqual(0, conn.GetUserID("admin", true));            
            Assert.AreEqual(0, conn.GetUserID("admin", false));            

            Assert.AreEqual(1, conn.GetUserID("admin@elpuig.xeill.net", true));                                                         
            Assert.AreEqual(1, conn.GetUserID("admin@elpuig.xeill.net", false));    

            Assert.AreEqual(0, conn.GetUserID("admin @ elpuig.xeill.net", true));
            Assert.AreEqual(1, conn.GetUserID("admin @ elpuig.xeill.net", false));
        }

        [Test]
        public void GetUserName()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetUserName(0));

            Assert.AreEqual("admin@elpuig.xeill.net", conn.GetUserName(1));
            Assert.AreEqual("demo", conn.GetUserName(5));
            Assert.AreEqual(null, conn.GetUserName(999));
            
        }

        [Test]
        public void GetUserData()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetUserData(string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => conn.GetUserData(0));

            Assert.AreEqual(20, conn.GetUserData(1).Rows.Count);
            Assert.AreEqual(20, conn.GetUserData("admin@elpuig.xeill.net").Rows.Count);
            Assert.AreEqual(12, conn.GetUserData(5).Rows.Count);
            Assert.AreEqual(12, conn.GetUserData("demo").Rows.Count);
            Assert.AreEqual(0, conn.GetUserData(999).Rows.Count);
            Assert.AreEqual(0, conn.GetUserData(_FAKE).Rows.Count);
        }
    }
}