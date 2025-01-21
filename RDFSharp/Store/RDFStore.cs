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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStore represents an abstract store in the RDF model.
    /// </summary>
    public abstract class RDFStore : RDFDataSource, IEquatable<RDFStore>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the store
        /// </summary>
        public long StoreID { get; set; }

        /// <summary>
        /// Type of the store
        /// </summary>
        public string StoreType { get; set; }

        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public abstract long QuadruplesCount { get; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store
        /// </summary>
        public override string ToString() => StoreType;

        /// <summary>
        /// Performs the equality comparison between two stores
        /// </summary>
        public bool Equals(RDFStore other)
            => other != null && StoreID.Equals(other.StoreID);
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore MergeGraph(RDFGraph graph);

        /// <summary>
        /// Asynchronously merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public Task<RDFStore> MergeGraphAsync(RDFGraph graph)
            => Task.Run(() => MergeGraph(graph));

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore AddQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Asynchronously adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public Task<RDFStore> AddQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => AddQuadruple(quadruple));
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the store
        /// </summary>
        public abstract RDFStore RemoveQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Asynchronously removes the given quadruple from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => RemoveQuadruple(quadruple));

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContext(RDFContext ctx);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextAsync(RDFContext ctx)
            => Task.Run(() => RemoveQuadruplesByContext(ctx));

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubject(RDFResource subj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given subject from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesBySubjectAsync(RDFResource subj)
            => Task.Run(() => RemoveQuadruplesBySubject(subj));

        /// <summary>
        /// Removes the quadruples with the given (non-blank) predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicate(RDFResource pred);

        /// <summary>
        /// Asynchronously removes the quadruples with the given (non-blank) predicate from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByPredicateAsync(RDFResource pred)
            => Task.Run(() => RemoveQuadruplesByPredicate(pred));

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByObject(RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByObjectAsync(RDFResource obj)
            => Task.Run(() => RemoveQuadruplesByObject(obj));

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByLiteral(RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByLiteralAsync(RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesByLiteral(lit));

        /// <summary>
        /// Removes the quadruples with the given context and subject
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubject(RDFContext ctx, RDFResource subj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context and subject from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextSubjectAsync(RDFContext ctx, RDFResource subj)
            => Task.Run(() => RemoveQuadruplesByContextSubject(ctx, subj));

        /// <summary>
        /// Removes the quadruples with the given context and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicate(RDFContext ctx, RDFResource pred);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context and predicate from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextPredicateAsync(RDFContext ctx, RDFResource pred)
            => Task.Run(() => RemoveQuadruplesByContextPredicate(ctx, pred));

        /// <summary>
        /// Removes the quadruples with the given context and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextObject(RDFContext ctx, RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context and object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextObjectAsync(RDFContext ctx, RDFResource obj)
            => Task.Run(() => RemoveQuadruplesByContextObject(ctx, obj));

        /// <summary>
        /// Removes the quadruples with the given context and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextLiteral(RDFContext ctx, RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context and literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextLiteralAsync(RDFContext ctx, RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesByContextLiteral(ctx, lit));

        /// <summary>
        /// Removes the quadruples with the given context, subject and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectPredicate(RDFContext ctx, RDFResource subj, RDFResource pred);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context, subject and predicate from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextSubjectPredicateAsync(RDFContext ctx, RDFResource subj, RDFResource pred)
            => Task.Run(() => RemoveQuadruplesByContextSubjectPredicate(ctx, subj, pred));

        /// <summary>
        /// Removes the quadruples with the given context, subject and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectObject(RDFContext ctx, RDFResource subj, RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context, subject and object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextSubjectObjectAsync(RDFContext ctx, RDFResource subj, RDFResource obj)
            => Task.Run(() => RemoveQuadruplesByContextSubjectObject(ctx, subj, obj));

        /// <summary>
        /// Removes the quadruples with the given context, subject and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectLiteral(RDFContext ctx, RDFResource subj, RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context, subject and literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextSubjectLiteralAsync(RDFContext ctx, RDFResource subj, RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesByContextSubjectLiteral(ctx, subj, lit));

        /// <summary>
        /// Removes the quadruples with the given context, predicate and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicateObject(RDFContext ctx, RDFResource pred, RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context, predicate and object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextPredicateObjectAsync(RDFContext ctx, RDFResource pred, RDFResource obj)
            => Task.Run(() => RemoveQuadruplesByContextPredicateObject(ctx, pred, obj));

        /// <summary>
        /// Removes the quadruples with the given context, predicate and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicateLiteral(RDFContext ctx, RDFResource pred, RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given context, predicate and literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByContextPredicateLiteralAsync(RDFContext ctx, RDFResource pred, RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesByContextPredicateLiteral(ctx, pred, lit));

        /// <summary>
        /// Removes the quadruples with the given subject and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectPredicate(RDFResource subj, RDFResource pred);

        /// <summary>
        /// Asynchronously removes the quadruples with the given subject and predicate from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesBySubjectPredicateAsync(RDFResource subj, RDFResource pred)
            => Task.Run(() => RemoveQuadruplesBySubjectPredicate(subj, pred));

        /// <summary>
        /// Removes the quadruples with the given subject and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectObject(RDFResource subj, RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given subject and object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesBySubjectObjectAsync(RDFResource subj, RDFResource obj)
            => Task.Run(() => RemoveQuadruplesBySubjectObject(subj, obj));

        /// <summary>
        /// Removes the quadruples with the given subject and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectLiteral(RDFResource subj, RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given subject and literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesBySubjectLiteralAsync(RDFResource subj, RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesBySubjectLiteral(subj, lit));

        /// <summary>
        /// Removes the quadruples with the given predicate and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicateObject(RDFResource pred, RDFResource obj);

        /// <summary>
        /// Asynchronously removes the quadruples with the given predicate and object from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByPredicateObjectAsync(RDFResource pred, RDFResource obj)
            => Task.Run(() => RemoveQuadruplesByPredicateObject(pred, obj));

        /// <summary>
        /// Removes the quadruples with the given predicate and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicateLiteral(RDFResource pred, RDFLiteral lit);

        /// <summary>
        /// Asynchronously removes the quadruples with the given predicate and literal from the store
        /// </summary>
        public Task<RDFStore> RemoveQuadruplesByPredicateLiteralAsync(RDFResource pred, RDFLiteral lit)
            => Task.Run(() => RemoveQuadruplesByPredicateLiteral(pred, lit));

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public abstract void ClearQuadruples();

        /// <summary>
        /// Asynchronously clears the quadruples of the store
        /// </summary>
        public Task ClearQuadruplesAsync()
            => Task.Run(() => ClearQuadruples());

        /// <summary>
        /// Compacts the reified quadruples by removing their 4 standard statements
        /// </summary>
        public void UnreifyQuadruples()
        {
            //Create SPARQL SELECT query for detecting reified quadruples
            RDFVariable T = new RDFVariable("T");
            RDFVariable C = new RDFVariable("C");
            RDFVariable S = new RDFVariable("S");
            RDFVariable P = new RDFVariable("P");
            RDFVariable O = new RDFVariable("O");
            RDFSelectQuery Q = new RDFSelectQuery()
                                .AddPatternGroup(new RDFPatternGroup()
                                  .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT))
                                  .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.SUBJECT, S))
                                  .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.PREDICATE, P))
                                  .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.OBJECT, O))
                                  .AddFilter(new RDFIsUriFilter(C))
                                  .AddFilter(new RDFIsUriFilter(T))
                                  .AddFilter(new RDFIsUriFilter(S))
                                  .AddFilter(new RDFIsUriFilter(P)));

            //Apply it to the store
            RDFSelectQueryResult R = Q.ApplyToStore(this);

            //Iterate results
            IEnumerator reifiedQuadruples = R.SelectResults.Rows.GetEnumerator();
            while (reifiedQuadruples.MoveNext())
            {
                //Get reification data (T, C, S, P, O)
                RDFPatternMember tRepresent = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?T"].ToString());
                RDFPatternMember tContext = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?C"].ToString());
                RDFPatternMember tSubject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?S"].ToString());
                RDFPatternMember tPredicate = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?P"].ToString());
                RDFPatternMember tObject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?O"].ToString());

                //Cleanup store from detected reifications
                if (tObject is RDFResource objRes)
                {
                    AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, objRes));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, objRes));
                }
                else
                {
                    AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, (RDFLiteral)tObject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFLiteral)tObject));
                }
            }
        }

        /// <summary>
        /// Asynchronously compacts the reified quadruples by removing their 4 standard statements
        /// </summary>
        public Task UnreifyQuadruplesAsync()
            => Task.Run(() => UnreifyQuadruples());
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public abstract bool ContainsQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Asynchronously checks if the store contains the given quadruple
        /// </summary>
        public Task<bool> ContainsQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => ContainsQuadruple(quadruple));

        /// <summary>
        /// Gets a memory store containing quadruples satisfying the given pattern
        /// </summary>
        public abstract RDFMemoryStore SelectQuadruples(RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store satisfying the given pattern
        /// </summary>
        internal Task<RDFMemoryStore> SelectQuadruplesAsync(RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
            => Task.Run(() => SelectQuadruples(ctx, subj, pred, obj, lit));

        /// <summary>
        /// Gets a store containing all quadruples
        /// </summary>
        public RDFMemoryStore SelectAllQuadruples()
            => SelectQuadruples(null, null, null, null, null);

        /// <summary>
        /// Asynchronously gets a memory store containing all quadruples of the store
        /// </summary>
        public Task<RDFMemoryStore> SelectAllQuadruplesAsync()
            => SelectQuadruplesAsync(null, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified context
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByContext(RDFContext ctx)
            => SelectQuadruples(ctx, null, null, null, null);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store with the specified context
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByContextAsync(RDFContext ctx)
            => SelectQuadruplesAsync(ctx, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified subject
        /// </summary>
        public RDFMemoryStore SelectQuadruplesBySubject(RDFResource subj)
            => SelectQuadruples(null, subj, null, null, null);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store with the specified subject
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesBySubjectAsync(RDFResource subj)
            => SelectQuadruplesAsync(null, subj, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified predicate
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByPredicate(RDFResource pred)
            => SelectQuadruples(null, null, pred, null, null);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store with the specified predicate
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByPredicateAsync(RDFResource pred)
            => SelectQuadruplesAsync(null, null, pred, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified object
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByObject(RDFResource obj)
            => SelectQuadruples(null, null, null, obj, null);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store with the specified object
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByObjectAsync(RDFResource obj)
            => SelectQuadruplesAsync(null, null, null, obj, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified literal
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral literal)
            => SelectQuadruples(null, null, null, null, literal);

        /// <summary>
        /// Asynchronously gets a memory store containing quadruples of the store with the specified literal
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByLiteralAsync(RDFLiteral literal)
            => SelectQuadruplesAsync(null, null, null, null, literal);

        /// <summary>
        /// Gets a list containing the graphs saved in the store
        /// </summary>
        public List<RDFGraph> ExtractGraphs()
        {
            Dictionary<long, RDFGraph> graphs = new Dictionary<long, RDFGraph>();
            foreach (RDFQuadruple q in (this is RDFMemoryStore memStore ? memStore : SelectAllQuadruples()))
            {
                // Step 1: Cache-Update
                if (!graphs.ContainsKey(q.Context.PatternMemberID))
                {
                    graphs.Add(q.Context.PatternMemberID, new RDFGraph());
                    graphs[q.Context.PatternMemberID].SetContext(((RDFContext)q.Context).Context);
                }

                // Step 2: Result-Update
                if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFResource)q.Object));
                else
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFLiteral)q.Object));
            }
            return graphs.Values.ToList();
        }

        /// <summary>
        /// Asynchronously gets a list containing the graphs saved in the store
        /// </summary>
        public Task<List<RDFGraph>> ExtractGraphsAsync()
            => Task.Run(() => ExtractGraphs());

        /// <summary>
        /// Gets a list containing the contexts saved in the store
        /// </summary>
        public List<RDFContext> ExtractContexts()
        {
            Dictionary<long, RDFPatternMember> contexts = new Dictionary<long, RDFPatternMember>();
            foreach (RDFQuadruple q in (this is RDFMemoryStore memStore ? memStore : SelectAllQuadruples()))
            {
                if (!contexts.ContainsKey(q.Context.PatternMemberID))
                    contexts.Add(q.Context.PatternMemberID, q.Context);
            }
            return contexts.Values.OfType<RDFContext>().ToList();
        }

        /// <summary>
        /// Asynchronously gets a list containing the contexts saved in the store
        /// </summary>
        public Task<List<RDFContext>> ExtractContextsAsync()
            => Task.Run(() => ExtractContexts());
        #endregion

        #region Convert

        #region Export
        /// <summary>
        /// Writes the store into a file in the given RDF format
        /// </summary>
        public void ToFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                throw new RDFStoreException("Cannot write RDF store to file because given \"filepath\" parameter is null or empty.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    RDFNQuads.Serialize(this, filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    RDFTriX.Serialize(this, filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    RDFTriG.Serialize(this, filepath);
                    break;
            }
        }

        /// <summary>
        /// Asynchronously writes the store into a file in the given RDF format
        /// </summary>
        public Task ToFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => ToFile(rdfFormat, filepath));

        /// <summary>
        /// Writes the store into a stream in the given RDF format
        /// </summary>
        public void ToStream(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
        {
            if (outputStream == null)
                throw new RDFStoreException("Cannot write RDF store to stream because given \"outputStream\" parameter is null.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    RDFNQuads.Serialize(this, outputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    RDFTriX.Serialize(this, outputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    RDFTriG.Serialize(this, outputStream);
                    break;
            }
        }

        /// <summary>
        /// Asynchronously writes the store into a stream in the given RDF format
        /// </summary>
        public Task ToStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
            => Task.Run(() => ToStream(rdfFormat, outputStream));

        /// <summary>
        /// Writes the store into a datatable with "Context-Subject-Predicate-Object" columns
        /// </summary>
        public DataTable ToDataTable()
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable(ToString());
            result.Columns.Add("?CONTEXT", typeof(string));
            result.Columns.Add("?SUBJECT", typeof(string));
            result.Columns.Add("?PREDICATE", typeof(string));
            result.Columns.Add("?OBJECT", typeof(string));

            //Iterate the quadruples of the store to populate the result datatable
            result.BeginLoadData();
            foreach (RDFQuadruple q in SelectAllQuadruples())
            {
                DataRow newRow = result.NewRow();
                newRow["?CONTEXT"] = q.Context.ToString();
                newRow["?SUBJECT"] = q.Subject.ToString();
                newRow["?PREDICATE"] = q.Predicate.ToString();
                newRow["?OBJECT"] = q.Object.ToString();
                result.Rows.Add(newRow);
            }
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Asynchronously writes the store into a datatable with "Context-Subject-Predicate-Object" columns
        /// </summary>
        public Task<DataTable> ToDataTableAsync()
            => Task.Run(() => ToDataTable());
        #endregion

        #endregion

        #endregion
    }
}