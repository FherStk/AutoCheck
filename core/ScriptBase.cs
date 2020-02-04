using System;
using System.IO;
using System.Linq;
using AutomatedAssignmentValidator.Helpers;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptBase<T> where T: Core.CopyDetectorBase, new(){
        protected Output Output {get; set;}
        protected string Path {get; set;}                          
        protected string Student {get; set;}
        protected float CopyThreshold {get; set;}

        public ScriptBase(string path, float copyThreshold=1f){
            this.Path = path;
            this.CopyThreshold = copyThreshold;
            this.Student = Utils.MoodleFolderToStudentName(path);
        }
        public virtual void Batch(){            
            if(!Directory.Exists(Path)) Output.WriteLine(string.Format("The provided path '{0}' does not exist.", Path), ConsoleColor.Red);   
            else{                            
                UnZip();
                T cd = CopyDetection();
                
                foreach(string f in Directory.EnumerateDirectories(Path))
                {
                    try{
                        if(cd.CopyDetected(f, CopyThreshold)){
                            Output.WriteLine(string.Format("Skipping script for the student ~{0}: ", Student), ConsoleColor.DarkYellow);                            
                            Output.Write("Potential copy detected!", ConsoleColor.DarkRed);
                            Output.Indent();

                            foreach(var item in cd.GetDetails(f)){
                                Terminal.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", item.file, Utils.MoodleFolderToStudentName(item.file)), ConsoleColor.Yellow);     
                                Terminal.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < CopyThreshold ? ConsoleColor.Green : ConsoleColor.Red));
                            }

                            Output.UnIndent();
                        }
                        else{
                            Output.WriteLine(string.Format("Running script for the student ~{0}: ", Student), ConsoleColor.DarkYellow);
                            Output.Indent();
                            Single();
                            Output.UnIndent();
                        }
                        
                    }
                    catch (Exception e){
                        Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                    }
                    finally{    
                        Output.UnIndent();                       
                    }
                }  
            } 
        }  
        public abstract void Single();
        protected void UnZip(){
            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.WriteLine(string.Format("Unzipping files for the student ~{0}: ", Student), ConsoleColor.DarkYellow);
                    Output.Indent();
                   
                    string zip = Directory.GetFiles(f, "*.zip", SearchOption.AllDirectories).FirstOrDefault();    
                    if(!string.IsNullOrEmpty(zip)){
                        Output.Write("Unzipping the zip file: ");

                        try{
                            Utils.ExtractZipFile(zip);
                            Output.WriteResponse();
                        }
                        catch(Exception e){
                            Output.WriteResponse(string.Format("ERROR {0}", e.Message));                           
                            continue;
                        }
                        
                        Output.Write("Removing the zip file: ");
                        try{
                            File.Delete(zip);
                            Output.WriteResponse();
                        }
                        catch(Exception e){
                            Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                            continue;
                        }                                                                    
                    }                    
                }
                catch (Exception e){
                    Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                }
                finally{    
                    Output.UnIndent();
                    Output.BreakLine();                 
                }
            }
        }
        protected T CopyDetection(){
            //TODO: maybe this could be a base class, called as a generic one... Within the inheriting script, stablish the type of Validator (can be Dummy or NULL)
            //      Also, Validator o PlainTextComparer can be renamed as CopyDetectors
            T cd = new T();            
            Output.WriteLine("Loading files for validation: ");
            Output.Indent();
            
            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.Write(string.Format("Loading files for the student ~{0}... ", Student), ConsoleColor.DarkYellow);                    
                    cd.LoadFile(f);    //TODO: must be empty on generic/base class
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                }                
            }
            Output.UnIndent();
            Output.BreakLine();

            Output.WriteLine("Validating files... ");
            try{               
                cd.Compare();      //TODO: must be empty on generic/base class     
                Output.WriteResponse();
            }
            catch (Exception e){
                Output.WriteResponse(string.Format("ERROR {0}", e.Message));
            } 
            
            return cd;
        }                           
    }
}