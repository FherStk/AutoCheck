using System;
using Npgsql;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{    

    class OdooValidator{
        private class Template{
            public string student {get; set;}
            public string database {get; set;}
            public string server {get; set;}
            public string username {get; set;}
            public string password {get; set;}     
            public string companyName {get; set;}
            public string providerName {get; set;}
            public string productName {get; set;}
            public float productPurchasePrice {get; set;}
            public float productSellPrice {get; set;}
            public string productVariantName {get; set;}
            public string[] productVariantValues {get; set;}
            public float purchaseAmountUntaxed {get; set;}
            public float saleAmountUntaxed {get; set;}
            public int[] productPurchaseQuantities {get; set;}
            public int[] productSellQuantities {get; set;}
            public int[] productReturnQuantities {get; set;}
            public int[] productScrappedQuantities {get; set;}
            public float refundAmountUntaxed {get; set;}
            public string user {get; set;}    
        }       
        private static int companyID;
        private static int providerID;
        private static int templateID;
        private static int[] prodIDs;
        private static int purchaseID;
        private static string purchaseCode;
        private static int posID;
        private static int saleID;
        private static string saleCode;
        private static string saleInvoiceCode;
        private static int userID;
        private static int success;
        private static int errors;
        private static CultureInfo cultureEN = CultureInfo.CreateSpecificCulture("en-EN");
        public static void ValidateDataBase(string server, string database)
        {   
            string student = database.Substring(5).Replace("_", " ");                 
            Template data = new Template(){    
                student = student,
                server = server,
                username = "postgres",
                password = "postgres",
                database = database,            
                companyName = string.Format("Samarretes Frikis {0}", student), //"Samarretes Frikis",
                providerName =  string.Format("Bueno Bonito y Barato {0}", student), //"Bueno Bonito y Barato",
                productName =  string.Format("Samarreta Friki {0}", student), //"Samarreta Friki", 
                productPurchasePrice = 9.99f,
                productSellPrice = 19.99f,
                productVariantName = "Talla",
                productVariantValues = new[]{"S", "M", "L", "XL"},                
                purchaseAmountUntaxed = 1198.80f,
                saleAmountUntaxed = 799.60f,
                refundAmountUntaxed = 399.80f,
                productPurchaseQuantities = new int[]{15, 30, 50, 25},
                productSellQuantities = new int[]{10, 10, 10, 10},
                productReturnQuantities = new int[]{5, 5, 5, 5},
                productScrappedQuantities = new int[]{0, 0, 0, 1},
                user = string.Format("{0}@elpuig.xeill.net", student.ToLower().Replace(" ", "_")) //"venedor@elpuig.xeill.net"//
            };

            string databaseName = string.Format("odoo_{0}", data.student.Replace(" ", "_"));
            Utils.Write("Checking the databse ");
            Utils.WriteLine(databaseName, ConsoleColor.Yellow);
            using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", data.server, data.username, data.password, data.database))){
                conn.Open();

                //Reset global data
                companyID = 0;
                providerID = 0;
                templateID = 0;
                prodIDs = new int[4];
                purchaseID = 0;
                purchaseCode = string.Empty;
                posID = 0;
                saleID = 0;
                saleCode = string.Empty;
                saleInvoiceCode = string.Empty;
                userID = 0;                                
                
                Utils.Write("     Getting the company data: ");
                ProcessResults(CheckCompany(conn, data));

                Utils.Write("     Getting the provider data: ");
                ProcessResults(CheckProvider(conn, data));

                Utils.Write("     Getting the product data: ");                        
                ProcessResults(CheckProducts(conn, data));

                Utils.Write("     Getting the purchase order data: ");
                ProcessResults(CheckPurchase(conn, data));

                Utils.Write("     Getting the cargo in movements: ");
                ProcessResults(CheckCargoIn(conn, data));

                Utils.Write("     Getting the purchase invoice data: ");
                ProcessResults(CheckPurchaseInvoice(conn, data));

                Utils.Write("     Getting the TPV sales data: ");
                ProcessResults(CheckTpvSale(conn, data));

                Utils.Write("     Getting the backoffice sale data: ");
                ProcessResults(CheckBackOfficeSale(conn, data));

                Utils.Write("     Getting the cargo out movements: ");
                ProcessResults(CheckCargoOut(conn, data));

                Utils.Write("     Getting the sale invoice data: ");
                ProcessResults(CheckSaleInvoice(conn, data));

                Utils.Write("     Getting the cargo return movements: ");
                ProcessResults(CheckCargoReturn(conn, data));

                Utils.Write("     Getting the refund invoice data: ");
                ProcessResults(CheckRefundInvoice(conn, data));

                Utils.Write("     Getting the scrapped cargo movements: ");
                ProcessResults(CheckScrappedCargo(conn, data));

                Utils.Write("     Getting the user data: ");
                ProcessResults(CheckUser(conn, data));     

                double score = Math.Round((float)success / (float)success + (float)errors, 2);
                Utils.BreakLine();
                Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
                Utils.WriteLine(score.ToString(), score < 4 ? ConsoleColor.Red: ConsoleColor.Green);
            }
        } 
        private static void ProcessResults(List<string> list){
            if(list.Count == 0) success++;
            else errors += list.Count;
            Utils.PrintResults(list);
        }                  
        private static List<string> CheckCompany(NpgsqlConnection conn, Template data){    
            List<string> errors = new List<string>();            

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT com.id, ata.file_size FROM public.res_company com
                                                            LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                                                            WHERE com.name='{0}'", data.companyName), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    //Critial first value must match the amount of tests to perform in case there's no critical error.
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any company named '{0}'", data.companyName));                            
                    else{                        
                        companyID = (int)dr["id"];
                        if(dr["file_size"] == System.DBNull.Value) errors.Add(string.Empty);
                    }
                }
            }

            return errors;
        }         
        private static List<string> CheckProvider(NpgsqlConnection conn, Template data){            
            List<string> errors = new List<string>();        
                            
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT pro.id, pro.is_company, ata.file_size FROM public.res_partner pro
                                                            LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                                                            WHERE pro.name='{0}' AND pro.parent_id IS NULL AND pro.company_id={1}", data.providerName, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any provider named '{0}'", data.providerName));
                    else{                        
                        providerID = (int)dr["id"] ;                                                                
                        if(dr["file_size"] == System.DBNull.Value) errors.Add(String.Format("Unable to find any picture attached to the provider named '{0}'.", data.providerName));
                        if(((bool)dr["is_company"]) == false) errors.Add(String.Format("The provider named '{0}' has not been set up as a company.", data.providerName));
                    }
                }
            }

            return errors;
        }
        private static List<string> CheckProducts(NpgsqlConnection conn, Template data){            
            List<string> errors = new List<string>();        
            

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_ID, ata.file_size, att.name AS attribute, val.name AS value FROM public.product_product pro
                                                                        LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                                                                        LEFT JOIN public.ir_attachment ata ON ata.res_id=tpl.id AND ata.res_model='product.template' AND ata.res_id = tpl.id  AND ata.res_field='image'
                                                                        LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                                                                        LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                                                                        LEFT JOIN public.product_attribute att ON att.id = val.attribute_id
                                                                        LEFT JOIN public.product_supplierinfo sup ON sup.product_tmpl_id = tpl.id
                                                                        WHERE tpl.name='{0}' AND tpl.company_id={1}", data.productName, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    while(dr.Read()){                        
                        templateID = (int)dr["template_id"] ;                                                                            
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != "product") errors.Add(String.Format("The product named '{0}' has not been set up as an stockable product.", data.productName));
                        if(dr["file_size"] == System.DBNull.Value) errors.Add(String.Format("Unable to find any picture attached to the product named '{0}'.", data.productName));
                        if(dr["attribute"] == System.DBNull.Value || dr["attribute"].ToString() != data.productVariantName) errors.Add(String.Format("The product named '{0}' does not contains variants called '{1}'.", data.productName, data.productVariantName));
                        if(dr["value"] == System.DBNull.Value || !data.productVariantValues.Contains(dr["value"].ToString())) errors.Add(String.Format("Unexpected variant value '{0}' found for the variant attribute '{1}' on product '{2}'.", dr["value"].ToString(), data.productVariantName, data.productName));
                        else{                            
                            switch(dr["value"].ToString()){
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
                            }
                        }

                        if(dr["supplier_id"] == System.DBNull.Value || ((int)dr["supplier_id"]) != providerID) errors.Add(String.Format("Unexpected supplier ID found for the product named '{2}': expected->'{0}'; current->'{1}'.", providerID, dr["supplier_id"].ToString(), data.productName));
                        if(dr["purchase_price"] == System.DBNull.Value || ((decimal)dr["purchase_price"]) != (decimal)data.productPurchasePrice) errors.Add(String.Format("Unexpected purchase price found for the product named '{2}': expected->'{0}'; current->'{1}'.", data.productPurchasePrice.ToString(), dr["purchase_price"].ToString(), data.productName));
                        if(dr["sell_price"] == System.DBNull.Value || ((decimal)dr["sell_price"]) != (decimal)data.productSellPrice) errors.Add(String.Format("Unexpected sale price found for the product named '{2}': expected->'{0}'; current->'{1}'.", data.productSellPrice.ToString(), dr["sell_price"].ToString(), data.productName));
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
        private static List<string> CheckPurchase(NpgsqlConnection conn, Template data){            
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.purchase_order 
                                                                            WHERE amount_untaxed={0:0.00} AND company_id={1}", data.purchaseAmountUntaxed, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any purchase order with the correct amount of '{0}'", data.purchaseAmountUntaxed));
                    else{
                        purchaseID = (int)dr["id"];
                        purchaseCode = dr["name"].ToString();
                    }
                }                    
            }

            if(purchaseID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT name, product_qty, price_unit, product_id FROM public.purchase_order_line 
                                                                                WHERE order_id='{0}' AND company_id='{1}'
                                                                                ORDER BY id ASC", purchaseID, companyID), conn)){
                    
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                        int line = 0;

                        //TODO: this order could be wrong... must be ordered by size (S, M, L, XL).
                        while(dr.Read()){                                
                            if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains(((int)dr["product_id"]))) errors.Add(String.Format("Unexpected product ID '{0}' found for the product named '{1}'.", dr["product_id"].ToString(), dr["name"].ToString()));
                            if(dr["product_qty"] == System.DBNull.Value || (decimal)dr["product_qty"] != data.productPurchaseQuantities[line]) errors.Add(String.Format("Unexpected product quantity found for the product named '{2}': expected->'{0}'; current->'{1}'.", ((int)data.productPurchaseQuantities[line]).ToString(), ((int)(decimal)dr["product_qty"]).ToString(), dr["name"].ToString()));
                            if(dr["price_unit"] == System.DBNull.Value || (decimal)dr["price_unit"] != (decimal)data.productPurchasePrice) errors.Add(String.Format("Unexpected price unit found for the product named '{2}': expected->'{0}'; current->'{1}'.", data.productPurchasePrice.ToString(), dr["price_unit"].ToString(), dr["name"].ToString()));
                            line++;
                        }

                        if(line < data.productVariantValues.Count())
                            errors.Add(String.Format("Unable to find some purchased products for the order '{0}'", purchaseCode));                        
                    }
                }
            }

            return errors;
        }
        private static List<string> CheckCargoIn(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name, product_id, product_qty, state, reference FROM public.stock_move
                                                                            WHERE origin='{0}' AND company_id={1}", purchaseCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){                    
                    int line = 0;

                    while(dr.Read()){                        
                        if(dr["reference"] == System.DBNull.Value) errors.Add("Unable to find any cargo in movement.");
                        if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains((int)dr["product_id"])) errors.Add("Unable to find any cargo in movement for correct products.");
                        if(dr["product_qty"] == System.DBNull.Value || !data.productPurchaseQuantities.Contains((int)(decimal)dr["product_qty"])) errors.Add("Unable to find any cargo in movement for correct quantities.");
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "done") errors.Add("Unable to find any cargo in movement with the correct state.");
                        
                        line++;
                    }

                    if(line < data.productVariantValues.Count())
                        errors.Add(String.Format("Unable to find some cargo movements for the order '{0}'", purchaseCode));
                }                
            }

            return errors;                   
        }
        private static List<string> CheckPurchaseInvoice(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            //TODO: continue fixing error messages from here :)    
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT number, amount_untaxed, type, state FROM public.account_invoice
                                                                            WHERE origin='{0}' AND company_id={1}", purchaseCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any purchase invoice for the order '{0}'", purchaseCode));
                    else{                        
                        if(dr["number"] == System.DBNull.Value) errors.Add(string.Empty);                        
                        if(dr["amount_untaxed"] == System.DBNull.Value || (decimal)dr["amount_untaxed"] != (decimal)data.purchaseAmountUntaxed) errors.Add(string.Empty);
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != "in_invoice") errors.Add(dr["type"].ToString());
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "paid") errors.Add(dr["state"].ToString());
                    }
                }                
            } 

            return errors;           
        }
        private static List<string> CheckTpvSale(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT order_id FROM public.pos_order_line
                                                                            WHERE product_id={0} AND price_unit={1} AND qty={2} AND company_id={3}", prodIDs[2], data.productSellPrice, 1, companyID), conn)){
                               
                var result = cmd.ExecuteScalar();
                if(result == null) errors.Add("Unable to find any TPV sales entry");
                else posID = (int)result;                    
            }

            if(posID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT state FROM public.pos_order
                                                                                WHERE id={0} AND company_id={1}", posID, companyID), conn)){
                    
                    var result = cmd.ExecuteScalar();
                    if(result == null || result.ToString() != "done") errors.Add(string.Empty);                    
                }          
            }

            return errors;
        }
        private static List<string> CheckBackOfficeSale(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();               
            
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id, name FROM public.sale_order
                                                                            WHERE amount_untaxed={0} AND company_id={1}", data.saleAmountUntaxed, companyID), conn)){
                
               using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any sale order with the correct amount of '{0}'", data.saleAmountUntaxed));
                    else{                        
                        saleID = (int)dr["id"];
                        saleCode = dr["name"].ToString();
                    }
                }                    
            }

            if(saleID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT name, product_uom_qty, price_unit, product_id FROM public.sale_order_line 
                                                                                WHERE order_id='{0}' AND company_id='{1}'", saleID, companyID), conn)){
                    
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                        int line = 0;

                        while(dr.Read()){                                
                            if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains(((int)dr["product_id"]))) errors.Add(dr["name"].ToString());                            
                            if(dr["product_uom_qty"] == System.DBNull.Value || (decimal)dr["product_uom_qty"] != data.productSellQuantities[line]) errors.Add(dr["product_uom_qty"].ToString());
                            if(dr["price_unit"] == System.DBNull.Value || (decimal)dr["price_unit"] != (decimal)data.productSellPrice) errors.Add(dr["price_unit"].ToString());                            
                            line++;
                        }

                        if(line < data.productVariantValues.Count())
                            errors.Add(String.Format("Unable to find some sold products for the sale '{0}'", saleCode));                        
                    }
                }
            }

            return errors;
        }
        private static List<string> CheckCargoOut(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
            
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name, product_id, product_qty, state, reference FROM public.stock_move
                                                                            WHERE to_refund=false AND origin='{0}' AND company_id={1}", saleCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){                    
                    int line = 0;

                    while(dr.Read()){                        
                        if(dr["reference"] == System.DBNull.Value) errors.Add(string.Empty);
                        if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains((int)dr["product_id"])) errors.Add(string.Empty);
                        if(dr["product_qty"] == System.DBNull.Value || !data.productSellQuantities.Contains((int)(decimal)dr["product_qty"])) errors.Add(string.Empty);
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "done") errors.Add(dr["state"].ToString());                                                
                        line++;
                    }

                    if(line < data.productVariantValues.Count())
                        errors.Add(String.Format("Unable to find some cargo movements for the sale '{0}'", saleCode));
                }                
            }  

            return errors;                             
        }
        private static List<string> CheckSaleInvoice(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
            
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT number, amount_untaxed, type, state FROM public.account_invoice
                                                                            WHERE origin='{0}' AND company_id={1}", saleCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any sales invoice for the order '{0}'", saleCode));
                    else{                                                 
                        if(dr["number"] == System.DBNull.Value) errors.Add(string.Empty);
                        else saleInvoiceCode = dr["number"].ToString();

                        if(dr["amount_untaxed"] == System.DBNull.Value || (decimal)dr["amount_untaxed"] != (decimal)data.saleAmountUntaxed) errors.Add(string.Empty);
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != "out_invoice") errors.Add(dr["type"].ToString());
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "paid") errors.Add(dr["state"].ToString());                        
                    }
                }                
            }   

            return errors;         
        }      
        private static List<string> CheckCargoReturn(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
            
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name, product_id, product_qty, state, reference FROM public.stock_move
                                                                            WHERE to_refund=true AND origin='{0}' AND company_id={1}", saleCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){                    
                    int line = 0;

                    while(dr.Read()){
                        if(dr["reference"] == System.DBNull.Value) errors.Add(string.Empty);                        
                        if(dr["product_id"] == System.DBNull.Value || !prodIDs.Contains((int)dr["product_id"])) errors.Add(string.Empty);                        
                        if(dr["product_qty"] == System.DBNull.Value || !data.productReturnQuantities.Contains((int)(decimal)dr["product_qty"])) errors.Add(string.Empty);
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "done") errors.Add(dr["state"].ToString());
                        line++;
                    }

                    if(line < data.productVariantValues.Count())
                        errors.Add(String.Format("Unable to find some cargo movements for the return '{0}'", saleCode));
                }                
            }  

            return errors;                             
        }   
        private static List<string> CheckRefundInvoice(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT number, amount_untaxed, type, state FROM public.account_invoice
                                                                            WHERE origin='{0}' AND company_id={1}", saleInvoiceCode, companyID), conn)){
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    if(!dr.Read()) errors.Add(String.Format("Unable to find any sales invoice for the order '{0}'", saleCode));
                    else{                        
                        if(dr["number"] == System.DBNull.Value) errors.Add(string.Empty);
                        if(dr["amount_untaxed"] == System.DBNull.Value || (decimal)dr["amount_untaxed"] != (decimal)data.refundAmountUntaxed) errors.Add(string.Empty);
                        if(dr["type"] == System.DBNull.Value || dr["type"].ToString() != "out_refund") errors.Add(dr["type"].ToString());
                        if(dr["state"] == System.DBNull.Value || dr["state"].ToString() != "paid") errors.Add(dr["state"].ToString());
                    }
                }                
            }  

            return errors;          
        }   
        private static List<string> CheckScrappedCargo(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT name FROM public.stock_move
                                                                            WHERE scrapped={0} AND product_id={1} AND product_qty={2} AND company_id={3}", true, prodIDs[3], data.productScrappedQuantities[3], companyID), conn)){
                
                var result = cmd.ExecuteScalar();                
                if(result == null) errors.Add(string.Format("No scrapped cargo has been found for the product '{0} ({1})'", data.productName, data.productVariantValues[3]));                
            } 

            return errors;                              
        }        
        private static List<string> CheckUser(NpgsqlConnection conn, Template data){
            List<string> errors = new List<string>();        
                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT id FROM public.res_users 
                                                                                    WHERE login='{0}' AND company_id={1}", data.user,companyID), conn)){
                
                var result = cmd.ExecuteScalar();                
                if(result == null) errors.Add(string.Format("No user has been found for the login '{0}", data.user));
                else userID = (int)result;
            } 

            if(userID > 0){
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(cultureEN, @"SELECT gid FROM public.res_groups_users_rel
                                                                                WHERE gid={0} AND uid={1}", 51, userID), conn)){
                    
                    var result = cmd.ExecuteScalar();                    
                    if(result == null) errors.Add(string.Empty);
                } 
            }

            return errors;
        }                  
    }
}