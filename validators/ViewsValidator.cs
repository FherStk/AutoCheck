using System;
using Npgsql;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class ViewsValidator: ValidatorBaseDataBase{     
        public ViewsValidator(string server, string database): base(server, database){                        
        } 

        public override List<TestResult> Validate()
        {                           
            Terminal.WriteLine(string.Format("Checking the databse ~{0}:", this.DataBase), ConsoleColor.Yellow);    
            Terminal.Indent();
                                 
            using (this.Conn){
                this.Conn.Open();                                           
                ClearResults();
                
                //question 1
                CloseTest(CheckViewExists()); 
                CloseTest(CheckViewIsCorrect()); 

                //question 2         
                CloseTest(CheckInsertRuleForEmployee()); 
                CloseTest(CheckInsertRuleForFactory()); 
                
                //question 3                     
                CloseTest(CheckUpdateRuleForEmployee());  
                CloseTest(CheckUpdateRuleForFactory());  

                //question 4                  
                CloseTest(CheckDeleteRuleForFabrica());    
                CloseTest(CheckDeleteRuleForResponsable());    
                CloseTest(CheckDeleteRuleWithNoFilter());    

                //question 5      
                OpenTest("Checking the permissions for the user ~it... ", ConsoleColor.Yellow);
                AppendTest(CheckTableMatchPrivileges("it", "gerencia", "responsables", "r", false), false);
                CloseTest(CheckSchemaMatchPrivileges("it", "gerencia", "U", false), 1);               
            }        

            //no more questions, your grace            
            PrintScore();
            Terminal.UnIndent();
            
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
        private (int idEmpleat, int idFabrica) InsertDataOnTables(string nomEmpleat, string cognomsEmpleat, string nomFabrica){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO rrhh.empleats (id, nom, cognoms, email, id_cap, id_departament)
                                                            VALUES ((SELECT MAX(id)+1 FROM rrhh.empleats), '{0}', '{1}', 'NONE', 1, 1);", nomEmpleat, cognomsEmpleat), this.Conn)){                
                
                cmd.ExecuteScalar();                   
            } 
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO produccio.fabriques (id, nom, pais, direccio, telefon, id_responsable)
                                                            VALUES ((SELECT MAX(id)+1 FROM produccio.fabriques), '{0}', 'SPAIN', 'NONE', 'NONE', (SELECT MAX(id) FROM rrhh.empleats));", nomFabrica), this.Conn)){                
                
                cmd.ExecuteScalar();                                       
            }
                        
            int idEmpleat = GetLastID("rrhh", "empleats");
            int idFabrica = GetLastID("produccio", "fabriques");                        

            return (idEmpleat: idEmpleat, idFabrica: idFabrica);
        }
        private (int idEmpleat, int idFabrica) InsertDataOnView(string nomEmpleat, string cognomsEmpleat, string nomFabrica){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO gerencia.responsables (nom_fabrica, nom_responsable, cognoms_responsable)
                                                                VALUES ('{0}', '{1}', '{2}');", nomFabrica, nomEmpleat, cognomsEmpleat), this.Conn)){                
                
                cmd.ExecuteScalar();                   
            }                     
                        
            int idEmpleat = GetLastID("rrhh", "empleats");
            int idFabrica = GetLastID("produccio", "fabriques");                        

            return (idEmpleat: idEmpleat, idFabrica: idFabrica);
        }
        private bool CompareViewData(int idEmpleat, string nomEmpleat, string cognomsEmpleat, int idFabrica, string nomFabrica){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"   SELECT * FROM gerencia.responsables 
                                                                            WHERE id_fabrica={0} AND id_responsable={1}", idFabrica, idEmpleat), this.Conn)){                                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    while(dr.Read()){         
                        if( dr["id_fabrica"].Equals(idFabrica) && dr["nom_fabrica"].ToString().Equals(nomFabrica) && 
                            dr["id_responsable"].Equals(idEmpleat) && dr["nom_responsable"].ToString().Equals(nomEmpleat) && dr["cognoms_responsable"].ToString().Equals(cognomsEmpleat))
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

            OpenTest("Checking the creation of the view ~gerencia.responsables... ", ConsoleColor.Yellow);
            try{
                //If not exists, an exception will be thrown                    
                CountRegisters("gerencia", "responsables");                                                             
            }
            catch{
                errors.Add("The view does not exists.");
                return errors;
            }                               

            return errors;
        }  
        private List<string> CheckViewIsCorrect(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the content for the view ~gerencia.responsables... ", ConsoleColor.Yellow);                      
            
            //Let's gona insert some values in order to check the correctness of the view's query
            //NOTE: no transactions will be used, because a single user (this process) will be using the database.                        
            string nomEmpleat = "TEST EMPLOYEE NAME 1";
            string cognomsEmpleat = "TEST EMPLOYEE SURNAME 1";
            string nomFabrica = "TEST FACTORY 1";                        

            try{
                (int idEmpleat, int idFabrica) = InsertDataOnTables(nomEmpleat, cognomsEmpleat, nomFabrica);                      
                if(!CompareViewData(idEmpleat, nomEmpleat, cognomsEmpleat, idFabrica, nomFabrica)) 
                    errors.Add("Unable to find some data into the report view.");
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;
        } 
        private List<string> CheckInsertRuleForEmployee(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule INSERT ON ~gerencia.responsables~ (for employee)... ", ConsoleColor.Yellow);
            string nomEmpleat = "TEST EMPLOYEE NAME 2";
            string cognomsEmpleat = "TEST EMPLOYEE SURNAME 2";
            string nomFabrica = "TEST FACTORY 2";
            
            try{
                (int idEmpleat, int idFabrica) = InsertDataOnView( nomEmpleat, cognomsEmpleat, nomFabrica);                                
                if(!CompareViewData(idEmpleat, nomEmpleat, cognomsEmpleat, idFabrica, nomFabrica)) 
                    errors.Add("Unable to find some data into the report view.");                
                            
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT * FROM rrhh.empleats WHERE id={0}", idEmpleat), this.Conn)){                
                    try{
                        using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                            bool found = false;   
                            int count = 0;        
                            string[] student = Utils.DataBaseToStudentName(this.DataBase).Split(" ");                        

                            while(dr.Read()){         
                                count++;

                                //The email must contain at least two items from the student's name
                                //FPS: Hotfix: the email is not working during the exam due the database name...
                                //string email = dr["email"].ToString().ToLower();                            
                                //if(dr["nom"].ToString().Equals(nomEmpleat) && dr["cognoms"].ToString().Equals(cognomsEmpleat) && dr["id_cap"].Equals(1) && dr["id_departament"].Equals(1) && student.Where(x => email.Contains(x.ToLower())).Count() >= 2){
                                if(dr["nom"].ToString().Equals(nomEmpleat) && dr["cognoms"].ToString().Equals(cognomsEmpleat) && dr["id_cap"].Equals(1) && dr["id_departament"].Equals(1)){
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
            } 
            catch(Exception e){
                errors.Add(e.Message);                
            }    

            return errors;
        }
        private List<string> CheckInsertRuleForFactory(){    
           List<string> errors = new List<string>();                             

            OpenTest("Checking the rule INSERT ON ~gerencia.responsables~ (for factory)... ", ConsoleColor.Yellow);
            string nomEmpleat = "TEST EMPLOYEE NAME 3";
            string cognomsEmpleat = "TEST EMPLOYEE SURNAME 3";
            string nomFabrica = "TEST FACTORY 3";
            
            try{
                (int idEmpleat, int idFabrica) = InsertDataOnView( nomEmpleat, cognomsEmpleat, nomFabrica);                                
                if(!CompareViewData(idEmpleat, nomEmpleat, cognomsEmpleat, idFabrica, nomFabrica)) 
                    errors.Add("Unable to find some data into the report view.");                
                                           
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT * FROM produccio.fabriques WHERE id={0}", idFabrica), this.Conn)){                
                    try{
                        using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                            bool found = false;   
                            int count = 0;                 

                            while(dr.Read()){         
                                count++;

                                if(dr["nom"].ToString().Equals(nomFabrica) && dr["pais"].ToString().ToUpper().Equals("SPAIN") && dr["direccio"].ToString().Equals("NONE") && dr["telefon"].ToString().Equals("+3493391000") && dr["id_responsable"].Equals(idEmpleat)){
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
            } 
            catch(Exception e){
                errors.Add(e.Message);                
            }    

            return errors;
        }
        private List<string> CheckUpdateRuleForEmployee(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule UPDATE ON ~gerencia.responsables~ (for employee)... ", ConsoleColor.Yellow);

            try{                    
                string nomEmpleat = "TEST EMPLOYEE NAME 4";
                string cognomsEmpleat = "TEST EMPLOYEE SURNAME 4";
                string nomFabrica = "TEST FACTORY 4";           
                (int idEmpleat, int idFabrica) = InsertDataOnView( nomEmpleat, cognomsEmpleat, nomFabrica);      

                nomEmpleat = "TEST EMPLOYEE NAME 4 UPDATED";
                cognomsEmpleat = "TEST EMPLOYEE SURNAME 4 UPDATED";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" UPDATE gerencia.responsables SET nom_responsable='{0}', cognoms_responsable='{1}' WHERE id_responsable={2};", nomEmpleat, cognomsEmpleat, idEmpleat), this.Conn)){                
                    cmd.ExecuteScalar();   
                }               

                if(!CompareViewData(idEmpleat, nomEmpleat, cognomsEmpleat, idFabrica, nomFabrica))
                    errors.Add("Updated view data missmatch.");
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;                                     
        }
        private List<string> CheckUpdateRuleForFactory(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule UPDATE ON ~gerencia.responsables~ (for factory)... ", ConsoleColor.Yellow);

            try{                    
                string nomEmpleat = "TEST EMPLOYEE NAME 5";
                string cognomsEmpleat = "TEST EMPLOYEE SURNAME 5";
                string nomFabrica = "TEST FACTORY 5";           
                (int idEmpleat, int idFabrica) = InsertDataOnView( nomEmpleat, cognomsEmpleat, nomFabrica);      

                nomFabrica = "TEST FACTORY 5 UPDATED";    
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" UPDATE gerencia.responsables SET nom_fabrica='{0}' WHERE id_fabrica={1};", nomFabrica, idFabrica), this.Conn)){                
                    cmd.ExecuteScalar();   
                }             

                if(!CompareViewData(idEmpleat, nomEmpleat, cognomsEmpleat, idFabrica, nomFabrica))
                    errors.Add("Updated view data missmatch.");
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;                                  
        }
        private List<string> CheckDeleteRuleForFabrica(){ 
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule DELETE ON ~gerencia.responsables~ (for id_fabrica)... ", ConsoleColor.Yellow);                                     

            try{
                (int idEmpleatDel, int idFabricaDel) = InsertDataOnTables("TEST EMPLOYEE NAME 6",  "TEST EMPLOYEE SURNAME 6", "TEST FACTORY 6");       
                (int idEmpleatNoDel, int idFabricaNoDel) = InsertDataOnTables("TEST EMPLOYEE NAME 7",  "TEST EMPLOYEE SURNAME 7", "TEST FACTORY 7");       
                
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" DELETE FROM gerencia.responsables WHERE id_fabrica={0};", idFabricaDel), this.Conn)){                
                    cmd.ExecuteScalar();   
                } 
                
                string msg = "Invalid id_responsable value found on fabriques after deletting on the view using the id_fabrica value on filter.";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT id_responsable FROM produccio.fabriques WHERE id={0};", idFabricaDel), this.Conn)){                
                    if(cmd.ExecuteScalar() != DBNull.Value) errors.Add(msg);   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT id_responsable FROM produccio.fabriques WHERE id={0};", idFabricaNoDel), this.Conn)){                
                    if(cmd.ExecuteScalar() == DBNull.Value) errors.Add(msg);   
                }

                if(CountRegisters("produccio", "fabriques", "id", idFabricaDel) == 0 || CountRegisters("produccio", "fabriques", "id", idFabricaNoDel) == 0)
                    errors.Add("No factory has been found after deletting on the view using the id_fabrica value on filter.");                   
            }
            catch(Exception e){
                errors.Add(e.Message);
                return errors;
            }
            
            return errors;
        }
        private List<string> CheckDeleteRuleForResponsable(){ 
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule DELETE ON ~gerencia.responsables~ (for id_responsable)... ", ConsoleColor.Yellow);                                     

            try{
                (int idEmpleatDel, int idFabricaDel) = InsertDataOnTables("TEST EMPLOYEE NAME 8",  "TEST EMPLOYEE SURNAME 8", "TEST FACTORY 8");
                (int idEmpleatNoDel, int idFabricaNoDel) = InsertDataOnTables("TEST EMPLOYEE NAME 9",  "TEST EMPLOYEE SURNAME 9", "TEST FACTORY 9");       
                
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" DELETE FROM gerencia.responsables WHERE id_responsable={0};", idEmpleatDel), this.Conn)){                
                    cmd.ExecuteScalar();   
                } 

                if(CountRegisters("produccio", "fabriques", "id_responsable", idEmpleatDel) > 0 || CountRegisters("produccio", "fabriques", "id_responsable", idEmpleatNoDel) == 0)
                    errors.Add("Invalid id_responsable value found on fabriques after deletting on the view using the id_responsable value on filter.");  

                if(CountRegisters("rrhh", "empleats", "id", idEmpleatDel) == 0 || CountRegisters("rrhh", "empleats", "id", idEmpleatNoDel) == 0)
                    errors.Add("No employee has been found after deletting on the view using the id_responsable value on filter.");

            }
            catch(Exception e){
                errors.Add(e.Message);
                return errors;
            }
            
            return errors;
        }        
        private List<string> CheckDeleteRuleWithNoFilter(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule DELETE ON ~gerencia.responsables~ (with no filter )... ", ConsoleColor.Yellow);

             try{
                (int idEmpleatDel, int idFabricaDel) = InsertDataOnTables("TEST EMPLOYEE NAME 10",  "TEST EMPLOYEE SURNAME 10", "TEST FACTORY 10");       
                (int idEmpleatDelToo, int idFabricaDelToo) = InsertDataOnTables("TEST EMPLOYEE NAME 11",  "TEST EMPLOYEE SURNAME 10", "TEST FACTORY 11");       
                
                
                using (NpgsqlCommand cmd = new NpgsqlCommand(@" DELETE FROM gerencia.responsables;", this.Conn)){                
                    cmd.ExecuteScalar();   
                } 

                string msg = "Invalid id_responsable value found on fabriques after deletting on the view with no filter.";
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT id_responsable FROM produccio.fabriques WHERE id={0};", idFabricaDel), this.Conn)){                
                    if(cmd.ExecuteScalar() != DBNull.Value) errors.Add(msg);   
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT id_responsable FROM produccio.fabriques WHERE id={0};", idFabricaDelToo), this.Conn)){                
                    if(cmd.ExecuteScalar() != DBNull.Value) errors.Add(msg);   
                }

                if(CountRegisters("produccio", "fabriques", "id_responsable", idEmpleatDel) > 0 || CountRegisters("produccio", "fabriques", "id_responsable", idEmpleatDelToo) > 0)
                    errors.Add("Invalid id_responsable value found on fabriques after deletting on the view with no filter.");   

                if(CountRegisters("produccio", "fabriques", "id", idFabricaDel) == 0 || CountRegisters("produccio", "fabriques", "id", idFabricaDelToo) == 0)
                    errors.Add("No factory has been found after deletting on the view with no filter.");                 

                if(CountRegisters("rrhh", "empleats", "id", idEmpleatDelToo) == 0 || CountRegisters("rrhh", "empleats", "id", idEmpleatDelToo) == 0)
                    errors.Add("No employee has been found after deletting on the view with no filter.");                                
            }
            catch(Exception e){
                errors.Add(e.Message);
                return errors;
            }
            
            return errors;                                
        }
    }
}
