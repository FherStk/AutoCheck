using System;
using Npgsql;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{    

    class OdooValidator: ValidatorBaseDataBase{
        public OdooValidator(string server, string database): base(server, database){                        
        } 
        private class Template{
            public string student {get; set;}
            public string database {get; set;}
            public string server {get; set;}
            public string username {get; set;}
            public string password {get; set;}     
            public string companyName {get; set;}
            public string providerName {get; set;}
            public string productName {get; set;}
            public decimal productPurchasePrice {get; set;}
            public decimal productSellPrice {get; set;}
            public string productVariantName {get; set;}
            public string[] productVariantValues {get; set;}
            public decimal purchaseAmountTotal {get; set;}
            public decimal saleAmountTotal {get; set;}
            public int productPosSellQuantity;
            public int[] productPurchaseQuantities {get; set;}
            public int[] productSellQuantities {get; set;}
            public int[] productReturnQuantities {get; set;}
            public int[] productScrappedQuantities {get; set;}
            public decimal refundAmountTotal {get; set;}
            public string user {get; set;}    
        }       
        private Template data; 
        private int companyID;
        private int providerID;
        private int templateID;
        private int[] prodIDs;
        private int purchaseID;
        private string purchaseCode;
        private int saleID;
        private string saleCode;
        private string saleInvoiceCode;
        private int userID;        
        private CultureInfo cultureEN = CultureInfo.CreateSpecificCulture("en-EN");
        public override List<TestResult> Validate()
        {   
            //TODO: new method to avoid opening and closing for simple messages...
            Terminal.WriteLine(string.Format("Checking the databse ~{0}:", this.DataBase), ConsoleColor.Yellow);            

            using (this.Conn){
               this.Conn.Open();                                           
                ClearResults();

                OpenTest("     Getting the company data: ");
                CloseTest(CheckCompany());

                OpenTest("     Getting the provider data: ");
                CloseTest(CheckProvider());

                OpenTest("     Getting the product data: ");                        
                CloseTest(CheckProducts());

                OpenTest("     Getting the purchase order data: ");
                CloseTest(CheckPurchase());

                OpenTest("     Getting the cargo in movements: ");
                CloseTest(CheckCargoIn());

                OpenTest("     Getting the purchase invoice data: ");
                CloseTest(CheckPurchaseInvoice());

                OpenTest("     Getting the POS sales data: ");
                CloseTest(CheckPosSale());

                OpenTest("     Getting the backoffice sale data: ");
                CloseTest(CheckBackOfficeSale());

                OpenTest("     Getting the cargo out movements: ");
                CloseTest(CheckCargoOut());

                OpenTest("     Getting the sale invoice data: ");
                CloseTest(CheckSaleInvoice());

                OpenTest("     Getting the cargo return movements: ");
                CloseTest(CheckCargoReturn());

                OpenTest("     Getting the refund invoice data: ");
                CloseTest(CheckRefundInvoice());

                OpenTest("     Getting the scrapped cargo movements: ");
                CloseTest(CheckScrappedCargo());

                OpenTest("     Getting the user data: ");
                CloseTest(CheckUser());                                     
            }

            PrintScore();
            return GlobalResults;
        }        
        private new void ClearResults(){
            base.ClearResults();

            this.companyID = 0;
            this.providerID = 0;
            this.templateID = 0;
            this.prodIDs = new int[4];
            this.purchaseID = 0;
            this.purchaseCode = string.Empty;
            this.saleID = 0;
            this.saleCode = string.Empty;
            this.saleInvoiceCode = string.Empty;
            this.userID = 0;
            
            //The idea behind using this kind of dataset is being able to use different statements for each student performing the exam.
            //TODO: test this with multi-statement exams because some changes will be needed (no more time for testing, sorry).
            string student = this.DataBase.Substring(5).Replace("_", " ");        
            this.data = new Template(){    
                student = student,
                server = this.Server,
                username = "postgres",
                password = "postgres",
                database = this.DataBase,            
                companyName = string.Format("Samarretes Frikis {0}", student), //"Samarretes Frikis",
                providerName =  string.Format("Bueno Bonito y Barato {0}", student), //"Bueno Bonito y Barato",
                productName =  string.Format("Samarreta Friki {0}", student), //"Samarreta Friki", 
                productPurchasePrice = 9.99m,
                productSellPrice = 19.99m,
                productVariantName = "Talla",
                productVariantValues = new[]{"S", "M", "L", "XL"},                
                purchaseAmountTotal = 1450.56m,
                saleAmountTotal = 799.60m,
                refundAmountTotal = 399.80m,
                productPosSellQuantity = 1,
                productPurchaseQuantities = new int[]{15, 30, 50, 25},
                productSellQuantities = new int[]{10, 10, 10, 10},
                productReturnQuantities = new int[]{5, 5, 5, 5},
                productScrappedQuantities = new int[]{0, 0, 0, 1},
                user = string.Format("{0}@elpuig.xeill.net", student.ToLower().Replace(" ", "_")) //"venedor@elpuig.xeill.net"//
            };
        }                          
        private string GetWhereForName(string expectedValue, string dbField){
            string company = expectedValue;
            company = company.Replace(data.student, "").Trim();
            string[] student = data.student.Split(" ");

            //TODO: check if student.length > 2
            return string.Format("{3} like '{0}%' AND {3} like '%{1}%' AND {3} like '%{2}%'", company, student[0], student[1], dbField);
        }
        private List<string> CheckCompany(){    
            List<string> errors = new List<string>();   

            //company         
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT com.id, com.name FROM public.res_company com WHERE {0}", GetWhereForName(data.companyName, "com.name")),this.Conn)){                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    //Critial first value must match the amount of tests to perform in case there's no critical error.
                    if(!dr.Read()){
                        errors.Add(String.Format("Unable to find any company named '{0}'", data.companyName));                            
                        companyID = int.MaxValue;
                    } 
                    else{                        
                        companyID = (int)dr["id"];
                        if(!dr["name"].ToString().Equals(data.companyName)) errors.Add(string.Format("Incorrect company name: expected->'{0}'; found->'{1}'", data.companyName, dr["name"].ToString()));
                    }
                }
            }

            //image, must be requested this way because some students create a new company instead of edit the current one
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT ata.file_size FROM public.ir_attachment ata 
                                                                        WHERE ata.res_model='res.partner' AND res_field='image'
                                                                        AND {0}", GetWhereForName(data.companyName, "ata.res_name")), this.Conn)){
                var image = cmd.ExecuteScalar();
                if(image == null) errors.Add(String.Format("Unable to find any logo attached to the company '{0}'", data.companyName));
            }

            //default company
            if(companyID > 1){
                companyID = 1;
                errors.Add("The default company is being used in order to store the business data.");
            }

            return errors;
        }             
        private List<string> CheckProvider(){            
            List<string> errors = new List<string>();        
                            
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT pro.id, pro.name, pro.is_company, ata.file_size FROM public.res_partner pro
                                                            LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                                                            WHERE {0} AND pro.parent_id IS NULL AND pro.company_id={1}
							    ORDER BY pro.id DESC", GetWhereForName(data.providerName, "pro.name"), companyID), this.Conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any provider named '{0}'", data.providerName));
                    else{                        
                        providerID = (int)dr["id"] ;                                                                
                        if(dr["file_size"] == System.DBNull.Value) errors.Add(String.Format("Unable to find any picture attached to the provider named '{0}'.", data.providerName));
                        if(((bool)dr["is_company"]) == false) errors.Add(String.Format("The provider named '{0}' has not been set up as a company.", data.providerName));
                        if(!dr["name"].ToString().Equals(data.providerName)) errors.Add(string.Format("Incorrect provider name: expected->'{0}'; found->'{1}'", data.providerName, dr["name"].ToString()));
                    }
                }
            }

            return errors;
        }
        private List<string> CheckProducts(){            
            List<string> errors = new List<string>();        
            

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_ID, ata.file_size, att.name AS attribute, val.name AS value FROM public.product_product pro
                                                                        LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                                                                        LEFT JOIN public.ir_attachment ata ON ata.res_id=tpl.id AND ata.res_model='product.template' AND ata.res_id = tpl.id  AND ata.res_field='image'
                                                                        LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                                                                        LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                                                                        LEFT JOIN public.product_attribute att ON att.id = val.attribute_id
                                                                        LEFT JOIN public.product_supplierinfo sup ON sup.product_tmpl_id = tpl.id
                                                                        WHERE {0} AND tpl.company_id={1}", GetWhereForName(data.productName, "tpl.name"), companyID), this.Conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    while(dr.Read()){                        
                        templateID = (int)dr["template_id"] ;                                                                            
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != "product") errors.Add(String.Format("The product named '{0}' has not been set up as an stockable product.", data.productName));
                        //if(dr["file_size"] == System.DBNull.Value) errors.Add(String.Format("Unable to find any picture attached to the product named '{0}'.", data.productName));
                        if(dr["attribute"] == System.DBNull.Value || dr["attribute"].ToString() != data.productVariantName) errors.Add(String.Format("The product named '{0}' does not contains variants called '{1}'.", data.productName, data.productVariantName));                        
                        if(!dr["name"].ToString().Equals(data.productName)) errors.Add(string.Format("Incorrect product name: expected->'{0}'; found->'{1}'", data.productName, dr["name"].ToString()));
                        
                        //TODO: this must be changed in order to work with "data" values (sorry, no time).
                        switch(dr["value"].ToString().Trim()){
                            case "S":
                                prodIDs[0] = (int)dr["product_id"];
                                break;
                            
                            case "M":
                                prodIDs[1] = (int)dr["product_id"];
                                break;

                            case "L":
                                prodIDs[2] = (int)dr["product_id"];
                                break;

                            case "XL":
                                prodIDs[3] = (int)dr["product_id"];
                                break;

                            default:
                                errors.Add(String.Format("Unexpected variant value '{0}' found for the variant attribute '{1}' on product '{2}'.", dr["value"].ToString(), data.productVariantName, data.productName));
                                break;
                        }
                        

                        if(dr["supplier_id"] == System.DBNull.Value || ((int)dr["supplier_id"]) != providerID) errors.Add(String.Format("Unexpected supplier ID found for the product named '{2}': expected->'{0}'; current->'{1}'.", providerID, dr["supplier_id"].ToString(), data.productName));
                        if(dr["purchase_price"] == System.DBNull.Value || ((decimal)dr["purchase_price"]) != data.productPurchasePrice) errors.Add(String.Format("Unexpected purchase price found for the product named '{2}': expected->'{0}'; current->'{1}'.", data.productPurchasePrice.ToString(), dr["purchase_price"].ToString(), data.productName));
                        if(dr["sell_price"] == System.DBNull.Value || ((decimal)dr["sell_price"]) != data.productSellPrice) errors.Add(String.Format("Unexpected sale price found for the product named '{2}': expected->'{0}'; current->'{1}'.", data.productSellPrice.ToString(), dr["sell_price"].ToString(), data.productName));
                    }
                        
                    if(templateID == 0) errors.Add(String.Format("Unable to find any product named '{0}'", data.productName)); 
                    else {
                        for(int i=0; i<prodIDs.Length; i++){
                            if(prodIDs[i] == 0) errors.Add(String.Format("Unable to find a product named '{0}' using the variant '{1}={2}'", data.productName, data.productVariantName, data.productVariantValues[i]));                         
                        }
                    }                        
                }
            }

            return errors;
        }
        private List<string> CheckPurchase(){            
            List<string> errors = new List<string>();        
                
            /*using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.purchase_order 	
                                                                                    WHERE (amount_total={0} OR amount_untaxed={0}) AND company_id={1}
                                                                                    ORDER BY id DESC", data.purchaseAmountTotal, companyID), this.Conn)){*/
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.purchase_order WHERE company_id={0} ORDER BY id DESC", companyID), this.Conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    //if(!dr.Read()) errors.Add(String.Format("Unable to find any purchase order with the correct total amount of '{0}'", data.purchaseAmountTotal));
                    if(!dr.Read()) errors.Add("Unable to find any purchase order");
                    else{
                        purchaseID = (int)dr["id"];
                        purchaseCode = dr["name"].ToString();
                    }
                }                    
            }

            if(purchaseID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT name, product_qty, price_unit, product_id FROM public.purchase_order_line 
                                                                                WHERE order_id='{0}' AND company_id='{1}'
                                                                                ORDER BY id ASC", purchaseID, companyID), this.Conn)){
                    
                    errors.AddRange(CheckOrderLines(cmd, data.productPurchaseQuantities, false));                    
                }
            }

            return errors;
        }        
        private List<string> CheckBackOfficeSale(){
            List<string> errors = new List<string>();               
            
                
            /*using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.sale_order
                                                                            WHERE (amount_total={0} OR amount_untaxed={0}) AND company_id={1}
                                                                            ORDER BY id DESC", data.saleAmountTotal, companyID), this.Conn)){*/
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.sale_order WHERE state='{0}' AND company_id={1} ORDER BY id DESC", "sale", companyID), this.Conn)){
                
               using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    //if(!dr.Read()) errors.Add(String.Format("Unable to find any sale order with the correct total amount of '{0}'", data.saleAmountTotal));
                    if(!dr.Read()) errors.Add("Unable to find any sale order");
                    else{                        
                        saleID = (int)dr["id"];
                        saleCode = dr["name"].ToString();
                    }
                }                    
            }

            if(saleID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT name, product_uom_qty, price_unit, product_id FROM public.sale_order_line 
                                                                                WHERE order_id='{0}' AND company_id='{1}'", saleID, companyID), this.Conn)){
                    
                    errors.AddRange(CheckOrderLines(cmd, data.productSellQuantities, true));                    
                }
            }

            return errors;
        }
        private List<string> CheckPosSale(){
            List<string> errors = new List<string>();        
            
            int posID = 0;
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name, state FROM public.pos_order WHERE company_id={0} ORDER BY id DESC", companyID), this.Conn)){
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add("Unable to find any POS order");
                    else{                        
                        posID = (int)dr["id"];
                        
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "done") 
                            errors.Add(String.Format("The statet for the POS sale '{0}' must be 'done'.", dr["name"].ToString()));                          
                    }
                }
            }
        
            if(posID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT order_id FROM public.pos_order_line
                                                                                    WHERE product_id={0} AND qty={1} AND order_id={2} AND company_id={3}
                                                                                    ORDER BY ID DESC", prodIDs[2], data.productPosSellQuantity, posID, companyID), this.Conn)){
                               
                    var result = cmd.ExecuteScalar();
                    if(result == null) errors.Add("Unable to find a POS order line with the correct values.");
                }        
            }

            return errors;
        }
        private List<string> CheckOrderLines(NpgsqlCommand cmd, int[] productQuantities, bool sale){
            List<string> errors = new List<string>(); 

             using (NpgsqlDataReader dr = cmd.ExecuteReader()){               
                int line = 0;
                string qtyField = string.Format("product_{0}", sale ? "uom_qty" : "qty");

                //TODO: this order could be wrong... must be ordered by size (S, M, L, XL).                
                while(dr.Read()){  
                    string variant = dr["name"].ToString();
                    if(variant.Contains("(")){
                        variant = variant.Substring(variant.IndexOf("(")+1);
                        variant = variant.Substring(0, variant.IndexOf(")")).Replace(" ", "");
                    }

                    int item = -1;
                    switch(variant){
                        case "S":
                            item = 0;
                            break;

                        case "M":
                            item = 1;
                            break;

                        case "L":
                            item = 2;
                            break;

                        case "XL":
                            item = 3;
                            break;
                    }


                    if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains(((int)dr["product_id"]))) errors.Add(String.Format("Unexpected product ID '{0}' found for the product named '{1}'.", dr["product_id"].ToString(), dr["name"].ToString()));                    
                    if(item != -1 && (dr[qtyField] == System.DBNull.Value || (int)(decimal)dr[qtyField] != productQuantities[item])) errors.Add(String.Format("Unexpected product quantity found for the product named '{2}': expected->'{0}'; current->'{1}'.", ((int)productQuantities[item]).ToString(), ((int)(decimal)dr[qtyField]).ToString(), dr["name"].ToString()));
                    line ++;
                }

                if(line < productQuantities.Count())
                    errors.Add(String.Format("Unable to find some sold products for the sale '{0}'", saleCode));                        
            }

            return errors;
        }
        private List<string> CheckPurchaseInvoice(){
            return CheckInvoice(purchaseCode);
        }
        private List<string> CheckSaleInvoice(){
            return CheckInvoice(saleCode);
        }                
        private List<string> CheckRefundInvoice(){
            return CheckInvoice(saleInvoiceCode);                  
        }   
        private List<string> CheckInvoice(string origin){
            List<string> errors = new List<string>();                                            
            
            string type = string.Empty;
            if(origin.Length > 0){            
                switch(origin.Substring(0, 2)){
                    case "PO":
                        type = "in_invoice";
                        break;

                    case "SO":
                        type = "out_invoice";                    
                        break;

                    default:
                        type = "out_refund";                                
                        break;
                }
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT number, amount_total, amount_untaxed, type, state FROM public.account_invoice
                                                                            WHERE origin='{0}' AND type='{1}' AND company_id={2}", origin, type, companyID), this.Conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any sales invoice for the order '{0}'", origin));
                    else{                        
                        if(origin.StartsWith("SO")) saleInvoiceCode = dr["number"].ToString();                        
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != type) errors.Add(string.Format("Unexpected type for the invoice '{2}': expected->'{0}'; current->'{1}'", type, dr["type"].ToString(), origin));
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "paid") errors.Add(string.Format("The invoice '{0}' status must be 'paid'", origin));            
                    }
                }                
            }  

            return errors;          
        } 
        private List<string> CheckCargoIn(){
            return CheckCargo(purchaseCode, data.productPurchaseQuantities, false);                  
        }
        private List<string> CheckCargoOut(){
            return CheckCargo(saleCode, data.productSellQuantities, false);                          
        }
        private List<string> CheckCargoReturn(){
            return CheckCargo(saleCode, data.productReturnQuantities, true);                                     
        } 
        private List<string> CheckScrappedCargo(){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name FROM public.stock_move
                                                                            WHERE scrapped={0} AND product_id={1} AND product_qty={2} AND company_id={3}", true, prodIDs[3], data.productScrappedQuantities[3], companyID), this.Conn)){
                
                var result = cmd.ExecuteScalar();                
                if(result == null) errors.Add(string.Format("No scrapped cargo has been found for the product '{0} ({1})'", data.productName, data.productVariantValues[3]));                
            } 

            return errors;                              
        }    
        private List<string> CheckCargo(string order, int[] productQuantities, bool isReturn){
            List<string> errors = new List<string>();
                        
            bool input = order.StartsWith("PO");
            if(isReturn) input = !input;          
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name, product_id, product_qty, state, reference FROM public.stock_move
                                                                            WHERE origin='{0}' AND reference LIKE '%/{1}/%' AND company_id={2}", order, (input ? "IN" : "OUT"), companyID), this.Conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){                    
                    int line = 0;

                    while(dr.Read()){     
                        if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains((int)dr["product_id"])) errors.Add("Unable to find any cargo movement for correct products.");
                        if(dr["product_qty"] == System.DBNull.Value || !productQuantities.Contains((int)(decimal)dr["product_qty"])) errors.Add("Unable to find any cargo movement for correct quantities.");
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "done") errors.Add("Unable to find any cargo movement with the correct state.");                                           
                        line++;
                    }

                    if(line < productQuantities.Count())
                        errors.Add(String.Format("Unable to find some cargo movements for the item '{0}'", order));
                }                
            }  

            return errors;                             
        }             
        private List<string> CheckUser(){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT usr.id FROM public.res_users usr
                                                                                    INNER JOIN public.res_partner prt ON usr.partner_id = prt.id
                                                                                    WHERE prt.name='{0}' AND usr.company_id={1}", data.student,companyID), this.Conn)){
                
                var result = cmd.ExecuteScalar();                
                if(result == null) errors.Add(string.Format("No user named '{0}' has been found.", data.student));
                else userID = (int)result;
            } 

            if(userID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT grp.name FROM public.res_groups_users_rel usr
                                                                                        INNER JOIN public.res_groups grp ON usr.gid = grp.id
                                                                                        WHERE usr.uid={0}", userID), this.Conn)){
                    List<string> currentPermissions = new List<string>();
                    List<string> expectedPermissions = new List<string>(){"Technical Features", "Contact Creation", "Sales Pricelists", "Manage Pricelist Items", "Manage Product Variants", "Tax display B2B", "User"};                    
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){     
                        while(dr.Read()){     
                            if(!expectedPermissions.Contains(dr["name"].ToString())) errors.Add(string.Format("The permission '{0}' was not expected for the user '{1}'.", dr["name"].ToString(), data.student));
                            currentPermissions.Add(dr["name"].ToString());
                        }
                    } 

                    foreach(string ep in expectedPermissions){
                        if(!currentPermissions.Contains(ep)) errors.Add(string.Format("The permission '{0}' was expected but not found for the user '{1}'.", ep, data.student));
                    }
                }
            }

            return errors;
        }                  
    }
}
