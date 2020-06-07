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

        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){
            if(!File.Exists(path)) throw new FileNotFoundException(path);      

            var yaml = new YamlStream();
            yaml.Load(new StringReader(File.ReadAllText(path)));

            var vars = new Dictionary<string, object>();
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var entry in mapping.Children)
            {
                switch(entry.Key.ToString().ToLower()){
                case "name":
                    Name = entry.Value.ToString();
                    break;

                case "vars":
                    var items = (YamlMappingNode)mapping.Children[new YamlScalarNode("vars")];
                    foreach (var item in items.Children){
                        var name = item.Key.ToString();
                        var value = item.Value.ToString();
                        var tmp = string.Empty;

                        foreach(Match match in Regex.Matches(value, "^{(.*?)}$")){
                            tmp = match.Value.TrimStart('{').TrimEnd('}');
                            
                            if(tmp.StartsWith("^") || tmp.StartsWith("$")){                        
                                //Check if the regex is valid and/or also the referred var exists.
                                if(tmp.StartsWith("^")){
                                    var regex = tmp.Substring(1, tmp.LastIndexOf("$"));
                                    tmp = tmp.Substring(tmp.LastIndexOf("$")+1);

                                    if(!vars.ContainsKey(tmp)) throw new InvalidDataException($"Unable to apply a regular expression ober the undefined variable {tmp} as requested within the variable '{name}'.");                            
                                    try{
                                        tmp = Regex.Replace(tmp, regex, "$0");
                                    }
                                    catch{
                                        throw new InvalidDataException($"Invalid regular expression defined inside the variable '{name}'.");
                                    }
                                }
                            }
                            
                            value += tmp;                                                
                        }

                        if(vars.ContainsKey(name)) throw new InvalidDataException($"Repeated variables defined with name '{name}'.");
                        else vars.Add(name, value);
                    }

                    

                    break;
                }
            }

            //Defaults
            if(string.IsNullOrEmpty(Name)) Name = Regex.Replace(Path.GetFileNameWithoutExtension(path).Replace("_", " "), "[A-Z]", " $0");

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