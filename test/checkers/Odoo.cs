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
using System.Collections.Generic;
using System.Collections.Concurrent;
using NUnit.Framework;

namespace AutoCheck.Test.Checkers
{   
    //TODO: Remove this file when migration from C# to YAML has been completed (this includes checker & connector mergind and also connector simplification).

    // [Parallelizable(ParallelScope.All)]   
    // public class Odoo : Core.Test
    // {  
    //     //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
                 
    //     private ConcurrentDictionary<string, AutoCheck.Checkers.Odoo> Pool = new ConcurrentDictionary<string, AutoCheck.Checkers.Odoo>();
        
    //     private AutoCheck.Checkers.Odoo Chk = null;


    //     [OneTimeSetUp]
    //     public override void OneTimeSetUp() 
    //     {            
    //         //The same database (but different checker instance, to allow parallel queries) will be shared along the different tests, because all the opperations 
    //         //are read-only; this will boost the test performance because loading the Odoo database is a long time opperation.
    //         this.Chk = new AutoCheck.Checkers.Odoo(1, "localhost", string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), "postgres", "postgres");
    //         base.OneTimeSetUp();    //needs "Chk" on "CleanUp"

    //         //Creates the database (should not exist)
    //         this.Chk.Connector.CreateDataBase(base.GetSampleFile("dump.sql"));
    //     }

    //     [OneTimeTearDown]
    //     public new void OneTimeTearDown(){     
    //         this.Pool.Clear(); 
    //     }

    //     protected override void CleanUp(){
    //         if(this.Chk.Connector.ExistsDataBase()) 
    //             this.Chk.Connector.DropDataBase();
    //     }
        
    //     [SetUp]
    //     public void Setup() 
    //     {            
    //         //Create a new and unique database connection for the current context (same DB for all tests)
    //         var chk = new AutoCheck.Checkers.Odoo(1, this.Chk.Host, this.Chk.Database, this.Chk.User, this.Chk.User);
                       
    //         //Storing the chkector instance for the current context
    //         var added = false;
    //         do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, chk);             
    //         while(!added);            
    //     }

    //     [TearDown]
    //     public void TearDown(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         chk.Dispose();
    //     }

    //     [Test]
    //     public void Constructor()
    //     {                                            
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Odoo(0, null, null, null, null));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Odoo(0, _FAKE, null, null, null));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Odoo(0, _FAKE, _FAKE, null, null));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Odoo(null,  _FAKE, _FAKE, _FAKE, _FAKE));
            
    //         Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Checkers.Odoo(0, _FAKE, _FAKE, _FAKE, _FAKE));            
            
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Odoo(1, _FAKE, _FAKE, _FAKE, _FAKE));
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Odoo(_FAKE, _FAKE, _FAKE, _FAKE, _FAKE));
    //     }

    //     [Test]
    //     public void CheckIfCompanyMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfCompanyMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfCompanyMatchesData(1, null));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfCompanyMatchesData(1, new Dictionary<string, object>(){{"name", "Play Puig"}}));            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfCompanyMatchesData(1, new Dictionary<string, object>(){{"name", "Play Puig"}, {"partner_id", 1}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfCompanyMatchesData(1, new Dictionary<string, object>(){{"name", "Play Puig"}, {"partner_id", 45}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfCompanyMatchesData(1, new Dictionary<string, object>(){{"name", "PlayPuig"}, {"partner_id", 45}}));                          
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfCompanyMatchesData(1, new Dictionary<string, object>(){{"name", "PlayPuig"}, {"partner_id", 45}, {_FAKE, null}}));
    //     }

    //     [Test]
    //     public void CheckIfProviderMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfProviderMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfProviderMatchesData(1, null));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfProviderMatchesData(11, new Dictionary<string, object>(){{"name", "Delta PC"}}));            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProviderMatchesData(11, new Dictionary<string, object>(){{"name", "Delta PC"}, {"state_id", 324}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProviderMatchesData(11, new Dictionary<string, object>(){{"name", "Delta PC"}, {"state_id", 0}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProviderMatchesData(11, new Dictionary<string, object>(){{"name", "DeltaPC"}, {"state_id", 0}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProviderMatchesData(11, new Dictionary<string, object>(){{"name", "DeltaPC"}, {"state_id", 0}, {_FAKE, null}}));
    //     }

    //     [Test]
    //     public void CheckIfProductMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfProductMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfProductMatchesData(1, null));

    //         //With no variants
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProductMatchesData(14, new Dictionary<string, object>(){{"name", "iPad Mini"}}));   
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProductMatchesData(14, new Dictionary<string, object>(){{"name", "iPad Mini"}, {"sell_price", 320.00m}}));   
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProductMatchesData(14, new Dictionary<string, object>(){{"name", "iPad Pro"}, {"sell_price", 320.00m}}));   
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProductMatchesData(14, new Dictionary<string, object>(){{"name", "iPad Pro"}, {"sell_price", 321}}));   
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfProductMatchesData(14, new Dictionary<string, object>(){{"name", "iPad Pro"}, {"sell_price", 321}, {_FAKE, null}}));   
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProductMatchesData(10, new Dictionary<string, object>(){{"name", "iPad Retina Display"}})); 

    //         //TODO: allow multi-variant check
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProductMatchesData(10, new Dictionary<string, object>(){{"name", "iPad Retina Display"}}, new Dictionary<string, string[]>(){{"Memory", new string[]{"16 GB", "32 GB"}}})); 
    //         Assert.AreEqual(new List<string>(), chk.CheckIfProductMatchesData(10, new Dictionary<string, object>(){{"name", "iPad Retina Display"}}, new Dictionary<string, string[]>(){{"Memory", new string[]{"16 GB", "32 GB"}}, {"Color", new string[]{"White", "Black"}}})); 
    //     }

    //     [Test]
    //     public void CheckIfPurchaseMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfPurchaseMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfPurchaseMatchesData(1, null));

    //         //With no variants (correct)
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 7.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 7.00m}, {"product_price_unit", 800.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 7.00m}, {"product_price_unit", 800.00m}, {"amount_total", 14563.00m}}));

    //         //With no variants (wrong)
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini FAKE"}, {"product_qty", 7.00m}, {"product_price_unit", 800.00m}, {"amount_total", 14563.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 0.00m}, {"product_price_unit", 800.00m}, {"amount_total", 14563.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 7.00m}, {"product_price_unit", 0.00m}, {"amount_total", 14563.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(4, new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 7.00m}, {"product_price_unit", 800.00m}, {"amount_total", 0.00m}}));            
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 50.00m}, {"product_price_unit", 500.00m}, {"amount_total", 62500.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 25.00m}, {"product_price_unit", 500.00m}, {"amount_total", 62500.00m}}));

    //         //With variants but implicit by using its full product name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 16 GB)"}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "[E-COM01] iPad Retina Display (Blanc, 16 GB)"}}, false, false));            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 16 GB)"}, {"amount_total", 62500.00m}, {"product_qty", 50.00m}, {"product_price_unit", 500.00m}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 16 GB)"}, {"amount_total", 62500.00m}, {"product_qty", 50.00m}, {"product_price_unit", 500.00m}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}, {"amount_total", 62500.00m}, {"product_qty", 25.00m}, {"product_price_unit", 500.00m}}, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}, {"amount_total", 62500.00m}, {"product_qty", 50.00m}, {"product_price_unit", 500.00m}}, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 32 GB)"}, {"amount_total", 62500.00m}, {"product_qty", 50.00m}, {"product_price_unit", 500.00m}}, false));

    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 62500.00m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Blanc", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"product_price_unit", 500.00m}}}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 62500.00m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Negre", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"product_price_unit", 500.00m}}}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 62500.00m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 25.00m}, {"product_price_unit", 500.00m}}}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 62500.00m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"product_price_unit", 500.00m}}}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPurchaseMatchesData(10, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 62500.00m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Negre", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"product_price_unit", 500.00m}}}}));
    //     }

    //     [Test]
    //     public void CheckIfSaleMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfSaleMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfSaleMatchesData(1, null));

    //         //With no variants (correct)
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 5.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 5.00m}, {"product_price_unit", 2950.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 5.00m}, {"product_price_unit", 2950.00m}, {"amount_total", 14981.00m}}));

    //         //With no variants (wrong)
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", _FAKE}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 1.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 5.00m}, {"product_price_unit", 2950.00f}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(7, new Dictionary<string, object>(){{"product_name", "Laptop E5023"}, {"product_qty", 5.00m}, {"product_price_unit", 2950.00m}, {"amount_total", 14981}}));
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}, {"product_price_unit", 800.40m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}, {"product_price_unit", 800.40m}, {"amount_total", 800.40m}}));

    //         //With variants but implicit by using its full product name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display (Blanc, 32 GB)"}}, false, false));            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}, {"product_qty", 1.00m}, {"product_price_unit", 800.40m}, {"amount_total", 800.40m}}, false));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display (Blanc, 32 GB)"}}, false, true));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}, {"product_qty", 1.00m}, {"product_price_unit", 800.40m}, {"amount_total", 800.40m}}, true, false));            

    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 800.40m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"product_price_unit", 800.40m}}}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSaleMatchesData(23, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"amount_total", 800.40m}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"Negre", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"product_price_unit", 800.40m}}}}));
    //     }

    //     [Test]
    //     public void CheckIfPosSaleMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfPosSaleMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfPosSaleMatchesData(1, null));

    //         //With no variants (correct)
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(2, new Dictionary<string, object>(){{"product_name", "Conference pears"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(2, new Dictionary<string, object>(){{"product_name", "Conference pears"}, {"product_qty", 1.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(2, new Dictionary<string, object>(){{"product_name", "Conference pears"}, {"product_qty", 1.00m}, {"product_price_unit", 1.70m}}));

    //         //With no variants (wrong)
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(2, new Dictionary<string, object>(){{"product_name",_FAKE}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(2, new Dictionary<string, object>(){{"product_name", "Conference pears"}, {"product_qty", 5.00m}}));
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}, {"product_price_unit", 750.0m}}));

    //         //With variants but implicit by using its full product name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (2.4 GHz, 16 GB, Black)"}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (2.4 GHz, 16 GB, Black)"}}, false, false));            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}, {"product_price_unit", 750.00m}}, false));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (2.4 GHz, 16 GB, Black)"}}, false, true));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}, {"product_price_unit", 750.00m}}, true, false));            

    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"2.4 GHz", "16 GB", "Black"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"product_price_unit", 750.00m}}}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfPosSaleMatchesData(3, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, new Dictionary<string[], Dictionary<string, object>>(){{new string[]{"2.4 GHz", "32 GB", "Black"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"product_price_unit", 750.00m}}}}));
    //     }

    //     [Test]
    //     public void CheckIfStockMovementMatchesData_PURCHASE()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(string.Empty, true, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(_FAKE, true, null));

    //         //With no variants
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_name", "Ink Cartridge"}, {"product_qty", 14.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_name", "Ink Cartridge"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_id", 5}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_id", 6}}));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_name", "Ink Cartridge X"}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", false, new Dictionary<string, object>(){{"product_name", "Ink Cartridge"}, {"product_qty", 15.00m}}));

    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 50.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 25.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display"}, {"product_qty", 50.00m}}, true, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display"}, {"product_qty", 25.00m}}, true, false));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display"}, {"product_qty", 25.00m}}, true, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display"}, {"product_qty", 25.00m}}));
            
    //         //With variants but implicit by using its full product name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 50.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display (Blanc, 32 GB)"}, {"product_qty", 25.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 50.00m}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Blanc, 32 GB)"}, {"product_qty", 25.00m}}, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 25.00m}}, false, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 25.00m}}, false));
            
    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){
    //                 {new string[]{"Negre", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"state", "done"}}},
    //                 {new string[]{"Blanc", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"state", "done"}}},                    
    //                 {new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 25.00m}, {"state", "done"}}}
    //             }
    //         ));

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", false, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){
    //                 {new string[]{"Negre", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"state", "done"}}},
    //                 {new string[]{"Blanc", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"state", "done"}}},                    
    //                 {new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"state", "done"}}}
    //             }
    //         ));
    //     }

    //     [Test]
    //     public void CheckIfStockMovementMatchesData_SALE()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(string.Empty, true, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(_FAKE, true, null));

    //         //With no variants
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_name", "Targeta gràfica"}, {"product_qty", 1.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_name", "[CARD] Targeta gràfica"}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 32}, {"product_qty", 1.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 32}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 32}, {"product_qty", 1.00m}}, false, true));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_name", "[CARD] Targeta gràfica"}, {"product_qty", 1.00m}}, false, true));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 1}, {"product_qty", 1.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 1}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO020", false, new Dictionary<string, object>(){{"product_id", 1}, {"product_qty", 1.00m}}, false, true));

    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display"}, {"product_qty", 1.00m}}, true, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_id", 12}, {"product_qty", 1.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_id", 12}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_id", 12}, {"product_qty", 1.00m}}, false, true));
                
    //         //With variants but implicit by using its full product name            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display (Blanc, 32 GB)"}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, new Dictionary<string, object>(){{"product_name", "[E-COM03] iPad Retina Display (Blanc, 16 GB)"}, {"product_qty", 1.00m}}, false, false));
                    
    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"state", "done"}}}
    //             }
    //         ));

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", false, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"state", "cancelled"}}}
    //             }
    //         ));
    //     }

    //     [Test]
    //     public void CheckIfStockMovementMatchesData_RETURN()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(string.Empty, true, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfStockMovementMatchesData(_FAKE, true, null));

    //         //With no variants
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00002", true, new Dictionary<string, object>(){{"product_name", "Pen drive, SP-2"}, {"product_qty", 5.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00006", true, new Dictionary<string, object>(){{"product_name", "Ink Cartridge"}, {"product_qty", 14.00m}}));
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 10.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 5.00m}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "[E-COM01] iPad Retina Display"}, {"product_qty", 10.00m}}, true, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display"}, {"product_qty", 5.00m}}, true, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 50.00m}}));

    //         //With variants but implicit by using its full product name        
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "[E-COM01] iPad Retina Display (Blanc, 16 GB)"}, {"product_qty", 10.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "[E-COM01] iPad Retina Display (Blanc, 16 GB)"}, {"product_qty", 10.00m}}, false, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 5.00m}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "iPad Retina Display (Negre, 16 GB)"}, {"product_qty", 5.00m}}, false));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, new Dictionary<string, object>(){{"product_name", "[E-COM01] iPad Retina Display (Blanc, 16 GB)"}, {"product_qty", 50.00m}}, false, false));
            
    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("PO00010", true, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"Blanc", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 10.00m}, {"state", "done"}}},
    //                 {new string[]{"Negre", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 5.00m}, {"state", "done"}}}
    //             }
    //         ));

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfStockMovementMatchesData("SO022", true, 
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"Blanc", "32 GB"}, new Dictionary<string, object>(){{"product_qty", 25.00m}, {"state", "done"}}},
    //                 {new string[]{"Negre", "16 GB"}, new Dictionary<string, object>(){{"product_qty", 50.00m}, {"state", "done"}}}
    //             }
    //         ));
    //     }    

    //     [Test]
    //     public void CheckIfScrappedStockMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(), true));

    //         //With no variants
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "iPad Mini"}, {"product_qty", 25.00m}}, true));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "Ink Cartridge"}, {"product_qty", 14.00m}}, true));
            
    //         //With variants but without using it
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}}, true));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display"}, {"product_qty", 1.00m}}, true, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}, {"product_qty", 1.00m}}, true, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display"}, {"product_qty", 1.00m}}, true, true));
            
    //         // //With variants but implicit by using its full product name        
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}}, false));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}}, false, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}}, true, false));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){{"product_name", "[E-COM02] iPad Retina Display (2.4 GHz, 16 GB, Black)"}, {"product_qty", 1.00m}}, true));
            
    //         //With variants but explicit by using variant name
    //         Assert.AreEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"2.4 GHz", "16 GB", "Black"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"state", "done"}}}
    //             }
    //         ));

    //          Assert.AreNotEqual(new List<string>(), chk.CheckIfScrappedStockMatchesData(
    //             new Dictionary<string, object>(){{"product_name", "iPad Retina Display"}}, 
    //             new Dictionary<string[], Dictionary<string, object>>(){                    
    //                 {new string[]{"16 GB", "Black"}, new Dictionary<string, object>(){{"product_qty", 1.00m}, {"state", "done"}}}
    //             }
    //         ));           
    //     }

    //     [Test]
    //     public void CheckIfInvoiceMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfInvoiceMatchesData(string.Empty, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfInvoiceMatchesData(_FAKE, null));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfInvoiceMatchesData("SO021", new Dictionary<string, object>(){{"state", "paid"}, {"amount_total", 40000.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfInvoiceMatchesData("SO022", new Dictionary<string, object>(){{"state", "paid"}, {"amount_total", 40000.00m}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfInvoiceMatchesData(_FAKE, new Dictionary<string, object>(){{"state", "paid"}, {"amount_total", 40000.00m}}));
    //     }

    //     [Test]
    //     public void CheckIfUserMatchesData()
    //     {                    
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         Assert.Throws<ArgumentOutOfRangeException>(() => chk.CheckIfUserMatchesData(0, null));
    //         Assert.Throws<ArgumentNullException>(() => chk.CheckIfUserMatchesData(1, null));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfUserMatchesData(1, new Dictionary<string, object>(){{"name", "admin@elpuig.xeill.net"}}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfUserMatchesData(6, new Dictionary<string, object>(){{"name", "portal"}}, new string[]{"Portal", "Tax display B2B"}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfUserMatchesData(1, new Dictionary<string, object>(){{"name", "admin@elpuig.xeill.net"}}, new string[]{"Settings", "Billing"}));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfUserMatchesData(6, new Dictionary<string, object>(){{"name", "portal"}}, new string[]{"Portal", "Tax display B2B"}, true));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfUserMatchesData(1, new Dictionary<string, object>(){{"name", "admin@elpuig.xeill.net"}}, new string[]{"Settings", "Billing"}, true));
    //     }
    // }
}