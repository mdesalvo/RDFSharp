/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Linq;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFGraphMetadata represents a collector for metadata describing contents of a RDFGraph.
    /// </summary>
    internal class RDFGraphMetadata {

        #region Properties
        /// <summary>
        /// Dictionary of resources acting as container subjects in the graph
        /// </summary>
        internal Dictionary<Int64, RDFModelEnums.RDFContainerTypes> Containers { get; set; }

        /// <summary>
        /// Dictionary of resources acting as collection subjects in the graph
        /// </summary>
        internal Dictionary<Int64, RDFCollectionItem> Collections { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty metadata
        /// </summary>
        internal RDFGraphMetadata() {
            this.Containers  = new Dictionary<Int64, RDFModelEnums.RDFContainerTypes>();
            this.Collections = new Dictionary<Int64, RDFCollectionItem>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Verifies if the given triple carries a container subj and, if so, collects it
        /// </summary>
        private void CollectContainers(RDFTriple triple) {
            if (triple != null && triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {

                //SUBJECT -> rdf:type -> rdf:[Bag|Seq|Alt]
                if (triple.Predicate.Equals(RDFVocabulary.RDF.TYPE)) {
                    //rdf:Bag
                    if (triple.Object.Equals(RDFVocabulary.RDF.BAG)) {
                        if (!this.Containers.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Containers.Add(triple.Subject.PatternMemberID, RDFModelEnums.RDFContainerTypes.Bag);
                        }
                    }
                    //rdf:Seq
                    else if (triple.Object.Equals(RDFVocabulary.RDF.SEQ)) {
                        if (!this.Containers.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Containers.Add(triple.Subject.PatternMemberID, RDFModelEnums.RDFContainerTypes.Seq);
                        }
                    }
                    //rdf:Alt
                    else if (triple.Object.Equals(RDFVocabulary.RDF.ALT)) {
                        if (!this.Containers.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Containers.Add(triple.Subject.PatternMemberID, RDFModelEnums.RDFContainerTypes.Alt);
                        }
                    }
                }

            }
        }

         /// <summary>
        /// Verifies if the given triple carries a collection subj and, if so, collects it
        /// </summary>
        private void CollectCollections(RDFTriple triple) {
            if (triple != null) {

                //SUBJECT -> rdf:type -> rdf:list
                if (triple.Predicate.Equals(RDFVocabulary.RDF.TYPE)) {
                    if (triple.Object.Equals(RDFVocabulary.RDF.LIST) && triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        if (!this.Collections.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Collections.Add(triple.Subject.PatternMemberID, new RDFCollectionItem(RDFModelEnums.RDFItemTypes.Resource, RDFVocabulary.RDF.NIL, RDFVocabulary.RDF.NIL));
                        }
                    }
                }

                //SUBJECT -> rdf:first -> [OBJECT|LITERAL]
                else if (triple.Predicate.Equals(RDFVocabulary.RDF.FIRST)) {
                    if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        if (!this.Collections.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Collections.Add(triple.Subject.PatternMemberID, new RDFCollectionItem(RDFModelEnums.RDFItemTypes.Resource, RDFVocabulary.RDF.NIL, RDFVocabulary.RDF.NIL));
                        }
                        this.Collections[triple.Subject.PatternMemberID].ItemType  = RDFModelEnums.RDFItemTypes.Resource;
                        this.Collections[triple.Subject.PatternMemberID].ItemValue = (RDFResource)triple.Object;
                    }
                    else {
                        if (!this.Collections.ContainsKey(triple.Subject.PatternMemberID)) {
                             this.Collections.Add(triple.Subject.PatternMemberID, new RDFCollectionItem(RDFModelEnums.RDFItemTypes.Literal, String.Empty, RDFVocabulary.RDF.NIL));
                        }
                        this.Collections[triple.Subject.PatternMemberID].ItemType  = RDFModelEnums.RDFItemTypes.Literal;
                        this.Collections[triple.Subject.PatternMemberID].ItemValue = (RDFLiteral)triple.Object;
                    }
                }

                //SUBJECT -> rdf:rest -> [BNODE|RDF:NIL]
                else if (triple.Predicate.Equals(RDFVocabulary.RDF.REST) && triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    if (!this.Collections.ContainsKey(triple.Subject.PatternMemberID)) {
                         this.Collections.Add(triple.Subject.PatternMemberID, new RDFCollectionItem(RDFModelEnums.RDFItemTypes.Resource, RDFVocabulary.RDF.NIL, RDFVocabulary.RDF.NIL));
                    }
                    this.Collections[triple.Subject.PatternMemberID].ItemNext = (RDFResource)triple.Object;
                }

            }
        }

        /// <summary>
        /// Clears the metadata of the graph
        /// </summary>
        internal void ClearMetadata() {
            this.Containers.Clear();
            this.Collections.Clear();
        }

        /// <summary>
        /// Updates the metadata of the graph with the info carried by the given triple 
        /// </summary>
        internal void UpdateMetadata(RDFTriple triple) {
            if (triple != null){
                this.CollectContainers(triple);
                this.CollectCollections(triple);
            }
        }
        #endregion

    }

}