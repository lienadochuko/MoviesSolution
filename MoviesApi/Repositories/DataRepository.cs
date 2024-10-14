using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApi.Repositories.DataAccess
{
	public interface IDataRepository
	{
		Task<IEnumerable<T>> DynamicRetrival<T>(string procedureName, string ConnectionString, SqlParameter[] sqlParameter, Func<SqlDataReader, T> mapFunction, CancellationToken cancellationToken = default);
       // Task<ActorsDto> GetSearchedActors(string id, string procedureName, string connectionString,CancellationToken cancellationToken = default);
        //Task<bool> UpdateActorFieldsAsync(string actorId, string firstName, string familyName, DateTime? dob, DateTime? dod, string gender,
         // string procedureName, string connectionString, CancellationToken cancellationToken = default);
        //Task<bool> DeleteActorAsync(string actorId, string procedureName, string connectionString, CancellationToken cancellationToken = default);

    }

    public class DataRepository : IDataRepository
    {
        public async Task<IEnumerable<T>> DynamicRetrival<T>(string procedureName, string connectionString, SqlParameter[] sqlParameter, Func<SqlDataReader, T> mapFunction, CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new()
            {
                CommandText = procedureName,
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300
            };

            if (sqlParameter != null)
            {
                command.Parameters.AddRange(sqlParameter);
            }

            await connection.OpenAsync(cancellationToken);

            List<T> result = new List<T>();

            await using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    T record = mapFunction(reader);
                    result.Add(record);
                }
            }

            return result;
        }        
    }
}
