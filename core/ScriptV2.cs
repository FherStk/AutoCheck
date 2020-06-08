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

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using AutoCheck.Exceptions;


namespace AutoCheck.Core{
    //TODO: This will be the new Script (without V2)
    public class ScriptV2{
        public string Name {get; private set;}
        public Dictionary<string, object> Vars {get; private set;}

        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){
            if(!File.Exists(path)) throw new FileNotFoundException(path);      
            Vars = new Dictionary<string, object>();

            var yaml = new YamlStream();
            yaml.Load(new StringReader(File.ReadAllText(path)));
            
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var entry in mapping.Children)
            {
                switch(entry.Key.ToString().ToLower()){
                case "name":
                    Name = entry.Value.ToString();
                    break;

                case "vars":
                    ParseVars((YamlMappingNode)mapping.Children[new YamlScalarNode("vars")]);
                    break;
                }
            }

            //Defaults
            if(string.IsNullOrEmpty(Name)) Name = Regex.Replace(Path.GetFileNameWithoutExtension(path).Replace("_", " "), "[A-Z]", " $0");

        }

        private void ParseVars(YamlMappingNode root){
            foreach (var item in root.Children){
                var name = item.Key.ToString();
                var value = item.Value.ToString();

                foreach(Match match in Regex.Matches(value, "{(.*?)}")){
                    var replace = match.Value.TrimStart('{').TrimEnd('}');
                    
                    if(replace.StartsWith("#") || replace.StartsWith("$")){                        
                        //Check if the regex is valid and/or also the referred var exists.
                        var regex = string.Empty;
                        if(replace.StartsWith("#")){
                            regex = replace.Substring(1, replace.LastIndexOf("$")-1);
                            replace = replace.Substring(replace.LastIndexOf("$"));
                        }

                        replace = replace.TrimStart('$');
                        if(!Vars.ContainsKey(replace.ToLower())) throw new InvalidDataException($"Unable to apply a regular expression ober the undefined variable {replace} as requested within the variable '{name}'.");                            
                        if(string.IsNullOrEmpty(regex)) replace = Vars[replace.ToLower()].ToString();
                        else {
                            try{
                                replace = Regex.Match(replace, regex).Value;
                            }
                            catch{
                                throw new InvalidDataException($"Invalid regular expression defined inside the variable '{name}'.");
                            }
                        }
                    }
                    
                    value = value.Replace(match.Value, replace);
                }
                
                if(Vars.ContainsKey(name)) throw new InvalidDataException($"Repeated variables defined with name '{name}'.");
                else Vars.Add(name, value);
            }
        }
        
        /// <summary>
        /// Validates a yaml script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        /// <returns>True if is valid, otherwise throws an exception.</returns>
        public static bool Validate(string path){
            var s = new ScriptV2(path);
            return true;
        }
    }
}