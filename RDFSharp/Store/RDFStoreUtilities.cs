/*
   Copyright 2012-2025 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStoreUtilities is a collector of reusable utility methods for RDF store management
    /// </summary>
    public static class RDFStoreUtilities
    {
        #region Select
        /// <summary>
        /// Parses the current quadruple of the data reader
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        public static RDFQuadruple ParseQuadruple(IDataReader fetchedQuadruples)
        {
            #region Guards
            if (fetchedQuadruples == null)
                throw new RDFStoreException("Cannot parse quadruple because given \"fetchedQuadruples\" parameter is null.");
            #endregion

            RDFContext qContext = new RDFContext(fetchedQuadruples["Context"].ToString());
            RDFResource qSubject = new RDFResource(fetchedQuadruples["Subject"].ToString());
            RDFResource qPredicate = new RDFResource(fetchedQuadruples["Predicate"].ToString());

            //SPO-flavour quadruple
            if (string.Equals(fetchedQuadruples["TripleFlavor"].ToString(), "1"))
            {
                RDFResource qObject = new RDFResource(fetchedQuadruples["Object"].ToString());
                return new RDFQuadruple(qContext, qSubject, qPredicate, qObject);
            }

            //SPL-flavour quadruple
            string literal = fetchedQuadruples["Object"].ToString();

            //PlainLiteral
            //Detect presence of semantically valid datatype indicator ("^^")
            int lastIndexOfDatatype = literal.LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
            if (lastIndexOfDatatype == -1
                 || lastIndexOfDatatype == literal.Length - 2 //EndsWith "^^"
                 || RDFModelUtilities.GetUriFromString(literal.Substring(lastIndexOfDatatype + 2)) == null)
            {
                RDFPlainLiteral pLit;
                if (RDFShims.EndingLangTagRegex.Value.IsMatch(literal))
                {
                    int lastIndexOfLanguage = literal.LastIndexOf('@');
                    string pLitValue = literal.Substring(0, lastIndexOfLanguage);
                    string pLitLang = literal.Substring(lastIndexOfLanguage + 1);
                    pLit = new RDFPlainLiteral(pLitValue, pLitLang);
                }
                else
                {
                    pLit = new RDFPlainLiteral(literal);
                }
                return new RDFQuadruple(qContext, qSubject, qPredicate, pLit);
            }

            //TypedLiteral
            string tLitValue = literal.Substring(0, lastIndexOfDatatype);
            string tLitDatatype = literal.Substring(lastIndexOfDatatype + 2);
            RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, RDFDatatypeRegister.GetDatatype(tLitDatatype));
            return new RDFQuadruple(qContext, qSubject, qPredicate, tLit);
        }
        #endregion
    }
}