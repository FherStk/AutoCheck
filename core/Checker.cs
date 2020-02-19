
namespace AutoCheck.Core{
    /// <summary>
    /// This class must be inherited in order to develop a custom checker.
    /// The checker is in charge of testing items using a connector, and the result will be always a list of errors. 
    /// </summary>       
    public abstract class Checker {      
        //I know... But in a near future, a common checker behaviour could be needed :p            
    }
}
