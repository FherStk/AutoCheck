/*
    Copyright © 2023 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using Google.DiffMatchPatch;

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// Copy detector for DMOJ contests.
    /// </summary>
    public class Dmoj: PlainText{                
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>           
        /// <param name="url">URL to the DMOJ's instance.</param>
        /// <param name="threshold">Above this comparisson percentage, it will be assumed as a pontential copy.</param>
        /// <param name="contestCode">DMOJ's contest code to check.</param>
        public Dmoj(string url, float threshold, string contestCode): base(threshold, contestCode)
        {                 
            this.SentenceMatchWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;    
        }  

        /// <summary>
        /// Loads the given file into the local collection, in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="folder">Path where the files will be looked for.</param>                       
        /// <param name="file">File that will be loaded into the copy detector.</param>
        public override void Load(string folder, string file){   
            //NOTA: en función del directorio de batch y del file puesto en el script, se invoca a este método con el folder y el file que se ha encontrado por cada iteración.
            //      en este caso eso no es necesario, porque se trata de cargar desde un servicio. 
            //      habria que definir el destino del batch como un "service" que habría que inventárselo. En ese service habría que definir lo necesario para la conexión, en este caso al DMOJ.
            //      se podria lanzar excepción si se intenta hacer cualquier cosa dentro de un "service" que no sea el copy_detector
            //      en un futuro podría interesar conectarse a un servicio remoto, descargar todo lo descargable de allí en local y empezar a lanzar un batch (como Moodle por ejemplo).
            
            //ALT:  como alternativa, se puede usar el INIT para descargar todas las entregas del DMOJ de un concurso.
            //      luego, basta con pasarle el anticopia de código fuente a esas carpetas generadas con el modo batch tradicional.
            //      esta opción parece más sencilla y factible.

            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");            
            if(Index.ContainsKey(folder)) throw new ArgumentInvalidException("Two compared files cannot share the same folder because this folder must be used as an unique key.");   //Because files from different folders (students) are compared, and the folder will be de unique key to distinguish between sources.

            Index.Add(folder, Files.Count);
            Files.Add(new File(folder, file));                        
        }

        // /// <summary>
        // /// Compares all the previously loaded files, between each other.
        // /// </summary>
        // public override void Compare(){  
        //     if(WordCountWeight + LineCountWeight + SentenceMatchWeight != 1f)
        //         throw new Exception("The summary of all the weights must be 100%, set the correct values and try again.");
            
        //     //Compute the changes and store the result in a matrix
        //     DiffMatchPatch dmp = new DiffMatchPatch();
        //     dmp.DiffTimeout = 0;

        //     Matches = new float[Files.Count(), Files.Count()];                
        //     Diffs = new List<Diff>[Files.Count(), Files.Count()];
        //     for(int i=0; i < Files.Count(); i++){
        //         File left = Files[i];

        //         for(int j=i; j < Files.Count(); j++){                                                                                            
        //             File right = Files[j];
                                        
        //             List<Diff> diff = dmp.DiffMain(left.ToString(), right.ToString());
        //             if(i == j) Matches[i,j] = 1;    //Optimization
        //             else{
        //                 float diffAmount = (float)diff.Where(x => x.Operation == Operation.EQUAL).Count() / diff.Count;
        //                 float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));                    
        //                 float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));
        //                 Matches[i,j] = (float)(diffWordCount * WordCountWeight) + (diffLineCount * LineCountWeight) + (diffAmount * SentenceMatchWeight);  
        //             }

        //             //This should be always added                                        
        //             Diffs[i,j] = diff;          
        //         } 
        //     }

        //     //Copy the results that has been already computed
        //     for(int i=0; i < Files.Count(); i++){
        //         for(int j=i+1; j < Files.Count(); j++){
        //             Matches[j,i] = Matches[i,j];
        //             Diffs[j,i] = Diffs[i,j];
        //         }
        //     }
        // }   
    }
}