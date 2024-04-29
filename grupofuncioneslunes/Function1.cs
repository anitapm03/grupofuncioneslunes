using System.Data.SqlClient;
using System.Net;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace grupofuncioneslunes
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            //vamos a pedir un parametro 
            var query = System.Web.HttpUtility.ParseQueryString
                (req.Url.Query);
            string idempleado = query["idempleado"];

            if (idempleado == null)
            {
                var responsebad = req.CreateResponse
                    (HttpStatusCode.BadRequest);
                responsebad.WriteString("Debe decir un id");
                return responsebad;
            }

            _logger.LogInformation("Empleado " + idempleado);

            //cadena de conexion
            string connectionString = @"Data Source=sqlanapereira.database.windows.net;Initial Catalog=AZURETAJAMAR;Persist Security Info=True;User ID=adminsql;Password=Admin123";
            SqlConnection cn = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            string sqlUpdate = "UPDATE EMP SET SALARIO = SALARIO + 1 "
                + "WHERE EMP_NO = " + idempleado;
            com.Connection = cn;
            com.CommandType = System.Data.CommandType.Text;
            com.CommandText = sqlUpdate;
            cn.Open();
            com.ExecuteNonQuery();
            string sqlselect = "SELECT * FROM EMP WHERE EMP_NO =" + idempleado;
            com.CommandText = sqlselect;
            SqlDataReader reader = com.ExecuteReader();
            string mensaje = "";
            if(reader.Read())
            {
                mensaje = "El empleado " + reader["APELLIDO"].ToString() +
                    " con oficio " + reader["OFICIO"].ToString() +
                    " ha incrementado su salario a " + reader["SALARIO"].ToString();

                reader.Close();
            }
            else
            {
                mensaje = "No existe el empleado";
            }
            cn.Close();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(mensaje);

            return response;
        }
    }
}
