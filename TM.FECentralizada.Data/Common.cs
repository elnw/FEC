using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Tools;
using System.Data;
using System.Data.SqlClient;

namespace TM.FECentralizada.Data
{
    public static class Common
    {

        public static List<Entities.Common.Parameters> GetParametersByKey(Entities.Common.Parameters oParametersRequest)
        {
            List<Entities.Common.Parameters> oListParameter = new List<Entities.Common.Parameters>();
            try
            {
                using (SqlConnection conn = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("Sp_Obtener_Parametros", conn))
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@pi_dominio", SqlDbType.VarChar) { Value = oParametersRequest.Domain , Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@pi_key_dominio", SqlDbType.VarChar ) { Value = oParametersRequest.KeyDomain , Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@pi_key_param", SqlDbType.VarChar ) { Value = oParametersRequest.KeyParam , Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar ) { Direction = ParameterDirection.Output , Size = 3000 });
                        cmd.Parameters.Add(new SqlParameter("@po_codigo_respuesta", SqlDbType.VarChar ) {  Direction = ParameterDirection.Output, Size = 50});
                      
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                oListParameter.Add
                                (
                                    new Entities.Common.Parameters()
                                    {
                                        Id = dr["Id"].ToString(),
                                        Domain = dr["Dominio"].ToString(),
                                        KeyDomain = dr["Key_Dominio"].ToString(),
                                        KeyParam = dr["Key_Param"].ToString(),
                                        Value = dr["Valor"].ToString(),
                                        ValueJson = dr["Valor_Json"].ToString(),
                                        ValueAdd = dr["Valor_Add"].ToString(),
                                        Description = dr["Descripcion"].ToString(),
                                        State = dr["Estado"].ToString(),
                                        RegistrationUser = dr["Usuario_Registro"].ToString(),
                                        CreateDate = dr["Fecha_Registro"].ToString(),
                                        ModifyUser = dr["Usuario_Modificacion"].ToString(),
                                        ModifyDate = dr["Fecha_Modificacion"].ToString()
                                    }
                                );
                            }
                        }
                       string CodeResponse = Convert.ToString(cmd.Parameters["@po_codigo_respuesta"].Value);
                       string MessageResponse = Convert.ToString(cmd.Parameters["@po_mensaje_respuesta"].Value);

                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Codigo = {0} , Mensaje = {1} ] ", CodeResponse, MessageResponse));
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return oListParameter;
        }

        public static int InsertAudit(string idGrupo,  int legado, int estado, int cantidadRegistros, int intentos, int procesoSunat)
        {
            string MessageResponse;
            string CodeResponse;
            try
            {
                using (SqlConnection conn = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("Sp_Insertar_Auditoria ", conn))
                    {
                        conn.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@IdGrupo", SqlDbType.VarChar) { Value = idGrupo, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@Legado", SqlDbType.Int) { Value = legado, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@IdEstado", SqlDbType.Int) { Value = estado, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@CantidadRegistros", SqlDbType.Int) { Value = cantidadRegistros, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@Intentos", SqlDbType.Int) { Value = intentos, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@ProcesoSunat", SqlDbType.Int) { Value = procesoSunat, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@AuditoriaId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });
                        cmd.Parameters.Add(new SqlParameter("@po_codigo_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 50 });

                        cmd.ExecuteNonQuery();

                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();
                        CodeResponse = cmd.Parameters["@po_codigo_respuesta"].Value.ToString();

                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Codigo = {0} , Mensaje = {1} ] ", CodeResponse, MessageResponse));
                        return Convert.ToInt32(cmd.Parameters["@AuditoriaId"].Value);
                    }
                        

                }
            }
            catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
                return 0;
            }
        }

        public static void UpdateAudit(int auditoriaId, int estadoid, int intentos)
        {
            string MessageResponse;
            string CodeResponse;
            try
            {
                using (SqlConnection conn = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("Sp_Actualizar_Auditoria ", conn))
                    {
                        conn.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@id_auditoria", SqlDbType.Int) { Value = auditoriaId, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@id_estado", SqlDbType.Int) { Value = estadoid, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@intentos", SqlDbType.Int) { Value = intentos, Direction = ParameterDirection.Input });

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });
                        cmd.Parameters.Add(new SqlParameter("@po_codigo_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 50 });

                        cmd.ExecuteNonQuery();

                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();
                        CodeResponse = cmd.Parameters["@po_codigo_respuesta"].Value.ToString();

                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Codigo = {0} , Mensaje = {1} ] ", CodeResponse, MessageResponse));


                    }
                }
            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void BulkInsertListToTable<T>(List<T> list, string tableName)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlBulkCopy objbulk = new SqlBulkCopy(connection))
                    {

                        //assign Destination table name  
                        objbulk.DestinationTableName = tableName;

                        //int i = 0;
                        foreach (var property in typeof(T).GetMembers().Where(x => x.MemberType == System.Reflection.MemberTypes.Property).ToList())
                        {
                            //if (i == 27) break;
                            objbulk.ColumnMappings.Add(property.Name, property.Name);
                           // i++;
                            
                        }

                        connection.Open();
                        //insert bulk Records into DataBase.  
                        objbulk.WriteToServer(Tools.Common.ConvertToDataTable(list));
                    }
                }
            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            
        }

        public static void UpdateDocumentInvoice(string alignet, string sendDate, string series)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using(SqlCommand cmd = new SqlCommand("update factura_cabecera set nombreArchivoAlignet = @p1, fechaEnvio = @p2 where serieNumero in (@p3);", connection))
                    {
                        connection.Open();
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@p1", alignet);
                        cmd.Parameters.AddWithValue("@p2", sendDate);
                        cmd.Parameters.AddWithValue("@p3", series);

                        cmd.ExecuteNonQuery();

                    }
                }
            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateDocumentCreditNote(string alignet, string sendDate, string series)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("update Nota_Credito_Cabecera set nombreArchivoAlignet = @p1, fechaEnvio = @p2 where serieNumero in (@p3);", connection))
                    {
                        connection.Open();
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@p1", alignet);
                        cmd.Parameters.AddWithValue("@p2", sendDate);
                        cmd.Parameters.AddWithValue("@p3", series);

                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateDocumentDebitNote(string alignet, string sendDate, string series)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("update Nota_Debito_Cabecera set nombreArchivoAlignet = @p1, fechaEnvio = @p2 where serieNumero in (@p3);", connection))
                    {
                        connection.Open();
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@p1", alignet);
                        cmd.Parameters.AddWithValue("@p2", sendDate);
                        cmd.Parameters.AddWithValue("@p3", series);

                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateInvoiceState()
        {
            string MessageResponse;
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_Actualizar_Estado_Facturas", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });

                        cmd.ExecuteNonQuery();

                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();


                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Mensaje = {0} ] ", MessageResponse));


                    }
                }
            }
            catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateCreditNoteState()
 {
            string MessageResponse;
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    connection.Open();
                  
                    using (SqlCommand cmd = new SqlCommand("Sp_Actualizar_Estado_NotasCredito", connection))

                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });

                        cmd.ExecuteNonQuery();

                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();


                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Mensaje = {0} ] ", MessageResponse));


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateDebitNoteState()
        {
            string MessageResponse;
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("Sp_Actualizar_Estado_NotasDebito", connection))

                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });

                        cmd.ExecuteNonQuery();

                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();


                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Mensaje = {0} ] ", MessageResponse));


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateBillState()
 {
            string MessageResponse;
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("Sp_Actualizar_Estado_Boletas", connection))

                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@po_mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });

                        cmd.ExecuteNonQuery();


                        MessageResponse = cmd.Parameters["@po_mensaje_respuesta"].Value.ToString();


                        Tools.Logging.Info(string.Format("GetParametersByKey > Respuesta : [ Mensaje = {0} ] ", MessageResponse));


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

    }
}
