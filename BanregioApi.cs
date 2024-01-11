﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{
    public class BanregioApi
    {
        DataTable _Cuentas, _Depositos, _EfectivoDesglose, _Cheques;
        List<Deposito> _Deposito = new List<Deposito>();
        List<Denominacion> _Denominacion = new List<Denominacion>();
        List<Cuenta> _ListaCuentas;
        List<DetalleCheque> _ListaCheques;
        SqlConnection _Cn;
        int TotalDepositos = 0;
        decimal TotalEfectivo = 0;
        decimal _importeTotalEfectivo, _importeTotalChequesPropios, _importeTotalChequesOtros;
        int _IdSesion, _IdCajaBancaria, _CorteTurno, _IdUsuario;
        string _NumeroCuenta, _FechaRecepcionProceso;
        DateTime _FechaAplicacion;
        public void SaveRepositoryFile(SqlConnection cn, DataTable Cuentas, DataTable Depositos, DataTable EfectivoDesglose, DataTable Cheques, int Id_Sesion, int Id_CajaBancaria, int Corte_Turno, DateTime Fecha_Aplicacion, int Id_Usuario)
        {
            _Cn = cn;
            _Cuentas = Cuentas;
            _Depositos = Depositos;
            _EfectivoDesglose = EfectivoDesglose;
            _Cheques = Cheques;
            _IdSesion = Id_Sesion;
            _IdCajaBancaria = Id_CajaBancaria;
            _CorteTurno = Corte_Turno;
            _IdUsuario = Id_Usuario;
            _FechaAplicacion = Fecha_Aplicacion;
            FileJson();
            foreach (var cuentas in _ListaCuentas)
            {
                _NumeroCuenta = cuentas.numeroCuenta;
                Guardar(cuentas);
            }
        }
        public Bank FileJson()
        {
            _ListaCuentas = new List<Cuenta>();
            for (int c = 0; c < _Cuentas.Rows.Count; c++)
            {
                if (_Cuentas.Rows[c][0].ToString().Length == 12)
                {
                    _ListaCuentas.Add(new Cuenta
                    {
                        depositos = ArmarDepositos(_Cuentas.Rows[c][0].ToString()),//Para que llene la variable numero de depositos
                        numeroCuenta = _Cuentas.Rows[c][0].ToString(),
                        instituto = "80001",
                        claveArchivo = _Cuentas.Rows[c][0].ToString(),
                        numeroDepositos = TotalDepositos,
                        importeTotalEfectivo = decimal.Round(Convert.ToDecimal(_Cuentas.Rows[c][1].ToString()), 2), //decimal.Round(Convert.ToDecimal(_Importes.Rows[i][5].ToString()), 2),
                        importeTotalChequesPropios = decimal.Round(Convert.ToDecimal(_Cuentas.Rows[c][2].ToString()), 2),//decimal.Round(Convert.ToDecimal(_Importes.Rows[i][6].ToString()), 2),
                        importeTotalChequesOtros = decimal.Round(Convert.ToDecimal(_Cuentas.Rows[c][3].ToString()), 2), //decimal.Round(Convert.ToDecimal(_Importes.Rows[i][7].ToString()), 2),
                        razonSocialCliente = _Cuentas.Rows[c][4].ToString()
                        
                    });
                }
            }

            DateTime formato_fecha = DateTime.Now;
            Bank Archivo = new Bank
            {
                fecha = formato_fecha.ToString("yyyy-MM-ddThh:mm:ss.000Z"),
                cuentas = _ListaCuentas,
            };
            return Archivo;
        }
        public List<Deposito> ArmarDepositos(string Cuenta)
        {
            TotalDepositos = 0;
            IEnumerable<DataRow> query = from Fichas in _Depositos.AsEnumerable()
                                         where Fichas.Field<string>("numeroCuenta") == Cuenta 
                                         select Fichas;
            DataTable result = query.CopyToDataTable<DataRow>();
            _Deposito = new List<Deposito>();
            TotalDepositos = result.Rows.Count;
            for (int i = 0; i < result.Rows.Count; i++)
            {
                //_FechaRecepcionProceso = result.Rows[i][12].ToString();
                _Deposito.Add(new Deposito
                {
                    consecutivo = i + 1,
                    divisa = result.Rows[i][3].ToString(),
                    //remesa = Convert.ToInt64(result.Rows[i][4].ToString().Substring(0, 5)),
                    remesa = Convert.ToInt64(result.Rows[i][4].ToString()),
                    referencia = result.Rows[i][5].ToString(),
                    importeTotal = decimal.Round(Convert.ToDecimal(result.Rows[i][6].ToString()), 2),
                    importeFicha = decimal.Round(Convert.ToDecimal(result.Rows[i][7].ToString()), 2),
                    diferencia = decimal.Round(Convert.ToDecimal(result.Rows[i][8].ToString()), 2),
                    tipoDiferencia = result.Rows[i][9].ToString(),
                    detalleEfectivo = result.Rows[i][10].ToString() == "S" ? ArmarEfectivoD(result.Rows[i]) : null, //EsEfectivo(Convert.ToDecimal( result.Rows[i][6].ToString()),Convert.ToDecimal(result.Rows[i][7].ToString()), Convert.ToDecimal(result.Rows[i][8].ToString())) ==true? ArmarEfectivoD(result.Rows[i]):null,
                    detalleCheque = result.Rows[i][11].ToString() == "S" ? ArmarCheques(Convert.ToInt64(result.Rows[i][2])) : new List<DetalleCheque>()
                });  
            }
            return _Deposito;
        }
        public DetalleEfectivo ArmarEfectivoD(DataRow fila)
        {
            var _EfectivoD = new DetalleEfectivo
            {

                fechaRecepcion = Convert.ToDateTime(fila[12]).ToString("yyyy-MM-ddThh:mm:ss.000Z"),
                //fechaRecepcion=Convert.ToDateTime( _FechaRecepcionProceso),
                desglose = ArmarDesglose(Convert.ToInt32(fila[2].ToString())),//Lo agregue al principio por que llena la variable TotalEfectivo
                importe = decimal.Round(TotalEfectivo, 2)

            };
            return _EfectivoD;

        }
        public List<DetalleCheque> ArmarCheques(long Id_Ficha)
        {
            IEnumerable<DataRow> query = from Ficha in _Cheques.AsEnumerable()
                                         where Ficha.Field<decimal>("Id_Ficha") == Id_Ficha
                                         select Ficha;
            DataTable result = query.CopyToDataTable<DataRow>();
            _ListaCheques = new List<DetalleCheque>();

            for (int i = 0; i < result.Rows.Count; i++)
            {
                _ListaCheques.Add(new DetalleCheque
                {

                    tipoCheque = result.Rows[i][1].ToString() == "S" ? "PROPIO" : "OTRO",
                    bandaMagnetica = BandaMagnetica(result.Rows[i][3].ToString()),
                    //numeroSeguridadInvisible= result.Rows[i][1].ToString() == "S" ? result.Rows[i][5].ToString() == "" ? "00000000" : result.Rows[i][5].ToString().Substring(0, 8) : null ,
                    numeroSeguridadInvisible = NumeroDeSeguridadInvisible(result.Rows[i]),
                    fechaRecepcion = Convert.ToDateTime(_FechaRecepcionProceso).ToString("yyyy-MM-ddThh:mm:ss.000Z"),
                    importe = decimal.Round(Convert.ToDecimal(result.Rows[i][4].ToString()), 2)

                });
            }
            return _ListaCheques;
        }
        string BandaMagnetica(string _Banda)
        {
            string NuevaBanda = "";
            foreach (char Caracter in _Banda)
            {
                if (char.IsNumber(Caracter))
                {
                    NuevaBanda += Caracter;
                }
            }
            return NuevaBanda;

        }
        string NumeroDeSeguridadInvisible(DataRow Fila)
        {
            if (Fila[1].ToString() == "S")
            {
                if (Fila[5].ToString() == "")
                {
                    return "00000000";
                }
                else if (Fila[5].ToString().Length == 8)
                {
                    return Fila[5].ToString();
                }
                else
                {
                    return "00000000";
                }
            }
            else
            {
                return null;
            }
        }
        public List<Denominacion> ArmarDesglose(long Id_Ficha)
        {
            TotalEfectivo = 0;
            IEnumerable<DataRow> query = from Denominacion in _EfectivoDesglose.AsEnumerable()
                                         where Denominacion.Field<decimal>("Id_Ficha") == Id_Ficha
                                         select Denominacion;
            DataTable result = query.CopyToDataTable<DataRow>();
            _Denominacion = new List<Denominacion>();
            for (int i = 0; i < result.Rows.Count; i++)
            {
                _Denominacion.Add(new Denominacion
                {
                    tipo = result.Rows[i][4].ToString() == "B" ? "BILLETE" : "MONEDA",
                    denominacion = decimal.Round(Convert.ToDecimal(result.Rows[i][1].ToString()), 2),
                    cantidad = Convert.ToInt32(result.Rows[i][2].ToString()),
                    importe = decimal.Round(Convert.ToDecimal(result.Rows[i][3].ToString()), 2)
                }
               );
                TotalEfectivo += decimal.Round(Convert.ToDecimal(result.Rows[i][3].ToString()), 2);
            }
            return _Denominacion;
        }
        public void Guardar(Cuenta _Cuenta)
        {
            DataTable tbl;
            SqlCommand cmd = null;
            try
            {
                //SqlConnection cnn = Conexion.creaConexion("");
                cmd = Conexion.creaComando("Sprocedure_Insert_ArchivosBanregio", _Cn);
                Conexion.creaParametro(cmd, "@Id_Sesion", SqlDbType.Int, _IdSesion);
                Conexion.creaParametro(cmd, "@Id_CajaBancaria", SqlDbType.Int, _IdCajaBancaria);
                Conexion.creaParametro(cmd, "@Corte_Turno", SqlDbType.Int, _CorteTurno);
                Conexion.creaParametro(cmd, "@Numero_Cuenta", SqlDbType.VarChar, _NumeroCuenta);
                Conexion.creaParametro(cmd, "@Fecha_Aplicacion", SqlDbType.DateTime, _FechaAplicacion);
                Conexion.creaParametro(cmd, "@Id_Usuario", SqlDbType.Int, _IdUsuario);
                Conexion.creaParametro(cmd, "@Total_Efectivo", SqlDbType.Decimal, _Cuenta.importeTotalEfectivo);
                Conexion.creaParametro(cmd, "@Total_Cheques", SqlDbType.Decimal, (_Cuenta.importeTotalChequesPropios+_Cuenta.importeTotalChequesOtros));
                //Conexion.creaParametro(cmd, "@Archivo", SqlDbType.NVarChar, JsonConvert.SerializeObject(_Cuenta));
                tbl = Conexion.ejecutaConsulta(cmd);
                _Cuenta.claveArchivo = tbl.Rows[0][1].ToString();
                cmd = Conexion.creaComando("Sprocedure_Insert_JsonBanregio", _Cn);
                Conexion.creaParametro(cmd, "@Id_Archivo", SqlDbType.Int, tbl.Rows[0][0].ToString());
                Conexion.creaParametro(cmd, "@Json", SqlDbType.NVarChar, JsonConvert.SerializeObject(_Cuenta));
                Conexion.ejecutarNonquery(cmd);
                //cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioCuentas", _Cn);
                //Conexion.creaParametro(cmd, "@numeroCuenta", SqlDbType.VarChar,_Cuenta.numeroCuenta);
                //Conexion.creaParametro(cmd, "@instituto", SqlDbType.VarChar, _Cuenta.instituto);
                //Conexion.creaParametro(cmd, "@claveArchivo", SqlDbType.VarChar, _Cuenta.claveArchivo);
                //Conexion.creaParametro(cmd, "@numeroDepositos", SqlDbType.Int,_Cuenta.numeroDepositos);
                //Conexion.creaParametro(cmd, "@importeTotalEfectivo", SqlDbType.Decimal,decimal.Round( _Cuenta.importeTotalEfectivo,2));
                //Conexion.creaParametro(cmd, "@importeTotalChequesPropios", SqlDbType.Decimal, _Cuenta.importeTotalChequesPropios);
                //Conexion.creaParametro(cmd, "@importeTotalChequesOtros", SqlDbType.Decimal, _Cuenta.importeTotalChequesOtros);
                //Conexion.creaParametro(cmd, "@razonSocialCliente", SqlDbType.VarChar, _Cuenta.razonSocialCliente);
                //Conexion.ejecutaScalar(cmd);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public  Cuenta Detalle_Efectivo(string Json)
        {
         return   JsonConvert.DeserializeObject<Cuenta>(Json);
        }
    }
}
