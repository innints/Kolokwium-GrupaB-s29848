

using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Exceptions;

namespace WebApplication1.Services;


public interface IDbService
{
    public Task<IEnumerable<PolitykWithPartieGetDto>> GetPolitycyWithPartieAsync();
    public Task<PartiaDetailsGetDto> CreatePartiaWithPolitycyAsync(PartiaWithPolitycyCreateDto studentData);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
     public async Task<IEnumerable<PolitykWithPartieGetDto>> GetPolitycyWithPartieAsync()
    {
        var politykDict = new Dictionary<int, PolitykWithPartieGetDto>();
       
        
        await using var connection = new SqlConnection(_connectionString);
        
        
        const string sql = """
                           select p.ID, p.Imie, p.Nazwisko,p.Powiedzenie, pa.Nazwa,pa.Skrot, pa.DataZalozenia, pz.Od,pz.Do 
                           from Polityk p
                           left join Przynaleznosc pz on p.ID=p.Polityk_ID
                           left join Partia pa on pz.Partia_ID = pa.ID;
                           """;
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var politykId = reader.GetInt32(0);

            if (!politykDict.TryGetValue(politykId, out var politykDetails))
            {
                politykDetails = new PolitykWithPartieGetDto
                {
                    ID = politykId,
                    Imie = reader.GetString(1),
                    Nazwisko = reader.GetString(2),
                    Powiedzenie = reader.GetString(3),
                    Przynaleznosc = []
                };

                politykDict.Add(politykId, politykDetails);
            }

            if (!await reader.IsDBNullAsync(4)) //sprawdza czy ma jakis kraj
            {


                politykDetails.Przynaleznosc.Add(new PolitykPartieGetDto
                {
                    Nazwa = reader.GetString(4),
                    Skrot = reader.GetString(5),
                    DataZalozenia = reader.GetDateTime(6),
                    Od=reader.GetDateTime(7),
                    Do=reader.GetDateTime(8),
                });
            }
        }


        return politykDict.Values; 
        }

        public async Task<PartiaDetailsGetDto> CreatePartiaWithPolitycyAsync(PartiaWithPolitycyCreateDto studentData)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var groups = new List<int>();
        
            if (studentData.Czlonkowie is not null && studentData.Czlonkowie.Count != 0)
            {
                foreach (var group in studentData.Czlonkowie)
                {
                    var groupCheckSql = """
                                        select Polityk
                                        from Partia 
                                        where Id = @Id;
                                        """;

                    await using var groupCheckCommand = new SqlCommand(groupCheckSql, connection);
                    groupCheckCommand.Parameters.AddWithValue("@Id", group);
                    await using var groupCheckReader = await groupCheckCommand.ExecuteReaderAsync();

                    if (!await groupCheckReader.ReadAsync())
                    {
                        throw new NotFoundException($"Polityk with id {group} does not exist");
                    }

                    groups.Add(
                    
                        groupCheckReader.GetInt32(0)
                     );
                }
            }
             await using var transaction = await connection.BeginTransactionAsync();

        try
        {

            var createPartiaSql = """
                                   insert into Partia
                                   output inserted.ID
                                   values (@Nazwa, @Skrot, @DataZalozenia);
                                   """;

            await using var createPartiaCommand =
                new SqlCommand(createPartiaSql, connection, (SqlTransaction)transaction);
            createPartiaCommand.Parameters.AddWithValue("@Nazwa", studentData.Nazwa);
            createPartiaCommand.Parameters.AddWithValue("@Skrot", studentData.Skrot);
            createPartiaCommand.Parameters.AddWithValue("@DataZalozenia", studentData.DataZalozenia);

            var createdPartiaId = Convert.ToInt32(await createPartiaCommand.ExecuteScalarAsync());

            foreach (var group in groups)
            {
                var partiaAssignmentSql = """
                                         insert into Przynaleznosc
                                         values (@PartiaId,@PolitykId,@Od,null);
                                         """;
                await using var groupAssignmentCommand =
                    new SqlCommand(partiaAssignmentSql, connection, (SqlTransaction)transaction);
                groupAssignmentCommand.Parameters.AddWithValue("@PartiaId", createdPartiaId);
                groupAssignmentCommand.Parameters.AddWithValue("@PolitykId", group);
                groupAssignmentCommand.Parameters.AddWithValue("@Od", studentData.DataZalozenia);
                
                

                await groupAssignmentCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            return new PartiaDetailsGetDto()
            {
                ID = createdPartiaId,
                Nazwa = studentData.Nazwa,
                Skrot = studentData.Skrot,
                DataZalozenia = studentData.DataZalozenia,
                //Politycy = groups
            };

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
            

        }
}