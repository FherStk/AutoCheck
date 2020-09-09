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

using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using AutoCheck.Core;
using AutoCheck.Exceptions;

namespace AutoCheck.Test.Checkers
{
    //TODO: Remove this file when migration from C# to YAML has been completed (this includes checker & connector mergind and also connector simplification).

    // [Parallelizable(ParallelScope.All)]    
    // public class Csv : Core.Test
    // {
    //     //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
    
    //     [Test]
    //     public void Constructor()
    //     {            
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Csv("", "someFile.ext"));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Csv("somePath", ""));
    //         Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Checkers.Csv("somePath", "someFile.ext"));
    //         Assert.Throws<FileNotFoundException>(() => new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "someFile.ext"));
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "empty.csv"));
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "correct1.csv", ';', '\''));
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "correct2.csv", ',', '"'));
    //         Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "incorrect.csv"));
    //     }   

    //     [Test]
    //     public void CheckIfRegistriesMatchesAmount()
    //     {    
    //         using(var csv = new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "correct1.csv"))
    //         {                               
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(1));
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(36634));
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(1, Operator.GREATER));
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(36634, Operator.GREATER));
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(36634, Operator.GREATEREQUALS));
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(36634, Operator.LOWER));
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesAmount(36634, Operator.LOWEREQUALS));
    //         }
    //     } 

    //     [Test]
    //     public void CheckIfRegistriesMatchesData()
    //     {    
    //         using(var csv = new AutoCheck.Checkers.Csv(this.SamplesScriptFolder, "correct2.csv"))
    //         {   
    //             //Non-strict                            
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){{"policyID", 119736}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", 498960}, {"hu_site_limit", 498960}, {"fl_site_limit", 498960}, {"fr_site_limit", 498960}, {"tiv_2011", 498960}, {"tiv_2012", "792148.9"}, {"eq_site_deductible", 0}, {"hu_site_deductible", "9979.2"}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "30.102261"}, {"point_longitude", "-81.711777"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}));
    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(csv.Connector.CsvDoc.Count, new Dictionary<string, object>(){{"policyID", 398149}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", "373488.3"}, {"hu_site_limit", "373488.3"}, {"fl_site_limit", 0}, {"fr_site_limit", 0}, {"tiv_2011", "373488.3"}, {"tiv_2012", "596003.67"}, {"eq_site_deductible", 0}, {"hu_site_deductible", 0}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "28.06444"}, {"point_longitude", "-82.77459"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}));

    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){{"policyID", 119736}, {"statecode", _FAKE}, {"county", "CLAY COUNTY"}, {"eq_site_limit", 498960}, {"hu_site_limit", 498960}, {"fl_site_limit", 498960}, {"fr_site_limit", 498960}, {"tiv_2011", 498960}, {"tiv_2012", "792148.9"}, {"eq_site_deductible", 0}, {"hu_site_deductible", "9979.2"}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "30.102261"}, {"point_longitude", "-81.711777"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}));
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){{"policyID", _FAKE}, {"statecode", _FAKE}, {"county", "CLAY COUNTY"}, {"eq_site_limit", 498960}, {"hu_site_limit", 498960}, {"fl_site_limit", 498960}, {"fr_site_limit", 498960}, {"tiv_2011", 498960}, {"tiv_2012", "792148.9"}, {"eq_site_deductible", 0}, {"hu_site_deductible", "9979.2"}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "30.102261"}, {"point_longitude", "-81.711777"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}));

    //             //Strict (fails due int (here) ->string (within the csv))
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){{"policyID", 119736}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", 498960}, {"hu_site_limit", 498960}, {"fl_site_limit", 498960}, {"fr_site_limit", 498960}, {"tiv_2011", 498960}, {"tiv_2012", "792148.9"}, {"eq_site_deductible", 0}, {"hu_site_deductible", "9979.2"}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "30.102261"}, {"point_longitude", "-81.711777"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}, true));
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(csv.Connector.CsvDoc.Count, new Dictionary<string, object>(){{"policyID", 398149}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", "373488.3"}, {"hu_site_limit", "373488.3"}, {"fl_site_limit", 0}, {"fr_site_limit", 0}, {"tiv_2011", "373488.3"}, {"tiv_2012", "596003.67"}, {"eq_site_deductible", 0}, {"hu_site_deductible", 0}, {"fl_site_deductible", 0}, {"fr_site_deductible", 0}, {"point_latitude", "28.06444"}, {"point_longitude", "-82.77459"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", 1}}, true));

    //             Assert.AreEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){{"policyID", "119736"}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", "498960"}, {"hu_site_limit", "498960"}, {"fl_site_limit", "498960"}, {"fr_site_limit", "498960"}, {"tiv_2011", "498960"}, {"tiv_2012", "792148.9"}, {"eq_site_deductible", "0"}, {"hu_site_deductible", "9979.2"}, {"fl_site_deductible", "0"}, {"fr_site_deductible", "0"}, {"point_latitude", "30.102261"}, {"point_longitude", "-81.711777"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", "1"}}, true));
    //             Assert.AreNotEqual(new List<string>(), csv.CheckIfRegistriesMatchesData(csv.Connector.CsvDoc.Count, new Dictionary<string, object>(){{"policyID", "398149"}, {"statecode", "FL"}, {"county", "CLAY COUNTY"}, {"eq_site_limit", "373488.3"}, {"hu_site_limit", "373488.3"}, {"fl_site_limit", "0"}, {"fr_site_limit", "0"}, {"tiv_2011", "373488.3"}, {"tiv_2012", "596003.67"}, {"eq_site_deductible", "0"}, {"hu_site_deductible", "0"}, {"fl_site_deductible", "0"}, {"fr_site_deductible", "0"}, {"point_latitude", "28.06444"}, {"point_longitude", "-82.77459"}, {"line", "Residential"}, {"construction", "Masonry"}, {"point_granularity", "1"}}, true));
    //         }
    //     }                       
    // }
}