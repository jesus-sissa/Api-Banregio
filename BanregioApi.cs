using Newtonsoft.Json;
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
        public List<string> message = new List<string>();
        public List<string> ErrorTrack = new List<string>();
        public List<Tuple<string, string, Deposito>> TrackDepositos = new List<Tuple<string, string, Deposito>>();
        public List<Denominacion> Trackdesglose = new List<Denominacion>();
        //public string message = null;
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
                    fechaRecepcion = Convert.ToDateTime(result.Rows[0][6]).ToString("yyyy-MM-ddThh:mm:ss.000Z"),
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
                return "";
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
            string _numerocuentamsg = null, idcuentamsg = null, _remisionmsg = null, iddepositomsg = null, iddetallemsg = null;
            int _ttDepositos, _cDepositos = 0, _ttDesgloses = 0, _cDesgloses = 0, _ttCheques = 0, _cCheques = 0;

            _numerocuentamsg = _Cuenta.numeroCuenta.ToString();
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
                Conexion.creaParametro(cmd, "@Total_Cheques", SqlDbType.Decimal, (_Cuenta.importeTotalChequesPropios + _Cuenta.importeTotalChequesOtros));
                //Conexion.creaParametro(cmd, "@Archivo", SqlDbType.NVarChar, JsonConvert.SerializeObject(_Cuenta));
                tbl = Conexion.ejecutaConsulta(cmd);
                _Cuenta.claveArchivo = tbl.Rows[0][1].ToString();
                //cmd = Conexion.creaComando("Sprocedure_Insert_JsonBanregio", _Cn);
                //Conexion.creaParametro(cmd, "@Id_Archivo", SqlDbType.Int, tbl.Rows[0][0].ToString());
                //Conexion.creaParametro(cmd, "@Json", SqlDbType.NVarChar, JsonConvert.SerializeObject(_Cuenta));
                //Conexion.ejecutarNonquery(cmd);
                cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioCuentas", _Cn);
                Conexion.creaParametro(cmd, "@numeroCuenta", SqlDbType.VarChar, _Cuenta.numeroCuenta);
                Conexion.creaParametro(cmd, "@instituto", SqlDbType.VarChar, _Cuenta.instituto);
                Conexion.creaParametro(cmd, "@claveArchivo", SqlDbType.VarChar, _Cuenta.claveArchivo);
                Conexion.creaParametro(cmd, "@numeroDepositos", SqlDbType.Int, _Cuenta.numeroDepositos);
                Conexion.creaParametro(cmd, "@importeTotalEfectivo", SqlDbType.Decimal, decimal.Round(_Cuenta.importeTotalEfectivo, 2));
                Conexion.creaParametro(cmd, "@importeTotalChequesPropios", SqlDbType.Decimal, _Cuenta.importeTotalChequesPropios);
                Conexion.creaParametro(cmd, "@importeTotalChequesOtros", SqlDbType.Decimal, _Cuenta.importeTotalChequesOtros);
                Conexion.creaParametro(cmd, "@razonSocialCliente", SqlDbType.VarChar, _Cuenta.razonSocialCliente);
                var _idcuenta = Conexion.ejecutaScalar(cmd);
                idcuentamsg = _idcuenta.ToString();
                //recorre la lista de depositos relacionados a la cuenta
                _ttDepositos = _Cuenta.depositos.Count - 1;
                foreach (var deposito in _Cuenta.depositos)
                {

                    //registro del deposito
                    cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioDepositos", _Cn);
                    Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, _idcuenta);
                    Conexion.creaParametro(cmd, "@consecutivo", SqlDbType.Int, deposito.consecutivo);
                    Conexion.creaParametro(cmd, "@divisa", SqlDbType.VarChar, deposito.divisa);
                    Conexion.creaParametro(cmd, "@remesa", SqlDbType.VarChar, deposito.remesa);
                    _remisionmsg = deposito.remesa.ToString();
                    Conexion.creaParametro(cmd, "@referencia", SqlDbType.VarChar, deposito.referencia);
                    Conexion.creaParametro(cmd, "@importeTotal", SqlDbType.Decimal, deposito.importeTotal);
                    Conexion.creaParametro(cmd, "@importeFicha", SqlDbType.Decimal, deposito.importeFicha);
                    Conexion.creaParametro(cmd, "@diferencia", SqlDbType.Decimal, deposito.diferencia);
                    Conexion.creaParametro(cmd, "@tipoDiferencia", SqlDbType.VarChar, deposito.tipoDiferencia);
                    var _id_Deposito = Conexion.ejecutaScalar(cmd);
                    TrackDepositos.Add(new Tuple<string, string, Deposito>(_numerocuentamsg + ":" + idcuentamsg, _id_Deposito.ToString(), deposito));
                    iddepositomsg = _id_Deposito.ToString();
                    //registro de detalle de efectivo del deposito
                    cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioDetalleEfectivo", _Cn);
                    Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, _id_Deposito);
                    Conexion.creaParametro(cmd, "@fechaRecepcion", SqlDbType.DateTime, deposito.detalleEfectivo.fechaRecepcion);
                    Conexion.creaParametro(cmd, "@importe", SqlDbType.Decimal, deposito.detalleEfectivo.importe);
                    var _id_DetalleEfectivo = Conexion.ejecutaScalar(cmd);
                    iddetallemsg = _id_DetalleEfectivo.ToString();
                    //recorrer lista de detalle de efectivo relacionado al deposito
                    _ttDesgloses = deposito.detalleEfectivo.desglose.Count();
                    foreach (var desglose in deposito.detalleEfectivo.desglose)
                    {
                        //registrar desglose de efectivo 
                        cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioDetalleEfectivoDesglose", _Cn);
                        Conexion.creaParametro(cmd, "@id_detalleefectivo", SqlDbType.Int, _id_DetalleEfectivo);
                        Conexion.creaParametro(cmd, "@tipo", SqlDbType.VarChar, desglose.tipo);
                        Conexion.creaParametro(cmd, "@denominacion", SqlDbType.Decimal, desglose.denominacion);
                        Conexion.creaParametro(cmd, "@cantidad", SqlDbType.Int, desglose.cantidad);
                        Conexion.creaParametro(cmd, "@importe", SqlDbType.Decimal, desglose.importe);
                        Conexion.ejecutaConsulta(cmd);
                        _cDesgloses++;
                    }

                    //recorrer lista de detalle de cheques relacionado al deposito
                    _ttCheques = deposito.detalleCheque.Count();
                    foreach (var cheque in deposito.detalleCheque)
                    {
                        //registrar detalle de cheque
                        cmd = Conexion.creaComando("Sprocedure_Insert_Pro_ArchivosBanregioDetalleCheques", _Cn);
                        Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, _id_Deposito);
                        Conexion.creaParametro(cmd, "@tipoCheque", SqlDbType.VarChar, cheque.tipoCheque);
                        Conexion.creaParametro(cmd, "@referenciaControl", SqlDbType.Int, cheque.referenciaControl);
                        Conexion.creaParametro(cmd, "@bandaMagnetica", SqlDbType.VarChar, cheque.bandaMagnetica);
                        Conexion.creaParametro(cmd, "@numeroSeguridadInvisible", SqlDbType.VarChar, cheque.numeroSeguridadInvisible);
                        Conexion.creaParametro(cmd, "@fechaRecepcion", SqlDbType.DateTime, cheque.fechaRecepcion);
                        Conexion.creaParametro(cmd, "@importe", SqlDbType.Decimal, cheque.importe);
                        Conexion.ejecutaConsulta(cmd);
                        _cCheques++;
                    }
                    _cDepositos++;
                }
                //agregar id cuenta a archivobanregio

                cmd = Conexion.creaComando("Sprocedure_Update_ProArchivosBanregio", _Cn);
                Conexion.creaParametro(cmd, "@Id_Archivo", SqlDbType.Int, tbl.Rows[0][0].ToString());
                Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, _idcuenta);
                Conexion.ejecutarNonquery(cmd);

                message.Add(String.Format("Cuenta:{0},TTDepositos:{1},InsDepositos:{2},TTCheques:{3},InsCheques:{4}", _numerocuentamsg, _ttDepositos, _cDepositos, _ttCheques, _cCheques));
                //message.Add(_numerocuentamsg + idcuentamsg + _remisionmsg + "|desgloses:[total:"+_ttDesgloses+",Insertados:" + _cDesgloses + "]|cheques:[Total:"+_ttCheques+",Insertados:" + _cCheques+"]");

            }
            catch (Exception ex)
            {
                ErrorTrack.Add(ex.StackTrace);

            }

        }

        public Cuenta Detalle_Efectivo(string Json)
        {
            return JsonConvert.DeserializeObject<Cuenta>(Json);
        }

        public CuentaI Detalle_EfectivoInt(string Json)
        {
            return JsonConvert.DeserializeObject<CuentaI>(Json);
        }

        public string JsonBanregioCuentas(SqlConnection cn, List<int> lcuentas)
        {
            SqlConnection _Cn;
            List<Cuenta> lc_cuentas = new List<Cuenta>();
           

            try
            {
                _Cn = cn;
             
                foreach (var icuenta in lcuentas)
                {
                    List<Deposito> lc_deposito = new List<Deposito>();
                    List<Denominacion> lc_desglose = new List<Denominacion>();
                    List<DetalleCheque> lc_ListaCheques = new List<DetalleCheque>();
                    SqlCommand cmd = null;

                    cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDepositos", _Cn);
                    Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, icuenta);
                    var tdepositos = Conexion.ejecutaConsulta(cmd);

                    for (int i = 0; i < tdepositos.Rows.Count; i++)
                    {

                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleEfectivo", _Cn);
                        Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, Convert.ToInt32(tdepositos.Rows[i][0]));
                        var tdetalleEfectivo = Conexion.ejecutaConsulta(cmd);
                        //agregar detalle
                        DetalleEfectivo _idetalle = new DetalleEfectivo();
                        _idetalle.fechaRecepcion = DateTime.Parse(tdetalleEfectivo.Rows[0][2].ToString()).ToString("yyyy-MM-ddThh:mm:ss.000Z");
                        _idetalle.importe = Convert.ToDecimal(tdetalleEfectivo.Rows[0][3]);

                        //desglose
                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleEfectivoDesglose", _Cn);
                        Conexion.creaParametro(cmd, "@id_detalleefectivo", SqlDbType.Int, Convert.ToInt32(tdetalleEfectivo.Rows[0][0]));
                        var tdetalleEfectivodesglose = Conexion.ejecutaConsulta(cmd);

                        for (int ids = 0; ids < tdetalleEfectivodesglose.Rows.Count; ids++)
                        {
                            lc_desglose.Add(new Denominacion
                            {
                                tipo = tdetalleEfectivodesglose.Rows[ids][1].ToString(),
                                denominacion = Convert.ToDecimal(tdetalleEfectivodesglose.Rows[ids][2]),
                                cantidad = Convert.ToInt32(tdetalleEfectivodesglose.Rows[ids][3]),
                                importe = Convert.ToDecimal(tdetalleEfectivodesglose.Rows[ids][4]),
                            });
                        }
                        //agregar desglose
                        _idetalle.desglose = lc_desglose;
                        //sacar cheques
                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleCheques", _Cn);
                        Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, Convert.ToInt32(tdepositos.Rows[i][0]));
                        var tdetallecheques = Conexion.ejecutaConsulta(cmd);

                        for (int ic = 0; ic < tdetallecheques.Rows.Count; ic++)
                        {
                            lc_ListaCheques.Add(new DetalleCheque
                            {
                                tipoCheque = tdetallecheques.Rows[ic][1].ToString(),
                                referenciaControl = Convert.ToInt32(tdetallecheques.Rows[ic][2]),
                                bandaMagnetica = tdetallecheques.Rows[ic][3].ToString(),
                                numeroSeguridadInvisible = tdetallecheques.Rows[ic][4].ToString(),
                                fechaRecepcion = tdetallecheques.Rows[ic][5].ToString(),
                                importe = Convert.ToDecimal(tdetallecheques.Rows[ic][6].ToString())
                            });
                        }

                        //agregar deposito
                        lc_deposito.Add(new Deposito
                        {
                            consecutivo = Convert.ToInt32(tdepositos.Rows[i][2]),
                            divisa = tdepositos.Rows[i][3].ToString(),
                            remesa = Convert.ToInt64(tdepositos.Rows[i][4]),
                            referencia = tdepositos.Rows[i][5].ToString(),
                            importeTotal = Convert.ToDecimal(tdepositos.Rows[i][6]),
                            importeFicha = Convert.ToDecimal(tdepositos.Rows[i][7]),
                            diferencia = Convert.ToDecimal(tdepositos.Rows[i][8]),
                            tipoDiferencia = tdepositos.Rows[i][9].ToString(),
                            detalleEfectivo = _idetalle,
                            detalleCheque = lc_ListaCheques
                        });
                    }

                    //sacar cuenta
                    cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioCuenta", _Cn);
                    Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, icuenta);
                    var tcuenta = Conexion.ejecutaConsulta(cmd);
                    Cuenta cuenta = new Cuenta();
                    cuenta.numeroCuenta = tcuenta.Rows[0][1].ToString();
                    cuenta.instituto = tcuenta.Rows[0][2].ToString();
                    cuenta.claveArchivo = tcuenta.Rows[0][3].ToString();
                    cuenta.numeroDepositos = Convert.ToInt32(tcuenta.Rows[0][4]);
                    cuenta.importeTotalEfectivo = Convert.ToDecimal(tcuenta.Rows[0][5]);
                    cuenta.importeTotalChequesPropios = Convert.ToDecimal(tcuenta.Rows[0][6]);
                    cuenta.importeTotalChequesOtros = Convert.ToDecimal(tcuenta.Rows[0][7]);
                    cuenta.razonSocialCliente = tcuenta.Rows[0][8].ToString();
                    cuenta.depositos = lc_deposito;

                    lc_cuentas.Add(cuenta);
                    lc_deposito.Clear();
                    lc_desglose.Clear();
                    lc_ListaCheques.Clear();
                }

            }
            catch (Exception ex)
            {
                ErrorTrack.Add(ex.ToString());
                ErrorTrack.Add(ex.StackTrace.ToString());

            }


            var json = new { fecha = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.000Z"), cuentas = lc_cuentas };

            return JsonConvert.SerializeObject(json);
        }


        public string JsonCuentas(SqlConnection cn, List<int> lcuentas)
        {
            SqlConnection _Cn;
            List<CuentaI> lc_cuentas = new List<CuentaI>();


            try
            {
                _Cn = cn;
             
                foreach (var icuenta in lcuentas)
                {
                    List<Deposito> lc_deposito = new List<Deposito>();
                    List<Denominacion> lc_desglose = new List<Denominacion>();
                    List<DetalleCheque> lc_ListaCheques = new List<DetalleCheque>();
                    SqlCommand cmd = null;

                    cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDepositos", _Cn);
                    Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, icuenta);
                    var tdepositos = Conexion.ejecutaConsulta(cmd);

                    for (int i = 0; i < tdepositos.Rows.Count; i++)
                    {

                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleEfectivo", _Cn);
                        Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, Convert.ToInt32(tdepositos.Rows[i][0]));
                        var tdetalleEfectivo = Conexion.ejecutaConsulta(cmd);
                        //agregar detalle
                        DetalleEfectivo _idetalle = new DetalleEfectivo();
                        _idetalle.fechaRecepcion = DateTime.Parse(tdetalleEfectivo.Rows[0][2].ToString()).ToString("yyyy-MM-ddThh:mm:ss.000Z");
                        _idetalle.importe = Convert.ToDecimal(tdetalleEfectivo.Rows[0][3]);

                        //desglose
                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleEfectivoDesglose", _Cn);
                        Conexion.creaParametro(cmd, "@id_detalleefectivo", SqlDbType.Int, Convert.ToInt32(tdetalleEfectivo.Rows[0][0]));
                        var tdetalleEfectivodesglose = Conexion.ejecutaConsulta(cmd);

                        for (int ids = 0; ids < tdetalleEfectivodesglose.Rows.Count; ids++)
                        {
                            lc_desglose.Add(new Denominacion
                            {
                                tipo = tdetalleEfectivodesglose.Rows[ids][1].ToString(),
                                denominacion = Convert.ToDecimal(tdetalleEfectivodesglose.Rows[ids][2]),
                                cantidad = Convert.ToInt32(tdetalleEfectivodesglose.Rows[ids][3]),
                                importe = Convert.ToDecimal(tdetalleEfectivodesglose.Rows[ids][4]),
                            });
                        }
                        //agregar desglose
                        _idetalle.desglose = lc_desglose;
                        //sacar cheques
                        cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioDetalleCheques", _Cn);
                        Conexion.creaParametro(cmd, "@id_deposito", SqlDbType.Int, Convert.ToInt32(tdepositos.Rows[i][0]));
                        var tdetallecheques = Conexion.ejecutaConsulta(cmd);

                        for (int ic = 0; ic < tdetallecheques.Rows.Count; ic++)
                        {
                            lc_ListaCheques.Add(new DetalleCheque
                            {
                                tipoCheque = tdetallecheques.Rows[ic][1].ToString(),
                                referenciaControl = Convert.ToInt32(tdetallecheques.Rows[ic][2]),
                                bandaMagnetica = tdetallecheques.Rows[ic][3].ToString(),
                                numeroSeguridadInvisible = tdetallecheques.Rows[ic][4].ToString(),
                                fechaRecepcion = tdetallecheques.Rows[ic][5].ToString(),
                                importe = Convert.ToDecimal(tdetallecheques.Rows[ic][6].ToString())
                            });
                        }

                        //agregar deposito
                        lc_deposito.Add(new Deposito
                        {
                            consecutivo = Convert.ToInt32(tdepositos.Rows[i][2]),
                            divisa = tdepositos.Rows[i][3].ToString(),
                            remesa = Convert.ToInt64(tdepositos.Rows[i][4]),
                            referencia = tdepositos.Rows[i][5].ToString(),
                            importeTotal = Convert.ToDecimal(tdepositos.Rows[i][6]),
                            importeFicha = Convert.ToDecimal(tdepositos.Rows[i][7]),
                            diferencia = Convert.ToDecimal(tdepositos.Rows[i][8]),
                            tipoDiferencia = tdepositos.Rows[i][9].ToString(),
                            detalleEfectivo = _idetalle,
                            detalleCheque = lc_ListaCheques
                        });
                    }

                    //sacar cuenta
                    cmd = Conexion.creaComando("Sprocedure_Get_Pro_ArchivosBanregioCuenta", _Cn);
                    Conexion.creaParametro(cmd, "@id_cuenta", SqlDbType.Int, icuenta);
                    var tcuenta = Conexion.ejecutaConsulta(cmd);
                    CuentaI cuenta = new CuentaI();
                    cuenta.id = tcuenta.Rows[0][0].ToString();
                    cuenta.numeroCuenta = tcuenta.Rows[0][1].ToString();
                    cuenta.instituto = tcuenta.Rows[0][2].ToString();
                    cuenta.claveArchivo = tcuenta.Rows[0][3].ToString();
                    cuenta.numeroDepositos = Convert.ToInt32(tcuenta.Rows[0][4]);
                    cuenta.importeTotalEfectivo = Convert.ToDecimal(tcuenta.Rows[0][5]);
                    cuenta.importeTotalChequesPropios = Convert.ToDecimal(tcuenta.Rows[0][6]);
                    cuenta.importeTotalChequesOtros = Convert.ToDecimal(tcuenta.Rows[0][7]);
                    cuenta.razonSocialCliente = tcuenta.Rows[0][8].ToString();
                    cuenta.depositos = lc_deposito;

                    lc_cuentas.Add(cuenta);
                 
                }

            }
            catch (Exception ex)
            {
                ErrorTrack.Add(ex.ToString());
                ErrorTrack.Add(ex.StackTrace.ToString());

            }


            //var json = new { fecha = DateTime.Now.ToString(), cuentas =  };

            return JsonConvert.SerializeObject(lc_cuentas);
        }


    }
}
