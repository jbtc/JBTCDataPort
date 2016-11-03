using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Utilities
{
    public class DataBase
    {

        //TODO: generalize

        /// <summary>
        /// insert system status
        /// </summary>
        /// <param name="sysID"></param>
        /// <param name="sysStatus"></param>
        /// <returns></returns>
        internal bool InsertSystemStatus(int sysID, string  sysStatus)
        {
            string connectionString = ConfigurationManager.AppSettings["dbConnectionString"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("InsertSystemStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@sysID", sysID));
                    command.Parameters.Add(new SqlParameter("@systemStatus", sysStatus));
                    
                    command.CommandTimeout = 5;

                    if (command.ExecuteNonQuery() > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception)
                {
                    /*Handle error*/
                    return false;
                }
            }
        }



        /// <summary>
        /// insert Address
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="idLocation"></param>
        /// <param name="AddressStreet1"></param>
        /// <param name="AddressStreet2"></param>
        /// <param name="AddressStreet3"></param>
        /// <param name="AddressCity"></param>
        /// <param name="AddressState"></param>
        /// <param name="AddressZIP"></param>
        /// <param name="AddressCounty"></param>
        /// <param name="AddressLocalTownship"></param>
        /// <param name="AddressCountry"></param>
        /// <returns></returns>
        internal bool InsertAddress(string Description, long idLocation, 
                                    string AddressStreet1,string AddressStreet2, string AddressStreet3,
                                    string AddressCity, string AddressState, string AddressZIP,
                                    string AddressCounty, string AddressLocalTownship, string AddressCountry)
        {
            string connectionString = ConfigurationManager.AppSettings["dbConnectionString"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("InsertAddress", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Description", Description));
                    command.Parameters.Add(new SqlParameter("@idLocation", idLocation));
                    command.Parameters.Add(new SqlParameter("@AddressStreet1", AddressStreet1));
                    command.Parameters.Add(new SqlParameter("@AddressStreet2", AddressStreet2));
                    command.Parameters.Add(new SqlParameter("@AddressStreet3", AddressStreet3));
                    command.Parameters.Add(new SqlParameter("@AddressCity", AddressCity));
                    command.Parameters.Add(new SqlParameter("@AddressState", AddressState));
                    command.Parameters.Add(new SqlParameter("@AddressZIP", AddressZIP));
                    command.Parameters.Add(new SqlParameter("@AddressCounty", AddressCounty));
                    command.Parameters.Add(new SqlParameter("@AddressLocalTownschip", AddressLocalTownship));
                    command.Parameters.Add(new SqlParameter("@AddressCountry", AddressCountry));

                    command.CommandTimeout = 5;

                    if (command.ExecuteNonQuery() > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception)
                {
                    /*Handle error*/
                    return false;
                }
            }
        }

        

        /// <summary>
        /// insert person
        /// </summary>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <param name="Description"></param>
        /// <param name="idLocation"></param>
        /// <param name="Function"></param>
        /// <returns></returns>
        internal bool InsertPerson( string FirstName, string LastName,
                                    string Description, int idLocation,
                                    string Function)
        {
            string connectionString = ConfigurationManager.AppSettings["dbConnectionString"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("InsertSystemStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@FirstName", FirstName));
                    command.Parameters.Add(new SqlParameter("@LastName", LastName));
                    command.Parameters.Add(new SqlParameter("@Description", Description));
                    command.Parameters.Add(new SqlParameter("@idLocation", idLocation));
                    command.Parameters.Add(new SqlParameter("@Function", Function));

                    command.CommandTimeout = 5;

                    if (command.ExecuteNonQuery() > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception)
                {
                    /*Handle error*/
                    return false;
                }
            }
        }



        /// <summary>
        /// insert phone number
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="idPhoneType"></param>
        /// <param name="idPerson"></param>
        /// <param name="idLocation"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        internal bool InsertPhone(string Description, int idPhoneType, 
                                  int idPerson, int idLocation, 
                                  string phoneNumber)
        {
            string connectionString = ConfigurationManager.AppSettings["dbConnectionString"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("InsertPhone", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Description", Description));                    
                    command.Parameters.Add(new SqlParameter("@idPhoneType", idPhoneType));
                    command.Parameters.Add(new SqlParameter("@idPerson", idPerson));
                    command.Parameters.Add(new SqlParameter("@idLocation", idLocation));
                    command.Parameters.Add(new SqlParameter("@PhoneNumber", phoneNumber));

                    command.CommandTimeout = 5;

                    if (command.ExecuteNonQuery() > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception)
                {
                    /*Handle error*/
                    return false;
                }
            }
        }


    }
}
