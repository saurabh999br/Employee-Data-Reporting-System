using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using PF_TRANSFER_IN__REGISTER_FORMAT.Models;

namespace PF_TRANSFER_IN__REGISTER_FORMAT.Data
{
    public class PFTransferRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PFTransferRepository> _logger;

        public PFTransferRepository(IConfiguration configuration, ILogger<PFTransferRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");

            _logger = logger;
        }

        public List<PFSettlementModel> GetPFTransferData()
        {
            List<PFSettlementModel> data = new List<PFSettlementModel>();

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM pf_transferin_register";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        con.Open();

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new PFSettlementModel
                                {
                                    SrNo = reader["srno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["srno"]),
                                    EmpNo = reader["empno"]?.ToString() ?? "",
                                    PF_Number = reader["pf_number"]?.ToString() ?? "",
                                    Date_Of_Transfer_In = reader["date_of_transferin"] == DBNull.Value
                                        ? DateTime.MinValue
                                        : Convert.ToDateTime(reader["date_of_transferin"]),

                                    TRNS_Type = reader["trns_type"]?.ToString() ?? "",

                                    Date_Of_Joining_Prior = reader["date_of_joining_prior"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(reader["date_of_joining_prior"]),

                                    Name_Of_Member = reader["name_of_member"]?.ToString() ?? "",
                                    Company_Name = reader["company_name"]?.ToString() ?? "",
                                    Trust_RPFC_Address = reader["trust_rpfc_address"]?.ToString() ?? "",
                                    From_PF_Account = reader["from_pf_account"]?.ToString() ?? "",
                                    To_PF_Account = reader["to_pf_account"]?.ToString() ?? "",

                                    Employee_Contb_Amount = reader["employee_contb_amount"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(reader["employee_contb_amount"]),

                                    Employer_Contb_Amount = reader["employer_contb_amount"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(reader["employer_contb_amount"]),

                                    Total_Contb_Amount =
                                        (reader["employee_contb_amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["employee_contb_amount"])) +
                                        (reader["employer_contb_amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["employer_contb_amount"])),

                                    Status = reader["status"]?.ToString() ?? "",
                                    FI_Document_Number = reader["fi_document_number"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching PF Transfer data");
            }

            return data;
        }
    }
}