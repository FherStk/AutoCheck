using YamlDotNet.RepresentationModel;

namespace AutoCheck.Web.Models{
    public class WebScript : AutoCheck.Core.Script{
        //LoadYamlFile is protected, because make it public could be danegerous; also, the Script constructors executes the scripts, so inheriting will do the trick.
        
        private YamlStream YAML {get; set;}

        public WebScript(string path): base(){
            this.YAML = this.LoadYamlFile(path);  
        }

        public YamlStream InjectTarget(Dictionary<string, string> target, Dictionary<string, string> vars){                      
            var root = (YamlMappingNode)YAML.Documents[0].RootNode;                                    

            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                switch(name){                        
                    case "single":                           
                    case "batch":                        
                        ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                            switch(name){                        
                                case "local":                                         
                                case "remote":   
                                    ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //os; host; user; password; vars                                        
                                        switch(name){
                                            case "os":
                                                var os = Enum.Parse(typeof(AutoCheck.Core.Utils.OS), target[name]);
                                                ((YamlScalarNode)node).Value = os.ToString();
                                                break;
                                            
                                            case "vars":
                                                ForEachChild(node, new Action<string, YamlNode>((name, node) => {
                                                    ((YamlScalarNode)node).Value = vars[name];
                                                })); 
                                                break;
                                            
                                            default:
                                                //folder; path
                                                ((YamlScalarNode)node).Value = target[name];
                                                break;
                                        }                                        
                                    }));                     
                                    break;
                            }
                        }));
                        break;
                }
            }));

            return this.YAML;
        } 

        public Dictionary<string, object> GetTargetData(){
            var data = new Dictionary<string, object>();            
            var root = (YamlMappingNode)YAML.Documents[0].RootNode;                        
            
            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                switch(name){                        
                    case "single":                           
                    case "batch":                        
                        ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                            switch(name){                        
                                case "local":                                         
                                case "remote":   
                                    ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //os; host; user; password; folder; path
                                        if(name != "vars") data.Add(name, ((YamlScalarNode)node).Value ?? string.Empty);
                                        else{
                                            var vars = new Dictionary<string, object>();
                                            data.Add(name, vars);

                                            ForEachChild(node, new Action<string, YamlNode>((name, node) => {
                                                vars.Add(name, ((YamlScalarNode)node).Value ?? string.Empty);
                                            }));      
                                        }
                                    }));                     
                                    break;
                            }
                        }));
                        break;
                }
            }));

            return data;
        }     
    } 
}