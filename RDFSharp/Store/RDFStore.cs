/*
   Copyright 2012-2016 Marco De Salvo

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
using System.Collections.Generic;
using RDFSharp.Model;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFStore represents an abstract RDF store, baseline for Memory or SQL-based implementations.
    /// </summary>
    public abstract class RDFStore: IEquatable<RDFStore> {

        #region Properties
        /// <summary>
        /// Unique representation of the store
        /// </summary>
        public Int64 StoreID { get; set; }

        /// <summary>
        /// Type of the store
        /// </summary>
        public String StoreType { get; set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store
        /// </summary>
        public override String ToString() {
            return this.StoreType;
        }

        /// <summary>
        /// Performs the equality comparison between two stores
        /// </summary>
        public Boolean Equals(RDFStore other) {
            return (other != null && this.StoreID.Equals(other.StoreID));
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore MergeGraph(RDFGraph graph);

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore AddQuadruple(RDFQuadruple quadruple);
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruples from the store
        /// </summary>
        public abstract RDFStore RemoveQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContext(RDFContext contextResource);

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource);

        /// <summary>
        /// Removes the quadruples with the given (non-blank) predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource);

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByObject(RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByLiteral(RDFLiteral objectLiteral);

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public abstract RDFStore ClearQuadruples();

        /// <summary>
        /// Compacts the reified quadruples by removing their 4 standard statements 
        /// </summary>
        public abstract RDFStore UnreifyQuadruples();
        #endregion

        #region Select
        /// <summary>
        /// Gets a list containing the graphs saved in the store
        /// </summary>
        public abstract List<RDFGraph> ExtractGraphs();

        /// <summary>
        /// Checks if the store contains the given quadruple 
        /// </summary>
        public abstract Boolean ContainsQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Gets a store containing all quadruples
        /// </summary>
        public abstract RDFMemoryStore SelectAllQuadruples();

        /// <summary>
        /// Gets a store containing quadruples with the specified context
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruplesByContext(RDFContext contextResource);

        /// <summary>
        /// Gets a store containing quadruples with the specified subject 
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruplesBySubject(RDFResource subjectResource);

        /// <summary>
        /// Gets a store containing quadruples with the specified predicate
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruplesByPredicate(RDFResource predicateResource);

        /// <summary>
        /// Gets a store containing quadruples with the specified object 
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruplesByObject(RDFResource objectResource);

        /// <summary>
        /// Gets a store containing quadruples with the specified literal 
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral objectLiteral);

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        internal abstract RDFMemoryStore SelectQuadruples(RDFContext  contextResource,
                                                          RDFResource subjectResource,
                                                          RDFResource predicateResource,
                                                          RDFResource objectResource,
                                                          RDFLiteral  objectLiteral);
        #endregion

        #endregion

    }

}