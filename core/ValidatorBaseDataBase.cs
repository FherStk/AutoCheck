using Npgsql;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBaseDataBase : ValidatorBase{
        public string Server {get; private set;}
        public string DataBase {get; private set;}
        public NpgsqlConnection Conn {get; private set;}

        protected ValidatorBaseDataBase(string server, string database): base(){
            this.Server = server;
            this.DataBase = database;
            this.Conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database));
        }         
    }
}