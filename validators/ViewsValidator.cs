using System;
using Npgsql;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class ViewsValidator: ValidatorBaseDataBase{     
        public ViewsValidator(string server, string database): base(server, database){                        
        } 

        public override List<TestResult> Validate()
        {               
            Terminal.WriteLine(string.Format("Getting the permissions for the database ~{0}:", this.DataBase), ConsoleColor.Yellow);
            Terminal.Indent();
                                 
            using (this.Conn){
                this.Conn.Open();                                           
                ClearResults();
                
                //question 1
                CloseTest(CheckViewExists(), 2); 

                //question 2         
                CloseTest(CheckInsertRule(), 3); 
                
                //question 3                                          
               
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
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT MAX({0}) FROM {1}.{2};", field, schema, table), this.Conn)){                                
                return (int)cmd.ExecuteScalar();                   
            } 
        }
        private bool CompareViewData(int idEmpleat, string nomEmpleat, int idFabrica, string nomFabrica, int idProducte, string nomProducte){
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"   SELECT * FROM gerencia.report 
                                                                            WHERE id_producte={0} AND id_fabrica={2} AND id_resposable={3}", idProducte, idFabrica, idEmpleat), this.Conn)){                                
                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    bool found = false;                    

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
        private List<string> CheckViewExists(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the view ~gerencia.reports... ");
            using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT COUNT(*) FROM gerencia.report", this.Conn)){
                
                try{
                    //If not exists, an exception will be thrown                    
                    cmd.ExecuteScalar();                                                                
                }
                catch(Exception e){
                    errors.Add("The view does not exists.");
                    return;
                }
            }     
            
            //Let's gona insert some values in order to check the correctness of the view's query
            //NOTE: no transactions will be used, because a single user (this process) will be using the database.                        
            string nomEmpleat = "TEST EMPLOYEE 1";
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO rrhh.empleats (id, nom, cognoms, email, id_cap, id_departament)
                                                            VALUES ((SELECT MAX(id)+1 FROM rrhh.empleats), '{0}', 'NONE', 'NONE', 4, 4);", nomProducte), this.Conn)){                
                try{
                     cmd.ExecuteScalar();   
                }
                catch(Exception e){
                    errors.Add(e.Message);
                    return;
                }
            } 
            
            string nomFabrica = "TEST FACTORY 1";
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO produccio.fabriques (id, nom, pais, direccio, telefon, id_responsable)
                                                            VALUES ((SELECT MAX(id)+1 FROM produccio.fabriques), '{0}', 'SPAIN', 'NONE', 'NONE', (SELECT MAX(id) FROM rrhh.empleats));", nomFabrica), this.Conn)){                
                try{
                     cmd.ExecuteScalar();   
                }
                catch(Exception e){
                    errors.Add(e.Message);
                    return;
                }
            } 
           
            string nomProducte = "TEST PRODUCT 1";
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO produccio.productes (id, nom, codi, descripcio)
                                                            VALUES ((SELECT MAX(id)+1 FROM produccio.productes), '{0}', 'NONE', 'NONE');", nomFabrica), this.Conn)){                
                try{
                     cmd.ExecuteScalar();   
                }
                catch(Exception e){
                    errors.Add(e.Message);
                    return;
                }
            }             

            using (NpgsqlCommand cmd = new NpgsqlCommand(@" INSERT INTO produccio.fabricacio (id_fabrica, id_producte)
                                                            VALUES ((SELECT MAX(id)+1 FROM produccio.fabriques), (SELECT MAX(id)+1 FROM produccio.productes));", this.Conn)){                
                try{
                     cmd.ExecuteScalar();   
                }
                catch(Exception e){
                    errors.Add(e.Message);
                    return;
                }
            } 

            //Getting the new IDs and checking the view's data
            int idEmpleat;
            int idProducte;
            int idFabrica;

            try{
                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");

                 if(!CompareViewData(idEmpleat, nomEmpleat, idFabrica, nomFabrica, idProducte, nomProducte)) 
                    errors.Add("Unable to find some data into the report view.");
            }
            catch(Exception e){
                errors.Add(e.Message);
                return;
            }

            return errors;
        }  
        private List<string> CheckInsertRule(){    
            List<string> errors = new List<string>();                             

            OpenTest("Checking the rule ~INSERT ON gerencia.reports... ");
            string nomEmpleat = "TEST EMPLOYEE 2";
            string nomFabrica = "TEST FACTORY 2";
            string nomProducte = "TEST PRODUCT 2";
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" INSERT INTO gerencia.reports (nom_producte, nom_fabrica, nom_responsable)
                                                            VALUES ('{0}', '{1}', '{2}');", nomProducte, nomFabrica, nomEmpleat), this.Conn)){                
                try{
                     cmd.ExecuteScalar();   
                }
                catch(Exception e){
                    errors.Add(e.Message);
                    return;
                }
            }

            //First: checking the view's data
            int idEmpleat;
            int idProducte;
            int idFabrica;

            try{
                idEmpleat = GetLastID("rrhh", "empleats");
                idProducte = GetLastID("produccio", "productes");
                idFabrica = GetLastID("produccio", "fabriques");
                
                if(!CompareViewData(idEmpleat, nomEmpleat, idFabrica, nomFabrica, idProducte, nomProducte)) 
                    errors.Add("Unable to find some data into the report view.");
            }
            catch(Exception e){
                errors.Add(e.Message);
                return;
            }
            
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

                            if(dr["nom"].ToString().Equals(nomFabrica) && dr["pais"].ToString().ToUpper().Equals("SPAIN") && dr["direccio"].ToString().Equals("C. Anselm Rius 10. Santa Coloma de Gramenet") && dr["telefon"].ToString().Equals("+3493391000") && dr["id_resposable"].Equals(idEmpleat)){
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

                            if(dr["nom"].ToString().Equals(nomFabrica) && dr["codi"].ToString().ToUpper().Equals("NONE") && dr["descripcio"].ToString().Equals("NONE")){
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
                    int count = (int)cmd.ExecuteScalar();                    
                    if(count == 0) errors.Add("Unable to find any new production entry after inserting through the view.");                                        
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }                               
            }             

            return errors;
        }

                                     
    }
}