using System;
using Npgsql;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    //TODO: Must be removed
    class ViewsExtendedValidator: ValidatorBaseDataBase{     
        public ViewsExtendedValidator(string server, string database): base(server, database){                        
        } 

        public override List<TestResult> Validate()
        {    
            //Note: This assignment validator is an extended variant of ViewsValidator, with a more complex statement and strict scoring system (all or nothing for each question).
            //Terminal.WriteLine(string.Format("Checking the databse ~{0}:", this.DataBase), ConsoleColor.Yellow);    
            //Terminal.Indent();
                                 
            using (this.Conn){
                this.Conn.Open();                                           
                ClearResults();
                
                //question 1
                CloseTest(CheckViewExists(), 2); 

                //question 2         
                CloseTest(CheckInsertRule(), 3); 
                
                //question 3                     
                CloseTest(CheckUpdateRule(), 2);  

                //question 4                  
                CloseTest(CheckDeleteRule(), 2);    

                //question 5      
                OpenTest("Checking the permissions for the user ~it... ", ConsoleColor.Yellow);
                AppendTest(CheckTableMatchPrivileges("it", "gerencia", "report", "r", false), false);
                CloseTest(CheckSchemaMatchPrivileges("it", "gerencia", "U", false), 1);               
            }        

            //no more questions, your grace            
            PrintScore();
            //Terminal.UnIndent();
            
            return GlobalResults;
        }  
        private new void ClearResults(){
            base.ClearResults();
        }
        private int GetLastID(string schema, string table, string field="id"){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT MAX({0}) FROM {1}.{2};", field, schema, table), this.Conn)){                                
                return (int)cmd.ExecuteScalar();                   
            } 
        }
        private bool CompareViewData(int idEmpleat, string nomEmpleat, int idFabrica, string nomFabrica, int idProducte, string nomProducte){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"   SELECT * FROM gerencia.report 
                                                                            WHERE id_producte={0} AND id_fabrica={1} AND id_responsable={2}", idProducte, idFabrica, idEmpleat), this.Conn)){                                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    while(dr.Read()){         
                        if( dr["id_fabrica"].Equals(idFabrica) && dr["nom_fabrica"].ToString().Equals(nomFabrica) && 
                            dr["id_producte"].Equals(idProducte) && dr["nom_producte"].ToString().Equals(nomProducte) && 
                            dr["id_responsable"].Equals(idEmpleat) && dr["nom_responsable"].ToString().Equals(nomEmpleat))
                        {
                            return true;
                        }                            
                    }
                }                 
            } 

            return false;
        }
        private long CountRegisters(string schema, string table){
            return CountRegisters(schema, table, null, 0);
        }
        private long CountRegisters(string schema, string table, string pkField, int pkValue){
            string query = string.Format("SELECT COUNT(*) FROM {0}.{1}", schema, table);
            if(!string.IsNullOrEmpty(pkField)) query = string.Format("{0} WHERE {1}={2}", query, pkField, pkValue);
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Conn)){                                
                return (long)cmd.ExecuteScalar();                
            }
        }
        private List<string> CheckViewExists(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the view ~gerencia.report... ", ConsoleColor.Yellow);
            try{
                //If not exists, an exception will be thrown                    
                CountRegisters("gerencia", "report");                                                             
            }
            catch{
                errors.Add("The view does not exists.");
                return errors;
            }            
            
            //Let's gona insert some values in order to check the correctness of the view's query
            //NOTE: no transactions will be used, because a single user (this process) will be using the database.      
            try{                  
                string nomEmpleat = "TEST EMPLOYEE 1";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO rrhh.empleats (id, nom, cognoms, email, id_cap, id_departament)
                                                                VALUES ((SELECT MAX(id)+1 FROM rrhh.empleats), '{0}', 'NONE', 'NONE', 4, 4);", nomEmpleat), this.Conn)){                
                    cmd.ExecuteScalar();                       
                } 
                
                string nomFabrica = "TEST FACTORY 1";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO produccio.fabriques (id, nom, pais, direccio, telefon, id_responsable)
                                                                VALUES ((SELECT MAX(id)+1 FROM produccio.fabriques), '{0}', 'SPAIN', 'NONE', 'NONE', (SELECT MAX(id) FROM rrhh.empleats));", nomFabrica), this.Conn)){                
                    cmd.ExecuteScalar();                      
                } 
            
                string nomProducte = "TEST PRODUCT 1";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO produccio.productes (id, nom, codi, descripcio)
                                                                VALUES ((SELECT MAX(id)+1 FROM produccio.productes), '{0}', 'NONE', 'NONE');", nomProducte), this.Conn)){                
                    cmd.ExecuteScalar();                      
                }             

                using (NpgsqlCommand cmd = new NpgsqlCommand(@" INSERT INTO produccio.fabricacio (id_fabrica, id_producte)
                                                                VALUES ((SELECT MAX(id) FROM produccio.fabriques), (SELECT MAX(id) FROM produccio.productes));", this.Conn)){                
                    cmd.ExecuteScalar();                      
                } 

                //Getting the new IDs and checking the view's data
                int idEmpleat;
                int idProducte;
                int idFabrica;

                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");

                if(!CompareViewData(idEmpleat, nomEmpleat, idFabrica, nomFabrica, idProducte, nomProducte)) 
                    errors.Add("Unable to find some data into the report view.");
               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }  
        private List<string> CheckInsertRule(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule INSERT ON ~gerencia.report... ", ConsoleColor.Yellow);
            string nomEmpleat = "TEST EMPLOYEE 2";
            string nomFabrica = "TEST FACTORY 2";
            string nomProducte = "TEST PRODUCT 2";

            try{
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO gerencia.report (nom_producte, nom_fabrica, nom_responsable)
                                                                VALUES ('{0}', '{1}', '{2}');", nomProducte, nomFabrica, nomEmpleat), this.Conn)){                
                
                        cmd.ExecuteScalar();                  
                }

                //First: checking the view's data
                int idEmpleat;
                int idProducte;
                int idFabrica;

                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");
                
                if(!CompareViewData(idEmpleat, nomEmpleat, idFabrica, nomFabrica, idProducte, nomProducte)) 
                    errors.Add("Unable to find some data into the report view.");
            
                
                //Second: checking the table's data  
                //Empleats
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT * FROM rrhh.empleats WHERE id={0}", idEmpleat), this.Conn)){                
                    try{
                        using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                            bool found = false;   
                            int count = 0;                 

                            while(dr.Read()){         
                                count++;

                                if(dr["nom"].ToString().Equals(nomEmpleat) && dr["cognoms"].ToString().ToUpper().Equals("NONE") && dr["email"].ToString().Equals("email@email.cat") && dr["id_cap"].Equals(4) && dr["id_departament"].Equals(4)){
                                    found = true;
                                    break;
                                }                        
                            }

                            if(count == 0) errors.Add("Unable to find any new employee after inserting through the view.");
                            else if(!found) errors.Add("Incorrect employee data found after inserting through the view.");
                        } 
                    }
                    catch(Exception e){
                        errors.Add(e.Message);
                    }                               
                }  

                //Fabriques
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT * FROM produccio.fabriques WHERE id={0}", idFabrica), this.Conn)){                
                    try{
                        using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                            bool found = false;   
                            int count = 0;                 

                            while(dr.Read()){         
                                count++;

                                if(dr["nom"].ToString().Equals(nomFabrica) && dr["pais"].ToString().ToUpper().Equals("SPAIN") && dr["direccio"].ToString().Equals("C. Anselm Rius 10. Santa Coloma de Gramenet") && dr["telefon"].ToString().Equals("+3493391000") && dr["id_responsable"].Equals(idEmpleat)){
                                    found = true;
                                    break;
                                }                        
                            }

                            if(count == 0) errors.Add("Unable to find any new factory after inserting through the view.");
                            else if(!found) errors.Add("Incorrect factory data found after inserting through the view.");
                        } 
                    }
                    catch(Exception e){
                        errors.Add(e.Message);
                    }                               
                } 

                //Productes
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT * FROM produccio.productes WHERE id={0}", idProducte), this.Conn)){                
                    try{
                        using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                            bool found = false;   
                            int count = 0;                 

                            while(dr.Read()){         
                                count++;

                                if(dr["nom"].ToString().Equals(nomProducte) && dr["codi"].ToString().ToUpper().Equals("NONE") && dr["descripcio"].ToString().Equals("NONE")){
                                    found = true;
                                    break;
                                }                        
                            }

                            if(count == 0) errors.Add("Unable to find any new product after inserting through the view.");
                            else if(!found) errors.Add("Incorrect product data found after inserting through the view.");
                        } 
                    }
                    catch(Exception e){
                        errors.Add(e.Message);
                    }                               
                } 

                //Produccio
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(*) FROM produccio.fabricacio WHERE id_producte={0} AND id_fabrica={1}", idProducte, idFabrica), this.Conn)){                
                    try{
                        if((long)cmd.ExecuteScalar() == 0) errors.Add("Unable to find any new production entry after inserting through the view.");                                        
                    }
                    catch(Exception e){
                        errors.Add(e.Message);
                    }                               
                }      
            }
            catch(Exception e){
                errors.Add(e.Message);
            }       

            return errors;
        }
        private List<string> CheckUpdateRule(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule UPDATE ON ~gerencia.report... ", ConsoleColor.Yellow);
            int idEmpleat;
            int idProducte;
            int idFabrica;

            try{
                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");
            

                string nomEmpleat = "TEST EMPLOYEE 3";
                string nomFabrica = "TEST FACTORY 3";
                string nomProducte = "TEST PRODUCT 3";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" UPDATE gerencia.report SET nom_producte='{0}' WHERE id_producte={1};", nomProducte, idProducte), this.Conn)){                
                   
                    cmd.ExecuteScalar();                      
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" UPDATE gerencia.report SET nom_fabrica='{0}' WHERE id_fabrica={1};", nomFabrica, idFabrica), this.Conn)){                
                    cmd.ExecuteScalar();                   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" UPDATE gerencia.report SET nom_responsable='{0}' WHERE id_responsable={1};", nomEmpleat, idEmpleat), this.Conn)){                
                    cmd.ExecuteScalar();   
                    
                }

                if(!CompareViewData(idEmpleat, nomEmpleat, idFabrica, nomFabrica, idProducte, nomProducte))
                    errors.Add("Updated view data missmatch.");                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;                                     
        }
        private List<string> CheckDeleteRule(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule DELETE ON ~gerencia.report... ", ConsoleColor.Yellow);
            int idEmpleat;
            int idProducte;
            int idFabrica;

            try{
                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");
           
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" DELETE FROM gerencia.report WHERE id_producte={0};", idProducte), this.Conn)){                
                    cmd.ExecuteScalar();   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" DELETE FROM gerencia.report WHERE id_fabrica={0};", idFabrica), this.Conn)){                
                    cmd.ExecuteScalar();   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" DELETE FROM gerencia.report WHERE id_responsable={0};", idEmpleat), this.Conn)){                
                    cmd.ExecuteScalar();   
                }

                //Checking the data
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT id_responsable FROM produccio.fabriques WHERE id={0};", idFabrica), this.Conn)){                
                    if(cmd.ExecuteScalar() != DBNull.Value)
                        errors.Add("Invalid id_responsable value found on fabriques after deletting on the view.");   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT COUNT(*) FROM produccio.fabricacio WHERE id_fabrica={0} AND id_producte={1};", idFabrica, idProducte), this.Conn)){                
                    if((long)cmd.ExecuteScalar() > 0)
                        errors.Add("Invalid production data found after deletting on the view.");   
                }

                if(CountRegisters("produccio", "fabriques", "id", idFabrica) == 0)
                    errors.Add("No factory has been found after deletting on the view.");   

                if(CountRegisters("produccio", "productes", "id", idProducte) == 0)
                    errors.Add("No product has been found after deletting on the view.");   

                
                if(CountRegisters("rrhh", "empleats", "id", idEmpleat) == 0)
                    errors.Add("No employee has been found after deletting on the view.");   
             }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;                                     
        }
    }
}