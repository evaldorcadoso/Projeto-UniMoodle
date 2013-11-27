using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UniMoodle
{
    class Program
    {
        static void Main(string[] args)
        {


            //var texto = "RénatÔæ~ Gomão * ¨";
            //texto = TiraAcentos(texto);
            //var resultado = normalizeText(texto);
            //Console.WriteLine(resultado);
            //Console.ReadLine();
        }


        public static string TiraAcentos(string texto)
        {
            string comAcentos = ":()/',+*&%#@!~`{}[]^?<>=’ $ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string semAcentos = "                           AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString()).Trim();
            }

            return texto;
        }


        public static String RemoverCaracteresEspeciais(String self)
        {
            var normalizedString = self;

            // Prepara a tabela de símbolos.
            var symbolTable = new Dictionary<string, char[]>();

            symbolTable.Add("a", new char[] { 'à', 'á', 'ä', 'â', 'ã', 'å' });
            symbolTable.Add("A", new char[] { 'À', 'Á', 'Ä', 'Â', 'Ã', 'Å' });

            symbolTable.Add("ae", new char[] { 'æ' });
            symbolTable.Add("AE", new char[] { 'Æ' });

            symbolTable.Add("oe", new char[] { 'œ' });
            symbolTable.Add("OE", new char[] { 'Œ' });

            symbolTable.Add("c", new char[] { 'ç' });
            symbolTable.Add("C", new char[] { 'Ç' });

            symbolTable.Add("e", new char[] { 'è', 'é', 'ë', 'ê' });
            symbolTable.Add("E", new char[] { 'È', 'É', 'Ë', 'Ê' });

            symbolTable.Add("i", new char[] { 'ì', 'í', 'ï', 'î' });
            symbolTable.Add("I", new char[] { 'Ì', 'Í', 'Ï', 'Î' });

            symbolTable.Add("o", new char[] { 'ò', 'ó', 'ö', 'ô', 'õ' });
            symbolTable.Add("O", new char[] { 'Ò', 'Ó', 'Ö', 'Ô', 'Õ' });

            symbolTable.Add("u", new char[] { 'ù', 'ú', 'ü', 'û' });
            symbolTable.Add("U", new char[] { 'Ù', 'Ú', 'Ü', 'Û' });

            symbolTable.Add("-", new char[] { '&' });

            symbolTable.Add("n", new char[] { 'ñ' });
            symbolTable.Add("N", new char[] { 'Ñ' });

            symbolTable.Add("y", new char[] { 'ý', 'ÿ' });
            symbolTable.Add("Y", new char[] { 'Ý', 'Ÿ' });
            //coloquei underline aqui
            symbolTable.Add("_", new char[] { '^', '¨', '#', '+', '%', '*', '~', '\'', '"', '`', '´' ,' '});

            // Substitui os símbolos.
            foreach (var key in symbolTable.Keys)
            {
                foreach (char symbol in symbolTable[key])
                {
                    normalizedString = normalizedString.Replace(symbol.ToString(), key);
                }
            }

            // Remove os outros caracteres especiais.
            normalizedString = Regex.Replace(normalizedString, "[^0-9a-zA-Z._ ]+?", "");
            return normalizedString;
        }


        private static string RemoveCaracteresEspeciais(string texto)
        {
            //texto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in texto.ToCharArray())
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString();
        }

        private static string normalizeText(string text)
        {
            StringBuilder resultado = new StringBuilder();
            //var regx;
            string normalize = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                var regx = new Regex(@"/[èéêë]/gi").IsMatch(text).ToString();

                var test = new Regex(@"/[èéêë]/gi").IsMatch(text) ? text.Replace(text, "e") : text;





                text = new Regex(@"/[àáâãäå]/gi").IsMatch(text) ? text.Replace(text, "a") : text;
                text = new Regex(@"/[ÀÁÂÃÄÅ]/gi").IsMatch(text) ? text.Replace(text, "A") : text;

                text = new Regex(@"/æ/gi").IsMatch(text) ? text.Replace(text, "ae") : text;
                text = new Regex(@"/Æ/gi").IsMatch(text) ? text.Replace(text, "AE") : text;

                text = new Regex(@"/&/gi").IsMatch(text) ? text.Replace(text, "-") : text;

                text = new Regex(@"/ç/gi").IsMatch(text) ? text.Replace(text, "c") : text;
                text = new Regex(@"/Ç/gi").IsMatch(text) ? text.Replace(text, "C") : text;

                text = new Regex(@"/[èéêë]/gi").IsMatch(text) ? text.Replace(text, "e") : text;
                text = new Regex(@"/[ÈÉÊË]/gi").IsMatch(text) ? text.Replace(text, "E") : text;

                text = new Regex(@"/['""'`´.]/gi").IsMatch(text) ? text.Replace(text, "") : text;

                text = new Regex(@"/[ìíîï]/gi").IsMatch(text) ? text.Replace(text, "i") : text;
                text = new Regex(@"/[ÌÍÎÏ]/gi").IsMatch(text) ? text.Replace(text, "I") : text;

                text = new Regex(@"/ñ/gi").IsMatch(text) ? text.Replace(text, "n") : text;
                text = new Regex(@"/Ñ/gi").IsMatch(text) ? text.Replace(text, "N") : text;

                text = new Regex(@"/[òóôõö]/gi").IsMatch(text) ? text.Replace(text, "o") : text;
                text = new Regex(@"/[ÒÓÔÕÖ]/gi").IsMatch(text) ? text.Replace(text, "O") : text;

                text = new Regex(@"/œ/gi").IsMatch(text) ? text.Replace(text, "oe") : text;
                text = new Regex(@"/Œ/gi").IsMatch(text) ? text.Replace(text, "OE") : text;

                text = new Regex(@"/[ùúûü]/gi").IsMatch(text) ? text.Replace(text, "u") : text;
                text = new Regex(@"/[ÙÚÛÜ]/gi").IsMatch(text) ? text.Replace(text, "U") : text;

                text = new Regex(@"/[ýÿ]/gi").IsMatch(text) ? text.Replace(text, "y") : text;
                text = new Regex(@"/[ÝŸ]/gi").IsMatch(text) ? text.Replace(text, "Y") : text;
                
                text = new Regex(@"/\s/g").IsMatch(text) ? text.Replace(text, "-") : text;
                text = new Regex(@"/[^¨#+%*~]/gi").IsMatch(text) ? text.Replace(text, "") : text;

                normalize = new Regex(@"/[^¨#+%*~]/gi").IsMatch(text) ? text.Replace(text, "") : text;


                resultado.Append((normalize));
            }
            return normalize;
        }
    }
}