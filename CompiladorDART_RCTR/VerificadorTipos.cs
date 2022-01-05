using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompiladorDART_RCTR
{
    public class VerificadorTipos
    {

        //copia del arbol original
        public NodoArbol arbolSintactico;

        /// <summary>
        /// constructor donde recibe el arbol original
        /// </summary>
        /// <param name="arbolSintactico"></param>
        public VerificadorTipos(NodoArbol arbolSintactico)
        {
            this.arbolSintactico = arbolSintactico;
            EjecutarVerificacionTipos(arbolSintactico);
        }

        public void EjecutarVerificacionTipos(NodoArbol arbolSintactico)
        {
            obtenerSiguienteVerificacion(arbolSintactico);
            if (arbolSintactico.hermano != null)
            {
                EjecutarVerificacionTipos(arbolSintactico.hermano);
            }
        }

        private void obtenerSiguienteVerificacion(NodoArbol arbolSintactico)
        {
            if (arbolSintactico.soyDeTipoNodo == TipoNodoArbol.Sentencia
               && arbolSintactico.soySentenciaDeTipo == TipoSentencia.ASIGNACION)
            {
                RecorridoAsignacion(arbolSintactico);
            }
            if (arbolSintactico.soyDeTipoNodo == TipoNodoArbol.Sentencia
               && arbolSintactico.soySentenciaDeTipo == TipoSentencia.FOR)
            {
                // RecorridoFor(arbolSintactico);
            }
            if (arbolSintactico.soyDeTipoNodo == TipoNodoArbol.Sentencia
               && arbolSintactico.soySentenciaDeTipo == TipoSentencia.IF)
            {
                RecorridoIF(arbolSintactico);
            }
            if (arbolSintactico.soyDeTipoNodo == TipoNodoArbol.Sentencia
                 && arbolSintactico.soySentenciaDeTipo == TipoSentencia.ESCRIBIR)
            {
                //RecorridoESCRIBIR(arbolSintactico);
            }

            if (arbolSintactico.soyDeTipoNodo == TipoNodoArbol.Sentencia
                && arbolSintactico.soySentenciaDeTipo == TipoSentencia.LEER)
            {
                //RecorridoLEER(arbolSintactico);
            }
        }

        public void RecorridoIF(NodoArbol miArbol)
        {
            RecorridoCondicional(miArbol.hijoIzquierdo);
            EjecutarVerificacionTipos(miArbol.hijoCentro);
            EjecutarVerificacionTipos(miArbol.hijoIzquierdo);

        }
        public void RecorridoCondicional(NodoArbol miArbol)
        {
            var tipoHijoIzquierdo = RecorridoPostOrdenExpresiones(miArbol.hijoIzquierdo);
            var tipoHijoDerecho = RecorridoPostOrdenExpresiones(miArbol.hijoDerecho);

            try
            {
                var tipoCondicional = FuncionEquivalenciaCondcional(tipoHijoIzquierdo,
                 tipoHijoDerecho, miArbol.soyOperacionCondicionaDeTipo);
            }
            catch (Exception)
            {

                var error = new Error()
                {
                    Codigo = 700,
                    Linea = miArbol.linea,
                    MensajeError = string.Format("el operador {0} no se puede aplicar a operandos del tipo {1}, {2}",
                miArbol.soyOperacionCondicionaDeTipo, tipoHijoDerecho, tipoHijoIzquierdo),
                    TipoError = tipoError.Semantico
                };
                TablaSimbolos.listaErroresSemantico.Add(error);

            }


        }
        private void RecorridoAsignacion(NodoArbol miArbol)
        {
            miArbol.tipoValorNodoHijoIzquierdo = RecorridoPostOrdenExpresiones(miArbol.hijoIzquierdo);

            miArbol.tipoValorNodoHijoIzquierdo =
                RecorridoPostOrdenExpresiones(miArbol.hijoIzquierdo);

            if (miArbol.SoyDeTipoDato != miArbol.tipoValorNodoHijoIzquierdo)
            {
                var error = new Error()
                {
                    Codigo = 700,
                    Linea = miArbol.linea,
                    MensajeError = string.Format("no se puede asinar un tipo {1} a un tipo {0}",
                    miArbol.SoyDeTipoDato, miArbol.tipoValorNodoHijoIzquierdo),
                    TipoError = tipoError.Semantico
                };

                TablaSimbolos.listaErroresSemantico.Add(error);
            }
        }

        private string RecorridoPostOrdenExpresiones(NodoArbol miArbol)
        {
            if (miArbol.hijoIzquierdo != null)
                miArbol.tipoValorNodoHijoIzquierdo =
                     RecorridoPostOrdenExpresiones(miArbol.hijoIzquierdo); 

            if (miArbol.hijoDerecho != null)
                miArbol.tipoValorNodoHijoDerecho =
                    RecorridoPostOrdenExpresiones(miArbol.hijoDerecho);

            if (miArbol.SoyDeTipoExpresion == tipoExpresion.Operador)
            {
                try
                {
                    return FuncionEquivalencia(miArbol.tipoValorNodoHijoIzquierdo,
                     miArbol.tipoValorNodoHijoDerecho, miArbol.soyDeTipoOperacion);
                }
                catch (Exception)
                {

                    var error = new Error()
                    {
                        Codigo = 700,
                        Linea = miArbol.linea,
                        MensajeError = string.Format("no se puede hacer la operacion" + miArbol.soyDeTipoOperacion + " con un tipo {1} y un tipo {0}",
                    miArbol.tipoValorNodoHijoDerecho, miArbol.tipoValorNodoHijoIzquierdo),
                        TipoError = tipoError.Semantico
                    };
                    TablaSimbolos.listaErroresSemantico.Add(error);

                }

            }

            else if (miArbol.SoyDeTipoExpresion == tipoExpresion.Constante)
            {
                return miArbol.SoyDeTipoDato;
            }
            else if (miArbol.SoyDeTipoExpresion == tipoExpresion.Identificador)
            {
                return miArbol.SoyDeTipoDato;
            }
            return "NADA";
        }

        private string FuncionEquivalencia(string tipoValorNodoHijoIzquierdo, string tipoValorNodoHijoDerecho,
            string soyDeTipoOperacion)
        {

            if (tipoValorNodoHijoIzquierdo == "STRING"
               && tipoValorNodoHijoDerecho == "CHAR"
               && soyDeTipoOperacion == "SUMA")
            {
                return "STRING";
            }



            if (tipoValorNodoHijoIzquierdo == "STRING"
                && tipoValorNodoHijoDerecho == "STRING"
                && soyDeTipoOperacion == "Suma")
            {
                return "STRING";
            }

            if (tipoValorNodoHijoIzquierdo == "INT"
                && tipoValorNodoHijoDerecho == "INT"
                && soyDeTipoOperacion == "Suma")
            {
                return "INT";
            }

            if (tipoValorNodoHijoIzquierdo == "INT"
                && tipoValorNodoHijoDerecho == "INT"
                && soyDeTipoOperacion == "Resta")
            {
                return "INT";
            }

            if (tipoValorNodoHijoIzquierdo == "INT"
               && tipoValorNodoHijoDerecho == "INT"
               && soyDeTipoOperacion == "Multiplicacion")
            {
                return "INT";
            }
            if (tipoValorNodoHijoIzquierdo == "INT"
               && tipoValorNodoHijoDerecho == "INT"
               && soyDeTipoOperacion == "Division")
            {
                return "DOUBLE";
            }

            if (tipoValorNodoHijoIzquierdo == "DOUBLE"
           && tipoValorNodoHijoDerecho == "DOUBLE"
           && (soyDeTipoOperacion == "Suma"
               || soyDeTipoOperacion == "Resta"
               || soyDeTipoOperacion == "Multiplicacion"
               || soyDeTipoOperacion == "Division"))
            {
                return "DOUBLE";
            }

            if (tipoValorNodoHijoIzquierdo == "DOUBLE"
          && tipoValorNodoHijoDerecho == "INT"
          && (soyDeTipoOperacion == "Suma"
              || soyDeTipoOperacion == "Resta"
              || soyDeTipoOperacion == "Multiplicacion"
              || soyDeTipoOperacion == "Division"))
            {
                return "DOUBLE";
            }


            if (tipoValorNodoHijoIzquierdo == "INT"
         && tipoValorNodoHijoDerecho == "DOUBLE"
         && (soyDeTipoOperacion == "Suma"
             || soyDeTipoOperacion == "Resta"
             || soyDeTipoOperacion == "Multiplicacion"
             || soyDeTipoOperacion == "Division"))
            {
                return "DOUBLE";
            }


            throw new Exception();
        }

        private string FuncionEquivalenciaCondcional(string tipoValorNodoHijoIzquierdo, string tipoValorNodoHijoDerecho,
            OperacionCondicional soyDeTipoOperacion)
        {
            if (tipoValorNodoHijoIzquierdo == "INT"
                && tipoValorNodoHijoDerecho == "INT"
                && (soyDeTipoOperacion == OperacionCondicional.MayorQue
                || soyDeTipoOperacion == OperacionCondicional.MenorQue
                || soyDeTipoOperacion == OperacionCondicional.Diferente
                || soyDeTipoOperacion == OperacionCondicional.IgualIgual
                || soyDeTipoOperacion == OperacionCondicional.MenorIgualQue
                || soyDeTipoOperacion == OperacionCondicional.MayorIgualQue))
            {
                return "BOOL";
            }



            throw new Exception();
        }

    }


}
